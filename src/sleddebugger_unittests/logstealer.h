/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "../sleddebugger/assert.h"

namespace sce { namespace Sled
{
	class LogStealer
	{
	public:
		LogStealer()
		{
			// Grab current handler
			m_handler = sce::Sled::Logging::logHandler();

			// Install our own handler
			sce::Sled::Logging::setLogHandler(LogFunc);
		}

		~LogStealer()
		{
			// Restore previous handler
			sce::Sled::Logging::setLogHandler(m_handler);
		}

	private:
		static void LogFunc(sce::Sled::Logging::Level lvl, const char *file, const int& line, const char *msg)
		{
			SCE_SLEDUNUSED(lvl);
			SCE_SLEDUNUSED(file);
			SCE_SLEDUNUSED(line);
			SCE_SLEDUNUSED(msg);

			// Do nothing
		}

	private:
		sce::Sled::Logging::Handler m_handler;
	};
}}
