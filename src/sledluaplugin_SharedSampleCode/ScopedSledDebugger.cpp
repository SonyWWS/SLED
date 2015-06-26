/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "ScopedSledDebugger.h"
#include <sce_sled/sled.h>

#include "../sleddebugger/assert.h"
#include "../sledluaplugin/Extras/extras.h"

#include "../sledcore/sleep.h"

#include <cstdio>
#include <cstring>

#include "fileutilities.h"
#include "luastuff.h"
#include "input.h"

namespace Examples
{
	using namespace sce::Sled;

	// Initialize static member variable
	int ScopedSledDebugger::s_iInstanceCount = 1;

	int32_t ScopedSledDebugger::CreateTcp(uint16_t port, bool bBlockUntilConnect, void *pFile1LoadItems, void *pFile2LoadItems, const char *pszName)
	{
		return Create(Protocol::kTcp, port, bBlockUntilConnect, pFile1LoadItems, pFile2LoadItems, pszName);
	}

	void ScopedSledDebugger::Update()
	{
		// Update networking
		const int32_t iRetval = debuggerUpdate(m_debugger);
		if (iRetval < 0)
			std::printf("ScopedSledDebugger %i: Error in Update(): %i\n", m_iInstanceCount, iRetval);
	
		const int defaultErrorHandlerStackIndex = 0;	

		// Run function supplying an error handler
		{
			const LuaStuff::StackReconciler recon(m_pLuaState1);

			int idx = defaultErrorHandlerStackIndex;
			SCE_SLED_VERIFY(luaPluginGetErrorHandlerAbsStackIdx(m_plugin, m_pLuaState1, &idx) == SCE_SLED_ERROR_OK);
		
			LuaStuff::RunFunctionWith3Args(m_pLuaState1, "times", 3, 5, 7, idx);
		}

		// Run function supplying an error handler
		{
			const LuaStuff::StackReconciler recon(m_pLuaState2);

			int idx = defaultErrorHandlerStackIndex;
			SCE_SLED_VERIFY(luaPluginGetErrorHandlerAbsStackIdx(m_plugin, m_pLuaState2, &idx) == SCE_SLED_ERROR_OK);

			LuaStuff::RunFunctionWith3Args(m_pLuaState2, "times2", 1, 3, 5, idx);
		}
	}

	void ScopedSledDebuggerThreadRun(void *pVoid)
	{
		SCE_SLED_ASSERT(pVoid != NULL);
		ScopedSledDebuggerThreadParams *pParams = static_cast<ScopedSledDebuggerThreadParams*>(pVoid);

		while (!(pParams->pInputParams->QuitPushed))
		{
			pParams->pScopedSledDebugger->Update();
			sceSledPlatformThreadSleepMilliseconds(2);
		}
	}
}
