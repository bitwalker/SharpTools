@echo off

powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%~dp0\build.ps1' %*;"
exit /B %errorlevel%