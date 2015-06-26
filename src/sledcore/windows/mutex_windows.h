/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef _SCE_SLEDPLATFORM_MUTEX_WINDOWS_H
#define _SCE_SLEDPLATFORM_MUTEX_WINDOWS_H

#include "../thread.h"

// Prevent windows.h including winsock.h which would later break winsock2.h
#pragma push_macro("_WINSOCKAPI_")
#define _WINSOCKAPI_
#include <windows.h>
#pragma pop_macro("_WINSOCKAPI_")

#define SCE_SLEDPLATFORM_INVALID_MUTEX NULL

#define USE_FAST_SYNCHRONIZATION 0

#if USE_FAST_SYNCHRONIZATION

typedef struct SceSledPlatformMutex {
	SceSledPlatformThreadID owner;
	int lockCount;
	CRITICAL_SECTION criticalSection;
} SceSledPlatformMutex;

#define SCE_SLEDPLATFORM_MUTEX_INITIALIZER { SCE_SLEDPLATFORM_THREADID_INITIALIZER, -1 }

#else

typedef struct SceSledPlatformMutex {
	HANDLE mutex;
	SceSledPlatformThreadID owner;
	int lockCount;
} SceSledPlatformMutex;
#define SCE_SLEDPLATFORM_MUTEX_INITIALIZER { SCE_SLEDPLATFORM_INVALID_MUTEX, SCE_SLEDPLATFORM_THREADID_INITIALIZER, -1 }

#endif

#endif /* _SCE_SLEDPLATFORM_MUTEX_WINDOWS_H */
