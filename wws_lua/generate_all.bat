:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

pushd "%~dp0lua-5.1.4"
(
..\..\..\Premake5\premake5.exe --file=lua-5.1.4.sln.lua vs2010
) || (popd & exit /b 1)
popd

pushd "%~dp0lua-5.1.4"
(
..\..\..\Premake5\premake5.exe --file=lua-5.1.4.sln.lua vs2013
) || (popd & exit /b 1)
popd

pushd "%~dp0lua-5.2.3"
(
..\..\..\Premake5\premake5.exe --file=lua-5.2.3.sln.lua vs2010
) || (popd & exit /b 1)
popd

pushd "%~dp0lua-5.2.3"
(
..\..\..\Premake5\premake5.exe --file=lua-5.2.3.sln.lua vs2013
) || (popd & exit /b 1)
popd
