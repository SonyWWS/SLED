/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sleddebugger/timer.h"

#include <unittest-cpp/UnitTest++/UnitTest++.h>

namespace sce { namespace Sled { namespace
{
	class HostedTimer
	{
	public:
		HostedTimer()
		{
			m_timer = 0;
			m_timerMem = 0;
		}

		~HostedTimer()
		{
			if (m_timer)
			{
				Timer::shutdown(m_timer);
				m_timer = 0;
			}

			if (m_timerMem)
			{
				delete [] m_timerMem;
				m_timerMem = 0;
			}
		}

		int32_t Setup()
		{
			std::size_t iMemSize;

			const int32_t iError = Timer::requiredMemory(&iMemSize);
			if (iError != 0)
				return iError;

			m_timerMem = new char[iMemSize];
			if (!m_timerMem)
				return -1;

			return Timer::create(m_timerMem, &m_timer);
		}

		Timer *m_timer;

	private:
		char *m_timerMem;
	};

	struct Fixture
	{
		Fixture()
		{
		}

		HostedTimer host;
	};

	TEST_FIXTURE(Fixture, CreateTimer)
	{
		CHECK_EQUAL(0, host.Setup());
	}

	TEST_FIXTURE(Fixture, CheckElapsed)
	{
		CHECK_EQUAL(0, host.Setup());
		for (int i = 0; i < 1000; i++);
		CHECK_EQUAL(true, host.m_timer->elapsed() >= 0.0f);
		host.m_timer->reset();
		for (int i = 0; i < 1000; i++);
		CHECK_EQUAL(true, host.m_timer->elapsed() >= 0.0f);
	}
}}}
