/*
** $Id: ldump.c,v 2.17.1.1 2013/04/12 18:48:47 roberto Exp $
** save precompiled Lua chunks
** See Copyright Notice in lua.h
*/

#include <stddef.h>
#include <assert.h>
#include <stdlib.h>
#include <string.h>

#define ldump_c
#define LUA_CORE

#include "lua.h"

#include "lobject.h"
#include "lstate.h"
#include "lundump.h"

#include "../../../src/sledcore/base_types.h"

typedef struct {
 lua_State* L;
 lua_Writer writer;
 void* data;
 int strip;
 int status;
 LuaDumpConfig *config;
} DumpState;

#define DumpMem(b,n,size,D)	DumpBlock(b,(n)*(size),D)
#define DumpVar(x,D)		DumpMem(&x,1,sizeof(x),D)

static void DumpBlock(const void* b, size_t size, DumpState* D)
{
 if (D->status==0)
 {
  lua_unlock(D->L);
  D->status=(*D->writer)(D->L,b,size,D->data);
  lua_lock(D->L);
 }
}

static void EndianSwapScalar(void* dst_, const void* src_, size_t n)
{
	char* dst = (char*)dst_;
	const char* src = (const char*)src_;

	size_t i = 0;
	size_t j = n - 1;

	lua_assert(src_ != dst_);
	lua_assert((n > 0) && ((n & (n - 1)) == 0));

	for (i = 0; i < n; ++i, --j)
		dst[j] = src[i];
}

static void EndianSwapArray(void* dst_, const void* src_, size_t n, size_t size)
{
	char* dst = (char*)dst_;
	const char* src = (const char*)src_;

	size_t i;

	lua_assert(src_ != dst_);

	for (i = 0; i < n; ++i)
		EndianSwapScalar(&dst[i * size], &src[i * size], size);
}

static void DumpChar(int y, DumpState* D)
{
 char x=(char)y;
 DumpMem(&x, 1, sizeof(x), D);
}

static const union
{ 
	unsigned char bytes[4];
	unsigned int value;
} s_hostOrder = { { 0, 1, 2, 3 } };

#define WWS_LUA_HOST_ORDER (s_hostOrder.value)

static int EndianSwapRequired(int dumpFormatIsLittleEndian)
{
	const int dumpFormatIsBigEndian = !dumpFormatIsLittleEndian;

	return (dumpFormatIsLittleEndian && (WWS_LUA_HOST_ORDER == WWS_LUA_BIG_ENDIAN)) || 
		   (dumpFormatIsBigEndian && (WWS_LUA_HOST_ORDER == WWS_LUA_LITTLE_ENDIAN));
}

static void DumpInt(int x, DumpState* D)
{
	const int srcSize = sizeof(x);
	const int dstSize = D->config->sizeof_int;
	const int endianSwap = EndianSwapRequired(D->config->endianness);

	lua_assert((dstSize == 4) || (dstSize == 8));
	lua_assert(dstSize >= srcSize);

	if (dstSize > srcSize)
	{
		const int64_t x64 = x;
		int64_t out = x64;

		if (endianSwap)
			EndianSwapScalar(&out, &x64, sizeof(out));

		DumpMem(&out, 1, sizeof(out), D);
	}
	else
	{
		int out = x;

		if (endianSwap)
			EndianSwapScalar(&out, &x, sizeof(out));

		DumpMem(&out, 1, sizeof(out), D);
	}

	// Original Lua implementation:
	// DumpVar(x, D);
}

static void DumpSizeT(size_t x, DumpState* D)
{
	const int srcSize = sizeof(size_t);
	const int dstSize = D->config->sizeof_size_t;
	const int endianSwap = EndianSwapRequired(D->config->endianness);

	lua_assert((dstSize == 4) || (dstSize == 8));
	lua_assert(dstSize >= srcSize);

	if (dstSize > srcSize)
	{
		const uint64_t x64 = x;
		uint64_t out = x64;

		if (endianSwap)
			EndianSwapScalar(&out, &x64, sizeof(out));

		DumpMem(&out, 1, sizeof(out), D);
	}
	else
	{
		size_t out = x;

		if (endianSwap)
			EndianSwapScalar(&out, &x, sizeof(out));

		DumpMem(&out, 1, sizeof(out), D);
	}
}

static void DumpNumber(lua_Number x, DumpState* D)
{
	const int srcSize = sizeof(x);
	const int dstSize = D->config->sizeof_lua_Number;
	const int endianSwap = EndianSwapRequired(D->config->endianness);

	lua_assert((dstSize == 4) || (dstSize == 8));
	lua_assert(dstSize >= srcSize);

	if (dstSize > srcSize)
	{
		const double x64 = x;
		double out = x64;

		if (endianSwap)
			EndianSwapScalar(&out, &x64, sizeof(out));

		DumpMem(&out, 1, sizeof(out), D);
	}
	else
	{
		lua_Number out = x;

		if (endianSwap)
			EndianSwapScalar(&out, &x, sizeof(out));

		DumpMem(&out, 1, sizeof(out), D);
	}

	// Original Lua implmenetation:
	// DumpVar(x, D);
}

static void DumpVector(const void* b, int n, size_t size, DumpState* D)
{
	const int endianSwap = EndianSwapRequired(D->config->endianness);

	DumpInt(n, D);

	if (endianSwap)
	{
		void* out = malloc(n * size);

		EndianSwapArray(out, b, n, size);
		DumpMem(out, n, size, D);

		free(out);
	}
	else
		DumpMem(b, n, size, D);

	// Original Lua implementation
	// DumpInt(n, D);
	// DumpMem(b, n, size, D);
}

static void DumpString(const TString* s, DumpState* D)
{
 if (s==NULL)
 {
  size_t size=0;
  //DumpVar(size,D);
  DumpSizeT(size, D);
 }
 else
 {
  size_t size=s->tsv.len+1;		/* include trailing '\0' */
  //DumpVar(size,D);
  DumpSizeT(size, D);
  DumpBlock(getstr(s),size*sizeof(char),D);
 }
}

#define DumpCode(f,D)	 DumpVector(f->code,f->sizecode,sizeof(Instruction),D)

static void DumpFunction(const Proto* f, DumpState* D);

static void DumpConstants(const Proto* f, DumpState* D)
{
 int i,n=f->sizek;
 DumpInt(n,D);
 for (i=0; i<n; i++)
 {
  const TValue* o=&f->k[i];
  DumpChar(ttypenv(o),D);
  switch (ttypenv(o))
  {
   case LUA_TNIL:
	break;
   case LUA_TBOOLEAN:
	DumpChar(bvalue(o),D);
	break;
   case LUA_TNUMBER:
	DumpNumber(nvalue(o),D);
	break;
   case LUA_TSTRING:
	DumpString(rawtsvalue(o),D);
	break;
    default: lua_assert(0);
  }
 }
 n=f->sizep;
 DumpInt(n,D);
 for (i=0; i<n; i++) DumpFunction(f->p[i],D);
}

static void DumpUpvalues(const Proto* f, DumpState* D)
{
 int i,n=f->sizeupvalues;
 DumpInt(n,D);
 for (i=0; i<n; i++)
 {
  DumpChar(f->upvalues[i].instack,D);
  DumpChar(f->upvalues[i].idx,D);
 }
}

static void DumpDebug(const Proto* f, DumpState* D)
{
 int i,n;
 DumpString((D->strip) ? NULL : f->source,D);
 n= (D->strip) ? 0 : f->sizelineinfo;
 DumpVector(f->lineinfo,n,sizeof(int),D);
 n= (D->strip) ? 0 : f->sizelocvars;
 DumpInt(n,D);
 for (i=0; i<n; i++)
 {
  DumpString(f->locvars[i].varname,D);
  DumpInt(f->locvars[i].startpc,D);
  DumpInt(f->locvars[i].endpc,D);
 }
 n= (D->strip) ? 0 : f->sizeupvalues;
 DumpInt(n,D);
 for (i=0; i<n; i++) DumpString(f->upvalues[i].name,D);
}

static void DumpFunction(const Proto* f, DumpState* D)
{
 DumpInt(f->linedefined,D);
 DumpInt(f->lastlinedefined,D);
 DumpChar(f->numparams,D);
 DumpChar(f->is_vararg,D);
 DumpChar(f->maxstacksize,D);
 DumpCode(f,D);
 DumpConstants(f,D);
 DumpUpvalues(f,D);
 DumpDebug(f,D);
}

static void DumpHeader(DumpState* D)
{
 lu_byte h[LUAC_HEADERSIZE];
 luaU_header(h, D->config);
 DumpBlock(h,LUAC_HEADERSIZE,D);
}

/*
** dump Lua function as precompiled chunk
*/
int luaU_dump (lua_State* L, const Proto* f, lua_Writer w, void* data, int strip)
{
 LuaDumpConfig config;
 DumpState D;
 D.L=L;
 D.writer=w;
 D.data=data;
 D.strip=strip;
 D.status=0;

 // Dump functions are expecting this config 
 config.endianness = (luaU_getHostOrder() == WWS_LUA_LITTLE_ENDIAN) ? 1 : 0;
 config.sizeof_int = sizeof(int);
 config.sizeof_size_t = sizeof(size_t);
 config.sizeof_lua_Number = sizeof(lua_Number);
 D.config = &config;

 DumpHeader(&D);
 DumpFunction(f,&D);
 return D.status;
}

/*
** dump Lua function as precompiled chunk
*/

int luaU_dumpEx (lua_State *L, const Proto *f, lua_Writer w, void *data, int strip, LuaDumpConfig *config)
{
	DumpState D;
	D.L = L;
	D.writer = w;
	D.data = data;
	D.strip = strip;
	D.status = 0;
	D.config = config;
	DumpHeader(&D);
	DumpFunction(f, &D);

	return D.status;
}

int luaU_getHostOrder()
{
	return WWS_LUA_HOST_ORDER;
}
