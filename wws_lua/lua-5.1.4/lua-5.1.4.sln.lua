-- Copyright (c) Sony Computer Entertainment America LLC.
-- All rights Reserved.

--
-- Premake build script for the lua-5.1.4 library solution
--

	include "../../build/premake/master.lua"

	sdk_solution "lua-5.1.4"
	
	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }

--
-- These projects make up this solution
--

	include "src/lua-5.1.4.lua"
	include "src/lua-5.1.4_compiler.lua"
	