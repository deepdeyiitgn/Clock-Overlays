@echo off
setlocal EnableDelayedExpansion

title Clock Overlays Installer

set "REPO=deepdeyiitgn/Clock-Overlays"
set "DOWNLOADS=%USERPROFILE%\Downloads"
set "LOGDIR=%DOWNLOADS%\ClockOverlays-Logs"

if not exist "%LOGDIR%" mkdir "%LOGDIR%"

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd_HHmmss"') do set TS=%%i

set "LOGFILE=%LOGDIR%\InstallLog_%TS%.txt"

echo ==================================================>>"%LOGFILE%"
echo Clock Overlays Bootstrapper>>"%LOGFILE%"
echo Started: %date% %time%>>"%LOGFILE%"
echo ==================================================>>"%LOGFILE%"

echo.
echo Fetching latest release...
echo Fetching latest release...>>"%LOGFILE%"

powershell -NoProfile -ExecutionPolicy Bypass ^
"$r=Invoke-RestMethod 'https://api.github.com/repos/%REPO%/releases/latest';" ^
"$a=$r.assets | ?{$_.name -like '*.exe'} | select -First 1;" ^
"Write-Output $a.browser_download_url" > "%TEMP%\clock_url.txt"

set /p DOWNLOAD_URL=<"%TEMP%\clock_url.txt"

if "%DOWNLOAD_URL%"=="" (
echo ERROR: Could not fetch release.
echo ERROR: Could not fetch release.>>"%LOGFILE%"
pause
exit /b 1
)

echo URL Found:
echo %DOWNLOAD_URL%

echo URL: %DOWNLOAD_URL%>>"%LOGFILE%"

for %%F in ("%DOWNLOAD_URL%") do set FILE=%%~nxF

set "INSTALLER=%DOWNLOADS%%FILE%"

echo.
echo Downloading installer...
echo Downloading installer...>>"%LOGFILE%"

powershell -NoProfile -ExecutionPolicy Bypass ^
"$ProgressPreference='Continue'; Invoke-WebRequest '%DOWNLOAD_URL%' -OutFile '%INSTALLER%'"

if not exist "%INSTALLER%" (
echo ERROR: Download failed.
echo ERROR: Download failed.>>"%LOGFILE%"
pause
exit /b 1
)

echo Download completed.
echo Download completed.>>"%LOGFILE%"

echo Launching installer...
echo Launching installer...>>"%LOGFILE%"

start "" "%INSTALLER%"

echo Installer launched successfully.
echo Installer launched successfully.>>"%LOGFILE%"

echo.
echo Log saved:
echo %LOGFILE%

pause
