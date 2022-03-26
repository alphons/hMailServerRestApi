@echo off

sc stop hMailServerConnector >nul 2>&1

if %errorLevel% == 0 (
	sc delete hMailServerConnector >nul 2>&1

	echo Service hMailServerConnector stopped and deleted
) else (
	echo Please run this script as admin
)

pause