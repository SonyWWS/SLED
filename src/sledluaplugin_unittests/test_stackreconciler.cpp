/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sledluaplugin/luautils.h"
#include "../sleddebugger/sleddebugger.h"
#include "../sleddebugger/assert.h"
#include "../sleddebugger/utilities.h"

#include <unittest-cpp/UnitTest++/UnitTest++.h>
#include <unittest-cpp/UnitTest++/ReportAssert.h>

#include <wws_lua/extras/Lua.Utilities/LuaInterface.h>

namespace sce { namespace Sled { namespace
{
	class StackReconcilerHelper
	{
	public:
		StackReconcilerHelper(lua_State *state)
			: m_bAsserted(false)
			, m_state(state)
		{
			if (s_pInstance)
				UnitTest::ReportAssert("StackReconcilerHelper used incorrectly!", __FILE__, __LINE__);

			if (!m_state)
				UnitTest::ReportAssert("StackReconcilerHelper used incorrectly!", __FILE__, __LINE__);

			s_pInstance = this;
		}

		~StackReconcilerHelper()
		{
			if (!s_pInstance)
				UnitTest::ReportAssert("StackReconcilerHelper used incorrectly!", __FILE__, __LINE__);

			s_pInstance = 0;
		}

		static Assert::FailureBehavior CatchAssertHandler(const char *condition, const char *file, const int& line, const char *msg)
		{
			SCE_SLEDUNUSED(condition);
			SCE_SLEDUNUSED(file);
			SCE_SLEDUNUSED(line);
			SCE_SLEDUNUSED(msg);

			if (!s_pInstance)
				UnitTest::ReportAssert("StackReconcilerHelper used incorrectly!", __FILE__, __LINE__);		

			// Clear out the stack
			LuaInterface::Pop(s_pInstance->m_state, LuaInterface::GetTop(s_pInstance->m_state));

			// Set flag
			s_pInstance->m_bAsserted = true;

			// Continue execution
			return Assert::kContinue;
		}

	#ifdef SCE_SLED_ASSERT_ENABLED
	#if SCE_SLED_ASSERT_ENABLED
		inline bool Asserted() const { return m_bAsserted; }
	#endif
	#endif

	private:
		bool m_bAsserted;
		lua_State *m_state;

	private:
		static StackReconcilerHelper *s_pInstance;
	};

	StackReconcilerHelper *StackReconcilerHelper::s_pInstance = 0;

	struct Fixture
	{
		Fixture()
		{
		}
	};

	TEST_FIXTURE(Fixture, StackReconciler_Create)
	{
		lua_State *state = LuaInterface::Open();
		const int32_t top = LuaInterface::GetTop(state);

		{
			StackReconciler recon(state);
		}

		CHECK_EQUAL(top, LuaInterface::GetTop(state));
		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, StackReconciler_AddABunchOfItemsToLuaAndDontPop)
	{
		lua_State *state = LuaInterface::Open();

		{
			StackReconciler recon(state);
 
			LuaInterface::PushString(state, "Hello world! 1");
			LuaInterface::PushNumber(state, 12341);
			LuaInterface::PushString(state, "Hello world! 2");
			LuaInterface::PushNumber(state, 12342);
			LuaInterface::PushString(state, "Hello world! 3");
			LuaInterface::PushNumber(state, 12343);
			LuaInterface::PushNil(state);
			LuaInterface::PushString(state, "Hello world! 4");
			LuaInterface::PushNumber(state, 12344);
			LuaInterface::PushString(state, "Hello world! 5");
			LuaInterface::PushNumber(state, 12345);
			LuaInterface::PushString(state, "Hello world! 6");
			LuaInterface::PushNumber(state, 12346);
		}

		CHECK_EQUAL(0, LuaInterface::GetTop(state));
		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, StackReconciler_AddABunchOfItemsToLuaAndThenPop)
	{
		lua_State *state = LuaInterface::Open();

		{
			StackReconciler recon(state);

			LuaInterface::PushString(state, "Hello world! 1");
			LuaInterface::PushNumber(state, 12341);
			LuaInterface::PushString(state, "Hello world! 2");
			LuaInterface::PushNumber(state, 12342);
			LuaInterface::PushString(state, "Hello world! 3");
			LuaInterface::PushNumber(state, 12343);
			LuaInterface::PushNil(state);
			LuaInterface::PushString(state, "Hello world! 4");
			LuaInterface::PushNumber(state, 12344);
			LuaInterface::PushString(state, "Hello world! 5");
			LuaInterface::PushNumber(state, 12345);
			LuaInterface::PushString(state, "Hello world! 6");
			LuaInterface::PushNumber(state, 12346);

			// Pop all values leaving recon with nothing to do
			LuaInterface::Pop(state, LuaInterface::GetTop(state));
		}

		CHECK_EQUAL(0, LuaInterface::GetTop(state));
		LuaInterface::Close(state);
	}

	TEST_FIXTURE(Fixture, StackReconciler_AddABunchOfItemsToLuaAndThenPopMoreThanStartedWith)
	{
		// Store default assert handler
		Assert::Handler handler = Assert::assertHandler();

		lua_State *state = LuaInterface::Open();
		StackReconcilerHelper helper(state);
	
		LuaInterface::PushString(state, "Hello world! 1");
		LuaInterface::PushNumber(state, 12341);
		LuaInterface::PushString(state, "Hello world! 2");

		{
			StackReconciler recon(state);

			LuaInterface::PushString(state, "Hello world! 1");
			LuaInterface::PushNumber(state, 12341);
			LuaInterface::PushString(state, "Hello world! 2");
			LuaInterface::PushNumber(state, 12342);
			LuaInterface::PushString(state, "Hello world! 3");
			LuaInterface::PushNumber(state, 12343);
			LuaInterface::PushNil(state);
			LuaInterface::PushString(state, "Hello world! 4");
			LuaInterface::PushNumber(state, 12344);
			LuaInterface::PushString(state, "Hello world! 5");
			LuaInterface::PushNumber(state, 12345);
			LuaInterface::PushString(state, "Hello world! 6");
			LuaInterface::PushNumber(state, 12346);

			// Pop all values leaving recon with less than it started with
			LuaInterface::Pop(state, LuaInterface::GetTop(state));

			// Redirect default assert handler to catch the StackReconciler destructor assert
			Assert::setAssertHandler(StackReconcilerHelper::CatchAssertHandler);
		}

		// Reset assert handler back to default
		Assert::setAssertHandler(handler);

		CHECK_EQUAL(0, LuaInterface::GetTop(state));
		LuaInterface::Close(state);

#ifdef SCE_SLED_ASSERT_ENABLED
	#if SCE_SLED_ASSERT_ENABLED
		CHECK_EQUAL(true, helper.Asserted());
	#endif
#endif // SCE_SLED_ASSERT_ENABLED
	}
}}}
