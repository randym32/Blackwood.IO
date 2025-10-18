# PowerShell script to build Blackwood.IO documentation with timeout protection

Write-Host "Building Blackwood.IO documentation..." -ForegroundColor Green

# Check if DocFX is installed
try {
    $docfxVersion = docfx --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        throw "DocFX not found"
    }
    Write-Host "DocFX version: $docfxVersion" -ForegroundColor Yellow
} catch {
    Write-Host "DocFX is not installed. Installing DocFX..." -ForegroundColor Yellow
    dotnet tool install -g docfx
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to install DocFX. Please install it manually." -ForegroundColor Red
        exit 1
    }
}

# Clean previous build
Write-Host "Cleaning previous build..." -ForegroundColor Yellow
if (Test-Path "..\docs") {
    Remove-Item "..\docs" -Recurse -Force
}

# Build the documentation with timeout protection
Write-Host "Generating documentation with timeout protection..." -ForegroundColor Yellow

# Create a job to run DocFX with timeout
$job = Start-Job -ScriptBlock {
    Set-Location $using:PWD
    Write-Host "Step 1: Extracting metadata from C# files..." -ForegroundColor Yellow
    docfx metadata docfx.json --logLevel Warning
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    Write-Host "Step 2: Building documentation..." -ForegroundColor Yellow
    docfx build docfx.json --logLevel Warning --disableGitFeatures
}

# Wait for the job to complete with a 10-minute timeout
$timeout = 600 # 10 minutes
$completed = Wait-Job -Job $job -Timeout $timeout

if ($completed) {
    $result = Receive-Job -Job $job
    Remove-Job -Job $job

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Documentation built successfully!" -ForegroundColor Green
        Write-Host "Output directory: ..\docs" -ForegroundColor Green
        Write-Host "Open ..\docs\index.html in your browser to view the documentation." -ForegroundColor Green
    } else {
        Write-Host "Documentation build failed!" -ForegroundColor Red
        Write-Host "Exit code: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "DocFX build timed out after $($timeout/60) minutes!" -ForegroundColor Red
    Write-Host "This usually indicates a hanging issue. Try the following:" -ForegroundColor Yellow
    Write-Host "1. Check if antivirus is blocking DocFX" -ForegroundColor Yellow
    Write-Host "2. Run as administrator" -ForegroundColor Yellow
    Write-Host "3. Disable real-time protection temporarily" -ForegroundColor Yellow
    Stop-Job -Job $job
    Remove-Job -Job $job
    exit 1
}