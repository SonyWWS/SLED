-- Copyright (c) Sony Computer Entertainment America LLC.
-- All rights Reserved.

--
-- Dependencies
--

include "lua-5.2.3.lua"

--
-- Premake build script for the lua 5.2.3 library
--

sdk_project "lua-5.2.3_compiler"
	sdk_build_location ".."
	sdk_kind "ConsoleApp"
	
	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }
	
	uuid "7FABCDEF-1234-4321-CCDE-CABCD1233EFA"
	
	language "C"
	
	defines { "WWS_LUA_VER=523", "SCE_LUA_VER=523" }
	
	files {
		"luac.c"
	}

	links { "lua-5.2.3" }
	
	objdir(path.join(sdk.rootdir, "tmp/wws_lua/%{prj.name}/%{sdk.platform(cfg)}"))
	targetdir(path.join(sdk.rootdir, "bin/wws_lua/%{sdk.platform(cfg)}"))
	
	vpaths { ["Source"] = "**.c" }
