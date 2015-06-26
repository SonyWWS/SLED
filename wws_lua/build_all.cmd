:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

pushd "%~dp0"
(
call "%~dp0\lua-5.1.4\build_all.cmd" && ^
call "%~dp0\lua-5.2.3\build_all.cmd" && ^
call "%~dp0\extras\build_all.cmd"
) || (exit /b 1)
popd
