@echo off
echo Building Blackwood.IO documentation...

REM Check if DocFX is installed
docfx --version >nul 2>&1
if %errorlevel% neq 0 (
    echo DocFX is not installed. Installing DocFX...
    dotnet tool install -g docfx
    if %errorlevel% neq 0 (
        echo Failed to install DocFX. Please install it manually.
        exit /b 1
    )
)

REM Clean previous build
echo Cleaning previous build...
if exist "..\docs" (
    rmdir /s /q "..\docs"
)

REM Build the documentation
echo Generating documentation...
echo Starting DocFX build with reduced logging...
echo Step 1: Extracting metadata from C# files...
docfx metadata docfx.json --logLevel Warning
echo Step 2: Building documentation...
docfx build docfx.json --logLevel Warning --disableGitFeatures

if %errorlevel% equ 0 (
    echo Documentation built successfully!
    echo Output directory: ..\docs
    echo Open ..\docs\index.html in your browser to view the documentation.
) else (
    echo Documentation build failed!
    exit /b 1
)
