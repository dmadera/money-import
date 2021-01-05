@echo off
set dir=%~dp0
set dir=%dir:~0,-1%

rem set "pathdata=c:\sklad\data"
rem set "pathto=c:\export\input"

rem if exist %pathdata% goto nodir
rem set "pathdata=\\server\sklad\data"
rem :nodir

rem xcopy %pathdata%\*.000 %pathto% /d/y/r

(
cd %dir%
echo %dir%\fand\fand
echo exit /b
) > %dir%\vdosplus\run.bat
start "SKLAD" %dir%\vdosplus\vdosplus.exe /HIGH
exit /b