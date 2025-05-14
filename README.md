
Add Local Nuget Source: 
```bash
dotnet nuget add source "D:\*\.net-microservice\packages" --name Play.Economy
```

Check Nuget Source: 
```bash
dotnet nuget list source 
```

Remove Nuget Source: 
```bash
dotnet nuget remove source Play.Economy
```

Clear nuget cache: 
```bash
dotnet nuget locals all --list
dotnet nuget locals global-packages --clear
```

Create Common Package: 
```bash
dotnet pack -o ../../../packages -p:PackageVersion=1.0.x
```

Pack And Push To GitHub Registry: 
```bash
version="1.0.6"
owner="net-microservices"
gh_pat="[YOUR_GITHUB_PAT]"

dotnet pack src/Play.Common \
  --configuration Release \
  -p:PackageVersion=$version \
  -p:RepositoryUrl=https://github.com/$owner/Play.Common \
  -o ../packages

dotnet nuget push ../packages/Play.Common.$version.nupkg \
  --api-key "$gh_pat" \
  --source "github"
```
Add Source For Shared Nuget Packages
```bash

owner="net-microservices"
gh_pat="[YOUR_GITHUB_PAT]"

dotnet nuget add source \
  --username USERNAME \
  --password "$gh_pat" \
  --store-password-in-clear-text \
  --name github \
  "https://nuget.pkg.github.com/$owner/index.json"
```
