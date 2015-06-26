/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sleddebugger/assert.h"
#include "../sleddebugger/errorcodes.h"
#include "../sleddebugger/stringarray.h"
#include "../sleddebugger/utilities.h"

#include "logstealer.h"

#include <unittest-cpp/UnitTest++/UnitTest++.h>

#include <cstring>
#include <cstdio>
#include <cstdlib>

namespace sce { namespace Sled { namespace
{
	class HostedStringArray
	{
	public:
		HostedStringArray()
		{
			m_array = 0;
			m_arrayMem = 0;
		}

		~HostedStringArray()
		{
			if (m_array) {
				StringArray::shutdown(m_array);
				m_array = 0;
			}

			if (m_arrayMem) {
				delete [] m_arrayMem;
				m_arrayMem = 0;
			}
		}

		int32_t Setup(uint16_t maxEntries, uint16_t maxEntryLen, bool allowDuplicates = false)
		{
			StringArrayConfig config;
			config.maxEntries = maxEntries;
			config.maxEntryLen = maxEntryLen;
			config.allowDuplicates = allowDuplicates;

			std::size_t iMemSize;

			const int32_t iError = StringArray::requiredMemory(&config, &iMemSize);
			if (iError != 0)
				return iError;

			m_arrayMem = new char[iMemSize];
			if (!m_arrayMem)
				return -1;

			return StringArray::create(&config, m_arrayMem, &m_array);
		}

		StringArray *m_array;

	private:
		char *m_arrayMem;
	};

	struct Fixture
	{
		Fixture()
		{
		}

		HostedStringArray host;
	};

	TEST_FIXTURE(Fixture, StringArray_AddABunchOfValues_NoDuplicates)
	{
		CHECK_EQUAL(0, host.Setup(5, 20));
		CHECK_EQUAL(false, host.m_array->allowDuplicates());

		CHECK_EQUAL(true, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());

		CHECK_EQUAL(0, host.m_array->getNumEntries());
		CHECK_EQUAL(5, host.m_array->getMaxEntries());
		CHECK_EQUAL(20, host.m_array->getMaxEntryLen());

		const char *pszString1 = "test_string1";
		CHECK_EQUAL(true, host.m_array->add(pszString1));
		CHECK_EQUAL(1, host.m_array->getNumEntries());

		CHECK_EQUAL(false, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());

		const char *pszString2 = "test_string2";
		CHECK_EQUAL(true, host.m_array->add(pszString2));
		CHECK_EQUAL(2, host.m_array->getNumEntries());

		CHECK_EQUAL(false, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());

		const char *pszString3 = "test_string3";
		CHECK_EQUAL(true, host.m_array->add(pszString3));
		CHECK_EQUAL(3, host.m_array->getNumEntries());

		CHECK_EQUAL(false, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());

		const char *pszString4 = "test_string4";
		CHECK_EQUAL(true, host.m_array->add(pszString4));
		CHECK_EQUAL(4, host.m_array->getNumEntries());

		CHECK_EQUAL(false, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());

		const char *pszString5 = "test_string5";
		CHECK_EQUAL(true, host.m_array->add(pszString5));
		CHECK_EQUAL(5, host.m_array->getNumEntries());

		CHECK_EQUAL(false, host.m_array->isEmpty());
		CHECK_EQUAL(true, host.m_array->isFull());

		int iCount = 0;

		StringArrayConstIterator iter(host.m_array);
		for (; iter(); ++iter)
		{
			switch (iCount)
			{
			case 0:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString1, iter.get()));
				break;
			case 1:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString2, iter.get()));
				break;
			case 2:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString3, iter.get()));
				break;
			case 3:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString4, iter.get()));
				break;
			case 4:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString5, iter.get()));
				break;
			}

			++iCount;
		}

		host.m_array->clear();

		CHECK_EQUAL(true, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());

		CHECK_EQUAL(0, host.m_array->getNumEntries());
		CHECK_EQUAL(5, host.m_array->getMaxEntries());
		CHECK_EQUAL(20, host.m_array->getMaxEntryLen());
	}

	TEST_FIXTURE(Fixture, StringArray_AddABunchOfValues_AllowDuplicates)
	{
		CHECK_EQUAL(0, host.Setup(5, 20, true));
		CHECK_EQUAL(true, host.m_array->allowDuplicates());

		CHECK_EQUAL(true, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());

		CHECK_EQUAL(0, host.m_array->getNumEntries());
		CHECK_EQUAL(5, host.m_array->getMaxEntries());
		CHECK_EQUAL(20, host.m_array->getMaxEntryLen());

		const char *pszString1 = "test_string1";
		CHECK_EQUAL(true, host.m_array->add(pszString1));
		CHECK_EQUAL(1, host.m_array->getNumEntries());

		CHECK_EQUAL(false, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());
	
		// Add string 1 again
		CHECK_EQUAL(true, host.m_array->add(pszString1));
		CHECK_EQUAL(2, host.m_array->getNumEntries());

		CHECK_EQUAL(false, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());

		const char *pszString2 = "test_string2";
		CHECK_EQUAL(true, host.m_array->add(pszString2));
		CHECK_EQUAL(3, host.m_array->getNumEntries());

		CHECK_EQUAL(false, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());

		// Add string 2 again
		CHECK_EQUAL(true, host.m_array->add(pszString2));
		CHECK_EQUAL(4, host.m_array->getNumEntries());

		CHECK_EQUAL(false, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());

		const char *pszString3 = "test_string3";
		CHECK_EQUAL(true, host.m_array->add(pszString3));
		CHECK_EQUAL(5, host.m_array->getNumEntries());

		CHECK_EQUAL(false, host.m_array->isEmpty());
		CHECK_EQUAL(true, host.m_array->isFull());

		StringArrayIndexedConstIterator iter(host.m_array);

		const bool b1eq2 = Utilities::areStringsEqual(pszString1, iter[1]);
		const bool b2eq1 = Utilities::areStringsEqual(pszString1, iter[0]);
		const bool b3eq4 = Utilities::areStringsEqual(pszString2, iter[3]);
		const bool b4eq3 = Utilities::areStringsEqual(pszString2, iter[2]);
		const bool b1eq3 = Utilities::areStringsEqual(pszString1, iter[2]);
		const bool b1eq5 = Utilities::areStringsEqual(pszString1, iter[4]);
		const bool b3eq5 = Utilities::areStringsEqual(pszString2, iter[4]);

		CHECK_EQUAL(true, b1eq2);
		CHECK_EQUAL(true, b2eq1);
		CHECK_EQUAL(true, b3eq4);
		CHECK_EQUAL(true, b4eq3);
		CHECK_EQUAL(false, b1eq3);
		CHECK_EQUAL(false, b1eq5);
		CHECK_EQUAL(false, b3eq5);

		host.m_array->clear();

		CHECK_EQUAL(true, host.m_array->isEmpty());
		CHECK_EQUAL(false, host.m_array->isFull());

		CHECK_EQUAL(0, host.m_array->getNumEntries());
		CHECK_EQUAL(5, host.m_array->getMaxEntries());
		CHECK_EQUAL(20, host.m_array->getMaxEntryLen());
	}

	TEST_FIXTURE(Fixture, StringArray_AddBunchesOfRandomValues)
	{
		const uint16_t iMaxEntries = 1000;
		const uint16_t iMaxEntryLen = 128;

		CHECK_EQUAL(0, host.Setup(iMaxEntries, iMaxEntryLen, true));

		char buffer[iMaxEntryLen - 1];

		const char *alphabet = "abcdefghijklmnopqrstuvwxyz";
		const int alphaLen = (int)std::strlen(alphabet) - 1;

		for (uint16_t i = 0; i < iMaxEntries; i++)
		{
			const int random = (std::rand() % (iMaxEntryLen - 3)) + 1;
			for (int j = 0; j < random; j++)
			{
				const int alpha = std::rand() % alphaLen;
				buffer[j] = alphabet[alpha];
			}
			buffer[random] = '\0';

			CHECK_EQUAL(true, host.m_array->add(buffer));
		}
	}

	TEST_FIXTURE(Fixture, CreateStringArray_ZeroMaxEntries)
	{
		// Bad values - doesn't make sense
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_INVALIDCONFIGURATION, host.Setup(0, 15));
	}

	TEST_FIXTURE(Fixture, CreateStringArray_ZeroMaxEntryLen)
	{
		// Bad values - doesn't make sense to have 5 entries of 0 length
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_INVALIDCONFIGURATION, host.Setup(5, 0));
	}

	TEST_FIXTURE(Fixture, CreateStringArray_ZeroBoth)
	{
		// This is okay and wastes no memory
		CHECK_EQUAL(0, host.Setup(0, 0));
	}

	TEST_FIXTURE(Fixture, StringArray_AddValueLongerThanMaxEntryLen)
	{
		const uint16_t iMaxEntryLen = 15;

		CHECK_EQUAL(0, host.Setup(3, iMaxEntryLen));

		const char *pszLongString = "Hello world! How are you doing today, world? I am great!";
	
		const bool bLongerThanMaxEntryLen = (uint16_t)std::strlen(pszLongString) > iMaxEntryLen;
		CHECK_EQUAL(true, bLongerThanMaxEntryLen);

		// Stop built-in logger from spamming when next part fails
		{
			LogStealer log;

			CHECK_EQUAL(false, host.m_array->add(pszLongString));
		}
	}

	TEST_FIXTURE(Fixture, StringArray_AddMoreValuesThanCanHold)
	{
		const uint16_t iMaxEntries = 5;

		CHECK_EQUAL(0, host.Setup(iMaxEntries, 15));
	
		char buffer[512];
		const char *pszString = "Entry#";

		// Fill to brim
		for (uint16_t i = 0; i < iMaxEntries; i++)
		{		
			std::sprintf(buffer, "%s%u", pszString, i);
			CHECK_EQUAL(true, host.m_array->add(buffer));
		}

		// Stop built-in logger from spamming when next part fails
		{
			LogStealer log;

			// Add just one more to kick over the limit
			std::sprintf(buffer, "%s%u", pszString, iMaxEntries);
			CHECK_EQUAL(false, host.m_array->add(buffer));
		}
	}

	TEST_FIXTURE(Fixture, StringArray_AddNull)
	{
		CHECK_EQUAL(0, host.Setup(5, 15));

		CHECK_EQUAL(false, host.m_array->add(0));
	}

	TEST_FIXTURE(Fixture, StringArray_AddEmpty)
	{
		CHECK_EQUAL(0, host.Setup(5, 15));

		CHECK_EQUAL(false, host.m_array->add(""));
	}

	TEST_FIXTURE(Fixture, StringArray_AddAndRemove)
	{
		CHECK_EQUAL(0, host.Setup(10, 32));

		const char *pszString1 = "test_string1";
		const char *pszString2 = "test_string2";
		const char *pszString3 = "test_string3";
		const char *pszString4 = "test_string4";
		const char *pszString5 = "test_string5";
		const char *pszString6 = "test_string6";
		const char *pszString7 = "test_string7";
		const char *pszString8 = "test_string8";
		const char *pszString9 = "test_string9";
		const char *pszString10 = "test_string10";

		CHECK_EQUAL(true, host.m_array->add(pszString1));
		CHECK_EQUAL(true, host.m_array->add(pszString2));
		CHECK_EQUAL(true, host.m_array->add(pszString3));
		CHECK_EQUAL(true, host.m_array->add(pszString4));
		CHECK_EQUAL(true, host.m_array->add(pszString5));
		CHECK_EQUAL(true, host.m_array->add(pszString6));
		CHECK_EQUAL(true, host.m_array->add(pszString7));
		CHECK_EQUAL(true, host.m_array->add(pszString8));
		CHECK_EQUAL(true, host.m_array->add(pszString9));
		CHECK_EQUAL(true, host.m_array->add(pszString10));
		CHECK_EQUAL(10, host.m_array->getNumEntries());

		StringArrayIndexedConstIterator iter(host.m_array);
	
		for (uint16_t i = 0; i < iter.getCount(); i++)
		{
			switch (i)
			{
			case 0:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString1, iter[i]));
				break;
			case 1:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString2, iter[i]));
				break;
			case 2:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString3, iter[i]));
				break;
			case 3:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString4, iter[i]));
				break;
			case 4:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString5, iter[i]));
				break;
			case 5:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString6, iter[i]));
				break;
			case 6:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString7, iter[i]));
				break;
			case 7:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString8, iter[i]));
				break;
			case 8:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString9, iter[i]));
				break;
			case 9:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString10, iter[i]));
				break;
			}
		}

		CHECK_EQUAL(false, host.m_array->remove("Hello world!"));
		CHECK_EQUAL(true, host.m_array->remove(pszString1));
		CHECK_EQUAL(9, host.m_array->getNumEntries());

		for (uint16_t i = 0; i < iter.getCount(); i++)
		{
			switch (i)
			{
			case 0:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString2, iter[i]));
				break;
			case 1:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString3, iter[i]));
				break;
			case 2:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString4, iter[i]));
				break;
			case 3: CHECK_EQUAL(true, Utilities::areStringsEqual(pszString5, iter[i]));
				break;
			case 4:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString6, iter[i]));
				break;
			case 5:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString7, iter[i]));
				break;
			case 6:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString8, iter[i]));
				break;
			case 7:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString9, iter[i]));
				break;
			case 8:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString10, iter[i]));
				break;
			}
		}

		CHECK_EQUAL(true, host.m_array->remove(pszString4));
		CHECK_EQUAL(true, host.m_array->remove(pszString5));
		CHECK_EQUAL(true, host.m_array->remove(pszString6));
		CHECK_EQUAL(6, host.m_array->getNumEntries());

		for (uint16_t i = 0; i < iter.getCount(); i++)
		{
			switch (i)
			{
			case 0:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString2, iter[i]));
				break;
			case 1:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString3, iter[i]));
				break;
			case 2:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString7, iter[i]));
				break;
			case 3:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString8, iter[i]));
				break;
			case 4:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString9, iter[i]));
				break;
			case 5:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString10, iter[i]));
				break;
			}
		}

		CHECK_EQUAL(true, host.m_array->add(pszString1));
		CHECK_EQUAL(true, host.m_array->add(pszString4));
		CHECK_EQUAL(true, host.m_array->add(pszString5));
		CHECK_EQUAL(true, host.m_array->add(pszString6));
		CHECK_EQUAL(10, host.m_array->getNumEntries());

		for (uint16_t i = 0; i < iter.getCount(); i++)
		{
			switch (i)
			{
			case 0:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString1, iter[i]));
				break;
			case 1:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString2, iter[i]));
				break;
			case 2:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString3, iter[i]));
				break;
			case 3:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString4, iter[i]));
				break;
			case 4:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString5, iter[i]));
				break;
			case 5:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString6, iter[i]));
				break;
			case 6:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString7, iter[i]));
				break;
			case 7:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString8, iter[i]));
				break;
			case 8:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString9, iter[i]));
				break;
			case 9:
				CHECK_EQUAL(true, Utilities::areStringsEqual(pszString10, iter[i]));
				break;
			}
		}

		{
			LogStealer ls;
			CHECK_EQUAL(false, host.m_array->add("Hello world!"));
		}
	}
}}}
