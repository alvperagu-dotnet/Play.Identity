# Play.Identity
Common libraries used by Play Economy services.

## Create and publish package
```powershell
$version="1.0.3"
$owner="alvperagu-dotnet"
$gh_pat="[TOKEN HERE]"
$name="Play.Identity.Contracts"

dotnet pack src\$name\ --configuration Release -p:PackageVersion=$version -p:RepositoryUrl=https://github.com/$owner/$name -o ..\packages

dotnet nuget push ..\packages\$name.$version.nupkg --api-key $gh_pat --source "github"
```