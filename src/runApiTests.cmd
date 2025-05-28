@echo off
echo SignPuddle API Test Runner
echo -------------------------

set TEST_PROJECT=SignPuddle.API.Tests
set PROJECT_DIR=.\%TEST_PROJECT%

cls
echo Starting continuous test watch at %time% on %date%
echo Press Ctrl+C to stop watching
echo.
cd %PROJECT_DIR%
dotnet watch test
cd ..
goto :end

:end
echo.
echo Press any key to return to menu...
pause > nul
goto :eof