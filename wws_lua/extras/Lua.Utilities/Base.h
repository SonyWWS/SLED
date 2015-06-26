/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#pragma once

#if WWS_LUA_VER >= 520
	#define LUA_VER_NAMESPACE_OPEN namespace Lua52 {
#elif WWS_LUA_VER >= 510
	#define LUA_VER_NAMESPACE_OPEN namespace Lua51 {
#else
	#error Unknown Lua version!
#endif

#define LUA_VER_NAMESPACE_CLOSE }

#if defined(WIN64)
	#define ARCH_NAMESPACE_OPEN namespace x64 {
#else
	#define ARCH_NAMESPACE_OPEN namespace x86 {
#endif

#define ARCH_NAMESPACE_CLOSE }
