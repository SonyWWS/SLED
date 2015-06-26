/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_SLEDDEBUGGER_H__
#define __SCE_LIBSLEDDEBUGGER_SLEDDEBUGGER_H__

#include "sleddebugger.h"

#include <cstdio>

struct SceSledPlatformMutex;

namespace sce {	namespace Sled
{
	class StringArray;
	class SledDebuggerPlugin;
	class NetworkBuffer;
	class Network;

	/**
	@brief Class describing a SLED debugger instance.

	Widely used class encapsulating the internals of a SLED debugger instance.
	Instantiate a <c>SledDebugger</c> from a <c>SledDebuggerConfig</c>.

	This class is closed, and its internal data is not accessible.

	The following are the main functions handling <c>SledDebugger</c>:

	<c>sce::Sled::debuggerCreate()</c>: Create a <c>SledDebugger</c> instance.

	<c>sce::Sled::debuggerRequiredMemory()</c>: Calculate the size in bytes required for a <c>SledDebugger</c> instance.

	<c>sce::Sled::debuggerShutdown()</c>: Shut down a <c>SledDebugger</c> instance.

	<c>sce::Sled::debuggerStartNetworking()</c>: Initialize networking and optionally block execution until a connection is made.

	<c>sce::Sled::debuggerStopNetworking()</c>: Stop networking (disconnect SLED if connected).

	<c>sce::Sled::debuggerUpdate()</c>: Poll sockets and process any incoming messages.

	<c>sce::Sled::debuggerAddPlugin()</c>: Add plugin to <c>SledDebugger</c>.

	<c>sce::Sled::debuggerRemovePlugin()</c>: Remove plugin from <c>SledDebugger</c>.

	<c>sce::Sled::debuggerBreakpointReached</c>: Plugins call this function when they encounter a breakpoint.

	For the full list of <c>SledDebugger</c> functions, see <c>sce::Sled</c>.
	*/	
	class SCE_SLED_LINKAGE SledDebugger
	{
	/// @cond
	public:		
		static int32_t create(const SledDebuggerConfig& debuggerConfig, void *pLocation, SledDebugger **ppDebugger);
		static int32_t requiredMemory(const SledDebuggerConfig& debuggerConfig, std::size_t *iRequiredMemory);
		static int32_t close(SledDebugger *pDebugger);
	private:
		SledDebugger(const SledDebuggerConfig& debuggerConfig, const void *pDebuggerSeats);
		~SledDebugger();
		SledDebugger(const SledDebugger&) : m_iMaxPlugins(0) {}
		SledDebugger& operator=(const SledDebugger&) { return *this; }
		void shutdown();
	public:
		int32_t	startNetworking();
		int32_t	stopNetworking();
		int32_t	update();
		const Version getVersion() const;
	public:
		inline bool isDebuggerConnected() const { return m_hConnectionState != kDisconnected; }
		bool isNetworking() const;
		int32_t	addPlugin(SledDebuggerPlugin *pPlugin);
		int32_t	removePlugin(SledDebuggerPlugin *pPlugin);
		bool scriptCacheAdd(const char *pszRelPathToScriptFile);
		bool scriptCacheRemove(const char *pszRelPathToScriptFile);
		void scriptCacheClear();
		inline DebuggerMode::Enum getDebuggerMode() const { return m_hDebuggerMode; }
		int32_t ttyNotify(const char *pszMessage);
	public:
		int32_t breakpointReached(const BreakpointParams *pParams);
	private:
		void internal_BreakpointBegin(const BreakpointParams *pParams);
		void internal_BreakpointEnd(const BreakpointParams *pParams);
		void internal_DebugModeChanged(const DebuggerMode::Enum& newMode);
	private:
		int32_t processMessages();
	public:
		int32_t	send(const uint8_t *pData, const int32_t& iSize);
		SceSledPlatformMutex *getMutex() const { return m_pMutex; }		
	private:
		int32_t internal_Connected();
		int32_t internal_Disconnected();
		int32_t internal_Update();
		bool internal_WaitForSuccess();
	private:
		void onClientConnected();
		void onClientDisconnected();
		void onClientMessage(const uint8_t *pData, const int32_t& iSize);
	public:
		static int32_t generateHash(const char *pszString, const int32_t& iLine, int32_t *iHash);
		static uint32_t generateFNV1AHash(const char *str);
	private:
		Network *m_pNetwork;
		SceSledPlatformMutex *m_pMutex;
	
		DebuggerMode::Enum m_hDebuggerMode;
	
		bool m_bInitialized;
		bool m_bUpdateGuard;
	
		StringArray *m_pScriptCache;
	
		SledDebuggerPlugin**	m_ppPlugins;
		const uint16_t			m_iMaxPlugins;
		uint16_t				m_iPluginCount;
	
		NetworkBuffer *m_pRecvBuf;
		NetworkBuffer *m_pSendBuf;
	private:
		enum ConnectionState
		{
			kDisconnected	= 0,
			kConnecting		= 1,
			kConnected		= 2
		};
	
		ConnectionState	m_hConnectionState;
	public:
		static const uint16_t kSDMPluginId = 0;
	/// @endcond
	};
}}

#endif // __SCE_LIBSLEDDEBUGGER_SLEDDEBUGGER_H__
