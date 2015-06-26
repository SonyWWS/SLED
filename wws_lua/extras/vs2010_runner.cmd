:: Copyright (c) Sony Computer Entertainment America LLC.
:: All rights Reserved.

@echo off

SET DEVENV100="%VS100COMNTOOLS%..\IDE\devenv.com"
IF NOT EXIST %DEVENV100% (
echo Visual Studio 2010 not found, skipping
goto end
)

SET VERB=%1
SET SOLUTION=%2

SET CONFIG=###%3%###
SET CONFIG=%CONFIG:"###=%
SET CONFIG=%CONFIG:###"=%
SET CONFIG=%CONFIG:###=%

SET PLATFORM=###%4%###
SET PLATFORM=%PLATFORM:"###=%
SET PLATFORM=%PLATFORM:###"=%
SET PLATFORM=%PLATFORM:###=%

echo.
echo ---- %VERB%ing "%CONFIG%debug|%PLATFORM%" %SOLUTION%...
echo.

pushd "%~dp0"
(
call %DEVENV100% /%VERB% "%CONFIG%debug|%PLATFORM%" %SOLUTION% && ^
call %DEVENV100% /%VERB% "%CONFIG%release|%PLATFORM%" %SOLUTION%
) || (echo. & echo **** Error %VERB%ing. & popd & exit /b 1)
popd

echo.
echo ---- %VERB% succeeded.

:end

exit /b 0
