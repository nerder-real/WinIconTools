@echo off
chcp 936 >nul
title 右键菜单切换（Win10 经典 / Win11 新版）
fltmc >nul 2>&1
if %errorlevel%==0 goto ADMIN
net session >nul 2>&1
if %errorlevel%==0 goto ADMIN
if /i "%~1"=="elevated" (
    echo.
    echo    [x] 提权后仍检测不到管理员权限，已停止。
    pause
    exit /b
)
echo 正在申请管理员权限，请在 UAC 弹窗中点 [是] ...
powershell.exe -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs -ArgumentList 'elevated'"
exit /b

:ADMIN
set "CU=HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}"
set "LM=HKLM\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}"

:MENU
cls
echo ==========================================================
echo    右键菜单切换（Win10 经典 / Win11 新版）
echo ==========================================================
echo    当前状态：
reg query "%CU%" >nul 2>&1
if %errorlevel%==0 (
    echo        Win10 经典右键菜单（%CU% 存在）
) else (
    reg query "%LM%" >nul 2>&1
    if %errorlevel%==0 (
        echo        Win10 经典右键菜单（HKLM 中存在键，可能由其它工具写入）
    ) else (
        echo        Win11 新版右键菜单（系统默认）
    )
)
echo.
echo    [1] 切换到 Win10 经典右键菜单
echo    [2] 切换到 Win11 新版右键菜单
echo    [0] 退出
echo.
set "c="
set /p c=请选择并回车: 
if "%c%"=="1" goto W10
if "%c%"=="2" goto W11
if "%c%"=="0" exit /b
goto MENU

:W10
echo.
echo [1/2] 先结束资源管理器，再写入 CLSID 键（确保生效）...
taskkill /f /im explorer.exe >nul 2>&1
ping -n 2 127.0.0.1 >nul
reg add "%CU%\InprocServer32" /f /ve >nul
echo [2/2] 重启资源管理器...
start "" "%windir%\explorer.exe"
ping -n 4 127.0.0.1 >nul
echo.
echo    完成。直接在文件/桌面右键即可看到效果。
echo    若样式未变化，请注销一次再登录。
echo.
pause
goto MENU

:W11
echo.
echo [1/3] 先结束资源管理器，再删除 CLSID 键...
taskkill /f /im explorer.exe >nul 2>&1
ping -n 2 127.0.0.1 >nul
reg delete "%CU%" /f >nul 2>&1
reg delete "%LM%" /f >nul 2>&1
reg query "%CU%" >nul 2>&1
if %errorlevel%==0 goto DELFAIL
reg query "%LM%" >nul 2>&1
if %errorlevel%==0 goto DELFAIL
echo [2/3] 注册表已清理。
echo [3/3] 重启资源管理器...
start "" "%windir%\explorer.exe"
ping -n 4 127.0.0.1 >nul
echo.
echo    完成。直接在文件/桌面右键即可看到效果。
echo.
pause
goto MENU

:DELFAIL
echo.
echo    [x] HKLM 中的 CLSID 键删除失败（TrustedInstaller 权限保护）。
echo        可尝试手动接管所有权后删除，或使用图形版工具。
echo        注意：部分系统删除后仍需注销一次才能生效。
start "" "%windir%\explorer.exe"
pause
goto MENU
