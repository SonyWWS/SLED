/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include <cstdio>
#include <cstdarg>

#include "assert.h"

#if SCE_SLEDTARGET_OS_WINDOWS
	#ifndef _WINDOWS_
		#define WIN32_LEAN_AND_MEAN
		#include <windows.h>
	#endif
#endif

namespace sce { namespace Sled { namespace Assert
{
	FailureBehavior DefaultAssertHandler(const char *condition, const char *file, const int& line, const char *msg)
	{
		char buf[2048];
		std::sprintf(buf,
			"Assert failure: %s:%d %s%s%s %s\n",
			file,
			line, 
			(condition != NULL) ? "\"" : "", 
			(condition != NULL) ? condition : "", 
			(condition != NULL) ? "\"" : "", 
			(msg != NULL) ? msg : "");

		std::printf("%s", buf);

#if SCE_SLEDTARGET_OS_WINDOWS == 1
		::OutputDebugStringA(buf);
#endif

		return Assert::kHalt;
	}

	Handler& assertHandler()
	{
		static Handler s_assertHandler = &DefaultAssertHandler;
		return s_assertHandler;
	}

	void setAssertHandler(Handler assertHandlerFunc)
	{
		assertHandler() = assertHandlerFunc;
	}

	FailureBehavior reportFailure(const char *condition, const char *file, const int& line, const char *message, ...)
	{
		char expandedMessage[2048];
		expandedMessage[0] = '\0';

		if (message != NULL)
		{
			va_list args;
			va_start(args, message);
			std::vsprintf(expandedMessage, message, args);
			va_end(args);
		}

		return assertHandler()(condition, file, line, expandedMessage);
	}
}}}

namespace sce { namespace Sled { namespace Logging
{
	void DefaultLogHandler(Level lvl, const char *file, const int& line, const char *msg)
	{
		static const char *s_pLogLevels[] =
		{
			"INFO",
			"WARN",
			"ERR",
			NULL
		};

		char buf[2048];
		std::sprintf(buf, "SledDebugger:%s %s:%d - %s\n", s_pLogLevels[lvl], file, line, ((msg != NULL) ? msg : ""));
		std::printf("%s", buf);

#if SCE_SLEDTARGET_OS_WINDOWS == 1
		::OutputDebugStringA(buf);
#endif
	}

	Handler& logHandler()
	{
		static Handler s_logHandler = &DefaultLogHandler;
		return s_logHandler;
	}

	void setLogHandler(Handler logHandlerFunc)
	{
		logHandler() = logHandlerFunc;
	}

	void log(Level lvl, const char *file, const int& line, const char *message, ...)
	{
		char expandedMessage[2048];
		expandedMessage[0] = '\0';

		if (message != NULL)
		{
			va_list args;
			va_start(args, message);
			std::vsprintf(expandedMessage, message, args);
			va_end(args);
		}

		return logHandler()(lvl, file, line, expandedMessage);
	}
}}}
