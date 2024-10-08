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

## Build the docker image
```powershell
$env:GH_OWNER="alvperagu-dotnet"
$env:GH_PAT ="[PAT HERE]"

docker build --secret id=GH_OWNER --secret id=GH_PAT -t play.identity:$version .
```

### Run the docker image
```powershell
$admingPass="[Password here]"
docker run -it --rm -p 5002:5002 --name identity -e MongoDbSettings__Host=mongo -e RabbitMQSettings__Host=rabbitmq -e IdentitySettings__AdminUserPassword=$admingPass --network playinfra_default play.identity:$version
``` 
