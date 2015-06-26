/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef SCE_SLEDPLATFORM_COMMON_H
#define SCE_SLEDPLATFORM_COMMON_H

#include "base_types.h"
#include <stddef.h> // size_t, NULL, etc

// If we are building this lib as a DLL, SCE_SLEDPLATFORM_EXPORTS will be set.
#ifdef SCE_SLEDPLATFORM_EXPORTS
#define SCE_SLEDPLATFORM_LINKAGE    SCE_SLEDEXPORT
#else
#define SCE_SLEDPLATFORM_LINKAGE    SCE_SLEDIMPORT
#endif

// Whenever the platform code encounters an error, it's considered
// a programming problem and we always halt.
#define SCE_SLEDPLATFORM_ASSERT(test)               \
        SCE_SLEDMACRO_BEGIN                         \
        {                                       \
            if (SCE_SLEDUNLIKELY(!(test)))          \
                SCE_SLEDNORETURN_STOP();            \
        }                                       \
        SCE_SLEDMACRO_END

// Always inline when requested.
#if SCE_SLEDHOST_COMPILER_MSVC
	#define SCE_SLEDPLATFORM_INLINE __forceinline
#endif

// Simple rounding divide.
#define SCE_SLEDPLATFORM_DIVIDE_ROUNDING_UP(val,divisor)   (((val) + (divisor) - 1) / (divisor))

#undef SCE_SLEDPLATFORM_EXCEPTION_SUPPORTED
#if SCE_SLEDTARGET_OS_WINDOWS
#define SCE_SLEDPLATFORM_EXCEPTION_SUPPORTED 1
#endif
#ifndef SCE_SLEDPLATFORM_EXCEPTION_SUPPORTED
#define SCE_SLEDPLATFORM_EXCEPTION_SUPPORTED 0
#endif

#endif  /* SCE_SLEDPLATFORM_COMMON_H */
