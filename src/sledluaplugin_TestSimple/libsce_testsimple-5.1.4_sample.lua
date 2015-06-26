-- Copyright (C) Sony Computer Entertainment America LLC. 
-- All Rights Reserved. 

--
-- Dependencies
--

include "../sledluaplugin/libsce_sledluaplugin-5.1.4.lua"
include "../sleddebugger/libsce_sleddebugger.lua"

--
-- Premake build script for the sce simple test sample
--

sdk_project "libsce_testsimple-5.1.4_sample"
	sdk_location "."
	sdk_kind "ConsoleApp"

	uuid "1223423A-2390-43F3-B433-B232456FECAB"

	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }

	files { 
		"*.cpp", 
		"../sleddebugger_unittests/scoped_network.*",
		"../sledluaplugin_SharedSampleCode/fileutilities.*",
		"../sledcore/*.h",
		"../sledcore/windows/*.h"
	}
	
	vpaths {
		["Headers"] = {
			"**.h",
			"../sleddebugger_unittests/**.h",
			"../sledluaplugin_SharedSampleCode/**.h" } }
	vpaths {
		["Source"] = {
			"**.cpp",
			"../sleddebugger_unittests/**.cpp",
			"../sledluaplugin_SharedSampleCode/**.cpp" } }
	
	includedirs { "../../", "../../../" }

	links { "libsce_sledluaplugin-5.1.4", "lua-5.1.4", "libsce_sleddebugger" }
	
	objdir(path.join(sdk.rootdir, "tmp/sce_sled/%{prj.name}/%{sdk.platform(cfg)}"))
	targetdir(path.join(sdk.rootdir, "bin/sce_sled/runtime/%{sdk.platform(cfg)}"))

	defines { "SCE_LUA_VER=514", "WWS_LUA_VER=514" }
	
	configuration { "Debug*" }
		defines { "SCE_SLED_ASSERT_ENABLED=1" }

	configuration { "Win*" }
		links { "ws2_32", get_sledcore_pathname() }
