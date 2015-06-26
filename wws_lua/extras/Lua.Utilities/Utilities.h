/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __LUA_UTILITIES_UTILITIES_H__
#define __LUA_UTILITIES_UTILITIES_H__

#pragma once

#include "Base.h"

namespace Sce { namespace Lua { namespace Utilities
{

LUA_VER_NAMESPACE_OPEN
ARCH_NAMESPACE_OPEN

public ref class LuaSyntaxChecker : public ILuaSyntaxChecker
{
public:
	LuaSyntaxChecker() {}
	~LuaSyntaxChecker() { this->!LuaSyntaxChecker(); }
protected:
	!LuaSyntaxChecker() {}

public:
	virtual bool CheckFile(System::Uri^ scriptFile);
	virtual bool CheckBuffer(System::String^ bufferContents);

	property System::String^ Error
	{
		virtual System::String^ get()				{ return m_error; }
	}

private:
	System::String^	m_error;
};

public ref class LuaParser : public ILuaParser
{
public:
	LuaParser();
	~LuaParser() { this->!LuaParser(); }
protected:
	!LuaParser() {}

public:
	virtual bool Parse(System::Uri^ scriptFile);

	property System::String^ Error
	{
		virtual System::String^ get()				{ return m_error; }
	}

	property ParserLogDelegate^ LogHandler
	{
		virtual ParserLogDelegate^ get()			{ return m_logHandler; }
		virtual void set(ParserLogDelegate^ value)	{ m_logHandler = value; }
	}

public:
	property System::Collections::Generic::IEnumerable<LuaVariableEntry^>^ Globals
	{
		virtual System::Collections::Generic::IEnumerable<LuaVariableEntry^>^ get()		{ return m_lstGlobals; }
	}

	property System::Collections::Generic::IEnumerable<LuaVariableEntry^>^ Locals
	{
		virtual System::Collections::Generic::IEnumerable<LuaVariableEntry^>^ get()		{ return m_lstLocals; }
	}

	property System::Collections::Generic::IEnumerable<LuaVariableEntry^>^ Upvalues
	{
		virtual System::Collections::Generic::IEnumerable<LuaVariableEntry^>^ get()		{ return m_lstUpvalues; }
	}

	property System::Collections::Generic::IEnumerable<LuaFunctionEntry^>^ Functions
	{
		virtual System::Collections::Generic::IEnumerable<LuaFunctionEntry^>^ get()		{ return m_lstFunctions; }
	}

	property System::Collections::Generic::IEnumerable<int>^ ValidBreakpointLines
	{
		virtual System::Collections::Generic::IEnumerable<int>^ get()					{ return m_lstBreakpoints->Keys; }
	}

private:
	void AddGlobal(System::String^ szVariable, int line);
	void AddLocal(System::String^ szVariable, int line);
	void AddUpvalue(System::String^ szVariable, int line);
	void AddFunction(System::String^ szFunction, int lineDefined, int lastLineDefined);
	void AddBreakpoint(int iLine);
	void Log(System::String^ message);
	
	static void CheckAndUpdateVarOccurrence(LuaVariableEntry^ var, System::Collections::Generic::List<LuaVariableEntry^>^ lstVars);

private:
	System::String^	m_error;
	ParserLogDelegate^ m_logHandler;
	System::Collections::Generic::List<LuaVariableEntry^>^	m_lstGlobals;
	System::Collections::Generic::List<LuaVariableEntry^>^	m_lstLocals;
	System::Collections::Generic::List<LuaVariableEntry^>^	m_lstUpvalues;
	System::Collections::Generic::List<LuaFunctionEntry^>^	m_lstFunctions;
	System::Collections::Generic::SortedList<int, int>^ m_lstBreakpoints;
};

public ref class LuaCompiler : public ILuaCompiler
{	
public:
	LuaCompiler() {}
	~LuaCompiler() { this->!LuaCompiler(); }
protected:
	!LuaCompiler() {}

public:
	virtual bool Compile(System::Uri^ scriptAbsFilePath, System::Uri^ scriptAbsDumpFilePath, Sce::Lua::Utilities::LuaCompilerConfig^ config);

	property System::String^ Error
	{
		virtual System::String^ get()				{ return m_error; }
	}

private:
	System::String^ m_error;
};

ARCH_NAMESPACE_CLOSE
LUA_VER_NAMESPACE_CLOSE

}}}

#endif // __LUA_UTILITIES_UTILITIES_H__
