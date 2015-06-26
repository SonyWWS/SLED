/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef SCE_SLEDPLATFORM_DATETIME_H
#define SCE_SLEDPLATFORM_DATETIME_H

#include "common.h"
#include <time.h> // time_t

#ifdef __cplusplus
extern "C" {
#endif

// --------------------------------------------------------------------------
// types

/** @brief An absolute point in time.
	@see <c>sceSledPlatformTimeGetCurrent</c>
	@see <c>sceSledPlatformTimeRelativeNanoseconds</c>, <c>sceSledPlatformTimeRelativeMicroseconds</c>
	@see <c>sceSledPlatformTimeRelativeMilliseconds</c>, <c>sceSledPlatformTimeRelativeSeconds</c> */
typedef int64_t SceSledPlatformTime;

/** @brief An interval between two points in time, using the same units as SceSledPlatformTime.
	@see <c>sceSledPlatformTimeIntervalToNanoseconds</c>, <c>sceSledPlatformTimeIntervalFromNanoseconds</c>
	@see <c>sceSledPlatformTimeIntervalToMicroseconds</c>, <c>sceSledPlatformTimeIntervalFromMicroseconds</c>
	@see <c>sceSledPlatformTimeIntervalToMilliseconds</c>, <c>sceSledPlatformTimeIntervalFromMilliseconds</c>
	@see <c>sceSledPlatformTimeIntervalToSeconds</c>, <c>sceSledPlatformTimeIntervalFromSeconds</c> */
typedef SceSledPlatformTime SceSledPlatformTimeInterval;

/** @brief An absolute date and time, like a file timestamp.
	
	A SceSledPlatformDate contains nanoseconds since the Unix epoch (00:00:00 Jan 1 1970 UTC)
	@see <c>SceSledPlatformStat</c>, <c>sceSledPlatformDateGetCurrent</c>, <c>sceSledPlatformDateFromComponents</c>, <c>sceSledPlatformDateToComponents</c> */
typedef uint64_t SceSledPlatformDate;

// --------------------------------------------------------------------------
// enums

#define SCE_SLEDPLATFORM_NANOSECONDS_PER_SECOND       1000000000LL  ///< Number of nanoseconds in a second.
#define SCE_SLEDPLATFORM_NANOSECONDS_PER_MILLISECOND  1000000LL     ///< Number of nanoseconds in a millisecond.
#define SCE_SLEDPLATFORM_MICROSECONDS_PER_SECOND      1000000LL     ///< Number of microseconds in a second.
#define SCE_SLEDPLATFORM_NANOSECONDS_PER_MICROSECOND  1000LL        ///< Number of nanoseconds in a microsecond.
#define SCE_SLEDPLATFORM_MICROSECONDS_PER_MILLISECOND 1000LL        ///< Number of microseconds in a millisecond.
#define SCE_SLEDPLATFORM_MILLISECONDS_PER_SECOND      1000LL        ///< Number of milliseconds in a second.
#define SCE_SLEDPLATFORM_SECONDS_PER_MINUTE           60LL          ///< Number of seconds in a minute.
#define SCE_SLEDPLATFORM_SECONDS_PER_HOUR             3600LL        ///< Number of seconds in an hour.
#define SCE_SLEDPLATFORM_SECONDS_PER_DAY              86400LL       ///< Number of seconds in a day.
#define SCE_SLEDPLATFORM_SECONDS_PER_YEAR             31557600LL    ///< Approximate number of seconds in a year.

#define SCE_SLEDPLATFORM_TIME_NULL         ((SceSledPlatformTime)0)                     ///< Special time value meaning "undefined"
#define SCE_SLEDPLATFORM_TIME_EARLIEST     ((SceSledPlatformTime)1)                     ///< Earliest possible time value.
#define SCE_SLEDPLATFORM_TIME_LATEST       ((SceSledPlatformTime)0x7FFFFFFFFFFFFFFFLL)  ///< Latest possible time value.

#define SCE_SLEDPLATFORM_DATE_NULL         ((SceSledPlatformTime)0)                     ///< Special date value meaning "undefined"
#define SCE_SLEDPLATFORM_DATE_LATEST       ((SceSledPlatformTime)0x7FFFFFFFFFFFFFFFLL)  ///< Latest possible date value.

// --------------------------------------------------------------------------
// time

/** @brief Gets the current time.
 	@result The current time. */
SCE_SLEDPLATFORM_LINKAGE SceSledPlatformTime sceSledPlatformTimeGetCurrent();

/** @brief Converts a SceSledPlatformTimeInterval to nanoseconds.
	@note The result is rounded upward, so an interval representing 1.5 nanoseconds will return the value 2.
	@param[in] interval   Interval to convert.
	@result The equivalent number of nanoseconds. */
SCE_SLEDPLATFORM_LINKAGE int64_t sceSledPlatformTimeIntervalToNanoseconds(SceSledPlatformTimeInterval interval);

/** @brief Converts nanoseconds to a SceSledPlatformTimeInterval.
	@param[in] ns         Nanoseconds to convert.
	@result The equivalent SceSledPlatformTimeInterval. */
SCE_SLEDPLATFORM_LINKAGE SceSledPlatformTimeInterval sceSledPlatformTimeIntervalFromNanoseconds(int64_t ns);

/** @brief Gets the Unix epoch as a SceSledPlatformTime.
	@result The SceSledPlatformTime equivalent to the Unix epoch (00:00:00 UTC 1970-01-01) */
SCE_SLEDPLATFORM_LINKAGE SceSledPlatformTime sceSledPlatformTimeGetUnixEpoch();

// --------------------------------------------------------------------------
// time utilities

/** @brief Converts microseconds to a SceSledPlatformTimeInterval.
	@param[in] us         Microseconds to convert.
	@result The equivalent SceSledPlatformTimeInterval. */
#define sceSledPlatformTimeIntervalFromMicroseconds(us)         \
	sceSledPlatformTimeIntervalFromNanoseconds((us) * SCE_SLEDPLATFORM_NANOSECONDS_PER_MICROSECOND)

/** @brief Converts milliseconds to a SceSledPlatformTimeInterval.
	@param[in] ms         Milliseconds to convert.
	@result The equivalent SceSledPlatformTimeInterval. */
#define sceSledPlatformTimeIntervalFromMilliseconds(ms)         \
	sceSledPlatformTimeIntervalFromNanoseconds((ms) * SCE_SLEDPLATFORM_NANOSECONDS_PER_MILLISECOND)

/** @brief Converts seconds to a SceSledPlatformTimeInterval.
	@param[in] s          Seconds to convert.
	@result The equivalent SceSledPlatformTimeInterval. */
#define sceSledPlatformTimeIntervalFromSeconds(s)               \
	sceSledPlatformTimeIntervalFromNanoseconds((s) * SCE_SLEDPLATFORM_NANOSECONDS_PER_SECOND)

/** @brief Converts a SceSledPlatformTimeInterval to microseconds.
	@note The result is rounded upward, so an interval representing 1.5 microseconds will return the value 2.
	@param[in] interval   Interval to convert.
	@result The equivalent number of microseconds. */
#define sceSledPlatformTimeIntervalToMicroseconds(interval)   \
	SCE_SLEDPLATFORM_DIVIDE_ROUNDING_UP(sceSledPlatformTimeIntervalToNanoseconds(interval), SCE_SLEDPLATFORM_NANOSECONDS_PER_MICROSECOND)

/** @brief Converts a SceSledPlatformTimeInterval to milliseconds.
	@note The result is rounded upward, so an interval representing 1.5 milliseconds will return the value 2.
	@param[in] interval   Interval to convert.
	@result The equivalent number of milliseconds. */
#define sceSledPlatformTimeIntervalToMilliseconds(interval)   \
	SCE_SLEDPLATFORM_DIVIDE_ROUNDING_UP(sceSledPlatformTimeIntervalToNanoseconds(interval), SCE_SLEDPLATFORM_NANOSECONDS_PER_MILLISECOND)

/** @brief Converts a SceSledPlatformTimeInterval to seconds.
	@note The result is rounded upward, so an interval representing 1.5 seconds will return the value 2.
	@param[in] interval   Interval to convert.
	@result The equivalent number of seconds. */
#define sceSledPlatformTimeIntervalToSeconds(interval)        \
	SCE_SLEDPLATFORM_DIVIDE_ROUNDING_UP(sceSledPlatformTimeIntervalToNanoseconds(interval), SCE_SLEDPLATFORM_NANOSECONDS_PER_SECOND)

/** @brief Returns a SceSledPlatformTime representing a number of nanoseconds from now.
	@param[in] ns         Nanoseconds from now.
	@result The equivalent SceSledPlatformTime. */
#define sceSledPlatformTimeRelativeNanoseconds(ns)            \
	(sceSledPlatformTimeGetCurrent() + sceSledPlatformTimeIntervalFromNanoseconds(ns))

/** @brief Returns a SceSledPlatformTime representing a number of microseconds from now.
	@param[in] us         Microseconds from now.
	@result The equivalent SceSledPlatformTime. */
#define sceSledPlatformTimeRelativeMicroseconds(us)           \
	(sceSledPlatformTimeGetCurrent() + sceSledPlatformTimeIntervalFromMicroseconds(us))

/** @brief Returns a SceSledPlatformTime representing a number of milliseconds from now.
	@param[in] ms         Milliseconds from now.
	@result The equivalent SceSledPlatformTime. */
#define sceSledPlatformTimeRelativeMilliseconds(ms)           \
	(sceSledPlatformTimeGetCurrent() + sceSledPlatformTimeIntervalFromMilliseconds(ms))

/** @brief Returns a SceSledPlatformTime representing a number of seconds from now.
	@param[in] s          Seconds from now.
	@result The equivalent SceSledPlatformTime. */
#define sceSledPlatformTimeRelativeSeconds(s)                 \
	(sceSledPlatformTimeGetCurrent() + sceSledPlatformTimeIntervalFromSeconds(s))

/** @brief Checks for a timeout condition.
	@param[in] start    The starting point.
	@param[in] timeout  The timeout interval.
	@result Returns true if the timeout interval has elapsed. */
#ifdef __cplusplus
inline SCE_SLEDPLATFORM_LINKAGE bool sceSledPlatformTimeoutElapsed(SceSledPlatformTime start, SceSledPlatformTimeInterval timeout) {
	SceSledPlatformTimeInterval elapsed = sceSledPlatformTimeGetCurrent() - start;
	return (elapsed > timeout);
}
#else
#define sceSledPlatformTimeoutElapsed(start,timeout)  ((sceSledPlatformTimeGetCurrent() - start) > timeout)
#endif

// --------------------------------------------------------------------------
// date

/** @brief Gets the current date.
 	@result The current date. */
SCE_SLEDPLATFORM_LINKAGE SceSledPlatformDate sceSledPlatformDateGetCurrent();

/** @brief Converts a struct tm into a SceSledPlatformDate.
	@param[in]  pComponents   Pointer to a struct tm.
	@result An equivalent date, or 0 if the date could not be represented. */
SCE_SLEDPLATFORM_LINKAGE SceSledPlatformDate sceSledPlatformDateFromComponents(const struct tm *pComponents);

/** @brief Converts a SceSledPlatformDate into a struct tm.
	@param[in]   date             Date to convert.
	@param[out]  pOutComponents   Filled in with the date components.
	@result The result is always equal to pOutComponents. */
SCE_SLEDPLATFORM_LINKAGE struct tm * sceSledPlatformDateToComponents(SceSledPlatformDate date, struct tm *pOutComponents);


#ifdef __cplusplus
}
#endif

#endif  /* SCE_SLEDPLATFORM_DATETIME_H */
