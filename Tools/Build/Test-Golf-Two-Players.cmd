@echo off
setlocal
for %%I in ("%~dp0..\..") do set "repo=%%~fI"
set "player=%repo%\Builds\WindowsFinal\WhatTheFish.exe"
if not exist "%player%" (
  echo Windows player missing. Build the complete Windows player first.
  exit /b 1
)
start "" "%player%" -sport Golf -localHost -port 7777 -screen-fullscreen 0 -screen-width 960 -screen-height 540
timeout /t 2 /nobreak >nul
start "" "%player%" -sport Golf -localClient -port 7777 -screen-fullscreen 0 -screen-width 960 -screen-height 540
echo Ready both players in the lobby. The host enters the island and starts the game.
echo In the focused Host window: F1/F3/F4 events, F2/F5 unused, F6 Island Rush, F7 clear.
