/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "sledluaplugin.h"
#include "../sleddebugger/sleddebugger.h"

#include "sledluaplugin_class.h"

#include <new>

namespace sce { namespace Sled
{
	int32_t luaPluginCreate(const LuaPluginConfig *config, void *location, LuaPlugin **outLuaPlugin)
	{
		if (config == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return LuaPlugin::create(*config, location, outLuaPlugin);
	}

	int32_t luaPluginRequiredMemory(const LuaPluginConfig *config, std::size_t *outRequiredMemory)
	{
		if (config == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return LuaPlugin::requiredMemory(*config, outRequiredMemory);
	}

	int32_t luaPluginShutdown(LuaPlugin *plugin)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		LuaPlugin::close(plugin);
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginGetId(const LuaPlugin *plugin, uint16_t *outId)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outId) = plugin->getId();
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginGetName(const LuaPlugin *plugin, const char **outName)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		if (outName == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outName) = plugin->getName();
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginGetVersion(const LuaPlugin *plugin, Version *outVersion)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outVersion) = plugin->getVersion();
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginRegisterLuaState(LuaPlugin *plugin, lua_State *luaState, const char *luaStateName)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return plugin->registerLuaState(luaState, luaStateName);
	}

	int32_t luaPluginUnregisterLuaState(LuaPlugin *plugin, lua_State *luaState)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return plugin->unregisterLuaState(luaState);
	}

	int32_t luaPluginResetProfileInfo(LuaPlugin *plugin)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		plugin->resetProfileInfo();
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginIsProfilerRunning(const LuaPlugin *plugin, bool *outResult)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outResult) = plugin->isProfilerRunning();
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginResetMemoryTrace(LuaPlugin *plugin)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		plugin->resetMemoryTrace();
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginIsMemoryTracerRunning(const LuaPlugin *plugin, bool *outResult)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outResult) = plugin->isMemoryTracerRunning();
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginDebuggerBreak(LuaPlugin *plugin, lua_State *luaState, const char *pszText)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		plugin->debuggerBreak(luaState, pszText);
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginDebuggerBreak(LuaPlugin *plugin, const char *pszText)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		plugin->debuggerBreak(pszText);
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginSetVarExcludeFlags(LuaPlugin *plugin, int32_t flags)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		plugin->setVarExcludeFlags(flags);
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginGetVarExcludeFlags(const LuaPlugin *plugin, int32_t *outFlags)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outFlags) = plugin->getVarExcludeFlags();
		return SCE_SLED_ERROR_OK;
	}

	int32_t luaPluginMemoryTraceNotify(LuaPlugin *plugin, void *userData, void *oldPtr, void *newPtr, std::size_t oldSize, std::size_t newSize, bool *outResult)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outResult) = plugin->memoryTraceNotify(userData, oldPtr, newPtr, oldSize, newSize);
		return SCE_SLED_ERROR_OK;
	}

	int32_t debuggerAddLuaPlugin(SledDebugger *debugger, LuaPlugin *plugin)
	{
		return debuggerAddPlugin(debugger, plugin);
	}
}}
