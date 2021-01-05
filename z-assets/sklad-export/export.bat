@echo off
set dir=%~dp0
set dir=%dir:~0,-1%

set "pathdata=c:\sklad\data"
set "pathto=%dir%\input"

if exist %pathdata% goto nodir
set "pathdata=\\server\sklad\data"
:nodir

xcopy %pathdata%\*.000 %pathto% /d/y/r

(
cd %dir%
echo %dir%\ufand\ufand %dir\export
echo exit /b
) > %dir%\vdosplus\run.bat
start "SKLAD" %dir%\vdosplus\vdosplus.exe /HIGH
exit /b