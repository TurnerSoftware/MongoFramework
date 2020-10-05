$repo = $env:GITHUB_REPOSITORY;
$commitHash = $env:GITHUB_SHA;
$url = "https://api.github.com/repos/$($repo)/$($commitHash)/pulls";
echo $url;
$json = Invoke-WebRequest $url -Headers @{Accept = "application/vnd.github.groot-preview+json"};
$obj = ConvertFrom-Json $json;
echo "::set-env name=GITHUB_PR::$($obj.number)";