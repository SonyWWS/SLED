/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDLUAPLUGIN_SLEDLUAPLUGIN_H__
#define __SCE_LIBSLEDLUAPLUGIN_SLEDLUAPLUGIN_H__

#include "../sleddebugger/plugin.h"
#include "../sleddebugger/common.h"
#include "sledluaplugin.h"

// Forward declarations
struct lua_State;
struct lua_Debug;
struct SceSledPlatformMutex;

namespace sce { namespace Sled
{
	// Forward declarations
	class NetworkBuffer;
	class NetworkBufferReader;
	class StringArray;
	class ProfileStack;
	struct LuaStateParams;
	struct MemTraceParams;
	class Breakpoint;
	class VarFilterNameContainer;
	struct LuaVariable;
	struct SledLuaVariable;

	/// @cond
	namespace SCMP { struct VarLookUp; }

	const static uint16_t kLuaPluginId = SCE_LIBSLEDLUAPLUGIN_ID;

	#define SCE_SLED_SLEDDEBUGGER_TABLE_STRING		"libsleddebugger"
	#define SCE_SLED_SLEDDEBUGGER_INSTANCE_STRING	"instance"
	#define SCE_SLED_SLEDDEBUGGER_VER_STRING		"version"

	#define SCE_SLED_LUAPLUGIN_TABLE_STRING			"libsledluaplugin"
	#define SCE_SLED_LUAPLUGIN_INSTANCE_STRING		"instance"
	#define SCE_SLED_LUAPLUGIN_VER_STRING			"version"
	#define SCE_SLED_LUAPLUGIN_ASSERT_STRING		"assert"
	#define SCE_SLED_LUAPLUGIN_BP_FUNC_STRING		"bp_func"
	#define SCE_SLED_LUAPLUGIN_TTY_STRING			"tty"
	#define SCE_SLED_LUAPLUGIN_ERRORHANDLER_STRING	"errorhandler"

	#define SCE_SLED_LUAPLUGIN_USERDATATOSTRING_STRING					"userdatatostring"
	#define SCE_SLED_LUAPLUGIN_USERDATA_STRING							"userdata"
	#define SCE_SLED_LUAPLUGIN_USERDATA_CALLBACK_STRING					"callback"
	#define SCE_SLED_LUAPLUGIN_USERDATA_FINISH_CALLBACK_STRING			"finishcallback"
	#define SCE_SLED_LUAPLUGIN_USERDATA_CALLBACK_USERDATA_STRING		"userdata"

	#define SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_STRING					"editandcontinue"
	#define SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_CALLBACK_STRING			"callback"
	#define SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_FINISH_CALLBACK_STRING	"finishcallback"
	#define SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_CALLBACK_USERDATA_STRING	"userdata"

	namespace LuaVariableScope
	{
		enum Enum
		{
			kGlobal			= 0,
			kLocal			= 1,
			kUpvalue		= 2,
			kEnvironment	= 3,
		};
	}

	namespace LuaVariableContext
	{
		enum Enum
		{
			kNormal			= 0,
			kWatchProject	= 1,
			kWatchCustom	= 2,
		};
	}
	/// @endcond

	/**
	@brief Class describing a Lua plugin instance.

	Widely used class encapsulating the internals of a Lua plugin instance.
	Instantiate a <c>LuaPlugin</c> from a <c>LuaPluginConfig</c>.

	This class is closed, and its internal data is not accessible.

	The following are the main functions handling <c>SledDebugger</c>:

	<c>sce::Sled::luaPluginCreate()</c>: Create <c>LuaPlugin</c> instance.

	<c>sce::Sled::luaPluginRequiredMemory()</c>: Calculate size in bytes required for <c>LuaPlugin</c> instance based on configuration structure.

	<c>sce::Sled::luaPluginShutdown()</c>: Shut down <c>LuaPlugin</c> instance.

	<c>sce::Sled::luaPluginGetId()</c>: Get ID of plugin.

	<c>sce::Sled::luaPluginRegisterLuaState()</c>: Register <c>lua_State</c>* with library.

	<c>sce::Sled::luaPluginUnregisterLuaState()</c>: Unregister <c>lua_State</c>* from library.

	<c>sce::Sled::luaPluginIsProfilerRunning()</c>: Check whether profiler is running.

	<c>sce::Sled::luaPluginIsMemoryTracerRunning()</c>: Check whether memory tracer is running.

	<c>sce::Sled::luaPluginDebuggerBreak()</c>: Force breakpoint on specific <c>lua_State</c> and send data via TTY.

	<c>sce::Sled::luaPluginMemoryTraceNotify()</c>: Provide way to report Lua allocations to library for tracking with memory tracer.

	<c>sce::Sled::debuggerAddLuaPlugin()</c>: Add <c>LuaPlugin</c> to <c>SledDebugger</c>.

	For the full list of <c>LuaPlugin</c> functions, see <c>sce::Sled</c>.
	*/
	class SCE_SLED_LINKAGE LuaPlugin : public SledDebuggerPlugin
	{
	/// @cond
	public:		
		static int32_t create(const LuaPluginConfig& luaConfig, void *pLocation, LuaPlugin **ppLuaPlugin);
		static int32_t requiredMemory(const LuaPluginConfig& luaConfig, std::size_t *iRequiredMemory);		
		static void close(LuaPlugin *pPlugin);
	private:
		LuaPlugin(const LuaPluginConfig& luaConfig, const void *pPluginSeats);
		virtual ~LuaPlugin();
		LuaPlugin(const LuaPlugin&) : SledDebuggerPlugin(), m_iMaxLuaStates(0), m_iMaxLuaStateNameLen(0), m_iMaxMemTraces(0), m_iMaxBreakpoints(0), m_iWorkBufMaxSize(0) {}
		LuaPlugin& operator=(const LuaPlugin&) { return *this; }
	private:
		virtual void shutdown();
	public:		
		virtual uint16_t getId() const { return SCE_LIBSLEDLUAPLUGIN_ID; }		
		virtual const char *getName() const { return SCE_LIBSLEDLUAPLUGIN_NAME; }		
		virtual const Version getVersion() const;
	private:
		virtual void clientConnected();
		virtual void clientDisconnected();
		virtual void clientMessage(const uint8_t *pData, int32_t iSize);
		virtual void clientBreakpointBegin(const BreakpointParams *pParams);
		virtual void clientBreakpointEnd(const BreakpointParams *pParams);
		virtual void clientDebugModeChanged(DebuggerMode::Enum newMode);
	private:
		void clientDisconnectedLua();
		void clientBreakpointBeginLua(const BreakpointParams *pParams);
		void clientDebugModeChangedLua(DebuggerMode::Enum newMode);
	public:						
		int32_t registerLuaState(lua_State *luaState, const char *pszName = 0);
		int32_t unregisterLuaState(lua_State *luaState);
		void resetProfileInfo();
		inline bool isProfilerRunning() const { return m_bProfilerRunning; }
		void resetMemoryTrace();
		inline bool isMemoryTracerRunning() const { return m_bMemoryTracerRunning; }
		void debuggerBreak(lua_State *luaState, const char *pszText);
		void debuggerBreak(const char *pszText);
		inline void setVarExcludeFlags(int32_t iFlags) { m_iVarExcludeFlags = iFlags; }
		inline int32_t getVarExcludeFlags() const { return m_iVarExcludeFlags; }
		bool memoryTraceNotify(void *ud, void *oldPtr, void *newPtr, std::size_t oldSize, std::size_t newSize);
		int32_t getErrorHandlerAbsStackIndex(lua_State *luaState, int *outAbsStackIndex);
	private:
		static LuaPlugin *getWhichLuaPlugin(lua_State *luaState);
		static void hookFunc(lua_State *luaState, lua_Debug *ar);
		static int luaAssert(lua_State *luaState);
		static int luaTTY(lua_State *luaState);
		static int luaErrorHandler(lua_State *luaState);	
	private:
		int32_t ttyNotify(const char *pszMessage);
		const char *trimFileName(const char *pszFileName);
		void luaAssertInternal(lua_State *luaState);
		void luaErrorHandlerInternal(lua_State *luaState);
		void hookFunc_Profiler(lua_State *luaState, lua_Debug *ar);
		void hookFunc_Breakpoint(lua_State *luaState, lua_Debug *ar);
		void tagFuncForLookUp(char *pszBuffer, std::size_t iBufLen, const char *pszFuncName, const char *pszFileName, const int32_t& iLine);
		bool isLineBreakpoint(lua_State *luaState, const char *pszSource, const int32_t& iCurrentLine);
	private:
		int32_t							m_iPathChopChars;
		ChopCharsCallback				m_pfnChopCharsCallback;

		EditAndContinueCallback			m_pfnEditAndContinueCallback;
		EditAndContinueFinishCallback	m_pfnEditAndContinueFinishCallback;
		void							*m_pEditAndContinueUserData;
	private:
		bool			m_bLookUpWatches;
		int32_t			m_iVarExcludeFlags;

		lua_State*		m_pCurHookLuaState;
		lua_Debug*		m_pCurHookLuaDebug;
		bool			m_bHitBreakpoint;
		const char*		m_pszSource;
		int				m_iLastNumStackLevels;
		bool			m_bAssertBreakpoint;
		bool			m_bErrorBreakpoint;

		NetworkBuffer	*m_pSendBuf;
		ProfileStack	*m_pProfileStack;

		const uint16_t	m_iMaxLuaStates;
		uint16_t		m_iNumLuaStates;
		LuaStateParams	*m_pLuaStates;
		const uint16_t	m_iMaxLuaStateNameLen;
		char			*m_pLuaStatesNames;

		const uint32_t	m_iMaxMemTraces;
		uint32_t		m_iNumMemTraces;
		MemTraceParams	*m_pMemTraces;

		bool m_bProfilerRunning;
		bool m_bMemoryTracerRunning;

		const uint16_t	m_iMaxBreakpoints;
		uint16_t		m_iNumBreakpoints;
		Breakpoint		*m_pBreakpoints;

		VarFilterNameContainer *m_pVarFilterNames;

		bool m_bGlobalVarFilterType[9];
		bool m_bLocalVarFilterType[9];
		bool m_bUpvalueVarFilterType[9];
		bool m_bEnvVarVarFilterTypes[9];

		StringArray *m_pEditAndContinue;
		SceSledPlatformMutex *m_pMutex;

		uint8_t *m_pWorkBuf;
		const uint32_t m_iWorkBufMaxSize;
	private:
		inline bool isGlobalVarTypeFiltered(int32_t iVarType) const	{ return m_bGlobalVarFilterType[iVarType]; }
		inline bool isLocalVarTypeFiltered(int32_t iVarType) const { return m_bLocalVarFilterType[iVarType]; }
		inline bool isUpvalueVarTypeFiltered(int32_t iVarType) const { return m_bUpvalueVarFilterType[iVarType]; }
		inline bool isEnvVarVarTypeFiltered(int32_t iVarType) const { return m_bEnvVarVarFilterTypes[iVarType]; }
		bool isVarNameFiltered(const char *pszName, char chWhat) const;
		inline bool isGlobalVarNameFiltered(const char *pszName) const { return isVarNameFiltered(pszName, 'g'); }
		inline bool isLocalVarNameFiltered(const char *pszName) const { return isVarNameFiltered(pszName, 'l'); }
		inline bool isUpvalueVarNameFiltered(const char *pszName) const { return isVarNameFiltered(pszName, 'u'); }
		inline bool isEnvVarVarNameFiltered(const char *pszName) const { return isVarNameFiltered(pszName, 'e'); }
	private:
		void setVariable(lua_State *luaState, const LuaVariable *pVar);
		void lookupVariable(lua_State *luaState, const LuaVariable *pVar);
		void lookupGlobalVariable(lua_State *luaState, const LuaVariable *pVar);
		void lookupLocalVariable(lua_State *luaState, const LuaVariable *pVar);
		void lookupUpvalueVariable(lua_State *luaState, const LuaVariable *pVar);
		void lookupEnvironmentVariable(lua_State *luaState, const LuaVariable *pVar);
		int32_t lookUpTypeVal(lua_State *luaState, int iIndex, char *pValue, const int& iValueLen, bool bPop = true);
		bool getStackIndexInfo(lua_State *luaState, int index, char *pName, int nameStrLen, int32_t *luaType);
		void getTableValues(lua_State *luaState, const LuaVariable *pVar, LuaVariableScope::Enum what, int32_t iVarIndex, int32_t iTableIndex, int32_t iStackLevel);
		bool luaPushValue(lua_State *luaState, int32_t iType, const char *pszValue);
		int32_t getGlobals(lua_State *luaState);
		void sendGlobal(const LuaVariable *parent, const char *name, int32_t nameType, const char *value, int32_t valueType);
		int32_t getLocals(lua_State *luaState, lua_Debug *ar, int32_t iStackLevel);
		void sendLocal(const LuaVariable *parent, const char *name, int32_t nameType, const char *value, int32_t valueType, int32_t stackLevel, int32_t index);
		int32_t getUpvalues(lua_State *luaState, int32_t iFuncIndex, int32_t iStackLevel);
		void sendUpvalue(const LuaVariable *parent, const char *name, int32_t nameType, const char *value, int32_t valueType, int32_t stackLevel, int32_t index);
		void getEnvironment(lua_State *luaState, int iFuncIndex, int32_t iStackLevel);
		void sendEnvVar(const LuaVariable *pVar, const char *name, int32_t nameType, const char *value, int32_t valueType, int32_t iStackLevel);
		void handleEditAndContinue(lua_State *luaState);
	private:
		void handleScmpBreakpointDetails(NetworkBufferReader *pReader);
		void handleScmpVarFilterStateNameBegin(NetworkBufferReader *pReader);
		void handleScmpVarFilterStateName(NetworkBufferReader *pReader);
		void handleScmpVarFilterStateNameEnd(NetworkBufferReader *pReader);
		void handleScmpVarFilterStateTypeBegin(NetworkBufferReader *pReader);
		void handleScmpVarFilterStateType(NetworkBufferReader *pReader);
		void handleScmpVarFilterStateTypeEnd(NetworkBufferReader *pReader);
		void handleScmpVarLookUp(NetworkBufferReader *pReader);
		void handleScmpVarLookUpNormal(SCMP::VarLookUp *pLookUp);
		void handleScmpVarLookUpCustom(SCMP::VarLookUp *pLookUp);
		void handleScmpVarUpdate(NetworkBufferReader *pReader);
		void handleScmpWatchLookUpBegin(NetworkBufferReader *pReader);
		void handleScmpWatchLookUpEnd(NetworkBufferReader *pReader);
		void handleScmpCallStackLookUpPerform(NetworkBufferReader *pReader);
		void handleScmpMemoryTraceToggle(NetworkBufferReader *pReader);
		void handleScmpProfilerToggle(NetworkBufferReader *pReader);
		void handleScmpProfileInfoLookUpPerform(NetworkBufferReader *pReader);
		void handleScmpDevCmd(NetworkBufferReader *pReader);
		void handleScmpEditAndContinue(NetworkBufferReader *pReader);
		void handleScmpLuaStateToggle(NetworkBufferReader *pReader);
	private:
		void handleScmpBreakpointDetailsLua(NetworkBufferReader *pReader);
		void handleScmpVarLookUpCustomLua(SCMP::VarLookUp *pLookUp);
		void handleScmpCallStackLookUpPerformLua(NetworkBufferReader *pReader);
		void handleScmpProfilerToggleLua(NetworkBufferReader *pReader);
		void handleScmpDevCmdLua(NetworkBufferReader *pReader);
		void handleScmpLuaStateToggleLua(NetworkBufferReader *pReader);
		/// @endcond		
	};
}}

#endif // __SCE_LIBSLEDLUAPLUGIN_SLEDLUAPLUGIN_H__
