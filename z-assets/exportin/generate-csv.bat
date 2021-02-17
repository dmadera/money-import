@echo off
SETLOCAL
SET "ERROR=0"
SET "sqlserver=%1"
SET "dir=%~dp0"
SET "dirsql=%dir%sql\"
SET "dircsv=%dir%csv\"
SET "ext=.txt"
SET "TMP1=%TEMP%\bat~%RANDOM%1.tmp"
SET "TMP2=%TEMP%\bat~%RANDOM%2.tmp"

echo Deleting files in %dircsv%
del /S /Q /F "%dircsv%*" >nul 2>&1

for /f %%f IN ('dir /b %dirsql%*.sql') do (
	sqlcmd -S %sqlserver% -i "%dirsql%%%f" -o "%TMP1%" -h -1 -s";" -W
	IF NOT "%ERRORLEVEL%" == "0" SET "ERROR=1" && GOTO :clean	
	more +1 "%TMP1%" > "%TMP2%"
	move /y "%TMP2%" "%dircsv%%%~nf%ext%" > nul
	echo Created - %dircsv%%%~nf%ext%
	del /F /Q %TMP1% >nul 2>&1
	del /F /Q %TMP2% >nul 2>&1
)

:clean
del /F /Q %TMP1% >nul 2>&1
del /F /Q %TMP2% >nul 2>&1

IF %ERROR%==0 GOTO :exitsuccess

del /S /Q /F "%dircsv%*" >nul 2>&1
endlocal
exit /B 1

:exitsuccess
endlocal
exit /B 0