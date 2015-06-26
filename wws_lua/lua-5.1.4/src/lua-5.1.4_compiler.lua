-- Copyright (c) Sony Computer Entertainment America LLC.
-- All rights Reserved.

--
-- Dependencies
--

include "lua-5.1.4.lua"

--
-- Premake build script for the lua 5.1.4 library
--

sdk_project "lua-5.1.4_compiler"
	sdk_build_location ".."
	sdk_kind "ConsoleApp"
	
	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }
	
	uuid "7F86714E-EA4D-43CF-9C3A-C42AC4924EFA"
	
	language "C"
	
	defines { "WWS_LUA_VER=514", "SCE_LUA_VER=514" }
	
	files {
		"luac.c",
		"print.c"
	}

	links { "lua-5.1.4" }
	
	objdir(path.join(sdk.rootdir, "tmp/wws_lua/%{prj.name}/%{sdk.platform(cfg)}"))
	targetdir(path.join(sdk.rootdir, "bin/wws_lua/%{sdk.platform(cfg)}"))
	
	vpaths { ["Source"] = "**.c" }