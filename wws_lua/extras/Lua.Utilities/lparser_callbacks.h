/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef lparser_callbacks_h
#define lparser_callbacks_h

#ifdef LUA_UTILITIES_NET

#define LUA_PARSER_DEBUG_TABLE		"WwsLuaParserTable"
#define LUA_PARSER_CB_VAR_GLOBAL	"CbVarGlobal"
#define LUA_PARSER_CB_VAR_LOCAL		"CbVarLocal"
#define LUA_PARSER_CB_VAR_UPVALUE	"CbVarUpvalue"
#define LUA_PARSER_CB_FUNC			"CbFunc"
#define LUA_PARSER_CB_BREAKPOINT	"CbBreakpoint"
#define LUA_PARSER_CB_LOG			"CbLog"

struct lua_State;

namespace lparser_callbacks
{

typedef void (__stdcall* VariableCallback)(const char*, int);
typedef void (__stdcall* FunctionCallback)(const char*, int, int);
typedef void (__stdcall* BreakpointCallback)(int);
typedef void (__stdcall* LogCallback)(const char*);

}

namespace Unmanaged
{
	void Log(lua_State* luaState, const char* format, ...);
}

#endif // LUA_UTILITIES_NET

#endif // lparser_callbacks_h
