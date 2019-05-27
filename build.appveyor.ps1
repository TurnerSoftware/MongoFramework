[CmdletBinding(PositionalBinding=$false)]
param(
    [bool] $IsTagBuild
)


Write-Host "Initialising AppVeyor build..." -ForegroundColor "Magenta"

if ($IsTagBuild) {
	.\build.ps1 -CreatePackages $True
}
else {
	.\build.ps1 -CheckCoverage $True
}