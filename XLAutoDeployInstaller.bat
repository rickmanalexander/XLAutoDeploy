@echo off

echo.
echo This program will install the XLAutoDeploy AddIn to the following folder: %APPDATA%\XLAutoDeploy\ 
echo.
echo *** Please close MS Excel before continuing with the installation. ***
echo.
pause

:reCheckBitness

wscript GetMSExcelStatus.vbs
rem echo GetMSExcelStatus script returned %errorlevel%

IF errorlevel 0 goto checkOfficeBitness

echo. The Excel application is open. Please close it to continue this process.
pause 

goto reCheckBitness

rem delete and re-create the folder containing the add-in
if exist %APPDATA%\XLAutoDeploy\ rmdir /s /q %APPDATA%\XLAutoDeploy\
mkdir %APPDATA%\XLAutoDeploy\ 

:checkOfficeBitness
wscript GetMSOfficeBitness.vbs
rem echo GetMSOfficeBitness script returned %errorlevel%

if errorlevel 64 goto 64Bit

:32Bit
set addInFileName=XLAutoDeploy-AddIn.xll
goto copyAddInFile

:64Bit
set addInFileName=XLAutoDeploy-AddIn64.xll
goto copyAddInFile

:copyAddInFile
copy "\\s-hdqfp03\publicshare\TechOps\Business Intelligence Applications\Core APIs\Excel AddIns\Production\XLAutoDeploy\%addInFileName%" %APPDATA%\XLAutoDeploy\

echo Please wait while the add-in is installed.
pause

wscript InstallXLAutoDeploy.vbs

echo.
echo Thank you for installing TopoLib.
echo.

exit