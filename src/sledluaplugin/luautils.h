/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDLUAPLUGIN_LUAUTILS_H__
#define __SCE_LIBSLEDLUAPLUGIN_LUAUTILS_H__

#include "../sledcore/base_types.h"
#include <cstddef>

#include "../sleddebugger/common.h"

// Forward declarations
struct lua_State;
struct lua_Debug;

namespace sce { namespace Sled
{
	// Forward declarations
	class ISequentialAllocator;
	class StringArray;

	// Forward declarations
	struct LuaPluginConfig;
	class VarFilterNameContainer;
	struct VarFilterNameContainerConfig;

	struct SCE_SLED_LINKAGE LuaStateParams
	{
		LuaStateParams() : luaState(0), debugging(1) {}
		LuaStateParams(lua_State *state, bool bDebugging = true);
		LuaStateParams(const LuaStateParams& rhs) { init(rhs); }
		LuaStateParams& operator=(const LuaStateParams& rhs) { init(rhs); return *this; }
		bool operator==(const LuaStateParams& rhs) const { return luaState == rhs.luaState; }

		inline bool isDebugging() const { return debugging != 0; }
		inline void setDebugging(bool value) { debugging = value ? 1 : 0; }

	private:
		void init(const LuaStateParams& rhs);
	public:

		lua_State*	luaState;
		uint8_t		debugging;
	};

	struct SCE_SLED_LINKAGE MemTraceParams
	{
		MemTraceParams() : what(' '), oldPtr(0), newPtr(0), oldSize(0), newSize(0) {}
		MemTraceParams(char chWhat, void *pOldPtr, void *pNewPtr, std::size_t iOldSize, std::size_t iNewSize)
			: what(chWhat), oldPtr(pOldPtr), newPtr(pNewPtr), oldSize(iOldSize), newSize(iNewSize) {}
		MemTraceParams(const MemTraceParams& rhs) { init(rhs); }
		MemTraceParams& operator=(const MemTraceParams& rhs) { init(rhs); return *this; }

	private:
		void init(const MemTraceParams& rhs);
	public:

		char			what;
		void*			oldPtr;
		void*			newPtr;
		std::size_t		oldSize;
		std::size_t		newSize;
	};

	class SCE_SLED_LINKAGE Breakpoint
	{
		static const uint16_t kStringLen = 256;
	public:
		Breakpoint() { setup(); }
		Breakpoint(const char *pszFile, const int32_t& iLine, const int32_t& iHash) { setup(pszFile, 0, iLine, iHash); }
		Breakpoint(const char *pszFile, const int32_t& iLine, const int32_t& iHash, const char *pszCondition) { setup(pszFile, pszCondition, iLine, iHash); }
		Breakpoint(const char *pszFile, const int32_t& iLine, const int32_t& iHash, const char *pszCondition, bool bResult) { setup(pszFile, pszCondition, iLine, iHash, bResult); }
		Breakpoint(const char *pszFile, const int32_t& iLine, const int32_t& iHash, const char *pszCondition, bool bResult, bool bUseFunctionEnvironment) { setup(pszFile, pszCondition, iLine, iHash, bResult, bUseFunctionEnvironment); }
		Breakpoint(const Breakpoint& rhs) { init(rhs); }
		Breakpoint& operator=(const Breakpoint& rhs) { init(rhs); return *this; }
	public:
		bool operator<(const Breakpoint& rhs) const { return m_iHash < rhs.m_iHash; }
		bool operator>(const Breakpoint& rhs) const { return m_iHash > rhs.m_iHash; }
		bool operator==(const Breakpoint& rhs) const;
	public:
		inline int32_t getHash() const { return m_iHash; }
		inline int32_t getLine() const { return m_iLine; }
		inline const char *getFile() const { return m_szFile; }
		inline const char *getCondition() const { return m_szCondition; }
		inline bool hasCondition() const { return m_szCondition[0] != '\0'; }
		void setCondition(const char *pszCondition);
		inline bool getResult() const { return m_result != 0; }
		inline void setResult(bool value) { m_result = value ? 1 : 0; }
		inline bool useFunctionEnvironment() const { return m_useFunctionEnvironment == 1; }
	private:
		void init(const Breakpoint& rhs);
		void setup(const char *pszFile = 0, const char *pszCondition = 0, const int32_t& iLine = 0, const int32_t& iHash = 0, const bool& bResult = true, const bool& bUseFunctionEnvironment = false);
	private:
		char	m_szFile[kStringLen];
		char	m_szCondition[kStringLen];
		int32_t	m_iLine;
		int32_t	m_iHash;
		uint8_t	m_result;
		uint8_t m_useFunctionEnvironment;
	};

	class SCE_SLED_LINKAGE StackReconciler
	{
	public:
		StackReconciler(lua_State *luaState);
		~StackReconciler();
	private:
		StackReconciler(const StackReconciler&);
		StackReconciler& operator=(const StackReconciler&);
	private:
		lua_State *m_luaState;
		const int m_iCount1;
	};

	namespace StringUtilities { SCE_SLED_LINKAGE void copyString(char *pszCopyTo, std::size_t len, const char *pszCopyFrom, ...); }

	namespace LuaUtils
	{
		// these functions might invoke metamethods
		SCE_SLED_LINKAGE bool PushRunningFunctionOnStack(lua_State *luaState, lua_Debug *ar, int *absFuncIdx);
		SCE_SLED_LINKAGE bool PushGlobalTableOnStack(lua_State *luaState, int *absGlobalTableIdx);
		SCE_SLED_LINKAGE bool PushFunctionEnvironmentTableOnStack(lua_State *luaState, int funcIdx, int *absEnvTableIdx);
		// TODO: raw versions if needed?
	}
}}

#endif // __SCE_LIBSLEDLUAPLUGIN_LUAUTILS_H__
