/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "luautils.h"
#include "../sleddebugger/assert.h"

#include <cstring>

extern "C"
{
	#include <wws_lua/lua-5.2.3/src/lua.h>
	#include <wws_lua/lua-5.2.3/src/lualib.h>
	#include <wws_lua/lua-5.2.3/src/lauxlib.h>
}

namespace sce { namespace Sled
{
	LuaStateParams::LuaStateParams(lua_State *state, bool bDebugging /* = true */)
		: luaState(state)
	{
		SCE_SLED_ASSERT(state != NULL);
		setDebugging(bDebugging);
	}

	StackReconciler::StackReconciler(lua_State *luaState)
		: m_luaState(luaState)
		, m_iCount1(::lua_gettop(luaState))
	{
	}

	StackReconciler::~StackReconciler()
	{
		const int iCount2 = ::lua_gettop(m_luaState);

		// Assert if too many items have been popped
		SCE_SLED_ASSERT_MSG(m_iCount1 <= iCount2, "Possible stack corruption? Too many items have been popped!?");

		// Pop off any excess
		if ((iCount2 != m_iCount1) && (iCount2 > m_iCount1))
			::lua_pop(m_luaState, iCount2 - m_iCount1);
	}

	namespace LuaUtils
	{
		bool PushRunningFunctionOnStack(lua_State *luaState, lua_Debug *ar, int *absFuncIdx)
		{
			if ((luaState == NULL) || (ar == NULL))
				return false;

			if (::lua_getinfo(luaState, "fn", ar) == 1)
			{
				if (absFuncIdx != NULL)
					(*absFuncIdx) = ::lua_gettop(luaState);

				return true;
			}

			return false;
		}

		bool PushGlobalTableOnStack(lua_State *luaState, int *absGlobalTableIdx)
		{
			if (luaState == NULL)
				return false;

			::lua_pushglobaltable(luaState);			
			if (::lua_type(luaState, -1) == LUA_TTABLE)
			{
				if (absGlobalTableIdx != NULL)
					(*absGlobalTableIdx) = ::lua_gettop(luaState);

				return true;
			}

			return false;
		}

		bool PushFunctionEnvironmentTableOnStack(lua_State *luaState, int funcIdx, int *absEnvTableIdx)
		{
			if (luaState == NULL)
				return false;

			if (::lua_type(luaState, funcIdx) != LUA_TFUNCTION)
				return false;

			const char* const envTableName = "_ENV";
			bool running = true; // just here to stop conditional-expression-constant-warning

			int index = 1;
			const char* upvalue = NULL;

			do 
			{
				upvalue = ::lua_getupvalue(luaState, funcIdx, index++);
				if (upvalue == NULL)
				{
					running = false;
					return false;
				}

				if (std::strcmp(envTableName, upvalue) == 0)
				{
					if (::lua_type(luaState, -1) == LUA_TTABLE)
					{
						if (absEnvTableIdx != NULL)
							(*absEnvTableIdx) = ::lua_gettop(luaState);

						return true;
					}

					return false;
				}

				// pop off upvalue we don't want/need
				::lua_pop(luaState, 1);

			} while (running);

			return false;
		}
	}
}}
