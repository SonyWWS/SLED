/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_NETWORK_H__
#define __SCE_LIBSLEDDEBUGGER_NETWORK_H__

#include "params.h"
#include "common.h"

#include "../sledcore/socket.h"

/*	target specific headers		*/
#if SCE_SLEDTARGET_OS_WINDOWS
#else
	#error Not supported
#endif

/// Namespace for sce classes and functions
namespace sce
{ 
/// Namespace for Sled classes and functions
namespace Sled
{
	class ISequentialAllocator;

	class SCE_SLED_LINKAGE Network 
	{
	public:
		static int32_t create(const NetworkParams& networkParams, void *pLocation, Network **ppNetwork);
		static int32_t requiredMemory(std::size_t *iRequiredMemory);
		static int32_t requiredMemoryHelper(ISequentialAllocator *pAllocator, void **ppThis);
		static void shutdown(Network *pNetwork);	
	public:
		inline bool isNetworking() const { return m_bNetworking; }
		inline bool isConnected() const { return m_bConnected; }	
	public:
		int32_t start();
		int32_t stop();
		int32_t accept(bool isBlocking);
		int32_t disconnect();
		int32_t send(const uint8_t *pData, const int32_t& iSize);
		int32_t recv(uint8_t *buf, const int32_t& iSize, bool isBlocking);
	public:
		const NetworkParams& getNetworkParams() const { return m_hNetworkParams; }
	private:
		Network(const NetworkParams& networkParams, void *pNetworkSeats);
		~Network();
		Network(const Network&);
		Network& operator=(const Network&);
	private:
		NetworkParams m_hNetworkParams;
		bool m_bNetworking;
		bool m_bConnected;
		bool m_bIpAddrChanged;

		// For Tcp
		SceSledPlatformSocket*	m_pListenSock;
		SceSledPlatformSocket*	m_pConnectSock;

		int32_t initializeTcp();
		int32_t startTcp();
		int32_t stopTcp();
		int32_t acceptTcp(bool isBlocking);
		int32_t sendTcp(const uint8_t *pData, const int32_t& iSize);
		int32_t recvTcp(uint8_t *buf, const int32_t& iSize, bool isBlocking);
	};
}}

#endif // __SCE_LIBSLEDDEBUGGER_NETWORK_H__
