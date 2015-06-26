-- Copyright (c) Sony Computer Entertainment America LLC.
-- All rights Reserved.

--
-- Premake build script for the sce_sled runtime solution
--

	include "build/premake/master.lua"

	sdk_solution "runtime"

	configurations { sdk.DEBUG, sdk.RELEASE }
	platforms { sdk.WIN_STATIC_ALL }

--
-- These projects make up this solution
--

	include "src/sleddebugger/libsce_sleddebugger.lua"
	include "src/sleddebugger_unittests/libsce_sleddebugger_unittests.lua"
	include "src/sledluaplugin/libsce_sledluaplugin-5.1.4.lua"
	include "src/sledluaplugin/libsce_sledluaplugin-5.2.3.lua"
	include "src/sledluaplugin_unittests/libsce_sledluaplugin-5.1.4_unittests.lua"
	include "src/sledluaplugin_unittests/libsce_sledluaplugin-5.2.3_unittests.lua"
	include "src/sledluaplugin_TestSimple/libsce_testsimple-5.1.4_sample.lua"
	include "src/sledluaplugin_TestTarget/libsce_testtarget-5.1.4_sample.lua"
	include "src/sledluaplugin_TestTarget/libsce_testtarget-5.2.3_sample.lua"
