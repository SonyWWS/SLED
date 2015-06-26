/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDLUAPLUGIN_ERRORCODES_H__
#define __SCE_LIBSLEDLUAPLUGIN_ERRORCODES_H__

#define SCE_SLED_LUA_ERROR_INVALIDCONFIGURATION			(int)(0x80831001)	///< Invalid configuration table; error code
#define SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE			(int)(0x80831002)	///< No debugger instance (not added to a SledDebugger instance); error code
#define SCE_SLED_LUA_ERROR_INVALIDLUASTATE				(int)(0x80831003)	///< Invalid Lua state; error code
#define SCE_SLED_LUA_ERROR_DUPLICATELUASTATE			(int)(0x80831004)	///< Already networking; error code
#define SCE_SLED_LUA_ERROR_OVERLUASTATELIMIT			(int)(0x80831005)	///< Plugin already added; error code
#define SCE_SLED_LUA_ERROR_LUASTATENOTFOUND				(int)(0x80831006)	///< Invalid plugin; error code
#define SCE_SLED_LUA_ERROR_LUASTATEALREADYREGISTERED	(int)(0x80831007)	///< Lua state already registered; error code

#endif // __SCE_LIBSLEDLUAPLUGIN_ERRORCODES_H__
