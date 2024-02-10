

Set-Variable -Name "Projekt" -Value "RWA"
Set-Variable -Name "TargetPath" -Value C:\Development\$Projekt
Set-Variable -Name "VERSION" -Value "0.0.0.177"

Remove-Item $TargetPath\LocalShared\*.*

Set-Location C:\Development\Softwaredeveloper.at\Infrastructure.Core
Set-Variable -Name "package" -Value "SoftwaredeveloperDotAt.Infrastructure.Core"
dotnet build --configuration Debug
dotnet pack --configuration Debug /p:Version=$VERSION --no-build --output .\nuget
#dotnet nuget push SoftwaredeveloperDotAt.Infrastructure.Core.${VERSION}.nupkg --api-key ${NUGET_TOKEN} --source https://api.nuget.org/v3/index.json
copy-item .\nuget\$package.$VERSION.nupkg $TargetPath\LocalShared

Set-Location $TargetPath\$Projekt.Application
dotnet add $Projekt.Application package $package -v $VERSION
Set-Location $TargetPath\$Projekt.Server
dotnet add $Projekt.Server package $package -v $VERSION


Set-Location C:\Development\Softwaredeveloper.at\Infrastructure.Core.SqlServer
Set-Variable -Name "package" -Value "SoftwaredeveloperDotAt.Infrastructure.Core.SqlServer"
Set-Variable -Name "packageId" -Value $package.$VERSION.nupkg
dotnet build --configuration Debug
dotnet pack --configuration Debug /p:Version=$VERSION --no-build --output .\nuget
copy-item .\nuget\$package.$VERSION.nupkg $TargetPath\LocalShared

Set-Location $TargetPath\$Projekt.Application
dotnet add $Projekt.Application package $package -v $VERSION
Set-Location $TargetPath\$Projekt.Server
dotnet add $Projekt.Server package $package -v $VERSION


Set-Location C:\Development\Softwaredeveloper.at\Infrastructure.Core.Web
Set-Variable -Name "package" -Value "SoftwaredeveloperDotAt.Infrastructure.Core.Web"
Set-Variable -Name "packageId" -Value $package.$VERSION.nupkg
dotnet build --configuration Debug
dotnet pack --configuration Debug /p:Version=$VERSION --no-build --output .\nuget
copy-item .\nuget\$package.$VERSION.nupkg $TargetPath\LocalShared

Set-Location $TargetPath\$Projekt.Server
dotnet add $Projekt.Server package $package -v $VERSION

