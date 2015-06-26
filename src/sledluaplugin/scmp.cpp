/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "scmp.h"

#include "../sleddebugger/buffer.h"
#include "../sleddebugger/utilities.h"
#include "luautils.h"
#include "../sleddebugger/assert.h"

#include <cstdio>
#include <cstring>

namespace sce { namespace Sled { namespace SCMP
{
	MemoryTrace::MemoryTrace(uint16_t iPluginId, char chWhat, void *pOldPtr, void *pNewPtr, std::size_t iOldSize, std::size_t iNewSize, NetworkBuffer *pBuffer /* = 0 */)
	{
		typeCode = LuaTypeCodes::kMemoryTrace;
		pluginId = iPluginId;
	
		what = (uint8_t)chWhat;
		StringUtilities::copyString(oldPtr, Sizes::kPtrLen, "0x%p", pOldPtr);
		StringUtilities::copyString(newPtr, Sizes::kPtrLen, "0x%p", pNewPtr);
		oldSize = (int32_t)iOldSize;
		newSize = (int32_t)iNewSize;
	
		length = kSizeOfBase
			+ kSizeOfuint8_t
			+ kSizeOfuint16_t + (int)std::strlen(oldPtr)
			+ kSizeOfuint16_t + (int)std::strlen(newPtr)
			+ kSizeOfint32_t
			+ kSizeOfint32_t;
	
		if (pBuffer)
			pack(pBuffer);
	}

	void MemoryTrace::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packUInt8_t(what);
		packer.packString(oldPtr);
		packer.packString(newPtr);
		packer.packInt32_t(oldSize);
		packer.packInt32_t(newSize);
	}

	MemoryTraceStream::MemoryTraceStream(uint16_t iPluginId, char chWhat, void *pOldPtr, void *pNewPtr, std::size_t iOldSize, std::size_t iNewSize, NetworkBuffer *pBuffer /* = 0 */)
	{
		typeCode = LuaTypeCodes::kMemoryTraceStream;
		pluginId = iPluginId;

		what = (uint8_t)chWhat;
		StringUtilities::copyString(oldPtr, Sizes::kPtrLen, "0x%p", pOldPtr);
		StringUtilities::copyString(newPtr, Sizes::kPtrLen, "0x%p", pNewPtr);
		oldSize = (int32_t)iOldSize;
		newSize = (int32_t)iNewSize;

		length = kSizeOfBase
			+ kSizeOfuint8_t
			+ kSizeOfuint16_t + (int)std::strlen(oldPtr)
			+ kSizeOfuint16_t + (int)std::strlen(newPtr)
			+ kSizeOfint32_t
			+ kSizeOfint32_t;

		if (pBuffer)
			pack(pBuffer);
	}

	void MemoryTraceStream::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packUInt8_t(what);
		packer.packString(oldPtr);
		packer.packString(newPtr);
		packer.packInt32_t(oldSize);
		packer.packInt32_t(newSize);
	}

	ProfileInfo::ProfileInfo(uint16_t iPluginId, const char *pszFuncName, const char *pszRelScriptPath, 
							 float flFnTimeElapsed, float flFnTimeElapsedAvg, float flFnTimeElapsedShortest, float flFnTimeElapsedLongest, 
							 float flFnTimeInnerElapsed, float flFnTimeInnerElapsedAvg, float flFnTimeInnerElapsedShortest, float flFnTimeInnerElapsedLongest, 
							 uint32_t iFnCallCount, int32_t iFnLine, int32_t iFnCalls, NetworkBuffer *pBuffer /* = 0 */)
	{
		typeCode = LuaTypeCodes::kProfileInfo;
		pluginId = iPluginId;

		Utilities::copyString(functionName, kStringLen, pszFuncName);
		Utilities::copyString(relScriptPath, kStringLen, pszRelScriptPath);
		fnTimeElapsed = flFnTimeElapsed;
		fnTimeElapsedAvg = flFnTimeElapsedAvg;
		fnTimeElapsedShortest = flFnTimeElapsedShortest;
		fnTimeElapsedLongest = flFnTimeElapsedLongest;
		fnTimeInnerElapsed = flFnTimeInnerElapsed;
		fnTimeInnerElapsedAvg = flFnTimeInnerElapsedAvg;
		fnTimeInnerElapsedShortest = flFnTimeInnerElapsedShortest;
		fnTimeInnerElapsedLongest = flFnTimeInnerElapsedLongest;
		fnCallCount = iFnCallCount;
		fnLine = iFnLine;
		fnCalls = iFnCalls;

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(functionName)
			+ kSizeOfuint16_t + (int)std::strlen(relScriptPath)
			+ (kSizeOffloat * 8)
			+ kSizeOfuint32_t
			+ kSizeOfint32_t
			+ kSizeOfint32_t;

		if (pBuffer)
			pack(pBuffer);
	}

	void ProfileInfo::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packString(functionName);
		packer.packString(relScriptPath);
		packer.packFloat(fnTimeElapsed);
		packer.packFloat(fnTimeElapsedAvg);
		packer.packFloat(fnTimeElapsedShortest);
		packer.packFloat(fnTimeElapsedLongest);
		packer.packFloat(fnTimeInnerElapsed);
		packer.packFloat(fnTimeInnerElapsedAvg);
		packer.packFloat(fnTimeInnerElapsedShortest);
		packer.packFloat(fnTimeInnerElapsedLongest);
		packer.packUInt32_t(fnCallCount);
		packer.packInt32_t(fnLine);
		packer.packInt32_t(fnCalls);
	}

	ProfileInfoLookUpPerform::ProfileInfoLookUpPerform(uint16_t iPluginId, const char *pszFuncName, char chWhat, int32_t iLine, const char *pszRelScriptPath)
	{
		typeCode = LuaTypeCodes::kProfileInfoLookUpPerform;
		pluginId = iPluginId;

		Utilities::copyString(functionName, kStringLen, pszFuncName);
		what = (uint8_t)chWhat;
		line = iLine;
		Utilities::copyString(relScriptPath, kStringLen, pszRelScriptPath);

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(functionName)
			+ kSizeOfuint8_t
			+ kSizeOfint32_t
			+ kSizeOfuint16_t + (int)std::strlen(relScriptPath);
	}

	void ProfileInfoLookUpPerform::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		reader->readString(functionName, kStringLen);
		what = reader->readUInt8_t();
		line = reader->readInt32_t();
		reader->readString(relScriptPath, kStringLen);
	}

	ProfileInfoLookUp::ProfileInfoLookUp(uint16_t iPluginId, const char *pszFuncName, const char *pszRelScriptPath,
										 float flFnTimeElapsed, float flFnTimeElapsedAvg, float flFnTimeElapsedShortest, float flFnTimeElapsedLongest, 
										 float flFnTimeInnerElapsed, float flFnTimeInnerElapsedAvg, float flFnTimeInnerElapsedShortest, float flFnTimeInnerElapsedLongest, 
										 uint32_t iFnCallCount, int32_t iFnLine, int32_t iFnCalls, NetworkBuffer *pBuffer /* = 0 */)
	{
		typeCode = LuaTypeCodes::kProfileInfoLookUp;
		pluginId = iPluginId;

		Utilities::copyString(functionName, kStringLen, pszFuncName);
		Utilities::copyString(relScriptPath, kStringLen, pszRelScriptPath);
		fnTimeElapsed = flFnTimeElapsed;
		fnTimeElapsedAvg = flFnTimeElapsedAvg;
		fnTimeElapsedShortest = flFnTimeElapsedShortest;
		fnTimeElapsedLongest = flFnTimeElapsedLongest;
		fnTimeInnerElapsed = flFnTimeInnerElapsed;
		fnTimeInnerElapsedAvg = flFnTimeInnerElapsedAvg;
		fnTimeInnerElapsedShortest = flFnTimeInnerElapsedShortest;
		fnTimeInnerElapsedLongest = flFnTimeInnerElapsedLongest;
		fnCallCount = iFnCallCount;
		fnLine = iFnLine;
		fnCalls = iFnCalls;

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(functionName)
			+ kSizeOfuint16_t + (int)std::strlen(relScriptPath)
			+ (kSizeOffloat * 8)
			+ kSizeOfuint32_t
			+ kSizeOfint32_t
			+ kSizeOfint32_t;

		if (pBuffer)
			pack(pBuffer);
	}

	void ProfileInfoLookUp::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packString(functionName);
		packer.packString(relScriptPath);
		packer.packFloat(fnTimeElapsed);
		packer.packFloat(fnTimeElapsedAvg);
		packer.packFloat(fnTimeElapsedShortest);
		packer.packFloat(fnTimeElapsedLongest);
		packer.packFloat(fnTimeInnerElapsed);
		packer.packFloat(fnTimeInnerElapsedAvg);
		packer.packFloat(fnTimeInnerElapsedShortest);
		packer.packFloat(fnTimeInnerElapsedLongest);
		packer.packUInt32_t(fnCallCount);
		packer.packInt32_t(fnLine);
		packer.packInt32_t(fnCalls);
	}

	VarFilterStateTypeBegin::VarFilterStateTypeBegin(uint16_t iPluginId, char chWhat)
	{
		length = kSizeOfBase + kSizeOfuint8_t;
		typeCode = LuaTypeCodes::kVarFilterStateTypeBegin;
		pluginId = iPluginId;

		what = (uint8_t)chWhat;
	}

	void VarFilterStateTypeBegin::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		what = reader->readUInt8_t();
	}

	VarFilterStateType::VarFilterStateType(uint16_t iPluginId, char chWhat, const uint8_t *pFilter)
	{
		length = kSizeOfBase + kSizeOfuint8_t + (kSizeOfuint8_t * 9);
		typeCode = LuaTypeCodes::kVarFilterStateType;
		pluginId = iPluginId;

		what = (uint8_t)chWhat;

		// Possible problem if pFilter is null or smaller than Filter
		for (int i = 0; i < 9; i++)
			filter[i] = pFilter[i];
	}

	void VarFilterStateType::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		what = reader->readUInt8_t();
		const int iSize = (int)reader->readUInt16_t(); // Read size of array
		for (int i = 0; i < iSize; i++)
			filter[i] = reader->readUInt8_t();
	}

	VarFilterStateTypeEnd::VarFilterStateTypeEnd(uint16_t iPluginId, char chWhat)
	{
		length = kSizeOfBase + kSizeOfuint8_t;
		typeCode = LuaTypeCodes::kVarFilterStateTypeEnd;
		pluginId = iPluginId;

		what = (uint8_t)chWhat;
	}

	void VarFilterStateTypeEnd::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		what = reader->readUInt8_t();
	}

	VarFilterStateNameBegin::VarFilterStateNameBegin(uint16_t iPluginId, char chWhat)
	{
		length = kSizeOfBase + kSizeOfuint8_t;
		typeCode = LuaTypeCodes::kVarFilterStateNameBegin;
		pluginId = iPluginId;

		what = (uint8_t)chWhat;
	}

	void VarFilterStateNameBegin::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		what = reader->readUInt8_t();
	}

	VarFilterStateName::VarFilterStateName(uint16_t iPluginId, char chWhat, const char *pszFilter)
	{
		typeCode = LuaTypeCodes::kVarFilterStateName;
		pluginId = iPluginId;

		what = (uint8_t)chWhat;
		Utilities::copyString(filter, kStringLen, pszFilter);

		length = kSizeOfBase
			+ kSizeOfuint8_t
			+ kSizeOfuint16_t + (int)std::strlen(filter);
	}

	void VarFilterStateName::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		what = reader->readUInt8_t();
		reader->readString(filter, kStringLen);
	}

	VarFilterStateNameEnd::VarFilterStateNameEnd(uint16_t iPluginId, char chWhat)
	{
		length = kSizeOfBase + kSizeOfuint8_t;
		typeCode = LuaTypeCodes::kVarFilterStateNameEnd;
		pluginId = iPluginId;

		what = (uint8_t)chWhat;
	}

	void VarFilterStateNameEnd::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		what = reader->readUInt8_t();
	}

	GlobalVar::GlobalVar(uint16_t iPluginId, const LuaVariable *pParent, const char *pszName, int16_t iNameType, const char *pszValue, int16_t iValueType, NetworkBuffer *pBuffer /* = 0 */)
		: parent(pParent)
		, name(pszName)
		, nameType(iNameType)
		, value(pszValue)
		, valueType(iValueType)
	{
		typeCode = LuaTypeCodes::kGlobalVar;
		pluginId = iPluginId;

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(name) // name
			+ kSizeOfint16_t // name type
			+ kSizeOfuint16_t + (int)std::strlen(value) // value
			+ kSizeOfint16_t // value type
			+ kSizeOfuint16_t; // # of parent and key-value-pairs

		const uint16_t extraOffset = parent == NULL ? 0 : parent->bFlag ? 1 : 0;

		if (pParent != NULL)
		{
			length += (kSizeOfuint16_t + (int)std::strlen(pParent->name));
			length += kSizeOfint16_t; // name type

			for (int i = 0; i < pParent->numKeyValues - extraOffset; ++i)
			{
				length += (kSizeOfuint16_t + (int)std::strlen(pParent->hKeyValues[i].name));
				length += kSizeOfint16_t; // name type
			}			
		}

		if (pBuffer)
			pack(pBuffer);
	}

	void GlobalVar::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);
	
		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);

		packer.packString(name);
		packer.packInt16_t(nameType);
		packer.packString(value);
		packer.packInt16_t(valueType);

		const uint16_t extraOffset = parent == NULL ? 0 : parent->bFlag ? 1 : 0;

		if (parent == NULL)
		{
			packer.packUInt16_t(0);
		}
		else
		{
			packer.packUInt16_t(parent->numKeyValues + 1 - extraOffset);		
		
			packer.packString(parent->name);
			packer.packInt16_t((int16_t)parent->nameType);
			for (int i = 0; i < parent->numKeyValues - extraOffset; ++i)
			{
				packer.packString(parent->hKeyValues[i].name);
				packer.packInt16_t((int16_t)parent->hKeyValues[i].type);
			}
		}
	}

	LocalVar::LocalVar(uint16_t iPluginId, const LuaVariable *pParent, const char *pszName, int16_t iNameType, const char *pszValue, int16_t iValueType, int16_t iStackLevel, int32_t iIndex, NetworkBuffer *pBuffer /* = 0 */)
		: parent(pParent)
		, name(pszName)
		, nameType(iNameType)
		, value(pszValue)
		, valueType(iValueType)
		, stackLevel(iStackLevel)
		, index(iIndex)
	{
		typeCode = LuaTypeCodes::kLocalVar;
		pluginId = iPluginId;

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(name) // name
			+ kSizeOfint16_t // name type
			+ kSizeOfuint16_t + (int)std::strlen(value) // value
			+ kSizeOfint16_t // value type
			+ kSizeOfint16_t // stack level
			+ kSizeOfint32_t // index		
			+ kSizeOfuint16_t; // # of parent and key-value-pairs

		const uint16_t extraOffset = parent == NULL ? 0 : parent->bFlag ? 1 : 0;

		if (pParent != NULL)
		{
			length += (kSizeOfuint16_t + (int)std::strlen(pParent->name));
			length += kSizeOfint16_t; // name type

			for (int i = 0; i < pParent->numKeyValues - extraOffset; ++i)
			{
				length += (kSizeOfuint16_t + (int)std::strlen(pParent->hKeyValues[i].name));
				length += kSizeOfint16_t; // name type
			}			
		}

		if (pBuffer)
			pack(pBuffer);
	}

	void LocalVar::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);

		packer.packString(name);
		packer.packInt16_t(nameType);
		packer.packString(value);
		packer.packInt16_t(valueType);

		packer.packInt16_t(stackLevel);
		packer.packInt32_t(index);

		const uint16_t extraOffset = parent == NULL ? 0 : parent->bFlag ? 1 : 0;

		if (parent == NULL)
		{
			packer.packUInt16_t(0);
		}
		else
		{
			packer.packUInt16_t(parent->numKeyValues + 1 - extraOffset);

			packer.packString(parent->name);
			packer.packInt16_t((int16_t)parent->nameType);
			for (int i = 0; i < parent->numKeyValues - extraOffset; ++i)
			{
				packer.packString(parent->hKeyValues[i].name);
				packer.packInt16_t((int16_t)parent->hKeyValues[i].type);
			}
		}
	}

	UpvalueVar::UpvalueVar(uint16_t iPluginId, const LuaVariable *pParent, const char *pszName, int16_t iNameType, const char *pszValue, int16_t iValueType, int16_t iStackLevel, int32_t iIndex, NetworkBuffer *pBuffer /* = 0 */)
		: parent(pParent)
		, name(pszName)
		, nameType(iNameType)
		, value(pszValue)
		, valueType(iValueType)
		, stackLevel(iStackLevel)
		, index(iIndex)
	{
		typeCode = LuaTypeCodes::kUpvalueVar;
		pluginId = iPluginId;

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(name) // name
			+ kSizeOfint16_t // name type
			+ kSizeOfuint16_t + (int)std::strlen(value) // value
			+ kSizeOfint16_t // value type
			+ kSizeOfint16_t // stack level
			+ kSizeOfint32_t // index		
			+ kSizeOfuint16_t; // # of parent and key-value-pairs

		const uint16_t extraOffset = parent == NULL ? 0 : parent->bFlag ? 1 : 0;

		if (pParent != NULL)
		{
			length += (kSizeOfuint16_t + (int)std::strlen(pParent->name));
			length += kSizeOfint16_t; // name type

			for (int i = 0; i < pParent->numKeyValues - extraOffset; ++i)
			{
				length += (kSizeOfuint16_t + (int)std::strlen(pParent->hKeyValues[i].name));
				length += kSizeOfint16_t; // name type
			}			
		}

		if (pBuffer)
			pack(pBuffer);
	}

	void UpvalueVar::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);

		packer.packString(name);
		packer.packInt16_t(nameType);
		packer.packString(value);
		packer.packInt16_t(valueType);

		packer.packInt16_t(stackLevel);
		packer.packInt32_t(index);

		const uint16_t extraOffset = parent == NULL ? 0 : parent->bFlag ? 1 : 0;

		if (parent == NULL)
		{
			packer.packUInt16_t(0);
		}
		else
		{
			packer.packUInt16_t(parent->numKeyValues + 1 - extraOffset);

			packer.packString(parent->name);
			packer.packInt16_t((int16_t)parent->nameType);
			for (int i = 0; i < parent->numKeyValues - extraOffset; ++i)
			{
				packer.packString(parent->hKeyValues[i].name);
				packer.packInt16_t((int16_t)parent->hKeyValues[i].type);
			}
		}
	}

	EnvVar::EnvVar(uint16_t iPluginId, const LuaVariable *pParent, const char *pszName, int16_t iNameType, const char *pszValue, int16_t iValueType, int16_t iStackLevel, NetworkBuffer *pBuffer /* = 0 */)
		: parent(pParent)
		, name(pszName)
		, nameType(iNameType)
		, value(pszValue)
		, valueType(iValueType)
		, stackLevel(iStackLevel)
	{
		typeCode = LuaTypeCodes::kEnvVar;
		pluginId = iPluginId;

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(name) // name
			+ kSizeOfint16_t // name type
			+ kSizeOfuint16_t + (int)std::strlen(value) // value
			+ kSizeOfint16_t // value type
			+ kSizeOfint16_t // stack level
			+ kSizeOfuint16_t; // # of parent and key-value-pairs

		const uint16_t extraOffset = parent == NULL ? 0 : parent->bFlag ? 1 : 0;

		if (pParent != NULL)
		{
			length += (kSizeOfuint16_t + (int)std::strlen(pParent->name));
			length += kSizeOfint16_t; // name type

			for (int i = 0; i < pParent->numKeyValues - extraOffset; ++i)
			{
				length += (kSizeOfuint16_t + (int)std::strlen(pParent->hKeyValues[i].name));
				length += kSizeOfint16_t; // name type
			}			
		}

		if (pBuffer)
			pack(pBuffer);
	}

	void EnvVar::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);

		packer.packString(name);
		packer.packInt16_t(nameType);
		packer.packString(value);
		packer.packInt16_t(valueType);

		packer.packInt16_t(stackLevel);

		const uint16_t extraOffset = parent == NULL ? 0 : parent->bFlag ? 1 : 0;

		if (parent == NULL)
		{
			packer.packUInt16_t(0);
		}
		else
		{
			packer.packUInt16_t(parent->numKeyValues + 1 - extraOffset);

			packer.packString(parent->name);
			packer.packInt16_t((int16_t)parent->nameType);
			for (int i = 0; i < parent->numKeyValues - extraOffset; ++i)
			{
				packer.packString(parent->hKeyValues[i].name);
				packer.packInt16_t((int16_t)parent->hKeyValues[i].type);
			}
		}
	}

	VarLookUp::VarLookUp(uint16_t iPluginId)
	{
		typeCode = LuaTypeCodes::kVarLookUp;
		pluginId = iPluginId;
		length = kSizeOfBase + sizeof(LuaVariable) + kSizeOfuint8_t;
		SCE_SLED_ASSERT_MSG(this == NULL, "this constructor should never be used");	
	}

	void VarLookUp::unpack(NetworkBufferReader *reader, uint8_t *scratchBuffer, uint16_t scratchBufferMaxSize)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();

		variable.what = (LuaVariableScope::Enum)reader->readUInt8_t();
		variable.context = (LuaVariableContext::Enum)reader->readUInt8_t();
		variable.numKeyValues = reader->readUInt16_t() - 1;
		variable.level = reader->readInt16_t();
		variable.index = reader->readInt32_t();
		extra = reader->readUInt8_t();

		std::size_t position = 0;
		reader->readString((char*)(scratchBuffer + position), scratchBufferMaxSize);
	
		variable.name = (char*)(scratchBuffer + position);
		variable.nameType = reader->readUInt16_t();

		position += (std::strlen(variable.name) + 1);

		for (int i = 0; i < variable.numKeyValues; ++i)
		{
			reader->readString((char*)(scratchBuffer + position), scratchBufferMaxSize - (uint16_t)position);
		
			variable.hKeyValues[i].name = (char*)(scratchBuffer + position);
			variable.hKeyValues[i].type = reader->readUInt16_t();

			position += (std::strlen(variable.hKeyValues[i].name) + 1);
		}
	}

	VarUpdate::VarUpdate(uint16_t iPluginId)
	{
		typeCode = LuaTypeCodes::kVarUpdate;
		pluginId = iPluginId;
		length = kSizeOfBase + sizeof(LuaVariable);
		SCE_SLED_ASSERT_MSG(this == NULL, "this constructor should never be used");
	}

	void VarUpdate::unpack(NetworkBufferReader *reader, uint8_t *scratchBuffer, uint16_t scratchBufferMaxSize)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();

		variable.what = (LuaVariableScope::Enum)reader->readUInt8_t();
		variable.context = (LuaVariableContext::Enum)reader->readUInt8_t();
		variable.numKeyValues = reader->readUInt16_t() - 1;
		variable.level = reader->readInt16_t();
		variable.index = reader->readInt32_t();

		std::size_t position = 0;

		reader->readString((char*)(scratchBuffer + position), scratchBufferMaxSize);
	
		variable.value = (char*)(scratchBuffer + position);
		variable.valueType = reader->readInt16_t();

		position += (std::strlen(variable.value) + 1);
		reader->readString((char*)(scratchBuffer + position), scratchBufferMaxSize - (uint16_t)position);

		variable.name = (char*)(scratchBuffer + position);
		variable.nameType = reader->readUInt16_t();

		position += (std::strlen(variable.name) + 1);

		for (int i = 0; i < variable.numKeyValues; ++i)
		{
			reader->readString((char*)(scratchBuffer + position), scratchBufferMaxSize - (uint16_t)position);

			variable.hKeyValues[i].name = (char*)(scratchBuffer + position);
			variable.hKeyValues[i].type = reader->readUInt16_t();

			position += (std::strlen(variable.hKeyValues[i].name) + 1);
		}

		variable.bTable = variable.numKeyValues > 0;
	}

	CallStack::CallStack(uint16_t iPluginId, const char *pszScriptFile, int32_t iCurrentLine, int32_t iLineDefined, int32_t iLastLineDefined, const char *pszFuncName, int16_t iStackLevel, NetworkBuffer *pBuffer /* = 0 */)
	{	
		typeCode = LuaTypeCodes::kCallStack;
		pluginId = iPluginId;

		Utilities::copyString(relScriptPath, kStringLen, pszScriptFile);
		currentLine = iCurrentLine;
		lineDefined = iLineDefined;
		lastLineDefined = iLastLineDefined;
		Utilities::copyString(functionName, kStringLen, pszFuncName);
		stackLevel = iStackLevel;

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(relScriptPath)
			+ kSizeOfint32_t
			+ kSizeOfint32_t
			+ kSizeOfint32_t
			+ kSizeOfuint16_t + (int)std::strlen(functionName)
			+ kSizeOfint16_t;

		if (pBuffer)
			pack(pBuffer);
	}

	void CallStack::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packString(relScriptPath);
		packer.packInt32_t(currentLine);
		packer.packInt32_t(lineDefined);
		packer.packInt32_t(lastLineDefined);
		packer.packString(functionName);
		packer.packInt16_t(stackLevel);
	}

	CallStackLookUpPerform::CallStackLookUpPerform(uint16_t iPluginId, int16_t iStackLevel)
	{
		length = kSizeOfBase + kSizeOfint16_t;
		typeCode = LuaTypeCodes::kCallStackLookUpPerform;
		pluginId = iPluginId;

		stackLevel = iStackLevel;
	}

	void CallStackLookUpPerform::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		stackLevel = reader->readInt16_t();
	}

	CallStackLookUp::CallStackLookUp(uint16_t iPluginId, const char *pszFuncName, int32_t iLineDefined, int16_t iStackLevel, NetworkBuffer *pBuffer /* = 0 */)
	{	
		typeCode = LuaTypeCodes::kCallStackLookUp;
		pluginId = iPluginId;

		Utilities::copyString(functionName, kStringLen, pszFuncName);
		lineDefined = iLineDefined;
		stackLevel = iStackLevel;

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(functionName)
			+ kSizeOfint32_t
			+ kSizeOfint16_t;

		if (pBuffer)
			pack(pBuffer);
	}

	void CallStackLookUp::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packString(functionName);
		packer.packInt32_t(lineDefined);
		packer.packInt16_t(stackLevel);
	}

	WatchLookUpBegin::WatchLookUpBegin(uint16_t iPluginId, LuaVariableScope::Enum eWhat, NetworkBuffer *pBuffer /* = 0 */)
	{
		length = kSizeOfBase + kSizeOfuint8_t;
		typeCode = LuaTypeCodes::kWatchLookUpBegin;
		pluginId = iPluginId;

		what = eWhat;

		if (pBuffer)
			pack(pBuffer);
	}

	void WatchLookUpBegin::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		what = (LuaVariableScope::Enum)reader->readUInt8_t();
	}

	void WatchLookUpBegin::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packUInt8_t((uint8_t)what);
	}

	WatchLookUpEnd::WatchLookUpEnd(uint16_t iPluginId, LuaVariableScope::Enum eWhat, NetworkBuffer *pBuffer /* = 0 */)
	{
		length = kSizeOfBase + kSizeOfuint8_t;
		typeCode = LuaTypeCodes::kWatchLookUpEnd;
		pluginId = iPluginId;

		what = eWhat;

		if (pBuffer)
			pack(pBuffer);
	}

	void WatchLookUpEnd::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		what = (LuaVariableScope::Enum)reader->readUInt8_t();
	}

	void WatchLookUpEnd::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packUInt8_t((uint8_t)what);
	}

	LuaStateAdd::LuaStateAdd(uint16_t iPluginId, const char *pszAddress, const char *pszName, bool bDebugging, NetworkBuffer *pBuffer /* = 0 */)
	{
		typeCode = LuaTypeCodes::kLuaStateAdd;
		pluginId = iPluginId;

		Utilities::copyString(address, Sizes::kPtrLen, pszAddress);
		Utilities::copyString(name, kNameLen, pszName);
		debugging = (bDebugging ? 1 : 0);

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(address)
			+ kSizeOfuint16_t + (int)std::strlen(name)
			+ kSizeOfuint8_t;

		if (pBuffer)
			pack(pBuffer);
	}

	void LuaStateAdd::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packString(address);
		packer.packString(name);
		packer.packUInt8_t(debugging);
	}

	LuaStateRemove::LuaStateRemove(uint16_t iPluginId, const char *pszAddress, NetworkBuffer *pBuffer /* = 0 */)
	{
		typeCode = LuaTypeCodes::kLuaStateRemove;
		pluginId = iPluginId;

		Utilities::copyString(address, Sizes::kPtrLen, pszAddress);

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(address);

		if (pBuffer)
			pack(pBuffer);
	}

	void LuaStateRemove::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packString(address);
	}

	LuaStateToggle::LuaStateToggle(uint16_t iPluginId, const char *pszAddress)
	{
		typeCode = LuaTypeCodes::kLuaStateToggle;
		pluginId = iPluginId;

		Utilities::copyString(address, Sizes::kPtrLen, pszAddress);

		length = kSizeOfBase
			+ kSizeOfuint16_t + (int)std::strlen(address);
	}

	void LuaStateToggle::unpack(NetworkBufferReader *reader)
	{
		length = reader->readInt32_t();
		typeCode = reader->readUInt16_t();
		pluginId = reader->readUInt16_t();
		reader->readString(address, Sizes::kPtrLen);
	}

	Limits::Limits(uint16_t iPluginId, uint16_t iMaxBreakpoints, uint16_t iMaxVarFilters, bool bProfilerEnabled, bool bMemoryTracerEnabled, NetworkBuffer *pBuffer /* = 0 */)
	{
		typeCode = LuaTypeCodes::kLimits;
		pluginId = iPluginId;

		maxBreakpoints = iMaxBreakpoints;
		maxVarFilters = iMaxVarFilters;
		profilerEnabled = bProfilerEnabled ? 1 : 0;
		memoryTracerEnabled = bMemoryTracerEnabled ? 1 : 0;

		length = kSizeOfBase
			+ (kSizeOfuint16_t * 2)
			+ (kSizeOfuint8_t * 2);

		if (pBuffer)
			pack(pBuffer);
	}

	void Limits::pack(NetworkBuffer *pBuffer)
	{
		NetworkBufferPacker packer(pBuffer);

		packer.packInt32_t(length);
		packer.packUInt16_t(typeCode);
		packer.packUInt16_t(pluginId);
		packer.packUInt16_t(maxBreakpoints);
		packer.packUInt16_t(maxVarFilters);
		packer.packUInt8_t(profilerEnabled);
		packer.packUInt8_t(memoryTracerEnabled);
	}
}}}
