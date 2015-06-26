/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_PLUGIN_H__
#define __SCE_LIBSLEDDEBUGGER_PLUGIN_H__

#include "params.h"
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
	/// Forward declaration.
	class SledDebugger;

	/// @brief
	/// Language plugin abstract base class.
	///
	/// Language plugin abstract base class. All language plugins must derive from this class.
	class SCE_SLED_LINKAGE SledDebuggerPlugin
	{
		friend class SledDebugger;
	public:	
		/// @brief
		/// Constructor.
		///
		/// <c>SledDebuggerPlugin</c> constructor.
		SledDebuggerPlugin() : m_pScriptMan(0), m_bInitialized(false) {}

		/// @brief
		/// Destructor.
		///	
		/// <c>SledDebuggerPlugin</c> destructor.
		virtual ~SledDebuggerPlugin() {}
	private:
		// Revoked
		SledDebuggerPlugin(const SledDebuggerPlugin&);
		SledDebuggerPlugin& operator=(const SledDebuggerPlugin&);
	private:
		virtual void shutdown() = 0;
	public:
		/// Get the ID of the plugin. The ID must be unique across all other language plugins. The ID of 0 (zero) is reserved for the <c>SledDebugger</c>.
		/// @brief
		/// Get ID of plugin.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return A number greater than zero
		///
		/// @see
		/// <c>getName</c>, <c>getVersion</c>
		virtual uint16_t getId() const = 0;

		/// Get the name of the plugin.
		/// @brief
		/// Get name of plugin.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Name of the plugin
		///
		/// @see
		/// <c>getId</c>, <c>getVersion</c>
		virtual const char *getName() const = 0;

		/// Get the version information of the plugin.
		/// @brief
		/// Get version information of plugin.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return Version information of plugin
		///
		/// @see
		/// <c>getId</c>, <c>getName</c>
		virtual const Version getVersion() const = 0;
	private:
		/// Function called by the <c>SledDebugger</c> when a SLED client connects. Should not be called manually.
		/// @brief
		/// Function called by the <c>SledDebugger</c> when SLED client connects.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @see
		/// <c>clientDisconnected</c>
		virtual void clientConnected() = 0;

		/// Function called by <c>SledDebugger</c> when a SLED client disconnects. Should not be called manually.
		/// @brief
		/// Function called by <c>SledDebugger</c> when a SLED client disconnects.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @see
		/// <c>clientConnected</c>
		virtual void clientDisconnected() = 0;

		/// Function called by <c>SledDebugger</c> when a message sent by SLED is dispatched to this plugin. Should not be called manually.
		/// @brief
		/// Function called by <c>SledDebugger</c> when message sent by SLED is dispatched to this plugin.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pData Network message data
		/// @param iSize Size of network message data
		virtual void clientMessage(const uint8_t *pData, int32_t iSize) = 0;

		/// Function called by <c>SledDebugger</c> when a breakpoint is being hit. Should not be called manually.
		/// The breakpoint might have occurred in another plugin and not this one, so be sure to keep track.
		/// @brief
		/// Function called by <c>SledDebugger</c> when breakpoint is being hit.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pParams Breakpoint parameters, including file, line number, and plugin that hit the breakpoint
		///
		/// @see
		/// <c>clientBreakpointEnd</c>
		virtual void clientBreakpointBegin(const BreakpointParams *pParams) = 0;

		/// Function called by <c>SledDebugger</c> when execution from a breakpoint is being resumed.
		/// Should not be called manually. The breakpoint might have occurred in another plugin and not this one, so be sure to keep track.
		/// @brief
		/// Function called by <c>SledDebugger</c> when execution from breakpoint is being resumed.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param pParams Breakpoint parameters, including file, line number, and plugin that hit the breakpoint
		///
		/// @see
		/// <c>clientBreakpointBegin</c>
		virtual void clientBreakpointEnd(const BreakpointParams *pParams) = 0;

		/// Function called by <c>SledDebugger</c> to let plugins know when the debug mode has changed (in case they need to respond to it).
		/// The previous mode is obtained through <c>SledDebugger::debuggerGetDebuggerMode()</c>. Should not be called manually.
		/// @brief
		/// Function called by <c>SledDebugger</c> to let plugins know when the debug mode has changed (in case they need to respond to it).
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param newMode The new debugger mode that is about to be set
		virtual void clientDebugModeChanged(DebuggerMode::Enum newMode) = 0;
	protected:
		SledDebugger *m_pScriptMan;		///< Pointer to <c>SledDebugger</c>
		bool m_bInitialized;			///< Flag to indicate whether or not the plugin has been initialized
	};
}}

#endif // __SCE_LIBSLEDDEBUGGER_PLUGIN_H__
