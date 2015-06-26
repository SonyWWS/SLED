/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_UTILITIES_H__
#define __SCE_LIBSLEDDEBUGGER_UTILITIES_H__

#include <cstddef>

#include "common.h"

/// Namespace for sce classes and functions.
namespace sce
{
/// Namespace for Sled classes and functions.
namespace Sled
{
/// Namespace for Utilities classes and functions.
///
/// @brief
/// Utilities namespace.
namespace Utilities
{
	/// Copy one string to another string.
	/// @brief
	/// Copy string to another string.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param pszCopyTo Target string. Cannot be NULL.
	/// @param len Maximum size of the target string buffer
	/// @param pszCopyFrom Source string. Can be NULL.
	///
	/// @see
	/// <c>appendString</c>, <c>areStringsEqual</c>,
	/// <c>findFirstOf</c>, <c>copySubstring</c>
	SCE_SLED_LINKAGE void copyString(char *pszCopyTo, std::size_t len, const char *pszCopyFrom);

	/// Append one string to another existing string.
	/// @brief
	/// Append one string to another string.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param pszAppendTo Target string. Cannot be NULL and should already be initialized.
	/// @param len Maximum size of the target string buffer
	/// @param pszAppendFrom Source string. Can be NULL.
	///
	/// @see
	/// <c>copyString</c>, <c>areStringsEqual</c>,
	/// <c>findFirstOf</c>, <c>copySubstring</c>
	SCE_SLED_LINKAGE void appendString(char *pszAppendTo, std::size_t len, const char *pszAppendFrom);

	/// Check whether or not two strings are equal.
	/// @brief
	/// Check whether or not strings equal.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param pszString1 String to use in comparison
	/// @param pszString2 String to use in comparison
	///
	/// @return True if strings are equal; false if they are not
	///
	/// @see
	/// <c>copyString</c>, <c>appendString</c>,
	/// <c>findFirstOf</c>, <c>copySubstring</c>
	SCE_SLED_LINKAGE bool areStringsEqual(const char *pszString1, const char *pszString2);

	/// Find the first occurrence of a character in a target string, starting from a specified position in that target string.
	/// @brief
	/// Find first occurrence of character in target string, starting from specified position in target string.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param pszSearch The string to look in
	/// @param chWhat The character to look for
	/// @param iStartPos The position in <c>pszSearch</c> to start looking
	///
	/// @return Position in <c>pszSearch</c> where searched-for character exists, or -1 if character was not found 
	/// or if starting position is invalid
	///
	/// @see
	/// <c>copyString</c>, <c>appendString</c>, <c>areStringsEqual</c>,
	/// <c>copySubstring</c>
	SCE_SLED_LINKAGE int findFirstOf(const char *pszSearch, char chWhat, int iStartPos);

	/// Find the first occurrence of a string in a target string, starting from a specified position in that target string.
	/// @brief
	/// Find first occurrence of string in target string, starting from specified position in target string.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param pszSearch The string to look in
	/// @param pszWhat The string to look for
	/// @param iStartPos The position in <c>pszSearch</c> to start looking
	///
	/// @return Position in <c>pszSearch</c> where searched-for string starts, or -1 if string was not found 
	/// or starting position is invalid.
	///
	/// @see
	/// <c>copyString</c>, <c>appendString</c>, <c>areStringsEqual</c>,
	/// <c>copySubstring</c>
	SCE_SLED_LINKAGE int findFirstOf(const char *pszSearch, const char *pszWhat, int iStartPos);

	/// Copy one string to another string,
	/// @brief
	/// Copy string to another string.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param pszCopyTo Target string. Cannot be NULL.
	/// @param len Maximum size of the target string buffer
	/// @param pszCopyFrom Source string. Cannot be NULL.
	/// @param iStartPos Starting position in the source string where the copy starts
	/// @param iCopyLen Number of characters to copy
	///
	/// @see
	/// <c>copyString</c>, <c>appendString</c>, <c>areStringsEqual</c>,
	/// <c>findFirstOf</c>
	SCE_SLED_LINKAGE void copySubstring(char *pszCopyTo, std::size_t len, const char *pszCopyFrom, const std::size_t& iStartPos, const std::size_t& iCopyLen);

	/// Typedef used to signal when the library needs a file opened by client code.
	/// @brief
	/// Typedef used to signal when library needs file opened by client code.
	///
	/// @param pszFilePath Path to the file that the client code needs to open
	/// @param pUserData Optional user provided data
	///
	/// @return File contents
	///
	/// @see
	/// <c>FileFinishCallback</c>, <c>openFileCallback</c>, <c>openFileFinishCallback</c>
	typedef const char *(*FileCallback)(const char *pszFilePath, void *pUserData);

	/// Typedef used to signal when the library is done using the file contents that the client code provided.
	/// @brief
	/// Typedef used to signal when library is done using file contents that client code provided.
	///
	/// @param pszFilePath Path to file that client code opened
	/// @param pUserData Optional user provided data
	///
	/// @see
	/// <c>FileCallback</c>, <c>openFileCallback</c>, <c>openFileFinishCallback</c>
	typedef void (*FileFinishCallback)(const char *pszFilePath, void *pUserData);

	/// Get or set the open file callback to use.
	/// @brief
	/// Get or set open file callback to use.
	///
	/// @return Open file callback to use
	///
	/// @see
	/// <c>FileCallback</c>, <c>FileFinishCallback</c>, <c>openFileFinishCallback</c>
	SCE_SLED_LINKAGE FileCallback& openFileCallback();

	/// Get or set the open file finish callback to use.
	/// @brief
	/// Get or set open file finish callback to use.
	///
	/// @return Open file finish callback to use
	///
	/// @see
	/// <c>FileCallback</c>, <c>FileFinishCallback</c>, <c>openFileCallback</c>
	SCE_SLED_LINKAGE FileFinishCallback& openFileFinishCallback();
}}}

#endif // __SCE_LIBSLEDDEBUGGER_UTILITIES_H__
