/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "sleddebugger.h"

#include "sleddebugger_class.h"
#include "assert.h"
#include "buffer.h"
#include "debug.h"
#include "network.h"
#include "plugin.h"
#include "scmp.h"
#include "sequentialallocator.h"
#include "stringarray.h"
#include "utilities.h"

#include "../sledcore/mutex.h"

#include <cstdio>
#include <new>
#include <cstring>	
#include <ctype.h>

// Temporary network buffer receive size
#define SCE_LIBSLEDDEBUGGER_NET_BUFRECV 512
#define SCE_LIBSLEDDEBUGGER_NET_BUFSTATIC 2048

// "positive" error codes. These don't need to be exposed in errorcodes.h
#define SCE_SLED_ERROR_NONE			0
#define SCE_SLED_ERROR_CONNECTED	1
#define SCE_SLED_ERROR_READY		2
#define SCE_SLED_ERROR_MESSAGE		3
#define SCE_SLED_ERROR_BP_BEG		4
#define SCE_SLED_ERROR_BP_SYNC		5
#define SCE_SLED_ERROR_BP_END		6
#define SCE_SLED_ERROR_BP_CONT		7

using namespace sce::Sled;	

void SledDebuggerConfig::init(const SledDebuggerConfig& rhs)
{
	maxPlugins = rhs.maxPlugins;
	maxScriptCacheEntries = rhs.maxScriptCacheEntries;
	maxScriptCacheEntryLen = rhs.maxScriptCacheEntryLen;
	maxRecvBufferSize = rhs.maxRecvBufferSize;
	maxSendBufferSize = rhs.maxSendBufferSize;
	net.setup(rhs.net.protocol, rhs.net.port, rhs.net.blockUntilConnect);
}

namespace
{
	struct DebuggerMemorySeats
	{
		void *m_this;
		void *m_plugins;
		void *m_scriptCache;
		void *m_recvBuf;
		void *m_sendBuf;
		void *m_network;
		void *m_mutex;

		void Allocate(const SledDebuggerConfig& debuggerConfig, ISequentialAllocator *pAllocator)
		{
			// For SledDebugger
			m_this = pAllocator->allocate(sizeof(SledDebugger), __alignof(SledDebugger));

			// For m_ppPlugins
			m_plugins = pAllocator->allocate(sizeof(SledDebuggerPlugin*) * debuggerConfig.maxPlugins, __alignof(SledDebuggerPlugin*));

			// For m_pScriptCache
			{
				StringArrayConfig config;
				config.allowDuplicates = false;
				config.maxEntries = debuggerConfig.maxScriptCacheEntries;
				config.maxEntryLen = debuggerConfig.maxScriptCacheEntryLen;
				StringArray::requiredMemoryHelper(&config, pAllocator, &m_scriptCache);
			}

			// For m_pRecvBuf
			{
				NetworkBufferConfig config;
				config.maxSize = debuggerConfig.maxRecvBufferSize;
				NetworkBuffer::requiredMemoryHelper(config, pAllocator, &m_recvBuf);
			}

			// For m_pSendBuf
			{
				NetworkBufferConfig config;
				config.maxSize = debuggerConfig.maxSendBufferSize;
				NetworkBuffer::requiredMemoryHelper(config, pAllocator, &m_sendBuf);
			}

			Network::requiredMemoryHelper(pAllocator, &m_network);

			// For m_pMutex
			m_mutex = pAllocator->allocate(sizeof(SceSledPlatformMutex), __alignof(SceSledPlatformMutex));
		}
	};
}

namespace
{	
	inline int32_t ValidateConfig(const SledDebuggerConfig& config)
	{
		if ((config.maxPlugins == 0) ||
			(config.maxRecvBufferSize == 0) ||
			(config.maxSendBufferSize == 0))
			return SCE_SLED_ERROR_INVALIDCONFIGURATION;

		// Check for valid network protocol
		const bool bValidProtocol =
			(config.net.protocol == Protocol::kTcp);

		if (!bValidProtocol)
			return SCE_SLED_ERROR_INVALIDPROTOCOL;

		return SCE_SLED_ERROR_OK;
	}
}

int32_t SledDebugger::requiredMemory(const SledDebuggerConfig& debuggerConfig, std::size_t *iRequiredMemory)
{
	if (iRequiredMemory == NULL)
		return SCE_SLED_ERROR_NULLPARAMETER;

	const int32_t iConfigError = ValidateConfig(debuggerConfig);
	if (iConfigError != 0)
		return iConfigError;

	SequentialAllocatorCalculator allocator;

	DebuggerMemorySeats seats;
	seats.Allocate(debuggerConfig, &allocator);

	*iRequiredMemory = allocator.bytesAllocated();
	return SCE_SLED_ERROR_OK;
}

int32_t SledDebugger::create(const SledDebuggerConfig& debuggerConfig, void *pLocation, SledDebugger **ppDebugger)
{
	if (pLocation == NULL)
		return SCE_SLED_ERROR_NULLPARAMETER;
	
	if (ppDebugger == NULL)
		return SCE_SLED_ERROR_NULLPARAMETER;

	std::size_t iMemSize = 0;
	const int32_t iConfigError = requiredMemory(debuggerConfig, &iMemSize);
	if (iConfigError != 0)
		return iConfigError;
	
	SequentialAllocator allocator(pLocation, iMemSize);

	DebuggerMemorySeats seats;
	seats.Allocate(debuggerConfig, &allocator);

	SCE_SLED_ASSERT(seats.m_this != NULL);
	SCE_SLED_ASSERT(seats.m_plugins != NULL);
	SCE_SLED_ASSERT(seats.m_scriptCache != NULL);
	SCE_SLED_ASSERT(seats.m_recvBuf != NULL);
	SCE_SLED_ASSERT(seats.m_sendBuf != NULL);
	SCE_SLED_ASSERT(seats.m_network != NULL);
	SCE_SLED_ASSERT(seats.m_mutex != NULL);

	// Everything looks good so create debugger instance
	*ppDebugger = new (seats.m_this) SledDebugger(debuggerConfig, &seats);
	
	return SCE_SLED_ERROR_OK;
}

int32_t SledDebugger::close(SledDebugger *pDebugger)
{
	if (pDebugger == NULL)
		return SCE_SLED_ERROR_NULLPARAMETER;

	pDebugger->shutdown();
	pDebugger->~SledDebugger();

	return SCE_SLED_ERROR_OK;
}	

SledDebugger::SledDebugger(const SledDebuggerConfig& debuggerConfig, const void *pDebuggerSeats)
	: m_hDebuggerMode(DebuggerMode::kNormal)
	, m_bUpdateGuard(false)
	, m_iMaxPlugins(debuggerConfig.maxPlugins)
	, m_iPluginCount(0)
	, m_hConnectionState(kDisconnected)
{
	SCE_SLED_ASSERT(pDebuggerSeats != NULL);

	const DebuggerMemorySeats *pSeats = static_cast<const DebuggerMemorySeats*>(pDebuggerSeats);

	if (debuggerConfig.maxPlugins > 0)
		m_ppPlugins = new (pSeats->m_plugins) SledDebuggerPlugin*[debuggerConfig.maxPlugins];
	
	{
		StringArrayConfig config;
		config.allowDuplicates = false;
		config.maxEntries = debuggerConfig.maxScriptCacheEntries;
		config.maxEntryLen = debuggerConfig.maxScriptCacheEntryLen;
		StringArray::create(&config, pSeats->m_scriptCache, &m_pScriptCache);
	}

	{
		NetworkBufferConfig config;
		config.maxSize = debuggerConfig.maxRecvBufferSize;
		NetworkBuffer::create(config, pSeats->m_recvBuf, &m_pRecvBuf);
	}

	{
		NetworkBufferConfig config;
		config.maxSize = debuggerConfig.maxSendBufferSize;
		NetworkBuffer::create(config, pSeats->m_sendBuf, &m_pSendBuf);
	}
	
	Network::create(debuggerConfig.net, pSeats->m_network, &m_pNetwork);	

	m_pMutex = new (pSeats->m_mutex) SceSledPlatformMutex;
	sceSledPlatformMutexAllocate(m_pMutex, SCE_SLEDPLATFORM_MUTEX_RECURSIVE);
	
	m_bInitialized = true;
}

SledDebugger::~SledDebugger()
{
}

void SledDebugger::shutdown()
{
	if (!m_bInitialized)
		return;	

	// Call Shutdown() on each plugin
	for (uint16_t i = 0; i < m_iPluginCount; i++)
	{
		// If already shutdown don't do it twice
		if (!m_ppPlugins[i]->m_bInitialized)
			continue;

		m_ppPlugins[i]->shutdown();
		m_ppPlugins[i]->m_bInitialized = false;
	}

	// Kill networking if it's still going
	stopNetworking();
	m_bInitialized = false;

	Network::shutdown(m_pNetwork);

	sceSledPlatformMutexDeallocate(m_pMutex);
}

int32_t SledDebugger::startNetworking()
{
	int32_t iRetval = 0;

	// Not initialized, get out
	if (!m_bInitialized)
		return SCE_SLED_ERROR_NOTINITIALIZED;

	iRetval = m_pNetwork->start();
	if (iRetval < 0)
		return iRetval;
	
	if (m_pNetwork->getNetworkParams().blockUntilConnect && m_pNetwork->isNetworking())
	{
		iRetval = m_pNetwork->accept(true);
		if (iRetval < 0)
		{
			stopNetworking();
			return iRetval;
		}
		iRetval = internal_Connected();
		if (iRetval < 0)
		{
			return iRetval;
		}
	}
	
	return SCE_SLED_ERROR_OK;
}

int32_t SledDebugger::stopNetworking()
{
	// If a client is connected send a disconnect notice
	if (isDebuggerConnected())
	{
		SCMP::Disconnect scmp(kSDMPluginId);
		send((uint8_t*)&scmp, scmp.length);
	}

	return m_pNetwork->stop();
}

int32_t SledDebugger::update()
{	
	const sce::SledPlatform::MutexLocker smg(m_pMutex);

	if (!m_pNetwork->isNetworking())
		return SCE_SLED_ERROR_NOTNETWORKING;

	if (m_bUpdateGuard)
		return SCE_SLED_ERROR_RECURSIVEUPDATE;	

	m_bUpdateGuard = true;

	const int32_t iRetval = internal_Update();
	if (iRetval != 0)
		DPRINTF("iRetval=%#x\n", iRetval);
	
	m_bUpdateGuard = false;

	return iRetval;
}

const Version SledDebugger::getVersion() const
{
	const Version ver(SCE_LIBSLEDDEBUGGER_VER_MAJOR, SCE_LIBSLEDDEBUGGER_VER_MINOR, SCE_LIBSLEDDEBUGGER_VER_REVISION);
	return ver;
}

bool SledDebugger::isNetworking() const
{ 
	return m_pNetwork->isNetworking(); 
}

int32_t SledDebugger::addPlugin(SledDebuggerPlugin *pPlugin)
{
	if (!pPlugin)
		return SCE_SLED_ERROR_INVALIDPLUGIN;

	if ((m_iPluginCount + 1) > m_iMaxPlugins)
		return SCE_SLED_ERROR_MAXPLUGINSREACHED;
	
	const uint16_t iPluginId = pPlugin->getId();
	
	// Check for duplicates	
	for (uint16_t i = 0; i < m_iPluginCount; i++)
	{
		if (m_ppPlugins[i]->getId() == iPluginId)
			return SCE_SLED_ERROR_PLUGINALREADYADDED;
	}

	// Add new plugin		
	pPlugin->m_pScriptMan = this;
	pPlugin->m_bInitialized = true;
	m_ppPlugins[m_iPluginCount++] = pPlugin;

	return SCE_SLED_ERROR_OK;
}

int32_t SledDebugger::removePlugin(SledDebuggerPlugin *pPlugin)
{
	if (!pPlugin)
		return SCE_SLED_ERROR_INVALIDPLUGIN;

	if(m_iPluginCount == 0)
		return SCE_SLED_ERROR_STAT;
	
	const uint16_t iPluginId = pPlugin->getId();
	
	// Check for duplicates	
	int idx = -1;	
	for (uint16_t i = 0; i < m_iPluginCount; i++)
	{
		if (m_ppPlugins[i]->getId() == iPluginId)
		{
			idx = i;
			break;
		}
	}

	if (idx == -1)
		return SCE_SLED_ERROR_SRCH;

	// Add new plugin		
	pPlugin->m_pScriptMan = NULL;
	pPlugin->m_bInitialized = false;

	for (int i = idx; i < m_iPluginCount - 1; i++)
		m_ppPlugins[i] = m_ppPlugins[i + 1];

	m_iPluginCount--;

	return SCE_SLED_ERROR_OK;
}

bool SledDebugger::scriptCacheAdd(const char *pszRelPathToScriptFile)
{		
	if (!pszRelPathToScriptFile)
		return false;

	const int len = (int)std::strlen(pszRelPathToScriptFile);
	if (len <= 0)
		return false;

	if (!m_pScriptCache)
		return false;

	return m_pScriptCache->add(pszRelPathToScriptFile);
}

bool SledDebugger::scriptCacheRemove(const char *pszRelPathToScriptFile)
{
	if (!pszRelPathToScriptFile)
		return false;

	if (!m_pScriptCache)
		return false;

	return m_pScriptCache->remove(pszRelPathToScriptFile);
}

void SledDebugger::scriptCacheClear()
{
	SCE_SLED_ASSERT(m_pScriptCache != NULL);
	m_pScriptCache->clear();
}

int32_t SledDebugger::breakpointReached(const BreakpointParams *pParams)
{
	const sce::SledPlatform::MutexLocker smg(m_pMutex);

	if (!pParams)
		return SCE_SLED_ERROR_NULLPARAMETER;

	if (!isNetworking())
		return SCE_SLED_ERROR_NOTNETWORKING;

	if (!isDebuggerConnected())
		return SCE_SLED_ERROR_NOCLIENTCONNECTED;	

	int32_t iUpdate = 0;

	///////////////////////////////////////////////////////////////////////
	// Breakpoint 'begin' logic chunk
	{
		// Send 'begin' message (breakpoint details) to SLED
		SCMP::Breakpoint::Begin scmpBpBeg(kSDMPluginId,
										  pParams->pluginId,
										  pParams->relFilePath,
										  pParams->lineNumber,
										  m_pSendBuf);
		send(m_pSendBuf->getData(), m_pSendBuf->getSize());

		//int iSpamCount = 0;

		// Wait to read 'begin' message from SLED
		iUpdate = internal_Update();
		while ((iUpdate != SCE_SLED_ERROR_BP_BEG) && isDebuggerConnected())
		{
			/*if (iSpamCount <= 0)
			{
				SCE_SLED_LOG(Logging::kInfo, "[SLED] [BP_R] [Update(): %i] [Waiting for BP_BEG]", iUpdate);
				iSpamCount++;
			}*/
			iUpdate = internal_Update();
		}

		// Stop if still not connected
		if (!isDebuggerConnected())
			return SCE_SLED_ERROR_NOCLIENTCONNECTED;		

		// Tell plugins we hit a breakpoint
		internal_BreakpointBegin(pParams);
	}	

	///////////////////////////////////////////////////////////////////////
	// Breakpoint 'sync' logic chunk
	{
		// Send 'sync' message to SLED
		SCMP::Breakpoint::Sync scmpBpSync(kSDMPluginId,
										  pParams->pluginId,
										  pParams->relFilePath,
										  pParams->lineNumber,
										  m_pSendBuf);
		send(m_pSendBuf->getData(), m_pSendBuf->getSize());

		//iSpamCount = 0;

		// Wait to read 'sync' message from SLED; a bunch of stuff will come over from SLED
		// before the actual sync message is received and plugins will respond to these
		// extra messages and send anything back
		iUpdate = internal_Update();
		while ((iUpdate != SCE_SLED_ERROR_BP_SYNC) && isDebuggerConnected())
		{
			/*if (iSpamCount <= 0)
			{
				SCE_SLED_LOG(Logging::kInfo, "[SLED] [BP_R] [Update(): %i] [Waiting for BP_SYNC]", iUpdate);
				iSpamCount++;
			}*/
			iUpdate = internal_Update();
		}

		// Stop if still not connected
		if (!isDebuggerConnected())
		{
			// Tell plugins about breakpoint is over
			internal_BreakpointEnd(pParams);
			return SCE_SLED_ERROR_NOCLIENTCONNECTED;
		}
	}

	///////////////////////////////////////////////////////////////////////
	// Breakpoint 'end' logic chunk
	{
		// Send 'end' message to SLED
		SCMP::Breakpoint::End scmpBpEnd(kSDMPluginId,
										pParams->pluginId,
										pParams->relFilePath,
										pParams->lineNumber,
										m_pSendBuf);
		send(m_pSendBuf->getData(), m_pSendBuf->getSize());

		//iSpamCount = 0;
		
		// Wait to read end message from SLED
		iUpdate = internal_Update();
		while ((iUpdate != SCE_SLED_ERROR_BP_END) && isDebuggerConnected())
		{
			/*if (iSpamCount <= 0)
			{
				SCE_SLED_LOG(Logging::kInfo, "[SLED] [BP_R] [Update(): %i] [Waiting for BP_END]", iUpdate);
				iSpamCount++;
			}*/
			iUpdate = internal_Update();
		}

		// Stop if still not connected
		if (!isDebuggerConnected())
		{
			// Tell plugins about breakpoint is over
			internal_BreakpointEnd(pParams);
			return SCE_SLED_ERROR_NOCLIENTCONNECTED;
		}
	}

	//iSpamCount = 0;

	///////////////////////////////////////////////////////////////////////
	// Wait for SLED to send 'continue' signal
	{
		// Halt/block execution by waiting for a 'continue-from-breakpoint' signal from SLED
		iUpdate = internal_Update();
		while ((iUpdate != SCE_SLED_ERROR_BP_CONT) && isDebuggerConnected())
		{
			/*if (iSpamCount <= 0)
			{
				SCE_SLED_LOG(Logging::kInfo, "[SLED] [BP_R] [Update(): %i] [Waiting for BP_CONT]", iUpdate);
				iSpamCount++;
			}*/
			iUpdate = internal_Update();
		}

		// Stop if still not connected
		if (!isDebuggerConnected())
		{
			// Tell plugins about breakpoint is over
			internal_BreakpointEnd(pParams);
			return SCE_SLED_ERROR_NOCLIENTCONNECTED;
		}
	}

	///////////////////////////////////////////////////////////////////////
	// Notify SLED we received 'continue' signal and that we're continuing
	{
		// Send 'continue' signal to SLED
		SCMP::Breakpoint::Continue scmpBpCont(kSDMPluginId,
											  pParams->pluginId,
											  pParams->relFilePath,
											  pParams->lineNumber,
											  m_pSendBuf);
		send(m_pSendBuf->getData(), m_pSendBuf->getSize());

		// Tell plugins that breakpoint is over
		internal_BreakpointEnd(pParams);
	}	

	return SCE_SLED_ERROR_OK;
}

void SledDebugger::internal_BreakpointBegin(const BreakpointParams *pParams)
{
	SCE_SLED_ASSERT(pParams != NULL);

	// Tell plugins to start sending stuff for this breakpoint if they need to
	for (uint16_t i = 0; i < m_iPluginCount; i++)
		m_ppPlugins[i]->clientBreakpointBegin(pParams);
}

void SledDebugger::internal_BreakpointEnd(const BreakpointParams *pParams)
{
	SCE_SLED_ASSERT(pParams != NULL);

	// Tell plugins the breakpoint is over and normal execution is resuming
	for (uint16_t i = 0; i < m_iPluginCount; i++)
		m_ppPlugins[i]->clientBreakpointEnd(pParams);
}

void SledDebugger::internal_DebugModeChanged(const DebuggerMode::Enum& newMode)
{
	DPRINTF("Internal_DebugModeChanged => %d\n", newMode);
	SCE_SLED_ASSERT((newMode >= DebuggerMode::kNormal) && (newMode <= DebuggerMode::kStop));

	// Relay to any plugins
	for (uint16_t i = 0; i < m_iPluginCount; i++)
		m_ppPlugins[i]->clientDebugModeChanged(newMode);
}

int32_t SledDebugger::processMessages()
{
	int32_t iRetval = 0;
	bool bReturnImmediately = false;
	bool bCanProcess = true;

	while (((int32_t)m_pRecvBuf->getSize() >= SCMP::Base::kSizeOfBase) && bCanProcess && !bReturnImmediately)
	{
		const uint8_t *pData = m_pRecvBuf->getData();
		SCE_SLED_ASSERT_MSG((reinterpret_cast<std::size_t>(pData) % 4) == 0, "pData must be 4 byte aligned");

		const SCMP::Base *pScmp = 0;
		memcpy(&pScmp, &pData, sizeof(SCMP::Base*));

		const SCMP::Base& scmp = *pScmp;
		//DPRINTF("ProcessMessages(): scmp.typeCode = %d\n", scmp.typeCode);

		if ((int32_t)m_pRecvBuf->getSize() >= scmp.length)
		{
			// Pass off to SDM or plugins
			onClientMessage(m_pRecvBuf->getData(), m_pRecvBuf->getSize());

			// Test for a couple special cases where we want
			// message processing to stop & return but there
			// may still be complete messages in the buffer
			if (scmp.isBreakpoint())
			{
				if (scmp.typeCode == SCMP::TypeCodes::kBreakpointBegin)
					iRetval = SCE_SLED_ERROR_BP_BEG;
				else if (scmp.typeCode == SCMP::TypeCodes::kBreakpointSync)
					iRetval = SCE_SLED_ERROR_BP_SYNC;
				else
					iRetval = SCE_SLED_ERROR_BP_END;
				bReturnImmediately = true;
			}
			else if (scmp.isDebug())
			{
				iRetval = SCE_SLED_ERROR_BP_CONT;
				bReturnImmediately = true;
			}
			else if (scmp.isReady())
			{
				iRetval = SCE_SLED_ERROR_READY;
				bReturnImmediately = true;
			}
			else
			{
				iRetval = SCE_SLED_ERROR_MESSAGE;
			}

			// Shuffle contents
			m_pRecvBuf->shuffle(scmp.length);
		}
		else
		{
			bCanProcess = false;
		}
	}

	return iRetval;
}

int32_t SledDebugger::send(const uint8_t *pData, const int32_t& iSize)
{
	if (!pData)
		return SCE_SLED_ERROR_NULLPARAMETER;

	if (iSize <= 0)
		return SCE_SLED_ERROR_INVALIDPARAMETER;

	if (!isNetworking())
		return SCE_SLED_ERROR_NOTNETWORKING;

	if (!isDebuggerConnected())
		return SCE_SLED_ERROR_NOCLIENTCONNECTED;

	const int32_t iRetval = m_pNetwork->send(pData, iSize);
	return iRetval;
}

int32_t SledDebugger::internal_Connected()
{
	DPRINTF("Internal_Connected()\n");
	// Common connection function - TCP path
	// will call this function when:
	// - Tcp: socket Accept()'s a connection

	int32_t iRetval = 0;

	// Update connection state
	m_hConnectionState = kConnecting;

	// Clear m_pNetwork buffer contents
	m_pRecvBuf->reset();

	// Connection negotiation steps:

	// 1) Connection is established and we are here

	// 1.1) Send endianness
	SCMP::Endianness endianness(kSDMPluginId);
	int ret = send((uint8_t*)&endianness, endianness.length);
	SCE_SLED_ASSERT(ret == endianness.length);

	// 2) SDM sends version details
	SCMP::Version version(kSDMPluginId,
						  SCE_LIBSLEDDEBUGGER_VER_MAJOR,
						  SCE_LIBSLEDDEBUGGER_VER_MINOR,
						  SCE_LIBSLEDDEBUGGER_VER_REVISION, 
						  m_pSendBuf);
	ret = send(m_pSendBuf->getData(), m_pSendBuf->getSize());
	SCE_SLED_ASSERT(ret == (int)m_pSendBuf->getSize());

	bool bConnected = false;

	// 3) Wait & read some data - hopefully a "success" message
	if (internal_WaitForSuccess())
	{
		// Got success message so we're here
		bConnected = true;

		// Send authenticated message
		SCMP::Authenticated auth(kSDMPluginId);
		send((uint8_t*)&auth, auth.length);
	}
						
	if (bConnected)
	{
		// Connected - fire event!
		iRetval = SCE_SLED_ERROR_CONNECTED;
		m_hConnectionState = kConnected;
		DPRINTF("call OnClientConnected()\n");
		onClientConnected();

		//int iSpamCount = 0;

		// Block & wait until SLED sends over initial data only stopping when a ready message comes in
		// or some other error has occurred
		int32_t iUpdate = internal_Update();
		while ((iUpdate != SCE_SLED_ERROR_READY) && isDebuggerConnected())
		{
			/*if (iSpamCount <= 0)
			{
				SCE_SLED_LOG(Logging::kInfo, "[SLED] [READY] [Update(): %i] [Waiting for READY]", iUpdate);
				iSpamCount++;
			}*/
			iUpdate = internal_Update();
		}
		//DPRINTF("iUpdate=%#x, SCE_SLED_ERROR_READY=%#x, IsDebuggerConnected()=%#x\n",	iUpdate, SCE_SLED_ERROR_READY, IsDebuggerConnected());

		// If still connected tell SLED
		if (isDebuggerConnected())
		{
			SCMP::Ready ready(kSDMPluginId);
			send((uint8_t*)&ready, ready.length);
		}

		DPRINTF("Internal_Connected succeeded\n");
		return SCE_SLED_ERROR_OK;
	}
	else
	{
		// Disconnect; something wasn't right
		SCMP::Disconnect scmp(kSDMPluginId);
		send((uint8_t*)&scmp, scmp.length);

		// Update state
		m_hConnectionState = kDisconnected;

		iRetval = m_pNetwork->disconnect();
		if(iRetval < 0)
			return iRetval;		

		DPRINTF("Internal_Connected failed\n");
		return SCE_SLED_ERROR_NEGOTIATION;
	}	
}

int32_t SledDebugger::internal_Disconnected()
{
	DPRINTF("Internal_Disonnected()\n");
	// Common disconnected function - TCP path
	// will call this function when:
	// - Tcp: socket receives <= 0 data

	const int32_t iRetval = 0;

	m_hConnectionState = kDisconnected;
	m_hDebuggerMode = DebuggerMode::kNormal;

	// Clear network buffer contents
	m_pRecvBuf->reset();

	// Relay to OnClientDisconnected()
	onClientDisconnected();

	return iRetval;
}

int32_t SledDebugger::internal_Update()
{
	// Check if any messages to process
	int32_t iRetval = processMessages();

	// Check if we need to return immediately
	if ((iRetval != 0) && (iRetval != SCE_SLED_ERROR_MESSAGE))
		return iRetval;

	if (!m_pNetwork->isConnected())
	{
		iRetval = m_pNetwork->accept(false);
		
		// accept failed.
		if (iRetval == SCE_SLED_ERROR_NOTNETWORKING)
			return SCE_SLED_ERROR_OK;
		
		if (iRetval != 0)
		{
			SCE_SLED_ASSERT(iRetval < 0);
			return iRetval;
		}

		DPRINTF("Connected\n");
		iRetval = internal_Connected();
	}
	else
	{
		uint8_t buf[SCE_LIBSLEDDEBUGGER_NET_BUFSTATIC];
		iRetval = m_pNetwork->recv(buf, SCE_LIBSLEDDEBUGGER_NET_BUFSTATIC, false);
		
		if (iRetval > 0)
			m_pRecvBuf->append(buf, iRetval);
		
		if (iRetval < 0)
		{
			iRetval = internal_Disconnected();
			DPRINTF("Disconnected\n");
		}
	}
	
	return iRetval;
}

bool SledDebugger::internal_WaitForSuccess()
{
	const int iSizeOfSuccess = SCMP::Base::kSizeOfBase;

	uint8_t buf[SCMP::Base::kSizeOfBase];
	const int iRetval = m_pNetwork->recv(buf, iSizeOfSuccess, true);
    DPRINTF("Recv => %d (expected=%d)\n", iRetval, iSizeOfSuccess);

	if (iRetval == iSizeOfSuccess)
	{
		SCMP::Base scmp;
		memcpy(&scmp, buf, sizeof(SCMP::Base));

		if (scmp.typeCode == SCMP::TypeCodes::kSuccess)
			return true;
	}

	DPRINTF("Internal_WaitForSuccess() failed, iRetval=%d (iSizeOfSuccess=%d)\n", iRetval, iSizeOfSuccess);
	return false;
}

int32_t SledDebugger::ttyNotify(const char *pszMessage)
{
	if (pszMessage == NULL)
		return SCE_SLED_ERROR_NULLPARAMETER;

	int iLen = (int)std::strlen(pszMessage);
	if (iLen <= 0)
		return SCE_SLED_ERROR_INVALIDPARAMETER;

	if (!isNetworking())
		return SCE_SLED_ERROR_NOTNETWORKING;

	if (!isDebuggerConnected())
		return SCE_SLED_ERROR_NOCLIENTCONNECTED;

	SCMP::TTYBegin ttyBeg(kSDMPluginId);
	send((uint8_t*)&ttyBeg, ttyBeg.length);

	int iOffset = 0;
	
	do
	{
		iLen = (int)std::strlen(pszMessage + iOffset);

		SCMP::TTY tty(kSDMPluginId, 
					  pszMessage + iOffset,
					  m_pSendBuf);
		send(m_pSendBuf->getData(), m_pSendBuf->getSize());

		if (iLen > SCMP::Base::kStringLen)
			iOffset += (SCMP::Base::kStringLen - 1);
	} while (iLen > SCMP::Base::kStringLen);

	SCMP::TTYEnd ttyEnd(kSDMPluginId);
	send((uint8_t*)&ttyEnd, ttyEnd.length);

	return SCE_SLED_ERROR_OK;
}

void SledDebugger::onClientConnected()
{
	//SCE_SLED_LOG(Logging::kInfo, "[SLED] Connected!");

	// Send off script cache contents
	StringArrayConstIterator iter(m_pScriptCache);
	for (; iter(); ++iter)
	{
		SCMP::ScriptCache scmp(kSDMPluginId, iter.get(), m_pSendBuf);
		send(m_pSendBuf->getData(), m_pSendBuf->getSize());
	}

	// Relay connection to any plugins
	for (uint16_t i = 0; i < m_iPluginCount; i++)
		m_ppPlugins[i]->clientConnected();

	// Send plugins ready message
	{
		SCMP::PluginsReady scmp(kSDMPluginId);
		send((uint8_t*)&scmp, scmp.length);
	}
}		

void SledDebugger::onClientDisconnected()
{
	// Relay disconnection to any plugins
	for (uint16_t i = 0; i < m_iPluginCount; i++)
		m_ppPlugins[i]->clientDisconnected();

	//SCE_SLED_LOG(Logging::kInfo, "[SLED] Disconnected");
}

void SledDebugger::onClientMessage(const uint8_t *pData, const int32_t& iSize)
{
	SCE_SLED_ASSERT((reinterpret_cast<uintptr_t>(pData) % __alignof(SCMP::Base)) == 0);

	const SCMP::Base *pScmp = 0;
	memcpy(&pScmp, &pData, sizeof(SCMP::Base*));
	
	const SCMP::Base& scmp = *pScmp;
	//SCE_SLED_LOG(Logging::kInfo, "[SLED] [SCMP::Base] [Length: %i] [TypeCode: %u] [PluginId: %u]", scmp.length, scmp.typeCode, scmp.pluginId);

	if (scmp.pluginId == kSDMPluginId)
	{
		// libsleddebugger specific message
		switch (scmp.typeCode)
		{
		case SCMP::TypeCodes::kDebugStart:
			internal_DebugModeChanged(DebuggerMode::kNormal);
			m_hDebuggerMode = DebuggerMode::kNormal;
			break;
		case SCMP::TypeCodes::kDebugStepInto:
			internal_DebugModeChanged(DebuggerMode::kStepInto);
			m_hDebuggerMode = DebuggerMode::kStepInto;
			break;
		case SCMP::TypeCodes::kDebugStepOver:
			internal_DebugModeChanged(DebuggerMode::kStepOver);
			m_hDebuggerMode = DebuggerMode::kStepOver;
			break;
		case SCMP::TypeCodes::kDebugStepOut:
			internal_DebugModeChanged(DebuggerMode::kStepOut);
			m_hDebuggerMode = DebuggerMode::kStepOut;
			break;
		case SCMP::TypeCodes::kDebugStop:
			internal_DebugModeChanged(DebuggerMode::kStop);
			m_hDebuggerMode = DebuggerMode::kStop;
			break;
		case SCMP::TypeCodes::kHeartbeat:
			{
				//SCE_SLED_LOG(Logging::kInfo, "[SLED] [SCMP::Heartbeat]");
				SCMP::Heartbeat scmpHb(kSDMPluginId);
				send((uint8_t*)&scmpHb, scmpHb.length);
			}
			break;
		case SCMP::TypeCodes::kProtocolDebugMark:
			{
				// Just respond with the same message
				SCMP::ProtocolDebugMark scmpPdm(kSDMPluginId);
				send((uint8_t*)&scmpPdm, scmpPdm.length);
			}
			break;
		}
	}
	else
	{
		// Figure out which plugin to dispatch to
		SledDebuggerPlugin *pPlugin = 0;

		if (m_iPluginCount == 1)
		{
			pPlugin = m_ppPlugins[0];
		}
		else
		{
			for (uint16_t i = 0; (i < m_iPluginCount) && !pPlugin; i++)
			{
				if (m_ppPlugins[i]->getId() == scmp.pluginId)
				{
					pPlugin = m_ppPlugins[i];
				}
			}
		}

		if (pPlugin)
			pPlugin->clientMessage(pData, iSize);
	}
}

int32_t SledDebugger::generateHash(const char *pszString, const int32_t& iLine, int32_t *iHash)
{
	if (!iHash)
		return SCE_SLED_ERROR_NULLPARAMETER;	

	if (!pszString)
		return SCE_SLED_ERROR_NULLPARAMETER;

	const int32_t iLen = (int32_t)std::strlen(pszString);
	if (iLen <= 0)
		return SCE_SLED_ERROR_INVALIDPARAMETER;
	
	*iHash = 0;
	for (int32_t i = 0; i < iLen; i++)
	{
		// Skip forward & backward slashes
		if ((pszString[i] == '\\') || (pszString[i] == '/'))
			continue;
		
		(*iHash) += (int32_t)::tolower(pszString[i]);
	}

	(*iHash) += iLine;
	return SCE_SLED_ERROR_OK;
}

uint32_t SledDebugger::generateFNV1AHash(const char *str)
{
	// blatantly copy/pasted/edited from http://encode.ru/threads/612-Fastest-decompressor!?p=22184&viewfull=1#post22184
	const uint32_t PRIME = 1607;
	uint32_t hash32 = 2166136261;

	const char *p = str;
	std::size_t len = std::strlen(str);

	for(; len >= sizeof(uint32_t); len -= sizeof(uint32_t), p += sizeof(uint32_t))
	{
		hash32 = (hash32 ^ *(uint32_t*)p) * PRIME;
	}
	if (len & sizeof(uint16_t))
	{
		hash32 = (hash32 ^ *(uint16_t*)p) * PRIME;
		p += sizeof(uint16_t);
	}
	if (len & 1) 
		hash32 = (hash32 ^ *p) * PRIME;

	return hash32 ^ (hash32 >> 16);
}
