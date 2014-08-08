ECHO APPVEYOR_REPO_BRANCH: %APPVEYOR_REPO_BRANCH%
Call tools\nuget.exe restore ..\Slack.Contour.sln
Call "C:\Program Files (x86)\MSBuild\12.0\Bin\MsBuild.exe" Package.build.xml