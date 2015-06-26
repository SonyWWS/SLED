/*
 * Copyright (C) Sony Computer Entertainment America LLC. 
 * All Rights Reserved. 
 */

#ifndef SCE_SLEDPLATFORM_SOCKET_WINDOWS_H
#define SCE_SLEDPLATFORM_SOCKET_WINDOWS_H

#include <memory.h>
#include <tchar.h>

#include <winsock2.h>

#if WINVER >= 0x0600

#include <Ws2tcpip.h>
#define SCE_SLEDTARGET_SUPPORTS_IPV6    1

#endif  /* WINVER >= 0x0600 */

typedef SOCKADDR_STORAGE SceSledPlatformSocketAddress;
typedef SOCKET SceSledPlatformSocket;
typedef struct hostent SceSledPlatformSocketHostent;

#endif