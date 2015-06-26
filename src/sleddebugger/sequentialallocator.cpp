/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "sequentialallocator.h"
#include "assert.h"

#include <cstddef>

namespace sce { namespace Sled
{
	SequentialAllocator::SequentialAllocator(void *memory, std::size_t memorySizeInBytes)
		: m_memory(static_cast<uint8_t*>(memory))
		, m_memorySize(memorySizeInBytes)
		, m_memoryAllocated(0)
	{	
	}

	void *SequentialAllocator::allocate(std::size_t size, std::size_t alignment)
	{
		SCE_SLED_ASSERT_MSG(alignment > 0 && (alignment & (alignment - 1)) == 0, "Alignment must be power of two");

		const uintptr_t memStart = (uintptr_t)m_memory;
		const uintptr_t naturalBase = (uintptr_t)memStart + m_memoryAllocated;
		const uintptr_t alignedBase = (naturalBase + (alignment - 1)) & ~(alignment - 1);
		const uintptr_t newEnd = alignedBase + size;

		SCE_SLED_ASSERT_MSG(newEnd <= (memStart + m_memorySize), "Exhausted buffer!");
		if (newEnd > (memStart + m_memorySize))
			return NULL;

		uint8_t* const allocMem = (uint8_t*)alignedBase;
		m_memoryAllocated = newEnd - memStart;

		SCE_SLED_ASSERT_MSG((reinterpret_cast<uintptr_t>(allocMem) % alignment) == 0, "Did not give back properly aligned address!");

		return allocMem;
	}

	SequentialAllocatorCalculator::SequentialAllocatorCalculator(void *memory /* = 0 */)
		: m_memory(static_cast<uint8_t*>(memory))
		, m_memoryAllocated(0)
	{
	}

	void *SequentialAllocatorCalculator::allocate(std::size_t size, std::size_t alignment)
	{
		SCE_SLED_ASSERT_MSG(alignment > 0 && (alignment & (alignment - 1)) == 0, "Alignment must be power of two");

		const uintptr_t memStart = (uintptr_t)m_memory;
		const uintptr_t naturalBase = (uintptr_t)memStart + m_memoryAllocated;
		const uintptr_t alignedBase = (naturalBase + (alignment - 1)) & ~(alignment - 1);
		const uintptr_t newEnd = alignedBase + size;

		const std::size_t padding = 4;

		uint8_t* const allocMem = (uint8_t*)alignedBase;
		m_memoryAllocated = (newEnd - memStart) + padding;

		SCE_SLED_ASSERT_MSG((reinterpret_cast<uintptr_t>(allocMem) % alignment) == 0, "Did not give back properly aligned address!");

		return allocMem;
	}
}}
