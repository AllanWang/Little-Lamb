set mypath=%cd%

@echo %mypath%

echo "Building Clone"

"C:\Program Files\Unity\Hub\Editor\2020.3.17f1\Editor\Unity.exe" -quit -batchmode -logFile stdout.log -projectPath %mypath%_clone_0\ -executeMethod BuildScript.BuildAll
@echo "Build complete"