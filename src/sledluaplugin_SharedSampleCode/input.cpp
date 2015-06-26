/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "input.h"
#include "../sleddebugger/assert.h"

#if SCE_SLEDTARGET_OS_WINDOWS
	#include <iostream>
#endif

#include <cstdio>
#include <cstring>

namespace Examples
{
	void InputThread(void *pVoid)
	{
		SCE_SLED_ASSERT(pVoid != NULL);

		InputParams *pParams = static_cast<InputParams*>(pVoid);

#if SCE_SLEDTARGET_OS_WINDOWS
		std::printf("Main: Press 'q' then 'enter' at any time to exit\n");
		char ch = ' ';
		while (ch != 'q')
		{
			std::cin >> ch;
		}

		pParams->QuitPushed = true;
		std::printf("Main: Quit button detected!\n");
#endif
	}
}
