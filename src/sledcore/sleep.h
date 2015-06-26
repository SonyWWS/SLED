/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef SCE_SLEDPLATFORM_SLEEP_H
#define SCE_SLEDPLATFORM_SLEEP_H

#include "datetime.h"

#ifdef __cplusplus
extern "C" {
#endif

extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformThreadSleepFor(SceSledPlatformTimeInterval interval);
extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformThreadSleepUntil(SceSledPlatformTime when);
extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformThreadSleepMilliseconds(int64_t ms);

#ifdef __cplusplus
}
#endif

#endif  /* SCE_SLEDPLATFORM_SLEEP_H */
