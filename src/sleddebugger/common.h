/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_COMMON_H__
#define __SCE_LIBSLEDDEBUGGER_COMMON_H__

#include "../sledcore/base_types.h"

// If we are building this lib as a DLL, SCE_SLED_EXPORTS will be set.
#ifdef SCE_SLED_EXPORTS
	#define SCE_SLED_LINKAGE    SCE_SLEDEXPORT
#else
	#define SCE_SLED_LINKAGE    SCE_SLEDIMPORT
#endif

#endif // __SCE_LIBSLEDDEBUGGER_COMMON_H__
