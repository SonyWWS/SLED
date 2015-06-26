/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_INPUT_EXAMPLE_H__
#define __SCE_INPUT_EXAMPLE_H__

namespace Examples
{
	struct InputParams
	{
		volatile bool QuitPushed;

		InputParams() : QuitPushed(false) {}
	};

	void InputThread(void *pVoid);
}

#endif // __SCE_SCEAALLOCATOR_EXAMPLE_H__
