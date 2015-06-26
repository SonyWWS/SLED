/*
** $Id: lundump.h,v 1.37.1.1 2007/12/27 13:02:25 roberto Exp $
** load precompiled Lua chunks
** See Copyright Notice in lua.h
*/

#ifndef lundump_h
#define lundump_h

#include "lobject.h"
#include "lzio.h"

/* load one chunk; from lundump.c */
LUAI_FUNC Proto* luaU_undump (lua_State* L, ZIO* Z, Mbuffer* buff, const char* name);

/* make header; from lundump.c */
LUAI_FUNC void luaU_header (char* h, LuaDumpConfig *config);

/* dump one chunk; from ldump.c */
LUAI_FUNC int luaU_dump (lua_State* L, const Proto* f, lua_Writer w, void* data, int strip);

/* dump one chunk; from ldump.c */
LUAI_FUNC int luaU_dumpEx (lua_State *L, const Proto *f, lua_Writer w, void *data, int strip, LuaDumpConfig *config);

#ifdef luac_c
/* print one chunk; from print.c */
LUAI_FUNC void luaU_print (const Proto* f, int full);
#endif

/* for header of binary files -- this is Lua 5.1 */
#define LUAC_VERSION		0x51

/* for header of binary files -- this is the official format */
#define LUAC_FORMAT		0

/* size of header of binary files */
#define LUAC_HEADERSIZE		12

#define WWS_LUA_LITTLE_ENDIAN	0x03020100u
#define WWS_LUA_BIG_ENDIAN		0x00010203u

/* will return WWS_LUA_LITTLE_ENDIAN or WWS_LUA_BIG_ENDIAN */
LUAI_FUNC int luaU_getHostOrder();

#endif
