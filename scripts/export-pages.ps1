$ErrorActionPreference = 'Stop'

$repo = Split-Path -Parent $PSScriptRoot
$webRoot = Join-Path $repo 'src/TauWeb.Web/wwwroot'
$output = Join-Path $repo '_site'
New-Item -ItemType Directory -Path $output -Force | Out-Null

$editions = [ordered]@{ v1 = 'Initial'; v2 = 'Production' }
$summaries = foreach ($edition in $editions.Keys) {
    $dataPath = Join-Path $webRoot "editions/$edition/content/tau-data.json"
    $data = Get-Content -LiteralPath $dataPath -Raw | ConvertFrom-Json
    if (-not $data.cards.Count) {
        throw "No cards found in $dataPath"
    }
    "<li><strong>$($editions[$edition])</strong> ($edition) · $($data.cards.Count) cards</li>"
}

Copy-Item -LiteralPath (Join-Path $webRoot 'favicon.svg') -Destination $output
Copy-Item -LiteralPath (Join-Path $webRoot 'editions/v2/generated/glyphs/cardback.webp') -Destination $output

@"
<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Tau Deck Library · Static Preview</title>
    <link rel="icon" href="./favicon.svg" type="image/svg+xml">
    <style>
        body { background: #f6f3ed; color: #211b28; font: 1rem/1.6 system-ui, sans-serif; }
        main { max-width: 38rem; margin: 3rem auto; padding: 0 1rem; }
        img { width: 7rem; height: auto; }
        a { color: #59388d; }
    </style>
</head>
<body>
    <main>
        <img src="./cardback.webp" alt="Tau Production card back">
        <h1>Tau Deck Library</h1>
        <p>This static preview is generated from the two Tau deck editions.</p>
        <ul>
            $($summaries -join "`n            ")
        </ul>
        <p><a href="https://taudeck.com/">Visit the full Tau Deck Library</a></p>
    </main>
</body>
</html>
"@ | Set-Content -LiteralPath (Join-Path $output 'index.html') -Encoding utf8
