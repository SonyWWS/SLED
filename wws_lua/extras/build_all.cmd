:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

pushd "%~dp0"
(
call "%~dp0\clean_all_vs2010.cmd" && ^
call "%~dp0\build_all_vs2010.cmd" && ^
call "%~dp0\clean_all_vs2013.cmd" && ^
call "%~dp0\build_all_vs2013.cmd"
) || (echo. & echo **** Error building wws_lua extras. & popd & exit /b 1)
popd
