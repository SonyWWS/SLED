/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "buffer.h"
#include "assert.h"
#include "errorcodes.h"
#include "scmp.h"
#include "sequentialallocator.h"
#include "common.h"

#include <cstring>
#include <new>

namespace sce { namespace Sled
{
	namespace
	{
		struct SCE_SLED_LINKAGE NetworkBufferSeats
		{
			void *m_this;
			void *m_pool;

			void Allocate(const NetworkBufferConfig& netBufferConfig, ISequentialAllocator *pAllocator)
			{
				m_this = pAllocator->allocate(sizeof(NetworkBuffer), __alignof(NetworkBuffer));
				m_pool = pAllocator->allocate(netBufferConfig.maxSize, 4);
			}
		};

		int32_t ValidateConfig(const NetworkBufferConfig& config)
		{
			if (config.maxSize <= 0)
				return SCE_SLED_ERROR_INVALIDCONFIGURATION;

			return SCE_SLED_ERROR_OK;
		}
	}

	int32_t NetworkBuffer::create(const NetworkBufferConfig& netBufferConfig, void *pLocation, NetworkBuffer **ppBuffer)
	{
		SCE_SLED_ASSERT(pLocation != NULL);
		SCE_SLED_ASSERT(ppBuffer != NULL);

		std::size_t iMemSize = 0;
		const int32_t iConfigError = requiredMemory(netBufferConfig, &iMemSize);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocator allocator(pLocation, iMemSize);

		NetworkBufferSeats seats;
		seats.Allocate(netBufferConfig, &allocator);

		SCE_SLED_ASSERT(seats.m_this != NULL);
		SCE_SLED_ASSERT(seats.m_pool != NULL);

		*ppBuffer = new (seats.m_this) NetworkBuffer(netBufferConfig, seats.m_pool);
		return SCE_SLED_ERROR_OK;
	}

	int32_t NetworkBuffer::requiredMemory(const NetworkBufferConfig& netBufferConfig, std::size_t *iRequiredMemory)
	{
		SCE_SLED_ASSERT(iRequiredMemory != NULL);

		const int32_t iConfigError = ValidateConfig(netBufferConfig);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocatorCalculator allocator;

		NetworkBufferSeats seats;
		seats.Allocate(netBufferConfig, &allocator);

		*iRequiredMemory = allocator.bytesAllocated();
		return SCE_SLED_ERROR_OK;
	}

	int32_t NetworkBuffer::requiredMemoryHelper(const NetworkBufferConfig& netBufferConfig, ISequentialAllocator *pAllocator, void **ppThis)
	{
		SCE_SLED_ASSERT(pAllocator != NULL);
		SCE_SLED_ASSERT(ppThis != NULL);

		const int32_t iConfigError = ValidateConfig(netBufferConfig);
		if (iConfigError != 0)
			return iConfigError;

		NetworkBufferSeats seats;
		seats.Allocate(netBufferConfig, pAllocator);

		*ppThis = seats.m_this;
		return SCE_SLED_ERROR_OK;
	}

	void NetworkBuffer::shutdown(NetworkBuffer *pBuffer)
	{
		SCE_SLED_ASSERT(pBuffer != NULL);
		pBuffer->~NetworkBuffer();
	}

	NetworkBuffer::NetworkBuffer(const NetworkBufferConfig& netBufferConfig, void *pMemoryPool)
		: m_iMaxSize(netBufferConfig.maxSize)
		, m_iSize(0)
		, m_pBuffer(reinterpret_cast< uint8_t* >(pMemoryPool))
	{
		SCE_SLED_ASSERT(pMemoryPool != NULL);
	}

	bool NetworkBuffer::append(const uint8_t *pData, const int32_t& iSize)
	{
		if (!pData)
			return false;

		if (iSize <= 0)
			return false;

		if ((m_iSize + iSize) > m_iMaxSize)
			return false;

		// Copy over data
		for (int32_t i = 0; i < iSize; i++)
			m_pBuffer[i + m_iSize] = pData[i];
		m_iSize += iSize;

		return true;
	}

	void NetworkBuffer::shuffle(const uint32_t& iHowMuch)
	{
		if (iHowMuch <= 0)
			return;

		uint32_t iNewHowMuch = iHowMuch;

		// Clamp
		if (iNewHowMuch > m_iSize)
			iNewHowMuch = m_iSize;

		// Move 'left'
		const uint32_t iPos = m_iSize - iNewHowMuch;
		for (uint32_t i = 0; i < iPos; i++)
			m_pBuffer[i] = m_pBuffer[iNewHowMuch + i];
		m_iSize -= iNewHowMuch;
	}

	NetworkBufferPacker::NetworkBufferPacker(NetworkBuffer *pBuffer)
		: m_pNetBuffer(pBuffer)
	{
		SCE_SLED_ASSERT(pBuffer != NULL);
		m_pNetBuffer->reset();
	}

	bool NetworkBufferPacker::packUInt8_t(const uint8_t& value)
	{
		if ((m_pNetBuffer->m_iSize + SCMP::Base::kSizeOfuint8_t) > m_pNetBuffer->m_iMaxSize)
			return false;

		m_pNetBuffer->m_pBuffer[m_pNetBuffer->m_iSize] = value;
		m_pNetBuffer->m_iSize += SCMP::Base::kSizeOfuint8_t;

		return true;
	}

	bool NetworkBufferPacker::packUInt16_t(const uint16_t& value)
	{
		if ((m_pNetBuffer->m_iSize + SCMP::Base::kSizeOfuint16_t) > m_pNetBuffer->m_iMaxSize)
			return false;

		std::memcpy(m_pNetBuffer->m_pBuffer + m_pNetBuffer->m_iSize, &value, SCMP::Base::kSizeOfuint16_t);
		m_pNetBuffer->m_iSize += SCMP::Base::kSizeOfuint16_t;

		return true;
	}

	bool NetworkBufferPacker::packUInt32_t(const uint32_t& value)
	{
		if ((m_pNetBuffer->m_iSize + SCMP::Base::kSizeOfuint32_t) > m_pNetBuffer->m_iMaxSize)
			return false;

		std::memcpy(m_pNetBuffer->m_pBuffer + m_pNetBuffer->m_iSize, &value, SCMP::Base::kSizeOfuint32_t);
		m_pNetBuffer->m_iSize += SCMP::Base::kSizeOfuint32_t;

		return true;
	}

	bool NetworkBufferPacker::packUInt64_t(const uint64_t& value)
	{
		if ((m_pNetBuffer->m_iSize + SCMP::Base::kSizeOfuint64_t) > m_pNetBuffer->m_iMaxSize)
			return false;

		std::memcpy(m_pNetBuffer->m_pBuffer + m_pNetBuffer->m_iSize, &value, SCMP::Base::kSizeOfuint64_t);
		m_pNetBuffer->m_iSize += SCMP::Base::kSizeOfuint64_t;

		return true;
	}

	bool NetworkBufferPacker::packInt16_t(const int16_t& value)
	{
		if ((m_pNetBuffer->m_iSize + SCMP::Base::kSizeOfint16_t) > m_pNetBuffer->m_iMaxSize)
			return false;

		std::memcpy(m_pNetBuffer->m_pBuffer + m_pNetBuffer->m_iSize, &value, SCMP::Base::kSizeOfint16_t);
		m_pNetBuffer->m_iSize += SCMP::Base::kSizeOfint16_t;

		return true;
	}

	bool NetworkBufferPacker::packInt32_t(const int32_t& value)
	{
		if ((m_pNetBuffer->m_iSize + SCMP::Base::kSizeOfint32_t) > m_pNetBuffer->m_iMaxSize)
			return false;

		std::memcpy(m_pNetBuffer->m_pBuffer + m_pNetBuffer->m_iSize, &value, SCMP::Base::kSizeOfint32_t);
		m_pNetBuffer->m_iSize += SCMP::Base::kSizeOfint32_t;

		return true;
	}

	bool NetworkBufferPacker::packInt64_t(const int64_t& value)
	{
		if ((m_pNetBuffer->m_iSize + SCMP::Base::kSizeOfint64_t) > m_pNetBuffer->m_iMaxSize)
			return false;

		std::memcpy(m_pNetBuffer->m_pBuffer + m_pNetBuffer->m_iSize, &value, SCMP::Base::kSizeOfint64_t);
		m_pNetBuffer->m_iSize += SCMP::Base::kSizeOfint64_t;

		return true;
	}

	bool NetworkBufferPacker::packFloat(const float& value)
	{
		if ((m_pNetBuffer->m_iSize + SCMP::Base::kSizeOffloat) > m_pNetBuffer->m_iMaxSize)
			return false;

		std::memcpy(m_pNetBuffer->m_pBuffer + m_pNetBuffer->m_iSize, &value, SCMP::Base::kSizeOffloat);
		m_pNetBuffer->m_iSize += SCMP::Base::kSizeOffloat;

		return true;
	}

	bool NetworkBufferPacker::packDouble(const double& value)
	{
		if ((m_pNetBuffer->m_iSize + SCMP::Base::kSizeOfdouble) > m_pNetBuffer->m_iMaxSize)
			return false;

		std::memcpy(m_pNetBuffer->m_pBuffer + m_pNetBuffer->m_iSize, &value, SCMP::Base::kSizeOfdouble);
		m_pNetBuffer->m_iSize += SCMP::Base::kSizeOfdouble;

		return true;
	}

	bool NetworkBufferPacker::packString(const char *value)
	{
		if (!value)
			return false;

		// ACK! The (len <= 0) doesn't let us pack empty strings - like in 
		// the case of no project settings or something else where its okay
		// to send an empty string!
		const uint16_t len = (uint16_t)std::strlen(value);
		/*if (len <= 0)
			return false;*/

		if ((m_pNetBuffer->m_iSize + SCMP::Base::kSizeOfuint16_t + len) > m_pNetBuffer->m_iMaxSize)
			return false;

		if (!packUInt16_t(len))
			return false;

		std::memcpy(m_pNetBuffer->m_pBuffer + m_pNetBuffer->m_iSize, value, len);
		m_pNetBuffer->m_iSize += (int32_t)len;

		return true;
	}

	NetworkBufferReader::NetworkBufferReader(const uint8_t *pData, const uint32_t& iSize)
		: m_pBuffer(pData)
		, m_iMaxSize(iSize)
		, m_iOffset(0)	
	{
		SCE_SLED_ASSERT(pData != NULL);
	}

	uint8_t NetworkBufferReader::readUInt8_t()
	{
		SCE_SLED_ASSERT((m_iOffset + SCMP::Base::kSizeOfuint8_t) <= m_iMaxSize);
		uint8_t ret = m_pBuffer[m_iOffset];
		m_iOffset += SCMP::Base::kSizeOfuint8_t;
		return ret;
	}

	uint16_t NetworkBufferReader::readUInt16_t()
	{
		SCE_SLED_ASSERT((m_iOffset + SCMP::Base::kSizeOfuint16_t) <= m_iMaxSize);
		uint16_t ret;
		std::memcpy(&ret, m_pBuffer + m_iOffset, SCMP::Base::kSizeOfuint16_t);
		m_iOffset += SCMP::Base::kSizeOfuint16_t;
		return ret;
	}

	uint32_t NetworkBufferReader::readUInt32_t()
	{
		SCE_SLED_ASSERT((m_iOffset + SCMP::Base::kSizeOfuint32_t) <= m_iMaxSize);
		uint32_t ret;
		std::memcpy(&ret, m_pBuffer + m_iOffset, SCMP::Base::kSizeOfuint32_t);
		m_iOffset += SCMP::Base::kSizeOfuint32_t;
		return ret;
	}

	uint64_t NetworkBufferReader::readUInt64_t()
	{
		SCE_SLED_ASSERT((m_iOffset + SCMP::Base::kSizeOfuint64_t) <= m_iMaxSize);
		uint64_t ret;
		std::memcpy(&ret, m_pBuffer + m_iOffset, SCMP::Base::kSizeOfuint64_t);
		m_iOffset += SCMP::Base::kSizeOfuint64_t;
		return ret;
	}

	int16_t NetworkBufferReader::readInt16_t()
	{
		SCE_SLED_ASSERT((m_iOffset + SCMP::Base::kSizeOfint16_t) <= m_iMaxSize);
		int16_t ret;
		std::memcpy(&ret, m_pBuffer + m_iOffset, SCMP::Base::kSizeOfint16_t);
		m_iOffset += SCMP::Base::kSizeOfint16_t;
		return ret;
	}

	int32_t NetworkBufferReader::readInt32_t()
	{
		SCE_SLED_ASSERT((m_iOffset + SCMP::Base::kSizeOfint32_t) <= m_iMaxSize);
		int32_t ret;
		std::memcpy(&ret, m_pBuffer + m_iOffset, SCMP::Base::kSizeOfint32_t);
		m_iOffset += SCMP::Base::kSizeOfint32_t;
		return ret;
	}

	int64_t NetworkBufferReader::readInt64_t()
	{
		SCE_SLED_ASSERT((m_iOffset + SCMP::Base::kSizeOfint64_t) <= m_iMaxSize);
		int64_t ret;
		std::memcpy(&ret, m_pBuffer + m_iOffset, SCMP::Base::kSizeOfint64_t);
		m_iOffset += SCMP::Base::kSizeOfint64_t;
		return ret;
	}

	float NetworkBufferReader::readFloat()
	{
		SCE_SLED_ASSERT((m_iOffset + SCMP::Base::kSizeOffloat) <= m_iMaxSize);
		float ret;
		std::memcpy(&ret, m_pBuffer + m_iOffset, SCMP::Base::kSizeOffloat);
		m_iOffset += SCMP::Base::kSizeOffloat;
		return ret;
	}

	double NetworkBufferReader::readDouble()
	{
		SCE_SLED_ASSERT((m_iOffset + SCMP::Base::kSizeOfdouble) <= m_iMaxSize);
		double ret;
		std::memcpy(&ret, m_pBuffer + m_iOffset, SCMP::Base::kSizeOfdouble);
		m_iOffset += SCMP::Base::kSizeOfdouble;
		return ret;
	}

	uint16_t NetworkBufferReader::peekStringLen() const
	{
		SCE_SLED_ASSERT((m_iOffset + SCMP::Base::kSizeOfuint16_t) <= m_iMaxSize);
		uint16_t ret;
		std::memcpy(&ret, m_pBuffer + m_iOffset, SCMP::Base::kSizeOfuint16_t);
		return ret + 1;
	}

	void NetworkBufferReader::readString(char *pBuffer, const uint16_t& iBufLen)
	{
		SCE_SLED_ASSERT(pBuffer != NULL);
		const uint16_t len = readUInt16_t();
	
		// Should be at least 1 greater for '\0'
		SCE_SLED_ASSERT(iBufLen > len);
	
		// m_iOffset has been updated from Read16()
		SCE_SLED_ASSERT((m_iOffset + len) <= m_iMaxSize);

		std::memcpy(pBuffer, m_pBuffer + m_iOffset, len);
		m_iOffset += (uint32_t)len;	
		pBuffer[len] = '\0';
	}
}}
