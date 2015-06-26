/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "stdafx.h"
#include "LuaInterface.h"

#ifndef _MANAGED
extern "C"
{
#endif

#include <wws_lua\lua-5.1.4\src\lua.h>
#include <wws_lua\lua-5.1.4\src\lualib.h>
#include <wws_lua\lua-5.1.4\src\lauxlib.h>

#ifndef _MANAGED
}
#endif

lua_State* LuaInterface::Open()
{
	return ::lua_open();
}

void LuaInterface::OpenLibs(lua_State* luaState)
{
	::luaL_openlibs(luaState);
}

void LuaInterface::Close(lua_State* luaState)
{
	::lua_close(luaState);
}

int LuaInterface::LoadBuffer(lua_State* luaState, const char* buffer, std::size_t length, const char* name)
{
	return ::luaL_loadbuffer(luaState, buffer, length, name);
}

int LuaInterface::LoadFile(lua_State* luaState, const char* filename)
{
	return ::luaL_loadfile(luaState, filename);
}

int LuaInterface::GetTop(lua_State* luaState)
{
	return ::lua_gettop(luaState);
}

void LuaInterface::GetGlobal(lua_State* luaState, const char* name)
{
	return ::lua_getglobal(luaState, name);
}

void LuaInterface::SetGlobal(lua_State* luaState, const char* name)
{
	::lua_setglobal(luaState, name);
}

const char* LuaInterface::ToString(lua_State* luaState, int index)
{
	return ::lua_tostring(luaState, index);
}

void LuaInterface::PushString(lua_State* luaState, const char* string)
{
	::lua_pushstring(luaState, string);
}

double LuaInterface::ToNumber(lua_State* luaState, int index)
{
	return (double)::lua_tonumber(luaState, index);
}

void LuaInterface::PushNumber(lua_State* luaState, double number)
{
	::lua_pushnumber(luaState, (lua_Number)number);
}

void LuaInterface::PushNil(lua_State* luaState)
{
	::lua_pushnil(luaState);
}

void* LuaInterface::ToUserdata(lua_State* luaState, int index)
{
	return ::lua_touserdata(luaState, index);
}

void LuaInterface::PushLightUserdata(lua_State* luaState, void* userdata)
{
	::lua_pushlightuserdata(luaState, userdata);
}

void LuaInterface::Pop(lua_State* luaState, int count)
{
	::lua_pop(luaState, count);
}

lua_State* LuaInterface::NewThread(lua_State* luaState)
{
	return ::lua_newthread(luaState);
}

void LuaInterface::NewTable(lua_State* luaState)
{
	::lua_newtable(luaState);
}

void LuaInterface::SetTable(lua_State* luaState, int index)
{
	::lua_settable(luaState, index);
}

void LuaInterface::GetField(lua_State* luaState, int index, const char* key)
{
	::lua_getfield(luaState, index, key);
}

void LuaInterface::SetField(lua_State* luaState, int index, const char* key)
{
	::lua_setfield(luaState, index, key);
}

bool LuaInterface::IsNil(lua_State* luaState, int index)
{
	return lua_isnil(luaState, index);
}

bool LuaInterface::IsTable(lua_State* luaState, int index)
{
	return lua_istable(luaState, index);
}

int LuaInterface::IsString(lua_State* luaState, int index)
{
	return lua_isstring(luaState, index);
}

int LuaInterface::IsCFunction(lua_State* luaState, int index)
{
	return ::lua_iscfunction(luaState, index);
}

bool LuaInterface::IsLightUserdata(lua_State* luaState, int index)
{
	return lua_islightuserdata(luaState, index);
}

LuaInterface::Errors::Enum LuaInterface::TranslateLuaError(int luaError)
{
	/* LuaInterface::Errors::ErrGcMm not used in Lua 5.1.4 */

	switch (luaError)
	{
		case 0: return LuaInterface::Errors::Ok;
		case 1: return LuaInterface::Errors::Yield;
		case 2: return LuaInterface::Errors::ErrRun;
		case 3: return LuaInterface::Errors::ErrSyntax;
		case 4: return LuaInterface::Errors::ErrMem;
		case 5: return LuaInterface::Errors::ErrErr;			
		default: return LuaInterface::Errors::ErrUnknown;
	}
}

int LuaInterface::DumpEx(lua_State* luaState, LuaInterface::Callbacks::LuaWriter writer, void* data, int strip, const LuaInterface::DumpConfig& config)
{
	LuaDumpConfig ldc;
	ldc.endianness = config.endianness;
	ldc.sizeof_int = config.sizeof_int;
	ldc.sizeof_lua_Number = config.sizeof_lua_Number;
	ldc.sizeof_size_t = config.sizeof_size_t;

	return ::lua_dumpEx(luaState, writer, data, strip, &ldc);
}
