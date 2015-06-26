/*
** $Id: lundump.h,v 1.39.1.1 2013/04/12 18:48:47 roberto Exp $
** load precompiled Lua chunks
** See Copyright Notice in lua.h
*/

#ifndef lundump_h
#define lundump_h

#include "lobject.h"
#include "lzio.h"

/* load one chunk; from lundump.c */
LUAI_FUNC Closure* luaU_undump (lua_State* L, ZIO* Z, Mbuffer* buff, const char* name);

/* make header; from lundump.c */
LUAI_FUNC void luaU_header (lu_byte* h, LuaDumpConfig *config);

/* dump one chunk; from ldump.c */
LUAI_FUNC int luaU_dump (lua_State* L, const Proto* f, lua_Writer w, void* data, int strip);

/* dump one chunk; from ldump.c */
LUAI_FUNC int luaU_dumpEx (lua_State *L, const Proto *f, lua_Writer w, void *data, int strip, LuaDumpConfig *config);

/* data to catch conversion errors */
#define LUAC_TAIL		"\x19\x93\r\n\x1a\n"

/* size in bytes of header of binary files */
#define LUAC_HEADERSIZE		(sizeof(LUA_SIGNATURE)-sizeof(char)+2+6+sizeof(LUAC_TAIL)-sizeof(char))

#define WWS_LUA_LITTLE_ENDIAN	0x03020100u
#define WWS_LUA_BIG_ENDIAN		0x00010203u

/* will return WWS_LUA_LITTLE_ENDIAN or WWS_LUA_BIG_ENDIAN */
LUAI_FUNC int luaU_getHostOrder();

#endif
