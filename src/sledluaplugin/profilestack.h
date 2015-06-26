/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDLUAPLUGIN_PROFILESTACK_H__
#define __SCE_LIBSLEDLUAPLUGIN_PROFILESTACK_H__

#include "../sledcore/base_types.h"
#include <cstdio>

#include "../sleddebugger/common.h"

namespace sce { namespace Sled
{
	// Forward declarations
	class Timer; 
	class ISequentialAllocator;

	// Forward declaration
	class ProfileStack;
	class ProfileEntry;
	struct LuaPluginConfig;

	class SCE_SLED_LINKAGE ProfileEntryFunctionConstIterator
	{
	public:
		ProfileEntryFunctionConstIterator(const ProfileEntry *pEntry);
		~ProfileEntryFunctionConstIterator() {}
	private:
		ProfileEntryFunctionConstIterator(const ProfileEntryFunctionConstIterator&);
		ProfileEntryFunctionConstIterator& operator=(const ProfileEntryFunctionConstIterator&);
	public:
		bool operator()() const;
		const ProfileEntry *get() const;
	public:
		inline ProfileEntryFunctionConstIterator& operator++() { ++m_iIndex; return *this; }
		inline void reset() { m_iIndex = 0; }
	private:
		const ProfileEntry *m_pEntry;
		uint16_t m_iIndex;
	};

	class SCE_SLED_LINKAGE ProfileEntry
	{
	public:
		typedef ProfileEntryFunctionConstIterator	ConstIterator;
	private:
		static const uint16_t kFuncLen = 256;
		static const uint16_t kSourceLen = 256;
		static const uint16_t kMaxFnCalls = 64;
	public:
		ProfileEntry(const char *pszFnName, const char *pszFnFile, const int& iFnLine);	
	private:
		ProfileEntry() {}
		~ProfileEntry() {}
		ProfileEntry(const ProfileEntry&);
		ProfileEntry& operator=(const ProfileEntry&);
	public:
		inline const char *getFnName() const				{ return m_szFnName; }
		inline uint32_t getFnNameHash() const				{ return m_uFnNameHash; }
		inline const char *getFnFile() const				{ return m_szFnFile; }
		inline uint32_t getFnFileHash() const				{ return m_uFnFileHash; }
		inline int32_t getFnLine() const					{ return m_iFnLine; }
		inline uint32_t getFnCallCount() const				{ return m_iFnCallCount; }
		inline float getFnTimeStart() const					{ return m_flFnTimeStart; }
		inline float getFnTimeStop() const					{ return m_flFnTimeStop; }	
		inline float getFnTimeElapsed() const				{ return m_flFnTimeElapsed; }
		inline float getFnTimeElapsedAvg() const			{ return m_flFnTimeElapsedAvg; }
		inline float getFnTimeElapsedShortest() const		{ return m_flFnTimeElapsedShortest; }
		inline float getFnTimeElapsedLongest() const		{ return m_flFnTimeElapsedLongest; }
		inline float getFnTimeInnerElapsed() const			{ return m_flFnTimeInnerElapsed; }
		inline float getFnTimeInnerElapsedAvg() const		{ return m_flFnTimeInnerElapsedAvg; }
		inline float getFnTimeInnerElapsedShortest() const	{ return m_flFnTimeInnerElapsedShortest; }
		inline float getFnTimeInnerElapsedLongest() const	{ return m_flFnTimeInnerElapsedLongest; }
		inline uint16_t getFnCalls() const					{ return m_iNumFnCalls; }
	private:
		char			m_szFnName[kFuncLen];
		char			m_szFnFile[kSourceLen];
		int32_t			m_iFnLine;
		uint32_t		m_uFnNameHash;
		uint32_t		m_uFnFileHash;
		// Number of times the function was called
		uint32_t		m_iFnCallCount;
		// Time the function started and stopped
		float			m_flFnTimeStart;
		float			m_flFnTimeStop;	
		// These values include the time functions inside them took as well
		float			m_flFnTimeElapsed;
		float			m_flFnTimeElapsedAvg;
		float			m_flFnTimeElapsedShortest;
		float			m_flFnTimeElapsedLongest;
		// Used to keep track of how long functions inside this function have
		// taken so they can be subtracted out later
		float			m_flFnTimeInner;
		// These values are just the time of the function itself (ie. subtracting
		// out any sub-functions called within this function)
		float			m_flFnTimeInnerElapsed;
		float			m_flFnTimeInnerElapsedAvg;
		float			m_flFnTimeInnerElapsedShortest;
		float			m_flFnTimeInnerElapsedLongest;
		// List of functions this function has called
		ProfileEntry*	m_hFnCalls[kMaxFnCalls];
		uint16_t		m_iNumFnCalls;	
	private:
		void addFnCall(ProfileEntry *m_pFunc);
	private:
		friend class ProfileEntryFunctionConstIterator;
		friend class ProfileStack;	
	};

	class SCE_SLED_LINKAGE ProfileStackFunctionConstIterator
	{
	public:
		ProfileStackFunctionConstIterator(const ProfileStack *pProfileStack);
		~ProfileStackFunctionConstIterator() {}
	private:
		ProfileStackFunctionConstIterator(const ProfileStackFunctionConstIterator&);
		ProfileStackFunctionConstIterator& operator=(const ProfileStackFunctionConstIterator&);
	public:
		bool operator()() const;
		const ProfileEntry *get() const;
	public:
		inline ProfileStackFunctionConstIterator& operator++() { ++m_iIndex; return *this; }
		inline void reset() { m_iIndex = 0; }
	private:
		const ProfileStack *m_pProfileStack;
		uint16_t					m_iIndex;
	};

	struct SCE_SLED_LINKAGE ProfileStackConfig
	{
		ProfileStackConfig() : maxFunctions(0), maxCallStackDepth(0) {}
		ProfileStackConfig(const ProfileStackConfig& rhs) { init(rhs); }
		ProfileStackConfig& operator=(const ProfileStackConfig& rhs) { init(rhs); return *this; }

		ProfileStackConfig(const LuaPluginConfig *pConfig);

	private:
		void init(const ProfileStackConfig& rhs);
	public:

		uint16_t		maxFunctions;		///< Maximum number of functions to track
		//uint16_t		maxFuncCalls;		///< Maximum number of entries to keep for a function call
		uint16_t		maxCallStackDepth;	///< Maximum callstack depth
	};

	class SCE_SLED_LINKAGE ProfileStack
	{
		friend class ProfileStackFunctionConstIterator;
	public:
		typedef ProfileStackFunctionConstIterator	ConstIterator;
	public:
		static int32_t create(const ProfileStackConfig& stackConfig, void *pLocation, ProfileStack **ppStack);
		static int32_t requiredMemory(const ProfileStackConfig& stackConfig, std::size_t *iRequiredMemory);
		static int32_t requiredMemoryHelper(const ProfileStackConfig& stackConfig, ISequentialAllocator *pAllocator, void **ppThis);
		static void shutdown(ProfileStack *pStack);
	private:
		ProfileStack(const ProfileStackConfig& stackConfig, const void *pStackSeats);
		~ProfileStack() {}
		ProfileStack(const ProfileStack&);
		ProfileStack& operator=(const ProfileStack&);
	public:
		void enterFn(const char *pszFnName, const char *pszFnFile, const int32_t& iFnLine);
		void leaveFn(const char *pszFnName, const char *pszFnFile, const int32_t& iFnLine);
		ProfileEntry *findFn(const char *pszFnName, const char *pszFnFile, const int32_t& iFnLine) const;
		void preBreakpoint();
		void postBreakpoint();
		void clear();
		uint16_t getMaxFunctions() const	{ return m_iMaxFuncs; }
		uint32_t getNumFunctions() const	{ return m_iNumFuncs; }
		inline bool isEmpty() const			{ return m_iNumFuncs == 0; }
		inline bool isFull() const			{ return m_iNumFuncs == m_iMaxFuncs; }	
	private:
		const uint16_t		m_iMaxFuncs;
		uint16_t			m_iNumFuncs;
		ProfileEntry*		m_pFuncs;	

		const uint16_t		m_iMaxCallStack;
		uint16_t			m_iNumCallStack;
		ProfileEntry**		m_ppCallStack;

		float				m_flBpStopTime;
		float				m_flBpTotalTime;
		Timer*				m_pTimer;
	};
}}

#endif // __SCE_LIBSLEDLUAPLUGIN_PROFILESTACK_H__
