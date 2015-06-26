:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

pushd "%~dp0"
(
call "build_all_sled_vs2010.cmd" && ^
call "build_all_sled_vs2013.cmd"
) || (popd & exit /b 1)
popd
