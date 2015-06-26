/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "profilestack.h"
#include "sledluaplugin.h"
#include "../sleddebugger/assert.h"
#include "../sleddebugger/errorcodes.h"
#include "../sleddebugger/sequentialallocator.h"
#include "../sleddebugger/sleddebugger_class.h"
#include "../sleddebugger/timer.h"
#include "../sleddebugger/utilities.h"

#include <new>
#include <cstring>

namespace sce { namespace Sled
{
	ProfileEntryFunctionConstIterator::ProfileEntryFunctionConstIterator(const ProfileEntry *pEntry)
		: m_pEntry(pEntry)
		, m_iIndex(0)
	{
		SCE_SLED_ASSERT(pEntry != NULL);
	}

	bool ProfileEntryFunctionConstIterator::operator()() const
	{
		return m_iIndex < m_pEntry->m_iNumFnCalls;
	}

	const ProfileEntry *ProfileEntryFunctionConstIterator::get() const
	{
		return (m_iIndex < m_pEntry->m_iNumFnCalls) ? m_pEntry->m_hFnCalls[m_iIndex] : 0;
	}

	ProfileEntry::ProfileEntry(const char *pszFnName, const char *pszFnFile, const int& iFnLine)
		: m_iFnLine(iFnLine)
		, m_uFnNameHash(SledDebugger::generateFNV1AHash(pszFnName))
		, m_uFnFileHash(SledDebugger::generateFNV1AHash(pszFnFile))
		, m_iFnCallCount(0)
		, m_flFnTimeStart(0.0f)
		, m_flFnTimeStop(0.0f)
		, m_flFnTimeElapsed(0.0f)
		, m_flFnTimeElapsedAvg(0.0f)
		, m_flFnTimeElapsedShortest(0.0f)
		, m_flFnTimeElapsedLongest(0.0f)
		, m_flFnTimeInner(0.0f)
		, m_flFnTimeInnerElapsed(0.0f)
		, m_flFnTimeInnerElapsedAvg(0.0f)
		, m_flFnTimeInnerElapsedShortest(0.0f)
		, m_flFnTimeInnerElapsedLongest(0.0f)
		, m_iNumFnCalls(0)
	{
		Utilities::copyString(m_szFnName, kFuncLen, pszFnName);
		Utilities::copyString(m_szFnFile, kSourceLen, pszFnFile);
		std::memset(m_hFnCalls, 0, kMaxFnCalls * sizeof(ProfileEntry*));
	}

	void ProfileEntry::addFnCall(ProfileEntry *m_pFunc)
	{
		SCE_SLED_ASSERT(m_pFunc != NULL);

		// Check if there's space
		if (m_iNumFnCalls == kMaxFnCalls)
			return;

		// Check for duplicate
		for (uint16_t i = 0; i < m_iNumFnCalls; i++)
		{
			if (m_hFnCalls[i] == m_pFunc)
				return;
		}

		// Add
		m_hFnCalls[m_iNumFnCalls++] = m_pFunc;
	}

	ProfileStackFunctionConstIterator::ProfileStackFunctionConstIterator(const ProfileStack *pProfileStack)
		: m_pProfileStack(pProfileStack)
		, m_iIndex(0)
	{
		SCE_SLED_ASSERT(pProfileStack != NULL);
	}

	bool ProfileStackFunctionConstIterator::operator()() const
	{
		return m_iIndex < m_pProfileStack->m_iNumFuncs;
	}

	const ProfileEntry *ProfileStackFunctionConstIterator::get() const
	{
		return (m_iIndex < m_pProfileStack->m_iNumFuncs) ? &m_pProfileStack->m_pFuncs[m_iIndex] : 0;
	}

	void ProfileStackConfig::init(const ProfileStackConfig& rhs)
	{
		maxFunctions = rhs.maxFunctions;
		maxCallStackDepth = rhs.maxCallStackDepth;
	}

	ProfileStackConfig::ProfileStackConfig(const LuaPluginConfig *pConfig)
	{
		SCE_SLED_ASSERT(pConfig != NULL);

		maxFunctions = pConfig->maxProfileFunctions;
		//maxFuncCalls = pConfig->maxProfileFunctionCalls;
		maxCallStackDepth = pConfig->maxProfileCallStackDepth;
	}

	namespace
	{
		struct ProfileStackSeats
		{
			void *m_this;
			void *m_funcs;
			void *m_callStack;
			void *m_timer;

			void Allocate(const ProfileStackConfig& stackConfig, ISequentialAllocator *pAllocator)
			{
				// For this
				m_this = pAllocator->allocate(sizeof(ProfileStack), __alignof(ProfileStack));

				// For m_pFuncs
				m_funcs = pAllocator->allocate(sizeof(ProfileEntry) * stackConfig.maxFunctions, __alignof(ProfileEntry));

				// For m_ppCallStack
				m_callStack = pAllocator->allocate(sizeof(ProfileEntry*) * stackConfig.maxCallStackDepth, __alignof(ProfileEntry*));

				// For m_pTimer
				Timer::requiredMemoryHelper(pAllocator, &m_timer);
			}
		};

		inline int32_t ValidateConfig(const ProfileStackConfig& config)
		{
			const bool bAnyFunctions = config.maxFunctions != 0;
			const bool bAnyCallStack = config.maxCallStackDepth != 0;

			if ((!bAnyFunctions && bAnyCallStack) ||
				(bAnyFunctions && !bAnyCallStack))
				return SCE_SLED_ERROR_INVALIDCONFIGURATION;

			return SCE_SLED_ERROR_OK;
		}
	}

	int32_t ProfileStack::create(const ProfileStackConfig& stackConfig, void *pLocation, ProfileStack **ppStack)
	{
		SCE_SLED_ASSERT(pLocation != NULL);
		SCE_SLED_ASSERT(ppStack != NULL);

		std::size_t iMemSize = 0;

		const int32_t iConfigError = requiredMemory(stackConfig, &iMemSize);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocator allocator(pLocation, iMemSize);

		ProfileStackSeats seats;
		seats.Allocate(stackConfig, &allocator);

		SCE_SLED_ASSERT(seats.m_this != NULL);
		SCE_SLED_ASSERT(seats.m_funcs != NULL);
		SCE_SLED_ASSERT(seats.m_callStack != NULL);
		SCE_SLED_ASSERT(seats.m_timer != NULL);
	
		*ppStack = new (seats.m_this) ProfileStack(stackConfig, &seats);
		return SCE_SLED_ERROR_OK;
	}

	int32_t ProfileStack::requiredMemory(const ProfileStackConfig& stackConfig, std::size_t *iRequiredMemory)
	{
		SCE_SLED_ASSERT(iRequiredMemory != NULL);

		const int32_t iConfigError = ValidateConfig(stackConfig);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocatorCalculator allocator;

		ProfileStackSeats seats;
		seats.Allocate(stackConfig, &allocator);

		*iRequiredMemory = allocator.bytesAllocated();
		return SCE_SLED_ERROR_OK;
	}

	int32_t ProfileStack::requiredMemoryHelper(const ProfileStackConfig& stackConfig, ISequentialAllocator *pAllocator, void **ppThis)
	{
		SCE_SLED_ASSERT(pAllocator != NULL);
		SCE_SLED_ASSERT(ppThis != NULL);

		const int32_t iConfigError = ValidateConfig(stackConfig);
		if (iConfigError != 0)
			return iConfigError;

		ProfileStackSeats seats;
		seats.Allocate(stackConfig, pAllocator);

		*ppThis = seats.m_this;
		return SCE_SLED_ERROR_OK;
	}

	void ProfileStack::shutdown(ProfileStack *pStack)
	{
		SCE_SLED_ASSERT(pStack != NULL);
		pStack->~ProfileStack();
	}

	ProfileStack::ProfileStack(const ProfileStackConfig& stackConfig, const void *pStackSeats)
		: m_iMaxFuncs(stackConfig.maxFunctions)	
		, m_iNumFuncs(0)
		, m_iMaxCallStack(stackConfig.maxCallStackDepth)
		, m_iNumCallStack(0)
	{
		SCE_SLED_ASSERT(pStackSeats != NULL);

		const ProfileStackSeats *pSeats = static_cast<const ProfileStackSeats*>(pStackSeats);

		m_pFuncs = new (pSeats->m_funcs) ProfileEntry[stackConfig.maxFunctions];
		m_ppCallStack = new (pSeats->m_callStack) ProfileEntry*[stackConfig.maxCallStackDepth];
	
		Timer::create(pSeats->m_timer, &m_pTimer);
	}

	//static void DumpFunctions(ProfileEntry *pFuncs, const uint16_t& iNumFuncs)
	//{
	//	SCE_SLED_ASSERT(pFuncs != NULL);
	//
	//	SCE_SLED_LOG(Logging::kInfo, "[SLED] [ProfileStack] Dumping...");
	//
	//	for (uint16_t i = 0; i < iNumFuncs; i++)
	//	{
	//		SCE_SLED_LOG(Logging::kInfo, "[SLED] \t%s", pFuncs[i].GetFnName());
	//	}
	//
	//	SCE_SLED_LOG(Logging::kInfo, "[SLED] [ProfileStack] Finished!");
	//}

	void ProfileStack::enterFn(const char *pszFnName, const char *pszFnFile, const int32_t& iFnLine)
	{	
		SCE_SLED_ASSERT(pszFnName != NULL);
		SCE_SLED_ASSERT(pszFnFile != NULL);
	
		// Find the existing function (if any)
		ProfileEntry *pEntry = findFn(pszFnName, pszFnFile, iFnLine);

		// Check if we can create a new entry	
		if (!pEntry && (m_iNumFuncs == m_iMaxFuncs))
		{
			SCE_SLED_LOG(Logging::kError, "[SLED] Profile function list has no free space; can't add entry for function %s!", pszFnName);
			return;
		}

		// Check if we can add to callstack
		if (m_iNumCallStack == m_iMaxCallStack)
		{
			SCE_SLED_LOG(Logging::kError, "[SLED] Profile callstack is full; can't add entry for function %s!", pszFnName);
			return;
		}	

		// Create new entry
		if (!pEntry)
		{
			void *pSeat = &m_pFuncs[m_iNumFuncs++];
			pEntry = new (pSeat) ProfileEntry(pszFnName, pszFnFile, iFnLine);
		}

		// Update function call references
		if (m_iNumCallStack >= 1)
		{
			ProfileEntry *pLastFn = m_ppCallStack[m_iNumCallStack - 1];

			// If function call entry doesn't exist then add it
			pLastFn->addFnCall(pEntry);
		}

		// Add current entry to call stack
		m_ppCallStack[m_iNumCallStack++] = pEntry;

		// Do profile stuff
		pEntry->m_flFnTimeStart = (m_pTimer->elapsed() - m_flBpTotalTime);
		pEntry->m_iFnCallCount++;
		pEntry->m_flFnTimeInner = 0.0f;		

		//// Print out functions
		//DumpFunctions(m_pFuncs, m_iNumFuncs);
	}

	void ProfileStack::leaveFn(const char *pszFnName, const char *pszFnFile, const int32_t& iFnLine)
	{
		// Won't be any entries to modify
		if (isEmpty())
			return;

		// Find this function
		ProfileEntry *pEntry = findFn(pszFnName, pszFnFile, iFnLine);

		// No entry; nothing to modify
		if (!pEntry)
			return;

		// Update call stack - pop off _this_ function that just ended
		if (m_iNumCallStack != 0)
			m_iNumCallStack--;

		//
		// Update profile information
		//

		// This is the time the function took from start to end - this value includes
		// any functions that were called inside this function as well
		const float flElapsed = (m_pTimer->elapsed() - m_flBpTotalTime) - pEntry->m_flFnTimeStart;

		/*
		static ProfileEntry* fnTimesWrapper = NULL;

		if ((fnTimesWrapper == NULL) && (strcmp(pszFnName, "timesWrapper") == 0))
			fnTimesWrapper = pEntry;
		
		if (fnTimesWrapper != NULL)
		{
			static int count = 0;

			if ((count % 100) == 0)
				std::printf("timesWrapper: %f\n", flElapsed);

			count++;
		}
		*/

		// This is the time the function took from start to end excluding time spent
		// in functions called from this function
		float flElapsedInner = flElapsed - pEntry->m_flFnTimeInner;
	
		// Clamp at zero
		if (flElapsedInner < 0.0f)
			flElapsedInner = 0.0f;
	
		if (pEntry->m_iFnCallCount == 1)
		{
			pEntry->m_flFnTimeElapsed = flElapsed;
			pEntry->m_flFnTimeElapsedAvg = flElapsed;
			pEntry->m_flFnTimeElapsedShortest = flElapsed;
			pEntry->m_flFnTimeElapsedLongest = flElapsed;

			pEntry->m_flFnTimeInnerElapsed = flElapsedInner;
			pEntry->m_flFnTimeInnerElapsedAvg = flElapsedInner;
			pEntry->m_flFnTimeInnerElapsedShortest = flElapsedInner;
			pEntry->m_flFnTimeInnerElapsedLongest = flElapsedInner;
		}
		else
		{
			if (flElapsed < pEntry->m_flFnTimeElapsedShortest)
				pEntry->m_flFnTimeElapsedShortest = flElapsed;
			if (flElapsed > pEntry->m_flFnTimeElapsedLongest)
				pEntry->m_flFnTimeElapsedLongest = flElapsed;

			pEntry->m_flFnTimeElapsed += flElapsed;
			pEntry->m_flFnTimeElapsedAvg = (pEntry->m_flFnTimeElapsed / (float)pEntry->m_iFnCallCount);

			if (flElapsedInner < pEntry->m_flFnTimeInnerElapsedShortest)
				pEntry->m_flFnTimeInnerElapsedShortest = flElapsedInner;
			if (flElapsedInner > pEntry->m_flFnTimeInnerElapsedLongest)
				pEntry->m_flFnTimeInnerElapsedLongest = flElapsedInner;

			pEntry->m_flFnTimeInnerElapsed += flElapsedInner;
			pEntry->m_flFnTimeInnerElapsedAvg = (pEntry->m_flFnTimeInnerElapsed / (float)pEntry->m_iFnCallCount);
		}

		// Update last function that called this function to keep track of 'inner' function times
		if (m_iNumCallStack >= 1)
		{
			ProfileEntry *pParentFunc = m_ppCallStack[m_iNumCallStack - 1];
			pParentFunc->m_flFnTimeInner += flElapsedInner;
		}

		//// Print out functions
		//DumpFunctions(m_pFuncs, m_iNumFuncs);
	}

	ProfileEntry *ProfileStack::findFn(const char *pszFnName, const char *pszFnFile, const int32_t& iFnLine) const	
	{
		const uint32_t fnNameHash = SledDebugger::generateFNV1AHash(pszFnName);
		const uint32_t fnFileHash = SledDebugger::generateFNV1AHash(pszFnFile);

		// Search to find the ProfileEntry* for this particular function
		for (uint32_t i = 0; i < m_iNumFuncs; i++)
		{
			if (iFnLine == m_pFuncs[i].getFnLine() &&
				fnNameHash == m_pFuncs[i].getFnNameHash() &&
				fnFileHash == m_pFuncs[i].getFnFileHash())
			{
				return &m_pFuncs[i];
			}
		}

		return 0;
	}

	void ProfileStack::preBreakpoint()
	{
		m_flBpStopTime = m_pTimer->elapsed();
	}

	void ProfileStack::postBreakpoint()
	{
		m_flBpTotalTime += (m_pTimer->elapsed() - m_flBpStopTime);
	}

	void ProfileStack::clear()
	{
		m_iNumFuncs = 0;
		m_iNumCallStack = 0;

		m_flBpStopTime = 0.0f;
		m_flBpTotalTime = 0.0f;

		m_pTimer->reset();
	}
}}
