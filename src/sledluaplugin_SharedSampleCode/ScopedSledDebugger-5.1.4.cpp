/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "ScopedSledDebugger.h"
#include <sce_sled/sled.h>

#include "../sleddebugger/assert.h"
#include "../sledluaplugin/Extras/extras.h"

#include "../sledcore/sleep.h"

#include <cstdio>
#include <cstring>

#include "fileutilities.h"
#include "luastuff.h"
#include "input.h"

extern "C"
{
	#include <wws_lua/lua-5.1.4/src/lua.h>
	#include <wws_lua/lua-5.1.4/src/lualib.h>
	#include <wws_lua/lua-5.1.4/src/lauxlib.h>
}

namespace Examples
{
	using namespace sce::Sled;

	int32_t ScopedSledDebugger::Create(Protocol::Enum protocol,
								   uint16_t port,
								   bool bBlockUntilConnect,
								   void *pFile1LoadItems,
								   void *pFile2LoadItems,
								   const char *pszName)
	{
		SCE_SLED_ASSERT_MSG(pszName != NULL, "Debugger name can't be null!");
		SCE_SLED_ASSERT_MSG(std::strlen(pszName) > 0, "Debugger name can't be empty!");
		SCE_SLED_ASSERT_MSG((int)std::strlen(pszName) <= kNameLen, "Debugger name too long for buffer!");
		sprintf(m_name, "%s", pszName);

		SCE_SLED_ASSERT_MSG(pFile1LoadItems != NULL, "pFile1LoadItems can't be null!");	
		SCE_SLED_ASSERT_MSG(pFile2LoadItems != NULL, "pFile2LoadItems can't be null!");
		FileUtil::FileLoadItems *pItems1 = static_cast<FileUtil::FileLoadItems*>(pFile1LoadItems);
		FileUtil::FileLoadItems *pItems2 = static_cast<FileUtil::FileLoadItems*>(pFile2LoadItems);

		const long lFile1Size = pItems1->GetFileSize();
		const char *pszFile1RelPath = pItems1->GetRelFilePath();
		const char *pszFile1Contents = pItems1->GetFileContents();

		const long lFile2Size = pItems2->GetFileSize();
		const char *pszFile2RelPath = pItems2->GetRelFilePath();
		const char *pszFile2Contents = pItems2->GetFileContents();

		SCE_SLED_ASSERT(lFile1Size > 0);
		SCE_SLED_ASSERT(lFile2Size > 0);
		SCE_SLED_ASSERT_MSG(pszFile1RelPath != NULL, "pszFile1RelPath can't be null!");
		SCE_SLED_ASSERT_MSG((int)std::strlen(pszFile1RelPath) > 0, "pszFile1RelPath can't be empty!");
		SCE_SLED_ASSERT_MSG(pszFile1Contents != NULL, "pszFile1Contents can't be null!");
		SCE_SLED_ASSERT_MSG((int)std::strlen(pszFile1Contents) > 0, "pszFile1Contents can't be empty!");
		SCE_SLED_ASSERT_MSG(pszFile2RelPath != NULL, "pszFile2RelPath can't be null!");
		SCE_SLED_ASSERT_MSG((int)std::strlen(pszFile2RelPath) > 0, "pszFile2RelPath can't be empty!");
		SCE_SLED_ASSERT_MSG(pszFile2Contents != NULL, "pszFile2Contents can't be null!");
		SCE_SLED_ASSERT_MSG((int)std::strlen(pszFile2Contents) > 0, "pszFile2Contents can't be empty!");

		int32_t iRetval = 0;

		{
			SledDebuggerConfig config;
			config.maxPlugins = 1;
			config.maxRecvBufferSize = 2048;
			config.maxSendBufferSize = 2048;
			config.maxScriptCacheEntries = 2;
			config.maxScriptCacheEntryLen = 64;
			config.net.setup(protocol, port, bBlockUntilConnect);

			std::size_t iMemSize = 0;
			iRetval = debuggerRequiredMemory(&config, &iMemSize);
			if (iRetval != SCE_SLED_ERROR_OK)
			{
				std::printf("ScopedSledDebugger %i: Error calculating required memory for SledDebugger! %i\n", m_iInstanceCount, iRetval);
				return iRetval;
			}

			m_pMemory = new char[iMemSize];
			SCE_SLED_ASSERT(m_pMemory != NULL);

			std::printf("ScopedSledDebugger %i: Creating SledDebugger instance (%lu bytes)...", m_iInstanceCount, (unsigned long)iMemSize);

			iRetval = debuggerCreate(&config, m_pMemory, &m_debugger);
			if (iRetval != SCE_SLED_ERROR_OK)
			{
				std::printf("ScopedSledDebugger %i: Error creating SledDebugger instance! %i\n", m_iInstanceCount, iRetval);
				return iRetval;
			}

			std::printf("done!\n");
		}

		{
			Version verInfo;
			debuggerGetVersion(m_debugger, &verInfo);
			std::printf("ScopedSledDebugger %i: SledDebugger version %i.%i.%i\n", m_iInstanceCount, (int)verInfo.majorNum, (int)verInfo.minorNum, (int)verInfo.revisionNum);
		}

		bool bRetval;

		// Add script1 to script cache
		iRetval = debuggerScriptCacheAdd(m_debugger, pszFile1RelPath, &bRetval);
		if (iRetval != SCE_SLED_ERROR_OK)
		{
			std::printf("ScopedSledDebugger %i: Error with scriptCacheAdd! %i\n", m_iInstanceCount, iRetval);
			return iRetval;
		}

		// Make sure added
		if (!bRetval)
			return SCE_SLED_ERROR_INVALIDPARAMETER;
	
		// Add script2 to script cache
		iRetval = debuggerScriptCacheAdd(m_debugger, pszFile2RelPath, &bRetval);
		if (iRetval != SCE_SLED_ERROR_OK)
		{
			std::printf("ScopedSledDebugger %i: Error with scriptCacheAdd! %i\n", m_iInstanceCount, iRetval);
		}

		// Make sure added
		if (!bRetval)
			return SCE_SLED_ERROR_INVALIDPARAMETER;

		// Add Lua plugin to SledDebugger
		m_pFileBuffer = new FileUtil::FileBuffer();
		iRetval = AddLuaplugin(2048,
							   2,
							   32,
							   5000,
							   64,
							   2,
							   64,
							   8,
							   5,
							   10,
							   600,
							   16,
							   0,
							   0,
							   FileUtil::OpenFile,
							   FileUtil::OpenFileFinish,
							   m_pFileBuffer);
		if (iRetval != 0)
			return iRetval;

		// Start networking
		{
			std::printf("ScopedSledDebugger %i: Starting networking...\n", m_iInstanceCount);
			switch (protocol)
			{
			case Protocol::kTcp:
				std::printf("ScopedSledDebugger %i: \t\t[TCP] Listening on port: %i\n", m_iInstanceCount, port);
				break;
			}
			std::printf("ScopedSledDebugger %i: \t\tWaiting for SLED? %s\n", m_iInstanceCount, bBlockUntilConnect ? "yes" : "no");

			iRetval = debuggerStartNetworking(m_debugger);
			if (iRetval != 0)
			{
				std::printf("ScopedSledDebugger %i: Failed: %i!\n", m_iInstanceCount, iRetval);
				return iRetval;
			}
			else
			{
				std::printf("ScopedSledDebugger %i: Successful!\n", m_iInstanceCount);
			}
		}

		// Create Lua states
		{
			m_pLuaState1 = ::lua_newstate(LuaStuff::Allocate, m_plugin);
			m_pLuaState2 = ::lua_newstate(LuaStuff::Allocate, m_plugin);
	
			// Open some Lua packages & register functions
			LuaStuff::OpenLibs(m_pLuaState1);
			LuaStuff::OpenLibs(m_pLuaState2);

			lua_register(m_pLuaState1, "CFunc1", LuaStuff::CFunc1);
			lua_register(m_pLuaState1, "CFunc2", LuaStuff::CFunc2);

			// Register actor lib
			LuaStuff::luaopen_Actor(m_pLuaState1);
		}

		// Load file contents into Lua states
		{
			iRetval = LuaStuff::LoadFileContentsIntoLuaState(m_pLuaState1, pszFile1Contents, lFile1Size, pszFile1RelPath);
			if (iRetval != 0)
			{
				std::printf("ScopedSledDebugger %i: Failed loading file %s into Lua state! %i\n", m_iInstanceCount, pszFile1RelPath, iRetval);
				return SCE_SLED_ERROR_INVALIDPARAMETER;
			}

			iRetval = LuaStuff::LoadFileContentsIntoLuaState(m_pLuaState2, pszFile2Contents, lFile2Size, pszFile2RelPath);
			if (iRetval != 0)
			{
				std::printf("ScopedSledDebugger %i: Failed loading file %s into Lua state! %i\n", m_iInstanceCount, pszFile2RelPath, iRetval);
				return SCE_SLED_ERROR_INVALIDPARAMETER;
			}		
		}

		// Register Lua states with LuaPlugin
		{
			const char *pszState1Name = "Main State";
			const char *pszState2Name = "Simple Script";

			iRetval = luaPluginRegisterLuaState(m_plugin, m_pLuaState1, pszState1Name);
			if (iRetval != 0)
			{
				std::printf("ScopedSledDebugger %i: LuaPlugin: Failed to register Lua state %s (0x%p) with LuaPlugin!\n", m_iInstanceCount, pszState1Name, m_pLuaState1);
				return iRetval;
			}

			iRetval = luaPluginRegisterLuaState(m_plugin, m_pLuaState2, pszState2Name);
			if (iRetval != 0)
			{
				std::printf("ScopedSledDebugger %i: LuaPlugin: Failed to register Lua state %s (0x%p) with LuaPlugin!\n", m_iInstanceCount, pszState2Name, m_pLuaState2);
				return iRetval;
			}
		}

		std::printf("ScopedSledDebugger %i: Successfully initialized!\n", m_iInstanceCount);

		return iRetval;
	}

	void ScopedSledDebugger::Close()
	{
		if (m_debugger)
			debuggerStopNetworking(m_debugger);

		if (m_plugin)
		{
			if (m_pLuaState1)
				luaPluginUnregisterLuaState(m_plugin, m_pLuaState1);

			if (m_pLuaState2)
				luaPluginUnregisterLuaState(m_plugin, m_pLuaState2);

			luaPluginShutdown(m_plugin);
			m_plugin = 0;
		}

		if (m_pLuaState1)
		{
			::lua_close(m_pLuaState1);
			m_pLuaState1 = 0;
		}

		if (m_pLuaState2)
		{
			::lua_close(m_pLuaState2);
			m_pLuaState2 = 0;
		}

		if (m_debugger)
		{
			debuggerShutdown(m_debugger);
			m_debugger = 0;
		}

		if (m_pLuaMemory)
		{
			delete [] m_pLuaMemory;
			m_pLuaMemory = 0;
		}

		if (m_pMemory)
		{
			delete [] m_pMemory;
			m_pMemory = 0;
		}

		if (m_pFileBuffer)
		{
			delete m_pFileBuffer;
			m_pFileBuffer = 0;
		}
	}

	int32_t ScopedSledDebugger::AddLuaplugin(uint32_t maxSendBufferSize,
										 uint16_t maxLuaStates,
										 uint16_t maxLuaStateNameLen,
										 uint32_t maxMemTraces,
										 uint16_t maxBreakpoints,
										 uint16_t maxEditAndContinues,
										 uint16_t maxEditAndContinueEntryLen,
										 uint16_t maxNumVarFilters,
										 uint16_t maxVarFilterPatternLen,
										 uint16_t maxPatternsPerVarFilter,
										 uint16_t maxProfileFunctions,
										 uint16_t maxProfileCallStackDepth,
										 int32_t numPathChopChars,
										 ChopCharsCallback pfnChopCharsCallback,
										 EditAndContinueCallback pfnEditAndContinueCallback,
										 EditAndContinueFinishCallback pfnEditAndContinueFinishCallback,
										 void *pEditAndContinueUserData)
	{
		if (m_plugin)
		{
			std::printf("ScopedSledDebugger %i: Lua plugin already added!\n", m_iInstanceCount);
			return -1;
		}

		int32_t iRetval = 0;

		{
			LuaPluginConfig config;
			config.maxSendBufferSize = maxSendBufferSize;
			config.maxLuaStates = maxLuaStates;
			config.maxLuaStateNameLen = maxLuaStateNameLen;
			config.maxMemTraces = maxMemTraces;
			config.maxBreakpoints = maxBreakpoints;
			config.maxEditAndContinues = maxEditAndContinues;
			config.maxEditAndContinueEntryLen = maxEditAndContinueEntryLen;
			config.maxNumVarFilters = maxNumVarFilters;
			config.maxVarFilterPatternLen = maxVarFilterPatternLen;
			config.maxPatternsPerVarFilter = maxPatternsPerVarFilter;
			config.maxProfileFunctions = maxProfileFunctions;
			config.maxProfileCallStackDepth = maxProfileCallStackDepth;
			config.numPathChopChars = numPathChopChars;
			config.pfnChopCharsCallback = pfnChopCharsCallback;
			config.pfnEditAndContinueCallback = pfnEditAndContinueCallback;
			config.pfnEditAndContinueFinishCallback = pfnEditAndContinueFinishCallback;
			config.pEditAndContinueUserData = pEditAndContinueUserData;

			std::size_t iMemSize;
			iRetval = luaPluginRequiredMemory(&config, &iMemSize);
			if (iRetval != 0)
			{
				std::printf("ScopedSledDebugger %i: Error calculating required memory for LuaPlugin! %i\n", m_iInstanceCount, iRetval);
				return iRetval;
			}

			m_pLuaMemory = new char[iMemSize];
			SCE_SLED_ASSERT(m_pLuaMemory != NULL);

			std::printf("ScopedSledDebugger %i: Creating Lua Plugin (%lu bytes)...", m_iInstanceCount, (unsigned long)iMemSize);

			iRetval = luaPluginCreate(&config, m_pLuaMemory, &m_plugin);
			if (iRetval != 0)
			{
				std::printf("ScopedSledDebugger %i: Error creating LuaPlugin instance! %i\n", m_iInstanceCount, iRetval);
				return iRetval;
			}

			std::printf("done!\n");
		}

		{
			Version verInfo;
			luaPluginGetVersion(m_plugin, &verInfo);
			std::printf("ScopedSledDebugger %i: LuaPlugin version %i.%i.%i\n", m_iInstanceCount, (int)verInfo.majorNum, (int)verInfo.minorNum, (int)verInfo.revisionNum);
		}

		return debuggerAddLuaPlugin(m_debugger, m_plugin);
	}
}
