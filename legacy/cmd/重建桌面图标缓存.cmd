@echo off
chcp 936 >nul
title 重建桌面图标缓存
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
echo.
echo    正在重建图标缓存并重启资源管理器...
echo.
echo [1/4] 启动看门狗（20 秒后确认资源管理器存活）...
start "" /min cmd /c "ping -n 21 127.0.0.1 >nul & tasklist /nh | findstr /i explorer.exe >nul || %windir%\explorer.exe"
echo [2/4] 结束资源管理器以释放缓存文件锁...
taskkill /f /im explorer.exe >nul 2>&1
ping -n 3 127.0.0.1 >nul
echo [3/4] 删除图标缓存...
del /f /q "%localappdata%\IconCache.db" >nul 2>&1
del /f /q "%localappdata%\Microsoft\Windows\Explorer\iconcache_*.db" >nul 2>&1
echo [4/4] 等待资源管理器自动恢复...
ping -n 6 127.0.0.1 >nul
tasklist /fi "imagename eq explorer.exe" 2>nul | find /i "explorer.exe" >nul
if errorlevel 1 (
    echo        系统未自动恢复，正在手动启动...
    start "" "%windir%\explorer.exe"
    ping -n 6 127.0.0.1 >nul
)
tasklist /fi "imagename eq explorer.exe" 2>nul | find /i "explorer.exe" >nul
if errorlevel 1 (
    echo    [x] 资源管理器未启动：Ctrl+Shift+Esc - 文件 - 运行新任务 - 输入 explorer.exe
) else (
    echo.
    echo    图标缓存已重建，资源管理器已重启。
)
echo.
pause
