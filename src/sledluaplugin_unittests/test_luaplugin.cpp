/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sledluaplugin/sledluaplugin.h"
#include "../sleddebugger/sleddebugger.h"
#include "../sleddebugger/utilities.h"
#include "../sledluaplugin/luautils.h"
#include "../sledluaplugin/scmp.h"

#include <unittest-cpp/UnitTest++/UnitTest++.h>
#include <string>

#include "logstealer.h"

#include <wws_lua/extras/Lua.Utilities/LuaInterface.h>

namespace sce { namespace Sled { namespace
{
	class HostedLuaPluginConfig
	{
	public:	
		LuaPluginConfig Default()
		{
			LuaPluginConfig config;
			Setup(config);
			return config;
		}

		LuaPluginConfig DefaultWithCustomLuaStates(uint16_t maxStates)
		{
			LuaPluginConfig config;
			Setup(config);
			config.maxLuaStates = maxStates;
			return config;
		}
	private:
		static void Setup(LuaPluginConfig& config)
		{
			config.maxSendBufferSize = 2048;
			config.maxLuaStates = 1;
			config.maxMemTraces = 0;
			config.maxBreakpoints = 64;
			config.maxEditAndContinues = 0; 
			config.maxEditAndContinueEntryLen = 0;
			config.maxNumVarFilters = 0;
			config.maxVarFilterPatternLen = 0;
			config.maxPatternsPerVarFilter = 0;
			config.maxProfileFunctions = 0;
			/*config.maxProfileFunctionCalls = 0;*/
			config.maxProfileCallStackDepth = 0;
			config.maxWorkBufferSize = 1024;
		}
	};

	class HostedLuaPlugin
	{
	public:
		HostedLuaPlugin()
		{
			m_plugin = 0;
			m_pluginMem = 0;

			m_debugger = 0;
			m_debuggerMem = 0;
		}

		~HostedLuaPlugin()
		{
			if (m_plugin)
			{
				luaPluginShutdown(m_plugin);
				m_plugin = 0;
			}

			if (m_debugger)
			{
				debuggerShutdown(m_debugger);
				m_debugger = 0;
			}

			if (m_pluginMem)
			{
				delete [] m_pluginMem;
				m_pluginMem = 0;
			}

			if (m_debuggerMem)
			{
				delete [] m_debuggerMem;
				m_debuggerMem = 0;
			}
		}

		int32_t Setup(const LuaPluginConfig& config)
		{
			std::size_t iMemSize;

			const int32_t iError = luaPluginRequiredMemory(&config, &iMemSize);
			if (iError != 0)
				return iError;

			m_pluginMem = new char[iMemSize];
			if (!m_pluginMem)
				return -1;

			return luaPluginCreate(&config, m_pluginMem, &m_plugin);
		}

		int32_t CreateDebuggerAndAddPlugin(SledDebugger **ppDebugger)
		{
			if (!ppDebugger)
				return -1;

			if (!m_plugin)
				return -2;

			SledDebuggerConfig config;
			std::size_t iMemSize;

			int32_t iRetval = debuggerRequiredMemory(&config, &iMemSize);
			if (iRetval != 0)
				return iRetval;

			m_debuggerMem = new char[iMemSize];
			if (!m_debuggerMem)
				return -3;

			iRetval = debuggerCreate(&config, m_debuggerMem, &m_debugger);
			if (iRetval != 0)
				return iRetval;

			*ppDebugger = m_debugger;

			iRetval = debuggerAddLuaPlugin((*ppDebugger), m_plugin);
			if (iRetval != 0)
				return iRetval;

			return 0;
		}

		LuaPlugin *m_plugin;

	private:
		char *m_pluginMem;

	private:
		SledDebugger *m_debugger;
		char *m_debuggerMem;
	};

	struct Fixture
	{
		Fixture()
		{
		}

		HostedLuaPlugin host;
		HostedLuaPluginConfig config;
	};

	namespace LuaHelpers
	{
		void OpenLibs(lua_State *state)
		{
			LuaInterface::OpenLibs(state);
		}
	}

	TEST_FIXTURE(Fixture, LuaPlugin_Create)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));
	}

	TEST_FIXTURE(Fixture, LuaPlugin_VerifyId)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		uint16_t pluginId;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetId(host.m_plugin, &pluginId));
		CHECK_EQUAL(SCE_LIBSLEDLUAPLUGIN_ID, pluginId);
	}

	TEST_FIXTURE(Fixture, LuaPlugin_VerifyName)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		const char *pluginName = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetName(host.m_plugin, &pluginName));
		CHECK_EQUAL(true, Utilities::areStringsEqual(SCE_LIBSLEDLUAPLUGIN_NAME, pluginName));
	}

	TEST_FIXTURE(Fixture, LuaPlugin_VerifyVersion)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		Version version;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVersion(host.m_plugin, &version));

		CHECK_EQUAL(SCE_LIBSLEDLUAPLUGIN_VER_MAJOR, version.majorNum);
		CHECK_EQUAL(SCE_LIBSLEDLUAPLUGIN_VER_MINOR, version.minorNum);
		CHECK_EQUAL(SCE_LIBSLEDLUAPLUGIN_VER_REVISION, version.revisionNum);
	}

	TEST_FIXTURE(Fixture, LuaPlugin_LuaState_NoDebugger_RegisterValidState)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		lua_State *state = LuaInterface::Open();
		CHECK_EQUAL((int32_t)SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));
		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaPlugin_LuaState_NoDebugger_RegisterInvalidState)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));
		CHECK_EQUAL((int32_t)SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE, luaPluginRegisterLuaState(host.m_plugin, 0, "Main"));
	}

	TEST_FIXTURE(Fixture, LuaPlugin_LuaState_NoDebugger_UnregisterValidState)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		lua_State *state = LuaInterface::Open();
		CHECK_EQUAL((int32_t)SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE, luaPluginUnregisterLuaState(host.m_plugin, state));
		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaPlugin_LuaState_NoDebugger_UnregisterInvalidState)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));
		CHECK_EQUAL((int32_t)SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE, luaPluginUnregisterLuaState(host.m_plugin, 0));
	}

	TEST_FIXTURE(Fixture, LuaPlugin_LuaState_Debugger_RegisterAndUnregisterValidState)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));
	
		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));

		lua_State *state = LuaInterface::Open();
		CHECK_EQUAL(0, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));
		CHECK_EQUAL(0, luaPluginUnregisterLuaState(host.m_plugin, state));
		LuaInterface::Close(state);	
	}

	TEST_FIXTURE(Fixture, LuaPlugin_LuaState_Debugger_RegisterAndUnregisterInvalidState)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));

		CHECK_EQUAL((int32_t)SCE_SLED_LUA_ERROR_INVALIDLUASTATE, luaPluginRegisterLuaState(host.m_plugin, 0, "Main"));
		CHECK_EQUAL((int32_t)SCE_SLED_LUA_ERROR_INVALIDLUASTATE, luaPluginUnregisterLuaState(host.m_plugin, 0));
	}

	TEST_FIXTURE(Fixture, LuaPlugin_LuaState_Debugger_UnregisterValidState)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));

		lua_State *state = LuaInterface::Open();
		CHECK_EQUAL((int32_t)SCE_SLED_LUA_ERROR_LUASTATENOTFOUND, luaPluginUnregisterLuaState(host.m_plugin, state));
		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaPlugin_LuaState_Debugger_RegisterDuplicateValidState)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));

		lua_State *state = LuaInterface::Open();
		CHECK_EQUAL(0, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));
		CHECK_EQUAL((int32_t)SCE_SLED_LUA_ERROR_DUPLICATELUASTATE, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));
		CHECK_EQUAL(0, luaPluginUnregisterLuaState(host.m_plugin, state));
		LuaInterface::Close(state);	
	}

	TEST_FIXTURE(Fixture, LuaPlugin_LuaState_Debugger_RegisterAndUnregisterMultipleStates)
	{
		const uint16_t maxStates = 16;
		lua_State *states[maxStates];

		CHECK_EQUAL(0, host.Setup(config.DefaultWithCustomLuaStates(maxStates)));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));

		for (uint16_t i = 0; i < maxStates; i++)
		{
			const std::size_t kNameLen = 4;
			char name[kNameLen];
			StringUtilities::copyString(name, kNameLen, "%u", i);

			states[i] = LuaInterface::Open();
			CHECK_EQUAL(0, luaPluginRegisterLuaState(host.m_plugin, states[i], name));
		}

		for (uint16_t i = 0; i < maxStates; i++)
		{
			CHECK_EQUAL(0, luaPluginUnregisterLuaState(host.m_plugin, states[i]));
			LuaInterface::Close(states[i]);	
		}
	}

	TEST(LuaPlugin_LuaState_Debugger_RegisterSingleStateWithMultipleLuaPlugins)
	{
		HostedLuaPluginConfig config;

		HostedLuaPlugin host1;
		CHECK_EQUAL(0, host1.Setup(config.Default()));

		HostedLuaPlugin host2;
		CHECK_EQUAL(0, host2.Setup(config.Default()));

		SledDebugger *pDebugger1 = 0;
		CHECK_EQUAL(0, host1.CreateDebuggerAndAddPlugin(&pDebugger1));

		SledDebugger *pDebugger2 = 0;
		CHECK_EQUAL(0, host2.CreateDebuggerAndAddPlugin(&pDebugger2));

		lua_State *state = LuaInterface::Open();
		LuaHelpers::OpenLibs(state);

		// Register with one library
		CHECK_EQUAL(0, luaPluginRegisterLuaState(host1.m_plugin, state, "Main"));

		// Register with another library
		{
			LogStealer logStealer;
			CHECK_EQUAL((int32_t)SCE_SLED_LUA_ERROR_LUASTATEALREADYREGISTERED, luaPluginRegisterLuaState(host2.m_plugin, state, "Main"));
		}

		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaPlugin_LuaState_Debugger_RegisterParentAndChildStates)
	{
		CHECK_EQUAL(0, host.Setup(config.DefaultWithCustomLuaStates(2)));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));

		lua_State *parent = LuaInterface::Open();
		LuaHelpers::OpenLibs(parent);

		lua_State *child = LuaInterface::NewThread(parent);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginRegisterLuaState(host.m_plugin, parent, "Parent"));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginRegisterLuaState(host.m_plugin, child, "Child"));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginUnregisterLuaState(host.m_plugin, child));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginUnregisterLuaState(host.m_plugin, parent));

		LuaInterface::Close(parent);
	}

	TEST_FIXTURE(Fixture, LuaPlugin_ProfilerBasicChecks)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginIsProfilerRunning(host.m_plugin, &bRetval));
		CHECK_EQUAL(false, bRetval);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginResetProfileInfo(host.m_plugin));
	}

	TEST_FIXTURE(Fixture, LuaPlugin_MemoryTracerBasicChecks)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginIsMemoryTracerRunning(host.m_plugin, &bRetval));
		CHECK_EQUAL(false, bRetval);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginResetMemoryTrace(host.m_plugin));
	}

	TEST_FIXTURE(Fixture, LuaPlugin_GetAndSetVarExcludeFlags)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		int32_t flags = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVarExcludeFlags(host.m_plugin, &flags));
		CHECK_EQUAL(VarExcludeFlags::kNone, flags);
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginSetVarExcludeFlags(host.m_plugin, VarExcludeFlags::kGlobals));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVarExcludeFlags(host.m_plugin, &flags));
		CHECK_EQUAL(VarExcludeFlags::kGlobals, flags & VarExcludeFlags::kGlobals);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginSetVarExcludeFlags(host.m_plugin, VarExcludeFlags::kNone));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVarExcludeFlags(host.m_plugin, &flags));
		CHECK_EQUAL(VarExcludeFlags::kNone, flags);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginSetVarExcludeFlags(host.m_plugin, VarExcludeFlags::kLocals));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVarExcludeFlags(host.m_plugin, &flags));
		CHECK_EQUAL(VarExcludeFlags::kLocals, flags & VarExcludeFlags::kLocals);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginSetVarExcludeFlags(host.m_plugin, VarExcludeFlags::kNone));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVarExcludeFlags(host.m_plugin, &flags));
		CHECK_EQUAL(VarExcludeFlags::kNone, flags);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginSetVarExcludeFlags(host.m_plugin, VarExcludeFlags::kUpvalues));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVarExcludeFlags(host.m_plugin, &flags));
		CHECK_EQUAL(VarExcludeFlags::kUpvalues, flags & VarExcludeFlags::kUpvalues);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginSetVarExcludeFlags(host.m_plugin, VarExcludeFlags::kNone));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVarExcludeFlags(host.m_plugin, &flags));
		CHECK_EQUAL(VarExcludeFlags::kNone, flags);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginSetVarExcludeFlags(host.m_plugin, VarExcludeFlags::kEnvironment));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVarExcludeFlags(host.m_plugin, &flags));
		CHECK_EQUAL(VarExcludeFlags::kEnvironment, flags & VarExcludeFlags::kEnvironment);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginSetVarExcludeFlags(host.m_plugin, VarExcludeFlags::kNone));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVarExcludeFlags(host.m_plugin, &flags));
		CHECK_EQUAL(VarExcludeFlags::kNone, flags);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginSetVarExcludeFlags(host.m_plugin, VarExcludeFlags::kGlobals | VarExcludeFlags::kLocals | VarExcludeFlags::kUpvalues | VarExcludeFlags::kEnvironment));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVarExcludeFlags(host.m_plugin, &flags));
		CHECK_EQUAL(VarExcludeFlags::kGlobals, flags & VarExcludeFlags::kGlobals);
		CHECK_EQUAL(VarExcludeFlags::kLocals, flags & VarExcludeFlags::kLocals);
		CHECK_EQUAL(VarExcludeFlags::kUpvalues, flags & VarExcludeFlags::kUpvalues);
		CHECK_EQUAL(VarExcludeFlags::kEnvironment, flags & VarExcludeFlags::kEnvironment);
	}

	TEST_FIXTURE(Fixture, LuaPlugin_Lua_CheckSledDebuggerPointerExists)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));	

		lua_State *state = LuaInterface::Open();
		LuaHelpers::OpenLibs(state);
		CHECK_EQUAL(0, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));

		const char *pszTableName = "libsleddebugger";
		const char *pszItemName = "instance";

		// Verify item exists in the Lua state	
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsTable(state, -1));

		LuaInterface::GetField(state, -1, pszItemName);
		CHECK_EQUAL(true, LuaInterface::IsLightUserdata(state, -1));

		// Verify the LuaPlugin exists
		SledDebugger *pSledDebugger = static_cast<SledDebugger*>(LuaInterface::ToUserdata(state, -1));
		CHECK_EQUAL(true, pSledDebugger != NULL);
		CHECK_EQUAL(true, pSledDebugger == pDebugger);
	
		CHECK_EQUAL(0, luaPluginUnregisterLuaState(host.m_plugin, state));

		// Verify table gone
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsNil(state, -1));

		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaPluain_Lua_CheckSledDebuggerVersionExists)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));	

		lua_State *state = LuaInterface::Open();
		LuaHelpers::OpenLibs(state);
		CHECK_EQUAL(0, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));

		const char *pszTableName = "libsleddebugger";
		const char *pszItemName = "version";

		// Verify item exists in the Lua state	
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsTable(state, -1));

		LuaInterface::GetField(state, -1, pszItemName);
		CHECK_EQUAL(1, LuaInterface::IsString(state, -1));

		sce::Sled::Version ver;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerGetVersion(pDebugger, &ver));

		char szVerString[256];
		StringUtilities::copyString(szVerString, 256, "%i.%i.%i", ver.majorNum, ver.minorNum, ver.revisionNum);
		CHECK_EQUAL(true, Utilities::areStringsEqual(szVerString, LuaInterface::ToString(state, -1)));

		CHECK_EQUAL(0, luaPluginUnregisterLuaState(host.m_plugin, state));

		// Verify table gone
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsNil(state, -1));

		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaPlugin_Lua_CheckLuaPluginPointerExists)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));	

		lua_State *state = LuaInterface::Open();
		LuaHelpers::OpenLibs(state);
		CHECK_EQUAL(0, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));

		const char *pszTableName = "libsledluaplugin";
		const char *pszItemName = "instance";

		// Verify item exists in the Lua state	
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsTable(state, -1));

		LuaInterface::GetField(state, -1, pszItemName);
		CHECK_EQUAL(true, LuaInterface::IsLightUserdata(state, -1));

		// Verify the LuaPlugin exists
		LuaPlugin *pPlugin = static_cast<LuaPlugin*>(LuaInterface::ToUserdata(state, -1));
		CHECK_EQUAL(true, pPlugin != NULL);
		CHECK_EQUAL(true, pPlugin == host.m_plugin);
	
		CHECK_EQUAL(0, luaPluginUnregisterLuaState(host.m_plugin, state));

		// Verify table gone
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsNil(state, -1));

		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaPluain_Lua_CheckVersionExists)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));	

		lua_State *state = LuaInterface::Open();
		LuaHelpers::OpenLibs(state);
		CHECK_EQUAL(0, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));

		const char *pszTableName = "libsledluaplugin";
		const char *pszItemName = "version";

		// Verify item exists in the Lua state	
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsTable(state, -1));

		LuaInterface::GetField(state, -1, pszItemName);
		CHECK_EQUAL(1, LuaInterface::IsString(state, -1));

		sce::Sled::Version ver;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, luaPluginGetVersion(host.m_plugin, &ver));

		char szVerString[256];
		StringUtilities::copyString(szVerString, 256, "%i.%i.%i", ver.majorNum, ver.minorNum, ver.revisionNum);
		CHECK_EQUAL(true, Utilities::areStringsEqual(szVerString, LuaInterface::ToString(state, -1)));

		CHECK_EQUAL(0, luaPluginUnregisterLuaState(host.m_plugin, state));

		// Verify table gone
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsNil(state, -1));

		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaPluain_Lua_CheckAssertExists)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));	

		lua_State *state = LuaInterface::Open();
		LuaHelpers::OpenLibs(state);
		CHECK_EQUAL(0, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));

		const char *pszTableName = "libsledluaplugin";
		const char *pszItemName = "assert";

		// Verify item exists in the Lua state	
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsTable(state, -1));

		LuaInterface::GetField(state, -1, pszItemName);
		CHECK_EQUAL(1, LuaInterface::IsCFunction(state, -1));

		CHECK_EQUAL(0, luaPluginUnregisterLuaState(host.m_plugin, state));

		// Verify table gone
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsNil(state, -1));

		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaPluain_Lua_CheckTTYExists)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));	

		lua_State *state = LuaInterface::Open();
		LuaHelpers::OpenLibs(state);
		CHECK_EQUAL(0, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));

		const char *pszTableName = "libsledluaplugin";
		const char *pszItemName = "tty";

		// Verify item exists in the Lua state	
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsTable(state, -1));

		LuaInterface::GetField(state, -1, pszItemName);
		CHECK_EQUAL(1, LuaInterface::IsCFunction(state, -1));

		CHECK_EQUAL(0, luaPluginUnregisterLuaState(host.m_plugin, state));

		// Verify table gone
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsNil(state, -1));

		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaPlugin_Lua_CheckUserDataExists)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));	

		lua_State *state = LuaInterface::Open();
		LuaHelpers::OpenLibs(state);
		CHECK_EQUAL(0, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));

		const char *pszTableName = "libsledluaplugin";

		// Verify table exists in the Lua state	
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsTable(state, -1));
		LuaInterface::Pop(state, 1);

		// Nagata: I changed the spec of userdata callback.
		// It is not registered at LuaPlugin() any more.
		// It may exist or not.

		CHECK_EQUAL(0, luaPluginUnregisterLuaState(host.m_plugin, state));

		// Verify table gone
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsNil(state, -1));

		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaPlugin_Lua_CheckEditAndContinueExists)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		SledDebugger *pDebugger = 0;
		CHECK_EQUAL(0, host.CreateDebuggerAndAddPlugin(&pDebugger));	

		lua_State *state = LuaInterface::Open();
		LuaHelpers::OpenLibs(state);
		CHECK_EQUAL(0, luaPluginRegisterLuaState(host.m_plugin, state, "Main"));

		const char *pszTableName = "libsledluaplugin";
		const char *pszSubTableName = "editandcontinue";

		// Verify table exists in the Lua state	
		LuaInterface::GetGlobal(state, pszTableName);
		CHECK_EQUAL(true, LuaInterface::IsTable(state, -1));

		// Verify subtable exists in the Lua state
		LuaInterface::GetField(state, -1, pszSubTableName);
		CHECK_EQUAL(true, LuaInterface::IsTable(state, -1));

		// Verify items exist in sub table
		LuaInterface::GetField(state, -1, "callback");
		CHECK_EQUAL(true, LuaInterface::IsLightUserdata(state, -1));
		LuaInterface::Pop(state, 1);
		LuaInterface::GetField(state, -1, "finishcallback");
		CHECK_EQUAL(true, LuaInterface::IsLightUserdata(state, -1));
		LuaInterface::Pop(state, 1);
		LuaInterface::GetField(state, -1, "userdata");
		CHECK_EQUAL(true, LuaInterface::IsLightUserdata(state, -1));
		LuaInterface::Pop(state, 1);

		CHECK_EQUAL(0, luaPluginUnregisterLuaState(host.m_plugin, state));

		// Verify table gone // libsledluapluginconfig is not removed because it is created by users.
		//LuaInterface::GetGlobal(state, pszTableName);
		//CHECK_EQUAL(true, LuaInterface::IsNil(state, -1));

		LuaInterface::Close(state);
	}

	TEST(Lua_SCMP_Sizes_Ptr)
	{
		// make sure pointer fits in char[SCMP::Sizes::kPtrLen]

		char temp1[SCMP::Sizes::kPtrLen];
		char temp2[256];
	
		int x = 25;
		int *ptrX = &x;

		StringUtilities::copyString(temp1, SCMP::Sizes::kPtrLen, "0x%p", ptrX);
		StringUtilities::copyString(temp2, 256, "0x%p", ptrX);
		CHECK_EQUAL(std::strlen(temp1), std::strlen(temp2));
		CHECK_EQUAL(true, Utilities::areStringsEqual(temp1, temp2));	
	}
}}}
