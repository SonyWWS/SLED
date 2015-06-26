/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "sledluaplugin.h"
#include "../sleddebugger/sleddebugger.h"

#include "../sleddebugger/assert.h"
#include "../sleddebugger/buffer.h"
#include "../sleddebugger/sequentialallocator.h"
#include "../sleddebugger/sleddebugger_class.h"
#include "../sleddebugger/stringarray.h"
#include "../sleddebugger/utilities.h"

#include "sledluaplugin_class.h"
#include "luautils.h"
#include "scmp.h"
#include "profilestack.h"
#include "varfilter.h"

#include "../sledcore/mutex.h"

#include <cstdlib>
#include <cstring>
#include <new>
#include <math.h>

namespace sce { namespace Sled
{
	namespace
	{
		inline void ResetVarFilterType(bool filter[9])
		{
			for (int i = 0; i < 9; i++)
				filter[i] = false;
		}

		inline void SetupVarFilterType(bool filter[9], const uint8_t values[9])
		{
			for (int i = 0; i < 9; i++)
				filter[i] = (values[i] == 1);
		}
		
		struct LuaPluginSeats
		{
			void *m_this;
			void *m_sendBuf;
			void *m_profileStack;
			void *m_luaStateParams;
			void *m_luaStatesNames;
			void *m_memTraceParams;
			void *m_breakpoints;
			void *m_varFilterContainer;
			void *m_editAndContinue;
			void *m_mutex;
			void *m_workBuf;

			void Allocate(const LuaPluginConfig& luaConfig, ISequentialAllocator *pAllocator)
			{
				// For this
				m_this = pAllocator->allocate(sizeof(LuaPlugin), __alignof(LuaPlugin));

				// For m_pSendBuf
				{
					NetworkBufferConfig config;
					config.maxSize = luaConfig.maxSendBufferSize;
					NetworkBuffer::requiredMemoryHelper(config, pAllocator, &m_sendBuf);
				}

				// For m_profileStack
				{
					ProfileStackConfig config(&luaConfig);
					ProfileStack::requiredMemoryHelper(config, pAllocator, &m_profileStack);
				}

				// For m_pLuaStates
				m_luaStateParams = pAllocator->allocate(sizeof(LuaStateParams) * luaConfig.maxLuaStates, __alignof(LuaStateParams));
				m_luaStatesNames = pAllocator->allocate(sizeof(char) * luaConfig.maxLuaStates * luaConfig.maxLuaStateNameLen, __alignof(char));

				// For m_pMemTraces
				m_memTraceParams = pAllocator->allocate(sizeof(MemTraceParams) * luaConfig.maxMemTraces, __alignof(MemTraceParams));

				// For m_pBreakpoints
				m_breakpoints = pAllocator->allocate(sizeof(Breakpoint) * luaConfig.maxBreakpoints, __alignof(Breakpoint));

				// For m_pVarFilterNames
				{
					VarFilterNameContainerConfig config(&luaConfig);
					VarFilterNameContainer::requiredMemoryHelper(config, pAllocator, &m_varFilterContainer);
				}

				// For m_pEditAndContinue
				{
					StringArrayConfig config;
					config.allowDuplicates = false;
					config.maxEntries = luaConfig.maxEditAndContinues;
					config.maxEntryLen = luaConfig.maxEditAndContinueEntryLen;
					StringArray::requiredMemoryHelper(&config, pAllocator, &m_editAndContinue);
				}

				m_mutex = pAllocator->allocate(sizeof(SceSledPlatformMutex), __alignof(SceSledPlatformMutex));

				m_workBuf = pAllocator->allocate(sizeof(uint8_t) * luaConfig.maxWorkBufferSize, __alignof(uint8_t));
			}
		};

		inline int32_t ValidateConfig(const LuaPluginConfig& config)
		{
			if ((config.maxLuaStates == 0) ||
				(config.maxSendBufferSize == 0) ||
				(config.maxWorkBufferSize == 0))
				return SCE_SLED_ERROR_INVALIDCONFIGURATION;

			return SCE_SLED_ERROR_OK;
		}		
	}

	int32_t LuaPlugin::create(const LuaPluginConfig& luaConfig, void *pLocation, LuaPlugin **ppLuaPlugin)
	{
		SCE_SLED_ASSERT(pLocation != NULL);
		SCE_SLED_ASSERT(ppLuaPlugin != NULL);

		std::size_t iMemSize = 0;
		const int32_t iConfigError = requiredMemory(luaConfig, &iMemSize);
		if (iConfigError != 0)
			return iConfigError;

		LuaPluginSeats seats;
		{
			SequentialAllocator allocator(pLocation, iMemSize);
			seats.Allocate(luaConfig, &allocator);

			SCE_SLED_ASSERT(seats.m_this != NULL);
			SCE_SLED_ASSERT(seats.m_sendBuf != NULL);
			SCE_SLED_ASSERT(seats.m_profileStack != NULL);
			SCE_SLED_ASSERT(seats.m_luaStateParams != NULL);
			SCE_SLED_ASSERT(seats.m_luaStatesNames != NULL);
			SCE_SLED_ASSERT(seats.m_memTraceParams != NULL);
			SCE_SLED_ASSERT(seats.m_breakpoints != NULL);
			SCE_SLED_ASSERT(seats.m_varFilterContainer != NULL);
			SCE_SLED_ASSERT(seats.m_editAndContinue != NULL);
			SCE_SLED_ASSERT(seats.m_mutex != NULL);
			SCE_SLED_ASSERT(seats.m_workBuf != NULL);
		}

		*ppLuaPlugin = new (seats.m_this) LuaPlugin(luaConfig, &seats);

		return SCE_SLED_ERROR_OK;
	}

	int32_t LuaPlugin::requiredMemory(const LuaPluginConfig& luaConfig, std::size_t *iRequiredMemory)
	{
		SCE_SLED_ASSERT(iRequiredMemory != NULL);

		const int32_t iConfigError = ValidateConfig(luaConfig);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocatorCalculator allocator;
		{
			LuaPluginSeats seats;
			seats.Allocate(luaConfig, &allocator);
		}

		*iRequiredMemory = allocator.bytesAllocated();	
		return SCE_SLED_ERROR_OK;
	}

	void LuaPlugin::close(LuaPlugin *pPlugin)
	{
		SCE_SLED_ASSERT(pPlugin != NULL);
		pPlugin->shutdown();
		pPlugin->~LuaPlugin();
	}

	LuaPlugin::LuaPlugin(const LuaPluginConfig& luaConfig, const void *pPluginSeats)
		: m_iPathChopChars(luaConfig.numPathChopChars)	
		, m_pfnChopCharsCallback(luaConfig.pfnChopCharsCallback)
		, m_pfnEditAndContinueCallback(luaConfig.pfnEditAndContinueCallback)
		, m_pfnEditAndContinueFinishCallback(luaConfig.pfnEditAndContinueFinishCallback)
		, m_pEditAndContinueUserData(luaConfig.pEditAndContinueUserData)
		, m_bLookUpWatches(false)
		, m_iVarExcludeFlags(VarExcludeFlags::kNone)
		, m_pCurHookLuaState(0)
		, m_pCurHookLuaDebug(0)
		, m_bHitBreakpoint(false)
		, m_pszSource(0)
		, m_iLastNumStackLevels(0)
		, m_bAssertBreakpoint(false)
		, m_bErrorBreakpoint(false)
		, m_iMaxLuaStates(luaConfig.maxLuaStates)
		, m_iNumLuaStates(0)
		, m_iMaxLuaStateNameLen(luaConfig.maxLuaStateNameLen)
		, m_iMaxMemTraces(luaConfig.maxMemTraces)
		, m_iNumMemTraces(0)
		, m_bProfilerRunning(false)
		, m_bMemoryTracerRunning(false)
		, m_iMaxBreakpoints(luaConfig.maxBreakpoints)	
		, m_iNumBreakpoints(0)
		, m_iWorkBufMaxSize(luaConfig.maxWorkBufferSize)
	{
		SCE_SLED_ASSERT(pPluginSeats != NULL);

		const LuaPluginSeats *pSeats = static_cast<const LuaPluginSeats*>(pPluginSeats);

		{
			NetworkBufferConfig config;
			config.maxSize = luaConfig.maxSendBufferSize;
			NetworkBuffer::create(config, pSeats->m_sendBuf, &m_pSendBuf);
		}

		{
			ProfileStackConfig config(&luaConfig);
			ProfileStack::create(config, pSeats->m_profileStack, &m_pProfileStack);
		}

		m_pLuaStates = new (pSeats->m_luaStateParams) LuaStateParams[luaConfig.maxLuaStates];
		m_pLuaStatesNames = new (pSeats->m_luaStatesNames) char[luaConfig.maxLuaStates * luaConfig.maxLuaStateNameLen];

		m_pMemTraces = new (pSeats->m_memTraceParams) MemTraceParams[luaConfig.maxMemTraces];
		m_pBreakpoints = new (pSeats->m_breakpoints) Breakpoint[luaConfig.maxBreakpoints];

		{
			VarFilterNameContainerConfig config(&luaConfig);
			VarFilterNameContainer::create(config, pSeats->m_varFilterContainer, &m_pVarFilterNames);
		}

		{
			StringArrayConfig config;
			config.allowDuplicates = false;
			config.maxEntries = luaConfig.maxEditAndContinues;
			config.maxEntryLen = luaConfig.maxEditAndContinueEntryLen;
			StringArray::create(&config, pSeats->m_editAndContinue, &m_pEditAndContinue);
		}

		ResetVarFilterType(m_bGlobalVarFilterType);
		ResetVarFilterType(m_bLocalVarFilterType);
		ResetVarFilterType(m_bUpvalueVarFilterType);
		ResetVarFilterType(m_bEnvVarVarFilterTypes);

		m_pMutex = new (pSeats->m_mutex) SceSledPlatformMutex;
		sceSledPlatformMutexAllocate(m_pMutex, SCE_SLEDPLATFORM_MUTEX_RECURSIVE);

		m_pWorkBuf = new (pSeats->m_workBuf) uint8_t[luaConfig.maxWorkBufferSize];
	}

	LuaPlugin::~LuaPlugin()
	{
	}

	void LuaPlugin::shutdown()
	{
		if (!m_bInitialized)
			return;

		m_bInitialized = false;

		sceSledPlatformMutexDeallocate(m_pMutex);
	}

	const Version LuaPlugin::getVersion() const
	{
		const Version ver(SCE_LIBSLEDLUAPLUGIN_VER_MAJOR, SCE_LIBSLEDLUAPLUGIN_VER_MINOR, SCE_LIBSLEDLUAPLUGIN_VER_REVISION);
		return ver;
	}

	void LuaPlugin::clientConnected()
	{
		const sce::SledPlatform::MutexLocker smg(m_pMutex); // SledDebugger is already locked

		//SCE_SLED_LOG(Logging::kInfo, "[SLED] [Lua] Connected");
		m_bLookUpWatches = false;

		// Send limits
		{
			const SCMP::Limits scmpLimits(kLuaPluginId,
										  m_iMaxBreakpoints,
										  m_pVarFilterNames->getMaxFilters(),
										  (m_pProfileStack->getMaxFunctions() != 0 ? true : false),
										  (m_iMaxMemTraces != 0 ? true : false),
										  m_pSendBuf);
			m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
		}

		// Send Lua state information
		{
			const SCMP::LuaStateBegin scmpLuaBeg(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpLuaBeg, scmpLuaBeg.length);

			for (uint16_t i = 0; i < m_iNumLuaStates; i++)
			{
				char szTemp[SCMP::Sizes::kPtrLen];
				StringUtilities::copyString(szTemp, SCMP::Sizes::kPtrLen, "0x%p", m_pLuaStates[i].luaState);

				const char *name = m_pLuaStatesNames + (i * m_iMaxLuaStateNameLen);

				const SCMP::LuaStateAdd scmpLua(kLuaPluginId, szTemp, name, m_pLuaStates[i].isDebugging(), m_pSendBuf);
				m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
			}

			const SCMP::LuaStateEnd scmpLuaEnd(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpLuaEnd, scmpLuaEnd.length);
		}
	}

	void LuaPlugin::clientDisconnected()
	{
		const sce::SledPlatform::MutexLocker smg(m_pMutex); // SledDebugger is already locked

		clientDisconnectedLua();
	
		// Clear breakpoints, profiler, memory tracer, var filters
		m_bLookUpWatches = false;
		m_iNumBreakpoints = 0;
		m_bMemoryTracerRunning = false;
		m_iNumMemTraces = 0;
		m_bProfilerRunning = false;
		m_pProfileStack->clear();
		ResetVarFilterType(m_bGlobalVarFilterType);
		ResetVarFilterType(m_bLocalVarFilterType);
		ResetVarFilterType(m_bUpvalueVarFilterType);
		ResetVarFilterType(m_bEnvVarVarFilterTypes);
		m_pVarFilterNames->clearAll();

		//SCE_SLED_LOG(Logging::kInfo, "[SLED] [Lua] Disconnected");
	}

	void LuaPlugin::clientMessage(const uint8_t *pData, int32_t iSize)
	{
		SCE_SLED_ASSERT((reinterpret_cast<uintptr_t>(pData) % __alignof(SCMP::Base)) == 0);

		const sce::SledPlatform::MutexLocker smg(m_pMutex); // SledDebugger is already locked	

		const SCMP::Base *pScmp = 0;
		std::memcpy(&pScmp, &pData, sizeof(SCMP::Base*));

		const SCMP::Base& scmpBase = *pScmp;

		NetworkBufferReader reader(pData, iSize);

		switch (scmpBase.typeCode)
		{
		case Sled::SCMP::TypeCodes::kBreakpointDetails:
			handleScmpBreakpointDetails(&reader);
			break;
		case SCMP::LuaTypeCodes::kVarFilterStateNameBegin:
			handleScmpVarFilterStateNameBegin(&reader);
			break;	
		case SCMP::LuaTypeCodes::kVarFilterStateName:
			handleScmpVarFilterStateName(&reader);
			break;
		case SCMP::LuaTypeCodes::kVarFilterStateNameEnd:
			handleScmpVarFilterStateNameEnd(&reader);
			break;
		case SCMP::LuaTypeCodes::kVarFilterStateTypeBegin:
			handleScmpVarFilterStateTypeBegin(&reader);
			break;
		case SCMP::LuaTypeCodes::kVarFilterStateType:
			handleScmpVarFilterStateType(&reader);
			break;
		case SCMP::LuaTypeCodes::kVarFilterStateTypeEnd:
			handleScmpVarFilterStateTypeEnd(&reader);
			break;
		case SCMP::LuaTypeCodes::kVarLookUp:
			handleScmpVarLookUp(&reader);
			break;
		case SCMP::LuaTypeCodes::kVarUpdate:
			handleScmpVarUpdate(&reader);
			break;
		case SCMP::LuaTypeCodes::kWatchLookUpBegin:
			handleScmpWatchLookUpBegin(&reader);
			break;
		case SCMP::LuaTypeCodes::kWatchLookUpEnd:
			handleScmpWatchLookUpEnd(&reader);
			break;
		case SCMP::LuaTypeCodes::kCallStackLookUpPerform:
			handleScmpCallStackLookUpPerform(&reader);
			break;
		case SCMP::LuaTypeCodes::kMemoryTraceToggle:
			handleScmpMemoryTraceToggle(&reader);
			break;
		case SCMP::LuaTypeCodes::kProfilerToggle:
			handleScmpProfilerToggle(&reader);
			break;
		case SCMP::LuaTypeCodes::kProfileInfoLookUpPerform:
			handleScmpProfileInfoLookUpPerform(&reader);
			break;
		case Sled::SCMP::TypeCodes::kDevCmd:
			handleScmpDevCmd(&reader);
			break;
		case Sled::SCMP::TypeCodes::kEditAndContinue:
			handleScmpEditAndContinue(&reader);
			break;
		case SCMP::LuaTypeCodes::kLuaStateToggle:
			handleScmpLuaStateToggle(&reader);
			break;
		}
	}

	void LuaPlugin::clientBreakpointBegin(const BreakpointParams *pParams)
	{
		const sce::SledPlatform::MutexLocker smg(m_pMutex); // SledDebugger is already locked

		// Check if this plugin expected to hit a breakpoint
		if (!m_bHitBreakpoint)
		{
			// Pause timers
			m_pProfileStack->preBreakpoint();
			return;
		}	
	
		// Get globals and callstack information (locals/upvalues/environment) if not excluded
		clientBreakpointBeginLua(pParams);

		// Send profile information
		if (m_pProfileStack->getNumFunctions() > 0)
		{
			const SCMP::ProfileInfoBegin piBeg(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&piBeg, piBeg.length);

			ProfileStack::ConstIterator iter(m_pProfileStack);
			for (; iter(); ++iter)
			{
				const ProfileEntry *pEntry = iter.get();

				const SCMP::ProfileInfo pi(kLuaPluginId,
										   pEntry->getFnName(), 
										   pEntry->getFnFile(),
										   pEntry->getFnTimeElapsed(), 
										   pEntry->getFnTimeElapsedAvg(),
										   pEntry->getFnTimeElapsedShortest(),
										   pEntry->getFnTimeElapsedLongest(), 
										   pEntry->getFnTimeInnerElapsed(),
										   pEntry->getFnTimeInnerElapsedAvg(),
										   pEntry->getFnTimeInnerElapsedShortest(),
										   pEntry->getFnTimeInnerElapsedLongest(), 
										   pEntry->getFnCallCount(),
										   pEntry->getFnLine(),
										   (int32_t)pEntry->getFnCalls(),
										   m_pSendBuf);
				m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
			}

			const SCMP::ProfileInfoEnd piEnd(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&piEnd, piEnd.length);
		}

		// Send memory trace information
		if (m_iNumMemTraces > 0)
		{
			const SCMP::MemoryTraceBegin mtBeg(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&mtBeg, mtBeg.length);

			for (uint32_t i = 0; i < m_iNumMemTraces; i++)
			{
				const SCMP::MemoryTrace mt(kLuaPluginId,
										   m_pMemTraces[i].what,
										   m_pMemTraces[i].oldPtr,
										   m_pMemTraces[i].newPtr,
										   m_pMemTraces[i].oldSize,
										   m_pMemTraces[i].newSize,
										   m_pSendBuf);
				m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
			}

			m_iNumMemTraces = 0;

			const SCMP::MemoryTraceEnd mtEnd(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&mtEnd, mtEnd.length);
		}
	}

	void LuaPlugin::clientBreakpointEnd(const BreakpointParams *pParams)
	{
		SCE_SLEDUNUSED(pParams);
		const sce::SledPlatform::MutexLocker smg(m_pMutex); // SledDebugger is already locked		

		// Check if this plugin expected to hit a breakpoint or not
		if (!m_bHitBreakpoint)
		{
			// Resume timers
			m_pProfileStack->postBreakpoint();
		}
	}

	void LuaPlugin::clientDebugModeChanged(DebuggerMode::Enum newMode)
	{
		const sce::SledPlatform::MutexLocker smg(m_pMutex); // SledDebugger is already locked

		clientDebugModeChangedLua(newMode);
	}

	void LuaPlugin::resetProfileInfo()
	{
		m_pProfileStack->clear();
	}

	void LuaPlugin::resetMemoryTrace()
	{
		m_iNumMemTraces = 0;
	}

	bool LuaPlugin::memoryTraceNotify(void *ud, void *oldPtr, void *newPtr, std::size_t oldSize, std::size_t newSize)
	{
		SCE_SLEDUNUSED(ud);

		// Not running and/or no space allocated
		if (!m_bMemoryTracerRunning || (m_iMaxMemTraces == 0))
			return false;

		// Figure out allocation/deallocation/reallocation
		const char chWhat = (newSize == 0) ? 'd' : ((oldPtr && newPtr) ? 'r' : 'a');

		// Add item to list
		const MemTraceParams hTemp(chWhat, oldPtr, newPtr, oldSize, newSize);
		m_pMemTraces[m_iNumMemTraces++] = hTemp;

		// Check if time to dump (like if we've reached max capacity)
		if (m_iNumMemTraces == m_iMaxMemTraces)
		{
			if (m_pScriptMan)
			{
				const sce::SledPlatform::MutexLocker smgSm(m_pScriptMan->getMutex());
				const sce::SledPlatform::MutexLocker smg(m_pMutex);

				const SCMP::MemoryTraceStreamBegin scmpMemTrBeg(kLuaPluginId);
				m_pScriptMan->send((uint8_t*)&scmpMemTrBeg, scmpMemTrBeg.length);

				for (uint32_t i = 0; i < m_iNumMemTraces; i++)
				{
					const SCMP::MemoryTraceStream scmpMemTr(kLuaPluginId,
															m_pMemTraces[i].what,
															m_pMemTraces[i].oldPtr,
															m_pMemTraces[i].newPtr,
															m_pMemTraces[i].oldSize,
															m_pMemTraces[i].newSize,
															m_pSendBuf);
					m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
				}

				const SCMP::MemoryTraceStreamEnd scmpMemTrEnd(kLuaPluginId);
				m_pScriptMan->send((uint8_t*)&scmpMemTrEnd, scmpMemTrEnd.length);
			}

			// Reset
			m_iNumMemTraces = 0;
		}

		return true;
	}

	int32_t LuaPlugin::ttyNotify(const char *pszMessage)
	{
		if (!m_pScriptMan)
			return SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE;

		return m_pScriptMan->ttyNotify(pszMessage);
	}

	const char *LuaPlugin::trimFileName(const char *pszFileName)
	{
		if (m_pfnChopCharsCallback)
		{
			return m_pfnChopCharsCallback(pszFileName);
		}
		else
		{
			int32_t iChop = m_iPathChopChars;
			if (pszFileName[0] == '@')
				++iChop;

			return pszFileName + iChop;
		}
	}

	void LuaPlugin::tagFuncForLookUp(char *pszBuffer, std::size_t iBufLen, const char *pszFuncName, const char *pszFileName, const int32_t& iLine)
	{
		SCE_SLED_ASSERT(pszBuffer != NULL);

		if (pszFuncName)
		{
			// If the function name is known then we simply use it
			Utilities::copyString(pszBuffer, iBufLen, pszFuncName);
		}
		else
		{
			// Otherwise generate some sort of tag so SLED can look up the function
			StringUtilities::copyString(pszBuffer, iBufLen, ":%i:%s", iLine, pszFileName);
		}
	}

	bool LuaPlugin::isVarNameFiltered(const char *pszName, char chWhat) const
	{
		return m_pVarFilterNames->isFiltered(pszName, chWhat);
	}

	void LuaPlugin::sendGlobal(const LuaVariable *parent, const char *name, int32_t nameType, const char *value, int32_t valueType)
	{
		const SCMP::GlobalVar global(kLuaPluginId, parent, name, (int16_t)nameType, value, (int16_t)valueType, m_pSendBuf);
		m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
	}

	void LuaPlugin::sendLocal(const LuaVariable *parent, const char *name, int32_t nameType, const char *value, int32_t valueType, int32_t stackLevel, int32_t index)
	{
		// Don't send temporary variables
		if (name[0] == '(')
			return;

		const SCMP::LocalVar var(kLuaPluginId, parent, name, (int16_t)nameType, value, (int16_t)valueType, (int16_t)stackLevel, index, m_pSendBuf);
		m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
	}

	void LuaPlugin::sendUpvalue(const LuaVariable *parent, const char *name, int32_t nameType, const char *value, int32_t valueType, int32_t stackLevel, int32_t index)
	{
		// Don't send temporary variables
		if (name[0] == '(')
			return;

		const SCMP::UpvalueVar var(kLuaPluginId, parent, name, (int16_t)nameType, value, (int16_t)valueType, (int16_t)stackLevel, index, m_pSendBuf);
		m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
	}

	void LuaPlugin::sendEnvVar(const LuaVariable *parent, const char *name, int32_t nameType, const char *value, int32_t valueType, int32_t stackLevel)
	{
		const SCMP::EnvVar var(kLuaPluginId, parent, name, (int16_t)nameType, value, (int16_t)valueType, (int16_t)stackLevel, m_pSendBuf);
		m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
	}

	void LuaPlugin::handleScmpBreakpointDetails(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		handleScmpBreakpointDetailsLua(pReader);
	}

	void LuaPlugin::handleScmpVarFilterStateNameBegin(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		const SCMP::VarFilterStateNameBegin vfs(pReader);
		//SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [Var Filter State Name Begin] [%c]", (char)vfs.what);
		m_pVarFilterNames->clear(vfs.what);
	}

	void LuaPlugin::handleScmpVarFilterStateName(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		const SCMP::VarFilterStateName vfs(pReader);
		//SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [Var Filter State Name] [%c] [%s]", (char)vfs.what, vfs.filter);
		m_pVarFilterNames->addFilter(vfs.what, vfs.filter);
	}

	void LuaPlugin::handleScmpVarFilterStateNameEnd(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		/*SCMP::VarFilterStateNameEnd vfs(pReader);
		SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [Var Filter State Name End] [%c]", (char)vfs.what);*/
	}

	void LuaPlugin::handleScmpVarFilterStateTypeBegin(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		const SCMP::VarFilterStateTypeBegin vfs(pReader);
		//SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [SCMP] [Lua] [Var Filter State Type Begin] [%c]", (char)vfs.what);
		switch ((char)vfs.what)
		{
		case 'g':
			ResetVarFilterType(m_bGlobalVarFilterType);
			break;
		case 'l':
			ResetVarFilterType(m_bLocalVarFilterType);
			break;
		case 'u':
			ResetVarFilterType(m_bUpvalueVarFilterType);
			break;
		case 'e':
			ResetVarFilterType(m_bEnvVarVarFilterTypes);
			break;
		}
	}

	void LuaPlugin::handleScmpVarFilterStateType(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		const SCMP::VarFilterStateType vfs(pReader);
		/*SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [SCMP] [Lua] [Var Filter State Type] [%c] [%s,%s,%s,%s,%s,%s,%s,%s,%s]",
			((char)vfs.what),
			(vfs.filter[0] == 1 ? "true" : "false"),
			(vfs.filter[1] == 1 ? "true" : "false"),
			(vfs.filter[2] == 1 ? "true" : "false"),
			(vfs.filter[3] == 1 ? "true" : "false"),
			(vfs.filter[4] == 1 ? "true" : "false"),
			(vfs.filter[5] == 1 ? "true" : "false"),
			(vfs.filter[6] == 1 ? "true" : "false"),
			(vfs.filter[7] == 1 ? "true" : "false"),
			(vfs.filter[8] == 1 ? "true" : "false"));*/
		switch ((char)vfs.what)
		{
		case 'g':
			SetupVarFilterType(m_bGlobalVarFilterType, vfs.filter);
			break;
		case 'l':
			SetupVarFilterType(m_bLocalVarFilterType, vfs.filter);
			break;
		case 'u':
			SetupVarFilterType(m_bUpvalueVarFilterType, vfs.filter);
			break;
		case 'e':
			SetupVarFilterType(m_bEnvVarVarFilterTypes, vfs.filter);
			break;
		}
	}

	void LuaPlugin::handleScmpVarFilterStateTypeEnd(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		//SCMP::VarFilterStateTypeEnd vfs(pReader);
		//SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [SCMP] [Lua] [Var Filter State Type End] [%c]", (char)vfs.what);
	}

	void LuaPlugin::handleScmpVarLookUp(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		SCMP::VarLookUp lookup(pReader, m_pWorkBuf, (uint16_t)m_iWorkBufMaxSize);

		if (lookup.extra == 1)
		{
			lookup.variable.bFlag = true;

			const SCMP::WatchLookUpClear scmpWtchLkClr(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpWtchLkClr, scmpWtchLkClr.length);
		}

		const bool sendWatchProjBegEnd =
			(lookup.extra == 1) &&
			((lookup.variable.context == LuaVariableContext::kWatchProject) ||
			(lookup.variable.context == LuaVariableContext::kWatchCustom));

		if (sendWatchProjBegEnd)
		{
			const SCMP::WatchLookUpProjectBegin scmpWtchLkPrjBeg(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpWtchLkPrjBeg, scmpWtchLkPrjBeg.length);
		}

		if (lookup.variable.context == LuaVariableContext::kNormal)
		{
			handleScmpVarLookUpNormal(&lookup);
		}
		else if (lookup.variable.context == LuaVariableContext::kWatchProject)
		{
			handleScmpVarLookUpNormal(&lookup);
		}
		else if (lookup.variable.context == LuaVariableContext::kWatchCustom)
		{
			const SCMP::WatchLookUpCustomBegin scmpWtchLkCstmBeg(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpWtchLkCstmBeg, scmpWtchLkCstmBeg.length);

			handleScmpVarLookUpCustom(&lookup);

			const SCMP::WatchLookUpCustomEnd scmpWtchLkCstmEnd(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpWtchLkCstmEnd, scmpWtchLkCstmEnd.length);
		}

		if (sendWatchProjBegEnd)
		{
			const SCMP::WatchLookUpProjectEnd scmpWtchLkPrjEnd(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpWtchLkPrjEnd, scmpWtchLkPrjEnd.length);
		}
	}

	void LuaPlugin::handleScmpVarLookUpNormal(SCMP::VarLookUp *pLookUp)
	{
		if ((pLookUp == NULL) || (m_pCurHookLuaState == NULL))
			return;

		lua_State* const luaState = m_pCurHookLuaState;

		const StackReconciler recon(luaState);

		if (!m_bLookUpWatches)
		{
			// Prepare network message based on var type
			switch (pLookUp->variable.what)
			{
				case LuaVariableScope::kGlobal:
				{
					const SCMP::GlobalVarLookUpBegin scmp(kLuaPluginId);
					m_pScriptMan->send((uint8_t*)&scmp, scmp.length);
				}
				break;
				case LuaVariableScope::kLocal:
				{
					const SCMP::LocalVarLookUpBegin scmp(kLuaPluginId);
					m_pScriptMan->send((uint8_t*)&scmp, scmp.length);
				}
				break;
				case LuaVariableScope::kUpvalue:
				{
					const SCMP::UpvalueVarLookUpBegin scmp(kLuaPluginId);
					m_pScriptMan->send((uint8_t*)&scmp, scmp.length);
				}
				break;
				case LuaVariableScope::kEnvironment:
				{
					const SCMP::EnvVarLookUpBegin scmp(kLuaPluginId);
					m_pScriptMan->send((uint8_t*)&scmp, scmp.length);
				}
				break;
			}
		}

		// Look up the variable
		lookupVariable(luaState, &(pLookUp->variable));

		if (!m_bLookUpWatches)
		{
			// Prepare network message based on var type
			switch (pLookUp->variable.what)
			{
				case LuaVariableScope::kGlobal:
				{
					const SCMP::GlobalVarLookUpEnd scmp(kLuaPluginId);
					m_pScriptMan->send((uint8_t*)&scmp, scmp.length);
				}
				break;
				case LuaVariableScope::kLocal:
				{
					const SCMP::LocalVarLookUpEnd scmp(kLuaPluginId);
					m_pScriptMan->send((uint8_t*)&scmp, scmp.length);
				}
				break;
				case LuaVariableScope::kUpvalue:
				{
					const SCMP::UpvalueVarLookUpEnd scmp(kLuaPluginId);
					m_pScriptMan->send((uint8_t*)&scmp, scmp.length);
				}
				break;
				case LuaVariableScope::kEnvironment:
				{
					const SCMP::EnvVarLookUpEnd scmp(kLuaPluginId);
					m_pScriptMan->send((uint8_t*)&scmp, scmp.length);
				}
				break;
			}
		}
	}

	void LuaPlugin::handleScmpVarLookUpCustom(SCMP::VarLookUp *pLookUp)
	{
		handleScmpVarLookUpCustomLua(pLookUp);	
	}

	void LuaPlugin::handleScmpVarUpdate(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		const SCMP::VarUpdate update(pReader, m_pWorkBuf, (uint16_t)m_iWorkBufMaxSize);
	
		setVariable(m_pCurHookLuaState, &update.variable);
	}

	void LuaPlugin::handleScmpWatchLookUpBegin(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		m_bLookUpWatches = true;

		// Unpack
		const SCMP::WatchLookUpBegin lookup(pReader);

		// Re-pack and send
		const SCMP::WatchLookUpBegin lkBeg(kLuaPluginId, lookup.what, m_pSendBuf);
		m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
	}

	void LuaPlugin::handleScmpWatchLookUpEnd(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		m_bLookUpWatches = false;

		// Unpack
		const SCMP::WatchLookUpEnd lookup(pReader);

		// Re-pack and send
		const SCMP::WatchLookUpEnd lkEnd(kLuaPluginId, lookup.what, m_pSendBuf);
		m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
	}

	void LuaPlugin::handleScmpCallStackLookUpPerform(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		handleScmpCallStackLookUpPerformLua(pReader);
	}

	void LuaPlugin::handleScmpMemoryTraceToggle(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);

		m_bMemoryTracerRunning = !m_bMemoryTracerRunning;
		m_iNumMemTraces = 0;
	}

	void LuaPlugin::handleScmpProfilerToggle(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		handleScmpProfilerToggleLua(pReader);
	}

	void LuaPlugin::handleScmpProfileInfoLookUpPerform(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		const SCMP::ProfileInfoLookUpPerform lookup(pReader);

		// Look for original function
		const ProfileEntry* pFunc = m_pProfileStack->findFn(lookup.functionName, lookup.relScriptPath, lookup.line);
		if (!pFunc)
		{
			// Try and find tagged function
			const std::size_t iFuncTagLen = Sled::SCMP::Base::kStringLen;
			char szFuncName[iFuncTagLen];
			tagFuncForLookUp(szFuncName, iFuncTagLen, 0, lookup.relScriptPath, lookup.line);
		
			// Look for tagged function
			pFunc = m_pProfileStack->findFn(szFuncName, lookup.relScriptPath, lookup.line);
		}			

		if (pFunc)
		{
			const SCMP::ProfileInfoLookUpBegin scmpPILkBeg(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpPILkBeg, scmpPILkBeg.length);

			ProfileEntry::ConstIterator iter(pFunc);
			for (; iter(); ++iter)
			{
				const ProfileEntry* const pEntry = iter.get();

				const SCMP::ProfileInfoLookUp scmpPIL(kLuaPluginId,
													  pEntry->getFnName(), 
													  pEntry->getFnFile(),
													  pEntry->getFnTimeElapsed(),
													  pEntry->getFnTimeElapsedAvg(),
													  pEntry->getFnTimeElapsedShortest(),
													  pEntry->getFnTimeElapsedLongest(),
													  pEntry->getFnTimeInnerElapsed(),
													  pEntry->getFnTimeInnerElapsedAvg(),
													  pEntry->getFnTimeInnerElapsedShortest(),
													  pEntry->getFnTimeInnerElapsedLongest(),
													  pEntry->getFnCallCount(),
													  pEntry->getFnLine(),
													  (int32_t)pEntry->getFnCalls(),
													  m_pSendBuf);
				m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());
			}

			const SCMP::ProfileInfoLookUpEnd scmpPILkEnd(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpPILkEnd, scmpPILkEnd.length);
		}
	}

	void LuaPlugin::handleScmpDevCmd(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		handleScmpDevCmdLua(pReader);
	}

	void LuaPlugin::handleScmpEditAndContinue(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		const Sled::SCMP::EditAndContinue editCont(pReader);

		if (m_pEditAndContinue->add(editCont.relScriptPath))
		{
			SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [EC_R] Scheduling edit & continue of script file: %s", editCont.relScriptPath);
		}
		else
		{
			SCE_SLED_LOG(Logging::kError, "[SLED] [LuaPlugin] [EC_R] Failed to schedule file for edit & continue! (file: %s)", editCont.relScriptPath);
		}
	}

	void LuaPlugin::handleScmpLuaStateToggle(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		handleScmpLuaStateToggleLua(pReader);
	}
}}
