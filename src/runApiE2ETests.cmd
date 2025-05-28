@echo off
REM filepath: c:\Code\SignWriting\SignPuddle\src\runE2ETests.cmd

echo SignPuddle API End-to-End Test Runner
echo ------------------------------------

set TEST_PROJECT=SignPuddle.API.E2ETests
set PROJECT_DIR=.\%TEST_PROJECT%

cls
echo Choose test execution mode:
echo 1. Run E2E tests once
echo 2. Watch mode (continuous E2E testing)
echo 3. Exit

choice /c 123 /n /m "Enter your choice (1-3): "

if errorlevel 3 goto :exit
if errorlevel 2 goto :watch
if errorlevel 1 goto :run

:run
cls
echo Running E2E tests at %time% on %date%
echo.
dotnet test %PROJECT_DIR%
echo.
echo Test run completed
goto :end

:watch
cls
echo Starting continuous E2E test watch at %time% on %date%
echo Press Ctrl+C to stop watching
echo.
cd %PROJECT_DIR%
dotnet watch test
cd ..
goto :end

:exit
echo Exiting E2E test runner...
goto :eof

:end
echo.
echo Press any key to return to menu...
pause > nul
goto :eof