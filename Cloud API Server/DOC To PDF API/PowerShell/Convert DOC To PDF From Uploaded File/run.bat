@echo off

powershell -NoProfile -ExecutionPolicy Bypass -Command "& .\ConvertDocToPdfFromUploadedFile.ps1"
echo Script finished with errorlevel=%errorlevel%

pause