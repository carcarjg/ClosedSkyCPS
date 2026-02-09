# IncrementVersion.ps1
# Auto-increments version in CSP 01A1 format
# Number goes 1-9, then letter increments A-Z

param(
    [string]$projectPath
)

Write-Host "Incrementing version for publish..." -ForegroundColor Cyan

# Read the project file
[xml]$proj = Get-Content $projectPath

# Find the ApplicationDisplayVersion property
$versionNode = $proj.Project.PropertyGroup | Where-Object { $_.ApplicationDisplayVersion } | Select-Object -First 1

if ($null -eq $versionNode) {
    Write-Host "ApplicationDisplayVersion not found, creating with default CSP 01A1" -ForegroundColor Yellow
    $versionNode = $proj.CreateElement("ApplicationDisplayVersion")
    $versionNode.InnerText = "CSP 01A1"
    $proj.Project.PropertyGroup[0].AppendChild($versionNode) | Out-Null
}

$currentVersion = $versionNode.ApplicationDisplayVersion
Write-Host "Current version: $currentVersion" -ForegroundColor Yellow

# Parse the version (CSP 01A1 format)
if ($currentVersion -match 'CSP (\d+)([A-Z])(\d)') {
    $major = $matches[1]  # "01" - never changes
    $letter = $matches[2]  # "A" - increments when number reaches 9
    $number = [int]$matches[3]  # "1" - increments 1-9
    
    # Increment the number
    $number++
    
    # If number exceeds 9, reset to 1 and increment letter
    if ($number -gt 9) {
        $number = 1
        $letterCode = [int][char]$letter
        $letterCode++
        
        # Check if we've exceeded Z
        if ($letterCode -gt [int][char]'Z') {
            Write-Host "ERROR: Version has reached maximum (CSP 01Z9)!" -ForegroundColor Red
            exit 1
        }
        
        $letter = [char]$letterCode
    }
    
    $newVersion = "CSP $major$letter$number"
    Write-Host "New version: $newVersion" -ForegroundColor Green
    
    # Update the version in the XML
    $versionNode.ApplicationDisplayVersion = $newVersion
    
    # Update InformationalVersion (used by VersionInfo.GetVersion())
    $infoVersionNode = $proj.Project.PropertyGroup | Where-Object { $_.InformationalVersion } | Select-Object -First 1
    if ($null -eq $infoVersionNode) {
        $infoVersionNode = $proj.CreateElement("InformationalVersion")
        $proj.Project.PropertyGroup[0].AppendChild($infoVersionNode) | Out-Null
    }
    $infoVersionNode.InformationalVersion = $newVersion
    
    # Also update the numeric version for AssemblyVersion
    # Convert to numeric: A=1, B=2, etc.
    $letterNum = ([int][char]$letter) - ([int][char]'A') + 1
    $numericVersion = "1.$letterNum.$number.0"
    
    $assemblyVersionNode = $proj.Project.PropertyGroup | Where-Object { $_.AssemblyVersion } | Select-Object -First 1
    if ($assemblyVersionNode) {
        $assemblyVersionNode.AssemblyVersion = $numericVersion
        $assemblyVersionNode.FileVersion = $numericVersion
        $assemblyVersionNode.Version = $numericVersion
    }
    
    # Save the project file
    $proj.Save($projectPath)
    
    Write-Host "Version updated successfully to $newVersion ($numericVersion)" -ForegroundColor Green
} else {
    Write-Host "ERROR: Invalid version format. Expected 'CSP 01A1', got '$currentVersion'" -ForegroundColor Red
    exit 1
}
