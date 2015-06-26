/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDLUAPLUGIN_SCMP_H__
#define __SCE_LIBSLEDLUAPLUGIN_SCMP_H__

#include "../sleddebugger/scmp.h"
#include "luavariable.h"

#include <cstddef>

#include "../sleddebugger/common.h"

namespace sce { namespace Sled { namespace SCMP
{
	namespace LuaTypeCodes
	{
		enum TypeCodes
		{
			kMemoryTraceBegin = 200,
			kMemoryTrace = 201,
			kMemoryTraceEnd = 202,
			kMemoryTraceStreamBegin = 203,
			kMemoryTraceStream = 204,
			kMemoryTraceStreamEnd = 205,

			kProfileInfoBegin = 207,
			kProfileInfo = 208,
			kProfileInfoEnd = 209,
			kProfileInfoLookUpPerform = 210,
			kProfileInfoLookUpBegin = 211,
			kProfileInfoLookUp = 212,
			kProfileInfoLookUpEnd = 213,

			kVarFilterStateTypeBegin = 214,
			kVarFilterStateType = 215,
			kVarFilterStateTypeEnd = 216,
			kVarFilterStateNameBegin = 217,
			kVarFilterStateName = 218,
			kVarFilterStateNameEnd = 219,

			kGlobalVarBegin = 220,
			kGlobalVar = 221,
			kGlobalVarEnd = 222,
			kGlobalVarLookUpBegin = 223,
			kGlobalVarLookUpEnd = 224,

			kLocalVarBegin = 230,
			kLocalVar = 231,
			kLocalVarEnd = 232,
			kLocalVarLookUpBegin = 233,
			kLocalVarLookUpEnd = 234,

			kUpvalueVarBegin = 240,
			kUpvalueVar = 241,
			kUpvalueVarEnd = 242,
			kUpvalueVarLookUpBegin = 243,
			kUpvalueVarLookUpEnd = 244,

			kEnvVarBegin = 250,
			kEnvVar = 251,
			kEnvVarEnd = 252,
			kEnvVarLookUpBegin = 253,
			kEnvVarLookUpEnd = 254,

			kVarLookUp = 255,
			kVarUpdate = 256,

			kCallStackBegin = 260,
			kCallStack = 261,
			kCallStackEnd = 262,
			kCallStackLookUpPerform = 263,
			kCallStackLookUpBegin = 264,
			kCallStackLookUp = 265,
			kCallStackLookUpEnd = 266,

			kWatchLookUpBegin = 270,
			kWatchLookUpEnd = 271,
			kWatchLookUpClear = 272,

			kWatchLookUpProjectBegin = 280,
			kWatchLookUpProjectEnd = 281,
			kWatchLookUpCustomBegin = 282,
			kWatchLookUpCustomEnd = 283,

			kLuaStateBegin = 290,
			kLuaStateAdd = 291,
			kLuaStateRemove = 292,
			kLuaStateEnd = 293,
			kLuaStateToggle = 294,

			kMemoryTraceToggle = 300,
			kProfilerToggle = 301,

			kLimits = 310,
		};
	}
	
	namespace Sizes
	{
		static const uint16_t kPtrLen = 32;
		static const uint16_t kVarNameLen = 256;
		static const uint16_t kVarValueLen = 256;
		static const uint16_t kVarKeyValueLen = 128;
	}

	struct SCE_SLED_LINKAGE MemoryTraceBegin : public Sled::SCMP::Base
	{
		MemoryTraceBegin(uint16_t iPluginId)
		{
			length = sizeof(MemoryTraceBegin);
			typeCode = LuaTypeCodes::kMemoryTraceBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE MemoryTrace : public Sled::SCMP::Base
	{
		MemoryTrace(uint16_t iPluginId, char chWhat, void *pOldPtr, void *pNewPtr, std::size_t iOldSize, std::size_t iNewSize, NetworkBuffer *pBuffer = 0);
		void pack(NetworkBuffer *pBuffer);	

		uint8_t		what;
		char		oldPtr[Sizes::kPtrLen];
		char		newPtr[Sizes::kPtrLen];
		int32_t		oldSize;
		int32_t		newSize;
	};

	struct SCE_SLED_LINKAGE MemoryTraceEnd : public Sled::SCMP::Base
	{
		MemoryTraceEnd(uint16_t iPluginId)
		{
			length = sizeof(MemoryTraceEnd);
			typeCode = LuaTypeCodes::kMemoryTraceEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE MemoryTraceStreamBegin : public Sled::SCMP::Base
	{
		MemoryTraceStreamBegin(uint16_t iPluginId)
		{
			length = sizeof(MemoryTraceStreamBegin);
			typeCode = LuaTypeCodes::kMemoryTraceStreamBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE MemoryTraceStream : public Sled::SCMP::Base
	{
		MemoryTraceStream(uint16_t iPluginId, char chWhat, void *pOldPtr, void *pNewPtr, std::size_t iOldSize, std::size_t iNewSize, NetworkBuffer *pBuffer = 0);
		void pack(NetworkBuffer *pBuffer);

		uint8_t		what;
		char		oldPtr[Sizes::kPtrLen];
		char		newPtr[Sizes::kPtrLen];
		int32_t		oldSize;
		int32_t		newSize;
	};

	struct SCE_SLED_LINKAGE MemoryTraceStreamEnd : public Sled::SCMP::Base
	{
		MemoryTraceStreamEnd(uint16_t iPluginId)
		{
			length = sizeof(MemoryTraceStreamEnd);
			typeCode = LuaTypeCodes::kMemoryTraceStreamEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE ProfileInfoBegin : public Sled::SCMP::Base
	{
		ProfileInfoBegin(uint16_t iPluginId)
		{
			length = sizeof(ProfileInfoBegin);
			typeCode = LuaTypeCodes::kProfileInfoBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE ProfileInfo : public Sled::SCMP::Base
	{
		ProfileInfo(uint16_t iPluginId, const char *pszFuncName, const char *pszRelScriptPath,
			float flFnTimeElapsed, float flFnTimeElapsedAvg, float flFnTimeElapsedShortest, float flFnTimeElapsedLongest,
			float flFnTimeInnerElapsed, float flFnTimeInnerElapsedAvg, float flFnTimeInnerElapsedShortest, float flFnTimeInnerElapsedLongest,
			uint32_t iFnCallCount, int32_t iFnLine, int32_t iFnCalls, NetworkBuffer *pBuffer = 0);
		void pack(NetworkBuffer *pBuffer);

		char		functionName[kStringLen];
		char		relScriptPath[kStringLen];
		float		fnTimeElapsed;
		float		fnTimeElapsedAvg;
		float		fnTimeElapsedShortest;
		float		fnTimeElapsedLongest;
		float		fnTimeInnerElapsed;
		float		fnTimeInnerElapsedAvg;
		float		fnTimeInnerElapsedShortest;
		float		fnTimeInnerElapsedLongest;
		uint32_t	fnCallCount;
		int32_t		fnLine;
		int32_t		fnCalls;
	};

	struct SCE_SLED_LINKAGE ProfileInfoEnd : public Sled::SCMP::Base
	{
		ProfileInfoEnd(uint16_t iPluginId)
		{
			length = sizeof(ProfileInfoEnd);
			typeCode = LuaTypeCodes::kProfileInfoEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE ProfileInfoLookUpPerform : public Sled::SCMP::Base
	{
		ProfileInfoLookUpPerform(uint16_t iPluginId, const char *pszFuncName, char chWhat, int32_t iLine, const char *pszRelScriptPath);
		ProfileInfoLookUpPerform(NetworkBufferReader *reader) { unpack(reader); }
		void unpack(NetworkBufferReader *reader);	

		char		functionName[kStringLen];
		uint8_t		what;
		int32_t		line;
		char		relScriptPath[kStringLen];
	};

	struct SCE_SLED_LINKAGE ProfileInfoLookUpBegin : public Sled::SCMP::Base
	{
		ProfileInfoLookUpBegin(uint16_t iPluginId)
		{
			length = sizeof(ProfileInfoLookUpBegin);
			typeCode = LuaTypeCodes::kProfileInfoLookUpBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE ProfileInfoLookUp : public Sled::SCMP::Base
	{
		ProfileInfoLookUp(uint16_t iPluginId, const char *pszFuncName, const char *pszRelScriptPath,
			float flFnTimeElapsed,  float flFnTimeElapsedAvg, float flFnTimeElapsedShortest, float flFnTimeElapsedLongest,
			float flFnTimeInnerElapsed, float flFnTimeInnerElapsedAvg, float flFnTimeInnerElapsedShortest, float flFnTimeInnerElapsedLongest,
			uint32_t iFnCallCount, int32_t iFnLine, int32_t iFnCalls, NetworkBuffer *pBuffer = 0);
		void pack(NetworkBuffer *pBuffer);	

		char		functionName[kStringLen];
		char		relScriptPath[kStringLen];
		float		fnTimeElapsed;
		float		fnTimeElapsedAvg;
		float		fnTimeElapsedShortest;
		float		fnTimeElapsedLongest;
		float		fnTimeInnerElapsed;
		float		fnTimeInnerElapsedAvg;
		float		fnTimeInnerElapsedShortest;
		float		fnTimeInnerElapsedLongest;
		uint32_t	fnCallCount;
		int32_t		fnLine;
		int32_t		fnCalls;
	};

	struct SCE_SLED_LINKAGE ProfileInfoLookUpEnd : public Sled::SCMP::Base
	{
		ProfileInfoLookUpEnd(uint16_t iPluginId)
		{
			length = sizeof(ProfileInfoLookUpEnd);
			typeCode = LuaTypeCodes::kProfileInfoLookUpEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE VarFilterStateTypeBegin : public Sled::SCMP::Base
	{
		VarFilterStateTypeBegin(uint16_t iPluginId, char chWhat);
		VarFilterStateTypeBegin(NetworkBufferReader *reader) { unpack(reader); }
		void unpack(NetworkBufferReader *reader);

		uint8_t		what;
	};

	struct SCE_SLED_LINKAGE VarFilterStateType : public Sled::SCMP::Base
	{
		VarFilterStateType(uint16_t iPluginId, char chWhat, const uint8_t *pFilter);	
		VarFilterStateType(NetworkBufferReader *reader) { unpack(reader); }
		void unpack(NetworkBufferReader *reader);	

		uint8_t		what;
		uint8_t		filter[9];
	};

	struct SCE_SLED_LINKAGE VarFilterStateTypeEnd : public Sled::SCMP::Base
	{
		VarFilterStateTypeEnd(uint16_t iPluginId, char chWhat);
		VarFilterStateTypeEnd(NetworkBufferReader *reader) { unpack(reader); }
		void unpack(NetworkBufferReader *reader);	

		uint8_t		what;
	};

	struct SCE_SLED_LINKAGE VarFilterStateNameBegin : public Sled::SCMP::Base
	{
		VarFilterStateNameBegin(uint16_t iPluginId, char chWhat);
		VarFilterStateNameBegin(NetworkBufferReader *reader) { unpack(reader); }
		void unpack(NetworkBufferReader *reader);
	
		uint8_t		what;
	};

	struct SCE_SLED_LINKAGE VarFilterStateName : public Sled::SCMP::Base
	{
		VarFilterStateName(uint16_t iPluginId, char chWhat, const char *pszFilter);	
		VarFilterStateName(NetworkBufferReader *reader) { unpack(reader); }
		void unpack(NetworkBufferReader *reader);	

		uint8_t		what;
		char		filter[kStringLen];
	};

	struct SCE_SLED_LINKAGE VarFilterStateNameEnd : public Sled::SCMP::Base
	{
		VarFilterStateNameEnd(uint16_t iPluginId, char chWhat);	
		VarFilterStateNameEnd(NetworkBufferReader *reader) { unpack(reader); }
		void unpack(NetworkBufferReader *reader);
	
		uint8_t		what;
	};

	struct SCE_SLED_LINKAGE GlobalVarBegin : public Sled::SCMP::Base
	{
		GlobalVarBegin(uint16_t iPluginId)
		{
			length = sizeof(GlobalVarBegin);
			typeCode = LuaTypeCodes::kGlobalVarBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE GlobalVar : public Sled::SCMP::Base
	{
		GlobalVar(uint16_t iPluginId, const LuaVariable *pParent, const char *pzsName, int16_t iNameType, const char *pszValue, int16_t iValueType, NetworkBuffer *pBuffer = 0);
		void pack(NetworkBuffer *pBuffer);	

		const LuaVariable *parent;

		const char	*name;
		int16_t		nameType;

		const char	*value;
		int16_t		valueType;
	};

	struct SCE_SLED_LINKAGE GlobalVarEnd : public Sled::SCMP::Base
	{
		GlobalVarEnd(uint16_t iPluginId)
		{
			length = sizeof(GlobalVarEnd);
			typeCode = LuaTypeCodes::kGlobalVarEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE GlobalVarLookUpBegin : public Sled::SCMP::Base
	{
		GlobalVarLookUpBegin(uint16_t iPluginId)
		{
			length = sizeof(GlobalVarLookUpBegin);
			typeCode = LuaTypeCodes::kGlobalVarLookUpBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE GlobalVarLookUpEnd : public Sled::SCMP::Base
	{
		GlobalVarLookUpEnd(uint16_t iPluginId)
		{
			length = sizeof(GlobalVarLookUpEnd);
			typeCode = LuaTypeCodes::kGlobalVarLookUpEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE LocalVarBegin : public Sled::SCMP::Base
	{
		LocalVarBegin(uint16_t iPluginId)
		{
			length = sizeof(LocalVarBegin);
			typeCode = LuaTypeCodes::kLocalVarBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE LocalVar : public Sled::SCMP::Base
	{
		LocalVar(uint16_t iPluginId, const LuaVariable *pParent, const char *pszName, int16_t iNameType, const char *pszValue, int16_t iValueType, int16_t iStackLevel, int32_t iIndex, NetworkBuffer *pBuffer = 0);	
		void pack(NetworkBuffer *pBuffer);	

		const LuaVariable *parent;

		const char	*name;
		int16_t		nameType;

		const char	*value;
		int16_t		valueType;

		int16_t		stackLevel;
		int32_t		index;	
	};

	struct SCE_SLED_LINKAGE LocalVarEnd : public Sled::SCMP::Base
	{
		LocalVarEnd(uint16_t iPluginId)
		{
			length = sizeof(LocalVarEnd);
			typeCode = LuaTypeCodes::kLocalVarEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE LocalVarLookUpBegin : public Sled::SCMP::Base
	{
		LocalVarLookUpBegin(uint16_t iPluginId)
		{
			length = sizeof(LocalVarLookUpBegin);
			typeCode = LuaTypeCodes::kLocalVarLookUpBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE LocalVarLookUpEnd : public Sled::SCMP::Base
	{
		LocalVarLookUpEnd(uint16_t iPluginId)
		{
			length = sizeof(LocalVarLookUpEnd);
			typeCode = LuaTypeCodes::kLocalVarLookUpEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE UpvalueVarBegin : public Sled::SCMP::Base
	{
		UpvalueVarBegin(uint16_t iPluginId)
		{
			length = sizeof(UpvalueVarBegin);
			typeCode = LuaTypeCodes::kUpvalueVarBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE UpvalueVar : public Sled::SCMP::Base
	{
		UpvalueVar(uint16_t iPluginId, const LuaVariable *pParent, const char *pszName, int16_t iNameType, const char *pszValue, int16_t iValueType, int16_t iStackLevel, int32_t iIndex, NetworkBuffer *pBuffer = 0);
		void pack(NetworkBuffer *pBuffer);	

		const LuaVariable *parent;

		const char	*name;
		int16_t		nameType;

		const char	*value;
		int16_t		valueType;

		int16_t		stackLevel;
		int32_t		index;	
	};

	struct SCE_SLED_LINKAGE UpvalueVarEnd : public Sled::SCMP::Base
	{
		UpvalueVarEnd(uint16_t iPluginId)
		{
			length = sizeof(UpvalueVarEnd);
			typeCode = LuaTypeCodes::kUpvalueVarEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE UpvalueVarLookUpBegin : public Sled::SCMP::Base
	{
		UpvalueVarLookUpBegin(uint16_t iPluginId)
		{
			length = sizeof(UpvalueVarLookUpBegin);
			typeCode = LuaTypeCodes::kUpvalueVarLookUpBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE UpvalueVarLookUpEnd : public Sled::SCMP::Base
	{
		UpvalueVarLookUpEnd(uint16_t iPluginId)
		{
			length = sizeof(UpvalueVarLookUpEnd);
			typeCode = LuaTypeCodes::kUpvalueVarLookUpEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE EnvVarBegin : public Sled::SCMP::Base
	{
		EnvVarBegin(uint16_t iPluginId)
		{
			length = sizeof(EnvVarBegin);
			typeCode = LuaTypeCodes::kEnvVarBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE EnvVar : public Sled::SCMP::Base
	{
		EnvVar(uint16_t iPluginId, const LuaVariable *pParent, const char *pszName, int16_t iNameType, const char *pszValue, int16_t iValueType, int16_t iStackLevel, NetworkBuffer *pBuffer = 0);
		void pack(NetworkBuffer *pBuffer);	

		const LuaVariable *parent;

		const char	*name;
		int16_t		nameType;

		const char	*value;
		int16_t		valueType;

		int16_t		stackLevel;	
	};

	struct SCE_SLED_LINKAGE EnvVarEnd : public Sled::SCMP::Base
	{
		EnvVarEnd(uint16_t iPluginId)
		{
			length = sizeof(EnvVarEnd);
			typeCode = LuaTypeCodes::kEnvVarEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE EnvVarLookUpBegin : public Sled::SCMP::Base
	{
		EnvVarLookUpBegin(uint16_t iPluginId)
		{
			length = sizeof(EnvVarLookUpBegin);
			typeCode = LuaTypeCodes::kEnvVarLookUpBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE EnvVarLookUpEnd : public Sled::SCMP::Base
	{
		EnvVarLookUpEnd(uint16_t iPluginId)
		{
			length = sizeof(EnvVarLookUpEnd);
			typeCode = LuaTypeCodes::kEnvVarLookUpEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE VarLookUp : public Sled::SCMP::Base
	{
		VarLookUp(uint16_t iPluginId);
		VarLookUp(NetworkBufferReader *reader, uint8_t *scratchBuffer, uint16_t scratchBufferMaxSize) { unpack(reader, scratchBuffer, scratchBufferMaxSize); }
		void unpack(NetworkBufferReader *reader, uint8_t *scratchBuffer, uint16_t scratchBufferMaxSize);

		LuaVariable	variable;
		uint8_t		extra;
	};

	struct SCE_SLED_LINKAGE VarUpdate : public Sled::SCMP::Base
	{
		VarUpdate(uint16_t iPluginId);
		VarUpdate(NetworkBufferReader *reader, uint8_t *scratchBuffer, uint16_t scratchBufferMaxSize) { unpack(reader, scratchBuffer, scratchBufferMaxSize); }
		void unpack(NetworkBufferReader *reader, uint8_t *scratchBuffer, uint16_t scratchBufferMaxSize);	

		LuaVariable variable;
	};

	struct SCE_SLED_LINKAGE CallStackBegin : public Sled::SCMP::Base
	{
		CallStackBegin(uint16_t iPluginId)
		{
			length = sizeof(CallStackBegin);
			typeCode = LuaTypeCodes::kCallStackBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE CallStack : public Sled::SCMP::Base
	{
		CallStack(uint16_t iPluginId, const char *pszScriptFile, int32_t iCurrentLine, int32_t iLineDefined, int32_t iLastLineDefined, const char *pszFuncName, int16_t iStackLevel, NetworkBuffer *pBuffer = 0);
		void pack(NetworkBuffer *pBuffer);	

		char		relScriptPath[kStringLen];
		int32_t		currentLine;
		int32_t		lineDefined;
		int32_t		lastLineDefined;
		char		functionName[kStringLen];
		int16_t		stackLevel;
	};

	struct SCE_SLED_LINKAGE CallStackEnd : public Sled::SCMP::Base
	{
		CallStackEnd(uint16_t iPluginId)
		{
			length = sizeof(CallStackEnd);
			typeCode = LuaTypeCodes::kCallStackEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE CallStackLookUpPerform : public Sled::SCMP::Base
	{
		CallStackLookUpPerform(uint16_t iPluginId, int16_t iStackLevel);	
		CallStackLookUpPerform(NetworkBufferReader *reader) { unpack(reader); }
		void unpack(NetworkBufferReader *reader);	

		int16_t		stackLevel;
	};

	struct SCE_SLED_LINKAGE CallStackLookUpBegin : public Sled::SCMP::Base
	{
		CallStackLookUpBegin(uint16_t iPluginId)
		{
			length = sizeof(CallStackLookUpBegin);
			typeCode = LuaTypeCodes::kCallStackLookUpBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE CallStackLookUp : public Sled::SCMP::Base
	{
		CallStackLookUp(uint16_t iPluginId, const char *pszFuncName, int32_t iLineDefined, int16_t iStackLevel, NetworkBuffer *pBuffer = 0);
		void pack(NetworkBuffer *pBuffer);	

		char		functionName[kStringLen];
		int32_t		lineDefined;
		int16_t		stackLevel;
	};

	struct SCE_SLED_LINKAGE CallStackLookUpEnd : public Sled::SCMP::Base
	{
		CallStackLookUpEnd(uint16_t iPluginId)
		{
			length = sizeof(CallStackLookUpEnd);
			typeCode = LuaTypeCodes::kCallStackLookUpEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE WatchLookUpBegin : public Sled::SCMP::Base
	{
		WatchLookUpBegin(uint16_t iPluginId, LuaVariableScope::Enum eWhat, NetworkBuffer *pBuffer = 0);
		WatchLookUpBegin(NetworkBufferReader *reader) { unpack(reader); }
		void unpack(NetworkBufferReader *reader);
		void pack(NetworkBuffer *pBuffer);	
	
		LuaVariableScope::Enum	what;
	};

	struct SCE_SLED_LINKAGE WatchLookUpEnd : public Sled::SCMP::Base
	{
		WatchLookUpEnd(uint16_t iPluginId, LuaVariableScope::Enum eWhat, NetworkBuffer *pBuffer = 0);
		WatchLookUpEnd(NetworkBufferReader *reader) { unpack(reader); }
		void unpack(NetworkBufferReader *reader);
		void pack(NetworkBuffer *pBuffer);	

		LuaVariableScope::Enum	what;
	};

	struct SCE_SLED_LINKAGE WatchLookUpClear : public Sled::SCMP::Base
	{
		WatchLookUpClear(uint16_t iPluginId)
		{
			length = sizeof(WatchLookUpClear);
			typeCode = LuaTypeCodes::kWatchLookUpClear;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE WatchLookUpProjectBegin : public Sled::SCMP::Base
	{
		WatchLookUpProjectBegin(uint16_t iPluginId)
		{
			length = sizeof(WatchLookUpProjectBegin);
			typeCode = LuaTypeCodes::kWatchLookUpProjectBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE WatchLookUpProjectEnd : public Sled::SCMP::Base
	{
		WatchLookUpProjectEnd(uint16_t iPluginId)
		{
			length = sizeof(WatchLookUpProjectEnd);
			typeCode = LuaTypeCodes::kWatchLookUpProjectEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE WatchLookUpCustomBegin : public Sled::SCMP::Base
	{
		WatchLookUpCustomBegin(uint16_t iPluginId)
		{
			length = sizeof(WatchLookUpCustomBegin);
			typeCode = LuaTypeCodes::kWatchLookUpCustomBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE WatchLookUpCustomEnd : public Sled::SCMP::Base
	{
		WatchLookUpCustomEnd(uint16_t iPluginId)
		{
			length = sizeof(WatchLookUpCustomEnd);
			typeCode = LuaTypeCodes::kWatchLookUpCustomEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE LuaStateBegin : public Sled::SCMP::Base
	{
		LuaStateBegin(uint16_t iPluginId)
		{
			length = sizeof(LuaStateBegin);
			typeCode = LuaTypeCodes::kLuaStateBegin;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE LuaStateAdd : public Sled::SCMP::Base
	{
		LuaStateAdd(uint16_t iPluginId, const char *pszAddress, const char *pszName, bool bDebugging, NetworkBuffer *pBuffer = 0);	
		void pack(NetworkBuffer *pBuffer);	

		static const uint16_t kNameLen = 32; // Same as LuaStateParams

		char		address[Sizes::kPtrLen];
		char		name[kNameLen];
		uint8_t		debugging;
	};

	struct SCE_SLED_LINKAGE LuaStateRemove : public Sled::SCMP::Base
	{
		LuaStateRemove(uint16_t iPluginId, const char *pszAddress, NetworkBuffer *pBuffer = 0);
		void pack(NetworkBuffer *pBuffer);	

		char	address[Sizes::kPtrLen];
	};

	struct SCE_SLED_LINKAGE LuaStateEnd : public Sled::SCMP::Base
	{
		LuaStateEnd(uint16_t iPluginId)
		{
			length = sizeof(LuaStateEnd);
			typeCode = LuaTypeCodes::kLuaStateEnd;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE LuaStateToggle : public Sled::SCMP::Base
	{
		LuaStateToggle(uint16_t iPluginId, const char *pszAddress);
		LuaStateToggle(NetworkBufferReader *reader) { unpack(reader); }
		void unpack(NetworkBufferReader *reader);
	
		char	address[Sizes::kPtrLen];
	};

	struct SCE_SLED_LINKAGE MemoryTraceToggle : public Sled::SCMP::Base
	{
		MemoryTraceToggle(uint16_t iPluginId)
		{
			length = sizeof(MemoryTraceToggle);
			typeCode = LuaTypeCodes::kMemoryTraceToggle;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE ProfilerToggle : public Sled::SCMP::Base
	{
		ProfilerToggle(uint16_t iPluginId)
		{
			length = sizeof(ProfilerToggle);
			typeCode = LuaTypeCodes::kProfilerToggle;
			pluginId = iPluginId;
		}
	};

	struct SCE_SLED_LINKAGE Limits : public Sled::SCMP::Base
	{
		Limits(uint16_t iPluginId, uint16_t iMaxBreakpoints, uint16_t iMaxVarFilters, bool bProfilerEnabled, bool bMemoryTracerEnabled, NetworkBuffer *pBuffer = 0);
		void pack(NetworkBuffer *pBuffer);

		uint16_t	maxBreakpoints;
		uint16_t	maxVarFilters;
		uint8_t		profilerEnabled;
		uint8_t		memoryTracerEnabled;
	};
}}}

#endif // __SCE_LIBSLEDLUAPLUGIN_SCMP_H__
