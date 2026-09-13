@echo off
chcp 936 >nul
title 快捷方式小箭头 - 去除 / 恢复
cd /d "%~dp0"

rem ==================== 管理员权限 ====================
rem 双重检测：fltmc 只认管理员（Filter Manager 必须提权才能查）；net session 兜底。
rem 只写 net session 有坑：Server 服务被停用/禁用时，即使当前已是管理员也会返回失败，
rem 脚本于是反复自提权、反复弹 UAC —— 表现出来就是"点了没反应/像卡死"。
fltmc >nul 2>&1
if %errorlevel%==0 goto ADMIN
net session >nul 2>&1
if %errorlevel%==0 goto ADMIN
rem 已经提权过一轮却还是检测不到管理员 -> 直接停，绝不再弹第二次
if /i "%~1"=="elevated" (
    echo.
    echo    [x] 提权后仍检测不到管理员权限，已主动停止，避免无限弹窗。
    echo        请右键本文件，选择【以管理员身份运行】。
    pause
    exit /b
)
echo 正在申请管理员权限，请在 UAC 弹窗中点 [是] ...
powershell.exe -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs -ArgumentList 'elevated'"
exit /b

:ADMIN
set "RK=HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons"
set "RK32=HKLM\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Explorer\Shell Icons"

:MENU
cls
echo ==========================================================
echo    快捷方式小箭头   去除 / 恢复      [cmd + ico]
echo ==========================================================
echo.
echo    当前状态：
reg query "%RK%" /v 29 >nul 2>&1
if %errorlevel%==0 (
    for /f "tokens=3*" %%a in ('reg query "%RK%" /v 29 ^| findstr /i "REG_"') do echo        已去除      29 = %%a
) else (
    echo        系统默认（快捷方式带小箭头）
)
echo.
echo    ---------------------- 去除 ----------------------
echo    [1] 免 ico  推荐   %SystemRoot%\explorer.exe,-264
echo                       一个文件都不用放，加载必定成功
echo    [2] 免 ico  备用   %SystemRoot%\System32\Taskbar.dll,-264
echo    [3] 部署 blank.ico  已实机验证（用同目录下的 blank.ico）
echo    ---------------------- 恢复 ----------------------
echo    [4] 恢复系统默认小箭头
echo    ---------------------- 工具 ----------------------
echo    [5] 仅重建图标缓存
echo    [0] 退出
echo.
set "c="
set /p c=请选择并回车: 
if "%c%"=="1" goto R1
if "%c%"=="2" goto R2
if "%c%"=="3" goto R3
if "%c%"=="4" goto RESTORE
if "%c%"=="5" goto ONLYREBUILD
if "%c%"=="0" exit /b
goto MENU

:R1
set "V=%SystemRoot%\explorer.exe,-264"
set "MSG=完成。请看桌面快捷方式：应当没有小箭头，也没有黑块/阴影。"
goto APPLY

:R2
set "V=%SystemRoot%\System32\Taskbar.dll,-264"
set "MSG=完成。请看桌面快捷方式：应当没有小箭头，也没有黑块/阴影。"
goto APPLY

:R3
set "SRC=%~dp0blank.ico"
if not exist "%SRC%" (
    echo.
    echo    [x] 找不到 blank.ico：%SRC%
    echo        blank.ico 必须和本 cmd 放在同一个目录里。
    echo        也可以改用 [1] 免 ico 方案，一个文件都不用放。
    pause
    goto MENU
)
copy /y "%SRC%" "%SystemRoot%\blank.ico" >nul
set "V=%SystemRoot%\blank.ico,0"
set "MSG=完成。请看桌面快捷方式：应当没有小箭头，也没有黑块/阴影。"
goto APPLY

:APPLY
echo.
echo [1/3] 写入注册表：
reg add "%RK%" /v 29 /t REG_SZ /d "%V%" /f >nul
if errorlevel 1 (
    echo    [x] 写入失败，请确认已用管理员身份运行。
    pause
    goto MENU
)
reg delete "%RK32%" /v 29 /f >nul 2>&1
echo        29 = %V%
if "%c%"=="3" (
    echo [2/3] blank.ico 已部署到 %SystemRoot%
) else (
    del /f /q "%SystemRoot%\blank.ico" >nul 2>&1
    del /f /q "%SystemRoot%\blank-alpha0.ico" >nul 2>&1
    echo [2/3] 免 ico：未部署任何图标文件
)
goto REBUILD

:RESTORE
echo.
echo [1/2] 删除 Shell Icons\29 ...
reg delete "%RK%" /v 29 /f >nul 2>&1
reg delete "%RK32%" /v 29 /f >nul 2>&1
del /f /q "%SystemRoot%\blank.ico" >nul 2>&1
del /f /q "%SystemRoot%\blank-alpha0.ico" >nul 2>&1
echo [2/2] 已恢复系统默认
set "MSG=已恢复系统默认，快捷方式的小箭头应该回来了。如果没回来，重启一次电脑即可。"
goto REBUILD

:ONLYREBUILD
echo.
set "MSG=图标缓存已重建，资源管理器已重启。"

:REBUILD
echo.
echo    正在重建图标缓存并重启资源管理器...
rem ---- 看门狗：约 20 秒后确认 explorer 是否活着，没活就拉起来 ----
rem     用绝对路径 %windir%\explorer.exe，不赌 PATH 里有没有 C:\Windows
start "" /min cmd /c "ping -n 21 127.0.0.1 >nul & tasklist /nh | findstr /i explorer.exe >nul || %windir%\explorer.exe"
rem ---- 只结束 explorer.exe：它是唯一锁住 iconcache 的进程 ----
rem     不要杀 runtimebroker / searchhost / taskhostw，会让系统失去响应
taskkill /f /im explorer.exe >nul 2>&1
rem ---- 用 ping 代替 timeout ----
rem     timeout /t 依赖控制台输入句柄，被重定向或由 Start-Process 拉起时会直接报错退出，
rem     等待时间变 0，导致"删缓存"与"拉起 explorer"之间没有缓冲，表现为时好时坏。
ping -n 3 127.0.0.1 >nul
rem     只删图标缓存，不动 thumbcache：缩略图缓存跟小箭头无关，删了只会让图片文件夹首次变慢
del /f /q "%localappdata%\IconCache.db" >nul 2>&1
del /f /q "%localappdata%\Microsoft\Windows\Explorer\iconcache_*.db" >nul 2>&1
rem     关键：errorlevel 必须【立刻】存进变量。
rem     因为后面那条 echo 一旦执行就会把 errorlevel 冲成 0，
rem     直接连着判断会出现"缓存已清空"却又被标记成占用的自相矛盾结果。
set "LOCKED="
set "RC="
dir /b "%localappdata%\Microsoft\Windows\Explorer\iconcache_*.db" 2>nul | findstr . >nul
set "RC=%errorlevel%"
if "%RC%"=="0" set "LOCKED=1"
if defined LOCKED echo        有 iconcache 文件被其它进程占用，没能删掉
if not defined LOCKED echo        图标缓存已清空
rem ---- 优先让 Windows 自己把 shell 拉回来（非提权启动，比我们从管理员进程启动更干净）----
ping -n 6 127.0.0.1 >nul
tasklist /fi "imagename eq explorer.exe" 2>nul | find /i "explorer.exe" >nul
if errorlevel 1 (
    echo        系统未自动恢复资源管理器，正在手动启动...
    start "" "%windir%\explorer.exe"
    ping -n 6 127.0.0.1 >nul
)
tasklist /fi "imagename eq explorer.exe" 2>nul | find /i "explorer.exe" >nul
if errorlevel 1 (echo        [严重] 资源管理器没起来：Ctrl+Shift+Esc - 文件 - 运行新任务 - 输入 explorer.exe) else (echo        资源管理器已重启 OK)
echo.
echo %MSG%
rem ---- 缓存没删干净时，注册表改了界面也可能没反应，必须明说，不能糊过去 ----
if defined LOCKED (
    echo.
    echo    [注意] 有图标缓存被其它进程占用，没能删除。
    echo           此时注册表虽然已经写入，但界面可能仍显示旧图标。
    echo           请【注销】或【重启】一次，让缓存强制重建。
)
echo    若图标没变化：注销再登录一次，或重启电脑。
echo.
pause
goto MENU
