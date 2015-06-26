/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "scmp.h"
#include "assert.h"
#include "buffer.h"
#include "utilities.h"

#include <cstring>

namespace sce { namespace Sled { namespace SCMP
{
	void Version::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packUInt16_t(majorNum);
		packer.packUInt16_t(minorNum);
		packer.packUInt16_t(revisionNum);
	}

	namespace Breakpoint
	{
		Details::Details(uint16_t iPluginId, const char *pszRelFilePath, int32_t iLine, const char *pszCondition, bool bResult, bool bUseFunctionEnvironment)
		{
			typeCode = TypeCodes::kBreakpointDetails;
			pluginId = iPluginId;

			Utilities::copyString(relFilePath, kStringLen, pszRelFilePath);
			line = iLine;
			Utilities::copyString(condition, kStringLen, pszCondition);
			result = (bResult ? 1 : 0);
			useFunctionEnvironment = (bUseFunctionEnvironment ? 1 : 0);

			length = kSizeOfBase
				+ kSizeOfuint16_t + (int32_t)std::strlen(relFilePath)
				+ kSizeOfint32_t
				+ kSizeOfuint16_t + (int32_t)std::strlen(condition)
				+ kSizeOfuint8_t
				+ kSizeOfuint8_t;
		}

		void Details::unpack(NetworkBufferReader *reader)
		{
			length = reader->readInt32_t();
			typeCode = reader->readUInt16_t();
			pluginId = reader->readUInt16_t();
			reader->readString(relFilePath, kStringLen);
			line = reader->readInt32_t();
			reader->readString(condition, kStringLen);
			result = reader->readUInt8_t();
			useFunctionEnvironment = reader->readUInt8_t();
		}

		Begin::Begin(uint16_t iPluginId, uint16_t iBreakPluginId, const char *pszRelFilePath, int32_t iLine, NetworkBuffer *pBuffer /* = 0 */)
		{
			typeCode = TypeCodes::kBreakpointBegin;
			pluginId = iPluginId;

			breakPluginId = iBreakPluginId;
			Utilities::copyString(relFilePath, kStringLen, pszRelFilePath);
			line = iLine;

			length = kSizeOfBase
				+ kSizeOfuint16_t +
				+ kSizeOfuint16_t + (int32_t)std::strlen(relFilePath)
				+ kSizeOfint32_t;

			if (pBuffer)
				pack(pBuffer);
		}

		void Begin::unpack(NetworkBufferReader *reader)
		{
			length = reader->readInt32_t();
			typeCode = reader->readUInt16_t();
			pluginId = reader->readUInt16_t();
			breakPluginId = reader->readUInt16_t();
			reader->readString(relFilePath, kStringLen);
			line = reader->readInt32_t();
		}

		void Begin::pack(NetworkBuffer *pBuffer)
		{
			NetworkBufferPacker packer(pBuffer);

			packer.packInt32_t(length);
			packer.packUInt16_t(typeCode);
			packer.packUInt16_t(pluginId);
			packer.packUInt16_t(breakPluginId);
			packer.packString(relFilePath);
			packer.packInt32_t(line);
		}

		Sync::Sync(uint16_t iPluginId, uint16_t iBreakPluginId, const char *pszRelFilePath, int32_t iLine, NetworkBuffer *pBuffer /* = 0 */)
		{
			typeCode = TypeCodes::kBreakpointSync;
			pluginId = iPluginId;

			breakPluginId = iBreakPluginId;
			Utilities::copyString(relFilePath, kStringLen, pszRelFilePath);
			line = iLine;

			length = kSizeOfBase
				+ kSizeOfuint16_t +
				+ kSizeOfuint16_t + (int32_t)std::strlen(relFilePath)
				+ kSizeOfint32_t;

			if (pBuffer)
				pack(pBuffer);
		}

		void Sync::unpack(NetworkBufferReader *reader)
		{
			length = reader->readInt32_t();
			typeCode = reader->readUInt16_t();
			pluginId = reader->readUInt16_t();
			breakPluginId = reader->readUInt16_t();
			reader->readString(relFilePath, kStringLen);
			line = reader->readInt32_t();
		}

		void Sync::pack(NetworkBuffer *pBuffer)
		{
			NetworkBufferPacker packer(pBuffer);

			packer.packInt32_t(length);
			packer.packUInt16_t(typeCode);
			packer.packUInt16_t(pluginId);
			packer.packUInt16_t(breakPluginId);
			packer.packString(relFilePath);
			packer.packInt32_t(line);
		}

		End::End(uint16_t iPluginId, uint16_t iBreakPluginId, const char *pszRelFilePath, int32_t iLine, NetworkBuffer *pBuffer /* = 0 */)
		{
			typeCode = TypeCodes::kBreakpointEnd;
			pluginId = iPluginId;

			breakPluginId = iBreakPluginId;
			Utilities::copyString(relFilePath, kStringLen, pszRelFilePath);
			line = iLine;

			length = kSizeOfBase
				+ kSizeOfuint16_t +
				+ kSizeOfuint16_t + (int32_t)std::strlen(relFilePath)
				+ kSizeOfint32_t;

			if (pBuffer)
				pack(pBuffer);
		}

		void End::unpack(NetworkBufferReader *reader)
		{
			length = reader->readInt32_t();
			typeCode = reader->readUInt16_t();
			pluginId = reader->readUInt16_t();
			breakPluginId = reader->readUInt16_t();
			reader->readString(relFilePath, kStringLen);
			line = reader->readInt32_t();
		}

		void End::pack(NetworkBuffer *pBuffer)
		{
			NetworkBufferPacker packer(pBuffer);

			packer.packInt32_t(length);
			packer.packUInt16_t(typeCode);
			packer.packUInt16_t(pluginId);
			packer.packUInt16_t(breakPluginId);
			packer.packString(relFilePath);
			packer.packInt32_t(line);
		}

		Continue::Continue(uint16_t iPluginId, uint16_t iBreakPluginId, const char *pszRelFilePath, int32_t iLine, NetworkBuffer *pBuffer /* = 0 */)
		{
			typeCode = TypeCodes::kBreakpointContinue;
			pluginId = iPluginId;

			breakPluginId = iBreakPluginId;
			Utilities::copyString(relFilePath, kStringLen, pszRelFilePath);
			line = iLine;

			length = kSizeOfBase
				+ kSizeOfuint16_t +
				+ kSizeOfuint16_t + (int32_t)std::strlen(relFilePath)
				+ kSizeOfint32_t;

			if (pBuffer)
				pack(pBuffer);
		}

		void Continue::unpack(NetworkBufferReader *reader)
		{
			length = reader->readInt32_t();
			typeCode = reader->readUInt16_t();
			pluginId = reader->readUInt16_t();
			breakPluginId = reader->readUInt16_t();
			reader->readString(relFilePath, kStringLen);
			line = reader->readInt32_t();
		}

		void Continue::pack(NetworkBuffer *pBuffer)
		{
			NetworkBufferPacker packer(pBuffer);

			packer.packInt32_t(length);
			packer.packUInt16_t(typeCode);
			packer.packUInt16_t(pluginId);
			packer.packUInt16_t(breakPluginId);
			packer.packString(relFilePath);
			packer.packInt32_t(line);
		}
	}

	ScriptCache::ScriptCache(uint16_t iPluginId, const char *pszRelScriptPath, NetworkBuffer *pBuffer /* = 0 */)
	{
		typeCode = TypeCodes::kScriptCache;
		pluginId = iPluginId;

		Utilities::copyString(relScriptPath, kStringLen, pszRelScriptPath);

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int32_t)std::strlen(relScriptPath);

		if (pBuffer)
			pack(pBuffer);
	}

	void ScriptCache::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packString(relScriptPath);
	}

	FunctionInfo::FunctionInfo(uint16_t iPluginId, const char *pszScriptFile, const char *pszFuncName, int32_t iLineDefined)
	{
		typeCode = TypeCodes::kFunctionInfo;
		pluginId = iPluginId;

		Utilities::copyString(relScriptPath, kStringLen, pszScriptFile);
		Utilities::copyString(functionName, kStringLen, pszFuncName);
		lineDefined = iLineDefined;

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int32_t)std::strlen(relScriptPath)
			+ kSizeOfuint16_t + (int32_t)std::strlen(functionName)
			+ kSizeOfint32_t;
	}

	void FunctionInfo::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		reader->readString(relScriptPath, kStringLen);
		reader->readString(functionName, kStringLen);
		lineDefined = reader->readInt32_t();
	}

	TTY::TTY(uint16_t iPluginId, const char *pszMessage, NetworkBuffer *pBuffer /* = 0 */)
	{					
		typeCode = TypeCodes::kTTY;
		pluginId = iPluginId;

		Utilities::copyString(message, kStringLen, pszMessage);

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int32_t)std::strlen(message);

		if (pBuffer)
			pack(pBuffer);
	}

	void TTY::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packString(message);
	}

	DevCmd::DevCmd(uint16_t iPluginId, const char *pszCommand)
	{
		typeCode = TypeCodes::kDevCmd;
		pluginId = iPluginId;

		Utilities::copyString(command, kStringLen, pszCommand);

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int32_t)std::strlen(command);
	}

	void DevCmd::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		reader->readString(command, kStringLen);
	}

	EditAndContinue::EditAndContinue(uint16_t iPluginId, const char *pszRelScriptPath)
	{
		typeCode = TypeCodes::kEditAndContinue;
		pluginId = iPluginId;

		Utilities::copyString(relScriptPath, kStringLen, pszRelScriptPath);

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int32_t)std::strlen(relScriptPath);
	}

	void EditAndContinue::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		reader->readString(relScriptPath, kStringLen);
	}
}}}
