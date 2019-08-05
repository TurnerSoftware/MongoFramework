[CmdletBinding(PositionalBinding=$false)]
param(
    [bool] $RunTests = $true,
	[bool] $CheckCoverage,
    [bool] $CreatePackages
)

$packageOutputFolder = "$PSScriptRoot\build-artifacts"
mkdir -Force $packageOutputFolder | Out-Null

Write-Host "Run Parameters:" -ForegroundColor Cyan
Write-Host "  RunTests: $RunTests"
Write-Host "  CheckCoverage: $CheckCoverage"
Write-Host "  CreatePackages: $CreatePackages"
Write-Host "Environment:" -ForegroundColor Cyan
Write-Host "  .NET Version:" (dotnet --version)
Write-Host "  Artifact Path: $packageOutputFolder"

Write-Host "Building solution..." -ForegroundColor "Magenta"
dotnet build -c Release
if ($LastExitCode -ne 0) {
    Write-Host "Build failed, aborting!" -Foreground "Red"
    Exit 1
}
Write-Host "Solution built!" -ForegroundColor "Green"

if ($RunTests -And -Not $CheckCoverage) {
    Write-Host "Running tests without coverage..." -ForegroundColor "Magenta"
	dotnet test tests/MongoFramework.Tests/MongoFramework.Tests.csproj
	if ($LastExitCode -ne 0) {
        Write-Host "Tests failed, aborting build!" -Foreground "Red"
        Exit 1
    }
	Write-Host "Tests passed!" -ForegroundColor "Green"
}
elseif ($RunTests -And $CheckCoverage) {
	Write-Host "Running tests with coverage..." -ForegroundColor "Magenta"
	Invoke-Expression ("$env:LOCALAPPDATA\Apps\OpenCover\" + 'OpenCover.Console.exe -register:user -target:"%LocalAppData%\Microsoft\dotnet\dotnet.exe" -targetargs:"test tests/MongoFramework.Tests/MongoFramework.Tests.csproj /p:DebugType=Full" -filter:"+[MongoFramework]* -[MongoFramework.Tests]*" -output:"' + $packageOutputFolder + '\coverage.xml" -oldstyle')
    if ($LastExitCode -ne 0 -Or -Not $?) {
        Write-Host "Failure performing tests with coverage, aborting!" -Foreground "Red"
		Exit 1
    }
	else {
		Write-Host "Tests passed!" -ForegroundColor "Green"
		Write-Host "Saving code coverage..." -ForegroundColor "Magenta"
		codecov -f "$packageOutputFolder\coverage.xml"
		if ($LastExitCode -ne 0 -Or -Not $?) {
			Write-Host "Failure saving code coverage!" -Foreground "Red"
		}
		else {
			Write-Host "Coverage saved!" -ForegroundColor "Green"
		}
	}
}

if ($CreatePackages) {
    Write-Host "Clearing existing $packageOutputFolder... " -NoNewline
    Get-ChildItem $packageOutputFolder | Remove-Item
    Write-Host "Packages cleared!" -ForegroundColor "Green"
	
    Write-Host "Packing..." -ForegroundColor "Magenta"
	dotnet pack --no-build -c Release /p:PackageOutputPath=$packageOutputFolder
	Write-Host "Packing complete!" -ForegroundColor "Green"
}