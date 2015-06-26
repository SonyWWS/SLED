/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __LUA_UTILITIES_LUAWRAPPER_H__
#define __LUA_UTILITIES_LUAWRAPPER_H__

#pragma once

#include "LuaInterface.h"

// Forward declaration
struct lua_State;

namespace Unmanaged
{

class StackReconciler
{
public:
	StackReconciler(lua_State* luaState);
	~StackReconciler();
private:
	StackReconciler(const StackReconciler&);
	StackReconciler& operator=(const StackReconciler&);
private:
	lua_State* m_luaState;
	const int m_count;
};

class LuaWrapper
{
public:
	LuaWrapper();
	~LuaWrapper();

	LuaInterface::Errors::Enum LoadFile(System::String^ scriptFile);
	LuaInterface::Errors::Enum LoadFile(System::String^ scriptFile,
										Sce::Lua::Utilities::ParserVariableDelegate^ globalCb,
										Sce::Lua::Utilities::ParserVariableDelegate^ localCb,
										Sce::Lua::Utilities::ParserVariableDelegate^ upvalueCb,
										Sce::Lua::Utilities::ParserFunctionDelegate^ funcCb,
										Sce::Lua::Utilities::ParserBreakpointDelegate^ bpCb,
										Sce::Lua::Utilities::ParserLogDelegate^ logCb);

	LuaInterface::Errors::Enum LoadBuffer(System::String^ scriptBuffer);
	LuaInterface::Errors::Enum Compile(System::String^ scriptAbsFilePath, System::String^ scriptAbsDumpFilePath, Sce::Lua::Utilities::LuaCompilerConfig^ config);	

private:
	static int Compile_Writer(lua_State* L, const void* pData, size_t size, void* pFile);

public:
	lua_State* m_luaState;

private:
	LuaWrapper(const LuaWrapper&);
	LuaWrapper& operator=(const LuaWrapper&);
};

}

#endif // __LUA_UTILITIES_LUAWRAPPER_H__
