@echo off
chcp 936 >nul
title 隐藏快捷方式小箭头
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
set "RK=HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons"
set "RK32=HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons"
cd /d "%~dp0"

:MENU
cls
echo ==========================================================
echo    隐藏快捷方式小箭头
echo ==========================================================
echo    当前状态：
reg query "%RK%" /v 29 >nul 2>&1
if %errorlevel%==0 (
    for /f "tokens=2*" %%a in ('reg query "%RK%" /v 29 ^| findstr /i "REG_"') do echo        已隐藏：29 = %%b
) else (
    echo        系统默认（快捷方式带小箭头）
)
echo.
echo    [1] 方案一（推荐）  explorer.exe,-264
echo    [2] 方案二          Taskbar.dll,-264
echo    [3] 方案三（备选）  部署 blank.ico（需与本脚本同目录）
echo    [4] 自定义方案      查看当前注册表参数
echo    [0] 恢复系统默认小箭头
echo.
set "c="
set /p c=请选择并回车: 
if "%c%"=="1" goto R1
if "%c%"=="2" goto R2
if "%c%"=="3" goto R3
if "%c%"=="4" goto R4
if "%c%"=="0" goto RESTORE
goto MENU

:R1
set "V=%SystemRoot%\explorer.exe,-264"
goto APPLY

:R2
set "V=%SystemRoot%\System32\Taskbar.dll,-264"
goto APPLY

:R3
if not exist "%~dp0blank.ico" (
    echo.
    echo    [x] 找不到 blank.ico：%~dp0blank.ico
    echo        blank.ico 必须和本脚本放在同一个目录里。
    pause
    goto MENU
)
copy /y "%~dp0blank.ico" "%SystemRoot%\blank.ico" >nul
set "V=%SystemRoot%\blank.ico,0"
goto APPLY

:R4
echo.
reg query "%RK%" /v 29 2>nul
reg query "%RK32%" /v 29 2>nul
echo.
echo    若以上均为空，说明尚未修改或由其它工具设置。
pause
goto MENU

:APPLY
echo.
echo [1/2] 写入注册表：29 = %V%
reg add "%RK%" /v 29 /t REG_SZ /d "%V%" /f >nul
reg delete "%RK32%" /v 29 /f >nul 2>&1
echo [2/2] 清理冲突文件...
reg query "%RK%" /v 77 2>nul | findstr /i "blank.ico" >nul
if %errorlevel%==0 (
    echo        UAC 盾牌方案正在使用 blank.ico，跳过删除
) else (
    del /f /q "%SystemRoot%\blank.ico" >nul 2>&1
    del /f /q "%SystemRoot%\blank-alpha0.ico" >nul 2>&1
)
goto REBUILD

:RESTORE
echo.
echo [1/2] 删除 Shell Icons\29 ...
reg delete "%RK%" /v 29 /f >nul 2>&1
reg delete "%RK32%" /v 29 /f >nul 2>&1
echo [2/2] 清理图标文件...
reg query "%RK%" /v 77 2>nul | findstr /i "blank.ico" >nul
if %errorlevel%==0 (
    echo        UAC 盾牌方案正在使用 blank.ico，跳过删除
) else (
    del /f /q "%SystemRoot%\blank.ico" >nul 2>&1
    del /f /q "%SystemRoot%\blank-alpha0.ico" >nul 2>&1
)
goto REBUILD

:REBUILD
echo.
echo    正在重建图标缓存并重启资源管理器...
start "" /min cmd /c "ping -n 21 127.0.0.1 >nul & tasklist /nh | findstr /i explorer.exe >nul || %windir%\explorer.exe"
taskkill /f /im explorer.exe >nul 2>&1
ping -n 3 127.0.0.1 >nul
del /f /q "%localappdata%\IconCache.db" >nul 2>&1
del /f /q "%localappdata%\Microsoft\Windows\Explorer\iconcache_*.db" >nul 2>&1
ping -n 6 127.0.0.1 >nul
tasklist /fi "imagename eq explorer.exe" 2>nul | find /i "explorer.exe" >nul
if errorlevel 1 (
    echo        系统未自动恢复资源管理器，正在手动启动...
    start "" "%windir%\explorer.exe"
    ping -n 6 127.0.0.1 >nul
)
echo.
echo    完成。若图标未变化，请注销再登录或重启电脑。
echo.
pause
goto MENU
