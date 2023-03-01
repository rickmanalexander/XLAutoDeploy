@echo off

WScript GetMSOfficeBitness.vbs

echo WScript returned %errorlevel%

echo bitness: errorlevel
goto :endpoint

:endpoint

pause