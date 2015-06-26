/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "utilities.h"
#include "assert.h"

#if SCE_SLEDTARGET_OS_WINDOWS
	#ifndef _WINDOWS_
		#define WIN32_LEAN_AND_MEAN
		#include <windows.h>
	#endif
#else
	#error Not currently supported!
#endif

#include <cstdio>
#include <cstring>

namespace sce { namespace Sled { namespace Utilities
{
	void copyString(char *pszCopyTo, std::size_t len, const char *pszCopyFrom)
	{
		copySubstring(pszCopyTo, len, pszCopyFrom, 0, pszCopyFrom == NULL ? 0 : std::strlen(pszCopyFrom));
	}

	void appendString(char *pszAppendTo, std::size_t len, const char *pszAppendFrom)
	{
		SCE_SLED_ASSERT(pszAppendTo != NULL);

		if ((pszAppendFrom != NULL) && (std::strlen(pszAppendFrom) > 0))
		{
			std::strncat(pszAppendTo, pszAppendFrom, len);
		}
	}

	bool areStringsEqual(const char *pszString1, const char *pszString2)
	{
		const std::size_t iLen1 = std::strlen(pszString1);
		const std::size_t iLen2 = std::strlen(pszString2);

		if (iLen1 != iLen2)
			return false;

#if SCE_SLEDTARGET_OS_WINDOWS
		return ::_stricmp(pszString1, pszString2) == 0;
#else
		#error Not supported!
#endif
	}

	int findFirstOf(const char *pszSearch, char chWhat, int iStartPos)
	{
		SCE_SLED_ASSERT(pszSearch != NULL);
		SCE_SLED_ASSERT(iStartPos >= 0);

		const int len = (int)std::strlen(pszSearch);
		for (int i = iStartPos; i < len; i++)
		{
			if (pszSearch[i] == chWhat)
				return i;
		}

		return -1;
	}

	int findFirstOf(const char *pszSearch, const char *pszWhat, int iStartPos)
	{
		SCE_SLED_ASSERT(pszSearch != NULL);
		SCE_SLED_ASSERT(pszWhat != NULL);
		SCE_SLED_ASSERT(iStartPos >= 0);

		const int iSearchLen = (int)std::strlen(pszSearch);
		const int iWhatLen = (int)std::strlen(pszWhat);

		for (int i = iStartPos; i < iSearchLen; i++)
		{
			if ((i + iWhatLen) > iSearchLen)
				return -1;

			bool bFlag = false;

			for (int j = 0; (j < iWhatLen) && !bFlag; j++)
			{
				if (pszSearch[i + j] != pszWhat[j])
					bFlag = true;
			}

			if (!bFlag)
				return i;
		}

		return -1;
	}

	void copySubstring(char *pszCopyTo, std::size_t len, const char *pszCopyFrom, const std::size_t& iStartPos, const std::size_t& iCopyLen)
	{
		SCE_SLED_ASSERT(pszCopyTo != NULL);

		const std::size_t actualCopyLen =
			pszCopyFrom == NULL
				? 0
				: ((iStartPos + iCopyLen) >= len) ? len - 1 : iCopyLen;

		std::size_t i = 0;
		for (; i < actualCopyLen; i++)
		{
			pszCopyTo[i] = pszCopyFrom[i + iStartPos];
		}

		pszCopyTo[i] = '\0';
	}

	FileCallback& openFileCallback()
	{
		static FileCallback pfnFileCallback = 0;
		return pfnFileCallback;
	}

	FileFinishCallback& openFileFinishCallback()
	{
		static FileFinishCallback pfnFileFinishCallback = 0;
		return pfnFileFinishCallback;
	}
}}}
