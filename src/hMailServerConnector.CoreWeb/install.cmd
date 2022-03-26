@echo off

cd /D "%~dp0"

sc create hMailServerConnector start= delayed-auto BinPath= "%cd%\hMailServerConnector.CoreWeb.exe" >nul 2>&1

if %errorLevel% == 0 (
	sc start hMailServerConnector >nul 2>&1

	echo Service hMailServerConnector installed and started
) else (
	echo Please run this script as admin
)

pause
