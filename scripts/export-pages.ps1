param(
    [ValidatePattern('^(?:/[A-Za-z0-9_-]+)?$')]
    [string] $PagesRoot = '/TauDeckWeb'
)

$ErrorActionPreference = 'Stop'
$repo = Split-Path -Parent $PSScriptRoot
$web = Join-Path $repo 'src/TauWeb.Web'
$webRoot = Join-Path $web 'wwwroot'
$dll = Join-Path $web 'bin/Release/net10.0/TauWeb.Web.dll'
$output = Join-Path $repo '_site'
$config = Get-Content -LiteralPath (Join-Path $web 'appsettings.json') -Raw | ConvertFrom-Json
$defaultEdition = $config.Editions.Default

if (-not (Test-Path -LiteralPath $dll -PathType Leaf)) {
    throw 'Build src/TauWeb.Web/TauWeb.Web.csproj in Release before exporting.'
}

function Write-Page([string] $Route, [string] $Html) {
    $folder = Join-Path $output $Route.TrimStart('/')
    New-Item -ItemType Directory -Path $folder -Force | Out-Null
    Set-Content -LiteralPath (Join-Path $folder 'index.html') -Value $Html -Encoding utf8
}

function Export-Page([string] $Route, [string] $Edition) {
    $url = "$origin$Route"
    $outputRoute = $Route
    if ($Edition) {
        $url += "?edition=$Edition"
        $outputRoute = "/$Edition$Route"
    }
    $html = (Invoke-WebRequest -Uri $url -ConnectionTimeoutSeconds 5 -OperationTimeoutSeconds 30).Content
    # The export copies source assets. Resolve Razor's build-specific asset aliases.
    $html = [regex]::Replace($html, '(?s)<script type="importmap">.*?</script>\s*', '')
    $html = $html.Replace('"/editions/', '"' + $PagesRoot + '/editions/')
    $html = $html.Replace('<body data-edition=', '<body data-pages-root="' + $PagesRoot + '" data-edition=')
    $html = [regex]::Replace($html, '\b(?:href|src)="(/(?!/)[^"]*)"', {
        param($match)
        $value = [System.Net.WebUtility]::HtmlDecode($match.Groups[1].Value)
        $location = $value -split '#', 2
        $fragment = if ($location.Count -eq 2) { '#' + $location[1] } else { '' }
        $parts = $location[0] -split '\?', 2
        $path = $parts[0]
        $assetPath = $path
        if ($PagesRoot -and $assetPath.StartsWith("$PagesRoot/")) {
            $assetPath = $assetPath.Substring($PagesRoot.Length)
        }
        if ($assetPath -match '^/(css/|js/|editions/|favicon\.)') {
            $sourcePath = $assetPath -replace '\.[a-z0-9]{10}(?=\.[a-z0-9]+$)', ''
            if (Test-Path -LiteralPath (Join-Path $webRoot $assetPath.TrimStart('/')) -PathType Leaf) {
                $sourcePath = $assetPath
            }
            if (-not (Test-Path -LiteralPath (Join-Path $webRoot $sourcePath.TrimStart('/')) -PathType Leaf)) {
                throw "Missing source asset for $value"
            }
            $destination = $PagesRoot + $sourcePath
        }
        elseif ($path -match '^/articles/([a-z0-9-]+)$' -and $parts.Count -eq 2 -and $parts[1] -eq 'handler=Source') {
            $destination = "$PagesRoot$path/$($Matches[1]).md"
        }
        elseif ($path -match '^/(articles|games)(?:/|$)') {
            $destination = $PagesRoot + $path.TrimEnd('/').ToLowerInvariant() + '/'
            if ($parts.Count -eq 2) {
                $destination += '?' + $parts[1]
            }
        }
        elseif ($path -eq '/' -or $path -match '^/(cards|systems)(?:/|$)') {
            $targetEdition = if ($Edition) { $Edition } else { $defaultEdition }
            $query = @()
            if ($parts.Count -gt 1) {
                foreach ($parameter in ($parts[1] -split '&')) {
                    if ($parameter -match '^edition=(v[0-9]+)$') {
                        $targetEdition = $Matches[1]
                    }
                    elseif ($parameter) {
                        $query += $parameter
                    }
                }
            }
            $destination = "$PagesRoot/$targetEdition" + $path.TrimEnd('/').ToLowerInvariant() + '/'
            if ($query.Count) {
                $destination += '?' + ($query -join '&')
            }
        }
        else {
            return $match.Value
        }

        return $match.Value.Substring(0, $match.Groups[1].Index - $match.Index) +
            [System.Net.WebUtility]::HtmlEncode($destination + $fragment) + '"'
    })

    Write-Page $outputRoute $html
}

function Write-Redirect([string] $Route, [string] $Target) {
    $title = 'Tau deck library'
    Write-Page $Route @"
<!doctype html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>$title</title>
    <link rel="icon" href="$PagesRoot/favicon.svg">
</head>
<body>
    <p><a id="destination" href="$Target">Continue to $title</a></p>
    <script>
        const destination = document.getElementById("destination");
        const url = new URL(destination.href);
        url.search = location.search;
        url.hash = location.hash;
        location.replace(url);
    </script>
</body>
</html>
"@
}

$listener = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, 0)
$listener.Start()
$port = $listener.LocalEndpoint.Port
$listener.Stop()
$origin = "http://127.0.0.1:$port"
$serverArgs = @{
    FilePath = 'dotnet'
    ArgumentList = @('"' + $dll + '"', '--urls', $origin, '--environment', 'Development', '--GameStore:Provider=Json', '--Games:ShowLibrary=false')
    WorkingDirectory = $web
    PassThru = $true
}
if ($IsWindows) { $serverArgs.WindowStyle = 'Hidden' }
$server = Start-Process @serverArgs

try {
    $ready = $false
    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        if ($server.HasExited) { throw "Razor host exited with code $($server.ExitCode)." }
        try {
            $null = Invoke-WebRequest -Uri $origin -ConnectionTimeoutSeconds 1 -OperationTimeoutSeconds 2
            $ready = $true
            break
        }
        catch {
            Start-Sleep -Milliseconds 250
        }
    }
    if (-not $ready) { throw 'Razor host did not respond.' }

    if (Test-Path -LiteralPath $output) {
        $resolvedOutput = (Resolve-Path -LiteralPath $output).ProviderPath
        if ($resolvedOutput -ne [IO.Path]::GetFullPath((Join-Path $repo '_site')) -or
            (Get-Item -LiteralPath $output).Attributes.HasFlag([IO.FileAttributes]::ReparsePoint)) {
            throw "Unexpected output directory: $resolvedOutput"
        }
        Remove-Item -LiteralPath $resolvedOutput -Recurse -Force
    }
    New-Item -ItemType Directory -Path $output | Out-Null
    foreach ($asset in 'css', 'js', 'favicon.svg') {
        Copy-Item -LiteralPath (Join-Path $webRoot $asset) -Destination $output -Recurse
    }
    New-Item -ItemType File -Path (Join-Path $output '.nojekyll') | Out-Null

    foreach ($edition in $config.Editions.Items.Id) {
        $editionAssets = Join-Path $output "editions/$edition"
        New-Item -ItemType Directory -Path $editionAssets | Out-Null
        Copy-Item -LiteralPath (Join-Path $webRoot "editions/$edition/generated") -Destination $editionAssets -Recurse

        $anatomy = Join-Path $webRoot "editions/$edition/content/card-anatomy.svg"
        if (Test-Path -LiteralPath $anatomy -PathType Leaf) {
            $editionContent = Join-Path $editionAssets 'content'
            New-Item -ItemType Directory -Path $editionContent | Out-Null
            Copy-Item -LiteralPath $anatomy -Destination $editionContent
        }

        $data = Get-Content -LiteralPath (Join-Path $webRoot "editions/$edition/content/tau-data.json") -Raw | ConvertFrom-Json
        $slugs = @($data.cards.slug)
        if (-not $slugs.Count -or ($slugs | Where-Object { $_ -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$' -or $_ -eq 'matrix' }) -or
            ($slugs | Sort-Object -Unique).Count -ne $slugs.Count) {
            throw "Invalid card routes for $edition."
        }
        foreach ($route in '/', '/cards', '/cards/matrix', '/systems') {
            Export-Page $route $edition
        }
        foreach ($card in $data.cards) {
            Export-Page "/cards/$($card.slug)" $edition
        }
        foreach ($system in $data.systems) {
            if ($system.key -cnotmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') {
                throw "Invalid system route in $edition."
            }
            Export-Page "/systems/$($system.key)" $edition
        }
        Write-Host "Exported $edition with $($data.cards.Count) card details and $($data.systems.Count) systems."
    }

    Export-Page '/articles'
    Export-Page '/games'
    $articleFiles = Get-ChildItem -LiteralPath (Join-Path $web 'Content/Articles') -Filter '*.md' -File
    foreach ($article in $articleFiles) {
        $slug = $article.BaseName
        $route = "/articles/$slug"
        # Razor applies article visibility and status to the source handler and detail page.
        $source = Invoke-WebRequest -Uri "$origin$($route)?handler=Source" -SkipHttpErrorCheck -ConnectionTimeoutSeconds 5 -OperationTimeoutSeconds 30
        if ($source.StatusCode -eq 404) { continue }
        if ($source.StatusCode -ne 200) { throw "Article source request failed for $slug ($($source.StatusCode))." }

        Export-Page $route
        $download = Join-Path $output "articles/$slug/$slug.md"
        [IO.File]::WriteAllBytes($download, $source.RawContentStream.ToArray())
    }

    Write-Redirect '/' "$PagesRoot/$defaultEdition/"
    Write-Redirect '/cards' "$PagesRoot/$defaultEdition/cards/"
    Write-Redirect '/cards/matrix' "$PagesRoot/$defaultEdition/cards/matrix/"
    Write-Redirect '/systems' "$PagesRoot/$defaultEdition/systems/"
}
finally {
    if (-not $server.HasExited) {
        Stop-Process -Id $server.Id
        $server.WaitForExit()
    }
}
