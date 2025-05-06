
Clear nuget cache: 
```bash
dotnet nuget locals all --list
dotnet nuget locals global-packages --clear
```

Create Common Package: 
```bash
dotnet pack -o ../../../packages -p:PackageVersion=1.0.x
```