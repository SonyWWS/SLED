/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_H__
#define __SCE_LIBSLEDDEBUGGER_H__

/* libsleddebugger version information */
#define SCE_LIBSLEDDEBUGGER_VER_MAJOR		5 ///< LibSledDebugger version details - major version number
#define SCE_LIBSLEDDEBUGGER_VER_MINOR		1 ///< LibSledDebugger version details - minor version number
#define SCE_LIBSLEDDEBUGGER_VER_REVISION	2 ///< LibSledDebugger version details - revision version number
#define SCE_LIBSLEDDEBUGGER_VER_OTHER		0 ///< LibSledDebugger version details - extra version number

#include "errorcodes.h"
#include "params.h"
#include "common.h"

#include <cstdio>

/// Namespace for sce classes and functions.
namespace sce
{
/// Namespace for Sled classes and functions.
namespace Sled
{
	class SledDebugger;
	class SledDebuggerPlugin;

	/// Create a <c>SledDebugger</c> instance.
	/// @brief
	/// Create <c>SledDebugger</c> instance.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param config Configuration structure that details the settings to use
	/// @param location Location in memory in which to place the <c>SledDebugger</c> instance. 
	/// It needs to be as big as the value returned by <c>debuggerRequiredMemory()</c>.
	/// @param outDebugger <c>SledDebugger</c> instance that is created
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Configuration structure is null
	/// @retval SCE_SLED_ERROR_INVALIDCONFIGURATION	Invalid value in the configuration structure
	///
	/// @see
	/// <c>debuggerRequiredMemory</c>, <c>debuggerShutdown</c>
	SCE_SLED_LINKAGE int32_t debuggerCreate(const SledDebuggerConfig *config, void *location, SledDebugger **outDebugger);

	/// Calculate the size in bytes required for a <c>SledDebugger</c> instance based on a configuration structure.
	/// @brief
	/// Calculate size in bytes required for <c>SledDebugger</c> instance based on configuration structure.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param config Configuration structure that details the settings to use
	/// @param outRequiredMemory The amount of memory that is needed for the <c>SledDebugger</c> instance
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Configuration structure is null
	/// @retval SCE_SLED_ERROR_INVALIDCONFIGURATION	Invalid value in the configuration structure
	///
	/// @see
	/// <c>debuggerCreate</c>
	SCE_SLED_LINKAGE int32_t debuggerRequiredMemory(const SledDebuggerConfig *config, std::size_t *outRequiredMemory);

	/// Shut down a <c>SledDebugger</c> instance.
	/// @brief
	/// Shut down <c>SledDebugger</c> instance.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> instance to shut down
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger
	///
	/// @see
	/// <c>debuggerCreate</c>
	SCE_SLED_LINKAGE int32_t debuggerShutdown(SledDebugger *debugger);

	/// Initialize networking and optionally block execution until a connection is made.
	/// @brief
	/// Initialize networking and optionally block execution until connection is made.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger
	/// @retval SCE_SLED_ERROR_ALREADYNETWORKING	Already networking
	/// @retval SCE_SLED_ERROR_NOTINITIALIZED		Not initialized
	/// @retval SCE_SLED_ERROR_NETSUBSYSTEMFAIL		Network subsystem failed
	/// @retval SCE_SLED_ERROR_TCPSOCKETINITFAIL	Tcp socket initialization failed
	/// @retval SCE_SLED_ERROR_TCPNONBLOCKINGFAIL	Tcp socket set non-blocking mode failed
	/// @retval SCE_SLED_ERROR_TCPLISTENFAIL		Tcp socket failed to listen
	/// @retval SCE_SLED_ERROR_INVALIDPROTOCOL		Invalid network protocol
	///
	/// @see
	/// <c>debuggerStopNetworking</c>, <c>debuggerIsConnected</c>, <c>debuggerUpdate</c>, <c>debuggerIsNetworking</c>
	SCE_SLED_LINKAGE int32_t debuggerStartNetworking(SledDebugger *debugger);

	/// Stop networking (disconnect SLED if connected). <c>debuggerStopNetworking()</c> expects <c>debuggerStartNetworking()</c> 
	/// to have already been called.
	/// @brief
	/// Stop networking (disconnect SLED if connected).
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger
	/// @retval SCE_SLED_ERROR_NOTNETWORKING		Not networking
	///
	/// @see
	/// <c>debuggerStartNetworking</c>, <c>debuggerIsConnected</c>, <c>debuggerUpdate</c>, <c>debuggerIsNetworking</c>
	SCE_SLED_LINKAGE int32_t debuggerStopNetworking(SledDebugger *debugger);

	/// Poll sockets and process any incoming messages. <c>debuggerUpdate()</c> should be called from the main game loop every frame. 
	/// Return an error if <c>debuggerStartNetworking()</c> has not been called or if <c>debuggerStopNetworking()</c> has been called.
	/// @brief
	/// Poll sockets and process any incoming messages.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to update
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger
	/// @retval SCE_SLED_ERROR_NOTNETWORKING		Not networking
	/// @retval SCE_SLED_ERROR_RECURSIVEUPDATE		Attempt to call <c>debuggerUpdate()</c> recursively
	/// @retval SCE_SLED_ERROR_INVALIDPROTOCOL		Invalid network protocol
	/// @retval SCE_SLED_ERROR_TCPSOCKETINVALID		Tcp socket is invalid
	/// @retval SCE_SLED_ERROR_TCPSOCKETINITFAIL	Tcp socket initialization failed
	/// @retval SCE_SLED_ERROR_EVENTQUEUEESRCH		Event queue invalid ID
	/// @retval SCE_SLED_ERROR_EVENTQUEUECANCELED	Event queue was forcibly destroyed
	/// @retval SCE_SLED_ERROR_EVENTQUEUEINVAL		Event queue invalid value specified
	/// @retval SCE_SLED_ERROR_EVENTQUEUEABORT		Event queue will be destroyed because the process terminated
	/// @retval SCE_SLED_ERROR_NEGOTIATION			Negotiation with SLED failed
	///
	/// @see
	/// <c>debuggerStartNetworking</c>, <c>debuggerStopNetworking</c>, <c>debuggerIsConnected</c>, <c>debuggerIsNetworking</c>
	SCE_SLED_LINKAGE int32_t debuggerUpdate(SledDebugger *debugger);

	/// Get version information for <c>SledDebugger</c>.
	/// @brief
	/// Get <c>SledDebugger</c> version information.
	///
	/// @par Calling Conditions
	/// Multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	/// @param outVersion Version information for <c>SledDebugger</c>
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger or <c>outVersion</c>
	SCE_SLED_LINKAGE int32_t debuggerGetVersion(const SledDebugger *debugger, Version *outVersion);

	/// Determine whether or not a SLED client is connected.
	/// @brief
	/// Determine whether or not SLED client connected.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	/// @param outResult True if a client is connected, false if a client is not connected
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger or <c>outResult</c>
	///
	/// @see
	/// <c>debuggerStartNetworking</c>, <c>debuggerStopNetworking</c>, <c>debuggerIsNetworking</c>
	SCE_SLED_LINKAGE int32_t debuggerIsConnected(const SledDebugger *debugger, bool *outResult);

	/// Determine whether or not networking is enabled. LibSledDebugger can only accept connections between the time that 
	/// <c>debuggerStartNetworking()</c> has been called and the time that <c>debuggerStopNetworking()</c> has been called. 
	/// During that period of time, <c>debuggerIsNetworking()</c> returns true. Outside of that period of time, <c>debuggerIsNetworking()</c> returns false.
	/// @brief
	/// Determine whether or not networking is enabled.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	/// @param outResult True if <c>debuggerStartNetworking()</c> has been called but <c>debuggerStopNetworking()</c> has not been called yet, 
	/// false otherwise
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger or <c>outResult</c>
	///
	/// @see
	/// <c>debuggerStartNetworking</c>, <c>debuggerStopNetworking</c>, <c>debuggerIsConnected</c>
	SCE_SLED_LINKAGE int32_t debuggerIsNetworking(const SledDebugger *debugger, bool *outResult);

	/// Add a plugin to <c>SledDebugger</c>.
	/// @brief
	/// Add plugin to <c>SledDebugger</c>.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	/// @param plugin Language plugin to add
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger or plugin
	/// @retval SCE_SLED_ERROR_INVALIDPLUGIN		Invalid plugin
	/// @retval SCE_SLED_ERROR_MAXPLUGINSREACHED	Maximum number of plugins reached
	/// @retval SCE_SLED_ERROR_PLUGINALREADYADDED	Plugin already added
	///
	/// @see
	/// <c>debuggerRemovePlugin</c>, <c>debuggerScriptCacheAdd</c>
	SCE_SLED_LINKAGE int32_t debuggerAddPlugin(SledDebugger *debugger, SledDebuggerPlugin *plugin);

	/// Remove a plugin from <c>SledDebugger</c>.
	/// @brief
	/// Remove plugin from <c>SledDebugger</c>.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	/// @param plugin Language plugin to remove
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger or plugin
	/// @retval SCE_SLED_ERROR_INVALIDPLUGIN		Invalid plugin
	/// @retval SCE_SLED_ERROR_SRCH					Plugin not found or doesn't exist
	///
	/// @see
	/// <c>debuggerAddPlugin</c>, <c>debuggerScriptCacheRemove</c>
	SCE_SLED_LINKAGE int32_t debuggerRemovePlugin(SledDebugger *debugger, SledDebuggerPlugin *plugin);

	/// Add a script file to the internal list of scripts, so that when SLED connects it knows which scripts are being debugged. 
	/// Path should be relative to the asset directory. SLED will try to open the file from its asset directory.
	/// @brief
	/// Add a script file to internal list of scripts so that when SLED connects it knows which scripts are being debugged.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	/// @param relativePathToScriptFile Relative path (from the asset directory) of the file
	/// @param outResult True if the file is added to internal list, false if the file is not added to internal list
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger or <c>outResult</c>
	///
	/// @see
	/// <c>debuggerAddPlugin</c>, <c>debuggerScriptCacheRemove</c>, <c>debuggerScriptCacheClear</c>
	SCE_SLED_LINKAGE int32_t debuggerScriptCacheAdd(SledDebugger *debugger, const char *relativePathToScriptFile, bool *outResult);

	/// Remove a script file from the internal list of scripts, so that when SLED connects it knows which scripts are being debugged.
	/// @brief
	/// Remove script file from internal list of scripts so that when SLED connects it knows which scripts are being debugged.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	/// @param relativePathToScriptFile Relative path (from the asset directory) of the file
	/// @param outResult True if the file is removed from the internal list, false if the file is not removed from the internal list
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger or <c>outResult</c>
	///
	/// @see
	/// <c>debuggerScriptCacheAdd</c>, <c>debuggerScriptCacheClear</c>, <c>debuggerRemovePlugin</c>
	SCE_SLED_LINKAGE int32_t debuggerScriptCacheRemove(SledDebugger *debugger, const char *relativePathToScriptFile, bool *outResult);

	/// Clear the internal list of scripts being debugged.
	/// @brief
	/// Clear internal list of scripts being debugged.
	///
	/// @param debugger <c>SledDebugger</c> to use
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger
	///
	/// @see
	/// <c>debuggerScriptCacheAdd</c>, <c>debuggerScriptCacheRemove</c>
	SCE_SLED_LINKAGE int32_t debuggerScriptCacheClear(SledDebugger *debugger);

	/// Get the current debugger mode. <c>debuggerGetDebuggerMode()</c> is used primarily by language plugins.
	/// @brief
	/// Get current debugger mode.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	/// @param outDebuggerMode Current debugger mode
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger or <c>outDebuggerMode</c>
	SCE_SLED_LINKAGE int32_t debuggerGetDebuggerMode(const SledDebugger *debugger, DebuggerMode::Enum *outDebuggerMode);

	/// Send a message to SLED's TTY window.
	/// @brief
	/// Send message to SLED's TTY window.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	/// @param pszMessage Data to send
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger or <c>pszMessage</c>
	/// @retval SCE_SLED_ERROR_INVALIDPARAMETER		<c>pszMessage</c> is empty
	/// @retval SCE_SLED_ERROR_NOTNETWORKING		Not networking
	/// @retval SCE_SLED_ERROR_NOCLIENTCONNECTED	SLED is not connected
	SCE_SLED_LINKAGE int32_t debuggerTtyNotify(SledDebugger *debugger, const char *pszMessage);

	/// Plugins call the <c>debuggerBreakpointReached()</c> function when they encounter a breakpoint. <c>debuggerBreakpointReached()</c> notifies
	/// other plugins that a breakpoint has been reached, and then handles breakpoint synchronization and communication
	/// with SLED. <c>debuggerBreakpointReached()</c> can be commandeered to forcibly halt execution and break in SLED if needed.
	/// @brief
	/// Plugins call this function when they encounter a breakpoint.
	///
	/// @par Calling Conditions
	/// Not multithread safe.
	///
	/// @param debugger <c>SledDebugger</c> to use
	/// @param params Breakpoint parameters, including file, line number, and the plugin that hit the breakpoint
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		Null debugger or params
	/// @retval SCE_SLED_ERROR_NOTNETWORKING		Not networking
	/// @retval SCE_SLED_ERROR_NOCLIENTCONNECTED	SLED is not connected
	SCE_SLED_LINKAGE int32_t debuggerBreakpointReached(SledDebugger *debugger, const BreakpointParams *params);

	/// Generate a simple hash from a string and a line number.
	/// @brief
	/// Generate simple hash from string and line number.
	///
	/// @par Calling Conditions
	/// Multithread safe.
	///
	/// @param pszString String to hash
	/// @param line Line number that gets used in the hash
	/// @param outHash Hash if the function was successful
	///
	/// @retval SCE_SLED_ERROR_OK					Success
	/// @retval SCE_SLED_ERROR_NULLPARAMETER		<c>pszString</c> or <c>outHash</c> is null
	/// @retval SCE_SLED_ERROR_INVALIDPARAMETER		<c>pszString</c> is empty
	SCE_SLED_LINKAGE int32_t debuggerGenerateHash(const char *pszString, int32_t line, int32_t *outHash);
}}

#endif // __SCE_LIBSLEDDEBUGGER_H__
