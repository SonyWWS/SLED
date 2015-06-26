/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_SEQUENTIALALLOCATOR_H__
#define __SCE_LIBSLEDDEBUGGER_SEQUENTIALALLOCATOR_H__

#include "../sledcore/base_types.h"
#include <cstdio>

#include "common.h"

/// Namespace for sce classes and functions.
namespace sce
{
/// Namespace for Sled classes and functions.
namespace Sled
{
#ifndef DOXYGEN_IGNORE
	/// @brief
	/// Sequential allocator interface.
	///
	/// Interface for sequential allocator.
	class SCE_SLED_LINKAGE ISequentialAllocator
	{
	public:
		/// <c>ISequentialAllocator</c> constructor.
		/// @brief
		/// Constructor.
		ISequentialAllocator() {}
		/// <c>ISequentialAllocator</c> destructor.
		/// @brief
		/// Destructor.
		virtual ~ISequentialAllocator() {}
	public:
		/// Allocate memory with given size and alignment.
		/// @brief
		/// Allocate memory.
		///
		/// @param size Memory size in bytes
		/// @param alignment Memory alignment
		///
		/// @return Allocated memory location
		///
		/// @see
		/// <c>bytesAllocated</c>
		virtual void *allocate(std::size_t size, std::size_t alignment) = 0;
		/// Get size of allocated memory.
		/// @brief
		/// Get allocated memory size.
		///
		/// @return Memory size in bytes
		///
		/// @see
		/// <c>allocate</c>
		virtual std::size_t bytesAllocated() const = 0;
	};

	/// @brief
	/// Sequential allocator class.
	///
	/// This class is used to determine the size needed for various items and takes into account alignment.
	class SCE_SLED_LINKAGE SequentialAllocator : public ISequentialAllocator
	{
	public:
		/// <c>SequentialAllocator</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param memory Allocated memory location
		/// @param memorySizeInBytes Allocated memory size in bytes
		SequentialAllocator(void *memory, std::size_t memorySizeInBytes);
		/// Allocate memory with given size and alignment.
		/// @brief
		/// Allocate memory.
		///
		/// @param size Memory size in bytes
		/// @param alignment Memory alignment
		///
		/// @return Allocated memory location
		///
		/// @see
		/// <c>bytesAllocated</c>
		virtual void *allocate(std::size_t size, std::size_t alignment);
		/// Get size of allocated memory.
		/// @brief
		/// Get allocated memory size.
		///
		/// @return Memory size in bytes
		///
		/// @see
		/// <c>allocate</c>
		virtual std::size_t bytesAllocated() const { return m_memoryAllocated; }
	private:
		SequentialAllocator(const SequentialAllocator&);
		SequentialAllocator& operator=(const SequentialAllocator&);
	private:
		uint8_t*			m_memory;	
		const std::size_t	m_memorySize;
		std::size_t			m_memoryAllocated;

	};

	/// @brief
	/// Sequential allocator calculator class.
	/// 
	/// This class is used to help determine the size needed for various items and takes into account alignment.
	class SCE_SLED_LINKAGE SequentialAllocatorCalculator : public ISequentialAllocator
	{
	public:
		/// <c>SequentialAllocatorCalculator</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param memory Allocated memory location
		SequentialAllocatorCalculator(void *memory = 0);
		/// Allocate memory with given size and alignment.
		/// @brief
		/// Allocate memory.
		///
		/// @param size Memory size in bytes
		/// @param alignment Memory alignment
		///
		/// @return Allocated memory location
		///
		/// @see
		/// <c>bytesAllocated</c>
		virtual void *allocate(std::size_t size, std::size_t alignment);
		/// Get size of allocated memory.
		/// @brief
		/// Get allocated memory size.
		///
		/// @return Memory size in bytes
		///
		/// @see
		/// <c>allocate</c>
		virtual std::size_t bytesAllocated() const { return m_memoryAllocated; }
	private:
		SequentialAllocatorCalculator(const SequentialAllocatorCalculator&);
		SequentialAllocatorCalculator& operator=(const SequentialAllocatorCalculator&);
	private:
		uint8_t*		m_memory;
		std::size_t		m_memoryAllocated;
	};
#endif //DOXYGEN_IGNORE
}}

#endif // __SCE_LIBSLEDDEBUGGER_SEQUENTIALALLOCATOR_H__
