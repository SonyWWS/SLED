/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#include "assert.h"
#include "buffer.h"
#include "debug.h"
#include "network.h"
#include "scmp.h"
#include "sequentialallocator.h"
#include "sleddebugger.h"
#include "common.h"

#include <new>
#include <cstdio>

#if SCE_SLEDTARGET_OS_WINDOWS
	#ifndef WIN32_LEAN_AND_MEAN
		#define WIN32_LEAN_AND_MEAN
	#endif
	#include <WinSock2.h>
#endif

using namespace sce::Sled;

namespace
{
	struct SCE_SLED_LINKAGE NetworkSeats
	{
		void *m_this;
		void *m_listenSock;
		void *m_connSock;

		void Allocate(ISequentialAllocator *pAllocator)
		{
			m_this = pAllocator->allocate(sizeof(Network), __alignof(Network));

			// For m_pListenSock
			m_listenSock = pAllocator->allocate(sizeof(SceSledPlatformSocket), __alignof(SceSledPlatformSocket));

			// For m_pConnectSock
			m_connSock = pAllocator->allocate(sizeof(SceSledPlatformSocket), __alignof(SceSledPlatformSocket));
		}
	};

	SCE_SLED_LINKAGE bool IsNetworkSubsystemStarted(Protocol::Enum protocol)
	{
#if SCE_SLEDTARGET_OS_WINDOWS
		SCE_SLEDUNUSED(protocol);
		return sceSledPlatformNetworkGetInitialized();
#endif
	}

	SCE_SLED_LINKAGE bool DoWeHaveIpAddress()
	{
#if SCE_SLEDTARGET_OS_WINDOWS	
		return true; /* don't care? */
#endif
	}
}

namespace
{
	SCE_SLED_LINKAGE void SocketClose(SceSledPlatformSocket *pSocket)
	{
		if (sceSledPlatformSocketIsInvalid(*pSocket))
			return;

		sceSledPlatformSocketShutdown(*pSocket, SCE_SLEDPLATFORM_SOCKET_SHUTDOWN_BOTH);
		sceSledPlatformSocketClose(*pSocket);
		sceSledPlatformSocketSetInvalid(pSocket);
	}

	SCE_SLED_LINKAGE bool SocketInit(SceSledPlatformSocket *pSocket)
	{
		SocketClose(pSocket);

		SceSledPlatformSocketError err =
			sceSledPlatformSocketSocket(
			SCE_SLEDPLATFORM_SOCKET_AF_INET,
			SCE_SLEDPLATFORM_SOCKET_PROTOCOL_TCP,
			pSocket);

		if (err != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
			return false;

		if (sceSledPlatformSocketIsInvalid(*pSocket))
			return false;

		err = sceSledPlatformSocketSetReuseAddr(*pSocket, true);
		if (err != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
			return false;

		return !sceSledPlatformSocketIsInvalid(*pSocket);
	}
}

int32_t Network::create(const NetworkParams& networkParams, void *pLocation, Network **ppNetwork)
{
	SCE_SLED_ASSERT(pLocation != NULL);
	SCE_SLED_ASSERT(ppNetwork != NULL);

	std::size_t iMemSize = 0;
	const int32_t iConfigError = requiredMemory(&iMemSize);
	if (iConfigError != 0)
		return iConfigError;

	SequentialAllocator allocator(pLocation, iMemSize);

	NetworkSeats seats;	
	seats.Allocate(&allocator);

	SCE_SLED_ASSERT(seats.m_this != NULL);
	SCE_SLED_ASSERT(seats.m_listenSock != NULL);
	SCE_SLED_ASSERT(seats.m_connSock != NULL);

	*ppNetwork = new (seats.m_this) Network(networkParams, &seats);

	return SCE_SLED_ERROR_OK;
}

int32_t Network::requiredMemory(std::size_t *iRequiredMemory)
{
	SCE_SLED_ASSERT(iRequiredMemory != NULL);

	SequentialAllocatorCalculator allocator;
	
	NetworkSeats seats;	
	seats.Allocate(&allocator);

	*iRequiredMemory = allocator.bytesAllocated();
	return SCE_SLED_ERROR_OK;
}

int32_t Network::requiredMemoryHelper(ISequentialAllocator *pAllocator, void **ppThis)
{
	SCE_SLED_ASSERT(pAllocator != NULL);
	SCE_SLED_ASSERT(ppThis != NULL);

	NetworkSeats seats;
	seats.Allocate(pAllocator);

	*ppThis = seats.m_this;
	return SCE_SLED_ERROR_OK;
}

void Network::shutdown(Network *pNetwork)
{
	SCE_SLED_ASSERT(pNetwork != NULL);
	pNetwork->~Network();
}

Network::Network(const NetworkParams& networkParams, void *pNetworkSeats)
	: m_hNetworkParams(networkParams)
	, m_bNetworking(false)
	, m_bConnected(false)
	, m_bIpAddrChanged(false)
	, m_pListenSock(NULL)
	, m_pConnectSock(NULL)
{
	SCE_SLED_ASSERT(pNetworkSeats != NULL);

	NetworkSeats *pSeats = static_cast<NetworkSeats*>(pNetworkSeats);

	m_pListenSock = new (pSeats->m_listenSock) SceSledPlatformSocket;
	m_pConnectSock = new (pSeats->m_connSock) SceSledPlatformSocket;

	sceSledPlatformSocketSetInvalid(m_pListenSock);
	sceSledPlatformSocketSetInvalid(m_pConnectSock);
}

Network::~Network()
{
}

int32_t Network::start(void)
{
	int32_t iRetval = 0;

	// Do this instead of "<init/term>_network()" stuff
	if (!IsNetworkSubsystemStarted(m_hNetworkParams.protocol))
		return SCE_SLED_ERROR_NETSUBSYSTEMFAIL;

	// Already networking, get out
	if (isNetworking())
		return SCE_SLED_ERROR_ALREADYNETWORKING;

	switch (m_hNetworkParams.protocol)
	{
	case Protocol::kTcp:
		m_bIpAddrChanged = !DoWeHaveIpAddress();
		iRetval = m_bIpAddrChanged ? 0 : startTcp();
		break;
	default:
		iRetval = SCE_SLED_ERROR_INVALIDPROTOCOL;
		break;
	}

	// Networking has started if no errors reported
	if (iRetval == 0)
		m_bNetworking = true;

	return iRetval;
}

int32_t Network::stop(void)
{
	int32_t iRetval = 0;
	if (!isNetworking())
		return SCE_SLED_ERROR_NOTNETWORKING;

	switch (m_hNetworkParams.protocol)
	{
	case Protocol::kTcp:
		iRetval = stopTcp();
		m_bIpAddrChanged = false;
		break;
	default:
		iRetval = SCE_SLED_ERROR_INVALIDPROTOCOL;
		break;
	}

	if (iRetval == 0)
		m_bNetworking = false;
	
	return iRetval;
}

int32_t Network::accept(bool isBlocking)
{
	int32_t iRetval = 0;

	if (!isNetworking())
		return SCE_SLED_ERROR_NOTNETWORKING;
	
	if (isConnected())
		return SCE_SLED_ERROR_ALREADYNETWORKING;

	switch (m_hNetworkParams.protocol)
	{
	case Protocol::kTcp:
		iRetval = acceptTcp(isBlocking);
		break;
	default:
		iRetval = SCE_SLED_ERROR_INVALIDPROTOCOL;
		break;
	}
	
	if (iRetval == 0)
		m_bConnected = true;

	return iRetval;
}

int32_t Network::send(const uint8_t *pData, const int32_t& iSize)
{
	int32_t iRetval = 0;
	
	if (!isNetworking())
		return SCE_SLED_ERROR_NOTNETWORKING;
	
	if (!isConnected())
		return SCE_SLED_ERROR_NOCLIENTCONNECTED;
	
	switch (m_hNetworkParams.protocol)
	{
	case Protocol::kTcp:
		iRetval = sendTcp(pData, iSize);
		break;
	default:
		iRetval = SCE_SLED_ERROR_INVALIDPROTOCOL;
		break;
	}

	return iRetval;
}

int32_t Network::recv(uint8_t *buf, const int32_t& iSize, bool isBlocking)
{
	int32_t iRetval = 0;

	if (!isNetworking())
		return SCE_SLED_ERROR_NOTNETWORKING;
	
	if (!isConnected())
		return SCE_SLED_ERROR_NOCLIENTCONNECTED;

	switch (m_hNetworkParams.protocol)
	{
	case Protocol::kTcp:
		iRetval = recvTcp(buf, iSize, isBlocking);
		break;
	default:
		iRetval = SCE_SLED_ERROR_INVALIDPROTOCOL;
		break;
	}

	if (iRetval < 0)
		m_bConnected = false;		

	return iRetval;
}

int32_t Network::disconnect(void)
{
	if (m_hNetworkParams.protocol == Protocol::kTcp)
	{
		const bool b = SocketInit(m_pConnectSock);
		SCE_SLED_ASSERT(b);
	}

	m_bConnected = false;

	return SCE_SLED_ERROR_OK;
}

int32_t Network::startTcp(void)
{
	SCE_SLED_ASSERT(!isNetworking());
	SCE_SLED_ASSERT(!isConnected());

	if (!SocketInit(m_pListenSock) || !SocketInit(m_pConnectSock))
		return SCE_SLED_ERROR_TCPSOCKETINITFAIL;

	// Bind
	{		
		uint32_t outAddr = 0; // INADDR_ANY
		if (sceSledPlatformSocketInetAddr("0.0.0.0", &outAddr) != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
			return SCE_SLED_ERROR_TCPLISTENFAIL;

		SceSledPlatformSocketAddress listenAddr;

		if (sceSledPlatformSocketAddressInit(&listenAddr, SCE_SLEDPLATFORM_SOCKET_AF_INET) != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
			return SCE_SLED_ERROR_TCPLISTENFAIL;

		if (sceSledPlatformSocketAddressSetIPV4Addr(&listenAddr, outAddr) != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
			return SCE_SLED_ERROR_TCPLISTENFAIL;

		const uint16_t port = htons(m_hNetworkParams.port);

		if (sceSledPlatformSocketAddressSetPort(&listenAddr, port) != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
			return SCE_SLED_ERROR_TCPLISTENFAIL;

		if (sceSledPlatformSocketBind(*m_pListenSock, &listenAddr) != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
			return SCE_SLED_ERROR_TCPLISTENFAIL;
	}
	
	if (sceSledPlatformSocketListen(*m_pListenSock, 10) != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
		return SCE_SLED_ERROR_TCPLISTENFAIL;

	return SCE_SLED_ERROR_OK;			
}

int32_t Network::stopTcp(void)
{
	SCE_SLED_ASSERT(isNetworking());

	SocketClose(m_pListenSock);
	SocketClose(m_pConnectSock);

	return SCE_SLED_ERROR_OK;
}

int32_t Network::acceptTcp(bool isBlocking)
{	
	SCE_SLED_ASSERT(isNetworking());
	SCE_SLED_ASSERT(!isConnected());

	if (m_bIpAddrChanged)
	{
		if (DoWeHaveIpAddress())
		{
			// hack for the startTcp() check; value needs to be false when calling startTcp()			
			m_bNetworking = false;

			const int32_t errStartTcp = startTcp();

			// hack since manually changed for the startTcp() check
			m_bNetworking = true;

			if (errStartTcp == SCE_SLED_ERROR_OK)
				m_bIpAddrChanged = false;				
			else
				return SCE_SLED_ERROR_NOTNETWORKING;
		}
		else
		{
			return SCE_SLED_ERROR_NOTNETWORKING;
		}
	}

	if (sceSledPlatformSocketIsInvalid(*m_pListenSock) || sceSledPlatformSocketIsInvalid(*m_pConnectSock))
		return SCE_SLED_ERROR_TCPSOCKETINVALID;
	
	const SceSledPlatformSocketError errSetBlocking = sceSledPlatformSocketSetBlocking(*m_pListenSock, isBlocking);
	if (errSetBlocking != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
	{
		if (errSetBlocking == SCE_SLEDPLATFORM_SOCKET_ERROR_EIPADDRCHANGE)
		{
			m_bIpAddrChanged = true;

			SocketClose(m_pListenSock);
			SocketClose(m_pConnectSock);

			return SCE_SLED_ERROR_NOTNETWORKING;
		}
		else
		{
			return SCE_SLED_ERROR_TCPNONBLOCKINGFAIL;
		}
	}

	//
	// Socket not connected
	//

	// Check for data first (this is an attempt to avoid an errno setting)
	if (!isBlocking)
	{
		int32_t select = 0;	
		const SceSledPlatformSocketError err =
			sceSledPlatformSocketSelect(*m_pListenSock, SCE_SLEDPLATFORM_SOCKET_SELECT_READ, 0, 0, &select);

		if (err != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
		{
			if (!SocketInit(m_pListenSock))
				return SCE_SLED_ERROR_TCPSOCKETINITFAIL;

			return SCE_SLED_ERROR_NOTNETWORKING;
		}

		if (select != SCE_SLEDPLATFORM_SOCKET_SELECT_READ)
			return SCE_SLED_ERROR_NOTNETWORKING;
	}	

	SceSledPlatformSocketAddress addr;
	
	// Check for incoming connection - block if necessary based on blocking
	// mode set in start() for listen socket
	if (sceSledPlatformSocketAccept(*m_pListenSock, m_pConnectSock, &addr) != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
	{
		// No connection yet; reset socket to try again
		if (!SocketInit(m_pConnectSock))
			return SCE_SLED_ERROR_TCPSOCKETINITFAIL;

		return SCE_SLED_ERROR_NOTNETWORKING;
	}
	
	// Call common connected event
	return SCE_SLED_ERROR_OK;
}

int32_t Network::sendTcp(const uint8_t *pData, const int32_t& iSize)
{
	SCE_SLED_ASSERT(isNetworking());
	SCE_SLED_ASSERT(isConnected());

	if (sceSledPlatformSocketIsInvalid(*m_pConnectSock))
		return SCE_SLED_ERROR_TCPSOCKETINVALID;

	int32_t iSelect = 0;
	SceSledPlatformSocketError err =
		sceSledPlatformSocketSelect(
			*m_pConnectSock,
			SCE_SLEDPLATFORM_SOCKET_SELECT_WRITE,
			10,
			0,
			&iSelect);
	
	if (err != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
		return SCE_SLED_ERROR_TCPFAILSELECTWRITE;

	if (iSelect != SCE_SLEDPLATFORM_SOCKET_SELECT_WRITE)
		return SCE_SLED_ERROR_TCPFAILSELECTWRITE;
	
	int32_t iSent = 0;
	err =
		sceSledPlatformSocketSend(
			*m_pConnectSock,
			pData,
			iSize,
			0,
			&iSent);
	
	if (err != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
	{
		const bool b = SocketInit(m_pConnectSock);
		SCE_SLED_ASSERT(b);
	}

	return iSent;
}

int32_t Network::recvTcp(uint8_t *buf, const int32_t& iSize, bool isBlocking)
{
	SCE_SLED_ASSERT(isNetworking());
	SCE_SLED_ASSERT(isConnected());

	int32_t iSelect = 0;	
	SceSledPlatformSocketError err =
		sceSledPlatformSocketSelect(
			*m_pConnectSock,
			SCE_SLEDPLATFORM_SOCKET_SELECT_READ,
			isBlocking ? -1 : 0,
			isBlocking ? -1 : 0,
			&iSelect);

	if (err != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE)
	{
		const bool b = SocketInit(m_pConnectSock);
		SCE_SLED_ASSERT(b);
		return -1;
	}

	if (iSelect != SCE_SLEDPLATFORM_SOCKET_SELECT_READ)
		return 0;

	int32_t iRecv = 0;
	err =
		sceSledPlatformSocketRecv(
			*m_pConnectSock,
			(int8_t*)buf,
			iSize,
			0,
			&iRecv);

	if (err == SCE_SLEDPLATFORM_SOCKET_ERROR_EIPADDRCHANGE)
		m_bIpAddrChanged = true;

	if ((err != SCE_SLEDPLATFORM_SOCKET_ERROR_NONE) || (iRecv <= 0))
	{
		const bool b = SocketInit(m_pConnectSock);
		SCE_SLED_ASSERT(b);
		return -1;
	}

	return iRecv;
}
