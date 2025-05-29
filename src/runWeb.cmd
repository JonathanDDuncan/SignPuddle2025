@echo off
echo Starting SignPuddle Web Application...

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

REM Start the development server
echo Starting development server...
npm run dev

REM If npm run dev fails, try npm start
if %errorlevel% neq 0 (
    echo Trying alternative start command...
    npm start
)

pause