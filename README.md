
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
Build docker image with secrets
```bash

version="1.0.0"
export GH_OWNER="net-microservices" # must correspond to the ids used in the Dockerfile
export GH_PAT="[PAT HERE]"

docker build --secret id=GH_OWNER --secret id=GH_PAT -t play.identity:$version .

```

Run Play.Identity docker image with infra network
```bash
adminpass="PASS"

docker run -it --rm -p 5002:5002 --name identity \ 
-e MongoDbSettings__Host=mongo \
-e RabbitMQSettings__Host=rabbitmq \
-e IdentitySettings__AdminUserPassword=$adminpass \
--network playinfra_default play.identity:1.0.x
```