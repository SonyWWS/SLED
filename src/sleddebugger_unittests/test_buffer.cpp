/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sleddebugger/assert.h"
#include "../sleddebugger/buffer.h"
#include "../sleddebugger/errorcodes.h"
#include "../sleddebugger/utilities.h"

#include <unittest-cpp/UnitTest++/UnitTest++.h>

#include <cstring>
#include <cstdio>
#include <cstdlib>

namespace sce { namespace Sled { namespace
{
	class HostedNetworkBuffer
	{
	public:
		HostedNetworkBuffer()
		{
			m_buffer = 0;
			m_bufferMem = 0;
		}

		~HostedNetworkBuffer()
		{
			if (m_buffer)
			{
				NetworkBuffer::shutdown(m_buffer);
				m_buffer = 0;
			}

			if (m_bufferMem)
			{
				delete [] m_bufferMem;
				m_bufferMem = 0;
			}
		}

		int32_t Setup(uint16_t maxSize)
		{
			NetworkBufferConfig config;
			config.maxSize = maxSize;

			std::size_t iMemSize;

			const int32_t iError = NetworkBuffer::requiredMemory(config, &iMemSize);
			if (iError != 0)
				return iError;

			m_bufferMem = new char[iMemSize];
			if (!m_bufferMem)
				return -1;

			return NetworkBuffer::create(config, m_bufferMem, &m_buffer);
		}

		NetworkBuffer *m_buffer;

	private:
		char *m_bufferMem;
	};

	struct Fixture
	{
		Fixture()
		{
		}

		HostedNetworkBuffer host;
	};

	namespace TestValues
	{
		// Test values
		const uint8_t  u8  = 220;
		const uint16_t u16 = 32711;	
		const uint32_t u32 = 652131;	
		const uint64_t u64 = 1234567;	
		const int16_t i16 = 32711;
		const int32_t i32 = 652131;
		const int64_t i64 = 1234567;
		const float f32 = 3.141529f;
		const double f64 = 3.141529123;
		const char *pszString = "Hello world! How are you today, world? I am great!";
		const char *pszEmptyString = "";

		uint16_t TotalNetworkBufferSizeNeededForAllTestValues()
		{
			const uint16_t iSize =
				sizeof(uint8_t) +
				sizeof(uint16_t) + sizeof(uint32_t) + sizeof(uint64_t) +
				(2 * sizeof(int16_t)) + (2 * sizeof(int32_t)) + (2 * sizeof(int64_t)) +
				(2 * sizeof(float)) + (2 * sizeof(double)) +
				(2 + (uint16_t)std::strlen(pszString)) +
				(2 + (uint16_t)std::strlen(pszEmptyString));

			return iSize;
		}
	}

	TEST_FIXTURE(Fixture, NetworkBuffer_CreateZeroSize)
	{
		CHECK_EQUAL((int32_t)SCE_SLED_ERROR_INVALIDCONFIGURATION, host.Setup(0));
	}

	TEST_FIXTURE(Fixture, NetworkBuffer_CreateAverageSize)
	{
		const uint16_t iMaxSize = 4 * 1024;

		CHECK_EQUAL(0, host.Setup(iMaxSize));
		CHECK_EQUAL(iMaxSize, host.m_buffer->getMaxSize());
	}

	TEST_FIXTURE(Fixture, NetworkBuffer_CreateLargeSize)
	{
		const uint16_t iMaxSize = 32 * 1024;
	
		CHECK_EQUAL(0, host.Setup(iMaxSize));
		CHECK_EQUAL(iMaxSize, host.m_buffer->getMaxSize());
	}

	TEST_FIXTURE(Fixture, NetworkBuffer_CreateEmptyAndVerifySize)
	{
		const uint16_t iMaxSize = 2 * 1024;

		CHECK_EQUAL(0, host.Setup(iMaxSize));

		CHECK_EQUAL((uint32_t)0, host.m_buffer->getSize());
		CHECK_EQUAL(iMaxSize, host.m_buffer->getMaxSize());
	}

	TEST_FIXTURE(Fixture, NetworkBuffer_CreateAndFillFullyAndVerifySizeAndData)
	{
		const uint16_t iMaxSize = 2 * 1024;

		CHECK_EQUAL(0, host.Setup(iMaxSize));
	
		uint8_t buffer[iMaxSize];
		for (uint32_t i = 0; i < iMaxSize; i++)
		{
			const uint8_t random = (uint8_t)(std::rand() % 255);
			buffer[i] = random;

			const uint8_t tempBuffer[] = { random };		
			CHECK_EQUAL(true, host.m_buffer->append(tempBuffer, sizeof(uint8_t)));
			CHECK_EQUAL(i + 1, host.m_buffer->getSize());
		}	
	
		CHECK_EQUAL(iMaxSize, host.m_buffer->getSize());

		host.m_buffer->shuffle(iMaxSize);
		CHECK_EQUAL((uint32_t)0, host.m_buffer->getSize());

		CHECK_EQUAL(true, host.m_buffer->append(buffer, sizeof(buffer)));
		CHECK_EQUAL(iMaxSize, host.m_buffer->getSize());

		CHECK_ARRAY_EQUAL(buffer, host.m_buffer->getData(), host.m_buffer->getSize());

		host.m_buffer->reset();
		CHECK_EQUAL((uint32_t)0, host.m_buffer->getSize());
	}

	TEST_FIXTURE(Fixture, NetworkBuffer_AppendAndShuffleABunch)
	{
		const int iCount = 1000;
		const uint16_t iMaxSize = 2 * 1024;

		CHECK_EQUAL(0, host.Setup(iMaxSize));

		for (int i = 0; i < iCount; i++)
		{
			const uint16_t iBlockSize = (uint16_t)((std::rand() % (iMaxSize - 1)) + 1);

			uint8_t buffer[iMaxSize];
			for (uint16_t j = 0; j < iBlockSize; j++)
			{
				const uint8_t random = (uint8_t)(std::rand() % 255);
				buffer[j] = random;
			}

			CHECK_EQUAL(true, host.m_buffer->append(buffer, iBlockSize));
			CHECK_EQUAL(iBlockSize, host.m_buffer->getSize());

			CHECK_ARRAY_EQUAL(buffer, host.m_buffer->getData(), host.m_buffer->getSize());

			host.m_buffer->shuffle(iBlockSize);
			CHECK_EQUAL((uint32_t)0, host.m_buffer->getSize());
		}	
	}

	TEST_FIXTURE(Fixture, NetworkBuffer_AppendNull)
	{
		CHECK_EQUAL(0, host.Setup(1));

		CHECK_EQUAL(false, host.m_buffer->append(0, 1));
		CHECK_EQUAL((uint32_t)0, host.m_buffer->getSize());
	}

	TEST_FIXTURE(Fixture, NetworkBuffer_AppendZero)
	{
		CHECK_EQUAL(0, host.Setup(1));

		const uint8_t buffer[] = { 0, 1, 2, 3 };

		CHECK_EQUAL(false, host.m_buffer->append(buffer, 0));
		CHECK_EQUAL((uint32_t)0, host.m_buffer->getSize());
	}

	TEST_FIXTURE(Fixture, NetworkBuffer_AppendOverMaxSize)
	{
		const uint16_t iMaxSize = 2 * 1024;

		CHECK_EQUAL(0, host.Setup(iMaxSize));

		uint8_t buffer[iMaxSize];
		for (uint16_t i = 0; i < iMaxSize; i++)
		{
			const uint8_t random = (uint8_t)(std::rand() % 255);
			buffer[i] = random;
		}

		CHECK_EQUAL(true, host.m_buffer->append(buffer, sizeof(buffer)));
		CHECK_EQUAL(iMaxSize, host.m_buffer->getSize());

		CHECK_EQUAL(false, host.m_buffer->append(buffer, 1));
		CHECK_EQUAL(iMaxSize, host.m_buffer->getSize());
	}

	TEST_FIXTURE(Fixture, NetworkBuffer_ShuffleMoreThanLeft)
	{
		const uint16_t iMaxSize = 2 * 1024;

		CHECK_EQUAL(0, host.Setup(iMaxSize));

		uint8_t buffer[iMaxSize];
		for (uint16_t i = 0; i < iMaxSize; i++)
		{
			const uint8_t random = (uint8_t)(std::rand() % 255);
			buffer[i] = random;
		}

		CHECK_EQUAL(true, host.m_buffer->append(buffer, sizeof(buffer)));
		CHECK_EQUAL(iMaxSize, host.m_buffer->getSize());

		while (host.m_buffer->getSize())
			host.m_buffer->shuffle(3);

		CHECK_EQUAL((uint32_t)0, host.m_buffer->getSize());
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_CreatePacker)
	{
		const uint16_t iMaxSize = 2 * 1024;
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		NetworkBufferPacker packer(host.m_buffer);
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_CreateReader)
	{
		const uint16_t iMaxSize = 2 * 1024;
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		NetworkBufferReader reader(host.m_buffer->getData(), host.m_buffer->getSize());
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_PackAndRead)
	{
		const uint16_t iMaxSize = TestValues::TotalNetworkBufferSizeNeededForAllTestValues();

		CHECK_EQUAL(0, host.Setup(iMaxSize));	

		const uint16_t kStringLen = 256;
		char szString[kStringLen];

		NetworkBufferPacker packer(host.m_buffer);
		CHECK_EQUAL(true, packer.packUInt8_t(TestValues::u8));
		CHECK_EQUAL(true, packer.packUInt16_t(TestValues::u16));
		CHECK_EQUAL(true, packer.packUInt32_t(TestValues::u32));
		CHECK_EQUAL(true, packer.packUInt64_t(TestValues::u64));
		CHECK_EQUAL(true, packer.packInt16_t(TestValues::i16));
		CHECK_EQUAL(true, packer.packInt16_t(-TestValues::i16));
		CHECK_EQUAL(true, packer.packInt32_t(TestValues::i32));
		CHECK_EQUAL(true, packer.packInt32_t(-TestValues::i32));
		CHECK_EQUAL(true, packer.packInt64_t(TestValues::i64));
		CHECK_EQUAL(true, packer.packInt64_t(-TestValues::i64));
		CHECK_EQUAL(true, packer.packFloat(TestValues::f32));
		CHECK_EQUAL(true, packer.packFloat(-TestValues::f32));
		CHECK_EQUAL(true, packer.packDouble(TestValues::f64));
		CHECK_EQUAL(true, packer.packDouble(-TestValues::f64));
		CHECK_EQUAL(true, packer.packString(TestValues::pszString));
		CHECK_EQUAL(true, packer.packString(TestValues::pszEmptyString));

		NetworkBufferReader reader(host.m_buffer->getData(), host.m_buffer->getSize());
		CHECK_EQUAL(TestValues::u8, reader.readUInt8_t());
		CHECK_EQUAL(TestValues::u16, reader.readUInt16_t());
		CHECK_EQUAL(TestValues::u32, reader.readUInt32_t());
		CHECK_EQUAL(TestValues::u64, reader.readUInt64_t());
		CHECK_EQUAL(TestValues::i16, reader.readInt16_t());
		CHECK_EQUAL(-TestValues::i16, reader.readInt16_t());
		CHECK_EQUAL(TestValues::i32, reader.readInt32_t());
		CHECK_EQUAL(-TestValues::i32, reader.readInt32_t());
		CHECK_EQUAL(TestValues::i64, reader.readInt64_t());
		CHECK_EQUAL(-TestValues::i64, reader.readInt64_t());
		CHECK_EQUAL(TestValues::f32, reader.readFloat());
		CHECK_EQUAL(-TestValues::f32, reader.readFloat());
		CHECK_EQUAL(TestValues::f64, reader.readDouble());
		CHECK_EQUAL(-TestValues::f64, reader.readDouble());
		reader.readString(szString, kStringLen);
		CHECK_EQUAL(true, Utilities::areStringsEqual(szString, TestValues::pszString));	
		reader.readString(szString, kStringLen);
		CHECK_EQUAL(true, Utilities::areStringsEqual(szString, TestValues::pszEmptyString));	
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_PackOverFlowing_uint8_t)
	{
		const uint16_t iMaxSize = 1;
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		NetworkBufferPacker packer(host.m_buffer);
		CHECK_EQUAL(true, packer.packUInt8_t(TestValues::u8));
		CHECK_EQUAL(false, packer.packUInt8_t(TestValues::u8));
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_PackOverFlowing_uint16_t)
	{
		const uint16_t iMaxSize = 1;
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		NetworkBufferPacker packer(host.m_buffer);
		CHECK_EQUAL(false, packer.packUInt16_t(TestValues::u16));
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_PackOverFlowing_uint32_t)
	{
		const uint16_t iMaxSize = 1;
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		NetworkBufferPacker packer(host.m_buffer);
		CHECK_EQUAL(false, packer.packUInt32_t(TestValues::u32));
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_PackOverFlowing_uint64_t)
	{
		const uint16_t iMaxSize = 1;
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		NetworkBufferPacker packer(host.m_buffer);
		CHECK_EQUAL(false, packer.packUInt64_t(TestValues::u64));
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_PackOverFlowing_int16_t)
	{
		const uint16_t iMaxSize = 1;
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		NetworkBufferPacker packer(host.m_buffer);
		CHECK_EQUAL(false, packer.packInt16_t(TestValues::i16));
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_PackOverFlowing_int32_t)
	{
		const uint16_t iMaxSize = 1;
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		NetworkBufferPacker packer(host.m_buffer);
		CHECK_EQUAL(false, packer.packInt32_t(TestValues::i32));
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_PackOverFlowing_int64_t)
	{
		const uint16_t iMaxSize = 1;
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		NetworkBufferPacker packer(host.m_buffer);
		CHECK_EQUAL(false, packer.packInt64_t(TestValues::i64));
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_PackOverFlowing_String)
	{
		const uint16_t iMaxSize = 1;
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		NetworkBufferPacker packer(host.m_buffer);
		CHECK_EQUAL(false, packer.packString(TestValues::pszString));
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_PackNullString)
	{
		const uint16_t iMaxSize = 2;
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		NetworkBufferPacker packer(host.m_buffer);
		CHECK_EQUAL(false, packer.packString(0));
	}

	TEST_FIXTURE(Fixture, NetworkBufferPackerReader_PackEmptyString)
	{
		const uint16_t iMaxSize = 2; // Size of uint16_t for string length + actual string
		CHECK_EQUAL(0, host.Setup(iMaxSize));

		// PackIng an empty string is allowed and sometimes necessary!
		NetworkBufferPacker packer(host.m_buffer);
		CHECK_EQUAL(true, packer.packString(""));
		CHECK_EQUAL((uint32_t)2, host.m_buffer->getSize());
	}
}}}
