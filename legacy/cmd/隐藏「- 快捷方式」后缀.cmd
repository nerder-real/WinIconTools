@echo off
chcp 936 >nul
title 隐藏「- 快捷方式」后缀
set "EK=HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer"
set "NK=HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Naming Templates"

:MENU
cls
echo ==========================================================
echo    隐藏「- 快捷方式」后缀
echo    （仅对【新建】的快捷方式生效，已有名称不会自动改变）
echo ==========================================================
echo    当前状态：
reg query "%EK%" /v Link >nul 2>&1
if %errorlevel%==0 (
    for /f "tokens=3" %%a in ('reg query "%EK%" /v Link ^| findstr /i "REG_"') do echo        已隐藏：Link = %%a（方案一）
) else (
    echo        默认（新建快捷方式仍会加上「- 快捷方式」）
)
echo.
echo    [1] 方案一（推荐）  Link = 00 00 00 00，新建快捷方式不带后缀
echo    [2] 自定义方案      查看当前注册表参数
echo    [0] 恢复默认命名
echo.
set "c="
set /p c=请选择并回车: 
if "%c%"=="1" goto R1
if "%c%"=="2" goto R2
if "%c%"=="0" goto RESTORE
goto MENU

:R1
echo.
echo    写入 Explorer\Link = 00000000 ...
reg add "%EK%" /v Link /t REG_BINARY /d 00000000 /f >nul
echo    完成。新建一个快捷方式试试效果。
echo.
pause
goto MENU

:R2
echo.
reg query "%EK%" /v Link 2>nul
reg query "%NK%" /v Shortcut 2>nul
echo.
echo    若以上均为空，说明尚未修改。
echo    点击「方案一」可隐藏后缀。
echo.
pause
goto MENU

:RESTORE
echo.
echo    删除 Link 与 Naming Templates\Shortcut ...
reg delete "%EK%" /v Link /f >nul 2>&1
reg delete "%NK%" /v Shortcut /f >nul 2>&1
echo    已恢复默认命名。新建快捷方式将重新显示「- 快捷方式」。
echo.
pause
goto MENU
