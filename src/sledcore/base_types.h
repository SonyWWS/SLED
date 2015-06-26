/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef SCE_SLEDBASE_TYPES_H
#define SCE_SLEDBASE_TYPES_H

#include "target_macros.h"

#if !defined(SCE_SLEDCOMMON_NO_STDINT_DEFS)
#if SCE_SLEDHOST_COMPILER_MSVC && !SCE_SLEDHOST_COMPILER_MSVC100

	typedef signed char			int8_t;
	typedef signed short		int16_t;
	typedef signed int			int32_t;
	typedef signed long long	int64_t;
	typedef unsigned char		uint8_t;
	typedef unsigned short		uint16_t;
	typedef unsigned int		uint32_t;
	typedef unsigned long long	uint64_t;

    #ifndef _INTPTR_T_DEFINED
    #if SCE_SLEDTARGET_RT_PTR_SIZE_64
    typedef __int64             intptr_t;
    #else
    typedef __w64 int           intptr_t;
    #endif
    #define _INTPTR_T_DEFINED
    #endif

    #ifndef _UINTPTR_T_DEFINED
    #if SCE_SLEDTARGET_RT_PTR_SIZE_64
    typedef unsigned __int64    uintptr_t;
    #else
    typedef __w64 unsigned int  uintptr_t;
    #endif
    #define _UINTPTR_T_DEFINED
    #endif

#else /* #if !SCE_SLEDHOST_COMPILER_MSVC */

	#include    <stdint.h>
	#include    <stddef.h>

#endif /* #if SCE_SLEDHOST_COMPILER_MSVC */


#endif	//	#if !defined(SCE_SLEDCOMMON_NO_STDINT_DEFS)


#if !defined(SCE_SLEDCOMMON_NO_STDBOOL_DEFS)

#ifndef __cplusplus
    #if SCE_SLEDHOST_COMPILER_MSVC

        #if !SCE_SLEDHOST_COMPILER_MSVC120
        typedef unsigned int _Bool;
        #endif

        #define bool   _Bool
        #define false  0
        #define true   1

    #else

        #include <stdbool.h>

    #endif  /* #if SCE_SLEDHOST_COMPILER_MSVC */
#endif  /* #ifndef __cplusplus */

#endif	//	#if !defined(SCE_SLEDCOMMON_NO_STDBOOL_DEFS)

#endif  //  #ifndef SCE_SLEDBASE_TYPES_H
