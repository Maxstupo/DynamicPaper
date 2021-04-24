set outputFolder=DynamicPaper_%CONFIGURATION%_v%APPVEYOR_BUILD_VERSION%

REM Copy build files into outputFolder
mkdir "%outputFolder%"
xcopy "%APPVEYOR_BUILD_FOLDER%\DynamicPaper\bin\%CONFIGURATION%" "DynamicPaper_%CONFIGURATION%_v%APPVEYOR_BUILD_VERSION%" /E /C

7z a "DynamicPaper.zip" "%outputFolder%"

REM copy the stpack utility
copy "%APPVEYOR_BUILD_FOLDER%\ShaderToyPacker\bin\%CONFIGURATION%\stpack.exe" "%APPVEYOR_BUILD_FOLDER%\stpack.exe"

REM make installer, if develop branch - remove version information from output filename
set "PATH=%PATH%;C:\Program Files (x86)\Inno Setup 6"   

if "%APPVEYOR_REPO_BRANCH%" == "develop" goto make-dev-installer


:make-installer
iscc dynamicpaper-installer.iss /DConfiguration=%CONFIGURATION%
exit /b

:make-dev-installer
iscc dynamicpaper-installer.iss /DConfiguration=%CONFIGURATION% /FDynamicPaper-Setup
exit /b