-- Copyright (c) Sony Computer Entertainment America LLC.
-- All rights Reserved.

--
-- Premake build script for the lua 5.1.4 library
--

sdk_project "lua-5.1.4"
	sdk_build_location ".."
	sdk_kind "Library"
	
	uuid "C3234523-74BB-4EEB-AC15-1D1234130406"
	
	language "C"
	
	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }
	
	defines { "WWS_LUA_VER=514", "SCE_LUA_VER=514" }
		
	files {
		"*.h",
		"lapi.c",
		"lauxlib.c",
		"lbaselib.c",
		"lcode.c",
		"ldblib.c",
		"ldebug.c",
		"ldo.c",
		"ldump.c",
		"lfunc.c",
		"lgc.c",
		"linit.c",
		"liolib.c",
		"llex.c",
		"lmathlib.c",
		"lmem.c",
		"loadlib.c",
		"lobject.c",
		"lopcodes.c",
		"loslib.c",
		"lparser.c",
		"lstate.c",
		"lstring.c",
		"lstrlib.c",
		"ltable.c",
		"ltablib.c",
		"ltm.c",
		"lundump.c",
		"lvm.c",
		"lzio.c"
	}
	
	vpaths { ["Headers"] = "**.h" }
	vpaths { ["Source"] = "**.c" }
	
	objdir(path.join(sdk.rootdir, "tmp/wws_lua/%{prj.name}/%{sdk.platform(cfg)}"))
	
	configuration { "Debug*" }
		defines { "LUA_USE_APICHECK" }
	
	configuration { "Win32*" }
		buildoptions { "/wd4709" }

	configuration { "Win64*" }
		buildoptions { "/wd4324", "/wd4709" }
