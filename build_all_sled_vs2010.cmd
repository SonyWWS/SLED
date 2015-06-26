:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off
setlocal
setlocal ENABLEDELAYEDEXPANSION


SET DEVENV100="%VS100COMNTOOLS%..\ide\devenv.com"
IF NOT EXIST %DEVENV100% (
echo devenv[2010] not found at %DEVENV100%, aborting
exit /B 2
)


echo.
echo ------------------------------
echo ---- Building SLED VS2010 ----
echo ------------------------------
echo.


echo.
echo ==== Building SLED ====
echo.

SET SLED_DEBUG_CONFIGURATION="Debug"
SET SLED_RELEASE_CONFIGURATION="Release"
echo Debug Configuration: !SLED_DEBUG_CONFIGURATION!
echo Release Configuration: !SLED_RELEASE_CONFIGURATION!

pushd "%~dp0"
(
!DEVENV100! "tool_vs2010.sln" /Clean !SLED_DEBUG_CONFIGURATION! && ^
!DEVENV100! "tool_vs2010.sln" /Clean !SLED_RELEASE_CONFIGURATION! && ^
!DEVENV100! "tool_vs2010.sln" /Build !SLED_DEBUG_CONFIGURATION! && ^
!DEVENV100! "tool_vs2010.sln" /Build !SLED_RELEASE_CONFIGURATION!
) || (echo. & echo **** Error building SLED VS2010. & popd & exit /b 1)
popd


:END
echo.
echo ----------------------------------------
echo ---- Building SLED Succeeded VS2010 ----
echo ----------------------------------------
echo.


exit /b 0
