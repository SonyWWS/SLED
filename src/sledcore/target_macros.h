/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef SCE_SLEDTARGETMACROS_H
#define SCE_SLEDTARGETMACROS_H

#undef SCE_SLEDHOST_COMPILER_MSVC
    #undef SCE_SLEDHOST_COMPILER_MSVC80
    #undef SCE_SLEDHOST_COMPILER_MSVC90
    #undef SCE_SLEDHOST_COMPILER_MSVC100
    #undef SCE_SLEDHOST_COMPILER_MSVC110
    #undef SCE_SLEDHOST_COMPILER_MSVC120

#undef SCE_SLEDALIGNOF
#undef SCE_SLEDALIGN_BEG
#undef SCE_SLEDALIGN_END
#undef SCE_SLEDWEAK_VAR_BEG
#undef SCE_SLEDWEAK_VAR_END
#undef SCE_SLEDDEPRECATED_BEG
#undef SCE_SLEDDEPRECATED_END
#undef SCE_SLEDEXPECT
#undef SCE_SLEDUNLIKELY
#undef SCE_SLEDLIKELY
#undef SCE_SLEDIMPORT
#undef SCE_SLEDEXPORT
#undef SCE_SLEDUNUSED
#undef SCE_SLEDBREAK
#undef SCE_SLEDSTOP
#undef SCE_SLEDNORETURN_STOP
#undef SCE_SLEDFUNCTION_NAME

#undef SCE_SLEDTARGET_CPU_X86
    #undef SCE_SLEDTARGET_CPU_X86_IA32
    #undef SCE_SLEDTARGET_CPU_X86_X64

#undef SCE_SLEDTARGET_RT_LITTLE_ENDIAN
#undef SCE_SLEDTARGET_RT_BIG_ENDIAN

#undef SCE_SLEDTARGET_RT_PTR_SIZE_32
#undef SCE_SLEDTARGET_RT_PTR_SIZE_64
 
#undef SCE_SLEDTARGET_CAPS_INT64
    #undef SCE_SLEDTARGET_CAPS_INT64_HW
    #undef SCE_SLEDTARGET_CAPS_INT64_SW
#undef SCE_SLEDTARGET_CAPS_INT128
    #undef SCE_SLEDTARGET_CAPS_INT128_HW
    #undef SCE_SLEDTARGET_CAPS_INT128_SW
#undef SCE_SLEDTARGET_CAPS_F16
    #undef SCE_SLEDTARGET_CAPS_F16_HW
    #undef SCE_SLEDTARGET_CAPS_F16_SW
#undef SCE_SLEDTARGET_CAPS_F32
    #undef SCE_SLEDTARGET_CAPS_F32_HW
    #undef SCE_SLEDTARGET_CAPS_F32_SW
#undef SCE_SLEDTARGET_CAPS_F64
    #undef SCE_SLEDTARGET_CAPS_F64_HW
    #undef SCE_SLEDTARGET_CAPS_F64_SW
#undef SCE_SLEDTARGET_CAPS_F80
    #undef SCE_SLEDTARGET_CAPS_F80_HW
    #undef SCE_SLEDTARGET_CAPS_F80_SW

// these can be predefined
//#undef SCE_SLEDTARGET_LINK_DYNAMIC
//#undef SCE_SLEDTARGET_LINK_STATIC

#undef SCE_SLEDTARGET_OS_WINDOWS
    #undef SCE_SLEDTARGET_OS_WIN32
    #undef SCE_SLEDTARGET_OS_WIN64

//  These are common across most toolchain / platforms, possibly redefined below
#define SCE_SLEDSTRINGIFY(x)                            #x
#define SCE_SLEDTOSTRING(x)                             SCE_SLEDSTRINGIFY(x)
#define SCE_SLEDSUBSTRING_CONCATENATE(s1,s2)            s1##s2
#define SCE_SLEDSTRING_CONCATENATE(s1,s2)               SCE_SLEDSUBSTRING_CONCATENATE(s1,s2)

#define SCE_SLEDMACRO_BEGIN                             do {
#define SCE_SLEDMACRO_END                               } while(0)

#if defined(_MSC_VER)
    #define SCE_SLEDHOST_COMPILER_MSVC                  1
    #define SCE_SLEDHOST_COMPILER_MSVC80                (_MSC_VER >= 1400 && _MSC_VER < 1500)
    #define SCE_SLEDHOST_COMPILER_MSVC90                (_MSC_VER >= 1500 && _MSC_VER < 1600)
    #define SCE_SLEDHOST_COMPILER_MSVC100               (_MSC_VER >= 1600 && _MSC_VER < 1700)
    #define SCE_SLEDHOST_COMPILER_MSVC110               (_MSC_VER >= 1700 && _MSC_VER < 1800)
    #define SCE_SLEDHOST_COMPILER_MSVC120               (_MSC_VER >= 1800 && _MSC_VER < 1900)

    #if SCE_SLEDHOST_COMPILER_MSVC80
        #error No longer supports Visual Studio 2005.
    #endif

    #if (_MSC_VER < 1500) || (_MSC_VER >= 1900)
        #error Unknown / unsupported Visual Studio Version.
    #endif

    #define SCE_SLEDALIGNOF(typ)                        __alignof(typ)
    #define SCE_SLEDALIGN_BEG(x)                        __declspec( align(x) )
    #define SCE_SLEDALIGN_END(x) 
    #define SCE_SLEDWEAK_VAR_BEG                        __declspec(selectany)
    #define SCE_SLEDWEAK_VAR_END
    #define SCE_SLEDDEPRECATED_BEG(msg)                 __declspec(deprecated(msg))
    #define SCE_SLEDDEPRECATED_END(msg)
    #define SCE_SLEDEXPECT(expr, val)                   (expr)

    #define SCE_SLEDUNUSED(var)                         (void)(var)
    #define SCE_SLEDNOOP(...)                           __noop(__VA_ARGS__)
    #define SCE_SLEDBREAK()                             __debugbreak()
    #define SCE_SLEDSTOP()                              __debugbreak()
    #define SCE_SLEDNORETURN_STOP()                     __debugbreak()
    #define SCE_SLEDRESTRICT                            __restrict
    #define SCE_SLEDFUNCTION_NAME                       __FUNCTION__

    #ifdef __cplusplus
    extern "C" {
    #endif

    #ifdef  _WIN64
        void *          __cdecl _alloca(unsigned __int64 sz);
    #else
        void *          __cdecl _alloca(unsigned int sz);
    #endif

    #ifdef __cplusplus
    }
    #endif

    #define SCE_SLEDALLOCA(sz)                          _alloca(sz)

    #undef SCE_SLEDMACRO_END
    #define SCE_SLEDMACRO_END                           \
        __pragma(warning(push))                     \
        __pragma(warning(disable: 4127))            \
        } while (0)                                 \
        __pragma(warning(pop))

    #if defined(_M_IX86) || defined(_M_X64)
        #define SCE_SLEDTARGET_CPU_X86                  1
        #if defined(_M_IX86)
            #define SCE_SLEDTARGET_CPU_X86_IA32         1
            #define SCE_SLEDTARGET_CAPS_INT64_SW        1
        #elif defined(_M_X64)
            #define SCE_SLEDTARGET_CPU_X86_X64          1
            #define SCE_SLEDTARGET_CAPS_INT64_HW        1
        #endif
        #define SCE_SLEDTARGET_RT_LITTLE_ENDIAN         1
        #define SCE_SLEDTARGET_CAPS_INT64               1
        #define SCE_SLEDTARGET_CAPS_F32                 1
        #define SCE_SLEDTARGET_CAPS_F32_HW              1
        #define SCE_SLEDTARGET_CAPS_F64                 1
        #define SCE_SLEDTARGET_CAPS_F64_HW              1
        #define SCE_SLEDTARGET_CAPS_F80                 1
        #define SCE_SLEDTARGET_CAPS_F80_HW              1
    #else
        #error "Microsoft Visual C++ with unknown target cpu"
    #endif

    #if defined(_WIN32) || defined(_WIN64)
        #define SCE_SLEDTARGET_OS_WINDOWS               1
        #if defined(_WIN32)
            #define SCE_SLEDTARGET_OS_WIN32             1
        #endif
        #if defined(_WIN64)
            #define SCE_SLEDTARGET_OS_WIN64             1
        #endif
    #else
        #error "Microsoft Visual C++ with unknown target runtime"
    #endif

#endif

/*
 * defined always
 */

#define SCE_SLEDUNLIKELY(expr)                          SCE_SLEDEXPECT(expr,0)
#define SCE_SLEDLIKELY(expr)                            SCE_SLEDEXPECT(expr,1)

// another possible test would be to compare <stdint.h>'s PTRDIFF_MAX or INTPTR_MAX to INT32_MAX
#if defined(__LP64__) || defined(_WIN64) || defined(_LP64)
    #define SCE_SLEDTARGET_RT_PTR_SIZE_64               1
#else
    #define SCE_SLEDTARGET_RT_PTR_SIZE_64               0
#endif
#define SCE_SLEDTARGET_RT_PTR_SIZE_32                   (!SCE_SLEDTARGET_RT_PTR_SIZE_64)

// Make sure both SCE_SLEDTARGET_LINK_ macros are set to opposite states, defaulting to STATIC if not specified
#if defined(SCE_SLEDTARGET_LINK_DYNAMIC) && defined(SCE_SLEDTARGET_LINK_STATIC)
    #if SCE_SLEDTARGET_LINK_DYNAMIC && SCE_SLEDTARGET_LINK_STATIC
        #error SCE_SLEDTARGET_LINK_DYNAMIC and SCE_SLEDTARGET_LINK_STATIC are both set
    #elif !SCE_SLEDTARGET_LINK_DYNAMIC && !SCE_SLEDTARGET_LINK_STATIC
        #error SCE_SLEDTARGET_LINK_DYNAMIC and SCE_SLEDTARGET_LINK_STATIC are both zero
    #endif
#elif !defined(SCE_SLEDTARGET_LINK_DYNAMIC) && !defined(SCE_SLEDTARGET_LINK_STATIC)
    #define SCE_SLEDTARGET_LINK_DYNAMIC                 0
    #define SCE_SLEDTARGET_LINK_STATIC                  1
#elif defined(SCE_SLEDTARGET_LINK_DYNAMIC)
    #define SCE_SLEDTARGET_LINK_STATIC                  (!SCE_SLEDTARGET_LINK_DYNAMIC)
#else
    #define SCE_SLEDTARGET_LINK_DYNAMIC                 (!SCE_SLEDTARGET_LINK_STATIC)
#endif

// Define a macro that can be used safely for __declspec in all situations.
#if SCE_SLEDTARGET_LINK_DYNAMIC
    #define SCE_SLEDIMPORT                              __declspec(dllimport)
    #define SCE_SLEDEXPORT                              __declspec(dllexport)
#else
    #define SCE_SLEDIMPORT
    #define SCE_SLEDEXPORT
#endif

/*
 * define the remaining settings to 0 so they may be used in runtime logic
 */

#if !defined(SCE_SLEDHOST_COMPILER_MSVC)
    #define SCE_SLEDHOST_COMPILER_MSVC              0
#endif
    #if !defined(SCE_SLEDHOST_COMPILER_MSVC80)
        #define SCE_SLEDHOST_COMPILER_MSVC80        0
    #endif
    #if !defined(SCE_SLEDHOST_COMPILER_MSVC90)
        #define SCE_SLEDHOST_COMPILER_MSVC90        0
    #endif
    #if !defined(SCE_SLEDHOST_COMPILER_MSVC100)
        #define SCE_SLEDHOST_COMPILER_MSVC100       0
    #endif
    #if !defined(SCE_SLEDHOST_COMPILER_MSVC110)
        #define SCE_SLEDHOST_COMPILER_MSVC110       0
    #endif
    #if !defined(SCE_SLEDHOST_COMPILER_MSVC120)
        #define SCE_SLEDHOST_COMPILER_MSVC120       0
    #endif

#if !defined(SCE_SLEDTARGET_CPU_X86)
    #define SCE_SLEDTARGET_CPU_X86                  0
#endif
    #if !defined(SCE_SLEDTARGET_CPU_X86_IA32)
        #define SCE_SLEDTARGET_CPU_X86_IA32         0
    #endif
    #if !defined(SCE_SLEDTARGET_CPU_X86_X64)
        #define SCE_SLEDTARGET_CPU_X86_X64          0
    #endif
    
#if !defined(SCE_SLEDTARGET_RT_LITTLE_ENDIAN)
    #define SCE_SLEDTARGET_RT_LITTLE_ENDIAN         0
#endif
#if !defined(SCE_SLEDTARGET_RT_BIG_ENDIAN)
    #define SCE_SLEDTARGET_RT_BIG_ENDIAN            0
#endif

// defined always with logic above
//#if !defined(SCE_SLEDTARGET_RT_PTR_SIZE_32)
//  #define SCE_SLEDTARGET_RT_PTR_SIZE_32           0
//#endif
//#if !defined(SCE_SLEDTARGET_RT_PTR_SIZE_64)
//  #define SCE_SLEDTARGET_RT_PTR_SIZE_64           0
//#endif

#if !defined(SCE_SLEDTARGET_CAPS_INT64)
    #define SCE_SLEDTARGET_CAPS_INT64               0
#endif
    #if !defined(SCE_SLEDTARGET_CAPS_INT64_HW)
        #define SCE_SLEDTARGET_CAPS_INT64_HW        0
    #endif
    #if !defined(SCE_SLEDTARGET_CAPS_INT64_SW)
        #define SCE_SLEDTARGET_CAPS_INT64_SW        0
    #endif
#if !defined(SCE_SLEDTARGET_CAPS_INT128)
    #define SCE_SLEDTARGET_CAPS_INT128              0
#endif
    #if !defined(SCE_SLEDTARGET_CAPS_INT128_HW)
        #define SCE_SLEDTARGET_CAPS_INT128_HW       0
    #endif
    #if !defined(SCE_SLEDTARGET_CAPS_INT128_SW)
        #define SCE_SLEDTARGET_CAPS_INT128_SW       0
    #endif
#if !defined(SCE_SLEDTARGET_CAPS_F16)
    #define SCE_SLEDTARGET_CAPS_F16                 0
#endif
    #if !defined(SCE_SLEDTARGET_CAPS_F16_HW)
        #define SCE_SLEDTARGET_CAPS_F16_HW          0
    #endif
    #if !defined(SCE_SLEDTARGET_CAPS_F16_SW)
        #define SCE_SLEDTARGET_CAPS_F16_SW          0
    #endif
#if !defined(SCE_SLEDTARGET_CAPS_F32)
    #define SCE_SLEDTARGET_CAPS_F32                 0
#endif
    #if !defined(SCE_SLEDTARGET_CAPS_F32_HW)
        #define SCE_SLEDTARGET_CAPS_F32_HW          0
    #endif
    #if !defined(SCE_SLEDTARGET_CAPS_F32_SW)
        #define SCE_SLEDTARGET_CAPS_F32_SW          0
    #endif
#if !defined(SCE_SLEDTARGET_CAPS_F64)
    #define SCE_SLEDTARGET_CAPS_F64                 0
#endif
    #if !defined(SCE_SLEDTARGET_CAPS_F64_HW)
        #define SCE_SLEDTARGET_CAPS_F64_HW          0
    #endif
    #if !defined(SCE_SLEDTARGET_CAPS_F64_SW)
        #define SCE_SLEDTARGET_CAPS_F64_SW          0
    #endif
#if !defined(SCE_SLEDTARGET_CAPS_F80)
    #define SCE_SLEDTARGET_CAPS_F80                 0
#endif
    #if !defined(SCE_SLEDTARGET_CAPS_F80_HW)
        #define SCE_SLEDTARGET_CAPS_F80_HW          0
    #endif
    #if !defined(SCE_SLEDTARGET_CAPS_F80_SW)
        #define SCE_SLEDTARGET_CAPS_F80_SW          0
    #endif

// defined always with logic above
//#if !defined(SCE_SLEDTARGET_LINK_DYNAMIC)
//  #define SCE_SLEDTARGET_LINK_DYNAMIC             0
//#endif
//#if !defined(SCE_SLEDTARGET_LINK_STATIC)
//  #define SCE_SLEDTARGET_LINK_STATIC              0
//#endif

#if !defined(SCE_SLEDTARGET_OS_WINDOWS)
    #define SCE_SLEDTARGET_OS_WINDOWS               0
#endif
    #if !defined(SCE_SLEDTARGET_OS_WIN32)
        #define SCE_SLEDTARGET_OS_WIN32             0
    #endif
    #if !defined(SCE_SLEDTARGET_OS_WIN64)
        #define SCE_SLEDTARGET_OS_WIN64             0
    #endif

#endif	//SCE_SLEDTARGETMACROS_H
