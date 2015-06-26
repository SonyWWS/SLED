/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_PARAMS_H__
#define __SCE_LIBSLEDDEBUGGER_PARAMS_H__

#if defined(_WIN32)
	#include "../sledcore/base_types.h"
#endif

#include "common.h"

/// Namespace for sce classes and functions.
namespace sce
{
/// Namespace for Sled classes and functions.
namespace Sled
{
	/// Namespace for scoping Protocol enumeration.
	///
	/// @brief
	/// Protocol enum namespace.
	namespace Protocol
	{
		/// Protocol enumeration.
		/// @brief
		/// Protocol enum.
		enum Enum
		{
			kTcp,	///< Tcp
		};
	}

	/// Namespace for scoping DebuggerMode enumeration.
	///
	/// @brief
	/// DebuggerMode enum namespace.
	namespace DebuggerMode
	{
		/// @brief
		/// Debugger mode enumeration.
		///
		/// Debugger mode enum.
		enum Enum
		{
			kNormal,		///< Normal
			kStepInto,		///< Step into
			kStepOver,		///< Step over
			kStepOut,		///< Step out
			kStop			///< Stop
		};
	}

	/// @brief
	/// Struct that describes details of network configuration structure.
	///
	/// Structure that describes details of the network configuration structure. The NetworkParams structure defines which network protocol to use:
	/// TCP, which port to use (if the protocol is TCP), and whether or not to wait for SLED to connect
	/// before continuing execution from <c>debuggerStartNetworking()</c>.	
	struct SCE_SLED_LINKAGE NetworkParams
	{
		Protocol::Enum		protocol;					///< Network protocol to use: TCP
		uint16_t			port;						///< Port to use. Relevant only if the protocol is TCP.
		bool				blockUntilConnect;			///< Whether or not to block program execution until SLED connects

		/// <c>NetworkParams</c> constructor.
		/// @brief
		/// Constructor.
		NetworkParams();

		/// NetworkParams setup function.
		/// @brief
		/// Setup function.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @param kProtocol Network protocol to use
		/// @param iPort Network port to use
		/// @param bBlockUntilConnect Whether or not to block program execution until SLED connects
		void setup(Protocol::Enum kProtocol, uint16_t iPort, bool bBlockUntilConnect);

		/// <c>NetworkParams</c> copy constructor.
		/// @brief
		/// Copy constructor.
		///
		/// @param rhs Item to copy from
		NetworkParams(const NetworkParams& rhs) { init(rhs); }

		/// <c>NetworkParams</c> assignment operator.
		/// @brief
		/// Assignment operator.
		///
		/// @param rhs Item to copy from
		///
		/// @return Assigned value
		NetworkParams& operator=(const NetworkParams& rhs) { init(rhs); return *this; }

	private:
		void init(const NetworkParams& rhs);
	};
	
	/// @brief
	/// Breakpoint params struct.
	///
	/// Breakpoint parameters struct.
	struct SCE_SLED_LINKAGE BreakpointParams
	{
		static const uint16_t kRelFilePathLen = 256;	///< Maximum length for RelFilePath

		uint16_t	pluginId;						///< Plugin that hit the breakpoint
		uint32_t	lineNumber;						///< Line number of the hit breakpoint
		char		relFilePath[kRelFilePathLen];	///< Relative path (from the asset directory) of the file that contains the breakpoint that was hit

		/// <c>BreakpointParams</c> constructor.
		/// @brief
		/// Constructor.
		BreakpointParams();

		/// <c>BreakpointParams</c> constructor with parameters.
		/// @brief
		/// Constructor with parameters.
		///
		/// @param iPluginId ID of the plugin that hit the breakpoint
		/// @param iLineNumber Line number of the hit breakpoint
		/// @param pszRelFilePath Relative path (from the asset directory) of the file that contains the breakpoint that was hit
		BreakpointParams(uint16_t iPluginId, uint32_t iLineNumber, const char *pszRelFilePath);

		/// <c>BreakpointParams</c> copy constructor.
		/// @brief
		/// Copy constructor.
		///
		/// @param rhs Item to copy from
		BreakpointParams(const BreakpointParams& rhs) { init(rhs); }

		/// BreakpointParams assignment operator.
		/// @brief
		/// Assignment operator.
		///
		/// @param rhs Item to copy from
		///
		/// @return Assigned value
		BreakpointParams& operator=(const BreakpointParams& rhs) { init(rhs); return *this; }

	private:
		void init(const BreakpointParams& rhs);
	};
	
	/// @brief
	/// Version detail.
	///
	/// Version detail information.
	struct SCE_SLED_LINKAGE Version
	{
		uint16_t majorNum;		///< Major version number
		uint16_t minorNum;		///< Minor version number
		uint16_t revisionNum;	///< Revision version number

		/// <c>Version</c> constructor.
		/// @brief
		/// Constructor.
		Version() : majorNum(0), minorNum(0), revisionNum(0) {}

		/// <c>Version</c> constructor with parameters.
		/// @brief
		/// Constructor with parameters.
		///
		/// @param iMajor Major version number
		/// @param iMinor Minor version number
		/// @param iRevision Revision version number
		Version(uint16_t iMajor, uint16_t iMinor, uint16_t iRevision) : majorNum(iMajor), minorNum(iMinor), revisionNum(iRevision) {}

		/// <c>Version</c> copy constructor.
		/// @brief
		/// Copy constructor.
		///
		/// @param rhs Item to copy from
		Version(const Version& rhs) { majorNum = rhs.majorNum; minorNum = rhs.minorNum; revisionNum = rhs.revisionNum; }

		/// <c>Version</c> assignment operator.
		/// @brief
		/// Assignment operator.
		///
		/// @param rhs Item to copy from
		///
		/// @return Assigned value
		Version& operator=(const Version& rhs) { majorNum = rhs.majorNum; minorNum = rhs.minorNum; revisionNum = rhs.revisionNum; return *this; }
	};
	
	/// @brief
	/// Structure describing details of SledDebugger instance.
	///
	/// The SledDebuggerConfig structure describes the details of a SledDebugger instance.
	struct SCE_SLED_LINKAGE SledDebuggerConfig
	{
		uint16_t		maxPlugins;				///< Maximum number of plugins that the SledDebugger will manage
		uint16_t		maxScriptCacheEntries;	///< Maximum number of files that the script cache will hold
		uint16_t		maxScriptCacheEntryLen;	///< Maximum string length of a script cache file entry
		uint32_t		maxRecvBufferSize;		///< Maximum size of the receive buffer (1024 recommended at a minimum)
		uint32_t		maxSendBufferSize;		///< Maximum size of the send buffer (1024 recommended at a minimum)	
		NetworkParams	net;					///< Network settings

		/// <c>SledDebuggerConfig</c> constructor.
		/// @brief
		/// Constructor.
		SledDebuggerConfig()
			: maxPlugins(1)
			, maxScriptCacheEntries(0)
			, maxScriptCacheEntryLen(0)
			, maxRecvBufferSize(2048)
			, maxSendBufferSize(2048)
			, net()
		{}

		/// <c>SledDebuggerConfig</c> copy constructor.
		/// @brief
		/// Copy constructor.
		///
		/// @param rhs Item to copy from
		SledDebuggerConfig(const SledDebuggerConfig& rhs) { init(rhs); }

		/// <c>SledDebuggerConfig</c> assignment operator.
		/// @brief
		/// Assignment operator.
		///
		/// @param rhs Item to copy from
		///
		/// @return Assigned value
		SledDebuggerConfig& operator=(const SledDebuggerConfig& rhs) { init(rhs); return *this; }

	private:
		void init(const SledDebuggerConfig& rhs);
	};
}}

#endif // __SCE_LIBSLEDDEBUGGER_PARAMS_H__
