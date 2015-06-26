/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef __SCE_LIBSLEDDEBUGGER_ERRORCODES_H__
#define __SCE_LIBSLEDDEBUGGER_ERRORCODES_H__

#define SCE_SLED_ERROR_OK					(int)(0x0)			///< Success
#define SCE_SLED_ERROR_INVALIDPROTOCOL		(int)(0x80830001)	///< Invalid network protocol; error code.
#define SCE_SLED_ERROR_NOTINITIALIZED		(int)(0x80830002)	///< Not initialized; error code.
#define SCE_SLED_ERROR_ALREADYNETWORKING	(int)(0x80830003)	///< Already networking; error code.
#define SCE_SLED_ERROR_PLUGINALREADYADDED	(int)(0x80830004)	///< Plugin already added; error code.
#define SCE_SLED_ERROR_INVALIDPLUGIN		(int)(0x80830005)	///< Invalid plugin; error code.
#define SCE_SLED_ERROR_MAXPLUGINSREACHED	(int)(0x80830006)	///< Maximum number of plugins reached; error code.
#define SCE_SLED_ERROR_RECURSIVEUPDATE		(int)(0x80830007)	///< Attempt to call the Update function recursively; error code.
#define SCE_SLED_ERROR_NETSUBSYSTEMFAIL		(int)(0x80830008)	///< Network subsystem; error code.
#define SCE_SLED_ERROR_TCPNONBLOCKINGFAIL	(int)(0x80830009)	///< Tcp socket set non-blocking mode failed; error code.
#define SCE_SLED_ERROR_TCPLISTENFAIL		(int)(0x80830010)	///< Tcp socket failed to listen; error code.
#define SCE_SLED_ERROR_TCPBINDFAIL			(int)(0x80830011)	///< Tcp socket bind failed; error code.
#define SCE_SLED_ERROR_TCPSOCKETINITFAIL	(int)(0x80830012)	///< Tcp socket initialization failed; error code.
#define SCE_SLED_ERROR_TCPSOCKETINVALID		(int)(0x80830013)	///< Tcp socket is invalid; error code.
#define SCE_SLED_ERROR_TCPNOTCONNECTED		(int)(0x80830014)	///< Tcp socket not connected; error code.
#define SCE_SLED_ERROR_TCPFAILSELECTWRITE	(int)(0x80830015)	///< Tcp socket failed to select() for writing; error code.
#define SCE_SLED_ERROR_NOTNETWORKING		(int)(0x80830016)	///< Not networking; error code.
#define SCE_SLED_ERROR_NEGOTIATION			(int)(0x80830017)	///< Negotiation with SLED failed; error code.
#define SCE_SLED_ERROR_INVALIDCONFIGURATION	(int)(0x80830036)	///< Error code for invalid parameter in configuration; error code.
#define SCE_SLED_ERROR_NULLPARAMETER		(int)(0x80830037)	///< Error code for null parameter to function; error code.
#define SCE_SLED_ERROR_INVALIDPARAMETER		(int)(0x80830038)	///< Error code for invalid parameter to function; error code.
#define SCE_SLED_ERROR_NOCLIENTCONNECTED	(int)(0x80830039)	///< Error code for no client connected; error code.
#define SCE_SLED_ERROR_NOTALIGNED			(int)(0x80830040)	///< Error code for invalid alignment.
#define SCE_SLED_ERROR_STAT                 (int)(0x80830041)	///< Error code. Invalid state.
#define SCE_SLED_ERROR_SRCH                 (int)(0x80830042)	///< Error code. No search.

#endif // __SCE_LIBSLEDDEBUGGER_ERRORCODES_H__
