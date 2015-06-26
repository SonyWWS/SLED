/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "lparser_callbacks.h"
#include "LuaInterface.h"

#include <cstdarg>
#include <cstdio>

namespace Unmanaged
{
#ifdef _MANAGED
	#pragma managed(push, off)
#endif
	void Log(lua_State* luaState, const char* format, ...)
	{
		if (luaState == NULL)
			return;

		if (format == NULL)
			return;

		char expandedMessage[2048];
		expandedMessage[0] = '\0';
		
		if (format != NULL)
		{
			va_list args;
			va_start(args, format);
			std::vsprintf(expandedMessage, format, args);
			va_end(args);
		}
		
		const int count1 = LuaInterface::GetTop(luaState);		

		LuaInterface::GetGlobal(luaState, LUA_PARSER_DEBUG_TABLE);
		if (LuaInterface::IsTable(luaState, -1))
		{
			LuaInterface::GetField(luaState, -1, LUA_PARSER_CB_LOG);			
			if (LuaInterface::IsLightUserdata(luaState, -1))
			{
				lparser_callbacks::LogCallback callback = (lparser_callbacks::LogCallback)LuaInterface::ToUserdata(luaState, -1);
				if (callback != NULL)
					callback(expandedMessage);
			}		
		}

		const int count2 = LuaInterface::GetTop(luaState);
		if ((count2 != count1) && (count2 > count1))
			LuaInterface::Pop(luaState, count2 - count1);

	}
#ifdef _MANAGED
	#pragma managed(pop)
#endif
}
