/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_SLEDDEBUGGER_EXAMPLE_H__
#define __SCE_SLEDDEBUGGER_EXAMPLE_H__

#include <sce_sled/sled.h>
#include "../sledcore/target_macros.h"

namespace sce { namespace Sled 
{
	class SledDebugger;
	class LuaPlugin;
}}

struct lua_State;

namespace Examples
{
	struct InputParams;

	namespace FileUtil { struct FileBuffer; }

	using namespace FileUtil;

	using namespace sce::Sled;

	class ScopedSledDebugger
	{
	public:
		static const int kNameLen = 32;
	public:
		ScopedSledDebugger()
			: m_debugger(0)
			, m_plugin(0)
			, m_pMemory(0)
			, m_pLuaMemory(0)
			, m_pLuaState1(0)
			, m_pLuaState2(0)
			, m_iInstanceCount(s_iInstanceCount)
			, m_pFileBuffer(0)
		{
			++s_iInstanceCount;
		}

		~ScopedSledDebugger() { Close(); }
	private:
		ScopedSledDebugger(const ScopedSledDebugger&);
		ScopedSledDebugger& operator=(const ScopedSledDebugger&);
	public:
		int32_t CreateTcp(uint16_t port,
						  bool bBlockUntilConnect,
						  void *pFile1LoadItems,
						  void *pFile2LoadItems,
						  const char *pszName = "TCP Debugger");
	private:
		int32_t Create(Protocol::Enum protocol,
					   uint16_t port,
					   bool bBlockUntilConnect,
					   void *pFile1LoadItems,
					   void *pFile2LoadItems,
					   const char *pszName);
	public:
		void Update();
		void Close();
	public:
		int GetInstanceCount() const { return m_iInstanceCount; }
		const char *GetName() const { return m_name; }
	private:
		int32_t AddLuaplugin(uint32_t maxSendBufferSize,
							 uint16_t maxLuaStates,
							 uint16_t maxLuaStateNameLen,
							 uint32_t maxMemTraces,
							 uint16_t maxBreakpoints,
							 uint16_t maxEditAndContinues,
							 uint16_t maxEditAndContinueEntryLen,
							 uint16_t maxNumVarFilters,
							 uint16_t maxVarFilterPatternLen,
							 uint16_t maxPatternsPerVarFilter,
							 uint16_t maxProfileFunctions,
							 uint16_t maxProfileCallStackDepth,
							 int32_t numPathChopChars,
							 ChopCharsCallback pfnChopCharsCallback,
							 EditAndContinueCallback pfnEditAndContinueCallback,
							 EditAndContinueFinishCallback pfnEditAndContinueFinishCallback,
							 void *pEditAndContinueUserData);
	private:
		SledDebugger*	m_debugger;
		LuaPlugin*		m_plugin;
	private:
		char*			m_pMemory;
		char*			m_pLuaMemory;
		lua_State*		m_pLuaState1;
		lua_State*		m_pLuaState2;
		const int		m_iInstanceCount;
		FileBuffer*		m_pFileBuffer;
		char			m_name[kNameLen];	
	private:
		static int s_iInstanceCount;
	};

	struct ScopedSledDebuggerThreadParams
	{
		ScopedSledDebugger *pScopedSledDebugger;
		InputParams *pInputParams;
	};

	void ScopedSledDebuggerThreadRun(void *pVoid);
}

#endif // __SCE_SLEDDEBUGGER_EXAMPLE_H__
