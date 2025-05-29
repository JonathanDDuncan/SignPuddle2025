@echo off
echo Starting SignPuddle Web Application Tests in Watch Mode...

REM Navigate to signpuddle-web directory
cd /d "%~dp0signpuddle-web"

REM Check if Node.js is installed
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: Node.js is not installed or not in PATH
    echo Please install Node.js from https://nodejs.org/
    pause
    exit /b 1
)

REM Check if package.json exists
if not exist package.json (
    echo Error: package.json not found in signpuddle-web directory
    echo Current directory: %CD%
    echo Please ensure package.json exists in the signpuddle-web directory
    pause
    exit /b 1
)

REM Install dependencies if node_modules doesn't exist
if not exist node_modules (
    echo Installing dependencies...
    npm install
    if %errorlevel% neq 0 (
        echo Error: Failed to install dependencies
        pause
        exit /b 1
    )
)

REM Check if Jest is installed
if not exist node_modules\.bin\jest.cmd (
    echo Jest not found, installing as dev dependency...
    npm install --save-dev jest
    if %errorlevel% neq 0 (
        echo Error: Failed to install Jest
        pause
        exit /b 1
    )
)

REM Run tests in watch mode using npx to ensure proper path resolution
echo Starting tests in watch mode...
npx jest --watch

REM If that fails, try other alternatives
if %errorlevel% neq 0 (
    echo Trying alternative test command...
    npm test
)

pause