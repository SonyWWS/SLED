/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_ASSERT_H__
#define __SCE_LIBSLEDDEBUGGER_ASSERT_H__

#include "../sledcore/target_macros.h"
#include "common.h"

/// Namespace for sce classes and functions.
///
/// @brief
/// sce namespace.
namespace sce
{
/// Namespace for Sled classes and functions.
///
/// @brief
/// SLED namespace.
namespace Sled
{
/// Namespace for Assert classes and functions.
///
/// @brief
/// Assert namespace.
namespace Assert
{
	/// Failure behavior enumeration.
	/// @brief
	/// FailureBehavior enumeration.
	enum FailureBehavior
	{
		kHalt,		///< Halt execution
		kContinue	///< Continue execution
	};

	/// Typedef for the assert failure handler.
	/// @brief
	/// Typedef for assert failure handler.
	///
	/// @param condition The assert condition
	/// @param file The file in which the assert triggered
	/// @param line The line number of the assert
	/// @param message The message to display when the assert triggers
	///
	/// @return FailureBehavior.
	typedef FailureBehavior (*Handler)(const char *condition, const char *file, const int& line, const char *message);

	/// Get the assert handler.
	/// @brief
	/// Get assert handler.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @return Assert failure handler
	///
	/// @see
	/// <c>setAssertHandler</c>
	SCE_SLED_LINKAGE Handler& assertHandler();

	/// Set the assert handler.
	/// @brief
	/// Set assert handler.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param assertHandler The assert handler to set
	///
	/// @see
	/// <c>assertHandler</c>
	SCE_SLED_LINKAGE void setAssertHandler(Handler assertHandler);

	/// Report a failure.
	/// @brief
	/// Report failure.
	///
	/// @param condition The assert condition
	/// @param file The file in which the assert triggered
	/// @param line The line number of the assert
	/// @param message Description of failure data format
	/// @param ... Variable parameter list containing failure data
	///
	/// @return FailureBehavior describing failure
	///
	/// @see
	/// <c>setAssertHandler</c>
	SCE_SLED_LINKAGE FailureBehavior reportFailure(const char *condition, const char *file, const int& line, const char *message, ...);
}

#ifndef DOXYGEN_IGNORE
/// Namespace for Logging classes and functions.
///
/// @brief
/// Logging namespace.
namespace Logging
{
	/// Enumeration for logging level.
	/// @brief
	/// Log level enumeration.
	enum Level
	{
		kInfo		= 0,	///< Information level
		kWarning	= 1,	///< Warning level
		kError		= 2		///< Error level
	};

	/// Typedef for the log handler.
	/// @brief
	/// Typedef for log handler.
	///
	/// @param level Logging level
	/// @param file File in which the log function is used
	/// @param line Line to which the log entry applies
	/// @param message Log message
	typedef void (*Handler)(Level level, const char *file, const int& line, const char *message);

	/// Get the log handler.
	/// @brief
	/// Get log handler.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @return Log handler
	///
	/// @see
	/// <c>setLogHandler</c>
	SCE_SLED_LINKAGE Handler& logHandler();

	/// Set the log handler.
	/// @brief
	/// Set log handler.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param logHandler Log handler to set
	///
	/// @see
	/// <c>logHandler</c>
	SCE_SLED_LINKAGE void setLogHandler(Handler logHandler);

	/// The SLED logging function.
	/// @brief
	/// SLED logging function.
	///
	/// @param level Logging level
	/// @param file File in which the log function is used
	/// @param line Line to which the log entry applies
	/// @param message Description of log data format
	/// @param ... Variable parameter list containing data to log
	///
	/// @see
	/// <c>setLogHandler</c>
	SCE_SLED_LINKAGE void log(Level level, const char *file, const int& line, const char *message, ...);
}
#endif //DOXYGEN_IGNORE
}}

#ifdef SCE_SLED_ASSERT_ENABLED
	#define SCE_SLED_ASSERT(condition) \
		SCE_SLEDMACRO_BEGIN \
			if (SCE_SLEDUNLIKELY(!(condition))) { \
				if (sce::Sled::Assert::reportFailure(#condition, __FILE__, __LINE__, 0) == \
					sce::Sled::Assert::kHalt) \
					SCE_SLEDSTOP(); \
			} \
		SCE_SLEDMACRO_END

	#define SCE_SLED_ASSERT_MSG(condition, msg, ...) \
		SCE_SLEDMACRO_BEGIN \
			if (SCE_SLEDUNLIKELY(!(condition))) { \
				if (sce::Sled::Assert::reportFailure(#condition, __FILE__, __LINE__, (msg), ##__VA_ARGS__) == \
					sce::Sled::Assert::kHalt) \
					SCE_SLEDSTOP(); \
			} \
		SCE_SLEDMACRO_END

	#define SCE_SLED_LOG(lvl, msg, ...) \
		SCE_SLEDMACRO_BEGIN \
			sce::Sled::Logging::log((lvl), __FILE__, __LINE__, (msg), ##__VA_ARGS__); \
		SCE_SLEDMACRO_END

#else
	/// Test given assertion.
	/// @brief
	/// Test assertion.
	///
	/// @param condition Condition to test
	#define SCE_SLED_ASSERT(condition) SCE_SLEDUNUSED((condition))

	/// Test given assertion, providing message.
	/// @brief
	/// Test assertion, providing message.
	///
	/// @param condition Condition to test
	/// @param msg Description of message data format
	/// @param ... Variable parameter list containing message data
	///
	/// @see
	/// <c>SCE_SLED_LOG</c>, <c>SCE_SLED_VERIFY</c>
	#define SCE_SLED_ASSERT_MSG(condition, msg, ...) \
		SCE_SLEDMACRO_BEGIN \
			SCE_SLEDUNUSED((condition)); \
			SCE_SLEDUNUSED((msg)); \
		SCE_SLEDMACRO_END

	/// Log message with formatted data.
	/// @brief
	/// Log formatted message.
	///
	/// @param lvl Logging level
	/// @param msg Description of log data format
	/// @param ... Variable parameter list containing log data
	///
	/// @see
	/// <c>SCE_SLED_ASSERT_MSG</c>, <c>SCE_SLED_VERIFY</c>
	#define SCE_SLED_LOG(lvl, msg, ...) \
		SCE_SLEDMACRO_BEGIN \
			SCE_SLEDUNUSED((lvl)); \
			SCE_SLEDUNUSED((msg)); \
		SCE_SLEDMACRO_END

#endif // SCE_SLED_ASSERT_ENABLED

/// Check that function has expected value, raising assertion otherwise.
/// @brief
/// Check function value.
///
/// @param func Function to test
///
/// @see
/// <c>SCE_SLED_LOG</c>, <c>SCE_SLED_ASSERT_MSG</c>
#define SCE_SLED_VERIFY(func) \
	SCE_SLEDMACRO_BEGIN \
		if (SCE_SLEDUNLIKELY(!(func))) { \
			if (sce::Sled::Assert::reportFailure(#func, __FILE__, __LINE__, 0) == sce::Sled::Assert::kHalt) \
				SCE_SLEDSTOP(); \
		} \
	SCE_SLEDMACRO_END

#endif // __SCE_LIBSLEDDEBUGGER_ASSERT_H__
