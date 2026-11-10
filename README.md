# TauDeckWeb

GitHub Pages serves the Tau deck library. Cards, Matrix, card details, and Systems have edition routes under `/v1/` and `/v2/`. The unversioned home, Cards, Matrix, and Systems indexes forward to the default edition. Articles and the prelaunch Games page are shared by both editions.

The export runs the Razor app on loopback during the build and writes static HTML and assets. The deployed site needs no server process. Article Markdown in `src/TauWeb.Web/Content/Articles/` controls which titles appear and which article pages and Markdown downloads are generated.

To export locally with .NET 10 and PowerShell 7.4 or later:

```powershell
dotnet build src/TauWeb.Web/TauWeb.Web.csproj --configuration Release
pwsh -File scripts/export-pages.ps1 -PagesRoot ''
```

Serve `_site` at the root of a local static web server. Omit `-PagesRoot ''` to build for the GitHub Pages project path `/TauDeckWeb/`.
