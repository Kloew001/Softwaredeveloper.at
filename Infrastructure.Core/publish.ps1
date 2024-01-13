
Set-Location C:\Development\Softwaredeveloper.at\Infrastructure.Core

Set-Variable -Name "VERSION" -Value "0.0.0.60"

Set-Variable -Name "package" -Value "SoftwaredeveloperDotAt.Infrastructure.Core"

dotnet build --configuration Debug
dotnet pack --configuration Debug /p:Version=$VERSION --no-build --output .\nuget
#dotnet nuget push SoftwaredeveloperDotAt.Infrastructure.Core.${VERSION}.nupkg --api-key ${NUGET_TOKEN} --source https://api.nuget.org/v3/index.json

copy-item .\nuget\$package.$VERSION.nupkg C:\Development\MTGM\LocalShared

#Set-Location -Path  C:\Development\MTGM

#Get-ChildItem *.csproj -Recurse `
#Update-Package $package -Version $VERSION -ProjectName MyProject

#foreach ($project in Get-Project) {
#    Update-Package $package.Id -Project:$project.Name 
#}