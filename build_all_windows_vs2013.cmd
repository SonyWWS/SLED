:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

SET DEVENV120="%VS120COMNTOOLS%..\IDE\devenv.com"
IF NOT EXIST %DEVENV120% (
echo Visual Studio 2013 not found, skipping
goto end
)

echo.
echo ---- Building VS2013 runtime...
echo.

pushd "%~dp0"
(
call build_premake_config_vs.cmd %DEVENV120% "Win32 Static DCRT" vs2013 && ^
call build_premake_config_vs.cmd %DEVENV120% "Win32 Static SCRT" vs2013 && ^
call build_premake_config_vs.cmd %DEVENV120% "Win64 Static DCRT" vs2013 && ^
call build_premake_config_vs.cmd %DEVENV120% "Win64 Static SCRT" vs2013
) || (echo. & echo **** Error building VS2013 runtime. & popd & exit /b 1)
popd

echo.
echo ---- Build of VS2013 runtime succeeded.

:end

exit /b 0
