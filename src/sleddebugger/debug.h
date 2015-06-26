/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_DEBUG_H__
#define __SCE_LIBSLEDDEBUGGER_DEBUG_H__

#include "../sledcore/target_macros.h"

// #define ENABLE_DEBUG_PRINTF

#ifdef ENABLE_DEBUG_PRINTF
	#include <cstdio>

	#define __STR(name) #name
	#define __STR_LINE(line) __STR(line)
	#define __FILELINE __FILE__":"__STR_LINE(__LINE__)": "

	#define DPRINTF(...) \
		SCE_SLEDMACRO_BEGIN \
		std::printf(__FILELINE __VA_ARGS__); \
		SCE_SLEDMACRO_END
#else
	#define DPRINTF(...) \
		SCE_SLEDMACRO_BEGIN \
		SCE_SLEDMACRO_END
#endif /* ENABLE_DEBUG_PRINTF */

#endif // __SCE_LIBSLEDDEBUGGER_DEBUG_H__
