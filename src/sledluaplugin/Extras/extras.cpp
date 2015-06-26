/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sledluaplugin.h"
#include "../../sleddebugger/sleddebugger.h"

#include "../sledluaplugin_class.h"

#include "extras.h"

namespace sce{ 	namespace Sled
{
	int32_t luaPluginGetErrorHandlerAbsStackIdx(LuaPlugin *plugin, lua_State *luaState, int *outAbsStackIndex)
	{
		if (plugin == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return plugin->getErrorHandlerAbsStackIndex(luaState, outAbsStackIndex);
	}
}}
