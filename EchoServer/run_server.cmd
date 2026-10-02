@echo off
setlocal
set "Root=%~dp0"
pushd "%ROOT%src\bin\Debug\net10.0"

.\EchoServer.exe

popd
endlocal