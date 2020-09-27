$baseBuildVersion = git describe --tags --abbrev=0
if ($baseBuildVersion -contains "fatal") {
	$baseBuildVersion = "0.0.0"
}

$isTagBuild = $False
$buildMetadata = "$($env:GITHUB_SHA.substring(0,7))-$($env:GITHUB_RUN_ID)"
$prereleaseVersion = "dev"

if ($env:GITHUB_REF -ne "false") {
	$baseBuildVersion = $env:GITHUB_REF
	$prereleaseVersion = $False
	$isTagBuild = $True
}

$buildVersion = "$baseBuildVersion+$buildMetadata"
if ($prereleaseVersion) {
	$buildVersion = "$baseBuildVersion-$prereleaseVersion+$buildMetadata"
}

Write-Host $buildVersion
Exit 1