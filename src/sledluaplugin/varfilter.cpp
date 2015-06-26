/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "varfilter.h"
#include "sledluaplugin.h"
#include "../sleddebugger/assert.h"
#include "../sleddebugger/errorcodes.h"
#include "../sleddebugger/sequentialallocator.h"
#include "../sleddebugger/stringarray.h"
#include "../sleddebugger/utilities.h"

#include <cstdio>
#include <cstring>
#include <cstdarg>
#include <ctype.h>
#include <new>

namespace sce { namespace Sled
{
	VarFilterNameConfig::VarFilterNameConfig(const VarFilterNameContainerConfig *pConfig)
	{
		maxPatterns = pConfig->maxPatternsPerFilter;
		maxPatternLen = pConfig->maxPatternLen;
	}

	namespace
	{
		struct VarFilterNameSeats
		{
			void *m_this;

			void *m_array;
			void *m_arrayPool;

			void Allocate(const VarFilterNameConfig& config, ISequentialAllocator *pAllocator)
			{
				m_this = pAllocator->allocate(sizeof(VarFilterName), __alignof(VarFilterName));
				m_array = pAllocator->allocate(sizeof(StringArray), __alignof(StringArray));
				m_arrayPool = pAllocator->allocate(config.maxPatterns * config.maxPatternLen, 4);
			}
		};

		inline int32_t ValidateConfig(const VarFilterNameConfig& config)
		{
			const bool bAnyPatterns = config.maxPatterns != 0;
			const bool bAnyLength = config.maxPatternLen != 0;

			if ((bAnyPatterns && !bAnyLength) || (!bAnyPatterns && bAnyLength))
				return SCE_SLED_ERROR_INVALIDCONFIGURATION;

			return SCE_SLED_ERROR_OK;
		}
	}

	int32_t VarFilterName::create(const VarFilterNameConfig& varFilterNameConfig, void *pLocation, VarFilterName **pFilter)
	{
		SCE_SLED_ASSERT(pLocation != NULL);
		SCE_SLED_ASSERT(pFilter != NULL);

		std::size_t iMemSize = 0;
		const int32_t iConfigError = requiredMemory(varFilterNameConfig, &iMemSize);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocator allocator(pLocation, iMemSize);

		VarFilterNameSeats seats;
		seats.Allocate(varFilterNameConfig, &allocator);

		SCE_SLED_ASSERT(seats.m_this != NULL);
		SCE_SLED_ASSERT(seats.m_array != NULL);
		SCE_SLED_ASSERT(seats.m_arrayPool != NULL);

		*pFilter = new (seats.m_this) VarFilterName(varFilterNameConfig, seats.m_array);
		return SCE_SLED_ERROR_OK;
	}

	int32_t VarFilterName::requiredMemory(const VarFilterNameConfig& varFilterNameConfig, std::size_t *iRequiredMemory)
	{
		SCE_SLED_ASSERT(iRequiredMemory != NULL);

		const int32_t iConfigError = ValidateConfig(varFilterNameConfig);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocatorCalculator allocator;

		VarFilterNameSeats seats;
		seats.Allocate(varFilterNameConfig, &allocator);

		*iRequiredMemory = allocator.bytesAllocated();
		return SCE_SLED_ERROR_OK;
	}

	int32_t VarFilterName::requiredMemoryHelper(const VarFilterNameConfig& varFilterNameConfig, ISequentialAllocator *pAllocator, void **ppThis)
	{
		SCE_SLED_ASSERT(pAllocator != NULL);
		SCE_SLED_ASSERT(ppThis != NULL);

		const int32_t iConfigError = ValidateConfig(varFilterNameConfig);
		if (iConfigError != 0)
			return iConfigError;

		VarFilterNameSeats seats;
		seats.Allocate(varFilterNameConfig, pAllocator);

		*ppThis = seats.m_this;
		return SCE_SLED_ERROR_OK;
	}

	void VarFilterName::shutdown(VarFilterName *pFilter)
	{
		SCE_SLED_ASSERT(pFilter != NULL);
		pFilter->~VarFilterName();
	}

	VarFilterName::VarFilterName(const VarFilterNameConfig& varFilterNameConfig, void *pMemoryPool)
		: m_bFirst(false)
		, m_bLast(false)
		, m_iPatternLens(0)
	{
		SCE_SLED_ASSERT(pMemoryPool != NULL);

		StringArrayConfig config;
		config.allowDuplicates = false;
		config.maxEntries = varFilterNameConfig.maxPatterns;
		config.maxEntryLen = varFilterNameConfig.maxPatternLen;
		StringArray::create(&config, pMemoryPool, &m_pPatterns);
	}

	bool VarFilterName::setup(char chWhat, const char *pszFilter)
	{
		SCE_SLED_ASSERT(pszFilter != NULL);

		m_chWhat = chWhat;

		const char chAsterisk = '*';
		const std::size_t len = std::strlen(pszFilter);

		if (pszFilter[0] == chAsterisk)
			m_bFirst = true;
		if (pszFilter[len - 1] == chAsterisk)
			m_bLast = true;

		// Start grabbing patterns out
		int iPos = m_bFirst ? 0 : -1;
		int iNewPos = -1;

		do
		{
			char szPattern[256];

			iNewPos = Utilities::findFirstOf(pszFilter, chAsterisk, iPos + 1);
			if (iNewPos != -1)
			{
				Utilities::copySubstring(szPattern, 256, pszFilter, iPos + 1, iNewPos - iPos - 1);
			}
			else
			{
				Utilities::copySubstring(szPattern, 256, pszFilter, iPos + 1, len - iPos - 1);
			}

			const int32_t patLen = (int32_t)std::strlen(szPattern);
			if (patLen > 0)
			{
				if (!m_pPatterns->add(szPattern))
					return false;

				// Sum up lengths
				m_iPatternLens += patLen;
			}

			iPos = iNewPos;

		 } while (iNewPos != -1);

		return true;
	}

	bool VarFilterName::isMatch(const char *pszName)
	{
		SCE_SLED_ASSERT(pszName != NULL);

		const int32_t nameLen = (int32_t)std::strlen(pszName);
		if (nameLen <= 0)
			return false;

		StringArrayIndexedConstIterator iter(m_pPatterns);

		if (!m_bFirst && !m_bLast && (m_pPatterns->getNumEntries() == 1))
		{
			// If first & last characters aren't asterisks and there's
			// only one pattern string then its a simple comparison
			if (Utilities::areStringsEqual(pszName, iter[0]))
				return true;
		}
		else
		{
			// Verify total length of patterns isn't bigger than the name
			if (m_iPatternLens > nameLen)
				return false;

			bool bFailed = false;
			int iPos = -1;
			const uint16_t iCount = m_pPatterns->getNumEntries();

			for (uint16_t i = 0; (i < iCount) && !bFailed; i++)
			{
				// Search for the pattern string in pszName
				iPos = Utilities::findFirstOf(pszName, iter[i], iPos + 1);

				// Didn't find pattern string in pszName
				if (iPos == -1)
				{
					bFailed = true;
					continue;
				}

				// On first iteration check bFirst condition
				if ((i == 0) && !m_bFirst && (iPos != 0))
				{
					bFailed = true;
					continue;
				}

				// On last iteration check bLast condition
				if ((i == (iCount - 1)) && !m_bLast && (iPos != (nameLen - (int32_t)std::strlen(iter[iCount - 1]))))
				{
					bFailed = true;
					continue;
				}
			}

			if (!bFailed)
				return true;
		}

		return false;
	}

	VarFilterNameContainerConfig::VarFilterNameContainerConfig(const LuaPluginConfig *pConfig)
	{
		SCE_SLED_ASSERT(pConfig != NULL);
		maxNumFilters = pConfig->maxNumVarFilters;
		maxPatternsPerFilter = pConfig->maxPatternsPerVarFilter;
		maxPatternLen = pConfig->maxVarFilterPatternLen;
	}

	namespace
	{
		struct VarFilterNameContainerSeats
		{
			void *m_this;	
			void *m_filters;
			void *m_freeList;
			void *m_pool;

			void Allocate(const VarFilterNameContainerConfig& varFilterContainerConfig, ISequentialAllocator *pAllocator)
			{
				// For this
				m_this = pAllocator->allocate(sizeof(VarFilterNameContainer), __alignof(VarFilterNameContainer));
		
				// For m_ppFilters
				m_filters = pAllocator->allocate(sizeof(VarFilterName*) * varFilterContainerConfig.maxNumFilters, __alignof(VarFilterName*));
		
				// For m_pFreeList
				m_freeList = pAllocator->allocate(sizeof(uint8_t) * varFilterContainerConfig.maxNumFilters, 4);

				void *poolStart = 0;
		
				// For m_pPool (contains any number of VarFilterName's)
				{
					// Figure out starting position of pool
					VarFilterNameConfig config(&varFilterContainerConfig);
					VarFilterName::requiredMemoryHelper(config, pAllocator, &m_pool);

					// Save for later
					poolStart = m_pool;

					// Loop through remaining for calculating total memory size needed
					for (uint16_t i = 1; i < varFilterContainerConfig.maxNumFilters; i++)
					{
						// For each VarFilterName
						VarFilterNameConfig newConfig(&varFilterContainerConfig);
						VarFilterName::requiredMemoryHelper(newConfig, pAllocator, &m_pool);
					}
				}

				// Restore start of pool
				m_pool = poolStart;
			}
		};

		inline int32_t ValidateConfig(const VarFilterNameContainerConfig& config)
		{
			const bool bAnyFilters = config.maxNumFilters != 0;
			const bool bAnyLength = config.maxPatternLen != 0;
			const bool bAnyPatternsPerFilter = config.maxPatternsPerFilter != 0;

			if (bAnyFilters && (!bAnyLength || !bAnyPatternsPerFilter))
				return SCE_SLED_ERROR_INVALIDCONFIGURATION;

			return SCE_SLED_ERROR_OK;
		}
	}

	int32_t VarFilterNameContainer::create(const VarFilterNameContainerConfig& varFilterContainerConfig, void *pLocation, VarFilterNameContainer **ppContainer)
	{
		SCE_SLED_ASSERT(pLocation != NULL);
		SCE_SLED_ASSERT(ppContainer != NULL);

		std::size_t iMemSize = 0;
		const int32_t iConfigError = requiredMemory(varFilterContainerConfig, &iMemSize);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocator allocator(pLocation, iMemSize);

		VarFilterNameContainerSeats seats;
		seats.Allocate(varFilterContainerConfig, &allocator);

		SCE_SLED_ASSERT(seats.m_this != NULL);
		SCE_SLED_ASSERT(seats.m_filters != NULL);
		SCE_SLED_ASSERT(seats.m_freeList != NULL);
		SCE_SLED_ASSERT(seats.m_pool != NULL);

		*ppContainer = new (seats.m_this) VarFilterNameContainer(varFilterContainerConfig, &seats);
		return SCE_SLED_ERROR_OK;
	}

	int32_t VarFilterNameContainer::requiredMemory(const VarFilterNameContainerConfig& varFilterContainerConfig, std::size_t *iRequiredMemory)
	{
		SCE_SLED_ASSERT(iRequiredMemory != NULL);

		const int32_t iConfigError = ValidateConfig(varFilterContainerConfig);
		if (iConfigError != 0)
			return iConfigError;

		SequentialAllocatorCalculator allocator;

		VarFilterNameContainerSeats seats;
		seats.Allocate(varFilterContainerConfig, &allocator);

		*iRequiredMemory = allocator.bytesAllocated();
		return SCE_SLED_ERROR_OK;
	}

	int32_t VarFilterNameContainer::requiredMemoryHelper(const VarFilterNameContainerConfig& varFilterContainerConfig, ISequentialAllocator *pAllocator, void **ppThis)
	{
		SCE_SLED_ASSERT(pAllocator != NULL);
		SCE_SLED_ASSERT(ppThis != NULL);

		const int32_t iConfigError = ValidateConfig(varFilterContainerConfig);
		if (iConfigError != 0)
			return iConfigError;

		VarFilterNameContainerSeats seats;
		seats.Allocate(varFilterContainerConfig, pAllocator);

		*ppThis = seats.m_this;
		return SCE_SLED_ERROR_OK;
	}

	void VarFilterNameContainer::shutdown(VarFilterNameContainer *pContainer)
	{
		SCE_SLED_ASSERT(pContainer != NULL);
		pContainer->~VarFilterNameContainer();
	}

	VarFilterNameContainer::VarFilterNameContainer(const VarFilterNameContainerConfig& varFilterContainerConfig, const void *pContainerSeats)
		: m_iMaxFilters(varFilterContainerConfig.maxNumFilters)
		, m_iNumFilters(0)
	{
		SCE_SLED_ASSERT(pContainerSeats != NULL);

		const VarFilterNameContainerSeats *pSeats = static_cast<const VarFilterNameContainerSeats*>(pContainerSeats);

		m_ppFilters = new (pSeats->m_filters) VarFilterName*[varFilterContainerConfig.maxNumFilters];
		m_pFreeList = new (pSeats->m_freeList) uint8_t[varFilterContainerConfig.maxNumFilters];

		// Mark list as free
		for (uint16_t i = 0; i < m_iMaxFilters; i++)
			m_pFreeList[i] = 1;

		const VarFilterNameConfig config(&varFilterContainerConfig);
	
		std::size_t iSizeOfVarNameFilter = 0;		
		VarFilterName::requiredMemory(config, &iSizeOfVarNameFilter);	

		SequentialAllocator allocator(pSeats->m_pool, (iSizeOfVarNameFilter * varFilterContainerConfig.maxNumFilters) + 1024);

		// Allocate var filters inside pool & hook up pointers
		for (uint16_t i = 0; i < m_iMaxFilters; i++)
		{
			void *pLocation = NULL;
			VarFilterName::requiredMemoryHelper(config, &allocator, &pLocation);

			VarFilterName *pFilter = 0;
			VarFilterName::create(config, pLocation, &pFilter);
			m_ppFilters[i] = pFilter;
		}
	}

	VarFilterNameContainer::~VarFilterNameContainer()
	{
	}

	bool VarFilterNameContainer::addFilter(char chWhat, const char *pszFilter)
	{
		SCE_SLED_ASSERT(pszFilter != NULL);

		if (isFull())
		{
			SCE_SLED_LOG(Logging::kInfo, "[SLED] Can't add variable filter as container is full! Item was (%c) %s!", chWhat, pszFilter);
			return false;
		}

		uint16_t iIndex = 0;

		// Find the first free spot
		for (uint16_t i = 0; i < m_iMaxFilters; i++)
		{
			if (m_pFreeList[i] == 1)
			{
				iIndex = i;
				break;
			}
		}

		// Set up new entry
		const bool bRetval = m_ppFilters[iIndex]->setup(chWhat, pszFilter);	
		if (bRetval)
		{
			// Mark spot as not free and increment item count
			m_pFreeList[iIndex] = 0;
			m_iNumFilters++;	
		}

		return bRetval;
	}

	bool VarFilterNameContainer::isFiltered(const char *pszName, char chWhat)
	{
		SCE_SLED_ASSERT(pszName != NULL);

		if (isEmpty())
			return false;

		for (uint16_t i = 0; i < m_iMaxFilters; i++)
		{
			// Skip free entries
			if (m_pFreeList[i] == 1)
				continue;

			// Skip if type doesn't match
			if (m_ppFilters[i]->m_chWhat != chWhat)
				continue;

			if (m_ppFilters[i]->isMatch(pszName))
				return true;
		}

		return false;
	}

	void VarFilterNameContainer::clear(char chWhat)
	{
		// Mark any non-free entries of type chWhat as free
		for (uint16_t i = 0; i < m_iMaxFilters; i++)
		{
			// Skip free entries
			if (m_pFreeList[i] == 1)
				continue;

			// Make it free if it matches
			if (m_ppFilters[i]->m_chWhat == chWhat)
			{
				// Clear patterns from this one
				m_ppFilters[i]->m_pPatterns->clear();
				// Make free
				m_pFreeList[i] = 1;
				// Decrement total filter count
				m_iNumFilters--;
			}
		}
	}

	void VarFilterNameContainer::clearAll()
	{
		m_iNumFilters = 0;

		// Mark list as free
		for (uint16_t i = 0; i < m_iMaxFilters; i++)
			m_pFreeList[i] = 1;
	}
}}
