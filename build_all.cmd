:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

pushd "%~dp0"
(
call "build_all_windows.cmd" && ^
call "build_all_sled.cmd"
) || (popd & exit /b 1)
popd
