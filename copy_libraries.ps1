# Get the directory where the script is located
$scriptDir = $PSScriptRoot

# Define the source directory (venv) and target directory (project) relative to the script location
$sourceDir = Join-Path $scriptDir "venv\lib\site-packages"
$targetDir = Join-Path $scriptDir "src\jastudio_src\_lib"

# One line per library and its dependencies
$pipLibrariesToCopy = @(
    "pythonnet", "clr_loader", "cffi", "_cffi_backend.cp313-win_amd64.pyd", "pycparser",
    "typed_linq_collections",
    "pyperclip",
    "beartype",
    "autoslot.py"
)

# Create the target directory if it doesn't exist
New-Item -ItemType Directory -Path $targetDir -Force | Out-Null

# Copy the pip libraries
foreach ($library in $pipLibrariesToCopy) {
    $sourcePath = Join-Path $sourceDir $library
    $targetPath = Join-Path $targetDir $library

    Write-Host "Copying $sourcePath       =>       $targetPath"

    $isDir = Test-Path $sourcePath -PathType Container

    # Remove the existing target if it exists
    if (Test-Path $targetPath) {
        Remove-Item $targetPath -Recurse -Force
    }

    # Copy directory or file
    if ($isDir) {
        Copy-Item $sourcePath $targetPath -Recurse
    } else {
        Copy-Item $sourcePath $targetPath
    }
}

Write-Host "Libraries copied successfully!"
