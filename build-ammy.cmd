tools\nuget\nuget.exe restore %~dp0\Ammy.sln
pushd tools\vswhere\
call run-msbuild.bat /m %~dp0\Ammy.sln /t:Clean
call run-msbuild.bat /m %~dp0\Ammy.sln
popd