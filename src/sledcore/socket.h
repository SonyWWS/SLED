/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef _SCE_SLEDPLATFORM_SOCKET_H
#define _SCE_SLEDPLATFORM_SOCKET_H

#include "common.h"
#include "datetime.h"

/* Types */
#if SCE_SLEDTARGET_OS_WINDOWS
#include "windows/socket_windows.h"
#else
#error "SceSledPlatformSocket has not been implemented for this platform!"
#endif

#ifdef __cplusplus
extern "C" {
#endif

/* Network errors */
typedef enum {
    SCE_SLEDPLATFORM_NETWORK_ERROR_NONE                         = -2000,

    SCE_SLEDPLATFORM_NETWORK_ERROR_MODULE_ALREADY_LOADED        = -1999,
    SCE_SLEDPLATFORM_NETWORK_ERROR_MODULE_INVALID_ID            = -1998,
    SCE_SLEDPLATFORM_NETWORK_ERROR_MODULE_NOT_LOADED            = -1997,
    SCE_SLEDPLATFORM_NETWORK_ERROR_NET_CORE_INTERFACE_BUSY      = -1996,
    SCE_SLEDPLATFORM_NETWORK_ERROR_NET_CORE_NOT_TERMINATED      = -1995,
    SCE_SLEDPLATFORM_NETWORK_ERROR_NET_INET_NOT_TERMINATED      = -1994,
    SCE_SLEDPLATFORM_NETWORK_ERROR_NET_INET_SET_IFADDR          = -1993,
    SCE_SLEDPLATFORM_NETWORK_ERROR_NET_INET_SOCKET_BUSY         = -1992,
    SCE_SLEDPLATFORM_NETWORK_ERROR_NET_RESOLVER_NOT_TERMINATED  = -1991,
    SCE_SLEDPLATFORM_NETWORK_ERROR_NOTINITIALISED               = -1990,
    SCE_SLEDPLATFORM_NETWORK_ERROR_SYSNOTREADY                  = -1989,
    SCE_SLEDPLATFORM_NETWORK_ERROR_VERNOTSUPPORTED              = -1988,

    SCE_SLEDPLATFORM_NETWORK_ERROR_UNKNOWN                      = -1987

} SceSledPlatformNetworkError;

/* Socket errors */
typedef enum {
    SCE_SLEDPLATFORM_SOCKET_ERROR_NONE                      = 0,

    SCE_SLEDPLATFORM_SOCKET_ERROR_EACCES                    = -1000,
    SCE_SLEDPLATFORM_SOCKET_ERROR_EADDRINUSE                = -999, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EADDRNOTAVAIL             = -998, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EAFNOSUPPORT              = -997, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EALREADY                  = -996, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EBADF                     = -995, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ECATCHALL                 = -994, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ECONNABORTED              = -993, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ECONNREFUSED              = -992, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ECONNRESET                = -991, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EDESTADDRREQ              = -990, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EFAULT                    = -989, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EHOSTUNREACH              = -988, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EINTR                     = -987, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EINVAL                    = -986, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EIPADDRCHANGE             = -985,
    SCE_SLEDPLATFORM_SOCKET_ERROR_EISCONN                   = -984, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EMFILE                    = -983, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EMSGSIZE                  = -982, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ENETDOWN                  = -981, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ENETRESET                 = -980, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ENETUNREACH               = -979, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ENOBUFS                   = -978, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ENOPROTOOPT               = -977, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ENOTCONN                  = -976, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ENOTSOCK                  = -975, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EOPNOTSUPP                = -974, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EPIPE                     = -973, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EPROTONOSUPPORT           = -972, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_ETIMEDOUT                 = -971, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_EWOULDBLOCK               = -970, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_CTX_BUSY                  = -969,
    SCE_SLEDPLATFORM_SOCKET_ERROR_HOST_NOT_FOUND            = -968,
    SCE_SLEDPLATFORM_SOCKET_ERROR_INADDR_NONE               = -967, 
    SCE_SLEDPLATFORM_SOCKET_ERROR_NO_DATA                   = -966,
    SCE_SLEDPLATFORM_SOCKET_ERROR_NO_DNS_SERVER             = -965,
    SCE_SLEDPLATFORM_SOCKET_ERROR_NO_RECOVERY               = -964,
    SCE_SLEDPLATFORM_SOCKET_ERROR_NOTINITIALISED            = -963,
    SCE_SLEDPLATFORM_SOCKET_ERROR_TRY_AGAIN                 = -962,

    SCE_SLEDPLATFORM_SOCKET_ERROR_UNKNOWN                   = -961

} SceSledPlatformSocketError;


/** \brief Address family types. SceSledPlatformSocket supports IPV4 on all platforms and IPV6 on platforms that allow it. */
typedef enum {
    SCE_SLEDPLATFORM_SOCKET_AF_INET  = 0
} SceSledPlatformSocketAddressFamilyType;

/** \brief Protocol types. SceSledPlatformSocket supports TCP and UDP sockets. */
typedef enum {
    SCE_SLEDPLATFORM_SOCKET_PROTOCOL_TCP     = 0,
} SceSledPlatformSocketProtocolType;

/** \brief Socket shutdown types. For use with sceSledPlatformSocketShutdown. */
typedef enum {
    SCE_SLEDPLATFORM_SOCKET_SHUTDOWN_RECEIVE = 0,
    SCE_SLEDPLATFORM_SOCKET_SHUTDOWN_SEND    ,
    SCE_SLEDPLATFORM_SOCKET_SHUTDOWN_BOTH
} SceSledPlatformSocketShutdownType;

/** \brief Socket select types. For use with sceSledPlatformSocketSelect. */
typedef enum {
    SCE_SLEDPLATFORM_SOCKET_SELECT_READ      = 1,
    SCE_SLEDPLATFORM_SOCKET_SELECT_WRITE     = 2,
    SCE_SLEDPLATFORM_SOCKET_SELECT_EXCEPT    = 4
} SceSledPlatformSocketSelectType;


extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformNetworkError sceSledPlatformNetworkInitialize ();
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformNetworkError sceSledPlatformNetworkTerminate ();
extern SCE_SLEDPLATFORM_LINKAGE bool sceSledPlatformNetworkGetInitialized ();
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketSocket (SceSledPlatformSocketAddressFamilyType addressFamily, SceSledPlatformSocketProtocolType protocol, SceSledPlatformSocket * outSocket);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketShutdown (SceSledPlatformSocket socket, SceSledPlatformSocketShutdownType type);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketClose (SceSledPlatformSocket socket);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketBind (SceSledPlatformSocket socket, const SceSledPlatformSocketAddress * address);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketListen (SceSledPlatformSocket socket, int32_t backlog);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketAccept (SceSledPlatformSocket socket, SceSledPlatformSocket * outSocket, SceSledPlatformSocketAddress * outAddress);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketConnect (SceSledPlatformSocket socket, const SceSledPlatformSocketAddress * address);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketSelect (SceSledPlatformSocket socket, uint32_t type, int32_t timeoutSec, int32_t timeoutUsec, int32_t * outReady);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketSend (SceSledPlatformSocket socket, const void * buffer, int32_t length, int32_t flags, int32_t * outBytesSent);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketRecv (SceSledPlatformSocket socket, void * buffer, int32_t length, int32_t flags, int32_t * outBytesRecv);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketSetSockOpt (SceSledPlatformSocket socket, int32_t level, int32_t option, const void * value, int32_t length);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketGetSockOpt (SceSledPlatformSocket socket, int32_t level, int32_t option, void * outValue, int32_t * inoutLength);
extern SCE_SLEDPLATFORM_LINKAGE void sceSledPlatformSocketSetInvalid (SceSledPlatformSocket * socket);
extern SCE_SLEDPLATFORM_LINKAGE bool sceSledPlatformSocketIsInvalid (SceSledPlatformSocket socket);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketSetBlocking (SceSledPlatformSocket socket, bool blocking);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketGetLastError ();
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketGetError (SceSledPlatformSocket socket);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketInetAddr (const char * ipv4, uint32_t * outAddr);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketAddressInit (SceSledPlatformSocketAddress * address, SceSledPlatformSocketAddressFamilyType type);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketAddressSetIPV4Addr (SceSledPlatformSocketAddress * address, uint32_t addr);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketAddressSetPort (SceSledPlatformSocketAddress * address, uint16_t port);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketSetReuseAddr (SceSledPlatformSocket socket, bool reuseAddr);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformNetworkError sceSledPlatformNetworkTranslateError(int32_t nativeError);
extern SCE_SLEDPLATFORM_LINKAGE SceSledPlatformSocketError sceSledPlatformSocketTranslateError(int32_t nativeError);


#ifdef __cplusplus
}
#endif

#endif  // _SCE_SLEDPLATFORM_SOCKET_H
