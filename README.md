# TauDeckWeb

This repository is the static-site migration of the Tau deck library. The current Pages preview renders a deck summary from both editions. The copied Razor application is the reference for migrating the interactive pages.

Run `pwsh -File scripts/export-pages.ps1` from the repository root to generate the preview in `_site`. The GitHub Actions workflow builds the reference application with .NET 10 and deploys only the generated preview to Pages.

The full library is at [taudeck.com](https://taudeck.com).
