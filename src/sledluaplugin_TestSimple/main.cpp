/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include <sce_sled/sled.h>

#include "../sleddebugger/assert.h"
#include "../sleddebugger/utilities.h"

#include "../sledluaplugin_SharedSampleCode/fileutilities.h"

#if SCE_SLEDTARGET_OS_WINDOWS
	#ifndef _WINDOWS_
		#include <windows.h>
	#endif // _WINDOWS_
	#include "../sleddebugger_unittests/scoped_network.h"
#else
	#error Not Supported
#endif

#include <cstdio>
#include <cstdlib>

extern "C"
{
	#include <wws_lua/lua-5.1.4/src/lua.h>
	#include <wws_lua/lua-5.1.4/src/lualib.h>
	#include <wws_lua/lua-5.1.4/src/lauxlib.h>
}

using namespace sce::Sled;

int main()
{
	std::printf("Main: TestSimple example started!\n");
	std::printf("Main: Bringing up network subsystem\n");
	ScopedNetwork sn;

	Examples::FileUtil::ValidateWorkingDirectory();

	int32_t iRetval = 0;

	//
	// Using libsleddebugger & libsledluaplugin. This is
	// just an example of how to use libsleddebugger &
	// libsledluaplugin. It's not a functional example
	// in that SLED can connect and debug it (see TestTarget
	// for that purpose) but it does compile and explain
	// what some of the configuration struture items mean
	// and how they're used.
	//

	//
	// libsleddebugger -	create SledDebugger instance
	//

	SledDebugger *pDebugger = 0;
	char *pDebuggerMemory = 0;

	// Create SledDebuggerConfig struct. This describes
	// what settings will be used and how much memory
	// will be used.
	{
		SledDebuggerConfig config;

		// Number of language plugins the SledDebugger
		// will host. Usually just one - LuaPlugin.
		config.maxPlugins = 1;

		// Default size for network receive buffer
		config.maxRecvBufferSize = 2048;

		// Default size for network send buffer
		config.maxSendBufferSize = 2048;

		// ScriptCache - this is optional. If using it
		// is usually the number of scripts that will be
		// used. When connecting SLED will try to add
		// any scripts from the script cache to the 
		// currently open project. This is just a mock
		// example and there are no scripts.
		config.maxScriptCacheEntries = 0;

		// If using the ScriptCache then this is the
		// maximum length that a script cache entry can
		// be. It should be the string length + 1 of the
		// longest script path you have.
		config.maxScriptCacheEntryLen = 0;

		// ScriptCache note: SLED is expecting the paths
		// be relative paths based off of the SLED asset
		// directory but absolute paths will work, too, 
		// they'll just be longer and require more memory
		// be used.

		// Set up networking

		// The first parameter is the protocol - TCP
		// The second parameter is the port to listen on &
		// the default is 11111
		// The third parameter is whether the SledDebugger
		// StartNetworking will block until SLED connects
		// or not
		config.net.setup(Protocol::kTcp, 11111, false);

		// Configuration struct is all done now lets figure
		// out how much memory is needed.

		std::size_t iMemSize;
		iRetval = debuggerRequiredMemory(&config, &iMemSize);
		if (iRetval != 0) {
			std::printf("Error calculating SledDebugger required memory: %i\n", iRetval);
			return iRetval;
		}

		// Allocate memory; SledDebugger gets placed inside
		// this buffer.
		pDebuggerMemory = new char[iMemSize];

		// Create SledDebugger instance
		iRetval = debuggerCreate(&config, pDebuggerMemory, &pDebugger);
		if (iRetval != 0) {
			std::printf("Error creating SledDebugger instance: %i\n", iRetval);
			return iRetval;
		}
	}

	// SledDebugger instance is created so now it can be used.
	// Functions that can now be used:
	// pDebugger->
	//			GetVersion()
	//			ScriptCacheAdd()
	//			ScriptCacheRemove()
	//			ScriptCacheClear()
	//			Update()
	//			AddPlugin()
	//			RemovePlugin()
	// NOTE: AddPlugin & RemovePlugin should be used only after
	// a language plugin - like libsledluaplugin - has been set
	// up and instantiated (which is done next in this example)
	// NOTE: Update() should be called from the main game loop

	//
	// libsledluaplugin -	create LuaPlugin instance and 
	//						add to SledDebugger as plugin
	// 

	LuaPlugin *pLuaPlugin = 0;
	char *pLuaPluginMemory = 0;

	// Create LuaPluginConfig struct. This describes
	// what settings will be used and how much memory
	// will be used.
	{
		LuaPluginConfig config;

		// Default size for LuaPlugin's send buffer.
		config.maxSendBufferSize = 2048;

		// Number of Lua states that will be debugged.
		config.maxLuaStates = 1;

		// Optional - only need if using memory trace
		// functionality. If not using memory trace
		// functionality then set to zero.
		config.maxMemTraces = 5000;

		// Maximum number of breakpoints that can be set.
		config.maxBreakpoints = 64;

		// Maximum number of scripts that cna be edited while
		// stopped on a breakpoint. The scripts aren't
		// actually reloaded until continuing from a breakpoint.
		// If set to 0 then edit & continue can't be used.
		config.maxEditAndContinues = 0;
		// Maximum length of an edit and continue entry - 
		// essentially the same meaning as script cache entry length.
		config.maxEditAndContinueEntryLen = 0;

		// Maximum number of variable filters that can be set. If 0
		// then variable filtering is disabled.
		config.maxNumVarFilters = 0;
		// Maximum string length for a variable filter pattern.
		config.maxVarFilterPatternLen = 0;
		// Maximum patterns per each variable filter.
		config.maxPatternsPerVarFilter = 0;

		// Maximum number of Lua functions that can be profiled. If not
		// using the profiler functionality then set to zero.  If using
		// then it's advisable to set it to the same number of Lua 
		// functions that exist throughout all Lua scripts.
		config.maxProfileFunctions = 0;
		// Maximum number of functions to keep track of that called a
		// particular function.
		config.maxProfileCallStackDepth = 0;

		// To hit a breakpoint the file name pulled out of the lua_Debug
		// struct during the line callback must match a file name in
		// the internal breakpoint list. The chop chars settings are to
		// give the game some control over parsing the file name that
		// gets pulled out of the lua_Debug structure.
		// numPathChopChars is used to chop off characters starting at
		// the beginning of the file name pulled out of the lua_Debug
		// structure. For instance, if the file name has some path
		// information at the beginning that won't exist on the SLED
		// side - like "/app_home/" - then setting numPathChoprs to 10
		// (the legnth of "/app_home/") - would return a string with
		// "/app_home/" removed.
		// pfnChopCharsCallback can be used if more parsing functionality
		// is needed (and is a C/C++ function).
		config.numPathChopChars = 0;
		config.pfnChopCharsCallback = 0;

		// A C/C++ function that gets called when a script is being
		// reloaded due to edit & continue. It is the game's job to
		// do the actual file loading and in this callback is where
		// that should happen. The callback returns the contents of
		// the file back to the library.
		config.pfnEditAndContinueCallback = 0;
		// A C/C++ function that gets called after edit & continue
		// has been performed on a script. This is useful if a
		// buffer had to be allocated for the file load and the game
		// now needs to deallocate that buffer.
		config.pfnEditAndContinueFinishCallback = 0;
		// Userdata to pass along to the edit & continue callbacks.
		config.pEditAndContinueUserData = 0;

		std::size_t iMemSize;
		iRetval = luaPluginRequiredMemory(&config, &iMemSize);
		if (iRetval != 0) {
			std::printf("Error calculating LuaPlugin required memory: %i\n", iRetval);
			return iRetval;
		}

		// Allocate memory; LuaPlugin gets placed inside
		// this buffer.
		pLuaPluginMemory = new char[iMemSize];

		// Create Lua plugin instance
		iRetval = luaPluginCreate(&config, pLuaPluginMemory, &pLuaPlugin);
		if (iRetval != 0) {
			std::printf("Error creating LuaPlugin instance: %i\n", iRetval);
			return iRetval;
		}
	}

	// Add LuaPlugin to SledDebugger
	iRetval = debuggerAddLuaPlugin(pDebugger, pLuaPlugin);
	if (iRetval != 0) {
		std::printf("SledDebugger failed to add plugin: %i\n", iRetval);
		return iRetval;
	}

	// ...sometime later start networking so
	// SLED can connect...
	iRetval = debuggerStartNetworking(pDebugger);
	if (iRetval != 0) {
		std::printf("SledDebugger failed to start networking: %i\n", iRetval);
		return iRetval;
	}

	// ...sometime later create Lua states...
	lua_State *pLuaState = ::lua_open();

	// ...load scripts into Lua states then use
	// ScriptCacheAdd() so that SLED is aware of
	// the scripts...

	// ...then register each Lua state with the LuaPlugin...
	iRetval = luaPluginRegisterLuaState(pLuaPlugin, pLuaState, "Main Lua State");
	if (iRetval != 0) {
		std::printf("LuaPlugin failed to register Lua state: %i\n", iRetval);
		return iRetval;
	}

	//
	// ...sometime later in the main game loop...
	//
	// while (true)
	{
		// ...
		debuggerUpdate(pDebugger);
		// ...
	}

	//
	// ...game does whatever...
	//

	// Shut down everything & free memory
	debuggerStopNetworking(pDebugger);

	// Unregister any Lua states before closing
	// them
	luaPluginUnregisterLuaState(pLuaPlugin, pLuaState);
	::lua_close(pLuaState);
	pLuaState = 0;

	// Shutdown LuaPlugin
	luaPluginShutdown(pLuaPlugin);
	pLuaPlugin = 0;

	// Shutdown SledDebugger
	debuggerShutdown(pDebugger);
	pDebugger = 0;

	// Free memory
	delete [] pLuaPluginMemory;
	pLuaPluginMemory = 0;

	delete [] pDebuggerMemory;
	pDebuggerMemory = 0;

	std::printf("Main: Program finished!\n");

	return 0;
}
