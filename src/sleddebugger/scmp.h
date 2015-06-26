/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_SCMP_H__
#define __SCE_LIBSLEDDEBUGGER_SCMP_H__

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
// Forward declarations.
class NetworkBuffer;
class NetworkBufferPacker;
class NetworkBufferReader;

/// Namespace for SLED Control Message Protocol classes and functions.
///
/// @brief
/// SLED Control Message Protocol namespace.
namespace SCMP
{
	/// Namespace for scoping TypeCodes enumeration.
	///
	/// @brief
	/// Scoping TypeCodes enumeration namespace.
	namespace TypeCodes
	{
		/// Type codes for network messages.
		/// @brief
		/// Network messages type codes.
		enum Enum
		{
			kBase = 0,					///< Base message

			kBreakpointDetails = 1,		///< Breakpoint details message
			kBreakpointBegin = 2,		///< Breakpoint begin message
			kBreakpointSync = 3,		///< Breakpoint sync message
			kBreakpointEnd = 4,			///< Breakpoint end message
			kBreakpointContinue = 5,	///< Breakpoint continue message

			kDisconnect = 6,			///< Disconnect message

			kHeartbeat = 8,				///< Heartbeat message
			kSuccess = 9,				///< Success message
			kFailure = 10,				///< Failure message
			kVersion = 11,				///< Version message

			kDebugStart = 12,			///< Debug start message
			kDebugStepInto = 13,		///< Debug step into message
			kDebugStepOver = 14,		///< Debug step over message
			kDebugStepOut = 15,			///< Debug step out message
			kDebugStop = 16,			///< Debug stop message

			kScriptCache = 17,			///< Script cache message

			kAuthenticated = 18,		///< Authenticated message

			kReady = 20,				///< Ready message
			kPluginsReady = 21,			///< Plugins ready message

			kFunctionInfo = 22,			///< Function information message

			kTTYBegin = 23,				///< TTY Begin message
			kTTY = 24,					///< TTY message
			kTTYEnd = 25,				///< TTY End message

			kDevCmd = 26,				///< Developer entered command message
			kEditAndContinue = 27,		///< Edit & Continue message

			kEndianness = 28,			///< Endianness message
			kProtocolDebugMark = 29,	///< Protocol Debug Mark message
		};
	}

	/// SLED Control Message Protocol base network message structure. All network messages must derive from <c>Base</c>.
	/// @brief
	/// SLED Control Message Protocol base network message structure.
	struct SCE_SLED_LINKAGE Base
	{
		int32_t		length;			///< Length of the message in bytes
		uint16_t	typeCode;		///< Property that identifies what type of message this is
		uint16_t	pluginId;		///< Plugin that this message should be sent to

		static const int kStringLen = 256;		///< Default string length used in SCMP messages that contain strings
		static const int kSizeOfBase = 8;		///< Size of the SCMP::Base structure in bytes (8)
		static const int kSizeOfuint8_t = 1;	///< Size of a uint8_t in bytes (1)
		static const int kSizeOfuint16_t = 2;	///< Size of a uint16_t in bytes (2)
		static const int kSizeOfuint32_t = 4;	///< Size of a uint32_t in bytes (4)
		static const int kSizeOfuint64_t = 8;	///< Size of a uint64_t in bytes (8)
		static const int kSizeOfint16_t = 2;	///< Size of a int16_t in bytes (2)
		static const int kSizeOfint32_t = 4;	///< Size of a int32_t in bytes (4)
		static const int kSizeOfint64_t = 8;	///< Size of a int64_t in bytes (8)
		static const int kSizeOffloat = 4;		///< Size of a float in bytes (4)
		static const int kSizeOfdouble = 8;		///< Size of a double in bytes (8)

		/// Convenience method to see if message represents a breakpoint command.
		/// @brief
		/// Convenience method to see if message represents breakpoint command.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return True if breakpoint command; false if not
		///
		/// @see
		/// <c>isDebug</c>, <c>isReady</c>, 
		inline bool isBreakpoint() const { return ((typeCode >= TypeCodes::kBreakpointBegin) && (typeCode <= TypeCodes::kBreakpointEnd)); }

		/// Convenience method to see if message represents a debug command.
		/// @brief
		/// Convenience method to see if message represents debug command.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return True if debug command; false if not
		///
		/// @see
		/// <c>isBreakpoint</c>, <c>isReady</c>, 
		inline bool isDebug() const { return ((typeCode >= TypeCodes::kDebugStart) && (typeCode <= TypeCodes::kDebugStop)); }

		/// Convenience method to see if message represents a ready command.
		/// @brief
		/// Convenience method to see if message represents ready command.
		///
		/// @par Calling Conditions
		/// Not multithread safe.
		///
		/// @return True if ready command; false if not
		///
		/// @see
		/// <c>isBreakpoint</c>, <c>isDebug</c>
		inline bool isReady() const { return typeCode == TypeCodes::kReady; }
	};

#ifndef DOXYGEN_IGNORE
	/// Heartbeat network message structure.
	/// @brief
	/// Heartbeat network message struct.
	struct SCE_SLED_LINKAGE Heartbeat : public Base
	{
		/// <c>Heartbeat</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		Heartbeat(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kHeartbeat;
			pluginId = iPluginId;
		}
	};

	/// Endianness network message structure.
	/// @brief
	/// Endianness network message struct.
	struct SCE_SLED_LINKAGE Endianness : public Base
	{
		/// <c>Endianness</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		Endianness(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kEndianness;
			pluginId = iPluginId;
		}
	};

	/// Protocol debug mark network message structure.
	/// @brief
	/// Protocol debug mark network message struct.
	struct SCE_SLED_LINKAGE ProtocolDebugMark : public Base
	{
		/// <c>ProtocolDebugMark</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		ProtocolDebugMark(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kProtocolDebugMark;
			pluginId = iPluginId;
		}
	};

	/// Version network message structure.
	/// @brief
	/// Version network message struct.
	///
	/// @see
	/// <c>Details</c>
	struct SCE_SLED_LINKAGE Version : public Base
	{
		/// <c>Version</c> constructor with parameters.
		/// @brief
		/// Constructor with parameters.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		/// @param iMajor Major version
		/// @param iMinor Minor version
		/// @param iRevision Revision version number
		/// @param pBuffer Message buffer
		Version(uint16_t iPluginId, uint16_t iMajor, uint16_t iMinor, uint16_t iRevision, NetworkBuffer *pBuffer = 0)
			: majorNum(iMajor), minorNum(iMinor), revisionNum(iRevision)
		{
			length = kSizeOfBase + (kSizeOfuint16_t * 3);
			typeCode = TypeCodes::kVersion;
			pluginId = iPluginId;

			if (pBuffer)
				pack(pBuffer);
		}

		/// Pack information into <c>Version</c> instance.
		/// @brief
		/// Pack information.
		///
		/// @param pBuffer Message buffer
		void pack(NetworkBuffer *pBuffer);

		uint16_t		majorNum;		///< Major version
		uint16_t		minorNum;		///< Minor version
		uint16_t		revisionNum;	///< Revision version number
	};

	/// Debug start network message structure.
	/// @brief
	/// DebugStart network message struct.
	///
	/// @see
	/// <c>Details</c>, <c>DebugStepInto</c>, <c>DebugStepOver</c>, <c>DebugStepOut</c>, <c>DebugStop</c>
	struct SCE_SLED_LINKAGE DebugStart : public Base
	{
		/// <c>DebugStart</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		DebugStart(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kDebugStart;
			pluginId = iPluginId;
		}
	};

	/// Debug step into network message structure.
	/// @brief
	/// DebugStepInto network message struct.
	///
	/// @see
	/// <c>Details</c>, <c>DebugStart</c>, <c>DebugStepOver</c>, <c>DebugStepOut</c>, <c>DebugStop</c>
	struct SCE_SLED_LINKAGE DebugStepInto : public Base
	{
		/// <c>DebugStepInto</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		DebugStepInto(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kDebugStepInto;
			pluginId = iPluginId;
		}
	};

	/// Debug step over network message structure.
	/// @brief
	/// DebugStepOver network message struct.
	///
	/// @see
	/// <c>Details</c>, <c>DebugStart</c>, <c>DebugStepInto</c>, <c>DebugStepOut</c>, <c>DebugStop</c>
	struct SCE_SLED_LINKAGE DebugStepOver : public Base
	{
		/// <c>DebugStepOver</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		DebugStepOver(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kDebugStepOver;
			pluginId = iPluginId;
		}
	};

	/// Debug step out network message structure.
	/// @brief
	/// DebugStepOut network message struct.
	///
	/// @see
	/// <c>Details</c>, <c>DebugStart</c>, <c>DebugStepInto</c>, <c>DebugStepOver</c>, <c>DebugStop</c>
	struct SCE_SLED_LINKAGE DebugStepOut : public Base
	{
		/// <c>DebugStepOut</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		DebugStepOut(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kDebugStepOut;
			pluginId = iPluginId;
		}
	};

	/// Debug stop network message structure.
	/// @brief
	/// DebugStop network message struct.
	///
	/// @see
	/// <c>Details</c>, <c>DebugStart</c>, <c>DebugStepInto</c>, <c>DebugStepOver</c>, <c>DebugStepOut</c>
	struct SCE_SLED_LINKAGE DebugStop : public Base
	{
		/// <c>DebugStop</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		DebugStop(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kDebugStop;
			pluginId = iPluginId;
		}
	};

	/// Namespace for Breakpoint.
	///
	/// @brief
	/// Breakpoint namespace.
	namespace Breakpoint
	{
		/// Details network message structure.
		/// @brief
		/// Details network message struct.
		///
		/// @see
		/// <c>Begin</c>, <c>Sync</c>, <c>End</c>, <c>Continue</c>, <c>Disconnect</c>, <c>Version</c>
		struct SCE_SLED_LINKAGE Details : public Base
		{
			/// <c>Details</c> constructor with parameters.
			/// @brief
			/// Constructor with parameters.
			///
			/// @param iPluginId LibSledDebugger plugin ID
			/// @param pszRelFilePath Relative path (from the asset directory) of the file that contains the breakpoint that was hit
			/// @param iLine Line number of the hit breakpoint
			/// @param pszCondition Breakpoint condition
			/// @param bResult Whether breakpoint is hit
			/// @param bUseFunctionEnvironment Whether to use function's environment or default environment ("_G") to test breakpoint condition
			Details(uint16_t iPluginId, const char *pszRelFilePath, int32_t iLine, const char *pszCondition, bool bResult, bool bUseFunctionEnvironment);
			/// <c>Details</c> constructor with <c>NetworkBufferReader</c>.
			/// @brief
			/// Constructor with <c>NetworkBufferReader</c>.
			///
			/// @param reader <c>NetworkBufferReader</c> to read breakpoint details information
			Details(NetworkBufferReader *reader) { unpack(reader); }
			/// Unpack breakpoint details information from <c>NetworkBufferReader</c>.
			/// @brief
			/// Unpack breakpoint details information.
			///
			/// @param reader <c>NetworkBufferReader</c> to read breakpoint details information
			void unpack(NetworkBufferReader *reader);

			char		relFilePath[kStringLen];	///< Relative path (from the asset directory) of file that contains the breakpoint that was hit
			int32_t		line;						///< Breakpoint line number
			char		condition[kStringLen];		///< Breakpoint condition
			uint8_t		result;						///< Whether breakpoint is hit
			uint8_t		useFunctionEnvironment;		///< Whether to use function environment to test breakpoint condition
		};

		/// Begin network message structure.
		/// @brief
		/// Begin network message struct.
		///
		/// @see
		/// <c>Details</c>, <c>Sync</c>, <c>End</c>, <c>Continue</c>, <c>Disconnect</c>
		struct SCE_SLED_LINKAGE Begin : public Base
		{
			/// <c>Begin</c> constructor with parameters.
			/// @brief
			/// Constructor with parameters.
			///
			/// @param iPluginId LibSledDebugger plugin ID
			/// @param iBreakPluginId ID of plugin triggering breakpoint
			/// @param pszRelFilePath Relative path (from the asset directory) of the file that contains the breakpoint that was hit
			/// @param iLine Line number of the hit breakpoint
			/// @param pBuffer <c>NetworkBuffer</c> message buffer
			Begin(uint16_t iPluginId, uint16_t iBreakPluginId, const char *pszRelFilePath, int32_t iLine, NetworkBuffer *pBuffer = 0);
			/// <c>Begin</c> constructor with <c>NetworkBufferReader</c>.
			/// @brief
			/// Constructor with <c>NetworkBufferReader</c>.
			///
			/// @param reader <c>NetworkBufferReader</c> to read breakpoint information
			Begin(NetworkBufferReader *reader) { unpack(reader); }
			/// Unpack breakpoint information from <c>NetworkBufferReader</c>.
			/// @brief
			/// Unpack breakpoint information.
			///
			/// @param reader <c>NetworkBufferReader</c> to read breakpoint information
			void unpack(NetworkBufferReader *reader);
			/// Pack breakpoint information in <c>NetworkBuffer</c>.
			/// @brief
			/// Pack breakpoint information.
			///
			/// @param pBuffer <c>NetworkBuffer</c> in which to pack breakpoint information
			void pack(NetworkBuffer *pBuffer);

			uint16_t	breakPluginId;				///< ID of plugin triggering breakpoint
			char		relFilePath[kStringLen];	///< Relative path (from the asset directory) of the file that contains the breakpoint that was hit
			int32_t		line;						///< Line number of the hit breakpoint
		};

		/// Sync network message structure.
		/// @brief
		/// Sync network message struct.
		///
		/// @see
		/// <c>Begin</c>, <c>End</c>, <c>Continue</c>, <c>Disconnect</c>
		struct SCE_SLED_LINKAGE Sync : public Base
		{
			/// <c>Sync</c> constructor with parameters.
			/// @brief
			/// Constructor with parameters.
			///
			/// @param iPluginId LibSledDebugger plugin ID
			/// @param iBreakPluginId ID of plugin triggering breakpoint
			/// @param pszRelFilePath Relative path (from the asset directory) of the file that contains the breakpoint that was hit
			/// @param iLine Line number of the hit breakpoint
			/// @param pBuffer <c>NetworkBuffer</c> message buffer
			Sync(uint16_t iPluginId, uint16_t iBreakPluginId, const char *pszRelFilePath, int32_t iLine, NetworkBuffer *pBuffer = 0);
			/// <c>Sync</c> constructor with <c>NetworkBufferReader</c>.
			/// @brief
			/// Constructor with <c>NetworkBufferReader</c>.
			///
			/// @param reader <c>NetworkBufferReader</c> to read breakpoint information
			Sync(NetworkBufferReader *reader) { unpack(reader); }
			/// Unpack breakpoint information from <c>NetworkBufferReader</c>.
			/// @brief
			/// Constructor.
			///
			/// @param reader <c>NetworkBufferReader</c> to read breakpoint information
			void unpack(NetworkBufferReader *reader);
			/// Pack breakpoint information in <c>NetworkBuffer</c>.
			/// @brief
			/// Pack breakpoint information.
			///
			/// @param pBuffer <c>NetworkBuffer</c> in which to pack breakpoint information
			void pack(NetworkBuffer *pBuffer);

			uint16_t	breakPluginId;				///< ID of plugin triggering breakpoint
			char		relFilePath[kStringLen];	///< Relative path (from the asset directory) of the file that contains the breakpoint that was hit
			int32_t		line;						///< Line number of the hit breakpoint
		};

		/// End network message structure.
		/// @brief
		/// End network message struct.
		///
		/// @see
		/// <c>Begin</c>, <c>Sync</c>, <c>Continue</c>, <c>Disconnect</c>
		struct SCE_SLED_LINKAGE End : public Base
		{
			/// <c>End</c> constructor with parameters.
			/// @brief
			/// Constructor with parameters.
			///
			/// @param iPluginId LibSledDebugger plugin ID
			/// @param iBreakPluginId ID of plugin triggering breakpoint
			/// @param pszRelFilePath Relative path (from the asset directory) of the file that contains the breakpoint that was hit
			/// @param iLine Line number of the hit breakpoint
			/// @param pBuffer <c>NetworkBuffer</c> message buffer
			End(uint16_t iPluginId, uint16_t iBreakPluginId, const char *pszRelFilePath, int32_t iLine, NetworkBuffer *pBuffer = 0);
			/// <c>End</c> constructor with <c>NetworkBufferReader</c>.
			/// @brief
			/// Constructor with <c>NetworkBufferReader</c>.
			///
			/// @param reader <c>NetworkBufferReader</c> to read breakpoint information
			End(NetworkBufferReader *reader) { unpack(reader); }
			/// Unpack breakpoint information from <c>NetworkBufferReader</c>.
			/// @brief
			/// Constructor.
			///
			/// @param reader <c>NetworkBufferReader</c> to read breakpoint information
			void unpack(NetworkBufferReader *reader);
			/// Pack breakpoint information in <c>NetworkBuffer</c>.
			/// @brief
			/// Pack breakpoint information.
			///
			/// @param pBuffer <c>NetworkBuffer</c> in which to pack breakpoint information
			void pack(NetworkBuffer *pBuffer);

			uint16_t	breakPluginId;				///< ID of plugin triggering breakpoint
			char		relFilePath[kStringLen];	///< Relative path (from the asset directory) of the file that contains the breakpoint that was hit
			int32_t		line;						///< Line number of the hit breakpoint
		};

		/// Continue network message structure.
		/// @brief
		/// Continue network message struct.
		///
		/// @see
		/// <c>Begin</c>, <c>Sync</c>, <c>End</c>, <c>Disconnect</c>
		struct SCE_SLED_LINKAGE Continue : public Base
		{
			/// <c>Continue</c> constructor with parameters.
			/// @brief
			/// Constructor with parameters.
			///
			/// @param iPluginId LibSledDebugger plugin ID
			/// @param iBreakPluginId ID of plugin triggering breakpoint
			/// @param pszRelFilePath Relative path (from the asset directory) of the file that contains the breakpoint that was hit
			/// @param iLine Line number of the hit breakpoint
			/// @param pBuffer <c>NetworkBuffer</c> message buffer
			Continue(uint16_t iPluginId, uint16_t iBreakPluginId, const char *pszRelFilePath, int32_t iLine, NetworkBuffer *pBuffer = 0);
			/// <c>Continue</c> constructor with <c>NetworkBufferReader</c>.
			/// @brief
			/// Constructor with <c>NetworkBufferReader</c>.
			///
			/// @param reader <c>NetworkBufferReader</c> to read breakpoint information
			Continue(NetworkBufferReader *reader) { unpack(reader); }
			/// Unpack breakpoint information from <c>NetworkBufferReader</c>.
			/// @brief
			/// Constructor.
			///
			/// @param reader <c>NetworkBufferReader</c> to read breakpoint information
			void unpack(NetworkBufferReader *reader);
			/// Pack breakpoint information in <c>NetworkBuffer</c>.
			/// @brief
			/// Pack breakpoint information.
			///
			/// @param pBuffer <c>NetworkBuffer</c> in which to pack breakpoint information
			void pack(NetworkBuffer *pBuffer);

			uint16_t	breakPluginId;				///< ID of plugin triggering breakpoint
			char		relFilePath[kStringLen];	///< Relative path (from the asset directory) of the file that contains the breakpoint that was hit
			int32_t		line;						///< Line number of the hit breakpoint
		};
	}

	/// Disconnect network message structure.
	/// @brief
	/// Disconnect network message struct.
	///
	/// @see
	/// <c>Begin</c>, <c>Sync</c>, <c>End</c>, <c>Continue</c>
	struct SCE_SLED_LINKAGE Disconnect : public Base
	{
		/// <c>Disconnect</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		Disconnect(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kDisconnect;
			pluginId = iPluginId;
		}
	};

	/// Success network message structure.
	/// @brief
	/// Success network message struct.
	///
	/// @see
	/// <c>Failure</c>
	struct SCE_SLED_LINKAGE Success : public Base
	{
		/// <c>Success</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		Success(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kSuccess;
			pluginId = iPluginId;
		}
	};

	/// Failure network message structure.
	/// @brief
	/// Failure network message struct.
	///
	/// @see
	/// <c>Success</c>
	struct SCE_SLED_LINKAGE Failure : public Base
	{
		/// <c>Failure</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		Failure(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kFailure;
			pluginId = iPluginId;
		}
	};

	/// Authenticated network message structure.
	/// @brief
	/// Authenticated network message struct.
	struct SCE_SLED_LINKAGE Authenticated : public Base
	{
		/// <c>Authenticated</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		Authenticated(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kAuthenticated;
			pluginId = iPluginId;
		}
	};

	/// Script cache network message structure.
	/// @brief
	/// ScriptCache network message struct.
	struct SCE_SLED_LINKAGE ScriptCache : public Base
	{
		/// <c>ScriptCache</c> constructor with parameters.
		/// @brief
		/// Constructor with parameters.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		/// @param pszRelScriptPath Relative path (from the asset directory) of the file that contains the script
		/// @param pBuffer <c>NetworkBuffer</c> message buffer
		ScriptCache(uint16_t iPluginId, const char *pszRelScriptPath, NetworkBuffer *pBuffer = 0);
		/// Pack script information in <c>NetworkBuffer</c>.
		/// @brief
		/// Pack script information.
		///
		/// @param pBuffer <c>NetworkBuffer</c> in which to pack script information
		void pack(NetworkBuffer *pBuffer);

		char	relScriptPath[kStringLen];	///< Relative path (from the asset directory) of the file that contains the script
	};

	/// Ready network message structure.
	/// @brief
	/// Ready network message struct.
	///
	/// @see
	/// <c>PluginsReady</c>
	struct SCE_SLED_LINKAGE Ready : public Base
	{
		/// <c>Ready</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		Ready(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kReady;
			pluginId = iPluginId;
		}
	};

	/// Plugins ready network message structure.
	/// @brief
	/// PluginsReady network message struct.
	///
	/// @see
	/// <c>Ready</c>
	struct SCE_SLED_LINKAGE PluginsReady : public Base
	{
		/// <c>PluginsReady</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		PluginsReady(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kPluginsReady;
			pluginId = iPluginId;
		}
	};

	/// Function info network message structure.
	/// @brief
	/// FunctionInfo network message struct.
	struct SCE_SLED_LINKAGE FunctionInfo : public Base
	{
		/// <c>FunctionInfo</c> constructor with parameters.
		/// @brief
		/// Constructor with parameters.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		/// @param pszScriptFile Script file name function is in
		/// @param pszFuncName Function name
		/// @param iLineDefined Line number function defined on
		FunctionInfo(uint16_t iPluginId, const char *pszScriptFile, const char *pszFuncName, int32_t iLineDefined);
		/// <c>FunctionInfo</c> constructor with <c>NetworkBufferReader</c>.
		/// @brief
		/// Constructor with <c>NetworkBufferReader</c>.
		///
		/// @param reader <c>NetworkBufferReader</c> to read function information
		FunctionInfo(NetworkBufferReader *reader) { unpack(reader); }
		/// Unpack function information from <c>NetworkBufferReader</c>.
		/// @brief
		/// Constructor.
		///
		/// @param reader <c>NetworkBufferReader</c> to read function information
		void unpack(NetworkBufferReader *reader);

		char		relScriptPath[kStringLen];	///< Script file name function is in
		char		functionName[kStringLen];	///< Function name
		int32_t		lineDefined;				///< Line number function defined on
	};

	/// TTY begin network message structure.
	/// @brief
	/// TTYBegin network message struct.
	///
	/// @see
	/// <c>TTY</c>, <c>TTYEnd</c>
	struct SCE_SLED_LINKAGE TTYBegin : public Base
	{
		/// <c>TTYBegin</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		TTYBegin(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kTTYBegin;
			pluginId = iPluginId;
		}
	};

	/// TTY network message structure.
	/// @brief
	/// TTY network message struct.
	///
	/// @see
	/// <c>TTYBegin</c>, <c>TTYEnd</c>
	struct SCE_SLED_LINKAGE TTY : public Base
	{
		/// <c>TTY</c> constructor with parameters.
		/// @brief
		/// Constructor with parameters.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		/// @param pszMessage Message text
		/// @param pBuffer <c>NetworkBuffer</c> message buffer
		TTY(uint16_t iPluginId, const char *pszMessage, NetworkBuffer *pBuffer = 0);
		/// Pack message information in <c>NetworkBuffer</c>.
		/// @brief
		/// Pack message information.
		///
		/// @param pBuffer <c>NetworkBuffer</c> in which to pack message information
		void pack(NetworkBuffer *pBuffer);

		char	message[kStringLen];	///< Message text
	};

	/// TTY end network message structure.
	/// @brief
	/// TTYEnd network message struct.
	///
	/// @see
	/// <c>TTYBegin</c>, <c>TTY</c>
	struct SCE_SLED_LINKAGE TTYEnd : public Base
	{
		/// <c>TTYEnd</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		TTYEnd(uint16_t iPluginId)
		{
			length = kSizeOfBase;
			typeCode = TypeCodes::kTTYEnd;
			pluginId = iPluginId;
		}
	};

	/// Developer command network message structure.
	/// @brief
	/// DevCmd network message struct.
	struct SCE_SLED_LINKAGE DevCmd : public Base
	{
		/// <c>DevCmd</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		/// @param pszCommand Developer command text
		DevCmd(uint16_t iPluginId, const char *pszCommand);
		/// <c>DevCmd</c> constructor with <c>NetworkBufferReader</c>.
		/// @brief
		/// Constructor with <c>NetworkBufferReader</c>.
		///
		/// @param reader <c>NetworkBufferReader</c> to read developer command information
		DevCmd(NetworkBufferReader *reader) { unpack(reader); }
		/// Unpack command information from <c>NetworkBufferReader</c>.
		/// @brief
		/// Constructor.
		///
		/// @param reader <c>NetworkBufferReader</c> to read developer command information
		void unpack(NetworkBufferReader *reader);

		char	command[kStringLen];	///< Developer command text
	};

	/// Edit and continue network message structure.
	/// @brief
	/// EditAndContinue network message struct.
	struct SCE_SLED_LINKAGE EditAndContinue : public Base
	{
		/// <c>EditAndContinue</c> constructor.
		/// @brief
		/// Constructor.
		///
		/// @param iPluginId LibSledDebugger plugin ID
		/// @param pszRelScriptPath Relative path (from the asset directory) of the file that contains the script
		EditAndContinue(uint16_t iPluginId, const char *pszRelScriptPath);
		/// <c>EditAndContinue</c> constructor with <c>NetworkBufferReader</c>.
		/// @brief
		/// Constructor with <c>NetworkBufferReader</c>.
		///
		/// @param reader <c>NetworkBufferReader</c> to read script information
		EditAndContinue(NetworkBufferReader *reader) { unpack(reader); }
		/// Unpack script information from <c>NetworkBufferReader</c>.
		/// @brief
		/// Constructor.
		///
		/// @param reader <c>NetworkBufferReader</c> to read script information
		void unpack(NetworkBufferReader *reader);

		char	relScriptPath[kStringLen];	///< Relative path (from the asset directory) of the file that contains the script
	};
#endif //DOXYGEN_IGNORE
}}}

#endif // __SCE_LIBSLEDDEBUGGER_SCMP_H__
