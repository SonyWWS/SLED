/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDLUAPLUGIN_EXTRAS_H__
#define __SCE_LIBSLEDLUAPLUGIN_EXTRAS_H__

// Forward declaration.
struct lua_State;

/// Namespace for sce classes and functions.
namespace sce
{
/// Namespace for Sled classes and functions.
namespace Sled
{
	// Forward declarations.
	class LuaPlugin;

	/// Push the libsledluaplugin Lua error handler function onto the stack. Supply the error handler stack index to lua_pcall and
	/// then if there is a runtime error and SLED is connected you can instantly see the problem spot in SLED.
	/// @brief
	/// Push the libsledluaplugin Lua error handler function onto the stack.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param plugin LuaPlugin to use.
	/// @param luaState Pointer to a lua_State.
	/// @param outAbsStackIndex Absolute stack index of error handler function if pushed on the stack.
	///
	/// @retval SCE_SLED_ERROR_OK						Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER			Null plugin, luaState, or outAbsStackIndex.
	/// @retval SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE	No debugger instance (not added to a SledDebugger instance)
	/// @retval SCE_SLED_LUA_ERROR_INVALIDLUASTATE		Lua state not registered (not registered to a LuaPlugin).
	int32_t luaPluginGetErrorHandlerAbsStackIdx(LuaPlugin *plugin, lua_State *luaState, int *outAbsStackIndex);
}}

#endif // __SCE_LIBSLEDLUAPLUGIN_EXTRAS_H__
