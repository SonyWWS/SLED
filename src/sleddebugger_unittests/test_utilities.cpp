/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sleddebugger/utilities.h"

#include <unittest-cpp/UnitTest++/UnitTest++.h>
#include <string>

namespace sce { namespace Sled { namespace
{
	TEST(Utilities_CopyString)
	{
		const std::size_t kStringLen = 256;
		char buffer[kStringLen];

		const char *pszString1 = "Hello world!";
	
		Utilities::copyString(buffer, kStringLen, pszString1);
		CHECK_EQUAL(true, Utilities::areStringsEqual(pszString1, buffer));

		Utilities::copyString(buffer, kStringLen, 0);
		CHECK_EQUAL(true, Utilities::areStringsEqual(buffer, "\0"));

		Utilities::copyString(buffer, kStringLen, "");
		CHECK_EQUAL(true, Utilities::areStringsEqual(buffer, "\0"));
	}

	TEST(Utilities_CopySubstring)
	{
		const std::size_t kStringLen = 256;
		char buffer[kStringLen];	

		const char *pszString1 = "Hello world!";
		const char *pszString2 = "Hello world!11Hello world!";

		Utilities::copySubstring(buffer, kStringLen, pszString2, 0, 12);
		CHECK_EQUAL(true, Utilities::areStringsEqual(buffer, pszString1));

		Utilities::copySubstring(buffer, kStringLen, pszString2, 14, 12);
		CHECK_EQUAL(true, Utilities::areStringsEqual(buffer, pszString1));
	}

	TEST(Utilities_AppendString)
	{
		const std::size_t kStringLen = 256;
		char buffer[kStringLen];
		buffer[0] = '\0';

		const char *pszString1 = "Hello world!";
		const char *pszString2 = "Hello world!11Hello world!";
	
		Utilities::appendString(buffer, kStringLen, pszString1);
		CHECK_EQUAL(true, Utilities::areStringsEqual(buffer, pszString1));

		Utilities::appendString(buffer, kStringLen, "11");
		Utilities::appendString(buffer, kStringLen, pszString1);
		CHECK_EQUAL(true, Utilities::areStringsEqual(buffer, pszString2));
	}

	TEST(Utilities_CopyFormatString)
	{
		const std::size_t kStringLen = 256;
		char buffer[kStringLen];

		const char *pszString1 = "http://wwws.blah.com/section/%s/index.html";

		Utilities::copyString(buffer, kStringLen, pszString1);
		CHECK_EQUAL(true, Utilities::areStringsEqual(pszString1, buffer));
	}

	TEST(Utilities_CheckTerminator)
	{
		const char terminator = '\0';
		const char *pszString1 = "Hello world!";

		// Not enough space
		{
			const std::size_t kLen = 4;
			char buffer[kLen];

			Utilities::copyString(buffer, kLen, pszString1);
			CHECK_EQUAL(terminator, buffer[kLen - 1]);
		}

		// Not enough space
		{
			const std::size_t kLen = std::strlen(pszString1);
			char *buffer = new char[kLen];

			Utilities::copyString(buffer, kLen, pszString1);
			CHECK_EQUAL(terminator, buffer[kLen - 1]);

			delete [] buffer;
		}

		// Exact space
		{
			const std::size_t kLen = std::strlen(pszString1) + 1;
			char *buffer = new char[kLen];

			Utilities::copyString(buffer, kLen, pszString1);
			CHECK_EQUAL(terminator, buffer[kLen - 1]);

			delete [] buffer;
		}

		// More space
		{
			const std::size_t kLen = std::strlen(pszString1) + 4;
			char *buffer = new char[kLen];

			Utilities::copyString(buffer, kLen, pszString1);
			CHECK_EQUAL(terminator, buffer[std::strlen(pszString1)]);

			delete [] buffer;
		}
	}
}}}
