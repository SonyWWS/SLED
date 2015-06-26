/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "stringarray.h"
#include "assert.h"
#include "errorcodes.h"
#include "sequentialallocator.h"
#include "utilities.h"

#include <cstring>
#include <new>

namespace sce { namespace Sled
{
	namespace Slot
	{
		static const uint8_t Free = 1;
		static const uint8_t Full = 0;
	}

	StringArrayConstIterator::StringArrayConstIterator(const StringArray *pStringArray)
		 : m_pStringArray(pStringArray)
	{
		m_iIndex = m_pStringArray->getFirstFullSlotIndex(0);
	}

	StringArrayConstIterator::StringArrayConstIterator() : m_pStringArray(0) {}

	bool StringArrayConstIterator::operator()() const
	{
		// Collapse down m_iIndex to a zero based index value
		uint16_t iNewIndex = m_iIndex;

		for (uint16_t i = 0; i < m_iIndex; i++)
		{
			if (m_pStringArray->m_pFreeList[i] == Slot::Free)
				--iNewIndex;
		}
	
		return iNewIndex < m_pStringArray->m_iNumEntries;
	}

	const char *StringArrayConstIterator::get() const
	{
		return m_pStringArray->operator[](m_iIndex);
	}

	StringArrayConstIterator& StringArrayConstIterator::operator++()
	{
		m_iIndex = m_pStringArray->getFirstFullSlotIndex(m_iIndex + 1);
		return *this;
	}

	void StringArrayConstIterator::reset()
	{
		m_iIndex = m_pStringArray->getFirstFullSlotIndex(0);
	}

	StringArrayIndexedConstIterator::StringArrayIndexedConstIterator(const StringArray *pStringArray)
		: m_pStringArray(pStringArray)
	{
	}

	StringArrayIndexedConstIterator::StringArrayIndexedConstIterator() : m_pStringArray(0) {}

	uint16_t StringArrayIndexedConstIterator::getCount() const
	{
		return m_pStringArray->getNumEntries();
	}

	const char *StringArrayIndexedConstIterator::operator[](uint16_t iIndex) const
	{
		// Inflate iIndex so that it maps to the string array
		const uint16_t iCount = iIndex + 1;

		for (uint16_t i = 0; i < iCount; i++)
		{
			iIndex = (i == 0) 
				? m_pStringArray->getFirstFullSlotIndex()
				: m_pStringArray->getFirstFullSlotIndex(iIndex + 1);
		}

		const char *pszString = m_pStringArray->operator[](iIndex);
		return pszString;
	}

	namespace
	{
		struct StringArraySeats
		{
			void *m_this;
			void *m_pool;
			void *m_freeList;

			void Allocate(const StringArrayConfig *pConfig, ISequentialAllocator *pAllocator)
			{
				m_this = pAllocator->allocate(sizeof(StringArray), __alignof(StringArray));
				m_pool = pAllocator->allocate(pConfig->maxEntries * pConfig->maxEntryLen, 4);
				m_freeList = pAllocator->allocate(sizeof(uint8_t) * pConfig->maxEntries, 4);
			}
		};

		inline int32_t ValidateConfig(const StringArrayConfig& config)
		{	
			const bool bAnyEntries = config.maxEntries != 0;
			const bool bAnyLength = config.maxEntryLen != 0;

			if ((bAnyEntries && !bAnyLength) || (!bAnyEntries && bAnyLength))
				return SCE_SLED_ERROR_INVALIDCONFIGURATION;

			return SCE_SLED_ERROR_OK;
		}
	}

	int32_t StringArray::create(const StringArrayConfig *pConfig, void *pLocation, StringArray **ppArray)
	{
		SCE_SLED_ASSERT(pConfig != NULL);
		SCE_SLED_ASSERT(pLocation != NULL);
		SCE_SLED_ASSERT(ppArray != NULL);

		std::size_t iMemSize = 0;
		const int32_t iConfigError = requiredMemory(pConfig, &iMemSize);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocator allocator(pLocation, iMemSize);

		StringArraySeats seats;	
		seats.Allocate(pConfig, &allocator);

		SCE_SLED_ASSERT(seats.m_this != NULL);
		SCE_SLED_ASSERT(seats.m_pool != NULL);
		SCE_SLED_ASSERT(seats.m_freeList != NULL);

		*ppArray = new (seats.m_this) StringArray(pConfig, &seats);
		return SCE_SLED_ERROR_OK;
	}

	int32_t StringArray::requiredMemory(const StringArrayConfig *pConfig, std::size_t *iRequiredMemory)
	{
		SCE_SLED_ASSERT(pConfig != NULL);
		SCE_SLED_ASSERT(iRequiredMemory != NULL);

		const int32_t iConfigError = ValidateConfig(*pConfig);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocatorCalculator allocator;

		StringArraySeats seats;	
		seats.Allocate(pConfig, &allocator);

		*iRequiredMemory = allocator.bytesAllocated();
		return SCE_SLED_ERROR_OK;
	}

	int32_t StringArray::requiredMemoryHelper(const StringArrayConfig *pConfig, ISequentialAllocator *pAllocator, void **ppThis)
	{
		SCE_SLED_ASSERT(pConfig != NULL);
		SCE_SLED_ASSERT(pAllocator != NULL);
		SCE_SLED_ASSERT(ppThis != NULL);

		const int32_t iConfigError = ValidateConfig(*pConfig);
		if (iConfigError != 0)
			return iConfigError;

		StringArraySeats seats;
		seats.Allocate(pConfig, pAllocator);

		*ppThis = seats.m_this;
		return SCE_SLED_ERROR_OK;
	}

	void StringArray::shutdown(StringArray *pStringArray)
	{
		SCE_SLED_ASSERT(pStringArray != NULL);
		pStringArray->~StringArray();
	}

	StringArray::StringArray(const StringArrayConfig *pConfig, const void *pStringArraySeats)
		: m_iMaxEntries(pConfig->maxEntries)
		, m_iMaxEntryLen(pConfig->maxEntryLen)
		, m_allowDups(pConfig->allowDuplicates ? 1 : 0)
		, m_iNumEntries(0)
	{
		SCE_SLED_ASSERT(pConfig != NULL);
		SCE_SLED_ASSERT(pStringArraySeats != NULL);

		const StringArraySeats *pSeats = static_cast<const StringArraySeats*>(pStringArraySeats);

		m_pBuffer = reinterpret_cast< uint8_t* >(pSeats->m_pool);
		m_pFreeList = new (pSeats->m_freeList) uint8_t[pConfig->maxEntries];

		// Mark list as free
		for (uint16_t i = 0; i < m_iMaxEntries; i++)
			m_pFreeList[i] = Slot::Free;
	}

	const char *StringArray::operator[](uint16_t iIndex) const
	{
		SCE_SLED_ASSERT(iIndex < m_iMaxEntries);

		if (iIndex >= m_iMaxEntries)
			return "";

		// Slot is free/invalid
		if (m_pFreeList[iIndex] == Slot::Free)
			return "";

		const int32_t offset = iIndex * m_iMaxEntryLen;
		const char *pBuffer = reinterpret_cast< const char* >(m_pBuffer) + offset;
		return pBuffer;
	}

	uint16_t StringArray::getFirstFreeSlotIndex(const uint16_t& iStartPos /* = 0 */) const
	{
		for (uint16_t i = iStartPos; i < m_iMaxEntries; i++)
		{
			if (m_pFreeList[i] == Slot::Free)
				return i;
		}

		return m_iMaxEntries + 1;
	}

	uint16_t StringArray::getFirstFullSlotIndex(const uint16_t& iStartPos /* = 0 */) const
	{
		for (uint16_t i = iStartPos; i < m_iMaxEntries; i++)
		{
			if (m_pFreeList[i] == Slot::Full)
				return i;
		}

		return m_iMaxEntries + 1;
	}

	bool StringArray::add(const char *pszEntry)
	{
		if (!pszEntry)
			return false;

		if (std::strlen(pszEntry) <= 0)
			return false;

		const uint16_t len = (uint16_t)std::strlen(pszEntry);

		// Can't add if full
		if (m_iNumEntries == m_iMaxEntries)
		{
			SCE_SLED_LOG(Logging::kError, "[SLED] Cannot add %s as StringArray is full!", pszEntry);
			return false;
		}

		// Can't add if longer than entry length
		if (len >= (m_iMaxEntryLen - 1))
		{
			SCE_SLED_LOG(Logging::kError, "[SLED] Length of %s exceeds the maximum entry length - not adding!", pszEntry);
			return false;
		}

		const bool bCheckForDups = (allowDuplicates() == false);

		bool bFound = false;

		if ((m_iNumEntries != 0) && bCheckForDups)
		{
			// Search for duplicates
			for (uint16_t i = 0; (i < m_iNumEntries) && !bFound; i++)
			{
				// Slot is free so continue past
				if (m_pFreeList[i] == Slot::Free)
					continue;

				if (Utilities::areStringsEqual(operator[](i), pszEntry))
					bFound = true;
			}
		}	

		if (!bFound)
		{
			// Find spot to place item
			const uint16_t iFreeSlot = getFirstFreeSlotIndex();
			SCE_SLED_ASSERT(iFreeSlot != (m_iMaxEntries + 1));

			//const int32_t offset = m_iNumEntries++ * m_iMaxEntryLen;
			//char* pBuffer = reinterpret_cast< char* >(m_pBuffer) + offset;
		
			char *pBuffer = reinterpret_cast< char* >(m_pBuffer) + (iFreeSlot * m_iMaxEntryLen);
			Utilities::copyString(pBuffer, m_iMaxEntryLen, pszEntry);

			// Update counter
			++m_iNumEntries;
			// Mark not free
			m_pFreeList[iFreeSlot] = Slot::Full;
		}

		return !bFound;
	}

	bool StringArray::remove(const char *pszEntry)
	{
		if (!pszEntry)
			return false;

		if (std::strlen(pszEntry) <= 0)
			return false;

		if (m_iNumEntries == 0)
			return false;

		uint16_t iNumRemoved = 0;
		bool bRemovedAny = false;

		// Find occurrences of pszEntry and mark that spot as free
		for (uint16_t i = 0; i < m_iMaxEntries; i++)
		{
			// Ignore free slots
			if (m_pFreeList[i] == Slot::Free)
				continue;

			// Check if items match
			if (Utilities::areStringsEqual(operator[](i), pszEntry))
			{
				++iNumRemoved;

				// Mark slot as free
				m_pFreeList[i] = Slot::Free;

				// Update return value
				bRemovedAny = true;
			}
		}

		// Update counter
		m_iNumEntries = m_iNumEntries - iNumRemoved;

		return bRemovedAny;
	}

	void StringArray::clear()
	{
		m_iNumEntries = 0;

		// Mark list as free
		for (uint16_t i = 0; i < m_iMaxEntries; i++)
			m_pFreeList[i] = Slot::Free;
	}
}}
