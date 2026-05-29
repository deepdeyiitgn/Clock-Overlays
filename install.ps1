# Clock Overlays Advanced Bootstrapper

$RepoOwner = "deepdeyiitgn"
$RepoName  = "Clock-Overlays"

$ApiUrl = "https://api.github.com/repos/$RepoOwner/$RepoName/releases/latest"

$DownloadsFolder = Join-Path $env:USERPROFILE "Downloads"

$LogFolder = Join-Path $DownloadsFolder "ClockOverlays-Logs"
New-Item -ItemType Directory -Force -Path $LogFolder | Out-Null

$Timestamp = Get-Date -Format "yyyyMMdd_HHmmss"

$LogFile = Join-Path $LogFolder "InstallLog_$Timestamp.txt"

function Write-Log {
    param([string]$Message)

    $Time = Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"
    $Line = "[$Time] $Message"

    Write-Host $Line
    Add-Content -Path $LogFile -Value $Line
}

Write-Log "=================================================="
Write-Log "Clock Overlays Bootstrapper Started"
Write-Log "Computer: $env:COMPUTERNAME"
Write-Log "User: $env:USERNAME"
Write-Log "OS: $([Environment]::OSVersion.VersionString)"
Write-Log "PowerShell: $($PSVersionTable.PSVersion)"
Write-Log "=================================================="

try {

    Write-Log "Fetching latest release..."

    $Release = Invoke-RestMethod -Uri $ApiUrl

    Write-Log "Release Name : $($Release.name)"
    Write-Log "Tag Version  : $($Release.tag_name)"
    Write-Log "Published At : $($Release.published_at)"

    $ExeAsset = $Release.assets |
        Where-Object { $_.name -like "*.exe" } |
        Select-Object -First 1

    if (-not $ExeAsset) {
        throw "No EXE asset found."
    }

    Write-Log "Asset Name   : $($ExeAsset.name)"
    Write-Log "Asset Size   : $([Math]::Round($ExeAsset.size / 1MB, 2)) MB"

    $InstallerPath = Join-Path $DownloadsFolder $ExeAsset.name

    Write-Log "Download URL:"
    Write-Log $ExeAsset.browser_download_url

    Write-Log "Download Destination:"
    Write-Log $InstallerPath

    $ProgressPreference = 'Continue'

    Write-Progress `
        -Activity "Clock Overlays" `
        -Status "Downloading installer..." `
        -PercentComplete 0

    Invoke-WebRequest `
        -Uri $ExeAsset.browser_download_url `
        -OutFile $InstallerPath

    Write-Progress `
        -Activity "Clock Overlays" `
        -Status "Download Complete" `
        -PercentComplete 100

    Write-Log "Download completed successfully."

    if (-not (Test-Path $InstallerPath)) {
        throw "Downloaded file not found."
    }

    $FileInfo = Get-Item $InstallerPath

    Write-Log "Downloaded File Size:"
    Write-Log "$([Math]::Round($FileInfo.Length / 1MB, 2)) MB"

    Write-Log "Launching installer..."

    # If using Inno Setup in future:
    # $InstallerArgs = "/LOG=`"$LogFolder\Installer_$Timestamp.log`""

    $Process = Start-Process `
        -FilePath $InstallerPath `
        -PassThru

    Write-Log "Installer PID: $($Process.Id)"

    while (-not $Process.HasExited) {

        Write-Progress `
            -Activity "Installer Running" `
            -Status "Waiting for installer to finish..." `
            -PercentComplete 50

        Start-Sleep -Seconds 2

        $Process.Refresh()

        Write-Log "Installer still running..."
    }

    Write-Progress `
        -Activity "Installer Running" `
        -Status "Installer Finished" `
        -PercentComplete 100

    Write-Log "Installer Exit Code: $($Process.ExitCode)"
    Write-Log "Installer finished."

}
catch {

    Write-Log "ERROR OCCURRED"
    Write-Log $_.Exception.Message
    Write-Log $_

}

Write-Log "=================================================="
Write-Log "Bootstrapper Finished"
Write-Log "Log Saved To:"
Write-Log $LogFile
Write-Log "=================================================="
