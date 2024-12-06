@echo off
set dir=%~dp0
set dir=%dir:~0,-1%

cd %dir%
del /S /Q /F input\*
del /S /Q /F output\*

net use X: \\192.168.15.50\Sklad exportsklad /user:exportsklad
xcopy X:\data\*.000 input\ /Y /Z
xcopy X:\stare\Sklad23\KARTY.000 input\KARTY20.000 /Y /Z
net use X: /delete /Y

(
cd %dir%
echo %dir%\ufand\ufandl %dir%\EXPORT
echo exit /b
) > %dir%\vdosplus\run.bat
start "SKLAD" %dir%\vdosplus\vdosplus.exe /HIGH
exit /b
