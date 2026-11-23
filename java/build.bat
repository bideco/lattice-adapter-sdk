@echo off
setlocal
set ROOT=%~dp0
set OUT=%ROOT%build\classes
set DIST=%ROOT%build\libs
if exist "%OUT%" rmdir /s /q "%OUT%"
if exist "%DIST%" rmdir /s /q "%DIST%"
mkdir "%OUT%" "%DIST%"
dir /b /s "%ROOT%src\main\java\*.java" > "%ROOT%build\sources.txt"
javac -encoding UTF-8 -d "%OUT%" @"%ROOT%build\sources.txt"
if errorlevel 1 exit /b 1
jar --create --file "%DIST%\lattice-adapter-sdk.jar" -C "%OUT%" .
if errorlevel 1 exit /b 1
echo Built "%DIST%\lattice-adapter-sdk.jar"
