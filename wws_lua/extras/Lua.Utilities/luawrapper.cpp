/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "stdafx.h"
#include "luawrapper.h"

#include <msclr\marshal_cppstd.h>
#include <string>

namespace Unmanaged
{

StackReconciler::StackReconciler(lua_State* luaState)
	: m_luaState(luaState)
	, m_count(LuaInterface::GetTop(luaState))
{
}

StackReconciler::~StackReconciler()
{
	const int count = LuaInterface::GetTop(m_luaState);
	if ((count != m_count) && (count > m_count))
		LuaInterface::Pop(m_luaState, count - m_count);
}

LuaWrapper::LuaWrapper()
	: m_luaState(NULL)
{
	m_luaState = LuaInterface::Open();
	LuaInterface::OpenLibs(m_luaState);	
}

LuaWrapper::~LuaWrapper()
{
	LuaInterface::Close(m_luaState);
}

LuaInterface::Errors::Enum LuaWrapper::LoadFile(System::String^ scriptFile)
{	
	const std::string str = msclr::interop::marshal_as<std::string>(scriptFile);

	return LuaInterface::TranslateLuaError(LuaInterface::LoadFile(m_luaState, str.c_str()));
}

LuaInterface::Errors::Enum LuaWrapper::LoadFile(System::String^ scriptFile,
												Sce::Lua::Utilities::ParserVariableDelegate^ globalCb,
												Sce::Lua::Utilities::ParserVariableDelegate^ localCb,
												Sce::Lua::Utilities::ParserVariableDelegate^ upvalueCb,
												Sce::Lua::Utilities::ParserFunctionDelegate^ funcCb,
												Sce::Lua::Utilities::ParserBreakpointDelegate^ bpCb,
												Sce::Lua::Utilities::ParserLogDelegate^ logCb)
{
	System::IntPtr ipGlobalCb =
		globalCb == nullptr
			? System::IntPtr::Zero
			: System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(globalCb);

	System::IntPtr ipLocalCb =
		localCb == nullptr
			? System::IntPtr::Zero
			: System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(localCb);

	System::IntPtr ipUpvalueCb =
		upvalueCb == nullptr
			? System::IntPtr::Zero
			: System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(upvalueCb);

	System::IntPtr ipFuncCb =
		funcCb == nullptr
			? System::IntPtr::Zero
			: System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(funcCb);

	System::IntPtr ipBpCb =
		bpCb == nullptr
			? System::IntPtr::Zero
			: System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(bpCb);

	System::IntPtr ipLogCb =
		logCb == nullptr
			? System::IntPtr::Zero
			: System::Runtime::InteropServices::Marshal::GetFunctionPointerForDelegate(logCb);

	// Tell Lua about the function pointers
	{
		lparser_callbacks::VariableCallback nativeGlobalCb = static_cast<lparser_callbacks::VariableCallback>(ipGlobalCb.ToPointer());
		lparser_callbacks::VariableCallback nativeLocalCb = static_cast<lparser_callbacks::VariableCallback>(ipLocalCb.ToPointer());
		lparser_callbacks::VariableCallback nativeUpvalueCb = static_cast<lparser_callbacks::VariableCallback>(ipUpvalueCb.ToPointer());
		lparser_callbacks::FunctionCallback nativeFuncCb = static_cast<lparser_callbacks::FunctionCallback>(ipFuncCb.ToPointer());
		lparser_callbacks::BreakpointCallback nativeBpCb = static_cast<lparser_callbacks::BreakpointCallback>(ipBpCb.ToPointer());
		lparser_callbacks::LogCallback nativeLogCb = static_cast<lparser_callbacks::LogCallback>(ipLogCb.ToPointer());

		const Unmanaged::StackReconciler recon(m_luaState);

		LuaInterface::GetGlobal(m_luaState, LUA_PARSER_DEBUG_TABLE);
		if (LuaInterface::IsNil(m_luaState, -1))
		{
			LuaInterface::Pop(m_luaState, 1);

			// Create parser debug table
			LuaInterface::NewTable(m_luaState);
			LuaInterface::SetGlobal(m_luaState, LUA_PARSER_DEBUG_TABLE);

			// Get parser debug table on top of stack
			LuaInterface::GetGlobal(m_luaState, LUA_PARSER_DEBUG_TABLE);

			// Set function pointers
			LuaInterface::PushLightUserdata(m_luaState, nativeGlobalCb);
			LuaInterface::SetField(m_luaState, -2, LUA_PARSER_CB_VAR_GLOBAL);
			LuaInterface::PushLightUserdata(m_luaState, nativeLocalCb);
			LuaInterface::SetField(m_luaState, -2, LUA_PARSER_CB_VAR_LOCAL);
			LuaInterface::PushLightUserdata(m_luaState, nativeUpvalueCb);
			LuaInterface::SetField(m_luaState, -2, LUA_PARSER_CB_VAR_UPVALUE);
			LuaInterface::PushLightUserdata(m_luaState, nativeFuncCb);
			LuaInterface::SetField(m_luaState, -2, LUA_PARSER_CB_FUNC);
			LuaInterface::PushLightUserdata(m_luaState, nativeBpCb);
			LuaInterface::SetField(m_luaState, -2, LUA_PARSER_CB_BREAKPOINT);
			LuaInterface::PushLightUserdata(m_luaState, nativeLogCb);
			LuaInterface::SetField(m_luaState, -2, LUA_PARSER_CB_LOG);
		}
	}

	const LuaInterface::Errors::Enum retval = LuaInterface::TranslateLuaError(LoadFile(scriptFile));

	{
		ipGlobalCb = System::IntPtr::Zero;
		ipLocalCb = System::IntPtr::Zero;
		ipUpvalueCb = System::IntPtr::Zero;
		ipFuncCb = System::IntPtr::Zero;
		ipBpCb = System::IntPtr::Zero;
		ipLogCb = System::IntPtr::Zero;
	}

	return retval;
}

LuaInterface::Errors::Enum LuaWrapper::LoadBuffer(System::String^ scriptBuffer)
{
	const std::string buffer = msclr::interop::marshal_as<std::string>(scriptBuffer);

	return LuaInterface::TranslateLuaError(LuaInterface::LoadBuffer(m_luaState, buffer.c_str(), buffer.length(), "buffer"));
}

int LuaWrapper::Compile_Writer(lua_State* L, const void* pData, size_t size, void* pFile)
{
	(void)L;
	return ((fwrite(pData, size, 1, (FILE *)pFile) != 1) && (size != 0));
}

LuaInterface::Errors::Enum LuaWrapper::Compile(System::String^ scriptAbsFilePath, System::String^ scriptAbsDumpFilePath, Sce::Lua::Utilities::LuaCompilerConfig^ config)
{
	// Load script into Lua state
	{
		const std::string strInFile = msclr::interop::marshal_as<std::string>(scriptAbsFilePath);

		const LuaInterface::Errors::Enum err =
			LuaInterface::TranslateLuaError(LuaInterface::LoadFile(m_luaState, strInFile.c_str()));

		if (err != LuaInterface::Errors::Ok)
			return err;
	}	

	// Create and open the dump file
	FILE* pDumpFile = 0;
	{
		const std::string strOutFile = msclr::interop::marshal_as<std::string>(scriptAbsDumpFilePath);

		if (fopen_s(&pDumpFile, strOutFile.c_str(), "wb") != 0)
			return LuaInterface::Errors::ErrOutputFile;
	}

	// Dump script to file
	{
		LuaInterface::DumpConfig dumpConfig;
		dumpConfig.endianness = (int)config->Endianness;
		dumpConfig.sizeof_int = config->SizeOfInt;
		dumpConfig.sizeof_size_t = config->SizeOfSizeT;
		dumpConfig.sizeof_lua_Number = config->SizeOfLuaNumber;
		
		const LuaInterface::Errors::Enum err =
			LuaInterface::TranslateLuaError(
				LuaInterface::DumpEx(m_luaState, Compile_Writer, pDumpFile, (config->StripDebugInfo ? 1 : 0), dumpConfig));
	
		// Close dump file
		fclose(pDumpFile);

		return err;
	}
}

}
