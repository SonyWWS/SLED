-- Copyright (c) Sony Computer Entertainment America LLC.
-- All rights Reserved.

--
-- Master Premake configuration script, included by all solution scripts.
--
	

	sdk = {}


--
-- Locate the top of the SDK source tree, making the hardcoded assumption
-- that it is two levels above the location of this script.
--

	sdk.rootdir = path.getabsolute("../..")


--
-- Well-known locations in the tree.
--

	sdk.bindir = path.join(sdk.rootdir, "bin/%{sdk.platform(cfg)}")
	sdk.libdir = path.join(sdk.rootdir, "lib/%{sdk.platform(cfg)}")
	sdk.objdir = path.join(sdk.rootdir, "tmp/%{sdk.platform(cfg)}/%{prj.name}")


--
-- Sony-specific configurations (for use with the configurations function).
--
	sdk.DEBUG = "Debug"
	sdk.DEVELOPMENT = "Development"
	sdk.PROFILE = "Profile"
	sdk.RELEASE = "Release"


--
-- Sony-specific platforms (for use with the platforms function).
--
	sdk.WIN32_DLL_DCRT = "Win32 DLL DCRT"
	sdk.WIN32_STATIC_DCRT = "Win32 Static DCRT"
	sdk.WIN32_STATIC_SCRT = "Win32 Static SCRT"
	sdk.WIN64_DLL_DCRT = "Win64 DLL DCRT"
	sdk.WIN64_STATIC_DCRT = "Win64 Static DCRT"
	sdk.WIN64_STATIC_SCRT = "Win64 Static SCRT"



	filter { "platforms:Win32 * or Win64 *" }
		system "Windows"

	filter { "platforms:Win32 *" }
		architecture "x32"

	filter { "platforms:Win64 *" }
		architecture "x64"


--
-- Extend Premake's solution function to configure the default set of build
-- configurations and platforms.
--

	function sdk_solution(name)
		local action = sdk.cleanaction()

		local sln = solution(name)
		filename (name .. "_" .. (action or ""))

		return sln
	end


--
-- Extend Premake's project function to provide the default SDK configuration,
-- such as output and include directories.
--

	function sdk_project(name)
		local prj = project(name)

		-- Prevent collisions between projects with the same file extensions.
		-- VS 2008 and 2010 got there first, so those remain undecorated. All
		-- other versions get a version suffix.

		local action = sdk.cleanaction()
		if action:startswith("vs") and action ~= "vs2008" and action ~= "vs2010" then
			if not prj.name:find(action) then
				filename (name .. "_" .. action)
			end
		end

		-- Remove all solution configurations and platforms; start with a clean slate.
		removeconfigurations { "*" }
		removeplatforms { "*" }

		-- The default location for an sdk project is in a /build subdirectory
		-- in the same folder as the source .lua file.  This can be changed
		-- via sdk_build_location (to move the /build directory), or sdk_location
		-- (to remove it all together but preserve Imogen .target file locations).
		sdk_build_location(".")

		premake.api.deprecations("off")

		language "C++"
		flags { "FatalWarnings", "NoRuntimeChecks" }
		warnings "Extra"

		-- set the default output paths for all projects
		targetdir(sdk.bindir)
		objdir(sdk.objdir)

		-- The /component root folder is always included
		--includedirs { path.join(sdk.rootdir, "components") }

		filter { "kind:SharedLib" }
			implibdir(sdk.libdir)

		filter { "kind:StaticLib" }
			targetdir(sdk.libdir)

		-- Avoid link issues on build server; see PREMAKE-15
		filter { "action:vs2008" }
			flags "NoImplicitLink"

		-- Describe the build configurations
		filter { "Debug" }
			flags { "Symbols" }
			defines { "_DEBUG", "DEBUG", "WWS_BUILD_DEBUG", "WWS_ASSERTS_ENABLED=1", "WWS_MINIMUM_LOG_LEVEL_COMPILE_TIME=kWwsLogDebug" }

		filter { "Development" }
			flags { "Symbols" }
			defines { "NDEBUG", "WWS_BUILD_DEVELOPMENT", "WWS_ASSERTS_ENABLED=1", "WWS_MINIMUM_LOG_LEVEL_COMPILE_TIME=kWwsLogDebug" }
			optimize "Speed"

		filter { "Profile" }
			flags { "Symbols" }
			defines { "NDEBUG", "WWS_BUILD_PROFILE", "WWS_ASSERTS_ENABLED=0", "WWS_MINIMUM_LOG_LEVEL_COMPILE_TIME=kWwsLogDisable" }
			optimize "Speed"

		filter { "Release" }
			flags { "Symbols" }
			defines { "NDEBUG", "WWS_BUILD_RELEASE", "WWS_ASSERTS_ENABLED=0", "WWS_MINIMUM_LOG_LEVEL_COMPILE_TIME=kWwsLogDisable" }
			optimize "Speed"

		-- Windows configuration

		filter { "platforms:Win32 * or Win64 *" }
			flags { "NoIncrementalLink" }
			defines { "WIN32", "_WIN32", "_WINDOWS", "_MBCS", "_CRT_SECURE_NO_WARNINGS", "_CRT_SECURE_NO_DEPRECATE" }
			debugformat "C7"

		filter { "platforms:Win64 *" }
			defines { "WIN64", "_WIN64" }

		filter { "platforms:* DLL *" }
			defines { "WWS_TARGET_LINK_DYNAMIC" }

		filter { "platforms:* SCRT" }
			flags { "StaticRuntime" }

		filter { "kind:SharedLib", "platforms:* DLL *" }
			defines { "_USRDLL", "_WINDLL" }

		filter { "kind:StaticLib or SharedLib", "system:Windows" }
			targetprefix("lib")

		-- Clear context and done

		premake.api.deprecations(iif(_OPTIONS.fataldeprecations, "error", "on"))

		filter {}

		return prj
	end


--
-- Sets the location for the project in the form of /build/{project name}.
-- The loc parameter should be the desired location of the /build directory
-- relative to the source .lua file.
--

	function sdk_build_location(loc)
		loc = path.getabsolute(path.join(_SCRIPT_DIR, loc))
		loc = path.join(loc, "build/%{prj.name}")
		sdk_location(loc)
	end
	
	
--
-- Extend Premake's location function to properly handle Imogen .target file
-- location.  The loc parameter should be the desired location of the project
-- file relative to the source .lua file.
--

	function sdk_location(loc)
		location(loc)
	end


--
-- Extend Premake's kind function to provide target specific configuration values,
-- like defines and compiler flags.
--

	function sdk_kind(value)
		value = value:lower()

		if value == "library" then
			kind("StaticLib")
			filter { "platforms:* DLL *" }
				kind("SharedLib")
		else
			kind(value)
		end

		filter {}
	end


--
-- Translate Premake's action name into the appropriate Imogen compatible version.
--

	function sdk.imogenaction()
		local action = sdk.cleanaction()
		local map = {
			vs2005 = "vc80",
			vs2008 = "vc90",
			vs2010 = "vc100",
			vs2012 = "vc110",
			vs2013 = "vc120",
			}
		return (map[action] or "")
	end


--
-- Translates the human-readable Premake platform into the right Imogen
-- compatible replacement.
--

	function sdk.platform(cfg)
		local action = sdk.cleanaction()

		-- convert spaces to underscores and make lowercase
		local platform = cfg.platform or ""
		platform = platform:gsub(" ", "_")
		platform = platform:lower()

		platform = platform .. "_" .. sdk.imogenaction()

		if cfg.buildcfg then
			platform = platform .. "_" .. cfg.buildcfg:lower()
		end

		return platform
	end
	

--
-- Returns the action name without any 'ng' suffix
-- TODO: this can go away once everyone is switched off the old names
--

	function sdk.cleanaction()
		local action = _ACTION
		if action:endswith("ng") then
			action = action:sub(1, -3)
		end
		return action
	end


--
-- Check for a custom configuration script at the top of the SDK tree.
--

	local local_config = "../../local_config.lua"

	if os.isfile(local_config) then
		include(local_config)
	end


--
-- Common platform groups (for use with the platforms function). Needs to be
-- built after customization script has run in case any of the configurations
-- or platforms are turned off there.
--

	sdk.CONFIGS_ALL = { sdk.DEBUG, sdk.DEVELOPMENT, sdk.PROFILE, sdk.RELEASE }

	sdk.WIN32_ALL = { sdk.WIN32_DLL_DCRT, sdk.WIN32_STATIC_DCRT, sdk.WIN32_STATIC_SCRT }
	sdk.WIN64_ALL = { sdk.WIN64_DLL_DCRT, sdk.WIN64_STATIC_DCRT, sdk.WIN64_STATIC_SCRT }
	sdk.WIN_DLL_ALL = { sdk.WIN32_DLL_DCRT, sdk.WIN64_DLL_DCRT }
	sdk.WIN_STATIC_ALL = { sdk.WIN32_STATIC_DCRT, sdk.WIN32_STATIC_SCRT, sdk.WIN64_STATIC_DCRT, sdk.WIN64_STATIC_SCRT }
	sdk.WIN_SCRT_ALL = { sdk.WIN32_STATIC_SCRT, sdk.WIN64_STATIC_SCRT }
	sdk.WIN_DCRT_ALL = { sdk.WIN32_STATIC_DCRT, sdk.WIN64_STATIC_DCRT }
	sdk.WIN_ALL = { sdk.WIN32_ALL, sdk.WIN64_ALL }

	sdk.PLATFORMS_ALL = { sdk.WIN_ALL }


--
-- Allow the configuration and platform lists to be overridden on the command
-- line. Processed after the groups are built so those symbols can be used
-- as part of the argument lists.
--

	newoption {
		trigger = "configs",
		value = "symbols",
		description = "Choose a subset of build configurations",
	}

	newoption {
		trigger = "platforms",
		value = "symbols",
		description = "Choose a subset of build platforms",
	}

	function sdk.customize(defaultList, userList)
		if userList then
			-- get a flat list of all possible values
			local possible = table.flatten(defaultList)

			-- turn user's list into a flat list of allowed values
			userList = userList:explode(",")

			local allowed = {}
			table.foreachi(userList, function(symbol)
				local item = sdk[symbol:upper()]
				if not item then
					error(string.format("No such configuration '%s'", symbol), 0)
				end
				if type(item) == "table" then
					allowed = table.join(allowed, table.flatten(item))
				else
					table.insert(allowed, item)
				end
			end)

			-- set all disallowed SDK configuration tokens to nil
			for key, value in pairs(sdk) do
				if table.contains(possible, value) and not table.contains(allowed, value) then
					sdk[key] = nil
				end
			end

			-- remove disallowed symbols from all configuration groups
			function purge(list)
				local n = #list
				for i = n, 1, -1 do
					local item = list[i]
					if type(item) == "table" then
						purge(item)
					elseif item then
						if not table.contains(allowed, item) then
							table.remove(list, i)
						end
					end
				end
			end

			purge(defaultList)
		end
	end


	sdk.customize(sdk.CONFIGS_ALL, _OPTIONS.configs)
	sdk.customize(sdk.PLATFORMS_ALL, _OPTIONS.platforms)


--
-- Command line argument to treat deprecation warnings as errors, to make it
-- a little easier to track them down.
--

	newoption {
		trigger = "fataldeprecations",
		description = "Treat deprecation warnings as errors",
	}


--
-- Add the wws_premake version information to generated projects.
--

	_WWS_PREMAKE_VERSION = _WWS_PREMAKE_VERSION or os.getenv("_WWS_PREMAKE_VERSION")
	if _WWS_PREMAKE_VERSION == "" or _WWS_PREMAKE_VERSION == "no" then
		_WWS_PREMAKE_VERSION = false
	end

	if _WWS_PREMAKE_VERSION == nil then
		_WWS_PREMAKE_VERSION = "wws_premake"

		-- grab the component version
		local f = io.open("wws_premake.component")
		if f then
			local t = f:read("*all")
			t:gsub("<version>(.-)</version>", function(c)
				_WWS_PREMAKE_VERSION = _WWS_PREMAKE_VERSION .. " " .. c
			end, 1)
			f:close()
		end

		-- see if I can grab SVN information too. Requires a command line client
		local br, rev
		local i = os.outputof('svn info .')
		i:gsub("Relative URL: (.-)\n", function(c) br = c end)
		i:gsub("Last Changed Rev: (.-)\n", function(c) rev = c end)
		if br and rev then
			i = string.format(" (SVN %s r%s)", br, rev)
			_WWS_PREMAKE_VERSION = _WWS_PREMAKE_VERSION .. i
		end
	end

	local function xmlDeclaration(base)
		base()
		if _WWS_PREMAKE_VERSION then
			_p('<!-- %s -->', _WWS_PREMAKE_VERSION)
		end
	end

	if _ACTION ~= "test" then
		premake.override(premake.vstudio.vc2010, "xmlDeclaration", xmlDeclaration)
		premake.override(premake.vstudio.cs2005, "xmlDeclaration", xmlDeclaration)

		premake.override(premake.make, "header", function(base, target)
			if _WWS_PREMAKE_VERSION then
				_p('# %s', _WWS_PREMAKE_VERSION)
			end
			base(target)
		end)
	end
