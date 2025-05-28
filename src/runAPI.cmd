@echo off
REM filepath: c:\Code\SignWriting\SignPuddle\src\runAPI.cmd
echo Starting SignPuddle API...

cd /d %~dp0
dotnet run --project SignPuddle.API/SignPuddle.API.csproj

echo SignPuddle API has been stopped.
pause