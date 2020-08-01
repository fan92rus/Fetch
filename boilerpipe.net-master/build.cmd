@echo off

set builddir="%CD%\build\"

rmdir /s /q %builddir%
mkdir %builddir%

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Boilerpipe.Net.sln /t:Clean;Build /p:Configuration=Release /v:m /nr:false

xcopy "%CD%\src\Core\bin\Release\Boilerpipe.Net.*" %builddir% /Y /R /V