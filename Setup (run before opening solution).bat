:: This script creates a symlink to the game binaries to account for different installation directories on different systems.

dpath="C:\torch-server"
cd %~dp0
mklink /J TorchBinaries %dpath%
if errorlevel 1 goto Error
echo Done! You can now open the Torch solution without issue.
goto EndFinal
:Error2
echo An error occured creating the symlink.
:EndFinal
pause
