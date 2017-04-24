set MSBuild="%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe"
set NoPause=true
%MSBuild% Ammy.WpfTest.csproj
bin\debug\Ammy.WpfTest.exe
