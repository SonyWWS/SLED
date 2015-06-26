/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "sledluaplugin.h"
#include "../sleddebugger/sleddebugger.h"

#include "../sleddebugger/assert.h"
#include "../sleddebugger/buffer.h"
#include "../sleddebugger/sequentialallocator.h"
#include "../sleddebugger/sleddebugger_class.h"
#include "../sleddebugger/stringarray.h"
#include "../sleddebugger/utilities.h"

#include "sledluaplugin_class.h"
#include "luautils.h"
#include "scmp.h"
#include "profilestack.h"
#include "varfilter.h"

#include "../sledcore/mutex.h"

extern "C"
{
	#include <wws_lua/lua-5.1.4/src/lua.h>
	#include <wws_lua/lua-5.1.4/src/lualib.h>
	#include <wws_lua/lua-5.1.4/src/lauxlib.h>
}

#include <cstdlib>
#include <cstring>
#include <new>
#include <math.h>

#define LUA_PRE_RAWGET_CHECK(state, i1, i2) \
	SCE_SLEDMACRO_BEGIN \
		if ((::lua_type((state), (i1)) == (LUA_TNIL)) || \
			(::lua_type((state), (i2)) == (LUA_TNIL)) || \
			(::lua_type((state), (i2)) != (LUA_TTABLE))) \
		{ \
			return; \
		} \
	SCE_SLEDMACRO_END

namespace sce { namespace Sled
{
	namespace
	{
		inline void GetBasedOnContext(LuaVariableContext::Enum context, lua_State* state, int idx)
		{
			// custom watches can use metamethods but other watches cannot
			if (context == LuaVariableContext::kWatchCustom)
				::lua_gettable(state, idx);
			else
				::lua_rawget(state, idx);
		}

		inline void SetBasedOnContext(LuaVariableContext::Enum context, lua_State* state, int idx)
		{
			// custom watches can use metamethods but other watches cannot
			if (context == LuaVariableContext::kWatchCustom)
				::lua_settable(state, idx);
			else
				::lua_rawset(state, idx);
		}
	}

	void LuaPlugin::clientDisconnectedLua()
	{
		// Remove hook function from any Lua states & restore debugging state
		for (uint16_t i = 0; i < m_iNumLuaStates; i++)
		{
			m_pLuaStates[i].setDebugging(true);
			::lua_sethook(m_pLuaStates[i].luaState, 0, 0, 0);
		}
	}

	void LuaPlugin::clientBreakpointBeginLua(const BreakpointParams *pParams)
	{
		SCE_SLEDUNUSED(pParams);

		const std::size_t iFuncTagLen = Sled::SCMP::Base::kStringLen;
		lua_State* const luaState = m_pCurHookLuaState;
		lua_Debug* const ar = m_pCurHookLuaDebug;		

		// Get globals if not excluded
		if ((m_iVarExcludeFlags & VarExcludeFlags::kGlobals) != VarExcludeFlags::kGlobals)
		{
			const SCMP::GlobalVarBegin scmpGlBeg(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpGlBeg, scmpGlBeg.length);

			getGlobals(luaState);

			const SCMP::GlobalVarEnd scmpGlEnd(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpGlEnd, scmpGlEnd.length);
		}

		// Start callstack information
		const SCMP::CallStackBegin scmpCsBeg(kLuaPluginId);
		m_pScriptMan->send((uint8_t*)&scmpCsBeg, scmpCsBeg.length);

		// Get some more info:
		//	n = name, namewhat
		//	f = pushes onto stack function running at the given level
		if (::lua_getinfo(luaState, "fn", ar) == 1)
		{	
			char szFuncName[iFuncTagLen];
			tagFuncForLookUp(szFuncName, iFuncTagLen, ar->name, m_pszSource, ar->linedefined);

			// Notify client to create a new callstack entry for this level (0)
			const SCMP::CallStack scmpCsLv0(kLuaPluginId, m_pszSource, ar->currentline, ar->linedefined, ar->lastlinedefined, szFuncName, 0, m_pSendBuf);
			m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());

			// Get locals for this stack level (0) and send to client (if not excluded)
			if ((m_iVarExcludeFlags & VarExcludeFlags::kLocals) != VarExcludeFlags::kLocals)
			{
				const SCMP::LocalVarBegin scmpLoBeg(kLuaPluginId);
				m_pScriptMan->send((uint8_t*)&scmpLoBeg, scmpLoBeg.length);

				getLocals(luaState, ar, 0);

				const SCMP::LocalVarEnd scmpLoEnd(kLuaPluginId);
				m_pScriptMan->send((uint8_t*)&scmpLoEnd, scmpLoEnd.length);
			}

			// Get upvalues for this stack level (0) and send to client (if not excluded)
			if ((m_iVarExcludeFlags & VarExcludeFlags::kUpvalues) != VarExcludeFlags::kUpvalues)
			{
				const SCMP::UpvalueVarBegin scmpUpBeg(kLuaPluginId);
				m_pScriptMan->send((uint8_t*)&scmpUpBeg, scmpUpBeg.length);

				getUpvalues(luaState, -1, 0);

				const SCMP::UpvalueVarEnd scmpUpEnd(kLuaPluginId);
				m_pScriptMan->send((uint8_t*)&scmpUpEnd, scmpUpEnd.length);
			}

			// Get environment for this function and send to client if not excluded
			if ((m_iVarExcludeFlags & VarExcludeFlags::kEnvironment) != VarExcludeFlags::kEnvironment)
			{
				const SCMP::EnvVarBegin scmpEnvBeg(kLuaPluginId);
				m_pScriptMan->send((uint8_t*)&scmpEnvBeg, scmpEnvBeg.length);

				getEnvironment(luaState, ::lua_gettop(luaState), 0);

				const SCMP::EnvVarEnd scmpEnvEnd(kLuaPluginId);
				m_pScriptMan->send((uint8_t*)&scmpEnvEnd, scmpEnvEnd.length);
			}

			// Pop function that was pushed on by lua_getinfo
			::lua_pop(luaState, 1);

			// Now loop through higher levels (if any) getting values
			lua_Debug arStack;
			int iLevel = 1;
			int iErr = ::lua_getstack(luaState, iLevel, &arStack);
			while (iErr == 1)
			{
				if (::lua_getinfo(luaState, "Snlf", &arStack) == 1)
				{
					const char *pszLevelSource = trimFileName(arStack.source);

					tagFuncForLookUp(szFuncName, iFuncTagLen, arStack.name, pszLevelSource, arStack.linedefined);

					// Notify client to create a new callstack entry for this level
					const SCMP::CallStack scmpCsLvX(kLuaPluginId, pszLevelSource, arStack.currentline, arStack.linedefined, arStack.lastlinedefined, szFuncName, (int16_t)iLevel, m_pSendBuf);
					m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());

					// Pop function pushed by lua_getinfo
					::lua_pop(luaState, 1);
				}

				++iLevel;
				iErr = ::lua_getstack(luaState, iLevel, &arStack);
			}

			// Note number of levels in the callstack
			m_iLastNumStackLevels = iLevel - 1;
		}

		const SCMP::CallStackEnd scmpCsEnd(kLuaPluginId);
		m_pScriptMan->send((uint8_t*)&scmpCsEnd, scmpCsEnd.length);
	}

	void LuaPlugin::clientDebugModeChangedLua(DebuggerMode::Enum newMode)
	{
		switch (newMode)
		{
		case DebuggerMode::kNormal:
			// The line hook may have been set by one of the other debugging
			// modes so remove it if there's a chance it is still there
			if (m_iNumBreakpoints == 0)
			{
				const int iProfileMask = m_bProfilerRunning ? LUA_MASKCALL | LUA_MASKRET : 0;

				for (uint16_t i = 0; i < m_iNumLuaStates; i++) {
					// Skip any states not being debugged
					if (!m_pLuaStates[i].isDebugging())
						continue;

					// Remove any hooks
					::lua_sethook(m_pLuaStates[i].luaState, NULL, 0, 0);

					// Re-add profiler hook if it should be on
					if (iProfileMask)
						::lua_sethook(m_pLuaStates[i].luaState, LuaPlugin::hookFunc, iProfileMask, 0);
				}
			}
			break;
		case DebuggerMode::kStepInto:
		case DebuggerMode::kStepOver:
		case DebuggerMode::kStepOut:
		case DebuggerMode::kStop:
			// Add the line hook if no breakpoints exist as removing all breakpoints
			// would have removed all line hooks
			if (m_iNumBreakpoints == 0)
			{
				const int iProfileMask = m_bProfilerRunning ? LUA_MASKRET | LUA_MASKRET : 0;

				for (uint16_t i = 0; i < m_iNumLuaStates; i++)
				{
					// Skip any states not being debugged
					if (!m_pLuaStates[i].isDebugging())
						continue;

					// Add line hook and/or profile hook
					::lua_sethook(m_pLuaStates[i].luaState, LuaPlugin::hookFunc, LUA_MASKLINE | iProfileMask, 0);
				}
			}
			break;
		}
	}

	int32_t LuaPlugin::registerLuaState(lua_State *luaState, const char *pszName)
	{
		const sce::SledPlatform::MutexLocker smg(m_pMutex); // SledDebugger is not needed to be locked

		// SledDebugger instance must be valid first
		if (!m_pScriptMan)
			return SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE;

		if (!luaState)
			return SCE_SLED_LUA_ERROR_INVALIDLUASTATE;

		LuaStateParams hTemp(luaState);

		// Check if already registered
		for (uint16_t i = 0; i < m_iNumLuaStates; i++)
		{
			if (hTemp == m_pLuaStates[i])
				return SCE_SLED_LUA_ERROR_DUPLICATELUASTATE;
		}

		// Verify we have space to add this Lua state
		if ((m_iNumLuaStates + 1) > m_iMaxLuaStates)
		{
			SCE_SLED_LOG(Logging::kError, "[SLED] [Lua] No space for Lua state!");
			return SCE_SLED_LUA_ERROR_OVERLUASTATELIMIT;
		}

		// Create global tables and shove everything in there!
		{
			// libsleddebugger table section
			{
				const StackReconciler recon(luaState);

				// Check if SledDebugger table exists
				::lua_getglobal(luaState, SCE_SLED_SLEDDEBUGGER_TABLE_STRING);
				if (lua_isnil(luaState, -1))
				{
					// Pop nil
					::lua_pop(luaState, 1);

					// Create a global SledDebugger table
					::lua_pushstring(luaState, SCE_SLED_SLEDDEBUGGER_TABLE_STRING);
					::lua_newtable(luaState);
					::lua_settable(luaState, LUA_GLOBALSINDEX);

					// Get SledDebugger table on top of stack
					::lua_getglobal(luaState, SCE_SLED_SLEDDEBUGGER_TABLE_STRING);

					// Push SledDebugger* and set SledDebugger["instance"] = SledDebugger*
					::lua_pushlightuserdata(luaState, m_pScriptMan);
					::lua_setfield(luaState, -2, SCE_SLED_SLEDDEBUGGER_INSTANCE_STRING);

					// Push SledDebugger version # and set SledDebugger["version"] = version_#
					const Version verSled = m_pScriptMan->getVersion();
					::lua_pushfstring(luaState, "%d.%d.%d", verSled.majorNum, verSled.minorNum, verSled.revisionNum);
					::lua_setfield(luaState, -2, SCE_SLED_SLEDDEBUGGER_VER_STRING);
				}
				else
				{
					// If table already exists then make sure embedded SledDebugger* is
					// equivalent to m_pScriptMan. We don't want to allow a lua_State* to
					// be registered to two different LuaPlugin's.

					::lua_getfield(luaState, -1, SCE_SLED_SLEDDEBUGGER_INSTANCE_STRING);
					if (!lua_islightuserdata(luaState, -1))
					{
						// Some kind of error - supposed to be SledDebugger*
						SCE_SLED_LOG(Logging::kError, "[SLED] [Lua] Lua state already registered!");
						return SCE_SLED_LUA_ERROR_LUASTATEALREADYREGISTERED;
					}

					SledDebugger* debugger = static_cast<SledDebugger*>(::lua_touserdata(luaState, -1));
					if (debugger != m_pScriptMan)
					{
						// Not the right SledDebugger*
						SCE_SLED_LOG(Logging::kError, "[SLED] [Lua] Lua state already registered!");
						return SCE_SLED_LUA_ERROR_LUASTATEALREADYREGISTERED;
					}
				}
			}

			// -------------------------------------------------------------------

			{
				const StackReconciler recon(luaState);

				// Check if LuaPlugin table exists
				::lua_getglobal(luaState, SCE_SLED_LUAPLUGIN_TABLE_STRING);
				if (!lua_isnil(luaState, -1))
				{
					// If table already exists then make sure embedded LuaPlugin* is
					// equivalent to "this". We don't want to allow a lua_State* to
					// be registered to two different LuaPlugin's.

					::lua_getfield(luaState, -1, SCE_SLED_LUAPLUGIN_INSTANCE_STRING);
					if (!lua_islightuserdata(luaState, -1))
					{
						// Some kind of error - supposed to be SledDebugger*
						SCE_SLED_LOG(Logging::kError, "[SLED] [Lua] Lua state already registered!");
						return SCE_SLED_LUA_ERROR_LUASTATEALREADYREGISTERED;
					}

					LuaPlugin *plugin = static_cast<LuaPlugin*>(::lua_touserdata(luaState, -1));
					if (plugin != this)
					{
						// Not the right LuaPLugin*
						SCE_SLED_LOG(Logging::kError, "[SLED] [Lua] Lua state already registered!");
						return SCE_SLED_LUA_ERROR_LUASTATEALREADYREGISTERED;
					}
				}
				else
				{
					// Pop nil
					::lua_pop(luaState, 1);

					// Create a global LuaPlugin table
					::lua_pushstring(luaState, SCE_SLED_LUAPLUGIN_TABLE_STRING);
					::lua_newtable(luaState);
					::lua_settable(luaState, LUA_GLOBALSINDEX);

					// Get LuaPlugin table on top of stack
					::lua_getglobal(luaState, SCE_SLED_LUAPLUGIN_TABLE_STRING);
					const int iTableIndex = ::lua_gettop(luaState);

					// Push LuapLugin* and set LuaPlugin["instance"] = LuaPlugin*
					::lua_pushlightuserdata(luaState, this);
					::lua_setfield(luaState, -2, SCE_SLED_LUAPLUGIN_INSTANCE_STRING);

					// Push LuaPlugin version # and set LuaPlugin["version"] = version_#
					const Version verLuaPlugin = getVersion();
					::lua_pushfstring(luaState, "%d.%d.%d", verLuaPlugin.majorNum, verLuaPlugin.minorNum, verLuaPlugin.revisionNum);
					::lua_setfield(luaState, -2, SCE_SLED_LUAPLUGIN_VER_STRING);

					// Push assert function and set LuaPlugin["assert"] = assert_func
					::lua_pushcfunction(luaState, LuaPlugin::luaAssert);
					::lua_setfield(luaState, -2, SCE_SLED_LUAPLUGIN_ASSERT_STRING);

					// Push breakpoint function and set LuaPlugin["bp_func"] = bp_func
					{
						const char* tempFunc = "function " SCE_SLED_LUAPLUGIN_TABLE_STRING ":" SCE_SLED_LUAPLUGIN_BP_FUNC_STRING "() end";
						const std::size_t tempFuncLen = std::strlen(tempFunc);

						::luaL_loadbuffer(luaState, tempFunc, tempFuncLen, SCE_SLED_LUAPLUGIN_BP_FUNC_STRING);
						::lua_setfield(luaState, -2, SCE_SLED_LUAPLUGIN_BP_FUNC_STRING);
					}					

					// Push tty function and set LuaPlugin["tty"] = tty_func
					::lua_pushcfunction(luaState, LuaPlugin::luaTTY);
					::lua_setfield(luaState, -2, SCE_SLED_LUAPLUGIN_TTY_STRING);				

					// Push errorhandler function and set LuaPlugin["errorhandler"] = errorhandler_func
					::lua_pushcfunction(luaState, LuaPlugin::luaErrorHandler);
					::lua_setfield(luaState, -2, SCE_SLED_LUAPLUGIN_ERRORHANDLER_STRING);

					SCE_SLED_ASSERT(iTableIndex == ::lua_gettop(luaState));

					// Pop LuaPlugin table.
					::lua_pop(luaState, 1);

					// -------------------------------------------------------------------

					::lua_getglobal(luaState, SCE_SLED_LUAPLUGIN_TABLE_STRING);
					const int iPluginConfigTableIndex = ::lua_gettop(luaState);

					// Set LuaPluginConfig[userdatatostring]
					::lua_pushstring(luaState, SCE_SLED_LUAPLUGIN_USERDATATOSTRING_STRING);
					::lua_pushnil(luaState);
					::lua_settable(luaState, iPluginConfigTableIndex);

					// Set LuaPluginConfig[editandcontinue]
					::lua_pushstring(luaState, SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_STRING);
					::lua_newtable(luaState);
					::lua_settable(luaState, iPluginConfigTableIndex);

					::lua_getfield(luaState, iPluginConfigTableIndex, SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_STRING);
					const int iEditAndContinueTableIndex = ::lua_gettop(luaState);

					::lua_pushstring(luaState, SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_CALLBACK_STRING);
					::lua_pushlightuserdata(luaState, (void*)m_pfnEditAndContinueCallback);
					::lua_settable(luaState, iEditAndContinueTableIndex);

					::lua_pushstring(luaState, SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_FINISH_CALLBACK_STRING);
					::lua_pushlightuserdata(luaState, (void*)m_pfnEditAndContinueFinishCallback);
					::lua_settable(luaState, iEditAndContinueTableIndex);

					::lua_pushstring(luaState, SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_CALLBACK_USERDATA_STRING);
					::lua_pushlightuserdata(luaState, m_pEditAndContinueUserData);
					::lua_settable(luaState, iEditAndContinueTableIndex);

					// pop LuaPluginConfig[editandcontinue] table
					::lua_pop(luaState, 1);

					// Pop LuaPluginConfig table.
					::lua_pop(luaState, 1);
				}
			}
		}

		// Add this Lua state
		{
			m_pLuaStates[m_iNumLuaStates] = hTemp;

			const uint16_t pos = m_iNumLuaStates * m_iMaxLuaStateNameLen;
			Utilities::copyString(m_pLuaStatesNames + pos, m_iMaxLuaStateNameLen, pszName);

			m_iNumLuaStates++;
		}

		const bool bAreThereBreakpoints = (m_iNumBreakpoints != 0);

		// Set hook function if any breakpoints, profiler running, or debug mode is not normal
		if (bAreThereBreakpoints || (m_pScriptMan->getDebuggerMode() != DebuggerMode::kNormal) || m_bProfilerRunning)
		{
			const int iProfileMask = m_bProfilerRunning ? (LUA_MASKCALL | LUA_MASKRET) : 0;
			const int iBreakpointMask = (bAreThereBreakpoints || (m_pScriptMan->getDebuggerMode() != DebuggerMode::kNormal)) ? LUA_MASKLINE : 0;

			::lua_sethook(luaState, LuaPlugin::hookFunc, iProfileMask | iBreakpointMask, 0);
		}

		// If already connected to SLED notify it of this Lua state
		if (m_pScriptMan->isDebuggerConnected())
		{
			const SCMP::LuaStateBegin scmpLuaStBeg(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpLuaStBeg, scmpLuaStBeg.length);

			char szTemp[SCMP::Sizes::kPtrLen];
			StringUtilities::copyString(szTemp, SCMP::Sizes::kPtrLen, "0x%p", luaState);

			const SCMP::LuaStateAdd scmpLuaSt(kLuaPluginId, szTemp, pszName, true, m_pSendBuf);
			m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());

			const SCMP::LuaStateEnd scmpLuaStEnd(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpLuaStEnd, scmpLuaStEnd.length);
		}				

		return SCE_SLED_ERROR_OK;
	}

	int32_t LuaPlugin::unregisterLuaState(lua_State *luaState)
	{
		const sce::SledPlatform::MutexLocker smg(m_pMutex); // SledDebugger is not needed to be  locked

		// SledDebugger instance must be valid first
		if (!m_pScriptMan)
			return SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE;

		if (!luaState)
			return SCE_SLED_LUA_ERROR_INVALIDLUASTATE;

		LuaStateParams hTemp(luaState);

		uint16_t iIndex = 0;
		bool bFound = false;

		for (uint16_t i = 0; (i < m_iNumLuaStates) && !bFound; i++)
		{
			if (hTemp == m_pLuaStates[i])
			{
				bFound = true;
				iIndex = i;
			}
		}

		if (!bFound)
			return SCE_SLED_LUA_ERROR_LUASTATENOTFOUND;

		// Remove from list
		--m_iNumLuaStates;
		for (uint16_t i = iIndex; i < m_iNumLuaStates; i++)
		{
			const uint16_t oldpos = (i + 1) * m_iMaxLuaStateNameLen;
			const char *name = m_pLuaStatesNames + oldpos;

			const uint16_t newpos = i * m_iMaxLuaStateNameLen;
			Utilities::copyString(m_pLuaStatesNames + newpos, m_iMaxLuaStateNameLen, name);

			m_pLuaStates[i] = m_pLuaStates[i + 1];
		}

		// Set global tables to nil
		{
			::lua_pushnil(luaState);
			::lua_setglobal(luaState, SCE_SLED_SLEDDEBUGGER_TABLE_STRING);

			::lua_pushnil(luaState);
			::lua_setglobal(luaState, SCE_SLED_LUAPLUGIN_TABLE_STRING);
		}

		// Remove any hooks
		::lua_sethook(luaState, NULL, 0, 0);

		// If SLED connected notify to remove this Lua state
		if (m_pScriptMan->isDebuggerConnected())
		{
			const SCMP::LuaStateBegin scmpLuaStBeg(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpLuaStBeg, scmpLuaStBeg.length);

			char szTemp[SCMP::Sizes::kPtrLen];
			StringUtilities::copyString(szTemp, SCMP::Sizes::kPtrLen, "0x%p", luaState);

			const SCMP::LuaStateRemove scmpLuaSt(kLuaPluginId, szTemp, m_pSendBuf);
			m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());

			const SCMP::LuaStateEnd scmpLuaStEnd(kLuaPluginId);
			m_pScriptMan->send((uint8_t*)&scmpLuaStEnd, scmpLuaStEnd.length);
		}

		return SCE_SLED_ERROR_OK;
	}

	void LuaPlugin::debuggerBreak(lua_State *luaState, const char *pszText)
	{
		const sce::SledPlatform::MutexLocker smgSm(m_pScriptMan->getMutex());
		const sce::SledPlatform::MutexLocker smg(m_pMutex);

		SCE_SLED_ASSERT(luaState != NULL);

		// If not connected then abort
		if (!m_pScriptMan->isDebuggerConnected())
			return;

		bool bFound = false;
		bool bDebugged = false;
		const LuaStateParams hTemp(luaState);

		// Check if Lua state is currently being debugged	
		for (uint16_t i = 0; (i < m_iNumLuaStates) && !bFound; i++)
		{
			if (m_pLuaStates[i] == hTemp)
			{
				bDebugged = m_pLuaStates[i].isDebugging();
				bFound = true;			
			}
		}

		// If debugged then add the line hook and force a breakpoint
		if (bDebugged && bFound)
		{
			// Add line hook to the Lua state
			const int iProfileMask = m_bProfilerRunning ? LUA_MASKCALL | LUA_MASKRET : 0;
			::lua_sethook(luaState, LuaPlugin::hookFunc, LUA_MASKLINE | iProfileMask, 0);

			// Notify to stop
			m_bAssertBreakpoint = true;

			// Optionally, send text to SLED
			if (pszText && (std::strlen(pszText) > 0))
			{
				ttyNotify(pszText);
				ttyNotify("\n");
			}
		}
	}

	void LuaPlugin::debuggerBreak(const char *pszText)
	{
		const sce::SledPlatform::MutexLocker smgSm(m_pScriptMan->getMutex());
		const sce::SledPlatform::MutexLocker smg(m_pMutex);

		// If not connected then abort
		if (!m_pScriptMan->isDebuggerConnected())
			return;	

		// Place the Lua hook on all states that are debug-able
		bool bDebugged = false;	

		for (uint16_t i = 0; i < m_iNumLuaStates; i++)
		{
			if (!m_pLuaStates[i].isDebugging())
				continue;

			bDebugged = true;

			const int iProfileMask = m_bProfilerRunning ? LUA_MASKCALL | LUA_MASKRET : 0;
			::lua_sethook(m_pLuaStates[i].luaState, LuaPlugin::hookFunc, LUA_MASKLINE | iProfileMask, 0);
		}

		// Found at least one Lua state to put the line hook on so force a breakpoint (eventually)
		if (bDebugged)
		{
			// Notify to stop (eventually)
			m_bAssertBreakpoint = true;

			// Optionally, send text to SLED
			if (pszText && (std::strlen(pszText) > 0))
			{
				ttyNotify(pszText);
				ttyNotify("\n");
			}
		}
	}

	int32_t LuaPlugin::getErrorHandlerAbsStackIndex(lua_State *luaState, int *outAbsStackIndex)
	{
		if (!luaState)
			return SCE_SLED_ERROR_NULLPARAMETER;

		if (!outAbsStackIndex)
			return SCE_SLED_ERROR_NULLPARAMETER;

		if (!m_pScriptMan)
			return SCE_SLED_LUA_ERROR_NODEBUGGERINSTANCE;	

		if (!luaState)
			return SCE_SLED_LUA_ERROR_INVALIDLUASTATE;

		// Safe index for use with lua_pcall
		(*outAbsStackIndex) = 0;

		// Look for global libsledluaplugin table
		::lua_getglobal(luaState, SCE_SLED_LUAPLUGIN_TABLE_STRING);
		if (lua_istable(luaState, -1))
		{
			// Look for errorhandler function
			::lua_getfield(luaState, -1, SCE_SLED_LUAPLUGIN_ERRORHANDLER_STRING);
			if (::lua_iscfunction(luaState, -1))
			{
				(*outAbsStackIndex) = ::lua_gettop(luaState);
			}
		}
		else
		{
			// Lua state has not been registered w/ any LuaPlugin
			return SCE_SLED_LUA_ERROR_INVALIDLUASTATE;
		}

		return SCE_SLED_ERROR_OK;
	}

	LuaPlugin *LuaPlugin::getWhichLuaPlugin(lua_State *luaState)
	{
		if (luaState == NULL)
			return NULL;

		const StackReconciler recon(luaState);
		LuaPlugin *pWhichPlugin = NULL;

		// Look for global libsledluaplugin table
		::lua_getglobal(luaState, SCE_SLED_LUAPLUGIN_TABLE_STRING);
		if (lua_istable(luaState, -1))
		{
			::lua_getfield(luaState, -1, SCE_SLED_LUAPLUGIN_INSTANCE_STRING);
			if (lua_islightuserdata(luaState, -1))
			{
				pWhichPlugin = static_cast<LuaPlugin*>(::lua_touserdata(luaState, -1));
			}
		}

		return pWhichPlugin;
	}

	void LuaPlugin::hookFunc(lua_State *luaState, lua_Debug *ar)
	{
		SCE_SLED_ASSERT(luaState != NULL);
		SCE_SLED_ASSERT(ar != NULL);

		if (!luaState || !ar)
			return;

		// Not much info on a tail call
		if (ar->event == LUA_HOOKTAILRET)
			return;

		// Try and find LuaPlugin from luaState
		LuaPlugin *pWhichPlugin = getWhichLuaPlugin(luaState);
		if (pWhichPlugin != NULL)
		{
			// Tie back into one of the non-static methods
			if (ar->event == LUA_HOOKLINE)
			{
				pWhichPlugin->hookFunc_Breakpoint(luaState, ar);
			}
			else
			{
				pWhichPlugin->hookFunc_Profiler(luaState, ar);
			}	
		}
		else
		{
			// Show some kind of error if we don't know where to dispatch
			SCE_SLED_LOG(Logging::kError, "[SLED] [LuaPlugin] Can't figure out which LuaPlugin* to dispatch to in HookFunc() function!");
		}
	}

	int LuaPlugin::luaAssert(lua_State *luaState)
	{
		SCE_SLED_ASSERT(luaState != NULL);

		if (!luaState)
			return 0;

		// Try and find LuaPlugin from luaState
		LuaPlugin *pWhichPlugin = getWhichLuaPlugin(luaState);
		if (pWhichPlugin != NULL)
		{
			pWhichPlugin->luaAssertInternal(luaState);
		}
		else
		{
			// Show some kind of error if we don't know where to dispatch
			SCE_SLED_LOG(Logging::kError, "[SLED] [LuaPlugin] Can't figure out which LuaPlugin* to dispatch to in LuaAssert() function!");
		}

		return 0;
	}

	int LuaPlugin::luaTTY(lua_State *luaState)
	{
		SCE_SLED_ASSERT(luaState != NULL);

		if (!luaState)
			return 0;

		// Nothing to print (?)
		const int iCount = ::lua_gettop(luaState);
		if (iCount <= 0)
			return 0;

		// Try and find LuaPlugin from luaState
		LuaPlugin *pWhichPlugin = getWhichLuaPlugin(luaState);
		if (pWhichPlugin != NULL)
		{
			// Pull out strings
			for (int i = 1; i <= iCount; i++)
			{
				const char *pszString = ::lua_tostring(luaState, i);
				if (pszString != NULL) {
					pWhichPlugin->ttyNotify(pszString);
				}
			}

			// print() adds newline so we shall too
			pWhichPlugin->ttyNotify("\n");
		}
		else
		{
			// Show some kind of error if we don't know where to dispatch
			SCE_SLED_LOG(Logging::kError, "[SLED] [LuaPlugin] Can't figure out which LuaPlugin* to dispatch to in LuaTTY() function!");
		}

		return 0;
	}

	int LuaPlugin::luaErrorHandler(lua_State *luaState)
	{
		SCE_SLED_ASSERT(luaState != NULL);

		if (!luaState)
			return 0;

		// Try and find LuaPlugin from luaState
		LuaPlugin *pWhichPlugin = getWhichLuaPlugin(luaState);
		if (pWhichPlugin != NULL)
		{
			pWhichPlugin->luaErrorHandlerInternal(luaState);		
		}
		else
		{
			// Show some kind of error if we don't know where to dispatch
			SCE_SLED_LOG(Logging::kError, "[SLED] [LuaPlugin] Can't figure out which LuaPlugin* to dispatch to in LuaErrorHandler() function!");
		}

		return 0;
	}	

	void LuaPlugin::luaAssertInternal(lua_State *luaState)
	{
		SCE_SLED_ASSERT(luaState != NULL);

		if (!luaState || !m_pScriptMan->isDebuggerConnected())
			return;

		// Need at least one argument on the stack
		const int iNumArgs = ::lua_gettop(luaState);
		if (iNumArgs < 1)
			return;

		// Check if stack contents can be converted to a boolean value
		if (lua_isboolean(luaState, 1) == 0)
			return;

		// Get as boolean value. 
		// If the value is false (ie. assertion fails) we want to break.
		const int iBoolean = ::lua_toboolean(luaState, 1);
		if (iBoolean == 1)
			return;

		// Check if there's a comment associated w/ the assert
		if (iNumArgs >= 2)
		{
			const char *pszText = ::lua_tostring(luaState, 2);
			debuggerBreak(luaState, pszText);
		}
		else
		{
			debuggerBreak(luaState, 0);
		}
	}

	void LuaPlugin::luaErrorHandlerInternal(lua_State *luaState)
	{
		SCE_SLED_ASSERT(luaState != NULL);

		if (!luaState || !m_pScriptMan->isDebuggerConnected())
			return;

		// Send over error string from Lua
		if (::lua_gettop(luaState) > 0)
			ttyNotify(::lua_tostring(luaState, -1));

		// Signal to stop
		m_bErrorBreakpoint = true;

		// Stack index 0 is this function
		// Stack index 1 is the function that called this function
		const int stackIndex = 1;

		const int success = 1;

		lua_Debug ar;
		if (::lua_getstack(luaState, stackIndex, &ar) == success)
		{
			// Force a stop now
			hookFunc_Breakpoint(luaState, &ar);
		}
	}

	void LuaPlugin::hookFunc_Profiler(lua_State *luaState, lua_Debug *ar)
	{
		SCE_SLED_ASSERT(luaState != NULL);
		SCE_SLED_ASSERT(ar != NULL);

		// Get info - fills out stuff in activation record:
		// S = short_src, source, linedefined, lastlinedefined, what
		// n = name, namewhat
		::lua_getinfo(luaState, "Sn", ar);

		const char *pszSource = 0;

		// Grab source file name (and if C code don't trim it)
		if ((ar->what) && (ar->what[0] == 'C'))
		{
			pszSource = ar->source;
		}
		else
		{
			pszSource = trimFileName(ar->source);
		}

		const std::size_t len = Sled::SCMP::Base::kStringLen;

		// Get function name or indicate that SLED should look up the function
		char szFuncName[len];
		tagFuncForLookUp(szFuncName, len, ar->name, pszSource, ar->linedefined);

		if (ar->event == LUA_HOOKCALL)
		{
			m_pProfileStack->enterFn(szFuncName, pszSource, ar->linedefined);
		}
		else
		{
			m_pProfileStack->leaveFn(szFuncName, pszSource, ar->linedefined);
		}
	}

	void LuaPlugin::hookFunc_Breakpoint(lua_State *luaState, lua_Debug *ar)
	{
		SCE_SLED_ASSERT(luaState != NULL);
		SCE_SLED_ASSERT(ar != NULL);

		// Get info - fills out stuff in activation record:
		// S = short_src, source, linedefined, lastlinedefined, what
		// l = current line
		::lua_getinfo(luaState, "Sl", ar);

		// If a C function, abort
		if (ar->what && (ar->what[0] == 'C'))
			return;			

		// Grab current debug mode
		const DebuggerMode::Enum iDebugMode = m_pScriptMan->getDebuggerMode();

		// Grab source file name
		const char *pszSource = trimFileName(ar->source);			

		// Will always stop for sure if there's a breakpoint or:
		//		iDebugMode == DebuggerMode::kStepInto
		//		iDebugMode == DebuggerMode::kStop				
		bool bNotBreakpointCaused = (iDebugMode == DebuggerMode::kStepInto) || (iDebugMode == DebuggerMode::kStop) || m_bAssertBreakpoint || m_bErrorBreakpoint;

		// Extra work to do for these two cases
		if ((iDebugMode == DebuggerMode::kStepOver) || (iDebugMode == DebuggerMode::kStepOut))
		{
			// Figure out number of callstack levels _this_ time
			lua_Debug tmpStack;
			int iLevel = 1;					
			int iErr = ::lua_getstack(luaState, iLevel, &tmpStack);
			while (iErr == 1)
			{
				++iLevel;
				iErr = ::lua_getstack(luaState, iLevel, &tmpStack);
			}

			// While loop adds an extra so compensate
			--iLevel;

			if (iDebugMode == DebuggerMode::kStepOver)
			{
				// For stepping over we want to stay at the same
				// level or less as last time we stopped
				bNotBreakpointCaused = (iLevel <= m_iLastNumStackLevels);
			}
			else if (iDebugMode == DebuggerMode::kStepOut)
			{
				// For stepping out we want less levels than last time we stopped
				bNotBreakpointCaused = (iLevel < m_iLastNumStackLevels);
			}
		}

		// See if this line is a breakpoint or there is some other criteria that is stopping us
		if (bNotBreakpointCaused || isLineBreakpoint(luaState, pszSource, ar->currentline))
		{
			const sce::SledPlatform::MutexLocker smgSm(m_pScriptMan->getMutex());
			const sce::SledPlatform::MutexLocker smg(m_pMutex);

			m_bAssertBreakpoint = false;
			m_bErrorBreakpoint = false;
			m_iLastNumStackLevels = 0;

			// Pause profile timer & store some stuff
			m_pProfileStack->preBreakpoint();
			m_pCurHookLuaState = luaState;
			m_pCurHookLuaDebug = ar;
			m_bHitBreakpoint = true;
			m_pszSource = pszSource; // note this

			// Run hit-breakpoint-logic; notify the script debugger manager
			const BreakpointParams params(kLuaPluginId, ar->currentline, pszSource);
			m_pScriptMan->breakpointReached(&params);

			// Do any edit & continue work
			handleEditAndContinue(luaState);

			// When control comes back to this function the breakpoint is over so we can
			// re-continue the profile timer and clear out some stuff
			m_pProfileStack->postBreakpoint();
			m_pCurHookLuaState = 0;
			m_pCurHookLuaDebug = 0;
			m_bHitBreakpoint = false;
			m_pszSource = 0;
		}
	}

	bool LuaPlugin::isLineBreakpoint(lua_State *luaState, const char *pszSource, const int32_t& iCurrentLine)
	{
		SCE_SLED_ASSERT(luaState != NULL);
		SCE_SLED_ASSERT(pszSource != NULL);

		if (m_iNumBreakpoints == 0)
			return false;

		bool bRetval = false;
		bool bBreak = false;

		// Calculate hash
		int32_t iHash = 0;
		SledDebugger::generateHash(pszSource, iCurrentLine, &iHash);

		// Check breakpoint & condition stuff
		Breakpoint bp(pszSource, iCurrentLine, iHash);

		for (uint16_t i = 0; (i < m_iNumBreakpoints) && !bRetval && !bBreak; i++)
		{
			if (m_pBreakpoints[i] == bp)
			{
				// Stop the loop regardless of what happens next
				bBreak = true;

				if (m_pBreakpoints[i].hasCondition())
				{				
					const StackReconciler recon(luaState);
					const int kFuncLen = 2048;

					// Save off beginning stack size
					const int iCount1 = ::lua_gettop(luaState);

					// Fill in activation record
					lua_Debug ar;
					::lua_getstack(luaState, 0, &ar);

					// Start to generate a function
					char szCondFunc[kFuncLen];
					Utilities::copyString(szCondFunc, kFuncLen, "function ");
					Utilities::appendString(szCondFunc, kFuncLen, SCE_SLED_LUAPLUGIN_TABLE_STRING);
					Utilities::appendString(szCondFunc, kFuncLen, ":");
					Utilities::appendString(szCondFunc, kFuncLen, SCE_SLED_LUAPLUGIN_BP_FUNC_STRING);
					Utilities::appendString(szCondFunc, kFuncLen, "(");

					int iLocal = 1;
					int iUpval = 1;
					int iTemps = 0;
					int iAbsFuncIndex = 0;

					bool bFirst = true;

					// Get non-temp local variables for the function header
					// and push them on the stack for the function call later
					const char *pszLocal = ::lua_getlocal(luaState, &ar, iLocal);
					while (pszLocal)
					{
						// Variables starting with { are temporary
						if (pszLocal[0] != '(')
						{
							// Build function header
							if (!bFirst)
								Utilities::appendString(szCondFunc, kFuncLen, ", ");

							Utilities::appendString(szCondFunc, kFuncLen, pszLocal);
						}
						else
						{
							// Pop temp var
							::lua_pop(luaState, 1);
							iTemps++;
						}

						bFirst = false;

						// Get next
						pszLocal = ::lua_getlocal(luaState, &ar, ++iLocal);
					}

					// Adjust value
					iLocal--;

					// Get running function on stack
					::lua_getinfo(luaState, "f", &ar);
					// Get absolute position of running function
					iAbsFuncIndex = ::lua_gettop(luaState);

					// Get proper table to use as the environment
					// for the generated function
					if (m_pBreakpoints[i].useFunctionEnvironment())
					{
						// Get the function's environment table
						::lua_getfenv(luaState, iAbsFuncIndex);
					}
					else
					{
						// Get the globals table
						::lua_pushstring(luaState, "_G");
						::lua_gettable(luaState, LUA_GLOBALSINDEX);
					}

					// Get non-temp upvalue variables for the function header
					// and push them on the stack for the function call later
					const char *pszUpval = ::lua_getupvalue(luaState, iAbsFuncIndex, iUpval);
					while (pszUpval)
					{
						if (pszUpval[0] != '(')
						{
							// Build function header
							if (!bFirst)
								Utilities::appendString(szCondFunc, kFuncLen, ", ");

							Utilities::appendString(szCondFunc, kFuncLen, pszUpval);
						}
						else
						{
							// Pop temp var
							::lua_pop(luaState, 1);
							iTemps++;
						}

						bFirst = false;

						// Get next
						pszUpval = ::lua_getupvalue(luaState, iAbsFuncIndex, ++iUpval);
					}

					// Adjust value
					iUpval--;

					// Finish generating function
					Utilities::appendString(szCondFunc, kFuncLen, ")\nreturn (");
					Utilities::appendString(szCondFunc, kFuncLen, m_pBreakpoints[i].getCondition());
					Utilities::appendString(szCondFunc, kFuncLen, ")\nend");

					// Remove function pushed on for upvalue retrieval
					::lua_remove(luaState, iAbsFuncIndex);

					// Load the function chunk to an actual Lua function
					::luaL_loadbuffer(luaState, szCondFunc, std::strlen(szCondFunc), SCE_SLED_LUAPLUGIN_BP_FUNC_STRING);

					// Execute the function so the var name and signature get registered
					::lua_pcall(luaState, 0, 0, 0);

					// Get the libsledluaplugin table on top of stack
					::lua_getglobal(luaState, SCE_SLED_LUAPLUGIN_TABLE_STRING);

					// Get libsledluaplugin.bp_func function on top of stack
					::lua_getfield(luaState, -1, SCE_SLED_LUAPLUGIN_BP_FUNC_STRING);

					// Set proper environment table for libsledluaplugin.bp_func
					// First: move the table to the top of the stack...
					::lua_pushvalue(luaState, -3);
					// ... and remove the duplicate
					::lua_remove(luaState, ::lua_gettop(luaState) - 3);
					// Second: set the generated function's environment
					// to be this new table
					::lua_setfenv(luaState, -2);

					// Relocate libsledluaplugin.bp_func function "below" all of the pushed on locals/upvalues				
					::lua_insert(luaState, iCount1 + 1);

					// Move libsledluaplugin table so that it's the first function arg (ie. "self")
					::lua_insert(luaState, iCount1 + 2);

					// Run the function to do the condition test
					::lua_pcall(luaState, (iLocal + iUpval - iTemps) + 1, 1, 0);

					// Check top of stack for boolean
					if (::lua_type(luaState, -1) == LUA_TBOOLEAN)
					{
						const int iResult = ::lua_toboolean(luaState, -1);
						const bool bResult = m_pBreakpoints[i].getResult();

						if (bResult && (iResult == 1))
						{
							bRetval = true;
						}
						else if (!bResult && (iResult == 0))
						{
							bRetval = true;
						}
					}
				}
				else
				{
					bRetval = true;
				}
			}
		}

		return bRetval;
	}

	void LuaPlugin::setVariable(lua_State *L, const LuaVariable *pVar)
	{
		SCE_SLED_ASSERT(L != NULL);

		if (!L)
			return;

		const StackReconciler recon(L);
		lua_Debug ar;

		switch (pVar->what)
		{
		case LuaVariableScope::kGlobal:
			{
				// Get globals table on the top of the stack
				::lua_pushstring(L, "_G");
				::lua_rawget(L, LUA_GLOBALSINDEX);					

				luaPushValue(L, pVar->nameType, pVar->name);

				if (pVar->bTable)
				{
					// Get to right table level
					::lua_rawget(L, -2);

					const int iCount = (int)pVar->numKeyValues;
					for (int i = 0; i < (iCount - 1); i++)
					{
						luaPushValue(L, pVar->hKeyValues[i].type, pVar->hKeyValues[i].name);
						::lua_rawget(L, -2);
					}

					luaPushValue(L, pVar->hKeyValues[pVar->numKeyValues - 1].type, pVar->hKeyValues[pVar->numKeyValues - 1].name);
				}

				// Push & set new value
				luaPushValue(L, pVar->valueType, pVar->value);
				::lua_rawset(L, -3);
			}
			break;

		case LuaVariableScope::kLocal:
			{
				// Get the right stack level where the local lives
				if (::lua_getstack(L, pVar->level, &ar) == 1)
				{
					if (!pVar->bTable)
					{
						luaPushValue(L, pVar->valueType, pVar->value);
						::lua_setlocal(L, &ar, pVar->index);
					}
					else
					{
						// Push the table on the top of the stack
						if (::lua_getlocal(L, &ar, pVar->index))
						{
							const int iCount = (int)pVar->numKeyValues;
							for (int i = 0; i < (iCount - 1); i++)
							{
								luaPushValue(L, pVar->hKeyValues[i].type, pVar->hKeyValues[i].name);
								::lua_rawget(L, -2);
							}

							luaPushValue(L, pVar->hKeyValues[pVar->numKeyValues - 1].type, pVar->hKeyValues[pVar->numKeyValues - 1].name);
							luaPushValue(L, pVar->valueType, pVar->value);
							::lua_rawset(L, -3);
						}
					}
				}
			}
			break;

		case LuaVariableScope::kUpvalue:
			{
				// Get the right stack level where the upvalue lives
				if (::lua_getstack(L, pVar->level, &ar) == 1)
				{
					// Push function running at this stack level onto the stack
					if (::lua_getinfo(L, "f", &ar) == 1)
					{
						if (!pVar->bTable)
						{
							luaPushValue(L, pVar->valueType, pVar->value);
							::lua_setupvalue(L, -2, pVar->index);
						}
						else
						{
							// Push table on top of stack
							if (::lua_getupvalue(L, -1, pVar->index))
							{
								const int iCount = (int)pVar->numKeyValues;
								for (int i = 0; i < (iCount - 1); i++)
								{
									luaPushValue(L, pVar->hKeyValues[i].type, pVar->hKeyValues[i].name);
									::lua_rawget(L, -2);
								}

								luaPushValue(L, pVar->hKeyValues[pVar->numKeyValues - 1].type, pVar->hKeyValues[pVar->numKeyValues - 1].name);
								luaPushValue(L, pVar->valueType, pVar->value);
								::lua_rawset(L, -3);
							}
						}
					}
				}
			}
			break;

		case LuaVariableScope::kEnvironment:
			{
				// Get right stack level where environment table lives
				if (::lua_getstack(L, pVar->level, &ar) == 1)
				{
					// Push function @ that stack level
					if (::lua_getinfo(L, "f", &ar) == 1)
					{
						const int iFuncIndex = ::lua_gettop(L);

						// Push environment table
						::lua_getfenv(L, iFuncIndex);

						if (::lua_type(L, -1) == LUA_TTABLE)
						{
							if (!pVar->bTable)
							{
								luaPushValue(L, pVar->nameType, pVar->name);
								luaPushValue(L, pVar->valueType, pVar->value);
								SetBasedOnContext(pVar->context, L, -3);
							}
							else
							{
								// Push base table from inside environment table
								luaPushValue(L, pVar->nameType, pVar->name);
								GetBasedOnContext(pVar->context, L, -2);

								const int iCount = (int)pVar->numKeyValues;
								for (int i = 0; i < (iCount - 1); i++)
								{
									luaPushValue(L, pVar->hKeyValues[i].type, pVar->hKeyValues[i].name);
									GetBasedOnContext(pVar->context, L, -2);
								}

								luaPushValue(L, pVar->hKeyValues[pVar->numKeyValues - 1].type, pVar->hKeyValues[pVar->numKeyValues - 1].name);
								luaPushValue(L, pVar->valueType, pVar->value);
								SetBasedOnContext(pVar->context, L, -3);
							}
						}
					}
				}
			}
			break;
		}
	}

	void LuaPlugin::lookupVariable(lua_State *luaState, const LuaVariable *pVar)
	{
		SCE_SLED_ASSERT(luaState != NULL);
		SCE_SLED_ASSERT(pVar != NULL);

		if (!luaState)
			return;

		switch (pVar->what)
		{
		case LuaVariableScope::kGlobal:
			lookupGlobalVariable(luaState, pVar);
			break;

		case LuaVariableScope::kLocal:
			lookupLocalVariable(luaState, pVar);
			break;

		case LuaVariableScope::kUpvalue:
			lookupUpvalueVariable(luaState, pVar);
			break;

		case LuaVariableScope::kEnvironment:
			lookupEnvironmentVariable(luaState, pVar);
			break;

		default:
			SCE_SLED_LOG(Logging::kError, "[SLED] [LuaPLugin] LookUpVariable of unknown type!");
			break;
		}
	}

	void LuaPlugin::lookupGlobalVariable(lua_State *luaState, const LuaVariable *pVar)
	{
		// Get globals on the top of the stack
		::lua_pushstring(luaState, "_G");
		::lua_rawget(luaState, LUA_GLOBALSINDEX);

		const char *lastName = pVar->name;

		luaPushValue(luaState, pVar->nameType, pVar->name);
		LUA_PRE_RAWGET_CHECK(luaState, -1, -2);
		::lua_rawget(luaState, -2);

		for (int i = 0; i < pVar->numKeyValues; i++)
		{
			lastName = pVar->hKeyValues[i].name;
			luaPushValue(luaState, pVar->hKeyValues[i].type, pVar->hKeyValues[i].name);
			LUA_PRE_RAWGET_CHECK(luaState, -1, -2);
			::lua_rawget(luaState, -2);
		}

		const int iLuaType = ::lua_type(luaState, -1);
		if (iLuaType == LUA_TNIL)
			return;
		if ((iLuaType == LUA_TTABLE) && !pVar->bFlag)
		{
			getTableValues(luaState, pVar, pVar->what, 0, -2, 0);
		}
		else
		{
			char szValue[SCMP::Sizes::kVarValueLen];
			const int32_t iType = lookUpTypeVal(luaState, -1, szValue, SCMP::Sizes::kVarValueLen);
			int32_t iKeyType = pVar->nameType;
			if (pVar->numKeyValues != 0)
				iKeyType = pVar->hKeyValues[pVar->numKeyValues - 1].type;
			sendGlobal(pVar, lastName, iKeyType, szValue, iType);
		}
	}

	void LuaPlugin::lookupLocalVariable(lua_State *luaState, const LuaVariable *pVar)
	{
		lua_Debug ar;

		// Get the right stack level where the local lives
		if (::lua_getstack(luaState, pVar->level, &ar) == 1)
		{
			// Push the local on the top of the stack
			const char *pszLocal = ::lua_getlocal(luaState, &ar, pVar->index);

			// Verify the local pushed is what we were expecting
			if (pszLocal && Utilities::areStringsEqual(pVar->name, pszLocal))
			{
				const char *lastName = pVar->name;

				for (int i = 0; i < pVar->numKeyValues; i++)
				{
					lastName = pVar->hKeyValues[i].name;
					luaPushValue(luaState, pVar->hKeyValues[i].type, pVar->hKeyValues[i].name);
					LUA_PRE_RAWGET_CHECK(luaState, -1, -2);
					::lua_rawget(luaState, -2);
				}

				const int iLuaType = ::lua_type(luaState, -1);
				if (iLuaType == LUA_TNIL)
					return;
				if ((iLuaType == LUA_TTABLE) && !pVar->bFlag)
				{
					getTableValues(luaState, pVar, pVar->what, pVar->index, -2, pVar->level);
				}
				else
				{
					char szValue[SCMP::Sizes::kVarValueLen];
					const int32_t iType = lookUpTypeVal(luaState, -1, szValue, SCMP::Sizes::kVarValueLen);
					int32_t iKeyType = pVar->nameType;
					if (pVar->numKeyValues != 0)
					{
						iKeyType = pVar->hKeyValues[pVar->numKeyValues - 1].type;
					}
					sendLocal(pVar, lastName, iKeyType, szValue, iType, pVar->level, pVar->index);
				}
			}
		}
	}

	void LuaPlugin::lookupUpvalueVariable(lua_State *luaState, const LuaVariable *pVar)
	{
		// Use lua_getstack to get to the appropriate stack level and then a
		// call to lua_getinfo "f" will push the appropriate function on the stack
		lua_Debug ar;

		// Get the right stack level where the upvalue lives
		if (::lua_getstack(luaState, pVar->level, &ar) == 1)
		{
			// Push the function running at this stack level onto the stack
			if (::lua_getinfo(luaState, "f", &ar) == 1)
			{
				// Push the upvalue on to the top of the stack
				const char *pszUpvalue = ::lua_getupvalue(luaState, -1, pVar->index);

				// Verify the upvalue pushed is what we were expecting
				if (pszUpvalue && Utilities::areStringsEqual(pVar->name, pszUpvalue))
				{
					const char *lastName = pVar->name;

					for (int i = 0; i < pVar->numKeyValues; i++)
					{
						lastName = pVar->hKeyValues[i].name;
						luaPushValue(luaState, pVar->hKeyValues[i].type, pVar->hKeyValues[i].name);
						LUA_PRE_RAWGET_CHECK(luaState, -1, -2);
						::lua_rawget(luaState, -2);
					}

					const int iLuaType = ::lua_type(luaState, -1);
					if (iLuaType == LUA_TNIL)
						return;
					if ((iLuaType == LUA_TTABLE) && !pVar->bFlag)
					{
						getTableValues(luaState, pVar, pVar->what, pVar->index, -2, pVar->level);
					}
					else
					{
						char szValue[SCMP::Sizes::kVarValueLen];
						const int32_t iType = lookUpTypeVal(luaState, -1, szValue, SCMP::Sizes::kVarValueLen);
						int32_t iKeyType = pVar->nameType;
						if (pVar->numKeyValues != 0)
						{
							iKeyType = pVar->hKeyValues[pVar->numKeyValues - 1].type;
						}
						sendUpvalue(pVar, lastName, iKeyType, szValue, iType, pVar->level, pVar->index);
					}
				}
			}
		}
	}

	void LuaPlugin::lookupEnvironmentVariable(lua_State *luaState, const LuaVariable *pVar)
	{
		lua_Debug ar;

		// Get the right stack level
		if (::lua_getstack(luaState, pVar->level, &ar) == 1)
		{
			// Push the function running at this stack level onto the stack
			if (::lua_getinfo(luaState, "f", &ar) == 1)
			{
				// Get the absolute index of the function
				const int iFuncIndex = ::lua_gettop(luaState);

				// Push environment table on stack
				::lua_getfenv(luaState, iFuncIndex);

				if (::lua_type(luaState, -1) == LUA_TTABLE)
				{
					const char *lastName = pVar->name;

					luaPushValue(luaState, pVar->nameType, pVar->name);
					LUA_PRE_RAWGET_CHECK(luaState, -1, -2);
					::lua_rawget(luaState, -2);

					for (int i = 0; i < pVar->numKeyValues; i++)
					{
						lastName = pVar->hKeyValues[i].name;
						luaPushValue(luaState, pVar->hKeyValues[i].type, pVar->hKeyValues[i].name);

						LUA_PRE_RAWGET_CHECK(luaState, -1, -2);
						::lua_rawget(luaState, -2);
					}

					const int iLuaType = ::lua_type(luaState, -1);
					if (iLuaType == LUA_TNIL)
						return;
					if ((iLuaType == LUA_TTABLE) && !pVar->bFlag)
					{
						getTableValues(luaState, pVar, pVar->what, pVar->index, -2, pVar->level);
					}
					else
					{
						char szValue[SCMP::Sizes::kVarValueLen];
						const int32_t iType = lookUpTypeVal(luaState, -1, szValue, SCMP::Sizes::kVarValueLen);
						int32_t iKeyType = pVar->nameType;
						if (pVar->numKeyValues != 0)
						{
							iKeyType = pVar->hKeyValues[pVar->numKeyValues - 1].type;
						}
						sendEnvVar(pVar, lastName, iKeyType, szValue, iType, pVar->level);
					}

					// Pop environment table
					::lua_pop(luaState, 1);
				}

				// Pop function pushed by lua_getinfo
				::lua_pop(luaState, 1);
			}
		}
	}

	int32_t LuaPlugin::lookUpTypeVal(lua_State *L, int iIndex, char *pValue, const int& iValueLen, bool bPop /* = true */)
	{
		SCE_SLED_ASSERT(L != NULL);
		SCE_SLED_ASSERT(pValue != NULL);

		if (!L || !pValue)
			return -1;

		// Check the type of the item on the top of the stack
		const int32_t iType = ::lua_type(L, iIndex);

		switch (iType)
		{
		case LUA_TNIL:
			Utilities::copyString(pValue, iValueLen, "nil");
			break;
		case LUA_TNUMBER:
			{
				{
					const StackReconciler recon(L);
					::lua_pushvalue(L, iIndex);

					lua_Number num = ::lua_tonumber(L, -1);
					if (floor(num) == num)
					{
						StringUtilities::copyString(pValue, iValueLen, "%lu", (unsigned long)num);
					}
					else
					{
						Utilities::copyString(pValue, iValueLen, ::lua_tostring(L, -1));
					}
				}
			}
			break;
		case LUA_TSTRING:
			{		
				// Copy value since lua_tostring can mess up lua_next
				::lua_pushvalue(L, iIndex);

				Utilities::copyString(pValue, iValueLen, ::lua_tostring(L, -1));

				// Pop copy
				::lua_pop(L, 1);
			}
			break;
		case LUA_TTABLE:
			Utilities::copyString(pValue, iValueLen, "<table>");
			break;
		case LUA_TFUNCTION:
			{
				lua_CFunction pFunc = ::lua_tocfunction(L, iIndex);			
				if (pFunc != NULL)
				{
					StringUtilities::copyString(pValue, iValueLen, "0x%p", pFunc);
				}
				else
				{
					Utilities::copyString(pValue, iValueLen, "Lua function");
				}
			}
			break;
		case LUA_TBOOLEAN:
			Utilities::copyString(pValue, iValueLen, (::lua_toboolean(L, iIndex) ? "true" : "false"));
			break;
		case LUA_TUSERDATA:
			{
				const int fixedIndex = iIndex < 0 ? ::lua_gettop(L) + iIndex + 1 : iIndex;
				// Fill in default value
				Utilities::copyString(pValue, iValueLen, "<userdata>");

				// Try calling __tostring metamethod using aux library
				if (::luaL_callmeta(L, fixedIndex, "__tostring") == 1)
				{
					Utilities::copyString(pValue, iValueLen, ::lua_tostring(L, -1));
					::lua_pop(L, 1);
					SCE_SLED_ASSERT(::lua_type(L, fixedIndex) == LUA_TUSERDATA);
				}
				else
				{
					// Metamethod is not found

					// Want to make sure we pop off all garbage before
					// we try and process the Lua userdata in case the
					// user sent in a pseudo stack index for iIndex
					const StackReconciler recon(L);
					SCE_SLED_ASSERT(::lua_type(L, fixedIndex) == LUA_TUSERDATA);

					bool hasUserdataToStringCallback = false;

					// Look for global libsledluapluginconfig table
					::lua_getglobal(L, SCE_SLED_LUAPLUGIN_TABLE_STRING);
					SCE_SLED_ASSERT(::lua_type(L, fixedIndex) == LUA_TUSERDATA);

					if (lua_istable(L, -1))
					{
						// Look for userdata subtable
						::lua_getfield(L, -1, SCE_SLED_LUAPLUGIN_USERDATATOSTRING_STRING);
						SCE_SLED_ASSERT(::lua_type(L, fixedIndex) == LUA_TUSERDATA);

						if (lua_isfunction(L, -1))
						{
							SCE_SLED_ASSERT(::lua_type(L, fixedIndex) == LUA_TUSERDATA);
							hasUserdataToStringCallback = true;

							SCE_SLED_ASSERT(::lua_type(L, fixedIndex) == LUA_TUSERDATA);
							::lua_pushvalue(L, fixedIndex);

							const int iRet = ::lua_pcall(L, 1, 1, 0);
							if (iRet == 0)
							{
								const char *pszStr = ::lua_tostring(L, -1);
								StringUtilities::copyString(pValue, iValueLen, "<userdata - 0x%x>", pszStr);
							}
							else
							{
								const char *pszStr = ::lua_tostring(L, -1);
								StringUtilities::copyString(pValue, iValueLen, "ERROR in " SCE_SLED_LUAPLUGIN_TABLE_STRING "[" SCE_SLED_LUAPLUGIN_USERDATA_STRING "] : %s", pszStr);
							}
							// pop return value or error message
							::lua_pop(L, 1);
						}
						else
						{
							::lua_pop(L, 1); // pop nil or invalid type value.
						}
					}
					else
					{
						::lua_pop(L, 1); // pop nil or invalid type value.
					}

					if (!hasUserdataToStringCallback)
					{
						// Set the pointer as string.
						void *pLuaUserData = ::lua_touserdata(L, fixedIndex);
						StringUtilities::copyString(pValue, iValueLen, "<userdata - 0x%x>", pLuaUserData);
					}
				}
			}
			break;
		case LUA_TTHREAD:
			{
				// Fill in default value
				Utilities::copyString(pValue, iValueLen, "<thread>");

				// Examine coroutine
				lua_State *pThread = ::lua_tothread(L, iIndex);
				if (pThread)
				{
					if (pThread == L)
					{
						Utilities::copyString(pValue, iValueLen, "running");
					}
					else
					{
						switch (::lua_status(pThread))
						{
						case LUA_YIELD: Utilities::copyString(pValue, iValueLen, "suspended"); break;
						case 0:
							lua_Debug ar;
							if (::lua_getstack(pThread, 0, &ar) > 0)
							{
								Utilities::copyString(pValue, iValueLen, "normal");
							}
							else if (::lua_gettop(pThread) == 0)
							{
								Utilities::copyString(pValue, iValueLen, "dead");
							}
							else
							{
								Utilities::copyString(pValue, iValueLen, "suspended");
							}
							break;
						default:
							Utilities::copyString(pValue, iValueLen, "dead");
							break;
						}
					}
				}
			}
			break;
		case LUA_TLIGHTUSERDATA:
			Utilities::copyString(pValue, iValueLen, "<lightuserdata>");
			break;
		}

		// Pop item on top of stack
		if (bPop)
			::lua_pop(L, 1);

		return iType;
	}

	bool LuaPlugin::getStackIndexInfo(lua_State *luaState, int index, char *pName, int nameStrLen, int32_t *luaType)
	{
		SCE_SLED_ASSERT(luaState != NULL);
		SCE_SLED_ASSERT(pName != NULL);

		if (!luaState || !pName)
			return false;

		const int32_t theType = lookUpTypeVal(luaState, index, pName, nameStrLen, false);

		if (luaType)
			(*luaType) = theType;

		return theType != LUA_TNONE;
	}

	void LuaPlugin::getTableValues(lua_State *luaState, const LuaVariable *pVar, LuaVariableScope::Enum what, int32_t iVarIndex, int32_t iTableIndex, int32_t iStackLevel)
	{
		SCE_SLED_ASSERT(luaState != NULL);

		int32_t keyLuaType = 0;
		char szKey[SCMP::Sizes::kVarNameLen];
		char szValue[SCMP::Sizes::kVarValueLen];

		const int keyIndex = -2;
		const int valIndex = -1;	

		::lua_pushnil(luaState);
		while (::lua_next(luaState, iTableIndex) != 0)
		{		
			if (getStackIndexInfo(luaState, keyIndex, szKey, SCMP::Sizes::kVarNameLen, &keyLuaType))
			{
				// don't pop the value in lookUpTypeVal
				const int32_t iType = lookUpTypeVal(luaState, valIndex, szValue, SCMP::Sizes::kVarValueLen, false);

				switch (what)
				{
				case LuaVariableScope::kGlobal:
					// Check filter before sending
					if (!isGlobalVarTypeFiltered(iType) && !isGlobalVarNameFiltered(szKey))
					{
						sendGlobal(pVar, szKey, keyLuaType, szValue, iType);
					}
					break;
				case LuaVariableScope::kLocal:
					// Check filter before sending
					if (!isLocalVarTypeFiltered(iType) && !isLocalVarNameFiltered(szKey))
					{
						sendLocal(pVar, szKey, keyLuaType, szValue, iType, iStackLevel, iVarIndex);
					}
					break;			
				case LuaVariableScope::kUpvalue:
					// Check filter before sending
					if (!isUpvalueVarTypeFiltered(iType) && !isUpvalueVarNameFiltered(szKey))
					{
						sendUpvalue(pVar, szKey, keyLuaType, szValue, iType, iStackLevel, iVarIndex);
					}
					break;
				case LuaVariableScope::kEnvironment:
					// Check filter before sending
					if (!isEnvVarVarTypeFiltered(iType) && !isEnvVarVarNameFiltered(szKey))
					{
						sendEnvVar(pVar, szKey, keyLuaType, szValue, iType, iStackLevel);
					}
					break;
				}
			}

			// Pop the value (leaving original key alone)
			::lua_pop(luaState, 1);
		}
	}

	bool LuaPlugin::luaPushValue(lua_State *luaState, int32_t iType, const char *pszValue)
	{
		bool bPushed = true;

		switch (iType)
		{
		case LUA_TNUMBER:
			::lua_pushnumber(luaState, (::lua_Number)std::atof(pszValue));
			break;
		case LUA_TBOOLEAN:
			::lua_pushboolean(luaState, std::atoi(pszValue));
			break;
		case LUA_TSTRING:
			::lua_pushstring(luaState, pszValue);
			break;
		default:
			bPushed = false;
			break;
		}

		return bPushed;
	}

	int32_t LuaPlugin::getGlobals(lua_State *luaState)
	{	
		int32_t iGlobals = 0;

		const StackReconciler recon(luaState);

		::lua_pushnil(luaState);
		while (::lua_next(luaState, LUA_GLOBALSINDEX) != 0)
		{
			iGlobals++;

			// Key is index -2
			if (::lua_isstring(luaState, -2))
			{
				// Make a copy since lua_tostring
				// modifies it and will make lua_next
				// fail because of the modification
				::lua_pushvalue(luaState, -2);

				// Modify the newly created key copy
				const char *name = ::lua_tostring(luaState, -1);

				char value[SCMP::Sizes::kVarValueLen];

				// Gets info about the value and also pops
				// the top element which is the key copy
				const int32_t valueType = lookUpTypeVal(luaState, -2, value, SCMP::Sizes::kVarValueLen);

				// If filtered ignore sending
				if (!isGlobalVarTypeFiltered(valueType) && !isGlobalVarNameFiltered(name))
					sendGlobal(NULL, name, ::lua_type(luaState, -2), value, valueType);
			}

			// Pop the value (leaving the original key alone)
			::lua_pop(luaState, 1);
		}

		return iGlobals;
	}

	int32_t LuaPlugin::getLocals(lua_State *luaState, lua_Debug *ar, int32_t iStackLevel)
	{
		int32_t iLocals = 1;

		const StackReconciler recon(luaState);

		// Get first
		const char *pszLocal = ::lua_getlocal(luaState, ar, iLocals);

		while (pszLocal)
		{
			char szValue[SCMP::Sizes::kVarValueLen];

			const int32_t iType = lookUpTypeVal(luaState, -1, szValue, SCMP::Sizes::kVarValueLen);

			// If filtered ignore sending
			if (!isLocalVarTypeFiltered(iType) && !isLocalVarNameFiltered(pszLocal))
				sendLocal(NULL, pszLocal, LUA_TSTRING, szValue, iType, iStackLevel, iLocals);

			// Get next
			pszLocal = ::lua_getlocal(luaState, ar, ++iLocals);
		}

		// Return total number of locals
		return iLocals - 1;
	}

	int32_t LuaPlugin::getUpvalues(lua_State *luaState, int32_t iFuncIndex, int32_t iStackLevel)
	{
		int32_t iUpvalues = 1;

		const StackReconciler recon(luaState);

		// Get first
		const char *pszUpvalue = ::lua_getupvalue(luaState, iFuncIndex, iUpvalues);

		while (pszUpvalue)
		{
			char szValue[SCMP::Sizes::kVarValueLen];

			const int32_t iType = lookUpTypeVal(luaState, -1, szValue, SCMP::Sizes::kVarValueLen);

			// If filtered ignore sending
			if (!isUpvalueVarTypeFiltered(iType) && !isUpvalueVarNameFiltered(pszUpvalue))
				sendUpvalue(NULL, pszUpvalue, LUA_TSTRING, szValue, iType, iStackLevel, iUpvalues);

			// Get next
			pszUpvalue = ::lua_getupvalue(luaState, iFuncIndex, ++iUpvalues);
		}

		// Return total number of upvalues
		return iUpvalues - 1;
	}

	void LuaPlugin::getEnvironment(lua_State *luaState, int iFuncIndex, int32_t iStackLevel)
	{
		if (::lua_type(luaState, iFuncIndex) != LUA_TFUNCTION)
			return;

		const StackReconciler recon(luaState);

		// Get the current environment table
		::lua_getfenv(luaState, iFuncIndex);

		if (::lua_type(luaState, -1) == LUA_TTABLE)
		{
			const int iTable = ::lua_gettop(luaState);

			::lua_pushnil(luaState);
			while (::lua_next(luaState, iTable) != 0)
			{
				// Key is index -2
				if (::lua_isstring(luaState, -2))
				{
					// Make a copy as lua_tostring modifies the item and makes lua_next fail
					::lua_pushvalue(luaState, -2);

					// Modify newly created key copy
					const char *pszKey = ::lua_tostring(luaState, -1);

					char szValue[SCMP::Sizes::kVarValueLen];

					const int32_t iType = lookUpTypeVal(luaState, -2, szValue, SCMP::Sizes::kVarValueLen);

					// If filtered ignore sending
					if (!isEnvVarVarTypeFiltered(iType) && !isEnvVarVarNameFiltered(pszKey))
						sendEnvVar(NULL, pszKey, ::lua_type(luaState, -2), szValue, iType, iStackLevel);
				}

				// Pop the value (leaving the original key alone)
				::lua_pop(luaState, 1);
			}
		}

		// Pop the environment table
		::lua_pop(luaState, 1);
	}

	void LuaPlugin::handleEditAndContinue(lua_State *L)
	{
		SCE_SLED_ASSERT(L != NULL);

		if (m_pEditAndContinue->isEmpty())
			return;

		// Keep stack pristine
		const StackReconciler recon(L);

		EditAndContinueCallback pfnEditAndContinueCallback = NULL;
		EditAndContinueFinishCallback pfnEditAndContinueFinishCallback = NULL;
		void *pEditAndContinueUserData = NULL;

		//
		// Try and pull callback functions from Lua state
		//
		{
			// Remove any temp garbage caused by finding callback functions
			const StackReconciler recon1(L);

			// Look for global libsledluaplugin table
			::lua_getglobal(L, SCE_SLED_LUAPLUGIN_TABLE_STRING);
			if (lua_istable(L, -1))
			{
				// Look for userdata subtable
				::lua_getfield(L, -1, SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_STRING);
				if (lua_istable(L, -1))
				{
					// Get userdata subtable index
					const int iTableIndex = ::lua_gettop(L);

					// Try and get userdata callback
					::lua_getfield(L, iTableIndex, SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_CALLBACK_STRING);
					if (lua_islightuserdata(L, -1))
						pfnEditAndContinueCallback = reinterpret_cast< EditAndContinueCallback >(::lua_touserdata(L, -1));

					// Try and get userdatafinish callback
					::lua_getfield(L, iTableIndex, SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_FINISH_CALLBACK_STRING);
					if (lua_islightuserdata(L, -1))
						pfnEditAndContinueFinishCallback = reinterpret_cast< EditAndContinueFinishCallback >(::lua_touserdata(L, -1));

					// Try and get user supplied userdata
					::lua_getfield(L, iTableIndex, SCE_SLED_LUAPLUGIN_EDITANDCONTINUE_CALLBACK_USERDATA_STRING);
					if (lua_islightuserdata(L, -1))
						pEditAndContinueUserData = ::lua_touserdata(L, -1);
				}
			}
		}

		// If nothing in Lua state try default Utilities::OpenFileCallback() callback
		if (pfnEditAndContinueCallback == NULL)
		{
			if (Utilities::openFileCallback() != NULL)
				pfnEditAndContinueCallback = Utilities::openFileCallback();
		}

		// If nothing in Lua state try default Utilities::OpenFileFinishCallback() callback
		if (pfnEditAndContinueFinishCallback == NULL)
		{
			if (Utilities::openFileFinishCallback() != NULL)
				pfnEditAndContinueFinishCallback = Utilities::openFileFinishCallback();
		}

		// If no function pointers set then we can't continue
		if (pfnEditAndContinueCallback == NULL)
		{
			SCE_SLED_LOG(Logging::kError, "[SLED] [LuaPlugin] [EC_R] No function pointers set up to open files - can't perform edit & continue! Script(s) will be out-of-sync!");
			return;
		}

		// Iterate through files trying to reload them
		StringArrayConstIterator iter(m_pEditAndContinue);
		for (; iter(); ++iter)
		{
			SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [EC_R] Reloading script file: %s", iter.get());

			const char *pszBuffer = (*pfnEditAndContinueCallback)(iter.get(), pEditAndContinueUserData);
			if (pszBuffer != NULL)
			{
				const std::size_t iBufLen = std::strlen(pszBuffer);

				const int iErr = ::luaL_loadbuffer(L, pszBuffer, iBufLen, iter.get());
				if (iErr != 0)
				{
					const char *pszError = ::lua_tostring(L, -1);
					ttyNotify(pszError);
					ttyNotify("\n");
					SCE_SLED_LOG(Logging::kError, "[SLED] [LuaPlugin] [EC_R] luaL_loadbuffer error: %s", pszError);
					::lua_pop(L, 1);
				}
				else
				{
					if (::lua_pcall(L, 0, 0, 0) != 0)
					{
						const char *pszError = ::lua_tostring(L, -1);
						ttyNotify(pszError);
						ttyNotify("\n");
						SCE_SLED_LOG(Logging::kError, "[SLED] [LuaPlugin] [EC_R] lua_pcall error: %s", pszError);
						::lua_pop(L, 1);
					}
					else
					{
						ttyNotify("File loaded successfully!\n");
						SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [EC_R] File re-loaded successfully!");
					}
				}
			}
			else
			{
				SCE_SLED_LOG(Logging::kError, "[SLED] [LuaPlugin] [EC_R] File empty or file open failed! %s", iter.get());
			}

			// Free memory or what not if needed
			if (pfnEditAndContinueFinishCallback != NULL)
				(*pfnEditAndContinueFinishCallback)(iter.get(), pEditAndContinueUserData);
		}

		// Clear edit & continue list
		m_pEditAndContinue->clear();
	}

	void LuaPlugin::handleScmpBreakpointDetailsLua(NetworkBufferReader *pReader)
	{
		const Sled::SCMP::Breakpoint::Details bp(pReader);
		//SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [BP] [F: %s] [L: %i] [Cond: %s] [R: %s] [UFE: %s]", bp.relFilePath, (int)bp.line, bp.condition, ((bp.result == 1) ? "true" : "false"), ((bp.useFunctionEnvironment) ? "true" : "false"));

		const bool bStartedEmpty = (m_iNumBreakpoints == 0);

		int32_t iHash = 0;
		SledDebugger::generateHash(bp.relFilePath, bp.line, &iHash);

		Breakpoint hBreakpoint(bp.relFilePath,
							   bp.line,
							   iHash,
							   bp.condition,
							   (bp.result == 1) ? true : false,
							   (bp.useFunctionEnvironment == 1) ? true : false);

		uint16_t iIndex = 0;
		bool bFound = false;
		bool bAddOrRemove = true;
	
		for (uint16_t i = 0; (i < m_iNumBreakpoints) && !bFound; i++)
		{
			if (hBreakpoint == m_pBreakpoints[i])
			{
				bFound = true;
				iIndex = i;
			}
		}

		if (bFound)
		{
			//
			// Remove breakpoint
			//

			/*
			SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [BP] Removing [F: %s] [L: %i] [Cond: %s] [R: %s]", 
				m_pBreakpoints[iIndex].GetFile(),
				m_pBreakpoints[iIndex].GetLine(),
				(m_pBreakpoints[iIndex].HasCondition() ? m_pBreakpoints[iIndex].GetCondition() : "None"),
				(m_pBreakpoints[iIndex].GetResult() ? "true" : "false"));*/

			--m_iNumBreakpoints;

			// Shuffle breakpoints (i is index to remove)				
			for (uint16_t i = iIndex; i < m_iNumBreakpoints; i++)
				m_pBreakpoints[i] = m_pBreakpoints[i + 1];				
		}
		else
		{	
			//
			// Add the breakpoint if we aren't over the max breakpoint limit
			//

			if ((m_iNumBreakpoints + 1) <= m_iMaxBreakpoints)
			{
				/*
				SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] [BP] Adding [F: %s] [L: %i] [Cond: %s] [R: %s]", 
					hBreakpoint.GetFile(),
					hBreakpoint.GetLine(),
					(hBreakpoint.HasCondition() ? hBreakpoint.GetCondition() : "None"),
					(hBreakpoint.GetResult() ? "true" : "false"));*/

				// Add breakpoint
				m_pBreakpoints[m_iNumBreakpoints++] = hBreakpoint;
			}
			else
			{
				// Too many breakpoints - alert user
				SCE_SLED_LOG(Logging::kError, "[SLED] [LuaPlugin] Can't add breakpoint - will be over breakpoint limit of %u!", m_iMaxBreakpoints);
				bAddOrRemove = false;
			}
		}
	
		if (bAddOrRemove)
		{
			// Update Lua hook masks
			const int iProfileMask = m_bProfilerRunning ? LUA_MASKCALL | LUA_MASKRET : 0;
			const bool bNowEmpty = (m_iNumBreakpoints == 0);

			for (uint16_t i = 0; i < m_iNumLuaStates; i++)
			{	
				if (bStartedEmpty)
				{
					// First breakpoint being added; add hooks to all debuggable Lua states
					if (m_pLuaStates[i].isDebugging())
						::lua_sethook(m_pLuaStates[i].luaState, LuaPlugin::hookFunc, LUA_MASKLINE | iProfileMask, 0);
				}
				else if (bNowEmpty && (m_pScriptMan->getDebuggerMode() == DebuggerMode::kNormal))
				{
					// If breakpoints list now empty it means we removed the last one so we should
					// clear any line hooks provided we aren't single stepping or stepping over so
					// debugging mode needs to be normal
					::lua_sethook(m_pLuaStates[i].luaState, 0, 0, 0);

					// Turn profiler back on if it was previously on
					if (iProfileMask)
						::lua_sethook(m_pLuaStates[i].luaState, LuaPlugin::hookFunc, iProfileMask, 0);
				}
			}
		}
	}

	void LuaPlugin::handleScmpVarLookUpCustomLua(SCMP::VarLookUp *pLookUp)
	{
		if ((pLookUp == NULL) || (m_pCurHookLuaState == NULL) || (m_pCurHookLuaDebug == NULL))
			return;

		lua_State *luaState = m_pCurHookLuaState;
		lua_Debug *ar = m_pCurHookLuaDebug;

		//std::printf("----\n");
		const StackReconciler recon(luaState);

		int funcIdx;
		if (!LuaUtils::PushRunningFunctionOnStack(luaState, ar, &funcIdx))
			return;

		//PrintType(::lua_type(luaState, -1));

		int tableIdx = 0;
		switch (pLookUp->variable.what)
		{
		case LuaVariableScope::kGlobal:
			{
				if (!LuaUtils::PushGlobalTableOnStack(luaState, &tableIdx))
					return;
			}
			break;

		case LuaVariableScope::kLocal:
			{
				// TODO:
			}
			break;

		case LuaVariableScope::kUpvalue:
			{
				// TODO:
			}
			break;

		case LuaVariableScope::kEnvironment:
			{
				if (!LuaUtils::PushFunctionEnvironmentTableOnStack(luaState, funcIdx, &tableIdx))
					return;
			}
			break;
		}

		//PrintType(::lua_type(luaState, -1));
		if (::lua_type(luaState, -1) != LUA_TTABLE)
			return;

		::lua_getfield(luaState, tableIdx, pLookUp->variable.name);
		//PrintType(::lua_type(luaState, -1));

		char *nameToUse = pLookUp->variable.name;
		int nameTypeToUse = pLookUp->variable.nameType;

		for (int i = 0; i < pLookUp->variable.numKeyValues; ++i)
		{
			//PrintType(::lua_type(luaState, -1));
			if (::lua_type(luaState, -1) != LUA_TTABLE)
				return;

			const int top = ::lua_gettop(luaState);
			::lua_getfield(luaState, top, pLookUp->variable.hKeyValues[i].name);
			//PrintType(::lua_type(luaState, -1));

			nameToUse = pLookUp->variable.hKeyValues[i].name;
			nameTypeToUse = pLookUp->variable.hKeyValues[i].type;
		}

		if ((::lua_type(luaState, -1) == LUA_TTABLE) && (!pLookUp->variable.bFlag))
		{
			//std::printf("Getting table values...\n");
			getTableValues(luaState, &(pLookUp->variable), pLookUp->variable.what, 0, ::lua_gettop(luaState), 0);
		}
		else
		{
			char szValue[SCMP::Sizes::kVarValueLen];
			const int32_t luaType = lookUpTypeVal(luaState, -1, szValue, SCMP::Sizes::kVarValueLen, false);

			//std::printf("Name: %s, NameType: %i, Value: %s, ValueType: %i\n", nameToUse, nameTypeToUse, szValue, luaType);

			switch (pLookUp->variable.what)
			{
			case LuaVariableScope::kGlobal:
				sendGlobal(&(pLookUp->variable), nameToUse, nameTypeToUse, szValue, luaType);
				break;
			case LuaVariableScope::kLocal:
				// TODO:
				break;
			case LuaVariableScope::kUpvalue:
				// TODO:
				break;
			case LuaVariableScope::kEnvironment:
				sendEnvVar(&(pLookUp->variable), nameToUse, nameTypeToUse, szValue, luaType, 0);
				break;
			}
		}
	}

	void LuaPlugin::handleScmpCallStackLookUpPerformLua(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		SCMP::CallStackLookUpPerform lookup(pReader);

		if (m_pCurHookLuaState)
		{
			lua_State *luaState = m_pCurHookLuaState;
			const StackReconciler recon(luaState);
			const int32_t iLevel = (int32_t)lookup.stackLevel;

			// Find right stack level and get locals & upvalues
			lua_Debug arStack;								
			if (::lua_getstack(luaState, iLevel, &arStack) == 1)
			{
				if (::lua_getinfo(luaState, "Snlf", &arStack) == 1)
				{
					const char *pszLevelSource = trimFileName(arStack.source);

					const std::size_t iFuncTagLen = Sled::SCMP::Base::kStringLen;
					char szFuncName[iFuncTagLen];
					tagFuncForLookUp(szFuncName, iFuncTagLen, arStack.name, pszLevelSource, arStack.linedefined);

					const SCMP::CallStackLookUpBegin scmpCsLBeg(kLuaPluginId);
					m_pScriptMan->send((uint8_t*)&scmpCsLBeg, scmpCsLBeg.length);

					const SCMP::CallStackLookUp scmpCsL(kLuaPluginId,
						szFuncName,
						arStack.linedefined,
						lookup.stackLevel,
						m_pSendBuf);
					m_pScriptMan->send(m_pSendBuf->getData(), m_pSendBuf->getSize());

					// Get locals for this stack level (0) and send to client (if not excluded)
					if ((m_iVarExcludeFlags & VarExcludeFlags::kLocals) != VarExcludeFlags::kLocals)
					{
						const SCMP::LocalVarBegin scmpLoBeg(kLuaPluginId);
						m_pScriptMan->send((uint8_t*)&scmpLoBeg, scmpLoBeg.length);

						getLocals(luaState, &arStack, iLevel);

						const SCMP::LocalVarEnd scmpLoEnd(kLuaPluginId);
						m_pScriptMan->send((uint8_t*)&scmpLoEnd, scmpLoEnd.length);
					}

					// Get upvalues for this stack level (0) and send to client (if not excluded)
					if ((m_iVarExcludeFlags & VarExcludeFlags::kUpvalues) != VarExcludeFlags::kUpvalues)
					{
						const SCMP::UpvalueVarBegin scmpUpBeg(kLuaPluginId);
						m_pScriptMan->send((uint8_t*)&scmpUpBeg, scmpUpBeg.length);

						getUpvalues(luaState, -1, iLevel);

						const SCMP::UpvalueVarEnd scmpUpEnd(kLuaPluginId);
						m_pScriptMan->send((uint8_t*)&scmpUpEnd, scmpUpEnd.length);
					}

					// Get environment for this function and send to client if not excluded
					if ((m_iVarExcludeFlags & VarExcludeFlags::kEnvironment) != VarExcludeFlags::kEnvironment)
					{
						const SCMP::EnvVarBegin scmpEnvBeg(kLuaPluginId);
						m_pScriptMan->send((uint8_t*)&scmpEnvBeg, scmpEnvBeg.length);

						getEnvironment(luaState, ::lua_gettop(luaState), iLevel);

						const SCMP::EnvVarEnd scmpEnvEnd(kLuaPluginId);
						m_pScriptMan->send((uint8_t*)&scmpEnvEnd, scmpEnvEnd.length);
					}

					const SCMP::CallStackLookUpEnd scmpCsLEnd(kLuaPluginId);
					m_pScriptMan->send((uint8_t*)&scmpCsLEnd, scmpCsLEnd.length);
				}
			}
		}
	}

	void LuaPlugin::handleScmpProfilerToggleLua(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);

		const int iProfileMask = m_bProfilerRunning ? 0 : LUA_MASKCALL | LUA_MASKRET;
		const int iBreakpointMask = (m_iNumBreakpoints == 0) ? 0 : LUA_MASKLINE;

		m_bProfilerRunning = !m_bProfilerRunning;
		m_pProfileStack->clear();

		for (uint16_t i = 0; i < m_iNumLuaStates; i++)
		{
			// Skip states not being debugged
			if (!m_pLuaStates[i].isDebugging())
				continue;

			// Remove any hooks
			::lua_sethook(m_pLuaStates[i].luaState, NULL, 0, 0);

			// Re-add any hooks
			if (iProfileMask || iBreakpointMask)
				::lua_sethook(m_pLuaStates[i].luaState, LuaPlugin::hookFunc, iProfileMask | iBreakpointMask, 0);
		}
	}

	void LuaPlugin::handleScmpDevCmdLua(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		const Sled::SCMP::DevCmd cmd(pReader);

		const std::size_t cmdLen = std::strlen(cmd.command);
		if (cmdLen > 0)
		{
			const std::size_t baseLen = Sled::SCMP::Base::kStringLen;
			const char chFullAccesIndicator = ':';
			const bool bFullAccess = cmd.command[0] != chFullAccesIndicator;

			if (m_pCurHookLuaState)
			{
				lua_State *L = m_pCurHookLuaState;
				const StackReconciler recon(L);

				// Save off beginning stack size
				const int iCount1 = ::lua_gettop(L);	

				if (bFullAccess)
				{
					// Load the command as a function
					::luaL_loadbuffer(L, cmd.command, cmdLen, "libsledluaplugin_devcmd_func");

					// Run the function to execute user code
					if (::lua_pcall(L, 0, LUA_MULTRET, 0) != 0)
					{
						// Report error
						char szError[baseLen];
						Utilities::copyString(szError, baseLen, ::lua_tostring(L, -1));
						Utilities::appendString(szError, baseLen, "\n");
						ttyNotify(szError);
					}
				}
				else
				{
					// Remove full access indicator
					const int iCmdOffset = 1;

					// Fill in activation record
					lua_Debug ar;
					::lua_getstack(L, 0, &ar);

					const int kFuncLen = 2048;

					// Generate a function						
					char szFunc[kFuncLen];
					Utilities::copyString(szFunc, kFuncLen, "function libsledluaplugin:devcmd_func(");

					int iLocal = 1;
					int iUpval = 1;
					int iTemps = 0;
					int iAbsFuncIndex = 0;

					bool bFirst = true;

					// Get non-temp local variables for the function header
					// and push them on the stack for the function call later
					const char *pszLocal = ::lua_getlocal(L, &ar, iLocal);
					while (pszLocal)
					{
						// Variables starting with ( are temporary
						if (pszLocal[0] != '(')
						{
							// Build function header
							if (!bFirst)
								Utilities::appendString(szFunc, kFuncLen, ", ");

							Utilities::appendString(szFunc, kFuncLen, pszLocal);
						}
						else
						{
							// Pop temp var
							::lua_pop(L, 1);
							iTemps++;
						}										

						bFirst = false;

						// Get next
						pszLocal = ::lua_getlocal(L, &ar, ++iLocal);
					}

					// Adjust value
					iLocal--;

					// Get running function on stack
					::lua_getinfo(L, "f", &ar);
					// Get absolute position of running function
					iAbsFuncIndex = ::lua_gettop(L);

					// Get non-temp upvalue variables for the function header
					// and push them on the stack for the function call later
					const char *pszUpval = ::lua_getupvalue(L, iAbsFuncIndex, iUpval);
					while (pszUpval)
					{
						if (pszUpval[0] != '(')
						{
							// Build function header
							if (!bFirst)
								Utilities::appendString(szFunc, kFuncLen, ", ");

							Utilities::appendString(szFunc, kFuncLen, pszUpval);
						}
						else
						{
							// Pop temp var
							::lua_pop(L, 1);
							iTemps++;
						}

						bFirst = false;

						// Get next
						pszUpval = ::lua_getupvalue(L, iAbsFuncIndex, ++iUpval);
					}

					// Adjust value
					iUpval--;

					// Finish generating function
					Utilities::appendString(szFunc, kFuncLen, ")\n");
					Utilities::appendString(szFunc, kFuncLen, cmd.command + iCmdOffset);
					Utilities::appendString(szFunc, kFuncLen, "\nend");

					//SCE_SLED_LOG(Logging::kInfo, "[SLED] [LuaPlugin] DevCmd generated function:\n%s\n", szFunc);

					// Remove function pushed on for upvalue retrieval
					::lua_remove(L, iAbsFuncIndex);

					// Load the function chunk to an actual Lua function
					::luaL_loadbuffer(L, szFunc, std::strlen(szFunc), "libsledluaplugin_devcmd_func");

					// Exec the function so the var name and signature get registered
					::lua_pcall(L, 0, 0, 0);

					// Get the libsledluaplugin table on top of stack
					::lua_pushstring(L, "libsledluaplugin");
					::lua_gettable(L, LUA_GLOBALSINDEX);

					// Get the libsledluaplugin.devcmd_func function on top of stack
					::lua_getfield(L, -1, "devcmd_func");

					// Relocate libsledluaplugin.devcmd_func function "below" all of the pushed on locals/upvalues
					::lua_insert(L, iCount1 + 1);

					// Move libsledluaplugin table so that it's the first function arg (ie. "self")
					::lua_insert(L, iCount1 + 2);

					// Run the function to execute the wrapper user code
					if (::lua_pcall(L, (iLocal + iUpval - iTemps) + 1, LUA_MULTRET, 0) != 0)
					{
						// Report error
						char szError[baseLen];
						Utilities::copyString(szError, baseLen, ::lua_tostring(L, -1));
						Utilities::appendString(szError, baseLen, "\n");
						ttyNotify(szError);
					}
				}
			}
		}
	}

	void LuaPlugin::handleScmpLuaStateToggleLua(NetworkBufferReader *pReader)
	{
		SCE_SLED_ASSERT(pReader != NULL);
		const SCMP::LuaStateToggle stateToggle(pReader);

		for (uint16_t i = 0; i < m_iNumLuaStates; i++)
		{
			char szTemp[SCMP::Sizes::kPtrLen];
			StringUtilities::copyString(szTemp, SCMP::Sizes::kPtrLen, "%p", m_pLuaStates[i].luaState);

			if (Utilities::areStringsEqual(szTemp, stateToggle.address))
			{
				if (m_pLuaStates[i].isDebugging())
				{
					// Remove all hooks
					::lua_sethook(m_pLuaStates[i].luaState, NULL, 0, 0);
				}
				else
				{
					// See if we need to set any hooks on this re-activated state
					const int iProfileMask = m_bProfilerRunning ? LUA_MASKCALL | LUA_MASKRET : 0;
					const int iBreakpointMask = (m_iNumBreakpoints == 0) ? 0 : LUA_MASKLINE;

					// Add hooks back if they're needed
					if (iProfileMask | iBreakpointMask)
						::lua_sethook(m_pLuaStates[i].luaState, LuaPlugin::hookFunc, iProfileMask | iBreakpointMask, 0);
				}

				// Change debugging state
				m_pLuaStates[i].setDebugging(!m_pLuaStates[i].isDebugging());
				break;
			}
		}
	}
}}
