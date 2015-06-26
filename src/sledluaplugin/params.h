/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDLUAPLUGIN_PARAMS_H__
#define __SCE_LIBSLEDLUAPLUGIN_PARAMS_H__

#if defined(_WIN32)
	#include "../sledcore/base_types.h"
#endif

#include "../sleddebugger/common.h"

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
	/// Typedef for a chop characters callback function.
	///
	/// @brief
	/// Typedef for chop characters callback function.
	///
	/// @param pszFilePath Path to file
	/// @return None
	typedef const char *(*ChopCharsCallback)(const char *pszFilePath);

	/// String describing the Lua user data that is sent back to SLED. This string shows up in the "Value" column 
	/// of the respective [Globals/Locals/Upvalues/etc.] window.
	/// @brief
	/// Typedef for user data callback function.
	///
	/// @param pLuaUserData Lua userdata
	/// @param pUserData Optional user-controlled userdata
	/// @return None
	typedef const char *(*UserDataCallback)(void *pLuaUserData, void *pUserData);

	/// Typedef for a user data callback function. Call this function after regular user data callback processing has finished 
	/// so that memory can be freed, if necessary.
	/// @brief
	/// Typedef for user data callback function.
	///
	/// @param pLuaUserData Lua user data
	/// @param pUserData Optional user-controlled userdata
	typedef void (*UserDataFinishCallback)(void *pLuaUserData, void *pUserData);

	/// Typedef for an edit and continue callback function. This function alerts the user that they need to load a specific file synchronously, 
	/// because LibSledLuaPlugin needs to operate on the contents. <c>EditAndContinueCallback()</c> returns a <c>const char*</c> to the contents of that file 
	/// to LibSledLuaPlugin for processing.
	/// @brief
	/// Typedef for an edit and continue callback function.
	///
	/// @param pszFilePath The relative path of the file that needs to be reloaded
	/// @param pUserData Optional user-controlled userdata
	/// @return A <c>const char*</c> to the contents of the reloaded file
	///
	/// @see
	/// <c>EditAndContinueFinishCallback</c>
	typedef const char *(*EditAndContinueCallback)(const char *pszFilePath, void *pUserData);

	/// Typedef for an edit and continue callback function. This function synchronously loads the file. 
	/// Call this function after regular edit and continue callback processing has finished so that memory can be freed, if necessary.
	/// @brief
	/// Typedef for edit and continue callback function.
	///
	/// @param pszFilePath The relative path of the file that needs to be reloaded
	/// @param pUserData Optional user-controlled userdata
	///
	/// @see
	/// <c>EditAndContinueCallback</c>
	typedef void (*EditAndContinueFinishCallback)(const char *pszFilePath, void *pUserData);

	/// Namespace to scope variable exclude flags. Variable exclude flags exclude certain variable groups from being processed and 
	/// sent to SLED when execution stops on a breakpoint.
	/// @brief
	/// Namespace to scope variable exclude flags.
	namespace VarExcludeFlags
	{
		/// @brief
		/// Variable exclude flags for execution stops on a breakpoint.
		///
		/// Variable exclude flags exclude certain variable groups from being processed and sent to SLED when execution stops on a breakpoint.		
		enum Enum
		{
			kNone			= 0,		///< Do not exclude any variable groups. This is the default behavior.
			kGlobals		= (1 << 1),	///< Exclude processing and sending global variables
			kLocals			= (1 << 2),	///< Exclude processing and sending local variables
			kUpvalues		= (1 << 3),	///< Exclude processing and sending upvalue variables
			kEnvironment	= (1 << 4),	///< Exclude processing and sending environment table variables
		};
	}
	
	/// @brief
	/// LuaPlugin configuration parameters.
	///
	/// Configuration parameters for LuaPlugin.
	struct SCE_SLED_LINKAGE LuaPluginConfig
	{
		/// Constructor to initialize items.
		///
		/// @brief
		/// LuaPluginConfig constructor.
		LuaPluginConfig()
			: maxSendBufferSize(2048)
			, maxLuaStates(1)
			, maxLuaStateNameLen(32)
			, maxMemTraces(0)
			, maxBreakpoints(64)
			, maxEditAndContinues(0)
			, maxEditAndContinueEntryLen(0)
			, maxNumVarFilters(0)
			, maxVarFilterPatternLen(0)
			, maxPatternsPerVarFilter(0)
			, maxProfileFunctions(0)
			, maxProfileCallStackDepth(0)
			, numPathChopChars(0)
			, pfnChopCharsCallback(0)
			, pfnEditAndContinueCallback(0)
			, pfnEditAndContinueFinishCallback(0)
			, pEditAndContinueUserData(0)
			, maxWorkBufferSize(1024)
		{}

		uint32_t	maxSendBufferSize;			///< Maximum size, in bytes, of the send buffer (1024 recommended at a minimum)

		uint16_t	maxLuaStates;				///< Maximum number of Lua states that can be debugged
		uint16_t	maxLuaStateNameLen;			///< Maximum length of a Lua state name
		uint32_t	maxMemTraces;				///< Maximum number of memory traces to hold in memory
		uint16_t	maxBreakpoints;				///< Maximum number of breakpoints

		uint16_t	maxEditAndContinues;		///< Maximum number of scripts that can be modified when stopped on a breakpoint while debugging
		uint16_t	maxEditAndContinueEntryLen;	///< Maximum length of an edit and continue entry

		uint16_t	maxNumVarFilters;			///< Maximum number of variable filters
		uint16_t	maxVarFilterPatternLen;		///< Maximum length of a single pattern in a filter
		uint16_t	maxPatternsPerVarFilter;	///< Maximum number of patterns per filter

		uint16_t	maxProfileFunctions;		///< Maximum number of functions to profile
		uint16_t	maxProfileCallStackDepth;	///< Maximum call stack depth

		int32_t		numPathChopChars;			///< The number of characters to strip off the beginning of a path string

		ChopCharsCallback				pfnChopCharsCallback;				///< Modified path to compare breakpoint against

		EditAndContinueCallback			pfnEditAndContinueCallback;			///< Edit and continue callback function
		EditAndContinueFinishCallback	pfnEditAndContinueFinishCallback;	///< Edit and continue finish callback function
		void*							pEditAndContinueUserData;			///< Edit and continue callback userdata

		uint32_t	maxWorkBufferSize;			///< Maximum size of the work buffer (1024 recommended at a minimum)
	};
}}

#endif // __SCE_LIBSLEDLUAPLUGIN_PARAMS_H__
