-- Copyright (C) Sony Computer Entertainment America LLC. 
-- All Rights Reserved.

--
-- Premake build script for the sce_sleddebugger library
--

sdk_project "libsce_sleddebugger"
	sdk_location "."
	sdk_kind "Library"
	
	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }

	targetname "sce_sleddebugger"

	uuid "C3E72BF5-72EE-460B-AC15-1D1234130406"

	files {
		"*.h",
		"*.cpp",
		"../sledcore/*.h",
		"../sledcore/windows/*.h"
	}

	vpaths { ["Headers"] = {"*.h"} }
	vpaths { ["Source/*"] = {"**.h", "**.cpp"} }

	includedirs { "../../", "../../../" }
	
	objdir(path.join(sdk.rootdir, "tmp/sce_sled/%{prj.name}/%{sdk.platform(cfg)}"))

	configuration { "Debug*" }
		defines { "SCE_SLED_ASSERT_ENABLED=1" }
