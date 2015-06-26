:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

REM Remove quotes from the build configuration.
SET _config=###%2%###
SET _config=%_config:"###=%
SET _config=%_config:###"=%
SET _config=%_config:###=%

echo.
echo ---- Building runtime %_config% (%3)...
echo.

pushd "%~dp0"
(
call %1 /Build "Debug|%_config%" runtime_%3.sln && ^
call %1 /Build "Release|%_config%" runtime_%3.sln
) || (echo. & echo **** Error building runtime. & popd & exit /b 1)
popd

echo.
echo ---- Build succeeded for runtime %_config% (%3).

exit /b 0
