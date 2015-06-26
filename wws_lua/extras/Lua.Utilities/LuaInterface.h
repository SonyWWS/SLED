/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __LUA_UTILITIES_LUAINTERFACE_H__
#define __LUA_UTILITIES_LUAINTERFACE_H__

#pragma once

#include <cstddef>
#include "lparser_callbacks.h"

// Forward declaration
struct lua_State;

namespace LuaInterface
{
	lua_State* Open();
	void OpenLibs(lua_State* luaState);
	void Close(lua_State* luaState);

	int LoadBuffer(lua_State* luaState, const char* buffer, std::size_t length, const char* name);
	int LoadFile(lua_State* luaState, const char* filename);	

	int GetTop(lua_State* luaState);

	void GetGlobal(lua_State* luaState, const char* name);
	void SetGlobal(lua_State* luaState, const char* name);

	const char* ToString(lua_State* luaState, int index);
	void PushString(lua_State* luaState, const char* string);
	double ToNumber(lua_State* luaState, int index);
	void PushNumber(lua_State* luaState, double number);	
	void PushNil(lua_State* luaState);
	void* ToUserdata(lua_State* luaState, int index);
	void PushLightUserdata(lua_State* luaState, void* userdata);
	void Pop(lua_State* luaState, int count);

	lua_State* NewThread(lua_State* luaState);
	void NewTable(lua_State* luaState);
	void SetTable(lua_State* luaState, int index);
	void GetField(lua_State* luaState, int index, const char* key);
	void SetField(lua_State* luaState, int index, const char* key);

	bool IsNil(lua_State* luaState, int index);
	bool IsTable(lua_State* luaState, int index);
	int IsString(lua_State* luaState, int index);
	int IsCFunction(lua_State* luaState, int index);
	bool IsLightUserdata(lua_State* luaState, int index);	

	namespace Errors
	{
		enum Enum
		{
			Ok,
			Yield,
			ErrRun,
			ErrSyntax,
			ErrMem,
			ErrGcMm,
			ErrErr,
			ErrOutputFile,
			ErrUnknown,
		};
	}

	Errors::Enum TranslateLuaError(int luaError);

	namespace Callbacks
	{
		/* must match Lua's lua_Writer signature */
		typedef int (*LuaWriter)(lua_State* luaState, const void* ptr, std::size_t sz, void* userdata);
	}

	struct DumpConfig
	{
		int endianness;			/* 1 = little endian, 0 = big endian */
		int sizeof_int;			/* size of int */
		int sizeof_size_t;		/* size of size_t */
		int sizeof_lua_Number;	/* size of lua_Number */
	};

	int DumpEx(lua_State* luaState, Callbacks::LuaWriter writer, void* data, int strip, const DumpConfig& config);
}

#endif // __LUA_UTILITIES_LUAINTERFACE_H__
