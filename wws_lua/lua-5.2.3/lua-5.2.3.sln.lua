-- Copyright (c) Sony Computer Entertainment America LLC.
-- All rights Reserved.

--
-- Premake build script for the lua-5.2.3 library solution
--

	include "../../build/premake/master.lua"

	sdk_solution "lua-5.2.3"
	
	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }

--
-- These projects make up this solution
--

	include "src/lua-5.2.3.lua"
	include "src/lua-5.2.3_compiler.lua"
