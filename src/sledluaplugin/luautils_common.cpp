/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "luautils.h"
#include "../sleddebugger/assert.h"
#include "../sleddebugger/utilities.h"

#include <cstdio>
#include <cstring>
#include <cstdarg>
#include <ctype.h>

namespace sce { namespace Sled
{
	void LuaStateParams::init(const LuaStateParams& rhs)
	{
		luaState = rhs.luaState;
		debugging = rhs.debugging;
	}

	void MemTraceParams::init(const MemTraceParams& rhs)
	{
		what = rhs.what;
		oldPtr = rhs.oldPtr;
		newPtr = rhs.newPtr;
		oldSize = rhs.oldSize;
		newSize = rhs.newSize;
	}

	void Breakpoint::setup(const char *pszFile /* = 0 */,
						   const char *pszCondition /* = 0 */,
						   const int32_t& iLine /* = 0 */,
						   const int32_t& iHash /* = 0 */,
						   const bool& bResult /* = true */,
						   const bool& bUseFunctionEnvironment /* = false */)
	{
		Utilities::copyString(m_szFile, kStringLen, pszFile);
		Utilities::copyString(m_szCondition, kStringLen, pszCondition);
		m_iLine = iLine;
		m_iHash = iHash;
		m_result = bResult ? 1 : 0;
		m_useFunctionEnvironment = bUseFunctionEnvironment ? 1 : 0;
	}

	void Breakpoint::init(const Breakpoint& rhs)
	{
		setup(rhs.m_szFile,
			rhs.m_szCondition,
			rhs.m_iLine,
			rhs.m_iHash,
			(rhs.m_result == 0 ? false : true),
			(rhs.m_useFunctionEnvironment == 0 ? false : true));
	}

	bool Breakpoint::operator==(const Breakpoint& rhs) const
	{
		// Compare hashes
		if (m_iHash != rhs.m_iHash)
			return false;

		// Compare line numbers
		if (m_iLine != rhs.m_iLine)
			return false;

		const uint32_t iThisLen = (uint32_t)std::strlen(m_szFile);
		const uint32_t iRhsLen = (uint32_t)std::strlen(rhs.m_szFile);

		// Compare lengths
		if (iThisLen != iRhsLen)
			return false;

		// Do string comparison (but ignore slashes & ignore case)
		for (uint32_t i = 0; i < iThisLen; i++)
		{
			// Ignore slashes
			if (((m_szFile[i] == '\\') || (m_szFile[i] == '/')) &&
				((rhs.m_szFile[i] == '\\') || (rhs.m_szFile[i] == '/')))
				continue;

			// Ignore case
			if (::tolower(m_szFile[i]) != ::tolower(rhs.m_szFile[i]))
				return false;
		}
	
		return true;
	}

	void Breakpoint::setCondition(const char *pszCondition)
	{
		Utilities::copyString(m_szCondition, kStringLen, pszCondition);
	}

	namespace StringUtilities
	{
		void copyString(char *pszCopyTo, std::size_t len, const char *pszCopyFrom, ...)
		{
			SCE_SLED_ASSERT(pszCopyTo != NULL);

			if ((pszCopyFrom != NULL) && (std::strlen(pszCopyFrom) > 0))
			{
				va_list args;
				va_start(args, pszCopyFrom);
#if SCE_SLEDTARGET_OS_WINDOWS
				const int written = ::_vsnprintf(pszCopyTo, len, pszCopyFrom, args);
#endif
				va_end(args);

				if ((written < 0) || (written >= (int)len))
					pszCopyTo[len - 1] = '\0';
			}
			else
			{
				pszCopyTo[0] = '\0';
			}
		}
	}
}}
