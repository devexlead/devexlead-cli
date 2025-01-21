# devex-cli
Developer Experience CLI


## Install CI/CD Version

1. Go to GitHub → Settings → Developer settings → Personal access tokens → Tokens (classic)
2. Generate Personal Access Token (PAT) has the required scope: `read:packages` and `repo` 
3. Run `dotnet nuget add source https://nuget.pkg.github.com/devexlead/index.json --name DevExLeadPackages --username <your-github-username> --password <your-github-token>`
4. Check sources `dotnet nuget list source`
5. Run `dotnet tool update devex --global --no-cache`
6. Check global tools `dotnet tool list -g`
