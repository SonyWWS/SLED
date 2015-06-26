/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef SCE_SLEDPLATFORM_MUTEX_H
#define SCE_SLEDPLATFORM_MUTEX_H

#include "common.h"

#if SCE_SLEDTARGET_OS_WINDOWS
#include "windows/mutex_windows.h"
#else
#error "SceSledPlatformMutex has not been implemented for this platform!"
#endif

/* ---------------------------------------------------------------------------------------------- */

#ifdef __cplusplus
extern "C" {
#endif

typedef enum
{
    SCE_SLEDPLATFORM_MUTEX_NORMAL = 0,
    SCE_SLEDPLATFORM_MUTEX_RECURSIVE = 1
} SceSledPlatformMutexType;
#define SCE_SLEDPLATFORM_MUTEX_NAME_DEFAULT            "SceSledPlatformMutex"

extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformMutexAllocate(SceSledPlatformMutex *pMutex, SceSledPlatformMutexType type);
extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformMutexAllocate2(SceSledPlatformMutex *pMutex, SceSledPlatformMutexType type, const char *pName);
extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformMutexDeallocate(SceSledPlatformMutex *pMutex);
// Fails/returns false  if the mutex has a lock held
extern SCE_SLEDPLATFORM_LINKAGE bool sceSledPlatformMutexTryDeallocate(SceSledPlatformMutex *pMutex);
extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformMutexLock(SceSledPlatformMutex *pMutex);
extern SCE_SLEDPLATFORM_LINKAGE bool sceSledPlatformMutexTryLock(SceSledPlatformMutex *pMutex);
extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformMutexUnlock(SceSledPlatformMutex *pMutex);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformThreadID sceSledPlatformMutexGetOwner(const SceSledPlatformMutex *pMutex);

#ifdef __cplusplus
}
#endif

/* ---------------------------------------------------------------------------------------------- */

#ifdef __cplusplus

#include "memory.h"
#include "thread.h"

namespace sce {
namespace SledPlatform {

/* Mutex class */
class SCE_SLEDPLATFORM_LINKAGE Mutex
{
public:
    SCE_SLEDALLOCATOR_NEW_AND_DELETE(Mutex,"sce::SledPlatform::Mutex")
    SCE_SLEDPLACEMENT_NEW_AND_DELETE
    SCE_SLEDDISABLE_STANDARD_DELETE
    
    Mutex() {}  // Must call Initialize separately
    Mutex(SceSledPlatformMutexType type)            { sceSledPlatformMutexAllocate(&m_mutex, type); }
    Mutex(SceSledPlatformMutexType type, const char *pName) { sceSledPlatformMutexAllocate2(&m_mutex, type, pName); }
	virtual ~Mutex()                    { sceSledPlatformMutexDeallocate(&m_mutex); }
    
	void Initialize(SceSledPlatformMutexType type)  { sceSledPlatformMutexAllocate(&m_mutex, type); }
	void Initialize(SceSledPlatformMutexType type, const char *pName)  { sceSledPlatformMutexAllocate2(&m_mutex, type, pName); }
	void Terminate()                    { sceSledPlatformMutexDeallocate(&m_mutex); }
	void Lock()                         { sceSledPlatformMutexLock(&m_mutex); }
	bool TryLock()                      { return sceSledPlatformMutexTryLock(&m_mutex); }
	void Unlock()                       { sceSledPlatformMutexUnlock(&m_mutex); }
	SceSledPlatformThreadID GetOwner()              { return sceSledPlatformMutexGetOwner(&m_mutex); }
	bool IsLockedBy(SceSledPlatformThreadID id)     { return sceSledPlatformThreadIDsEqual(sceSledPlatformMutexGetOwner(&m_mutex), id); }
	bool IsLockedByCurrentThread()      { return sceSledPlatformThreadIDsEqual(sceSledPlatformMutexGetOwner(&m_mutex), sceSledPlatformThreadGetCurrentThreadID()); }
	
	operator SceSledPlatformMutex& ()               { return m_mutex; }
	SceSledPlatformMutex* operator & ()             { return &m_mutex; }
	
	SceSledPlatformMutex   m_mutex;
};

/* MutexLocker class */
class SCE_SLEDPLATFORM_LINKAGE MutexLocker
{
public:
	MutexLocker(SceSledPlatformMutex *pMutex) : m_pMutex(pMutex), m_locked(false)
	{ Lock(); }

	MutexLocker(SceSledPlatformMutex *pMutex, bool lock) : m_pMutex(pMutex), m_locked(false)
	{ if (lock) Lock(); }

	~MutexLocker()          { Unlock(); }

	void Lock()             { if (!m_locked) { m_locked = true; sceSledPlatformMutexLock(m_pMutex); } }
	void Unlock()           { if (m_locked) { m_locked = false; sceSledPlatformMutexUnlock(m_pMutex); } }

	SceSledPlatformMutex * GetMutex()   { return m_pMutex; }
	bool IsLocked() const   { return m_locked; }

protected:
	SceSledPlatformMutex *m_pMutex;
	bool m_locked;
};



} /* namespace SledPlatform */
} /* namespace sce */

#endif /* __cplusplus */

#endif  /* SCE_SLEDPLATFORM_MUTEX_H */
