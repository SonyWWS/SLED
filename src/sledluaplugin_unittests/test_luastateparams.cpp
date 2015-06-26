/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sledluaplugin/luautils.h"
#include "../sleddebugger/utilities.h"

#include <unittest-cpp/UnitTest++/UnitTest++.h>

#include <wws_lua/extras/Lua.Utilities/LuaInterface.h>

namespace sce { namespace Sled { namespace
{
	struct Fixture
	{
		Fixture()
		{
		}
	};

	TEST_FIXTURE(Fixture, LuaStateParams_CreateDefault)
	{
		LuaStateParams params;
		CHECK_EQUAL(true, params.luaState == NULL);
		CHECK_EQUAL(true, params.isDebugging());
	}

	TEST_FIXTURE(Fixture, LuaStateParams_CreateWithLuaState)
	{
		lua_State *state = LuaInterface::Open();

		LuaStateParams params(state);
		CHECK_EQUAL(true, params.luaState == state);
		CHECK_EQUAL(true, params.isDebugging());

		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaStateParams_CreateWithLuaStateAndDebugging)
	{
		lua_State *state = LuaInterface::Open();

		LuaStateParams params(state, false);
		CHECK_EQUAL(true, params.luaState == state);
		CHECK_EQUAL(true, !params.isDebugging());

		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaStateParams_ToggleDebugging)
	{
		LuaStateParams params;

		CHECK_EQUAL(true, params.isDebugging());
		params.setDebugging(false);
		CHECK_EQUAL(false, params.isDebugging());
		params.setDebugging(true);
		CHECK_EQUAL(true, params.isDebugging());
		params.setDebugging(false);
		CHECK_EQUAL(false, params.isDebugging());
	}

	TEST_FIXTURE(Fixture, LuaStateParams_CheckEqual)
	{
		lua_State *state = LuaInterface::Open();

		LuaStateParams params1(state, false);
		LuaStateParams params2(state, true);

		CHECK_EQUAL(true, params1 == params2);

		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, LuaStateParams_CheckNotEqual)
	{
		lua_State *state1 = LuaInterface::Open();
		lua_State *state2 = LuaInterface::Open();

		LuaStateParams params1(state1, false);
		LuaStateParams params2(state2, true);

		CHECK_EQUAL(false, params1 == params2);

		LuaInterface::Close(state1);
		LuaInterface::Close(state2);
	}

	TEST_FIXTURE(Fixture, LuaStateParams_CheckCopyConstructor)
	{
		lua_State *state = LuaInterface::Open();

		LuaStateParams params1(state, false);
		LuaStateParams params2(params1);

		CHECK_EQUAL(true, params1.luaState == params2.luaState);
		CHECK_EQUAL(true, params1.debugging == params2.debugging);

		LuaInterface::Close(state);
	}
}}}
