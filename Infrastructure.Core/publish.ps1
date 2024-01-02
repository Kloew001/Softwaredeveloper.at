dotnet build --configuration Debug
dotnet pack --configuration Debug /p:Version=0.0.0.14 --no-build --output .
#dotnet nuget push SoftwaredeveloperDotAt.Infrastructure.Core.${VERSION}.nupkg --api-key ${NUGET_TOKEN} --source https://api.nuget.org/v3/index.json

