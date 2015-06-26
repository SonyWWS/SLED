/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef SCE_SLEDPLATFORM_THREAD_H
#define SCE_SLEDPLATFORM_THREAD_H

#include "common.h"

#if SCE_SLEDTARGET_OS_WINDOWS
#include "windows/thread_windows.h"
#else
#error "SceSledPlatformThread has not been implemented for this platform!"
#endif

/* ---------------------------------------------------------------------------------------------- */

#ifdef __cplusplus
extern "C" {
#endif

/* These will be defined by platform-specific headers:
#define SCE_SLEDPLATFORM_THREAD_NAME_MAX               Storage size used for thread name.
#define SCE_SLEDPLATFORM_THREAD_PRIORITY_INCREASE      Delta (+1 or -1) used to increase priority by one step.
#define SCE_SLEDPLATFORM_THREAD_PRIORITY_DECREASE      Delta (-1 or +1) used to decrease priority by one step. */

/* Thread struct is platform-specific */
typedef struct SceSledPlatformThread SceSledPlatformThread;

/* Thread entrypoint */
typedef void (*SceSledPlatformThreadEntry)(void *);

/* Thread attributes */
struct SceSledPlatformThreadAttr
{
    size_t         stackSize;
    intptr_t       affinity;
    int            priority;
    int            flags;         /* platform-specific thread flags (e.g. NOUSE_VFP) */
    char           name[SCE_SLEDPLATFORM_THREAD_NAME_MAX];
};
typedef struct SceSledPlatformThreadAttr SceSledPlatformThreadAttr;
#define SCE_SLEDPLATFORM_THREAD_NAME_DEFAULT            "SceSledPlatformThread"
#define SCE_SLEDPLATFORM_THREADATTR_INITIALIZER         { 0, 0, 0, 0, "" }

extern SCE_SLEDPLATFORM_LINKAGE const SceSledPlatformThreadID SCE_SLEDPLATFORM_INVALID_THREADID;

extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformThreadInvalidate(SceSledPlatformThread *pThread);
extern SCE_SLEDPLATFORM_LINKAGE int sceSledPlatformThreadCreateWithError(SceSledPlatformThread *pThread, SceSledPlatformThreadAttr *pAttr, SceSledPlatformThreadEntry pEntryPoint, void *pParameter);
extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformThreadCreate(SceSledPlatformThread *pThread, SceSledPlatformThreadAttr *pAttr, SceSledPlatformThreadEntry pEntryPoint, void *pParameter);
extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformThreadJoin(SceSledPlatformThread *pThread);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformThreadID sceSledPlatformThreadGetID(const SceSledPlatformThread *pThread);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformThreadID sceSledPlatformThreadGetCurrentThreadID();
extern SCE_SLEDPLATFORM_LINKAGE bool sceSledPlatformThreadIDsEqual(SceSledPlatformThreadID a, SceSledPlatformThreadID b);
extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformThreadSetPriority(SceSledPlatformThreadID id, int priority);
extern SCE_SLEDPLATFORM_LINKAGE int sceSledPlatformThreadGetPriority(SceSledPlatformThreadID id);
extern SCE_SLEDPLATFORM_LINKAGE int sceSledPlatformThreadGetCurrentThreadPriority();
extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformThreadSetAffinity(SceSledPlatformThreadID id, intptr_t affinity);
extern SCE_SLEDPLATFORM_LINKAGE intptr_t sceSledPlatformThreadGetAffinity(SceSledPlatformThreadID id);
extern SCE_SLEDPLATFORM_LINKAGE intptr_t sceSledPlatformThreadGetCurrentThreadAffinity();

#ifdef __cplusplus
}
#endif

/* ---------------------------------------------------------------------------------------------- */

#ifdef __cplusplus

#include "memory.h"
#include "sleep.h"

namespace sce {
namespace SledPlatform {

/* Thread class */
class SCE_SLEDPLATFORM_LINKAGE Thread
{
public:
    SCE_SLEDALLOCATOR_NEW_AND_DELETE(Thread,"sce::SledPlatform::Thread")
    SCE_SLEDPLACEMENT_NEW_AND_DELETE
    SCE_SLEDDISABLE_STANDARD_DELETE
    
    typedef SceSledPlatformThreadAttr  Attr;
    typedef SceSledPlatformThreadID    ID;
    typedef SceSledPlatformThreadEntry Entry;
    static const ID kInvalidID;
    
    Thread();
    Thread(Thread::Attr *pAttr);
    Thread(const char *name, int priority, size_t stackSize);
    Thread(const char *name, int priority, size_t stackSize, intptr_t affinity, int flags);
    virtual ~Thread();
    
    void SetAttributes(const Thread::Attr *pAttr);
    void SetAttributes(size_t stackSize, int priority, const char *pName);
    void SetAttributes(size_t stackSize, intptr_t affinity, int priority, int flags, const char *pName);
    const Thread::Attr * GetAttributes() const { return &m_attr; }
    const char * GetName() const               { return m_attr.name; }
    
    bool IsStarted() const                     { return m_started; }
    void Start(Thread::Entry pEntryPoint, void *pParameter);
    
    bool IsDone() const                        { return m_done; }
    void Join();
    
    Thread::ID GetID() const                   { return sceSledPlatformThreadGetID(&m_thread); }
    operator Thread::ID () const               { return sceSledPlatformThreadGetID(&m_thread); }
    static Thread::ID CurrentThreadID()        { return sceSledPlatformThreadGetCurrentThreadID(); }
    bool IsCurrentThread() const;
    
    void SetPriority(int priority)             { return sceSledPlatformThreadSetPriority(sceSledPlatformThreadGetID(&m_thread),priority); }
    int GetPriority() const                    { return sceSledPlatformThreadGetPriority(sceSledPlatformThreadGetID(&m_thread)); }
    static int CurrentThreadPriority()         { return sceSledPlatformThreadGetCurrentThreadPriority(); }
    
    void SetAffinity(intptr_t affinity)        { return sceSledPlatformThreadSetAffinity(sceSledPlatformThreadGetID(&m_thread),affinity); }
    intptr_t GetAffinity() const               { return sceSledPlatformThreadGetAffinity(sceSledPlatformThreadGetID(&m_thread)); }
    static intptr_t CurrentThreadAffinity()    { return sceSledPlatformThreadGetCurrentThreadAffinity(); }
    
	static void SleepFor(SceSledPlatformTimeInterval interval) { sceSledPlatformThreadSleepFor(interval); }
	static void SleepUntil(SceSledPlatformTime time)           { sceSledPlatformThreadSleepUntil(time); }
	static void SleepMilliseconds(int64_t ms)      { sceSledPlatformThreadSleepMilliseconds(ms); }
	
protected:
    SceSledPlatformThread     m_thread;
    Thread::Attr  m_attr;
    Thread::Entry m_pEntryPoint;
    void *        m_pParameter;
    uint8_t       m_started : 1;
    uint8_t       m_done : 1;
    uint8_t       m_reserved : 6;
    
    static void EntryGlue(void *);
    void AssertInvariants() const;
    
private:
    Thread(const Thread &) {}                       // prevent copying
    Thread & operator= (Thread &) { return *this; } // prevent assignment
};

inline bool operator==(const Thread &a, const Thread &b)         { return (&a == &b); }
inline bool operator==(const Thread &a, const Thread::ID &b)     { return sceSledPlatformThreadIDsEqual(a, b); }
inline bool operator==(const Thread::ID &a, const Thread &b)     { return sceSledPlatformThreadIDsEqual(a, b); }

inline bool operator!=(const Thread &a, const Thread &b)         { return (&a != &b); }
inline bool operator!=(const Thread &a, const Thread::ID &b)     { return !sceSledPlatformThreadIDsEqual(a, b); }
inline bool operator!=(const Thread::ID &a, const Thread &b)     { return !sceSledPlatformThreadIDsEqual(a, b); }


} /* namespace SledPlatform */
} /* namespace sce */

#endif /* __cplusplus */

#endif  /* SCE_SLEDPLATFORM_THREAD_H */
