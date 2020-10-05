$repo = $env:GITHUB_REPOSITORY;
$commitHash = $env:GITHUB_SHA;
$url = "https://api.github.com/repos/$($repo)/commits/$($commitHash)/pulls";
$json = Invoke-WebRequest $url -Headers @{Accept = "application/vnd.github.groot-preview+json"};
$obj = ConvertFrom-Json $json;
echo $obj;
echo "::set-env name=PR_NUMBER::$($obj.number)";