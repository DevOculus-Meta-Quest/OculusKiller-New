@echo off
:: Check for admin rights
net session >nul 2>&1
if %errorLevel% == 0 (
    goto :startScript
) else (
    echo Requesting elevated privileges...
    goto :getAdminRights
)

:getAdminRights
:: Create a temporary VBScript to elevate the batch file
echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs"
echo UAC.ShellExecute "%~s0", "", "", "runas", 1 >> "%temp%\getadmin.vbs"
"%temp%\getadmin.vbs"
exit

:startScript
setlocal enabledelayedexpansion

:: Check if OculusDash.exe exists in the script's directory
if not exist "%~dp0OculusDash.exe" (
    echo OculusDash.exe not found in the current directory. Please ensure it's present and try again.
    echo.
    pause
    exit
)

:: Define the Oculus directory and file paths
set OCULUS_DIR=C:\Program Files\Oculus\Support\oculus-dash\dash\bin
set OCULUS_EXE=%OCULUS_DIR%\OculusDash.exe
set OCULUS_BK=%OCULUS_DIR%\OculusDash.bk

:: Check if Oculus directory exists
if not exist "%OCULUS_DIR%" (
    echo Oculus directory not found. Please ensure Oculus is installed correctly.
    echo.
    pause
    exit
)

:: Check if OculusDash.exe exists
if exist "%OCULUS_EXE%" (
    :: Check if OculusDash.bk exists
    if exist "%OCULUS_BK%" (
        echo OculusDash.bk backup found.
        echo.
        echo Uninstalling OculusKiller...
        echo.
        :: Delete the current OculusDash.exe
        del "%OCULUS_EXE%"
        if errorlevel 1 (
            echo Error deleting OculusDash.exe. Please check permissions and try again.
            echo.
            pause
            exit
        )
        :: Rename OculusDash.bk back to OculusDash.exe
        ren "%OCULUS_BK%" OculusDash.exe
        if errorlevel 1 (
            echo Error renaming OculusDash.bk. Please check permissions and try again.
            echo.
            pause
            exit
        )
        echo OculusKiller uninstalled successfully!
        echo.
        pause
    ) else (
        echo OculusDash.exe found.
        echo.
        echo Installing OculusKiller...
        echo.
        :: Rename OculusDash.exe to OculusDash.bk
        ren "%OCULUS_EXE%" OculusDash.bk
        if errorlevel 1 (
            echo Error renaming OculusDash.exe. Please check permissions and try again.
            echo.
            pause
            exit
        )
        :: Copy OculusDash.exe from the script's directory to the Oculus directory
        copy "%~dp0OculusDash.exe" "%OCULUS_DIR%"
        if errorlevel 1 (
            echo Error copying OculusDash.exe. Please check permissions and try again.
            echo.
            pause
            exit
        )
        echo OculusKiller installed successfully!
        echo.
        pause
    )
) else (
    if exist "%OCULUS_BK%" (
        echo OculusDash.bk found but OculusDash.exe is missing.
        echo.
        echo Please check the Oculus installation or manually restore OculusDash.bk.
        echo.
        pause
    ) else (
        echo Neither OculusDash.exe nor OculusDash.bk found in the Oculus directory.
        echo.
        echo Please ensure Oculus is installed correctly and try again.
        echo.
        pause
    )
)

endlocal
exit
