pushd lib\Nitra
 @echo | call Clear.cmd
 @echo | call BuildBoot.cmd
"C:\Program Files (x86)\MSBuild\14.0\Bin\MsBuild.exe" /m Nitra.sln
popd
call build-ammy.cmd