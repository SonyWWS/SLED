:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

REM Remove quotes from the build configuration.
SET _config=###%2%###
SET _config=%_config:"###=%
SET _config=%_config:###"=%
SET _config=%_config:###=%

echo.
echo ---- Building lua-5.1.4 %_config% (%3)...
echo.

pushd "%~dp0"
(
call %1 /Build "Debug|%_config%" lua-5.1.4_%3.sln && ^
call %1 /Build "Release|%_config%" lua-5.1.4_%3.sln
) || (echo. & echo **** Error building lua-5.1.4. & popd & exit /b 1)
popd

echo.
echo ---- Build succeeded for lua-5.1.4 %_config% (%3).

exit /b 0
