/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "sleddebugger.h"
#include "sleddebugger_class.h"
#include <new>

namespace sce { namespace Sled
{
	int32_t debuggerCreate(const SledDebuggerConfig *config, void *location, SledDebugger **outDebugger)
	{
		if (config == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return SledDebugger::create(*config, location, outDebugger);
	}

	int32_t debuggerRequiredMemory(const SledDebuggerConfig *config, std::size_t *outRequiredMemory)
	{
		if (config == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return SledDebugger::requiredMemory(*config, outRequiredMemory);
	}

	int32_t debuggerShutdown(SledDebugger *debugger)
	{
		return SledDebugger::close(debugger);
	}

	int32_t debuggerStartNetworking(SledDebugger *debugger)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return debugger->startNetworking();
	}

	int32_t debuggerStopNetworking(SledDebugger *debugger)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return debugger->stopNetworking();
	}

	int32_t debuggerUpdate(SledDebugger *debugger)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return debugger->update();
	}

	int32_t debuggerGetVersion(const SledDebugger *debugger, Version *outVersion)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		if (outVersion == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outVersion) = debugger->getVersion();
		return SCE_SLED_ERROR_OK;
	}

	int32_t debuggerIsConnected(const SledDebugger *debugger, bool *outResult)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		if (outResult == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outResult) = debugger->isDebuggerConnected();
		return SCE_SLED_ERROR_OK;
	}

	int32_t debuggerIsNetworking(const SledDebugger *debugger, bool *outResult)
	{
		if (outResult == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outResult) = debugger->isNetworking();
		return SCE_SLED_ERROR_OK;
	}

	int32_t debuggerAddPlugin(SledDebugger *debugger, SledDebuggerPlugin *plugin)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return debugger->addPlugin(plugin);
	}

	int32_t debuggerRemovePlugin(SledDebugger *debugger, SledDebuggerPlugin *plugin)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return debugger->removePlugin(plugin);
	}

	int32_t debuggerScriptCacheAdd(SledDebugger *debugger, const char *relativePathToScriptFile, bool *outResult)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		if (outResult == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outResult) = debugger->scriptCacheAdd(relativePathToScriptFile);
		return SCE_SLED_ERROR_OK;
	}

	int32_t debuggerScriptCacheRemove(SledDebugger *debugger, const char *relativePathToScriptFile, bool *outResult)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		if (outResult == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outResult) = debugger->scriptCacheRemove(relativePathToScriptFile);
		return SCE_SLED_ERROR_OK;
	}

	int32_t debuggerScriptCacheClear(SledDebugger *debugger)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		debugger->scriptCacheClear();
		return SCE_SLED_ERROR_OK;
	}

	int32_t debuggerGetDebuggerMode(const SledDebugger *debugger, DebuggerMode::Enum *outDebuggerMode)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		if (outDebuggerMode == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		(*outDebuggerMode) = debugger->getDebuggerMode();
		return SCE_SLED_ERROR_OK;
	}

	int32_t debuggerTtyNotify(SledDebugger *debugger, const char *pszMessage)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return debugger->ttyNotify(pszMessage);
	}

	int32_t debuggerBreakpointReached(SledDebugger *debugger, const BreakpointParams *params)
	{
		if (debugger == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return debugger->breakpointReached(params);
	}

	int32_t debuggerGenerateHash(const char *pszString, int32_t line, int32_t *outHash)
	{
		if (outHash == NULL)
			return SCE_SLED_ERROR_NULLPARAMETER;

		return SledDebugger::generateHash(pszString, line, outHash);
	}
}}
