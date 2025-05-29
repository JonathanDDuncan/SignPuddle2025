@echo off
echo Starting SignPuddle Cypress Tests in New Window...

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

REM Check if Cypress is installed
if not exist node_modules\.bin\cypress.cmd (
    echo Cypress not found, installing as dev dependency...
    npm install --save-dev cypress
    if %errorlevel% neq 0 (
        echo Error: Failed to install Cypress
        pause
        exit /b 1
    )
)

REM Check if cypress.config.js exists
if not exist cypress.config.js if not exist cypress.json (
    echo Warning: No Cypress configuration file found
    echo Creating basic cypress.config.js...
    echo const { defineConfig } = require('cypress'^); > cypress.config.js
    echo. >> cypress.config.js
    echo module.exports = defineConfig({ >> cypress.config.js
    echo   e2e: { >> cypress.config.js
    echo     baseUrl: 'http://localhost:3000', >> cypress.config.js
    echo     specPattern: 'cypress/e2e/**/*.cy.{js,jsx,ts,tsx}', >> cypress.config.js
    echo     watchForFileChanges: true >> cypress.config.js
    echo   } >> cypress.config.js
    echo }^); >> cypress.config.js
)

REM Create a temporary batch file to run Cypress in new window
echo @echo off > temp_cypress.cmd
echo echo Starting Cypress tests in headless mode... >> temp_cypress.cmd
echo npx cypress run >> temp_cypress.cmd
echo echo. >> temp_cypress.cmd
echo echo Cypress tests completed. >> temp_cypress.cmd
echo pause >> temp_cypress.cmd

REM Run Cypress in new command window
echo Opening new window to run Cypress tests...
start "Cypress Tests" cmd /k temp_cypress.cmd

REM Clean up temporary file after a short delay
timeout /t 2 >nul
del temp_cypress.cmd

echo Cypress tests started in new window.
pause