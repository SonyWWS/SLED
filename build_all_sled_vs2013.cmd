:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off
setlocal
setlocal ENABLEDELAYEDEXPANSION


SET DEVENV120="%VS120COMNTOOLS%..\ide\devenv.com"
IF NOT EXIST %DEVENV120% (
echo devenv[2013] not found at %DEVENV120%, aborting
exit /B 2
)


echo.
echo ------------------------------
echo ---- Building SLED VS2013 ----
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
!DEVENV120! "tool_vs2013.sln" /Clean !SLED_DEBUG_CONFIGURATION! && ^
!DEVENV120! "tool_vs2013.sln" /Clean !SLED_RELEASE_CONFIGURATION! && ^
!DEVENV120! "tool_vs2013.sln" /Build !SLED_DEBUG_CONFIGURATION! && ^
!DEVENV120! "tool_vs2013.sln" /Build !SLED_RELEASE_CONFIGURATION!
) || (echo. & echo **** Error building SLED VS2013. & popd & exit /b 1)
popd


:END
echo.
echo ----------------------------------------
echo ---- Building SLED Succeeded VS2013 ----
echo ----------------------------------------
echo.


exit /b 0
