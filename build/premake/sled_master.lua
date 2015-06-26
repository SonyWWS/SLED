-- Copyright (c) Sony Computer Entertainment America LLC.
-- All rights Reserved.

-- -------------------------------------------------------------------------------------------------------------------------------------
-- master script for sce_sled premake config files - include to use helper functions
-- -------------------------------------------------------------------------------------------------------------------------------------

-- ---------------------------------------------------------------------------------------------------------------------------------
-- Convenience Functions
-- ---------------------------------------------------------------------------------------------------------------------------------

function get_project_path()
	if _ACTION == "vs2008ng" or _ACTION == "vs2008" or _ACTION == "vs2010ng" or _ACTION == "vs2010" or _ACTION == "vs2013ng" or _ACTION == "vs2013" then
		return "$(ProjectDir)";
	end		
	return " ";
end

function get_target_path()	
	if _ACTION == "vs2008ng" or _ACTION == "vs2008" or _ACTION == "vs2010ng" or _ACTION == "vs2010" or _ACTION == "vs2013ng" or _ACTION == "vs2013" then
		return "$(OutDir)";
	end		
	return " ";
end

function get_target_pathname()	
	if _ACTION == "vs2008ng" or _ACTION == "vs2008" or _ACTION == "vs2010ng" or _ACTION == "vs2010" or _ACTION == "vs2013ng" or _ACTION == "vs2013" then
		return "$(TargetPath)";
	end		
	return " ";
end

function get_sledcore_pathname()
	if _ACTION == "vs2008ng" or _ACTION == "vs2008" or _ACTION == "vs2010ng" or _ACTION == "vs2010" or _ACTION == "vs2013ng" or _ACTION == "vs2013" then
		return "\"" .. get_project_path() .. "..\\sledcore\\lib\\%{sdk.platform(cfg)}\\libsce_sledcore\"";
	end		
	return " ";
end
