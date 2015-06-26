/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef _SCE_SLEDPLATFORM_THREAD_WINDOWS_H
#define _SCE_SLEDPLATFORM_THREAD_WINDOWS_H


#define SCE_SLEDPLATFORM_THREAD_NAME_MAX             (32)
#define SCE_SLEDPLATFORM_THREADID_INITIALIZER        (0)
#define SCE_SLEDPLATFORM_THREAD_PRIORITY_INCREASE    (1)
#define SCE_SLEDPLATFORM_THREAD_PRIORITY_DECREASE    (-1)

struct SceSledPlatformThread
{
	void *thread;
    unsigned int id;
    void (*pEntryPoint)(void *);
    void *pParameter;
    char name[SCE_SLEDPLATFORM_THREAD_NAME_MAX];
};
#define SCE_SLEDPLATFORM_THREAD_INITIALIZER     { SCE_SLEDPLATFORM_THREADID_INITIALIZER, 0, 0, 0, "" }

typedef int32_t	SceSledPlatformThreadID;

// Caller must include <windows.h> before using this API
#define sceSledPlatformWindowsSetThreadNameInline(id,name) \
    SCE_SLEDMACRO_BEGIN                                                     \
        struct {                                                        \
            DWORD dwType; /* 0x1000 */                                  \
            LPCSTR szName; /* new name */                               \
            DWORD dwThreadID; /* thread ID (-1=calling thread) */       \
            DWORD dwFlags; /* reserved */                               \
        } info_;                                                        \
                                                                        \
        info_.dwType = 0x1000;                                          \
        info_.szName = name;                                            \
        info_.dwThreadID = id;                                          \
        info_.dwFlags = 0;                                              \
                                                                        \
        __try {                                                         \
            RaiseException( 0x406D1388, 0,                              \
                sizeof(info_)/sizeof(ULONG_PTR), (ULONG_PTR*)&info_ );  \
        } __except(EXCEPTION_CONTINUE_EXECUTION) {}                     \
    SCE_SLEDMACRO_END

// Caller must include <windows.h> before using this API
#define sceSledPlatformWindowsSetCurrentThreadNameInline(name) \
    sceSledPlatformWindowsSetThreadNameInline((DWORD)-1,name)

#endif /* _SCE_SLEDPLATFORM_THREAD_WINDOWS_H */

