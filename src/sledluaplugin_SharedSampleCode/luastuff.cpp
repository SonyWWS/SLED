/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "luastuff.h"

#include <sce_sled/sled.h>

#include "../sleddebugger/assert.h"

#if SCE_SLEDTARGET_OS_WINDOWS
	#ifndef _WINDOWS_
		#define WIN32_LEAN_AND_MEAN
		#include <windows.h>
	#endif
#endif

#include <cstdio>
#include <cstdlib>
#include <cstring>

#if WWS_LUA_VER >= 520
extern "C"
{
	#include <wws_lua/lua-5.2.3/src/lua.h>
	#include <wws_lua/lua-5.2.3/src/lualib.h>
	#include <wws_lua/lua-5.2.3/src/lauxlib.h>
}
#elif WWS_LUA_VER >= 510
extern "C"
{
	#include <wws_lua/lua-5.1.4/src/lua.h>
	#include <wws_lua/lua-5.1.4/src/lualib.h>
	#include <wws_lua/lua-5.1.4/src/lauxlib.h>
}
#else
	#error Unknown Lua version!
#endif

#ifndef LUASTUFF_ENABLE_SPAM
#define LUASTUFF_ENABLE_SPAM 1
#endif

namespace LuaStuff
{
	int CFunc1(lua_State *pLuaState)
	{
		::lua_pushstring(pLuaState, "CFunc1");
		return 1;
	}

	int CFunc2(lua_State *pLuaState)
	{
		::lua_pushstring(pLuaState, "CFunc2");
		return 1;
	}

	void *Allocate(void *ud, void *ptr, std::size_t oldSize, std::size_t newSize)
	{
		sce::Sled::LuaPlugin *pLuaPlugin = 0;
		if (ud != NULL)
			pLuaPlugin = static_cast<sce::Sled::LuaPlugin*>(ud);

		bool bResult;

		if (newSize == 0)
		{
			if (pLuaPlugin)
				luaPluginMemoryTraceNotify(pLuaPlugin, ud, ptr, 0, oldSize, newSize, &bResult);

			if (ptr)
				std::free(ptr);
		}
		else
		{
			void *pMemory = 0;

			if (!ptr)
				pMemory = std::malloc(newSize);
			else
				pMemory = std::realloc(ptr, newSize);

			if (pLuaPlugin)
				luaPluginMemoryTraceNotify(pLuaPlugin, ud, ptr, pMemory, oldSize, newSize, &bResult);

			return pMemory;
		}

		return 0;
	}

	void Actor::SetName(const char *pszName)
	{
		std::sprintf(m_szName, "%s", pszName);
	}

	const char *Actor::GetAnimName() const
	{
		const char *pszName;

		switch (m_uAnimIndex)
		{
		case 0: pszName = "Idle";
			break;
		case 1:
			pszName = "Walk";
			break;
		case 2:
			pszName = "Swim";
			break;
		case 3:
			pszName = "Jump";
			break;
		default:
			pszName = "Unknown";
			break;
		}

		return pszName;
	}

	const char *Actor::GetItem1Name() const
	{
		const char *pszName;

		switch (m_uItem1Index)
		{
		case 0:
			pszName = "Knife";
			break;
		case 1:
			pszName = "Pistol";
			break;
		case 2:
			pszName = "Rifle";
			break;
		case 3:
			pszName = "Rocket Launcher";
			break;
		default:
			pszName = "Unknown";
			break;
		}

		return pszName;
	}

	const char *Actor::GetItem2Name() const
	{
		const char *pszName;

		switch (m_uItem2Index)
		{
		case 0:
			pszName = "Knife";
			break;
		case 1:
			pszName = "Pistol";
			break;
		case 2:
			pszName = "Rifle";
			break;
		case 3:
			pszName = "Rocket Launcher";
			break;
		default:
			pszName = "Unknown";
			break;
		}

		return pszName;
	}

	static int NewActor(lua_State *pLuaState)
	{
		Actor *pActor = (Actor*)::lua_newuserdata(pLuaState, sizeof(Actor));
		pActor->Init();

		pActor->SetName(::lua_tostring(pLuaState, -2));
		::lua_remove(pLuaState, -2);

		luaL_getmetatable(pLuaState, "ActorMeta");
		::lua_setmetatable(pLuaState, -2);
		return 1;
	}

	static int ActorWrapperGetName(lua_State *pLuaState)
	{
		Actor *pActor = (Actor*)::lua_touserdata(pLuaState, 1);
		::lua_pushstring(pLuaState, pActor->GetName());
		return 1;
	}

	static int ActorWrapperGetAnimIndex(lua_State *pLuaState)
	{
		Actor *pActor = (Actor*)::lua_touserdata(pLuaState, 1);
		::lua_pushnumber(pLuaState, (lua_Number)pActor->GetAnimIndex());
		return 1;
	}

	static int ActorWrapperSetAnimIndex(lua_State *pLuaState)
	{
		Actor *pActor = (Actor*)::lua_touserdata(pLuaState, 1);
		pActor->SetAnimIndex((uint16_t)::lua_tonumber(pLuaState, 2));
		return 0;
	}

	static int ActorWrapperGetAnimName(lua_State *pLuaState)
	{
		Actor *pActor = (Actor*)::lua_touserdata(pLuaState, 1);
		::lua_pushstring(pLuaState, pActor->GetAnimName());
		return 1;
	}

	static int ActorWrapperGetItem1Index(lua_State *pLuaState)
	{
		Actor *pActor = (Actor*)::lua_touserdata(pLuaState, 1);
		::lua_pushnumber(pLuaState, (lua_Number)pActor->GetItem1Index());
		return 1;
	}

	static int ActorWrapperSetItem1Index(lua_State *pLuaState)
	{
		Actor *pActor = (Actor*)::lua_touserdata(pLuaState, 1);
		pActor->SetItem1Index((uint16_t)::lua_tonumber(pLuaState, 2));
		return 0;
	}

	static int ActorWrapperGetItem1Name(lua_State *pLuaState)
	{
		Actor *pActor = (Actor*)::lua_touserdata(pLuaState, 1);
		::lua_pushstring(pLuaState, pActor->GetItem1Name());
		return 1;
	}

	static int ActorWrapperGetItem2Index(lua_State *pLuaState)
	{
		Actor *pActor = (Actor*)::lua_touserdata(pLuaState, 1);
		::lua_pushnumber(pLuaState, (lua_Number)pActor->GetItem2Index());
		return 1;
	}

	static int ActorWrapperSetItem2Index(lua_State *pLuaState)
	{
		Actor *pActor = (Actor*)::lua_touserdata(pLuaState, 1);
		pActor->SetItem2Index((uint16_t)::lua_tonumber(pLuaState, 2));
		return 0;
	}

	static int ActorWrapperGetItem2Name(lua_State *pLuaState)
	{
		Actor *pActor = (Actor*)::lua_touserdata(pLuaState, 1);
		::lua_pushstring(pLuaState, pActor->GetItem2Name());
		return 1;
	}

	static const struct 
#if SCE_LUA_VER >= 511
		luaL_Reg
#else
		luaL_reg 
#endif
		ActorLib_f[] = 
	{
		{"NewActor", NewActor},
		{NULL, NULL}
	};

	static const struct 
#if SCE_LUA_VER >= 511
		luaL_Reg
#else
		luaL_reg 
#endif
		ActorLib_m[] = 
	{
		{"GetName", ActorWrapperGetName},
		{"GetAnimIndex", ActorWrapperGetAnimIndex},
		{"SetAnimIndex", ActorWrapperSetAnimIndex},
		{"GetAnimName", ActorWrapperGetAnimName},
		{"GetItem1Index", ActorWrapperGetItem1Index},
		{"SetItem1Index", ActorWrapperSetItem1Index},
		{"GetItem1Name", ActorWrapperGetItem1Name},
		{"GetItem2Index", ActorWrapperGetItem2Index},
		{"SetItem2Index", ActorWrapperSetItem2Index},
		{"GetItem2Name", ActorWrapperGetItem2Name},
		{"__tostring", ActorWrapperGetName},
		{NULL, NULL}
	};

	int luaopen_Actor(lua_State *pLuaState)
	{
		::luaL_newmetatable(pLuaState, "ActorMeta");

#if WWS_LUA_VER >= 520
		SCE_SLEDUNUSED(ActorLib_m);	
		SCE_SLEDUNUSED(ActorLib_f);

		::lua_pushvalue(pLuaState, -1);
		::lua_setfield(pLuaState, -2, "__index");
	
		/*::luaL_register(pLuaState, NULL, ActorLib_m);
		::luaL_register(pLuaState, "Actor", ActorLib_f);*/
#elif WWS_LUA_VER >= 510
		::lua_pushvalue(pLuaState, -1);
		::lua_setfield(pLuaState, -2, "__index");

		::luaL_register(pLuaState, NULL, ActorLib_m);
		::luaL_register(pLuaState, "Actor", ActorLib_f);
#else
		#error Unknown Lua version!
#endif

		return 1;
	}

	int LookupTypeVal(lua_State *pLuaState, const int& iIndex, char *pType, char *pValue, bool bPop /* = true */)
	{
		if (!pLuaState || !pType || !pValue)
			return -1;

		// Check the type of the item on the top of the stack
		const int iType = ::lua_type(pLuaState, iIndex);

		switch (iType)
		{
		case LUA_TNIL:
			std::sprintf(pType, "LUA_TNIL");
			std::sprintf(pValue, "nil");
			break;
		case LUA_TNUMBER:
			std::sprintf(pType, "LUA_TNUMBER");
			std::sprintf(pValue, LUA_NUMBER_SCAN, ::lua_tonumber(pLuaState, iIndex));
			break;
		case LUA_TSTRING:
			std::sprintf(pType, "LUA_TSTRING");
			std::sprintf(pValue, "%s", ::lua_tostring(pLuaState, iIndex));
			break;
		case LUA_TTABLE:
			std::sprintf(pType, "LUA_TTABLE");
			std::sprintf(pValue, "<table>");
			break;
		case LUA_TFUNCTION:
			{
				std::sprintf(pType, "LUA_TFUNCTION");

				lua_CFunction f = ::lua_tocfunction(pLuaState, iIndex);
				if (f != NULL)
				{
					std::sprintf(pValue, "0x%p", f);
				}
				else
				{
					std::sprintf(pValue, "Lua function");
				}
			}
			break;
		case LUA_TBOOLEAN:
			std::sprintf(pType, "LUA_TBOOLEAN");
			std::sprintf(pValue, "%s", ::lua_toboolean(pLuaState, iIndex) ? "true" : "false");
			break;
		case LUA_TUSERDATA:
			std::sprintf(pType, "LUA_TUSERDATA");
			std::sprintf(pValue, "<userdata>");
			break;
		case LUA_TTHREAD:
			std::sprintf(pType, "LUA_TTHREAD");
			std::sprintf(pValue, "<thread>");
			break;
		case LUA_TLIGHTUSERDATA:
			std::sprintf(pType, "LUA_TLIGHTUSERDATA");
			std::sprintf(pValue, "<lightuserdata>");
			break;
		}

		// Pop item on top of stack
		if (bPop)
			::lua_pop(pLuaState, 1);

		return iType;
	}

	void ShowStack(lua_State *pLuaState, const char *pszMsg)
	{
		if (!pLuaState || !pszMsg)
			return;

#if LUASTUFF_ENABLE_SPAM
		std::printf("=BEG= (%s)\n", pszMsg);
#endif

		const int iItems = ::lua_gettop(pLuaState);
		for (int i = 1; i <= iItems; i++)
		{
			char szType[256];
			char szValue[256];

			// Don't modify the stack
			LookupTypeVal(pLuaState, -i, szType, szValue, false);

#if LUASTUFF_ENABLE_SPAM
			std::printf("Index: -%i, Type: %s, Value: %s\n", i, szType, szValue);
#endif
		}

#if LUASTUFF_ENABLE_SPAM
		std::printf("=END=\n\n");
#endif
	}

	void DefaultTTYHandler(const char *pszMessage)
	{
		std::printf("%s", pszMessage);
	}

	void OpenLibs(lua_State *pLuaState)
	{
#if SCE_LUA_VER >= 510
		::luaL_openlibs(pLuaState);
#else
		::luaopen_base(pLuaState);
		::luaopen_debug(pLuaState);
		::luaopen_string(pLuaState);
		::luaopen_math(pLuaState);
		::luaopen_table(pLuaState);
#endif
	}

	void RunFunctionWith3Args(lua_State *pLuaState, const char *pszFunction, int iArg1, int iArg2, int iArg3, int errorHandlerAbsIndex)
	{
		SCE_SLED_ASSERT(pLuaState != NULL);
		SCE_SLED_ASSERT(pszFunction != NULL);

		const StackReconciler recon(pLuaState);	

		// Run the function
		{
			::lua_getglobal(pLuaState, pszFunction);
			if (!lua_isnil(pLuaState, -1))
			{
				::lua_pushnumber(pLuaState, (lua_Number)iArg1);
				::lua_pushnumber(pLuaState, (lua_Number)iArg2);
				::lua_pushnumber(pLuaState, (lua_Number)iArg3);
				::lua_pcall(pLuaState, 3, 1, errorHandlerAbsIndex);
			}
		}
	}

	void RunThreadFunctionWith3Args(lua_State *pLuaState, const char *pszFunction, int iArg1, int iArg2, int iArg3)
	{
		SCE_SLED_ASSERT(pLuaState != NULL);
		SCE_SLED_ASSERT(pszFunction != NULL);

		const StackReconciler recon(pLuaState);

		::lua_getglobal(pLuaState, pszFunction);
		if (!lua_isnil(pLuaState, -1))
		{
			::lua_pushnumber(pLuaState, (lua_Number)iArg1);
			::lua_pushnumber(pLuaState, (lua_Number)iArg2);
			::lua_pushnumber(pLuaState, (lua_Number)iArg3);
#if WWS_LUA_VER >= 520
			const int retval = ::lua_resume(pLuaState, NULL, 3);
#elif WWS_LUA_VER >= 510
			const int retval = ::lua_resume(pLuaState, 3);
#else
			#error Unknown Lua version!
#endif
			SCE_SLED_ASSERT(retval == 0);
		}
	}

	int LoadFileContentsIntoLuaState(lua_State *pLuaState, const char *pszFileContents, const int& iFileLen, const char *pszBufferName)
	{
		int iError = 0;

		SCE_SLED_ASSERT(pLuaState != NULL);
		SCE_SLED_ASSERT(pszFileContents != NULL);
		SCE_SLED_ASSERT(pszBufferName != NULL);
		SCE_SLED_ASSERT((int)std::strlen(pszFileContents) > 0);
		SCE_SLED_ASSERT(iFileLen > 0);
		SCE_SLED_ASSERT((int)std::strlen(pszBufferName) > 0);

		iError = ::luaL_loadbuffer(pLuaState, pszFileContents, iFileLen, pszBufferName);
		if (iError != 0)
		{
			std::printf("Lua: luaL_loadbuffer failed - error code: %i!\n", iError);
		}
		else
		{
			std::printf("Lua: luaL_loadbuffer succeeded!\n");

			// Execute the script
			iError = ::lua_pcall(pLuaState, 0, 0, 0);
			if (iError != 0)
			{
				std::printf("Lua: lua_pcall failed - error code: %i\n", iError);
			}
			else
			{
				std::printf("Lua: lua_pcall succeeded!\n");
			}
		}

		return iError;
	}

	static char s_userDataBuf[16];
	const char *CustomUserDataCallback(void *pLuaUserData, void *pUserData)
	{
		SCE_SLEDUNUSED(pUserData);

		if (pLuaUserData == NULL)
			return "Name: <Error>";

		// All userdata in this example are of type "CActor"
		Actor *pActor = (Actor*)pLuaUserData;
		std::sprintf(s_userDataBuf, "Name: %s", pActor->GetName());
		return s_userDataBuf;
	}

	void CustomUserDataFinishCallback(void *pLuaUserData, void *pUserData)
	{
		SCE_SLEDUNUSED(pLuaUserData);
		SCE_SLEDUNUSED(pUserData);
	}

	StackReconciler::StackReconciler(lua_State *pLuaState)
		: m_pLuaState(pLuaState)
		, m_iCount1(::lua_gettop(pLuaState))
	{
	}

	StackReconciler::~StackReconciler()
	{
		const int iCount2 = ::lua_gettop(m_pLuaState);

		// Assert if too many items have been popped
		SCE_SLED_ASSERT_MSG(m_iCount1 <= iCount2, "Possible stack corruption? Too many items have been popped!?");

		if ((iCount2 != m_iCount1) && (iCount2 > m_iCount1))
			::lua_pop(m_pLuaState, iCount2 - m_iCount1);
	}
}
