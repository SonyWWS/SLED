/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "timer.h"
#include "assert.h"
#include "errorcodes.h"
#include "sequentialallocator.h"

#include <new>
#include <cstdio>

#if SCE_SLEDTARGET_OS_WINDOWS
	#ifndef _WINDOWS_
		#define WIN32_LEAN_AND_MEAN
		#include <windows.h>
	#endif
#else
	#error Not supported
#endif

namespace sce { namespace Sled
{
	class TimerImpl
	{
	public:
		TimerImpl() { Start(); }
		~TimerImpl() {}
	private:
		TimerImpl(const TimerImpl&);
		TimerImpl& operator=(const TimerImpl&);
	public:
		void Start()
		{
#if SCE_SLEDTARGET_OS_WINDOWS
			::QueryPerformanceCounter(&m_start);
			::QueryPerformanceFrequency(&m_freq);
#endif
		}

		float Elapsed() const
		{
#if SCE_SLEDTARGET_OS_WINDOWS
			LARGE_INTEGER end;
			::QueryPerformanceCounter(&end);
			return (float(end.QuadPart - m_start.QuadPart) / m_freq.QuadPart);
#endif
		}

		void Reset() { Start(); }
	private:
#if SCE_SLEDTARGET_OS_WINDOWS
		LARGE_INTEGER m_start;
		LARGE_INTEGER m_freq;
#endif
	};

	namespace
	{
		struct TimerSeats
		{
			void *m_this;

			void Allocate(ISequentialAllocator *pAllocator)
			{
				m_this = pAllocator->allocate(sizeof(Timer), __alignof(Timer));
			}
		};

		inline int32_t ValidateConfig()
		{
			return SCE_SLED_ERROR_OK;
		}
	}

	int32_t Timer::create(void *pLocation, Timer **ppTimer)
	{
		SCE_SLED_ASSERT(pLocation != NULL);
		SCE_SLED_ASSERT(ppTimer != NULL);

		std::size_t iMemSize = 0;
		const int32_t iConfigError = requiredMemory(&iMemSize);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocator allocator(pLocation, iMemSize);

		TimerSeats seats;
		seats.Allocate(&allocator);

		SCE_SLED_ASSERT(seats.m_this != NULL);

		*ppTimer = new (seats.m_this) Timer();
		return SCE_SLED_ERROR_OK;
	}

	int32_t Timer::requiredMemory(std::size_t *iRequiredMemory)
	{
		SCE_SLED_ASSERT(iRequiredMemory != NULL);

		const int32_t iConfigError = ValidateConfig();
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocatorCalculator allocator;

		TimerSeats seats;
		seats.Allocate(&allocator);

		*iRequiredMemory = allocator.bytesAllocated();
		return SCE_SLED_ERROR_OK;
	}

	int32_t Timer::requiredMemoryHelper(ISequentialAllocator *pAllocator, void **ppThis)
	{
		SCE_SLED_ASSERT(pAllocator != NULL);
		SCE_SLED_ASSERT(ppThis != NULL);

		const int32_t iConfigError = ValidateConfig();
		if (iConfigError != 0)
			return iConfigError;

		TimerSeats seats;
		seats.Allocate(pAllocator);

		*ppThis = seats.m_this;
		return SCE_SLED_ERROR_OK;
	}

	void Timer::shutdown(Timer *pTimer)
	{
		SCE_SLED_ASSERT(pTimer != NULL);
		pTimer->~Timer();
	}

	Timer::Timer()
	{
		uint8_t *pData = static_cast<uint8_t*>(m_data);
		m_impl = new (pData) TimerImpl();
	}

	void Timer::reset()
	{
		m_impl->Reset();
	}

	float Timer::elapsed() const
	{
		return m_impl->Elapsed();
	}
}}
