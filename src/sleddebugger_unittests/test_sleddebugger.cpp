/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sleddebugger/sleddebugger.h"
#include "../sleddebugger/plugin.h"
#include "../sleddebugger/utilities.h"

#include "logstealer.h"

#include <unittest-cpp/UnitTest++/UnitTest++.h>

#include <cstring>
#include <cstdio>
#include <cstdlib>

namespace sce { namespace Sled { namespace
{
	class HostedSledDebuggerConfig
	{
	public:	
		SledDebuggerConfig Default()
		{
			SledDebuggerConfig config;
			Setup(config);
			return config;
		}

		SledDebuggerConfig Invalid()
		{
			SledDebuggerConfig config;
			config.maxPlugins = 0;
			config.maxScriptCacheEntries = 0;
			config.maxScriptCacheEntryLen = 0;
			config.maxRecvBufferSize = 0;
			config.maxSendBufferSize = 0;
			return config;
		}

		SledDebuggerConfig BareMinimum()
		{
			SledDebuggerConfig config;
			Setup(config);
			config.maxScriptCacheEntries = 0;
			config.maxScriptCacheEntryLen = 0;
			return config;
		}

		SledDebuggerConfig DefaultWithCustomPluginCount(uint16_t maxPlugins)
		{
			SledDebuggerConfig config;
			Setup(config);
			config.maxPlugins = maxPlugins;
			return config;
		}

		SledDebuggerConfig DefaultWithCustomNetwork(Protocol::Enum protocol, uint16_t iPort, bool bWaitForClientConnect)
		{
			SledDebuggerConfig config;
			Setup(config);
			config.net.setup(protocol, iPort, bWaitForClientConnect);
			return config;
		}

		SledDebuggerConfig DefaultWithCustomScriptCache(uint16_t maxEntries, uint16_t maxEntryLen)
		{
			SledDebuggerConfig config;
			Setup(config);
			config.maxScriptCacheEntries = maxEntries;
			config.maxScriptCacheEntryLen = maxEntryLen;
			return config;
		}
	private:
		static void Setup(SledDebuggerConfig& config)
		{
			config.maxPlugins = 1;
			config.maxScriptCacheEntries = 64;
			config.maxScriptCacheEntryLen = 64;
			config.maxRecvBufferSize = 2048;
			config.maxSendBufferSize = 2048;
			config.net.setup(Protocol::kTcp, 11111, false);
		}
	};

	class HostedSledDebugger
	{
		class HostedSledDebuggerPlugin : public SledDebuggerPlugin
		{
		public:
			HostedSledDebuggerPlugin(uint16_t id, const char *pszName = "HostedPlugin")
			{
				m_id = id;
				std::sprintf(m_name, "%s%u", pszName, id);
			}	
			virtual ~HostedSledDebuggerPlugin() {}
		public:
			virtual uint16_t getId() const { return m_id; }
			virtual const char *getName() const { return m_name; }
			virtual const Version getVersion() const { const Version ver(1, 2, 3); return ver; }
		private:
			virtual void shutdown() {}
			virtual void clientConnected() {}
			virtual void clientDisconnected() {}
			virtual void clientMessage(const uint8_t *pData, int32_t iSize) { (void)pData; (void)iSize; }
			virtual void clientBreakpointBegin(const BreakpointParams *pParams) { (void)pParams; }
			virtual void clientBreakpointEnd(const BreakpointParams *pParams) { (void)pParams; }
			virtual void clientDebugModeChanged(DebuggerMode::Enum newMode) { (void)newMode; }
		private:
			uint16_t		m_id;
			char	m_name[256];
		};
	public:
		HostedSledDebugger()
		{
			m_debugger = 0;
			m_debuggerMem = 0;
			m_plugins = 0;
		}

		~HostedSledDebugger()
		{
			if (m_debugger)
			{
				debuggerShutdown(m_debugger);
				m_debugger = 0;
			}

			DeallocatePluginStorage();

			if (m_debuggerMem) {
				delete [] m_debuggerMem;
				m_debuggerMem = 0;
			}
		}

		int32_t Setup(const SledDebuggerConfig& config)
		{
			std::size_t iMemSize;

			const int32_t iError = debuggerRequiredMemory(&config, &iMemSize);
			if (iError != 0)
				return iError;

			m_debuggerMem = new char[iMemSize];
			if (!m_debuggerMem)
				return -1;

			AllocatePluginStorage(config.maxPlugins);

			return debuggerCreate(&config, m_debuggerMem, &m_debugger);
		}

		int32_t CreatePlugin(SledDebuggerPlugin **plugin)
		{
			if (!plugin)
				return -1;

			if ((m_pluginCount + 1) > m_pluginMax)
				return -1;

			*plugin = new HostedSledDebuggerPlugin(m_pluginCount);
			m_plugins[m_pluginCount++] = *plugin;

			return SCE_SLED_ERROR_OK;
		}

		SledDebugger *m_debugger;

	private:
		char *m_debuggerMem;	
	private:
		void AllocatePluginStorage(uint16_t maxPlugins)
		{
			DeallocatePluginStorage();

			m_pluginCount = 0;
			m_pluginMax = maxPlugins;		
			m_plugins = new SledDebuggerPlugin*[m_pluginMax];
		}

		void DeallocatePluginStorage()
		{
			if (!m_plugins)
				return;

			for (uint16_t i = 0; i < m_pluginCount; i++)
				delete m_plugins[i];

			delete [] m_plugins;
			m_plugins = 0;
		}
	private:
		SledDebuggerPlugin **m_plugins;
		uint16_t m_pluginCount;
		uint16_t m_pluginMax;
	};

	struct Fixture
	{
		Fixture()
		{
		}

		HostedSledDebugger host;
		HostedSledDebuggerConfig config;
	};

	TEST_FIXTURE(Fixture, SledDebugger_Create)
	{
		SledDebuggerConfig defaultConfig = config.Default();
		SledDebuggerConfig invalidConfig = config.Invalid();

		CHECK_EQUAL(debuggerRequiredMemory(&defaultConfig, NULL), SCE_SLED_ERROR_NULLPARAMETER);
		std::size_t iSize;
		CHECK_EQUAL(debuggerRequiredMemory(&invalidConfig, &iSize), SCE_SLED_ERROR_INVALIDCONFIGURATION);
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.Default()));

		// Check some default values
		DebuggerMode::Enum debuggerMode;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerGetDebuggerMode(host.m_debugger, &debuggerMode));
		CHECK_EQUAL(DebuggerMode::kNormal, debuggerMode);

		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsConnected(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);
	}

	TEST_FIXTURE(Fixture, SledDebugger_CreateInvalid)
	{
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_INVALIDCONFIGURATION, host.Setup(config.Invalid()));
	}

	TEST(SledDebugger_CreateSingleTcp)
	{
		HostedSledDebugger host1;
		HostedSledDebuggerConfig config;

		CHECK_EQUAL(SCE_SLED_ERROR_OK, host1.Setup(config.DefaultWithCustomNetwork(Protocol::kTcp, 11111, false)));

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host1.m_debugger));

		const int iSpinCount = 50;
		for (int i = 0; i < iSpinCount; i++)
		{
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerUpdate(host1.m_debugger));
		}

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStopNetworking(host1.m_debugger));
	}

	TEST(SledDebugger_CreateMultipleTcp)
	{
		HostedSledDebugger host1;
		HostedSledDebugger host2;
		HostedSledDebugger host3;
		HostedSledDebuggerConfig config;

		CHECK_EQUAL(SCE_SLED_ERROR_OK, host1.Setup(config.DefaultWithCustomNetwork(Protocol::kTcp, 11111, false)));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host2.Setup(config.DefaultWithCustomNetwork(Protocol::kTcp, 11112, false)));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host3.Setup(config.DefaultWithCustomNetwork(Protocol::kTcp, 11113, false)));

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host1.m_debugger));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host2.m_debugger));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host3.m_debugger));

		const int iSpinCount = 50;
		for (int i = 0; i < iSpinCount; i++)
		{
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerUpdate(host1.m_debugger));
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerUpdate(host2.m_debugger));
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerUpdate(host3.m_debugger));
		}

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStopNetworking(host1.m_debugger));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStopNetworking(host2.m_debugger));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStopNetworking(host3.m_debugger));	
	}

	TEST(SledDebugger_CreateMultipleTcpWithPlugins)
	{
		HostedSledDebugger host1;
		HostedSledDebugger host2;
		HostedSledDebugger host3;
		HostedSledDebuggerConfig config;

		CHECK_EQUAL(SCE_SLED_ERROR_OK, host1.Setup(config.DefaultWithCustomNetwork(Protocol::kTcp, 11111, false)));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host2.Setup(config.DefaultWithCustomNetwork(Protocol::kTcp, 11112, false)));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host3.Setup(config.DefaultWithCustomNetwork(Protocol::kTcp, 11113, false)));

		SledDebuggerPlugin *plugin1 = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host1.CreatePlugin(&plugin1));
		SledDebuggerPlugin *plugin2 = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host2.CreatePlugin(&plugin2));
		SledDebuggerPlugin *plugin3 = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host3.CreatePlugin(&plugin3));

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerAddPlugin(host1.m_debugger, plugin1));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerAddPlugin(host2.m_debugger, plugin2));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerAddPlugin(host3.m_debugger, plugin3));

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host1.m_debugger));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host2.m_debugger));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host3.m_debugger));

		const int iSpinCount = 50;
		for (int i = 0; i < iSpinCount; i++)
		{
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerUpdate(host1.m_debugger));
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerUpdate(host2.m_debugger));
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerUpdate(host3.m_debugger));
		}

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStopNetworking(host1.m_debugger));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStopNetworking(host2.m_debugger));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStopNetworking(host3.m_debugger));	
	}

	TEST_FIXTURE(Fixture, SledDebugger_CreateBareMinimum)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.BareMinimum()));
	}

	TEST_FIXTURE(Fixture, SledDebugger_CheckVersion)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.Default()));

		Version version;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerGetVersion(host.m_debugger, &version));

		CHECK_EQUAL(SCE_LIBSLEDDEBUGGER_VER_MAJOR, version.majorNum);
		CHECK_EQUAL(SCE_LIBSLEDDEBUGGER_VER_MINOR, version.minorNum);
		CHECK_EQUAL(SCE_LIBSLEDDEBUGGER_VER_REVISION, version.revisionNum);
	}

	TEST_FIXTURE(Fixture, SledDebugger_ScriptCache_FillAndTryDuplicatesAndClear)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.DefaultWithCustomScriptCache(5, 64)));

		{
			// Stop logger from reporting errors
			LogStealer logStealer;

			const char *pszString1 = "/app_home/game/assets/scripts/file1.lua";
			const char *pszString2 = "/app_home/game/assets/scripts/file2.lua";
			const char *pszString3 = "/app_home/game/assets/scripts/file3.lua";
			const char *pszString4 = "/app_home/game/assets/scripts/file4.lua";
			const char *pszString5 = "/app_home/game/assets/scripts/file5.lua";
			const char *pszString6 = "/app_home/game/assets/scripts/file6.lua";

			bool bRetval;
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString1, &bRetval));
			CHECK_EQUAL(true, bRetval);
		
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString2, &bRetval));
			CHECK_EQUAL(true, bRetval);
		
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString3, &bRetval));
			CHECK_EQUAL(true, bRetval);
		
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString4, &bRetval));
			CHECK_EQUAL(true, bRetval);
		
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString5, &bRetval));
			CHECK_EQUAL(true, bRetval);
		
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString6, &bRetval));	// Full; can't add
			CHECK_EQUAL(false, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheClear(host.m_debugger));

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString1, &bRetval));
			CHECK_EQUAL(true, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString1, &bRetval));	// Duplicate; can't add
			CHECK_EQUAL(false, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString2, &bRetval));
			CHECK_EQUAL(true, bRetval);
		
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString2, &bRetval));	// Duplicate; can't add
			CHECK_EQUAL(false, bRetval);
		
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString3, &bRetval));
			CHECK_EQUAL(true, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString3, &bRetval));	// Duplicate; can't add
			CHECK_EQUAL(false, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString4, &bRetval));
			CHECK_EQUAL(true, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString4, &bRetval));	// Duplicate; can't add
			CHECK_EQUAL(false, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString5, &bRetval));
			CHECK_EQUAL(true, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString5, &bRetval));	// Duplicate; can't add
			CHECK_EQUAL(false, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString6, &bRetval));	// Full; can't add
			CHECK_EQUAL(false, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheRemove(host.m_debugger, pszString1, &bRetval));
			CHECK_EQUAL(true, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString1, &bRetval));
			CHECK_EQUAL(true, bRetval);
		}
	}

	TEST_FIXTURE(Fixture, SledDebugger_ScriptCache_ZeroEntriesAllocButTryAdd)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.DefaultWithCustomScriptCache(0, 0)));

		{
			// Stop logger from reporting errors
			LogStealer logStealer;

			const char *pszString1 = "/app_home/game/assets/scripts/file1.lua";

			bool bRetval;
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString1, &bRetval));
			CHECK_EQUAL(false, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString1, &bRetval));
			CHECK_EQUAL(false, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheClear(host.m_debugger));

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString1, &bRetval));
			CHECK_EQUAL(false, bRetval);

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, pszString1, &bRetval));
			CHECK_EQUAL(false, bRetval);
		}
	}

	TEST_FIXTURE(Fixture, SledDebugger_ScriptCache_AddNull)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.Default()));

		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, 0, &bRetval));
		CHECK_EQUAL(false, bRetval);
	}

	TEST_FIXTURE(Fixture, SledDebugger_ScriptCache_AddEmptyString)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.Default()));
	
		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, "", &bRetval));
		CHECK_EQUAL(false, bRetval);
	}

	TEST_FIXTURE(Fixture, SledDebugger_ScriptCache_ZeroEntriesAllocButTryAddNull)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.DefaultWithCustomScriptCache(0, 0)));
	
		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, 0, &bRetval));
		CHECK_EQUAL(false, bRetval);
	}

	TEST_FIXTURE(Fixture, SledDebugger_ScriptCache_ZeroEntriesAllocButTryAddEmptyString)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.DefaultWithCustomScriptCache(0, 0)));
	
		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerScriptCacheAdd(host.m_debugger, "", &bRetval));
		CHECK_EQUAL(false, bRetval);
	}

	TEST_FIXTURE(Fixture, SledDebugger_Plugin_AddNullPlugin)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.Default()));
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_INVALIDPLUGIN, debuggerAddPlugin(host.m_debugger, 0));
	}

	TEST_FIXTURE(Fixture, SledDebugger_Plugin_AddManyPlugins)
	{
		const uint16_t iMaxPlugins = 100;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.DefaultWithCustomPluginCount(iMaxPlugins)));

		for (uint16_t i = 0; i < iMaxPlugins; i++)
		{
			SledDebuggerPlugin *plugin = 0;
			CHECK_EQUAL(SCE_SLED_ERROR_OK, host.CreatePlugin(&plugin));

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerAddPlugin(host.m_debugger, plugin));
		}
	}

	TEST_FIXTURE(Fixture, SledDebugger_Plugin_AddTooManyPlugins)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.Default()));

		SledDebuggerPlugin *plugin = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.CreatePlugin(&plugin));
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerAddPlugin(host.m_debugger, plugin));
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_MAXPLUGINSREACHED, debuggerAddPlugin(host.m_debugger, plugin));
	}

	TEST_FIXTURE(Fixture, SledDebugger_Plugin_AddDuplicatePlugins)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.DefaultWithCustomPluginCount(2)));

		SledDebuggerPlugin *plugin = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.CreatePlugin(&plugin));

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerAddPlugin(host.m_debugger, plugin));
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_PLUGINALREADYADDED, debuggerAddPlugin(host.m_debugger, plugin));
	}

	TEST_FIXTURE(Fixture, SledDebugger_Networking_StartAndUpdateABitAndStop)
	{
		const uint16_t iSpinCount = 100;

		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.Default()));

		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host.m_debugger));

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(true, bRetval);

		for (uint16_t i = 0; i < iSpinCount; i++)
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerUpdate(host.m_debugger));

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStopNetworking(host.m_debugger));

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);
	}

	TEST_FIXTURE(Fixture, SledDebugger_Networking_StartAndUpdateABitAndStopWithPluginLoaded)
	{
		const uint16_t iSpinCount = 100;

		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.Default()));

		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);

		SledDebuggerPlugin *plugin = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.CreatePlugin(&plugin));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerAddPlugin(host.m_debugger, plugin));

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host.m_debugger));
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(true, bRetval);

		for (uint16_t i = 0; i < iSpinCount; i++)
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerUpdate(host.m_debugger));

		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStopNetworking(host.m_debugger));
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);
	}

	TEST_FIXTURE(Fixture, SledDebugger_Networking_StartAndUpdateABitAndStopOverAndOver)
	{
		const uint16_t iCount = 5;
		const uint16_t iSpinCount = 100;

		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.Default()));

		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);

		for (uint16_t j = 0; j < iCount; j++)
		{
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host.m_debugger));
		
			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
			CHECK_EQUAL(true, bRetval);

			for (uint16_t i = 0; i < iSpinCount; i++)
				CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerUpdate(host.m_debugger));

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStopNetworking(host.m_debugger));

			CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
			CHECK_EQUAL(false, bRetval);
		}
	}

	TEST_FIXTURE(Fixture, SledDebugger_Networking_StartNetworkingMultipleTimes)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.Default()));
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host.m_debugger));
	
		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(true, bRetval);

		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_ALREADYNETWORKING, debuggerStartNetworking(host.m_debugger));
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(true, bRetval);

		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_ALREADYNETWORKING, debuggerStartNetworking(host.m_debugger));
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(true, bRetval);

		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_ALREADYNETWORKING, debuggerStartNetworking(host.m_debugger));
	}

	TEST_FIXTURE(Fixture, SledDebugger_Networking_StopNetworkingMultipleTimes)
	{
		CHECK_EQUAL(SCE_SLED_ERROR_OK, host.Setup(config.Default()));

		bool bRetval;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);

		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_NOTNETWORKING, debuggerStopNetworking(host.m_debugger));
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);
	
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_NOTNETWORKING, debuggerStopNetworking(host.m_debugger));
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStartNetworking(host.m_debugger));
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(true, bRetval);
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerStopNetworking(host.m_debugger));
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);
	
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_NOTNETWORKING, debuggerStopNetworking(host.m_debugger));
	
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerIsNetworking(host.m_debugger, &bRetval));
		CHECK_EQUAL(false, bRetval);
	}

	TEST_FIXTURE(Fixture, SledDebugger_Hash_GenerateValid)
	{
		const char *pszString1 = "/app_home/game/assets/scripts/file1.lua";
		const char *pszString2 = "\\app_home\\game\\assets\\scripts\\file1.lua";
		const char *pszString3 = "/app_home/game/assets/scripts/weapons/guns/file1.lua";

		int32_t hash1 = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerGenerateHash(pszString1, 15, &hash1));
		int32_t hash2 = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerGenerateHash(pszString2, 15, &hash2));
		int32_t hash3 = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerGenerateHash(pszString3, 15, &hash3));
		int32_t hash4 = 0;
		CHECK_EQUAL(SCE_SLED_ERROR_OK, debuggerGenerateHash(pszString2, 55, &hash4));

		CHECK_EQUAL(true, hash1 == hash2);
		CHECK_EQUAL(true, hash3 != hash2);
		CHECK_EQUAL(true, hash3 != hash4);
		CHECK_EQUAL(true, hash1 != hash4);
	}

	TEST_FIXTURE(Fixture, SledDebugger_Hash_TryNullHashPtr)
	{
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_NULLPARAMETER, debuggerGenerateHash(0, 21, 0));
	}

	TEST_FIXTURE(Fixture, SledDebugger_Hash_TryNullString)
	{
		int32_t hash1 = 0;
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_NULLPARAMETER, debuggerGenerateHash(0, 21, &hash1));
	}

	TEST_FIXTURE(Fixture, SledDebugger_Hash_TryEmptyString)
	{
		int32_t hash1 = 0;
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_INVALIDPARAMETER, debuggerGenerateHash("", 21, &hash1));
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_INVALIDPARAMETER, debuggerGenerateHash("\0", 21, &hash1));
	}
}}}
