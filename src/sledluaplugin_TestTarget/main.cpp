/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include <sce_sled/sled.h>

#include "../sleddebugger/assert.h"
#include "../sleddebugger/utilities.h"

#include "../sledcore/sleep.h"
#include "../sledcore/thread.h"

#include "../sledluaplugin_SharedSampleCode/fileutilities.h"
#include "../sledluaplugin_SharedSampleCode/luastuff.h"
#include "../sledluaplugin_SharedSampleCode/ScopedSledDebugger.h"
#include "../sledluaplugin_SharedSampleCode/input.h"
#include "../sleddebugger_unittests/scoped_network.h"

#if SCE_SLEDTARGET_OS_WINDOWS
	#ifndef _WINDOWS_
		#include <windows.h>
	#endif // _WINDOWS_
#else
	#error Not Supported
#endif

#include <cstdio>
#include <cstdlib>

#if WWS_LUA_VER >= 520
extern "C"
{
	#include <wws_lua/lua-5.2.3/src/lua.h>
	#include <wws_lua/lua-5.2.3/src/lualib.h>
	#include <wws_lua/lua-5.2.3/src/lauxlib.h>
}
#elif WWS_LUA_VER >= 510
extern "C"
{
	#include <wws_lua/lua-5.1.4/src/lua.h>
	#include <wws_lua/lua-5.1.4/src/lualib.h>
	#include <wws_lua/lua-5.1.4/src/lauxlib.h>
}
#else
	#error Unknown Lua version!
#endif

using namespace sce::Sled;

namespace Thread
{
	int Priority()
	{
		return 0;
	}

	int InputStackSize()
	{
		return 8 * 1024;
	}

	int ThreadStackSize()
	{
		return InputStackSize() + (64 * 1024);
	}
}

namespace
{
	int GetNumberOfTcpDebuggers()
	{
		const int tcpDebuggers = 2;
		return tcpDebuggers;
	}

	class ScopedFreeScopedSledDebuggers
	{
	public:
		ScopedFreeScopedSledDebuggers(Examples::ScopedSledDebugger *debuggers) : m_debuggers(debuggers) {}
		~ScopedFreeScopedSledDebuggers() { if (m_debuggers != NULL) delete [] m_debuggers; m_debuggers = NULL; }
	private:
		Examples::ScopedSledDebugger *m_debuggers;
	};

	class ScopedThread : public sce::SledPlatform::Thread
	{
	public:
		virtual ~ScopedThread() { Join(); }
	};

	class ScopedFreeScopedThreads
	{
	public:
		ScopedFreeScopedThreads(ScopedThread *threads) : m_threads(threads) {}
		~ScopedFreeScopedThreads() { if (m_threads != NULL) delete [] m_threads; m_threads = NULL; }
	private:
		ScopedThread *m_threads;
	};

	class ScopedFreeScopedSledDebuggerThreadParams
	{
	public:
		ScopedFreeScopedSledDebuggerThreadParams(Examples::ScopedSledDebuggerThreadParams *params) : m_params(params) {}
		~ScopedFreeScopedSledDebuggerThreadParams() { if (m_params != NULL) delete [] m_params; m_params = NULL; }
	private:
		Examples::ScopedSledDebuggerThreadParams *m_params;
	};
}

int main()
{
	std::printf("Main: TestTarget example started!\n");
	std::printf("Main: Bringing up network subsystem\n");
	ScopedNetwork sn;

	Examples::FileUtil::ValidateWorkingDirectory();

	std::printf("Misc:\n");
	std::printf("\tInput thread stack size: %i bytes\n", Thread::InputStackSize());
	std::printf("\tRunner thread stack size: %i bytes\n", Thread::ThreadStackSize());

	// Set up file callbacks for SledDebugger to use
	Utilities::openFileCallback() = Examples::FileUtil::OpenFile;
	Utilities::openFileFinishCallback() = Examples::FileUtil::OpenFileFinish;	

	// Relative path to Lua script file for this example
#if WWS_LUA_VER >= 520
	const char* pszRelFilePath1 = "libsce_testtarget_script3.lua";
	const char* pszRelFilePath2 = "libsce_testtarget_script4.lua";
#elif WWS_LUA_VER >= 510
	const char* pszRelFilePath1 = "libsce_testtarget_script1.lua";
	const char* pszRelFilePath2 = "libsce_testtarget_script2.lua";
#else
#error Unknown Lua version!
#endif

	// Set up containers to hold file information
	Examples::FileUtil::FileLoadItems hFileLoadItems1(pszRelFilePath1);
	Examples::FileUtil::FileLoadItems hFileLoadItems2(pszRelFilePath2);

	// Open each file (libsce_testtarget_script1.lua & libsce_testtarget_script2.lua)
	Examples::FileUtil::FileOpen(hFileLoadItems1/*, 1024 * 5*/);
	Examples::FileUtil::FileOpen(hFileLoadItems2/*, 1024 * 2*/);

	char name[Examples::ScopedSledDebugger::kNameLen];

	//
	// Create TCP SledDebugger's each with a LuaPlugin running two lua_State*'s
	//

	const int numberOfTcpDebuggers = GetNumberOfTcpDebuggers();	
	Examples::ScopedSledDebugger *debuggersTcp =
		numberOfTcpDebuggers > 0
			? new Examples::ScopedSledDebugger[numberOfTcpDebuggers]
			: NULL;

	const ScopedFreeScopedSledDebuggers sfssd1(debuggersTcp);

	for (int i = 0; i < numberOfTcpDebuggers; ++i)
	{
		sprintf(name, "Debugger %i", i);

		if (debuggersTcp[i].CreateTcp(11111 + (uint16_t)i, false, &hFileLoadItems1, &hFileLoadItems2, name) != 0)
		{
			std::printf("Failed to create SledDebugger %i\n", debuggersTcp[i].GetInstanceCount());
			return -1;
		}
	}

	std::printf("Main: Everything loaded successfully! Starting threads...\n");	

	// Let certain keyboard/controller input signal to quit the program
	Examples::InputParams inputParams;
	ScopedThread inputThread;
	inputThread.SetAttributes(Thread::InputStackSize(), Thread::Priority(), "input");
	inputThread.Start(Examples::InputThread, &inputParams);	

	// Run each debugger on its own thread so that they can be independent
	// of each other when one hits a breakpoint and blocks

	{
		const int num = numberOfTcpDebuggers;

		Examples::ScopedSledDebuggerThreadParams *params =
			new Examples::ScopedSledDebuggerThreadParams[num];
		const ScopedFreeScopedSledDebuggerThreadParams sfssdtp(params);

		ScopedThread *threads = new ScopedThread[num];
		const ScopedFreeScopedThreads sfst(threads);

		for (int i = 0; i < num; ++i)
		{
			params[i].pScopedSledDebugger = &(debuggersTcp[i]);

			params[i].pInputParams = &inputParams;

			threads[i].SetAttributes(Thread::ThreadStackSize(), Thread::Priority(), params[i].pScopedSledDebugger->GetName());
			threads[i].Start(Examples::ScopedSledDebuggerThreadRun, &(params[i]));
			std::printf("Main: thread \"%s\" started\n", params[i].pScopedSledDebugger->GetName());
		}

		while (!inputParams.QuitPushed)
			sceSledPlatformThreadSleepMilliseconds(5);
	}

	std::printf("Main: Program finished!\n");


	return 0;
}
