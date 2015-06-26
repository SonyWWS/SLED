/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDLUAPLUGIN_H__
#define __SCE_LIBSLEDLUAPLUGIN_H__

/* libsledluaplugin version information */
#define SCE_LIBSLEDLUAPLUGIN_VER_MAJOR		5 ///< Major version
#define SCE_LIBSLEDLUAPLUGIN_VER_MINOR		1 ///< Minor version
#define SCE_LIBSLEDLUAPLUGIN_VER_REVISION	2 ///< Revision version
#define SCE_LIBSLEDLUAPLUGIN_VER_OTHER		0 ///< Extra version number

// The ID must be the same between this library and the Sled.Lua.dll.
#define SCE_LIBSLEDLUAPLUGIN_ID		1					///< Id of LibSledLuaPlugin. The ID must be the same between this library and the <c>Sled.Lua.dll</c>.
#define SCE_LIBSLEDLUAPLUGIN_NAME	"SLED Lua Plugin"	///< Plugin name

/* libsleddebugger headers */
#include "../sleddebugger/params.h"
#include "../sleddebugger/common.h"

/* libsledluaplugin headers */
#include "errorcodes.h"
#include "params.h"

#include <cstddef>

// Forward declaration.
struct lua_State;

/// Namespace for sce classes and functions.
namespace sce
{
/// Namespace for Sled classes and functions.
namespace Sled
{
	// Forward declarations.
	class SledDebugger;
	class LuaPlugin;

	/// Create a <c>LuaPlugin</c> instance.
	/// @brief
	/// Create <c>LuaPlugin</c> instance.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param config A configuration structure that details the settings to use
	/// @param location The location in memory in which to place the <c>LuaPlugin</c> instance.
	/// It must be as big as the value returned by <c>luaPluginRequiredMemory()</c>.
	/// @param outLuaPlugin The <c>LuaPlugin</c> instance that is created
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null configuration structure
	/// @retval SCE_SLED_ERROR_INVALIDCONFIGURATION		Invalid value in the configuration structure
	///
	/// @see
	/// <c>luaPluginRequiredMemory</c>, <c>luaPluginShutdown</c>, <c>debuggerAddLuaPlugin</c>
	SCE_SLED_LINKAGE int32_t luaPluginCreate(const LuaPluginConfig *config, void *location, LuaPlugin **outLuaPlugin);

	/// Calculate the size in bytes required for a <c>LuaPlugin</c> instance based on a configuration structure.
	/// @brief
	/// Calculate size in bytes required for <c>LuaPlugin</c> instance based on configuration structure.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param config The configuration structure that details the settings to use
	/// @param outRequiredMemory The amount of memory that is needed for the <c>LuaPlugin</c> instance
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null configuration structure
	/// @retval SCE_SLED_ERROR_INVALIDCONFIGURATION		Invalid value in the configuration structure
	///
	/// @see
	/// <c>luaPluginCreate</c>
	SCE_SLED_LINKAGE int32_t luaPluginRequiredMemory(const LuaPluginConfig *config, std::size_t *outRequiredMemory);

	/// Shut down a <c>LuaPlugin</c> instance.
	/// @brief
	/// Shut down <c>LuaPlugin</c> instance.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin The <c>LuaPlugin</c> instance to shut down
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin
	///
	/// @see
	/// <c>luaPluginCreate</c>
	SCE_SLED_LINKAGE int32_t luaPluginShutdown(LuaPlugin *plugin);

	/// Get ID of the plugin. The ID must be unique across all other language plugins. The ID 0 (zero) is reserved for the <c>SledDebugger</c> class.
	/// @brief
	/// Get ID of plugin.
	///
	/// @par Calling Conditions
	/// Multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param outId A number greater than zero
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin or outId
	///
	/// @see
	/// <c>luaPluginGetName</c>, <c>luaPluginGetVersion</c>
	SCE_SLED_LINKAGE int32_t luaPluginGetId(const LuaPlugin *plugin, uint16_t *outId);

	/// Get the name of the plugin.
	/// @brief
	/// Get name of plugin.
	///
	/// @par Calling Conditions
	/// Multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param outName The name of the plugin
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin or outName
	///
	/// @see
	/// <c>luaPluginGetId</c>, <c>luaPluginGetVersion</c>
	SCE_SLED_LINKAGE int32_t luaPluginGetName(const LuaPlugin *plugin, const char **outName);

	/// Get the plugin version information.
	/// @brief
	/// Get plugin version information.
	///
	/// @par Calling Conditions
	/// Multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param outVersion The plugin's version information
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin or outVersion
	///
	/// @see
	/// <c>luaPluginGetId</c>, <c>luaPluginGetName</c>
	SCE_SLED_LINKAGE int32_t luaPluginGetVersion(const LuaPlugin *plugin, Version *outVersion);

	/// Register a <c>lua_State</c>* with the library. Lua states must be registered with the library if they are to be debugged.
	/// @brief
	/// Register <c>lua_State</c>* with library.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param luaState Pointer to a <c>lua_State</c>
	/// @param luaStateName A name for the <c>lua_State</c>. This name shows up on the <c>lua_State</c> GUI in SLED to help easily identify the <c>lua_State</c>.
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin
	/// @retval SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE	No debugger instance (not added to a <c>SledDebugger</c> instance)
	/// @retval SCE_SLED_LUA_ERROR_INVALIDLUASTATE		<c>lua_State</c> is null
	/// @retval SCE_SLED_LUA_ERROR_DUPLICATELUASTATE	<c>lua_State</c> is already registered
	/// @retval SCE_SLED_LUA_ERROR_OVERLUASTATELIMIT	No space for <c>lua_State</c>
	///
	/// @see
	/// <c>luaPluginUnregisterLuaState</c>
	SCE_SLED_LINKAGE int32_t luaPluginRegisterLuaState(LuaPlugin *plugin, lua_State *luaState, const char *luaStateName = 0);

	/// Unregister a <c>lua_State</c>* from the library. Lua states must be registered with the library if they are to be debugged.
	/// @brief
	/// Unregister <c>lua_State</c>* from library.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param luaState Pointer to a <c>lua_State</c>
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin
	/// @retval SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE	No debugger instance (not added to a <c>SledDebugger</c> instance)
	/// @retval SCE_SLED_LUA_ERROR_INVALIDLUASTATE		<c>lua_State</c> is null
	/// @retval SCE_SLED_LUA_ERROR_LUASTATENOTFOUND		<c>lua_State</c> is not registered
	///
	/// @see
	/// <c>luaPluginRegisterLuaState</c>
	SCE_SLED_LINKAGE int32_t luaPluginUnregisterLuaState(LuaPlugin *plugin, lua_State *luaState);

	/// Reset the internal profile information list.
	/// @brief
	/// Reset internal profile information list.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin
	///
	/// @see
	/// <c>luaPluginIsProfilerRunning</c>, <c>luaPluginResetMemoryTrace</c>
	SCE_SLED_LINKAGE int32_t luaPluginResetProfileInfo(LuaPlugin *plugin);

	/// Determine whether or not the profiler is running.
	/// @brief
	/// Check whether profiler is running.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param outResult True if the profiler is running; false if it is not running
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin or <c>outResult</c>
	///
	/// @see
	/// <c>luaPluginIsMemoryTracerRunning</c>, <c>luaPluginResetProfileInfo</c>
	SCE_SLED_LINKAGE int32_t luaPluginIsProfilerRunning(const LuaPlugin *plugin, bool *outResult);

	/// Reset the internal memory trace list.
	/// @brief
	/// Reset internal memory trace list.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin
	///
	/// @see
	/// <c>luaPluginIsMemoryTracerRunning</c>, <c>luaPluginResetProfileInfo</c>, <c>luaPluginMemoryTraceNotify</c>
	SCE_SLED_LINKAGE int32_t luaPluginResetMemoryTrace(LuaPlugin *plugin);

	/// Check whether or not the memory tracer is running.
	/// @brief
	/// Check whether memory tracer is running.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param outResult True if the memory tracer is running; false if it is not running
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin or <c>outResult</c>
	///
	/// @see
	/// <c>luaPluginIsProfilerRunning</c>, <c>luaPluginResetMemoryTrace</c>, <c>luaPluginMemoryTraceNotify</c>
	SCE_SLED_LINKAGE int32_t luaPluginIsMemoryTracerRunning(const LuaPlugin *plugin, bool *outResult);

	/// Force a breakpoint on a specific <c>lua_State</c> and send data via TTY.
	/// @brief
	/// Force breakpoint on specific <c>lua_State</c> and send data via TTY.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param luaState Pointer to a <c>lua_State</c>
	/// @param pszText Data to send via TTY
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin
	SCE_SLED_LINKAGE int32_t luaPluginDebuggerBreak(LuaPlugin *plugin, lua_State *luaState, const char *pszText);

	/// Force a breakpoint on the next <c>lua_State</c> that runs an instruction and send data via TTY.
	/// @brief
	/// Force breakpoint on next <c>lua_State</c> that runs an instruction and send data via TTY.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param pszText Data to send via TTY
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin
	///
	/// @see
	/// <c>luaPluginGetVarExcludeFlags</c>, <c>luaPluginGetVarExcludeFlags</c>
	SCE_SLED_LINKAGE int32_t luaPluginDebuggerBreak(LuaPlugin *plugin, const char *pszText);

	/// Set the variable groups to exclude from processing and sending when hitting a breakpoint.
	/// @brief
	/// Set variable exclude flags.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param flags An OR'd set of variable groups to exclude from processing and sending when hitting a breakpoint
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin
	///
	/// @see
	/// <c>luaPluginGetVarExcludeFlags</c>, <c>luaPluginDebuggerBreak</c>
	SCE_SLED_LINKAGE int32_t luaPluginSetVarExcludeFlags(LuaPlugin *plugin, int32_t flags);

	/// Get the current variable exclude flags.
	/// @brief
	/// Get current variable exclude flags.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param outFlags Current variable exclude flags
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin or outFlags
	///
	/// @see
	/// <c>luaPluginSetVarExcludeFlags</c>, <c>luaPluginDebuggerBreak</c>
	SCE_SLED_LINKAGE int32_t luaPluginGetVarExcludeFlags(const LuaPlugin *plugin, int32_t *outFlags);

	/// Provide a way to report Lua allocations to the library for tracking using the memory tracer. 
	/// Call this function from the allocator that is making all the Lua allocations, deallocations, and reallocations.
	/// @brief
	/// Provide way to report Lua allocations to library for tracking with memory tracer.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin <c>LuaPlugin</c> to use
	/// @param userData A pointer to user data (not currently used for anything)
	/// @param oldPtr A pointer to old memory being deallocated
	/// @param newPtr A pointer to new memory being allocated
	/// @param oldSize Old memory size
	/// @param newSize New memory size
	/// @param outResult True if information is logged; false if memory tracer is not running
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin or <c>outResult</c>
	///
	/// @see
	/// <c>luaPluginIsMemoryTracerRunning</c>, <c>luaPluginResetMemoryTrace</c>
	SCE_SLED_LINKAGE int32_t luaPluginMemoryTraceNotify(LuaPlugin *plugin, void *userData, void *oldPtr, void *newPtr, std::size_t oldSize, std::size_t newSize, bool *outResult);

	/// Add a <c>LuaPlugin</c> to the <c>SledDebugger</c>. It's a helper method, because <c>LuaPlugin</c> is an incomplete type and
	/// the <c>SledDebugger</c> <c>debuggerAddPlugin()</c> method is expecting a <c>SledDebuggerPlugin</c> instance.
	/// @brief
	/// Add <c>LuaPlugin</c> to <c>SledDebugger</c>.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	/// @param plugin <c>LuaPlugin</c> to use
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null debugger or plugin
	///
	/// @see
	/// <c>luaPluginCreate</c>
	SCE_SLED_LINKAGE int32_t debuggerAddLuaPlugin(SledDebugger *debugger, LuaPlugin *plugin);
}}

#endif // __SCE_LIBSLEDLUAPLUGIN_H__
