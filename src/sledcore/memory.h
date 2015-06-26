/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef SCE_SLEDMEMORY_H
#define SCE_SLEDMEMORY_H

#include "target_macros.h"
#include "base_types.h"
#include <stddef.h>						// for size_t


///	@brief
///	A #define to be included in a class to provide placement new functionality.
///	@par	Example
///	Classes use it like this:
///	@code
/// class object {
///     SCE_SLEDPLACEMENT_NEW_AND_DELETE
///     object();
///     ~object();
/// };
///	@endcode
/// Clients use it like this:
///	@code
///	object *pObj = new (&obj) object;
///	pObj->~object();
///	@endcode
#ifndef SCE_SLEDPLACEMENT_NEW_AND_DELETE
#define SCE_SLEDPLACEMENT_NEW_AND_DELETE \
 inline static void * operator new(size_t, void *p) { return p; } \
 inline static void   operator delete(void *, void *) {}
#endif

///	@brief
///	A #define to be included in a class to provide allocator-based operator new and delete.
///	@par	Example
///	Classes use it like this:
///	@code
///	class object {
///     SCE_SLEDALLOCATOR_NEW_AND_DELETE(object,"object")
///     object();
///     ~object();
/// };
///	@endcode
/// Clients use it like this:
///	@code
///	object *pObj = new (pAllocator) object;
///	pObj->DeleteWithAllocator(a);
///	@endcode
#define SCE_SLEDALLOCATOR_NEW_AND_DELETE(type,name)                             \
 inline static void * operator new(size_t s, const SceSledMemoryAllocator *a)   \
      { return a->Allocate(a->pAllocatorContext,s,__alignof(type),name,__FILE__,__LINE__); }   \
 inline static void   operator delete(void *p, const SceSledMemoryAllocator *a) \
      { a->Deallocate(a->pAllocatorContext,p,__alignof(type),name,__FILE__,__LINE__); }        \
 inline void DeleteWithAllocator(const SceSledMemoryAllocator *a)               \
      { this->~type(); a->Deallocate(a->pAllocatorContext,this,__alignof(type),name,__FILE__,__LINE__); }

///	@brief
///	A #define to be included in a class to disable standard delete.
///	@note	After inclusion of this macro, calling delete on the including class will cause a runtime halt.
///	@par	Example
/// Classes use it like this:
///	@code
/// class object {
///     SCE_SLEDDISABLE_STANDARD_DELETE
///     object();
///     ~object();
/// };
///	@endcode
/// This prevents clients from doing this, which would otherwise invoke the global operator delete:
///
///	@code
/// delete pObj;
///	@endcode
#define SCE_SLEDDISABLE_STANDARD_DELETE \
    inline static void operator delete(void *) { SCE_SLEDNORETURN_STOP(); }

/** Function pointer definition for handling of WWS SDK allocations through a custom allocator structure.

	@param pAllocatorContext	A pointer to some blind context to be passed by the caller.
	@param sizeInBytes			Size of the allocation in bytes.
	@param alignmentInBytes		Alignment of the allocation in bytes.
	@param pIdentifier			An identifier string (usually the calling component name).
	@param pFile				The filename in which the allocation originated.
	@param line					The line number within the file at which the allocation originated.
	@return						A pointer to the allocated memory				
*/
typedef void * (*SceSledMemoryAllocateFunc)(
	void*					pAllocatorContext,
	size_t					sizeInBytes,
	size_t					alignmentInBytes,
	const char*				pIdentifier,
	const char*				pFile,
	int						line);


/** Function pointer definition for handling of WWS SDK deallocations through a custom allocator structure.

	@param pAllocatorContext	A pointer to some blind context to be passed by the caller.
	@param pMemory				A pointer to the memory to be deallocated (should have been allocated through the same struct).
	@param alignmentInBytes		Alignment of the allocation in bytes.
	@param pIdentifier			An identifier string (usually the calling component name).
	@param pFile				The filename in which the allocation originated.
	@param line					The line number within the file at which the allocation originated.
**/
typedef void (*SceSledMemoryDeallocateFunc)(
	void*					pAllocatorContext,
	void*					pMemory,
	size_t					alignmentInBytes,
	const char*				pIdentifier,
	const char*				pFile,
	int						line);

/// @brief
/// Structure that defines a custom memory allocator.
typedef struct SceSledMemoryAllocator
{
	void*					pAllocatorContext;	///< Custom allocator context
	SceSledMemoryAllocateFunc   Allocate;			///< User-specified allocation callback.
	SceSledMemoryDeallocateFunc Deallocate;			///< User-specified deallocation callback.
} SceSledMemoryAllocator;



#endif	//SCE_SLEDMEMORY_H

