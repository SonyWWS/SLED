-- Copyright (C) Sony Computer Entertainment America LLC. 
-- All Rights Reserved. 

--
-- Dependencies
--

include "../../wws_lua/lua-5.1.4/src/lua-5.1.4.lua"

--
-- Premake build script for the libsce_sledluaplugin-5.1.4 library
--

sdk_project "libsce_sledluaplugin-5.1.4"
	sdk_location "."
	sdk_kind "Library"

	targetname "sce_sledluaplugin-5.1.4"

	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }

	uuid "C3E72BF5-432B-460B-AC15-1EB234123416"

	files {		
		"errorcodes.h",
		"luautils.h",
		"luautils_5.1.4.cpp",
		"luautils_common.cpp",
		"luavariable.h",
		"params.h",
		"profilestack.*",
		"scmp.*",
		"sledluaplugin.*",
		"sledluaplugin_class.cpp",
		"sledluaplugin_class.h",		
		"sledluaplugin_class_5.1.4.cpp",
		"varfilter.*",
		"../sledcore/*.h",
		"../sledcore/windows/*.h"
	}

	vpaths { ["Headers"] = {"*.h"} }
	vpaths { ["Source/*"] = {"**.h", "**.cpp"} }

	includedirs { "../../", "../../../" }
	
	objdir(path.join(sdk.rootdir, "tmp/sce_sled/%{prj.name}/%{sdk.platform(cfg)}"))

	defines { "SCE_LUA_VER=514", "WWS_LUA_VER=514" }
	
	configuration { "Debug*" }
		defines { "SCE_SLED_ASSERT_ENABLED=1" }
		
	configuration { "Win*" }
		linkoptions { "/ignore:4221" }
