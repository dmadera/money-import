@echo off
set dir=%~dp0
set dir=%dir:~0,-1%

cd %dir%

(
cd %dir%
echo %dir%\ufand\ufandl %dir%\EXPORT
echo exit /b
) > %dir%\vdosplus\run.bat
start "SKLAD" %dir%\vdosplus\vdosplus.exe /HIGH
exit /b