/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "params.h"
#include "utilities.h"

namespace sce { namespace Sled
{
	NetworkParams::NetworkParams()
		: protocol(Protocol::kTcp)
		, port(11111)
		, blockUntilConnect(false)
	{
	}

	void NetworkParams::init(const NetworkParams& rhs)
	{
		protocol = rhs.protocol;
		port = rhs.port;
		blockUntilConnect = rhs.blockUntilConnect;
	}

	void NetworkParams::setup(Protocol::Enum kProtocol, uint16_t iPort, bool bBlockUntilConnect)
	{
		protocol = kProtocol;
		port = iPort;
		blockUntilConnect = bBlockUntilConnect;
	}

	BreakpointParams::BreakpointParams()
		: pluginId(0)
		, lineNumber(0)
	{
		relFilePath[0] = '\0';
	}

	BreakpointParams::BreakpointParams(uint16_t iPluginId, uint32_t iLineNumber, const char *pszRelFilePath)
		: pluginId(iPluginId)
		, lineNumber(iLineNumber)
	{
		Utilities::copyString(relFilePath, kRelFilePathLen, pszRelFilePath);
	}

	void BreakpointParams::init(const BreakpointParams& rhs)
	{
		pluginId = rhs.pluginId;
		lineNumber = rhs.lineNumber;
		Utilities::copyString(relFilePath, kRelFilePathLen, rhs.relFilePath);
	}
}}
