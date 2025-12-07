param(
    [switch]$VerboseOutput
)

Write-Host "Publishing CurveEditor as Windows single-file executable (Release, win-x64)..."

$arguments = @(
    "publish",
    "src/CurveEditor",
    "-c", "Release",
    "-p:PublishProfile=WinSingleFile"
)

if ($VerboseOutput) {
    $arguments += "-v:normal"
}

dotnet @arguments

Write-Host "Publish complete. Output is under src/CurveEditor/bin/Release/net8.0/win-x64/publish" -ForegroundColor Green
