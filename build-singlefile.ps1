param(
    [switch]$VerboseOutput
)

Write-Host "Publishing MotorEditor as Windows single-file executable (Release, win-x64)..."

$arguments = @(
    "publish",
    "src/MotorEditor.Avalonia",
    "-c", "Release",
    "-p:PublishProfile=WinSingleFile"
)

if ($VerboseOutput) {
    $arguments += "-v:normal"
}

dotnet @arguments

Write-Host "Publish complete. Output is under src/MotorEditor.Avalonia/bin/Release/net8.0/win-x64/publish" -ForegroundColor Green
