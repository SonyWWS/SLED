-- Copyright (C) Sony Computer Entertainment America LLC. 
-- All Rights Reserved. 

--
-- Dependencies
--

include "../sledluaplugin/libsce_sledluaplugin-5.2.3.lua"
include "../sleddebugger/libsce_sleddebugger.lua"
include "../../build/premake/sled_master.lua"

--
-- Premake build script for the sce target test sample
--

sdk_project "libsce_testtarget-5.2.3_sample"
	sdk_location "."
	sdk_kind "ConsoleApp"

	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }

	uuid "12FEFEFE-EED1-2BB0-B433-B0124356980A"

	files { 
		"*.cpp", 
		"../sleddebugger_unittests/scoped_network.h",
		"../sleddebugger_unittests/scoped_network.cpp",
		"../sledluaplugin_SharedSampleCode/fileutilities.*",
		"../sledluaplugin_SharedSampleCode/input.*",
		"../sledluaplugin_SharedSampleCode/luastuff.*",
		"../sledluaplugin/Extras/extras.*",
		"../sledluaplugin_SharedSampleCode/ScopedSledDebugger.h",
		"../sledluaplugin_SharedSampleCode/ScopedSledDebugger.cpp",
		"../sledluaplugin_SharedSampleCode/ScopedSledDebugger-5.2.3.cpp",
		"libsce_testtarget_5.2.3_sample.spf",
		"libsce_testtarget_script3.lua",
		"libsce_testtarget_script4.lua",
		"../sledcore/*.h",
		"../sledcore/windows/*.h"
	}

	vpaths {
		["Headers"] = {
			"**.h",
			"../sleddebugger/**.h",
			"../sleddebugger_unittests/**.h",
			"../sledluaplugin/**.h",
			"../sledluaplugin/Extras/**.h",
			"../sledluaplugin_SharedSampleCode/**.h" } }			
	vpaths {
		["Source"] = {
			"**.cpp", "**.lua", "**.spf",
			"../sleddebugger_unittests/**.cpp",
			"../sledluaplugin/Extras/**.cpp",
			"../sledluaplugin_SharedSampleCode/**.cpp" } }
			
	includedirs { "../../", "../../../" }

	links { "libsce_sledluaplugin-5.2.3", "lua-5.2.3", "libsce_sleddebugger" }
	
	objdir(path.join(sdk.rootdir, "tmp/sce_sled/%{prj.name}/%{sdk.platform(cfg)}"))
	targetdir(path.join(sdk.rootdir, "bin/sce_sled/runtime/%{sdk.platform(cfg)}"))

	defines { "SCE_LUA_VER=523", "WWS_LUA_VER=523" }

	postbuildcommands { "cmd.exe /c copy \"" .. get_project_path() .. "\\*script3.lua\" \"" .. get_target_path() .. "\" /y" }
	postbuildcommands { "cmd.exe /c copy \"" .. get_project_path() .. "\\*script4.lua\" \"" .. get_target_path() .. "\" /y" }
	postbuildcommands { "cmd.exe /c copy \"" .. get_project_path() .. "\\*5.2.3*.spf\" \"" .. get_target_path() .. "\" /y" }

	configuration { "Debug*" }
		defines { "SCE_SLED_ASSERT_ENABLED=1" }

	configuration { "Win*" }
		links { "ws2_32", get_sledcore_pathname() }
