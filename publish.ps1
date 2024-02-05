
Set-Variable -Name "TargetPath" -Value "C:\Development\MTGM"
Set-Variable -Name "VERSION" -Value "0.0.0.127"
Remove-Item $TargetPath\LocalShared\*.*

Set-Location C:\Development\Softwaredeveloper.at\Infrastructure.Core
Set-Variable -Name "package" -Value "SoftwaredeveloperDotAt.Infrastructure.Core"
dotnet build --configuration Debug
dotnet pack --configuration Debug /p:Version=$VERSION --no-build --output .\nuget
#dotnet nuget push SoftwaredeveloperDotAt.Infrastructure.Core.${VERSION}.nupkg --api-key ${NUGET_TOKEN} --source https://api.nuget.org/v3/index.json
copy-item .\nuget\$package.$VERSION.nupkg $TargetPath\LocalShared

Set-Location $TargetPath
dotnet add MTGM.Application package $package -v $VERSION
dotnet add MTGM.Server package $package -v $VERSION


Set-Location C:\Development\Softwaredeveloper.at\Infrastructure.Core.PostgreSQL
Set-Variable -Name "package" -Value "SoftwaredeveloperDotAt.Infrastructure.Core.PostgreSQL"
Set-Variable -Name "packageId" -Value $package.$VERSION.nupkg
dotnet build --configuration Debug
dotnet pack --configuration Debug /p:Version=$VERSION --no-build --output .\nuget
copy-item .\nuget\$package.$VERSION.nupkg $TargetPath\LocalShared

Set-Location $TargetPath
dotnet add MTGM.Application package $package -v $VERSION
dotnet add MTGM.Server package $package -v $VERSION


Set-Location C:\Development\Softwaredeveloper.at\Infrastructure.Core.Web
Set-Variable -Name "package" -Value "SoftwaredeveloperDotAt.Infrastructure.Core.Web"
Set-Variable -Name "packageId" -Value $package.$VERSION.nupkg
dotnet build --configuration Debug
dotnet pack --configuration Debug /p:Version=$VERSION --no-build --output .\nuget
copy-item .\nuget\$package.$VERSION.nupkg $TargetPath\LocalShared

Set-Location $TargetPath
dotnet add MTGM.Server package $package -v $VERSION

