set mypath=%cd%

@echo %mypath%

echo "Cloning contents"
rmdir %mypath%_Clone\ /s /q
xcopy . %mypath%_Clone\ /e /f /y /exclude:clone_excluded_files.txt > nul

echo "Building Clone"

"C:\Program Files\Unity\Hub\Editor\2020.3.17f1\Editor\Unity.exe" -quit -batchmode -logFile stdout.log -projectPath %mypath%_Clone\ -executeMethod BuildScript.BuildAll
@echo "Build complete"