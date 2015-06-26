/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_STRINGARRAY_H__
#define __SCE_LIBSLEDDEBUGGER_STRINGARRAY_H__

#include "../sledcore/base_types.h"
#include <cstdio>

#include "common.h"

/// Namespace for sce classes and functions.
namespace sce
{
/// Namespace for Sled classes and functions.
namespace Sled
{
	// Forward declaration.
	class ISequentialAllocator;

#ifndef DOXYGEN_IGNORE
	/// <c>StringArray</c> configuration structure.
	/// @brief
	/// <c>StringArray</c> configuration struct.
	struct SCE_SLED_LINKAGE StringArrayConfig
	{
		uint16_t maxEntryLen;	///< Maximum length of an entry
		uint16_t maxEntries;	///< Maximum number of entries
		bool allowDuplicates;	///< Whether or not to allow duplicates
	};

	// Forward declaration.
	class StringArray;

	/// String array const iterator structure. Helper class to iterate through a <c>StringArray</c>.
	/// @brief
	/// String array const iterator structure.
	///
	/// @param Example
	/// @code
	/// StringArrayConstIterator iter(pStringArray);
	/// for (; iter(); ++iter)
	/// {
	///     printf("Item: %s\n", iter.Get());
	/// }
	/// @endcode
	class SCE_SLED_LINKAGE StringArrayConstIterator
	{
	public:
		/// <c>StringArrayConstIterator</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param pStringArray <c>StringArray</c> to iterate through.
		StringArrayConstIterator(const StringArray *pStringArray);

		/// <c>StringArrayConstIterator</c> destructor.
		/// @brief
		/// Destructor.
		~StringArrayConstIterator() {}
	private:
		StringArrayConstIterator();
		StringArrayConstIterator(const StringArrayConstIterator&);
		StringArrayConstIterator& operator=(const StringArrayConstIterator&);
	public:
		/// Check whether or not the item exists at the current iteration.
		/// @brief
		/// Check whether or not item exists at current iteration.
		///
		/// @retval True if the item exists; false if at the end of iteration
		///
		/// @see
		/// <c>get</c>, <c>reset</c>
		bool operator()() const;

		/// Get the current item that the iterator points to.
		/// @brief
		/// Get current item iterator points to.
		///
		/// @retval Item from <c>StringArray</c>
		///
		/// @see
		/// <c>reset</c>
		const char *get() const;
	public:
		/// Iterate to the next item in the <c>StringArray</c>.
		/// @brief
		/// Iterate to next item in <c>StringArray</c>.
		///
		/// @retval Reference to iterator after updating internal iteration index
		///
		/// @see
		/// <c>get</c>, <c>reset</c>
		StringArrayConstIterator& operator++();

		/// Reset iteration to the starting item in the <c>StringArray</c>.
		/// @brief
		/// Reset iteration to starting item in <c>StringArray</c>.
		///
		/// @see
		/// <c>get</c>
		void reset();
	private:
		const StringArray *const	m_pStringArray;
		uint16_t m_iIndex;
	};

	/// String array indexed const iterator structure. Helper class to have indexed-based iteration over/access to a string array.
	/// @brief
	/// String array indexed const iterator structure.
	///
	/// @param Example
	/// @code
	/// StringArrayIndexedConstIterator iter(pStringArray);
	/// for (uint16_t i = 0; i < iter.GetCount(); i++)
	/// {
	///     printf("Item %i: %s\n", i, iter[i]);
	/// }
	/// @endcode
	class SCE_SLED_LINKAGE StringArrayIndexedConstIterator
	{
	public:
		/// <c>StringArrayIndexedConstIterator</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param pStringArray The <c>StringArray</c> to iterate through
		StringArrayIndexedConstIterator(const StringArray *pStringArray);

		/// <c>StringArrayIndexedConstIterator</c> destructor.
		/// @brief
		/// Destructor.
		~StringArrayIndexedConstIterator() {}
	private:
		StringArrayIndexedConstIterator();
		StringArrayIndexedConstIterator(const StringArrayIndexedConstIterator&);
		StringArrayIndexedConstIterator& operator=(const StringArrayIndexedConstIterator&);
	public:
		/// Get the number of items in the <c>StringArray</c>.
		/// @brief
		/// Get number of items in <c>StringArray</c>.
		///
		/// @retval The number of items in the <c>StringArray</c>
		uint16_t getCount() const;

		/// Get the item at a specific index in the <c>StringArray</c>.
		/// @brief
		/// Get item at specific index in <c>StringArray</c>.
		///
		/// @param iIndex Index of the item to get
		///
		/// @retval The item at given index in <c>StringArray</c>
		const char *operator[](uint16_t iIndex) const;
	private:
		const StringArray *const	m_pStringArray;
	};

	/// Class for string arrays.
	/// @brief
	/// String array class.
	class SCE_SLED_LINKAGE StringArray
	{
		friend class StringArrayConstIterator;
		friend class StringArrayIndexedConstIterator;
	public:
		/// Create a <c>StringArray</c> instance.
		/// @brief
		/// Create <c>StringArray</c> instance.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pConfig Configuration structure that details the settings to use
		/// @param pLocation Location in memory in which to place the <c>StringArray</c> instance.
		/// It needs to be as big as the value returned by <c>requiredMemory()</c>.
		/// @param ppArray <c>StringArray</c> instance that is created
		///
		/// @retval 0										Success
		/// @retval SCE_SLED_ERROR_INVALIDCONFIGURATION		Invalid value in the configuration structure
		///
		/// @see
		/// <c>requiredMemory</c>, <c>shutdown</c>
		static int32_t create(const StringArrayConfig *pConfig, void *pLocation, StringArray **ppArray);

		/// Calculate the size in bytes required for a <c>StringArray</c> instance, based on a configuration structure.
		/// @brief
		/// Calculate size in bytes required for <c>StringArray</c>, based on configuration structure.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pConfig Configuration structure that details the settings to use
		/// @param iRequiredMemory The amount of memory that is needed for the <c>StringArray</c> instance
		///
		/// @retval 0										Success
		/// @retval SCE_SLED_ERROR_INVALIDCONFIGURATION		Invalid value in the configuration structure
		///
		/// @see
		/// <c>create</c>
		static int32_t requiredMemory(const StringArrayConfig *pConfig, std::size_t *iRequiredMemory);

#ifndef DOXYGEN_IGNORE
		static int32_t requiredMemoryHelper(const StringArrayConfig *pConfig, ISequentialAllocator *pAllocator, void **ppThis);
#endif // DOXYGEN_IGNORE

		/// Shut down a <c>StringArray</c> instance.
		/// @brief
		/// Shut down <c>StringArray</c> instance.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pStringArray <c>StringArray</c> instance to shut down
		///
		/// @see
		/// <c>create</c>
		static void shutdown(StringArray *pStringArray);
	private:
		StringArray(const StringArrayConfig *pConfig, const void *pStringArraySeats);
		~StringArray() {}
		StringArray(const StringArray&);
		StringArray& operator=(const StringArray&);
	private:
		const char *operator[](uint16_t iIndex) const;
		uint16_t getFirstFreeSlotIndex(const uint16_t& iStartPos = 0) const;
		uint16_t getFirstFullSlotIndex(const uint16_t& iStartPos = 0) const;
	public:
		/// Determine whether or not the <c>StringArray</c> is empty.
		/// @brief
		/// Determine whether or not <c>StringArray</c> is empty.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Whether or not the <c>StringArray</c> is empty
		///
		/// @see
		/// <c>isFull</c>, <c>getNumEntries</c>, <c>getMaxEntries</c>, <c>getMaxEntryLen</c>
		inline bool isEmpty() const { return m_iNumEntries == 0; }

		/// Determine whether or not the <c>StringArray</c> is full.
		/// @brief
		/// Determine whether or not <c>StringArray</c> is full.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Whether or not the <c>StringArray</c> is full
		///
		/// @see
		/// <c>isEmpty</c>, <c>getNumEntries</c>, <c>getMaxEntries</c>, <c>getMaxEntryLen</c>
		inline bool isFull() const { return m_iNumEntries == m_iMaxEntries; }

		/// Determine whether or not duplicate entries are allowed.
		/// @brief
		/// Determine whether or not duplicates allowed.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Whether or not duplicates are allowed
		inline bool allowDuplicates() const { return m_allowDups != 0; }

		/// Get the number of entries.
		/// @brief
		/// Get number of entries.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Number of entries
		///
		/// @see
		/// <c>isEmpty</c>, <c>isFull</c>, <c>getMaxEntries</c>, <c>getMaxEntryLen</c>
		inline uint16_t getNumEntries() const { return m_iNumEntries; }

		/// Get the maximum number of entries.
		/// @brief
		/// Get maximum number of entries.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Maximum number of entries
		///
		/// @see
		/// <c>isEmpty</c>, <c>isFull</c>, <c>getNumEntries</c>, <c>getMaxEntryLen</c>
		inline uint16_t getMaxEntries() const { return m_iMaxEntries; }

		/// Get the maximum entry length.
		/// @brief
		/// Get maximum entry length.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Maximum entry length
		///
		/// @see
		/// <c>isEmpty</c>, <c>isFull</c>, <c>getNumEntries</c>, <c>getMaxEntries</c>
		inline uint16_t getMaxEntryLen() const { return m_iMaxEntryLen; }
	public:
		/// Add a string to the <c>StringArray</c>.
		/// @brief
		/// Add string to <c>StringArray</c>.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pszEntry String to add
		///
		/// @return True if the string is added; false if the string was not added or there was an error
		///
		/// @see
		/// <c>remove</c>, <c>clear</c>
		bool add(const char *pszEntry);

		/// Remove a string from the <c>StringArray</c>.
		/// @brief
		/// Remove string from <c>StringArray</c>.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pszEntry String to remove
		///
		/// @return True if the string was removed; false if the string was not removed or there was an error
		///
		/// @see
		/// <c>add</c>, <c>clear</c>
		bool remove(const char *pszEntry);

		/// Clear the contents of <c>StringArray</c>.
		/// @brief
		/// Clear contents of <c>StringArray</c>.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @see
		/// <c>add</c>, <c>remove</c>
		void clear();
	private:
		const uint16_t	m_iMaxEntries;
		const uint16_t	m_iMaxEntryLen;
		const uint8_t	m_allowDups;
		uint16_t		m_iNumEntries;
		uint8_t*		m_pBuffer;
		uint8_t*		m_pFreeList;
	};
#endif //DOXYGEN_IGNORE
}}

#endif // __SCE_LIBSLEDDEBUGGER_STRINGARRAY_H__
