@echo off

echo Removing NoCap binaries
del /Q "%LOCALAPPDATA%\NoCap\"
echo Removing Start menu entries
del /Q "%APPDATA%\Microsoft\Windows\Start Menu\NoCap\"
echo Removing desktop icon
del /Q "%USERPROFILE%\Desktop\NoCap.lnk"
echo Removing startup entry
del /Q "%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup\NoCap.lnk"

echo.

echo NoCap should be uninstalled

pause