/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "scoped_network.h"

#include "../sledcore/socket.h"
#include "../sleddebugger/assert.h"

namespace sce { namespace Sled
{
	ScopedNetwork::ScopedNetwork()
	{
		SCE_SLED_VERIFY(sceSledPlatformNetworkInitialize() == SCE_SLEDPLATFORM_NETWORK_ERROR_NONE);
	}

	ScopedNetwork::~ScopedNetwork()
	{
		SCE_SLED_VERIFY(sceSledPlatformNetworkTerminate() == SCE_SLEDPLATFORM_NETWORK_ERROR_NONE);
	}

	bool ScopedNetwork::IsValid()
	{
		return sceSledPlatformNetworkGetInitialized();
	}
}}
