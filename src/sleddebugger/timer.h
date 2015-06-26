/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_TIMER_H__
#define __SCE_LIBSLEDDEBUGGER_TIMER_H__

#include "../sledcore/base_types.h"
#include <cstdio>

#include "common.h"

/// Namespace for sce classes and functions.
namespace sce
{
/// Namespace for Sled classes and functions.
namespace Sled
{
	// Forward declarations.
	class ISequentialAllocator;
	class TimerImpl;

	/// Multi-platform timer class.
	/// @brief
	/// Multi-platform timer.
	class SCE_SLED_LINKAGE Timer
	{
	public:
		/// Create a Timer instance.
		/// @brief
		/// Create Timer instance.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pLocation Location in memory in which to place the Timer instance. It needs to be as big as the value returned by <c>requiredMemory()</c>.
		/// @param ppTimer Timer instance that is created
		///
		/// @retval 0 Success
		///
		/// @see
		/// <c>requiredMemory</c>, <c>shutdown</c>, <c>reset</c>
		static int32_t create(void *pLocation, Timer **ppTimer);

		/// Calculate the size in bytes required for a <c>Timer</c> instance.
		/// @brief
		/// Calculate size in bytes required for <c>Timer</c> instance.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param iRequiredMemory The amount of memory that is needed for the <c>Timer</c> instance
		///
		/// @retval 0 Success
		///
		/// @see
		/// <c>create</c>
		static int32_t requiredMemory(std::size_t *iRequiredMemory);

	#ifndef DOXYGEN_IGNORE
	/// @cond
		static int32_t requiredMemoryHelper(ISequentialAllocator *pAllocator, void **ppThis);
	/// @endcond
	#endif // DOXYGEN_IGNORE

		/// Shut down a <c>Timer</c> instance.
		/// @brief
		/// Shut down <c>Timer</c> instance.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pTimer <c>Timer</c> instance to shut down
		///
		/// @see
		/// <c>create</c>, <c>reset</c>
		static void shutdown(Timer *pTimer);
	private:
		Timer();
		~Timer() {}
		Timer(const Timer&);
		Timer& operator=(const Timer&);
	public:
		/// Reset the <c>Timer</c>.
		/// @brief
		/// Reset <c>Timer</c>.
		///
		/// @see
		/// <c>shutdown</c>
		void reset();

		/// Get the elapsed time of the <c>Timer</c>.
		/// @brief
		/// Get elapsed time of <c>Timer</c>.
		///
		/// @return Elapsed time of <c>Timer</c>.
		float elapsed() const;
	private:
		TimerImpl*	m_impl;
		uint8_t		m_data[96];
	};
}}

#endif // __SCE_LIBSLEDDEBUGGER_TIMER_H__
