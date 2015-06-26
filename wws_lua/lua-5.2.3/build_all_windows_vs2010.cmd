:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

SET DEVENV100="%VS100COMNTOOLS%..\IDE\devenv.com"
IF NOT EXIST %DEVENV100% (
echo Visual Studio 2010 not found, skipping
goto end
)

echo.
echo ---- Building VS2010 wws_lua...
echo.

pushd "%~dp0"
(
call build_premake_config_vs.cmd %DEVENV100% "Win32 Static DCRT" vs2010 && ^
call build_premake_config_vs.cmd %DEVENV100% "Win32 Static SCRT" vs2010 && ^
call build_premake_config_vs.cmd %DEVENV100% "Win64 Static DCRT" vs2010 && ^
call build_premake_config_vs.cmd %DEVENV100% "Win64 Static SCRT" vs2010
) || (echo. & echo **** Error building VS2010 wws_lua. & popd & exit /b 1)
popd

echo.
echo ---- Build of VS2010 wws_lua succeeded.

:end

exit /b 0
