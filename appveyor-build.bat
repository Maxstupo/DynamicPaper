set outputFolder=DynamicPaper_%CONFIGURATION%_v%APPVEYOR_BUILD_VERSION%

REM Copy build files into outputFolder
mkdir "%outputFolder%"
xcopy "%APPVEYOR_BUILD_FOLDER%\DynamicPaper\bin\%CONFIGURATION%" "DynamicPaper_%CONFIGURATION%_v%APPVEYOR_BUILD_VERSION%" /E /C

7z a "DynamicPaper.zip" "%outputFolder%"


REM make installer, if develop branch - remove version information from output filename
set PATH=%PATH%;"C:\\Program Files (x86)\\Inno Setup 6"   

if %APPVEYOR_REPO_BRANCH% == "develop" (
   iscc dynamicpaper-installer.iss /DConfiguration=%CONFIGURATION% /FDynamicPaper-Setup.exe
) else (
   iscc dynamicpaper-installer.iss /DConfiguration=%CONFIGURATION%
)