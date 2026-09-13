@echo off
chcp 65001 >nul
setlocal
cd /d "%~dp0"

set "CSC=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
if not exist "%CSC%" set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo [x] 未找到 .NET Framework 自带的 csc.exe 编译器。
    pause
    exit /b 1
)

if not exist "app.ico" (
    echo 正在生成应用图标 app.ico ...
    powershell -NoProfile -ExecutionPolicy Bypass -File gen-icon.ps1
)

echo 正在编译：桌面图标美化工具.exe ...
"%CSC%" /nologo /target:winexe /platform:anycpu /optimize+ ^
    /out:"..\桌面图标美化工具.exe" ^
    /win32manifest:app.manifest ^
    /win32icon:app.ico ^
    /resource:blank.ico,blank.ico ^
    /resource:logo64.png,logo64.png ^
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll ^
    /codepage:65001 Program.cs

if errorlevel 1 (
    echo [x] 编译失败。
    pause
    exit /b 1
)
echo [OK] 已生成：快捷方式小箭头.exe（双击运行，UAC 弹窗点“是”即可）
pause
