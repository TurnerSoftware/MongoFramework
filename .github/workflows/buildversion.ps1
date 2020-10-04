$githubRunId = $env:GITHUB_RUN_ID;
$prNumber = $env:GITHUB_PR;
$gitSourceVersion = git describe --tags --abbrev=7 --always 2>$1;
$gitSourceVersionSplit = [regex]::split($gitSourceVersion, "-(?=\d+-\w+)");
$version = $(if($gitSourceVersionSplit.length -eq 1){"0.0.0"}else{$gitSourceVersionSplit[0]});
$commitsSinceTag = '0';
$commitHash = $gitSourceVersionSplit[0];
if ($gitSourceVersionSplit.length -eq 2) {
    $gitMetadata = $gitSourceVersionSplit[1].split("-");
    $commitsSinceTag = $gitMetadata[0];
    $commitHash = $gitMetadata[1];
}
$buildMetadata = "$($commitHash)-$($githubRunId)";
$customSuffix = $(if($prNumber -ne ''){"-PR$($prNumber)"}elseif($commitsSinceTag -ne '0'){"-dev"});
echo "::set-env name=BUILD_VERSION::$($version)$($customSuffix)+$($buildMetadata)";