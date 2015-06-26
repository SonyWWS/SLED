-- Copyright (C) Sony Computer Entertainment America LLC. 
-- All Rights Reserved. 

--
-- Dependencies
--

include "../sleddebugger/libsce_sleddebugger.lua"
include "../../build/premake/sled_master.lua"

--
-- Premake build script for the libsce_sleddebugger unit tests
--

sdk_project "libsce_sleddebugger_unittests"
	sdk_location "."
	sdk_kind "ConsoleApp"

	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }

	uuid "12343516-CCDB-4230-B433-B01234342322"

	files {
		"*.h",
		"*.cpp",
		"../../../unittest-cpp/UnitTest++/*.h",
		"../../../unittest-cpp/UnitTest++/*.cpp",
		"../../../unittest-cpp/UnitTest++/Win32/*.h",
		"../../../unittest-cpp/UnitTest++/Win32/*.cpp",
		"../sledcore/*.h",
		"../sledcore/windows/*.h"
	}

	vpaths { ["Headers"] = {"**.h"} }
	vpaths { ["Source"] = {"**.cpp"} }
	
	includedirs { "../../", "../../../" }

	links { "libsce_sleddebugger" }	
	
	objdir(path.join(sdk.rootdir, "tmp/sce_sled/%{prj.name}/%{sdk.platform(cfg)}"))
	targetdir(path.join(sdk.rootdir, "tmp/sce_sled/%{prj.name}/%{sdk.platform(cfg)}"))
	
	defines { "UNITTEST_RUN_STDOUT_AND_XML", "UNITTEST_NO_DEFERRED_REPORTER" }
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
