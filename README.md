# Developer Experience CLI

[![CI](https://github.com/devexlead/devex-cli/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/devexlead/devex-cli/actions/workflows/ci-cd.yml)
[![CI](https://github.com/devexlead/devex-cli/actions/workflows/release.yml/badge.svg)](https://github.com/devexlead/devex-cli/actions/workflows/release.yml)



## Install CI/CD Version

1. Go to `GitHub` → `Settings` → `Developer settings` → `Personal access tokens` → `Tokens (classic)`
2. Generate Personal Access Token (PAT) with the required scope: `read:packages` and `repo` 
3. Run `dotnet nuget remove source DevExLeadPackages` and `dotnet nuget add source https://nuget.pkg.github.com/devexlead/index.json --name DevExLeadPackages --username <your-github-username> --password <your-github-token>`
4. Check sources `dotnet nuget list source`
5. Check the version you want to install from https://github.com/devexlead/devex-cli/pkgs/nuget/dxc
6. Run `dotnet tool update --global dxc --no-cache --version <version> --verbosity detailed`
7. Check global tools `dotnet tool list -g`

**Troubleshooting**
- Ensure your `nuget.config` file has the correct source and credentials. Open or create `nuget.config` in your user profile directory (e.g., `%AppData%\NuGet\nuget.config` on Windows).
- Test Authentication `& "C:\Windows\System32\curl.exe" -u "$username:$pat" https://nuget.pkg.github.com/devexlead/index.json`

