/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sledluaplugin/sledluaplugin.h"
#include "../sleddebugger/utilities.h"
#include "../sledluaplugin/profilestack.h"

#include <unittest-cpp/UnitTest++/UnitTest++.h>

#include <cstdlib>
#include <cstdio>
#include <cstring>

#include <wws_lua/extras/Lua.Utilities/LuaInterface.h>

namespace sce { namespace Sled { namespace
{
	class HostedProfileStackConfig
	{
	public:	
		ProfileStackConfig Default()
		{
			ProfileStackConfig config;
			Setup(config);
			return config;
		}
	private:
		static void Setup(ProfileStackConfig& config)
		{
			config.maxFunctions = 100;
			config.maxCallStackDepth = 64;
			//config.maxFunctions = 1;
			//config.maxCallStackDepth = 1;
		}
	};

	class HostedProfileStack
	{
	public:
		HostedProfileStack()
		{
			m_stack = 0;
			m_stackMem = 0;
		}

		~HostedProfileStack()
		{
			if (m_stack)
			{
				ProfileStack::shutdown(m_stack);
				m_stack = 0;
			}

			if (m_stackMem)
			{
				delete [] m_stackMem;
				m_stackMem = 0;
			}
		}

		int32_t Setup(const ProfileStackConfig& config)
		{
			std::size_t iMemSize;

			const int32_t iError = ProfileStack::requiredMemory(config, &iMemSize);
			if (iError != 0)
				return iError;

			m_stackMem = new char[iMemSize];
			std::memset(m_stackMem, 0xAB, iMemSize);
			if (!m_stackMem)
				return -1;

			return ProfileStack::create(config, m_stackMem, &m_stack);
		}

		ProfileStack *m_stack;

	private:
		char *m_stackMem;
	};

	struct Fixture
	{
		Fixture()
		{
		}

		HostedProfileStack host;
		HostedProfileStackConfig config;
	};

	TEST_FIXTURE(Fixture, ProfileStack_Create)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));
		CHECK_EQUAL(true, host.m_stack->isEmpty());
		CHECK_EQUAL(false, host.m_stack->isFull());
	}

	TEST_FIXTURE(Fixture, ProfileStack_EnterAndExitFunctions)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		const uint32_t iNumFuncs = 6;
		const char *functionList[] =
		{
			"main",
			"sub1",
			"sub2",
			"sub3",
			"sub4",
			"sub5",
			NULL
		};

		const char *pszFile = "/app_home/game/scripts/level1.lua";

		for (uint32_t i = 0; i < iNumFuncs; i++)
			host.m_stack->enterFn(functionList[i], pszFile, i);

		CHECK_EQUAL(iNumFuncs, host.m_stack->getNumFunctions());

		for (uint32_t i = iNumFuncs; i > 0; i--)
			host.m_stack->leaveFn(functionList[i - 1], pszFile, i - 1);

		CHECK_EQUAL(iNumFuncs, host.m_stack->getNumFunctions());

		host.m_stack->clear();
		CHECK_EQUAL((uint32_t)0, host.m_stack->getNumFunctions());
	}

	TEST_FIXTURE(Fixture, ProfileStack_FindFunctions)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		const uint32_t iNumFuncs = 6;
		const char *functionList[] =
		{
			"main",
			"sub1",
			"sub2",
			"sub3",
			"sub4",
			"sub5",
			NULL
		};

		const char *pszFile = "/app_home/game/scripts/level1.lua";

		for (uint32_t i = 0; i < iNumFuncs; i++)
			host.m_stack->enterFn(functionList[i], pszFile, i);

		CHECK_EQUAL(iNumFuncs, host.m_stack->getNumFunctions());

		for (uint32_t i = iNumFuncs; i > 0; i--)
			host.m_stack->leaveFn(functionList[i - 1], pszFile, i - 1);

		CHECK_EQUAL(iNumFuncs, host.m_stack->getNumFunctions());

		for (uint32_t i = 0; i < iNumFuncs; i++)
			CHECK_EQUAL(true, host.m_stack->findFn(functionList[i], pszFile, i) != NULL);

		host.m_stack->clear();
		CHECK_EQUAL((uint32_t)0, host.m_stack->getNumFunctions());
	}

	TEST_FIXTURE(Fixture, ProfileStack_PreAndPostBreakpoint)
	{
		CHECK_EQUAL(0, host.Setup(config.Default()));

		const uint32_t iNumFuncs = 6;
		const char *functionList[] =
		{
			"main",
			"sub1",
			"sub2",
			"sub3",
			"sub4",
			"sub5",
			NULL
		};

		const char *pszFile = "/app_home/game/scripts/level1.lua";

		for (uint32_t i = 0; i < iNumFuncs; i++)
			host.m_stack->enterFn(functionList[i], pszFile, i);

		CHECK_EQUAL(iNumFuncs, host.m_stack->getNumFunctions());

		host.m_stack->preBreakpoint();

		for (uint32_t i = 0; i < 1000; i++);

		host.m_stack->postBreakpoint();

		for (uint32_t i = iNumFuncs; i > 0; i--)
			host.m_stack->leaveFn(functionList[i - 1], pszFile, i - 1);

		CHECK_EQUAL(iNumFuncs, host.m_stack->getNumFunctions());

		for (uint32_t i = 0; i < iNumFuncs; i++)
			CHECK_EQUAL(true, host.m_stack->findFn(functionList[i], pszFile, i) != NULL);

		host.m_stack->clear();
		CHECK_EQUAL((uint32_t)0, host.m_stack->getNumFunctions());
	}
}}}
