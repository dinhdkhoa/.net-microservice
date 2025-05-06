
Add Local Nuget Source: 
```bash
dotnet nuget add source "D:\*\.net-microservice\packages" --name Play.Economy
```

Check Nuget Source: 
```bash
dotnet nuget list source
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