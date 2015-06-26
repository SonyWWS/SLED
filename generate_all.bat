:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

pushd "%~dp0"
(
..\Premake5\Premake5.exe --file=runtime.sln.lua vs2010
) || (popd & exit /b 1)
popd

pushd "%~dp0"
(
..\Premake5\Premake5.exe --file=runtime.sln.lua vs2013
) || (popd & exit /b 1)
popd
