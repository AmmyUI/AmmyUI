tools\nuget\nuget.exe restore %~dp0\Ammy.sln
"c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MsBuild.exe" /m %~dp0\Ammy.sln /t:Clean
"c:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MsBuild.exe" /m %~dp0\Ammy.sln