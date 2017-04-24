..\..\..\Nitra.LanguageCompiler\bin\Debug\Nitra.LanguageCompiler.exe -lang:naml -ns:Ammy -out:..\..\..\..\ bin\Debug\Ammy.dll

set MSBuild="%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
set NoPause=true

..\..\..\..\namlVsPackage\.nuget\nuget.exe restore ..\..\..\..\namlVsPackage\namlVsPackage.sln 
%MSBuild% ..\..\..\..\namlVsPackage\namlVsPackage.sln