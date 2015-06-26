-- Copyright (C) Sony Computer Entertainment America LLC. 
-- All Rights Reserved. 

--
-- Dependencies
--

include "../sledluaplugin/libsce_sledluaplugin-5.2.3.lua"
include "../sleddebugger/libsce_sleddebugger.lua"
include "../../build/premake/sled_master.lua"

--
-- Premake build script for the libsce_sleddebugger unit tests
--

sdk_project "libsce_sledluaplugin-5.2.3_unittests"
	sdk_location "."
	sdk_kind "ConsoleApp"

	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }

	uuid "124234EE-EED1-8329-B433-B01234118790"

	files { 
		"*.h", 
		"*.cpp", 
		"../sleddebugger_unittests/scoped_network.h",
		"../sleddebugger_unittests/scoped_network.cpp",
		"../../wws_lua/extras/Lua.Utilities/LuaInterface.h",
		"../../wws_lua/extras/Lua.Utilities/LuaInterface-5.2.3.cpp",
		"../../../unittest-cpp/UnitTest++/*.h",
		"../../../unittest-cpp/UnitTest++/*.cpp",
		"../../../unittest-cpp/UnitTest++/Win32/*.h",
		"../../../unittest-cpp/UnitTest++/Win32/*.cpp",
		"../sledcore/*.h",
		"../sledcore/windows/*.h"
	}
	
	vpaths { ["Headers"] = {"**.h", "../sleddebugger_unittests/**.h"} }
	vpaths { ["Source"] = {"**.cpp", "../sleddebugger_unittests/**.cpp"} }
	
	includedirs { "../../", "../../../" }

	links { "libsce_sledluaplugin-5.2.3", "lua-5.2.3", "libsce_sleddebugger" }
	
	objdir(path.join(sdk.rootdir, "tmp/sce_sled/%{prj.name}/%{sdk.platform(cfg)}"))
	targetdir(path.join(sdk.rootdir, "tmp/sce_sled/%{prj.name}/%{sdk.platform(cfg)}"))

	defines { "SCE_LUA_VER=523", "WWS_LUA_VER=523", "UNITTEST_XML_NAME=libsce_sledluaplugin-5.2.3_unittests" }
	defines { "UNITTEST_XML_SUITE=%{sdk.platform(cfg)}" }
	defines { "UNITTEST_XML_NAME=libsce_sleddebugger_unittests" }
	
	filter { "platforms:*DLL*" }
		defines { "UNITTEST_DLL_EXPORT" }	

	filter { "system:Windows" }
		files { "Win32/*" }
		defines { "UNITTEST_XML_PATH=./bin/%{sdk.platform(cfg)}/test-reports" }
		flags { "SEH" }
	
	configuration { "Debug*" }
		defines { "SCE_SLED_ASSERT_ENABLED=1" }

	configuration { "Win*" }
		links { "ws2_32", get_sledcore_pathname() }
		postbuildcommands { "\"" .. get_target_pathname(prj) .. "\"" }
