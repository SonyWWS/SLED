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

	TEST_FIXTURE(Fixture, MemTraceParams_CreateDefault)
	{
		MemTraceParams params;

		CHECK_EQUAL(true, params.what == ' ');
		CHECK_EQUAL(true, params.oldPtr == NULL);
		CHECK_EQUAL(true, params.newPtr == NULL);
		CHECK_EQUAL(true, params.oldSize == 0);
		CHECK_EQUAL(true, params.newSize == 0);
	}

	TEST_FIXTURE(Fixture, MemTraceParams_CreateWithValues)
	{
		const char ch = 'd';

		std::size_t num1 = 5;
		std::size_t num2 = 6;
		std::size_t *pNum1 = &num1;
		std::size_t *pNum2 = &num2;

		void *pVoid1 = reinterpret_cast< void* >(pNum1);
		void *pVoid2 = reinterpret_cast< void* >(pNum2);

		MemTraceParams params(ch, pVoid1, pVoid2, num1, num2);

		CHECK_EQUAL(true, params.what == ch);
		CHECK_EQUAL(true, params.oldPtr == pVoid1);
		CHECK_EQUAL(true, params.newPtr == pVoid2);
		CHECK_EQUAL(true, params.oldSize == num1);
		CHECK_EQUAL(true, params.newSize == num2);
	}

	TEST_FIXTURE(Fixture, MemTraceParams_CheckCopyConstructor)
	{
		const char ch = 'd';

		std::size_t num1 = 5;
		std::size_t num2 = 6;
		std::size_t *pNum1 = &num1;
		std::size_t *pNum2 = &num2;

		void *pVoid1 = reinterpret_cast< void* >(pNum1);
		void *pVoid2 = reinterpret_cast< void* >(pNum2);

		MemTraceParams params1(ch, pVoid1, pVoid2, num1, num2);
		MemTraceParams params2(params1);

		CHECK_EQUAL(true, params1.what == params2.what);
		CHECK_EQUAL(true, params1.oldPtr == params2.oldPtr);
		CHECK_EQUAL(true, params1.newPtr == params2.newPtr);
		CHECK_EQUAL(true, params1.oldSize == params2.oldSize);
		CHECK_EQUAL(true, params1.newSize == params2.newSize);
	}
}}}
