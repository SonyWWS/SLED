:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

SET DEVENV100="%VS100COMNTOOLS%..\IDE\devenv.com"
IF NOT EXIST %DEVENV100% (
echo Visual Studio 2010 not found, skipping
goto end
)

SET SOLUTION="DotNetUtilities.vs2010.sln"

echo.
echo ---- Cleaning VS 2010 wws_lua extras...
echo.

pushd "%~dp0"
(
call vs2010_runner.cmd clean %SOLUTION% "win32_dotnet_clr4_" "Any CPU" && ^
call vs2010_runner.cmd clean %SOLUTION% "win64_dotnet_clr4_" "Any CPU" 
) || (echo. & echo **** Error cleaning VS 2010 wws_lua extras. & popd & exit /b 1)
popd

echo.
echo ---- Clean of VS 2010 wws_lua extras succeeded.

:end

exit /b 0
