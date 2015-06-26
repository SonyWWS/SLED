/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "stdafx.h"

#include "Utilities.h"
#include "luawrapper.h"

using namespace System::Runtime::InteropServices;

namespace Sce { namespace Lua { namespace Utilities
{

LUA_VER_NAMESPACE_OPEN
ARCH_NAMESPACE_OPEN

namespace
{
	class ScopedGCHandle
	{
	public:
		ScopedGCHandle(System::Object^ obj) { m_handle = GCHandle::Alloc(obj); }
		~ScopedGCHandle() { m_handle.Free(); }
	private:
		System::Runtime::InteropServices::GCHandle m_handle;
	};
}

bool LuaSyntaxChecker::CheckFile(System::Uri^ scriptFile)
{
	bool retval = false;
	Unmanaged::LuaWrapper lua;

	const LuaInterface::Errors::Enum err = lua.LoadFile(scriptFile->LocalPath);
	switch (err)
	{		
		case LuaInterface::Errors::Ok:
			retval = true;
			break;

		case LuaInterface::Errors::ErrSyntax:
			m_error = gcnew System::String(LuaInterface::ToString(lua.m_luaState, -1));
			break;

		case LuaInterface::Errors::ErrMem:
			m_error = gcnew System::String("Insufficient memory");
			break;

		default:
			m_error = gcnew System::String("Unknown error!");
			break;
	}

	return retval;
}

bool LuaSyntaxChecker::CheckBuffer(System::String^ scriptBuffer)
{
	bool retval = false;
	Unmanaged::LuaWrapper lua;

	const LuaInterface::Errors::Enum err = lua.LoadBuffer(scriptBuffer);
	switch (err)
	{
		case LuaInterface::Errors::Ok:
			retval = true;
			break;

		case LuaInterface::Errors::ErrSyntax:
			m_error = gcnew System::String(LuaInterface::ToString(lua.m_luaState, -1));
			break;

		case LuaInterface::Errors::ErrMem:
			m_error = gcnew System::String("Insufficient memory");
			break;

		default:
			m_error = gcnew System::String("Unknown error!");
			break;
	}

	return retval;
}

LuaParser::LuaParser()
{
	m_lstGlobals = gcnew System::Collections::Generic::List<LuaVariableEntry^>();
	m_lstLocals = gcnew System::Collections::Generic::List<LuaVariableEntry^>();
	m_lstUpvalues = gcnew System::Collections::Generic::List<LuaVariableEntry^>();
	m_lstFunctions = gcnew System::Collections::Generic::List<LuaFunctionEntry^>();
	m_lstBreakpoints = gcnew System::Collections::Generic::SortedList<int, int>();
}

bool LuaParser::Parse(System::Uri^ scriptFile)
{
	bool retval = false;
	Unmanaged::LuaWrapper lua;

	// Clear out any previous values
	m_lstGlobals->Clear();
	m_lstLocals->Clear();
	m_lstUpvalues->Clear();
	m_lstFunctions->Clear();
	m_lstBreakpoints->Clear();	

	ParserVariableDelegate^ globalCb = gcnew ParserVariableDelegate(this, &LuaParser::AddGlobal);
	ParserVariableDelegate^ localCb = gcnew ParserVariableDelegate(this, &LuaParser::AddLocal);
	ParserVariableDelegate^ upvalueCb = gcnew ParserVariableDelegate(this, &LuaParser::AddUpvalue);
	ParserFunctionDelegate^ funcCb = gcnew ParserFunctionDelegate(this, &LuaParser::AddFunction);
	ParserBreakpointDelegate^ bpCb = gcnew ParserBreakpointDelegate(this, &LuaParser::AddBreakpoint);
	ParserLogDelegate^ logCb = gcnew ParserLogDelegate(this, &LuaParser::Log);

	// Make sure delegates aren't collected
	const ScopedGCHandle sgchGlobal(globalCb);
	const ScopedGCHandle sgchLocal(localCb);
	const ScopedGCHandle sgchUpvalue(upvalueCb);
	const ScopedGCHandle sgchFunc(funcCb);
	const ScopedGCHandle sgchBp(bpCb);
	const ScopedGCHandle sgchLog(logCb);

	// Parse
	const LuaInterface::Errors::Enum err = lua.LoadFile(scriptFile->LocalPath, globalCb, localCb, upvalueCb, funcCb, bpCb, logCb);
	switch (err)
	{
		case LuaInterface::Errors::Ok:
			retval = true;
			break;

		case LuaInterface::Errors::ErrSyntax:
			m_error = gcnew System::String(LuaInterface::ToString(lua.m_luaState, -1));
			break;

		case LuaInterface::Errors::ErrMem:
			m_error = gcnew System::String("Insufficient memory");
			break;

		case LuaInterface::Errors::ErrRun:
			m_error = gcnew System::String("Runtime error");
			break;

		default:
			m_error = gcnew System::String("Unknown error!");
			break;
	}

	return retval;
}

void LuaParser::AddGlobal(System::String^ name, int line)
{	
	LuaVariableEntry^ entry = gcnew LuaVariableEntry(name, line);

	// Check for duplicates to set the occurrence value correctly before adding
	CheckAndUpdateVarOccurrence(entry, m_lstGlobals);

	m_lstGlobals->Add(entry);
}

void LuaParser::AddLocal(System::String^ name, int line)
{
	LuaVariableEntry^ entry = gcnew LuaVariableEntry(name, line);

	// Check for duplicates to set the occurrence value correctly before adding
	CheckAndUpdateVarOccurrence(entry, m_lstLocals);

	m_lstLocals->Add(entry);
}

void LuaParser::AddUpvalue(System::String^ name, int line)
{
	LuaVariableEntry^ entry = gcnew LuaVariableEntry(name, line);

	// Check for duplicates to set the occurrence value correctly before adding
	CheckAndUpdateVarOccurrence(entry, m_lstUpvalues);

	m_lstUpvalues->Add(entry);
}

void LuaParser::AddFunction(System::String^ name, int lineDefined, int lastLineDefined)
{
	const int iCount = m_lstFunctions->Count;
	for (int i = 0; i < iCount; i++)
	{
		if (m_lstFunctions[i]->LineDefined != lineDefined)
			continue;

		if (m_lstFunctions[i]->LastLineDefined != lastLineDefined)
			continue;

		if (System::String::Compare(m_lstFunctions[i]->Name, name) != 0)
			continue;

		// All criteria matched so it's a duplicate so don't add
		return;
	}

	LuaFunctionEntry^ entry = gcnew LuaFunctionEntry(name, lineDefined, lastLineDefined);

	m_lstFunctions->Add(entry);
}

void LuaParser::AddBreakpoint(int line)
{
	if (m_lstBreakpoints->ContainsKey(line))
		return;

	m_lstBreakpoints->Add(line, line);
}

void LuaParser::Log(System::String^ message)
{
	if (LogHandler != nullptr)
		LogHandler(message);
}

void LuaParser::CheckAndUpdateVarOccurrence(LuaVariableEntry^ var, System::Collections::Generic::List<LuaVariableEntry^>^ lstVars)
{
	const int iCount = lstVars->Count;
	if (iCount > 1)
	{
		for (int i = iCount - 1; i >= 0; i--)
		{
			if (lstVars[i]->Line != var->Line)
				continue;

			if (System::String::Compare(lstVars[i]->Name, var->Name) != 0)
				continue;

			var->Occurrence = lstVars[i]->Occurrence + 1;
			return;
		}
	}
}

bool LuaCompiler::Compile(System::Uri^ scriptAbsFilePath, System::Uri^ scriptAbsDumpFilePath, Sce::Lua::Utilities::LuaCompilerConfig^ config)
{
	bool retval = false;
	Unmanaged::LuaWrapper lua;	

	// Compile
	const LuaInterface::Errors::Enum err = lua.Compile(scriptAbsFilePath->LocalPath, scriptAbsDumpFilePath->LocalPath, config);
	switch (err)
	{
		case LuaInterface::Errors::Ok:
			retval = true;
			break;

		case LuaInterface::Errors::ErrSyntax:
			m_error = gcnew System::String(LuaInterface::ToString(lua.m_luaState, -1));
			break;

		case LuaInterface::Errors::ErrMem:
			m_error = gcnew System::String("Insufficient memory");
			break;

		case LuaInterface::Errors::ErrRun:
			m_error = gcnew System::String("Runtime error");
			break;

		case LuaInterface::Errors::ErrOutputFile:
			m_error = gcnew System::String("Failed to create output file");
			break;

		default: m_error = gcnew System::String("Unknown error!");
			break;
	}

	return retval;
}

ARCH_NAMESPACE_CLOSE
LUA_VER_NAMESPACE_CLOSE

}}}
