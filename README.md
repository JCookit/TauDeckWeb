# TauDeckWeb

This repository is the static-site migration of the Tau deck library. GitHub Pages serves the home page, Cards browser, Matrix, and card detail pages for both editions. Systems, Articles, and Games link to the live site at [taudeck.com](https://taudeck.com).

The export runs the Razor app on loopback during the build and writes static HTML and assets. The deployed site needs no server process. Edition routes live under `/v1/` and `/v2/`, with the unversioned home, Cards, and Matrix routes forwarding to the default edition.

To export locally with .NET 10 and PowerShell 7.4 or later:

```powershell
dotnet build src/TauWeb.Web/TauWeb.Web.csproj --configuration Release
pwsh -File scripts/export-pages.ps1 -PagesRoot ''
```

Serve `_site` at the root of a local static web server. Omit `-PagesRoot ''` to build for the GitHub Pages project path `/TauDeckWeb/`.
