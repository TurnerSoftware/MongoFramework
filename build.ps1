[CmdletBinding(PositionalBinding=$false)]
param(
    [bool] $CreatePackages,
    [bool] $RunTests = $true,
	[bool] $CheckCoverage,
    [string] $PullRequestNumber
)

Write-Host "Run Parameters:" -ForegroundColor Cyan
Write-Host "  CreatePackages: $CreatePackages"
Write-Host "  RunTests: $RunTests"
Write-Host "  dotnet --version:" (dotnet --version)

$packageOutputFolder = "$PSScriptRoot\build-artifacts"

if ($PullRequestNumber) {
    Write-Host "Building for a pull request (#$PullRequestNumber), skipping packaging." -ForegroundColor Yellow
    $CreatePackages = $false
}

Write-Host "Building solution... " -ForegroundColor "Magenta"
dotnet build -c Release
if ($LastExitCode -ne 0) {
    Write-Host "Build failed, aborting!" -Foreground "Red"
    Exit 1
}
Write-Host "Solution built!" -ForegroundColor "Green"

if ($RunTests) {
    Write-Host "Running tests... " -ForegroundColor "Magenta"
	dotnet test tests/MongoFramework.Tests/MongoFramework.Tests.csproj
	if ($LastExitCode -ne 0) {
        Write-Host "Tests failed, aborting build!" -Foreground "Red"
        Exit 1
    }
	Write-Host "Tests passed!" -ForegroundColor "Green"
}

if ($CheckCoverage) {
	Write-Host "Checking code coverage... " -ForegroundColor "Magenta"
	OpenCover.Console.exe -register:user -target:"%LocalAppData%\Microsoft\dotnet\dotnet.exe" -targetargs:"test tests/MongoFramework.Tests/MongoFramework.Tests.csproj /p:DebugType=Full" -filter:"+[MongoFramework]* -[MongoFramework.Tests]*" -output:"$packageOutputFolder\coverage.xml" -oldstyle
    if ($LastExitCode -ne 0 -Or -Not $?) {
        Write-Host "Failure checking code coverage!" -Foreground "Red"
    }
	else {
		Write-Host "Saving code coverage... " -ForegroundColor "Magenta"
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
    mkdir -Force $packageOutputFolder | Out-Null
    Write-Host "Clearing existing $packageOutputFolder... " -NoNewline
    Get-ChildItem $packageOutputFolder | Remove-Item
    Write-Host "Packages cleared!" -ForegroundColor "Green"
	
    Write-Host "Packing... " -ForegroundColor "Magenta"
	dotnet pack --no-build -c Release /p:PackageOutputPath=$packageOutputFolder
	Write-Host "Packing complete!" -ForegroundColor "Green"
}