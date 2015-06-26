/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDLUAPLUGIN_VARFILTER_H__
#define __SCE_LIBSLEDLUAPLUGIN_VARFILTER_H__

#include "../sledcore/base_types.h"
#include <cstddef>

#include "../sleddebugger/common.h"

namespace sce { namespace Sled
{
	// Forward declarations
	class ISequentialAllocator;
	class StringArray;

	// Forward declarations
	struct LuaPluginConfig;
	class VarFilterNameContainer;
	struct VarFilterNameContainerConfig;

	struct SCE_SLED_LINKAGE VarFilterNameConfig
	{
		VarFilterNameConfig(const VarFilterNameContainerConfig *pConfig);

		uint16_t maxPatterns;
		uint16_t maxPatternLen;
	};

	class SCE_SLED_LINKAGE VarFilterName
	{
	public:
		static int32_t create(const VarFilterNameConfig& varFilterNameConfig, void *pLocation, VarFilterName **pFilter);
		static int32_t requiredMemory(const VarFilterNameConfig& varFilterNameConfig, std::size_t *iRequiredMemory);
		static int32_t requiredMemoryHelper(const VarFilterNameConfig& varFilterNameConfig, ISequentialAllocator *pAllocator, void **ppThis);
		static void shutdown(VarFilterName *pFilter);
	private:
		VarFilterName(const VarFilterNameConfig& varFilterNameConfig, void *pMemoryPool);
	public:
		bool setup(char chWhat, const char *pszFilter);
		bool isMatch(const char *pszName);
	private:	
		bool			m_bFirst;
		bool			m_bLast;
		int32_t			m_iPatternLens;
		uint8_t			m_chWhat;
		StringArray*	m_pPatterns;
	private:
		friend class VarFilterNameContainer;
	};

	struct SCE_SLED_LINKAGE VarFilterNameContainerConfig
	{
		VarFilterNameContainerConfig(const LuaPluginConfig *pConfig);

		uint16_t		maxNumFilters;			///< Maximum number of variable filters
		uint16_t		maxPatternsPerFilter;	///< Maximum number of patterns per each filter
		uint16_t		maxPatternLen;			///< Maximum length of a pattern in a filter
	};

	class SCE_SLED_LINKAGE VarFilterNameContainer
	{
	public:
		static int32_t create(const VarFilterNameContainerConfig& varFilterContainerConfig, void *pLocation, VarFilterNameContainer **pContainer);
		static int32_t requiredMemory(const VarFilterNameContainerConfig& varFilterContainerConfig, std::size_t *iRequiredMemory);
		static int32_t requiredMemoryHelper(const VarFilterNameContainerConfig& varFilterContainerConfig, ISequentialAllocator *pAllocator, void **ppThis);
		static void shutdown(VarFilterNameContainer *pContainer);
	private:
		VarFilterNameContainer(const VarFilterNameContainerConfig& varFilterContainerConfig, const void *pContainerSeats);
		~VarFilterNameContainer();
		VarFilterNameContainer(const VarFilterNameContainer&);
		VarFilterNameContainer& operator=(const VarFilterNameContainer&);
	public:
		const uint16_t& getMaxFilters() const { return m_iMaxFilters; }
		uint16_t getNumFilters() const { return m_iNumFilters; }
		bool isEmpty() const { return m_iNumFilters == 0; }
		bool isFull() const { return m_iNumFilters == m_iMaxFilters; }	
	public:
		bool addFilter(char chWhat, const char *pszFilter);
		bool isFiltered(const char *pszName, char chWhat);
		void clear(char chWhat);
		void clearAll();
	private:
		const uint16_t		m_iMaxFilters;
		uint16_t			m_iNumFilters;
		VarFilterName**		m_ppFilters;
		uint8_t*			m_pFreeList;
	};
}}

#endif // __SCE_LIBSLEDLUAPLUGIN_VARFILTER_H__
