/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDLUAPLUGIN_LUAVARIABLE_H__
#define __SCE_LIBSLEDLUAPLUGIN_LUAVARIABLE_H__

#include "sledluaplugin_class.h"

#include <cstring>

#include "../sleddebugger/common.h"

namespace sce { namespace Sled
{
	struct SCE_SLED_LINKAGE LuaVariableKeyValue
	{
		char		*name;
		int32_t		type;
	};

	struct SCE_SLED_LINKAGE LuaVariable
	{
		static const int32_t kMaxKeyValues = 128;

		char		*name;
		int32_t		nameType;
		char		*value;
		int32_t		valueType;
		LuaVariableScope::Enum	what;
		LuaVariableContext::Enum context;
		bool		bTable;
		bool		bFlag;

		int32_t		level;
		int32_t		index;

		uint16_t	numKeyValues;
		LuaVariableKeyValue	hKeyValues[kMaxKeyValues];	

		LuaVariable()
			: name(0)
			, nameType(0)
			, value(0)
			, valueType(0)
			, what(LuaVariableScope::kGlobal)
			, context(LuaVariableContext::kNormal)
			, bTable(false)
			, bFlag(false)
			, level(0)
			, index(0)
			, numKeyValues(0)		
		{
			std::memset(hKeyValues, 0, sizeof(LuaVariableKeyValue) * kMaxKeyValues);
		}
	};
}}

#endif // __SCE_LIBSLEDLUAPLUGIN_LUAVARIABLE_H__
