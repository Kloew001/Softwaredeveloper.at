param(
    [string]$SolutionRoot = "."
)

# Für ZipFile/ZipArchive in Windows PowerShell
Add-Type -AssemblyName System.IO.Compression.FileSystem

# ==============================================
# Helper: TargetFramework(s) aus csproj lesen
# ==============================================
function Get-ProjectFrameworks {
    param(
        [string]$CsprojPath
    )

    $content = Get-Content -LiteralPath $CsprojPath -Raw

    $tfList = @()

    # 1) Neuer SDK-Style: <TargetFramework> / <TargetFrameworks>
    $m = [regex]::Match($content, '<TargetFrameworks?\s*>\s*([^<]+)\s*</TargetFrameworks?>', 'IgnoreCase')
    if ($m.Success) {
        $value = $m.Groups[1].Value
        $value.Split(';') | ForEach-Object {
            if (-not [string]::IsNullOrWhiteSpace($_)) {
                $tfList += $_.Trim()
            }
        }
    }

    # 2) Alter .NET Framework-Style: <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    if (-not $tfList -or $tfList.Count -eq 0) {
        $m2 = [regex]::Match($content, '<TargetFrameworkVersion>\s*v?([^<]+)\s*</TargetFrameworkVersion>', 'IgnoreCase')
        if ($m2.Success) {
            $v = $m2.Groups[1].Value.Trim()
            $digits = ($v -replace '[^\d]', '')
            if ($digits.Length -gt 0) {
                $tfList += "net$digits"
            }
        }
    }

    $tfList | Sort-Object -Unique
}

# ==============================================
# Helper: TFMs normalisieren / Kompatibilität
# ==============================================
function Normalize-PackageTfm {
    param(
        [string]$Tfm
    )

    if ([string]::IsNullOrWhiteSpace($Tfm)) { return $null }

    $tfm = $Tfm.ToLowerInvariant()
    $tfm = $tfm.Split('-', 2)[0]   # net8.0-windows10... -> net8.0
    return $tfm
}

function Test-FrameworkCompatible {
    param(
        [string]$ProjectTfm,
        [string]$PackageTfm
    )

    if ([string]::IsNullOrWhiteSpace($PackageTfm)) { return $true }

    $p = $ProjectTfm.ToLowerInvariant()
    $f = $PackageTfm.ToLowerInvariant()

    if ($p -eq $f) { return $true }

    # netstandard2.x: sehr breit kompatibel
    if ($f -like "netstandard2.*") { return $true }

    # netX.Y (z.B. net6.0, net8.0)
    $pMatch = [regex]::Match($p, '^net(\d+)\.(\d+)$')
    $fMatch = [regex]::Match($f, '^net(\d+)\.(\d+)$')
    if ($pMatch.Success -and $fMatch.Success) {
        $pMajor = [int]$pMatch.Groups[1].Value
        $fMajor = [int]$fMatch.Groups[1].Value

        if ($fMajor -le $pMajor) {
            return $true
        }
    }

    # netcoreappX.Y
    $pcMatch = [regex]::Match($p, '^netcoreapp(\d+)\.(\d+)$')
    $fcMatch = [regex]::Match($f, '^netcoreapp(\d+)\.(\d+)$')
    if ($pcMatch.Success -and $fcMatch.Success) {
        $pMajor = [int]$pcMatch.Groups[1].Value
        $fMajor = [int]$fcMatch.Groups[1].Value
        if ($fMajor -le $pMajor) {
            return $true
        }
    }

    return $false
}

# ==============================================
# Helper: NuGet-API
# ==============================================
function Get-PackageVersions {
    param(
        [string]$PackageId
    )

    $idLower = $PackageId.ToLowerInvariant()
    $url = "https://api.nuget.org/v3-flatcontainer/$idLower/index.json"

    try {
        $result = Invoke-RestMethod -UseBasicParsing -Uri $url -Method Get -ErrorAction Stop
        return $result.versions
    }
    catch {
        Write-Warning "  -> Failed to query versions for $PackageId : $_"
        return @()
    }
}

function Get-PackageFrameworksFromNupkg {
    param(
        [string]$PackageId,
        [string]$Version
    )

    $idLower = $PackageId.ToLowerInvariant()
    $vLower  = $Version.ToLowerInvariant()

    $url = "https://api.nuget.org/v3-flatcontainer/$idLower/$vLower/$idLower.$vLower.nupkg"

    $tempFile = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), "$idLower.$vLower.nupkg")

    try {
        Invoke-WebRequest -Uri $url -OutFile $tempFile -UseBasicParsing -ErrorAction Stop
    }
    catch {
        Write-Warning "    -> Failed to download $PackageId $Version : $_"
        if (Test-Path $tempFile) { Remove-Item $tempFile -Force -ErrorAction SilentlyContinue }
        return @()
    }

    $tfms = @()

    try {
        $zip = [System.IO.Compression.ZipFile]::OpenRead($tempFile)

        foreach ($entry in $zip.Entries) {
            $full = $entry.FullName.Replace('\','/')

            if ($full.StartsWith("lib/") -or $full.StartsWith("ref/")) {
                $parts = $full.Split('/')
                if ($parts.Length -ge 3 -and $parts[-1].ToLower().EndsWith(".dll")) {
                    $rawTfm = $parts[1]
                    $norm = Normalize-PackageTfm $rawTfm
                    if ($norm) {
                        $tfms += $norm
                    }
                }
            }
        }

        $zip.Dispose()

        $tfms = $tfms | Sort-Object -Unique

        if (-not $tfms -or $tfms.Count -eq 0) {
            return @("")  # „Any“
        }

        return $tfms
    }
    finally {
        if (Test-Path $tempFile) {
            Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
        }
    }
}

function Test-PackageVersionDeprecated {
    param(
        [string]$PackageId,
        [string]$Version
    )

    $idLower = $PackageId.ToLowerInvariant()
    $vLower  = $Version.ToLowerInvariant()

    $url = "https://api.nuget.org/v3/registration5-gz-semver2/$idLower/$vLower.json"

    try {
        $meta = Invoke-RestMethod -Uri $url -Method Get -UseBasicParsing -ErrorAction Stop
    }
    catch {
        # Wenn die Metadaten nicht geladen werden können -> wir tun so, als wäre die Version NICHT deprecated
        return $false
    }

    # Sehr einfacher Zugriff: erste Leaf-Item-Struktur prüfen
    if ($meta.items -and $meta.items[0].items -and $meta.items[0].items[0].deprecation) {
        return $true
    }

    return $false
}

function Get-LatestCompatibleVersion {
    param(
        [string]$PackageId,
        [string[]]$ProjectTfms
    )

    Write-Host "  -> Fetching versions for $PackageId"

    $allVersions = Get-PackageVersions -PackageId $PackageId
    if (-not $allVersions -or $allVersions.Count -eq 0) {
        return $null
    }

    $stable = $allVersions | Where-Object { $_ -notmatch '-' }
    if (-not $stable -or $stable.Count -eq 0) {
        return $null
    }

    $sorted = $stable | Sort-Object { [version]$_ } -Descending

    foreach ($v in $sorted) {

        # deprecated Versionen ignorieren
        if (Test-PackageVersionDeprecated -PackageId $PackageId -Version $v) {
            Write-Host " deprecated -> skipping"
            continue
        }

        Write-Host "    Checking $PackageId $v ..." -NoNewline

        $pkgTfms = Get-PackageFrameworksFromNupkg -PackageId $PackageId -Version $v
        if (-not $pkgTfms) {
            Write-Host " no tfms -> treated as compatible"
            return $v
        }

        $allProjectCompatible = $true

        foreach ($ptfm in $ProjectTfms) {
            $oneTfmCompatible = $false
            foreach ($pkgTfm in $pkgTfms) {
                if (Test-FrameworkCompatible -ProjectTfm $ptfm -PackageTfm $pkgTfm) {
                    $oneTfmCompatible = $true
                    break
                }
            }
            if (-not $oneTfmCompatible) {
                $allProjectCompatible = $false
                break
            }
        }

        if ($allProjectCompatible) {
            Write-Host " compatible"
            return $v
        }
        else {
            Write-Host " not compatible"
        }
    }

    return $null
}

# ==============================================
# Directory.Packages.props laden/verwalten
# ==============================================
$script:DirectoryPropsPath = $null
$script:DirectoryPropsXml  = $null
$script:DirectoryPackageMap = @{}

$propsFile = Get-ChildItem -Path $SolutionRoot -Recurse -Filter "Directory.Packages.props" -File | Select-Object -First 1

if ($propsFile) {
    $script:DirectoryPropsPath = $propsFile.FullName
    $script:DirectoryPropsXml = New-Object System.Xml.XmlDocument
    $script:DirectoryPropsXml.PreserveWhitespace = $true
    $script:DirectoryPropsXml.Load($script:DirectoryPropsPath)

    $script:DirectoryPackageMap = @{}

    if ($script:DirectoryPropsXml.Project -and $script:DirectoryPropsXml.Project.ItemGroup) {
        foreach ($ig in $script:DirectoryPropsXml.Project.ItemGroup) {
            if ($ig.PackageVersion) {
                foreach ($pv in $ig.PackageVersion) {
                    $id = $pv.Include
                    if ($id) {
                        $script:DirectoryPackageMap[$id] = $pv
                    }
                }
            }
        }
    }

    Write-Host "Using Directory.Packages.props at: $($script:DirectoryPropsPath)"
}
else {
    Write-Host "No Directory.Packages.props found (versions will be per csproj)."
}

function Set-PropsPackageVersion {
    param(
        [string]$PackageId,
        [string]$Version
    )

    if (-not $script:DirectoryPropsXml) { return }

    $ns = $script:DirectoryPropsXml.Project.NamespaceURI

    if (-not $script:DirectoryPackageMap.ContainsKey($PackageId)) {
        # neuen Eintrag anlegen
        $ig = $script:DirectoryPropsXml.Project.ItemGroup | Where-Object { $_.PackageVersion } | Select-Object -First 1
        if (-not $ig) {
            $ig = $script:DirectoryPropsXml.CreateElement("ItemGroup", $ns)
            $script:DirectoryPropsXml.Project.AppendChild($ig) | Out-Null
        }

        $pv = $script:DirectoryPropsXml.CreateElement("PackageVersion", $ns)

        $incAttr = $script:DirectoryPropsXml.CreateAttribute("Include")
        $incAttr.Value = $PackageId
        $pv.Attributes.Append($incAttr) | Out-Null

        $verAttr = $script:DirectoryPropsXml.CreateAttribute("Version")
        $verAttr.Value = $Version
        $pv.Attributes.Append($verAttr) | Out-Null

        $ig.AppendChild($pv) | Out-Null

        $script:DirectoryPackageMap[$PackageId] = $pv
    }
    else {
        $pv = $script:DirectoryPackageMap[$PackageId]
        if ($pv.Version) {
            $pv.Version = $Version
        }
        else {
            $attr = $pv.Attributes | Where-Object { $_.Name -eq "Version" }
            if ($attr) {
                $attr.Value = $Version
            }
            else {
                $verAttr = $script:DirectoryPropsXml.CreateAttribute("Version")
                $verAttr.Value = $Version
                $pv.Attributes.Append($verAttr) | Out-Null
            }
        }
    }
}

# ==============================================
# Projekt bearbeiten
# ==============================================
function Update-ProjectPackages {
    param(
        [string]$CsprojPath
    )

    Write-Host ""
    Write-Host "=== Project: $CsprojPath ==="

    $tfms = Get-ProjectFrameworks -CsprojPath $CsprojPath
    if (-not $tfms -or $tfms.Count -eq 0) {
        Write-Warning "  -> No TargetFramework(s) found, skipping."
        return
    }

    Write-Host "  TargetFramework(s): $($tfms -join ', ')"

    $xml = New-Object System.Xml.XmlDocument
    $xml.PreserveWhitespace = $true
    $xml.Load($CsprojPath)
    $packageRefs = $xml.Project.ItemGroup.PackageReference
    if (-not $packageRefs) {
        Write-Host "  -> No PackageReference entries."
        return
    }

    # ---------- Parallel/Sequential Auflösung der besten Versionen ----------
    $packageIds = $packageRefs |
        ForEach-Object { $_.Include } |
        Where-Object { $_ } |
        Sort-Object -Unique

    if ($packageIds.Count -eq 0) {
        Write-Host "  -> No PackageReference entries with Include."
        return
    }

    $bestMap = @{}

    if ($PSVersionTable.PSVersion.Major -ge 7) {
        Write-Host "  Resolving best versions in PARALLEL for $($packageIds.Count) packages..."

        $packageIds | ForEach-Object -Parallel {
            param($tfmList)

            $id = $_
            $best = Get-LatestCompatibleVersion -PackageId $id -ProjectTfms $tfmList

            [PSCustomObject]@{
                Id   = $id
                Best = $best
            }

        } -ThrottleLimit 6 -ArgumentList (,$tfms) | ForEach-Object {
            $bestMap[$_.Id] = $_.Best
        }
    }
    else {
        Write-Host "  Resolving best versions SEQUENTIALLY for $($packageIds.Count) packages (PowerShell < 7)..."

        foreach ($id in $packageIds) {
            $bestMap[$id] = Get-LatestCompatibleVersion -PackageId $id -ProjectTfms $tfms
        }
    }
    # -----------------------------------------------------------------------

    foreach ($pr in $packageRefs) {
        $id = $pr.Include
        if (-not $id) { continue }

        # Herausfinden, wie dieses Paket bisher gemanagt wird:
        $managedByProps = $script:DirectoryPackageMap.ContainsKey($id)

        $currentVersion = $null

        if ($managedByProps -and $script:DirectoryPropsXml) {
            # Version kommt aus Directory.Packages.props
            $pv = $script:DirectoryPackageMap[$id]
            $currentVersion = $pv.Version
            if (-not $currentVersion) {
                $attr = $pv.Attributes | Where-Object { $_.Name -eq "Version" }
                if ($attr) { $currentVersion = $attr.Value }
            }
        }
        else {
            # Version aus csproj
            $currentVersion = $pr.Version
            if (-not $currentVersion) {
                $attr = $pr.Attributes | Where-Object { $_.Name -eq "Version" }
                if ($attr) { $currentVersion = $attr.Value }
            }
        }

        Write-Host ""
        Write-Host "  Package: $id (current: $currentVersion) (managedByProps: $managedByProps)"
		
		# Wenn Version ein Range ist (z.B. [2.3.10]) -> nicht anfassen
        if ($currentVersion -and $currentVersion -match '\[') {
            Write-Host "    -> Version range detected ($currentVersion), skipping update."
            continue
        }
		
        $best = $bestMap[$id]

        if (-not $best) {
            Write-Warning "    -> No compatible version found, keeping current."
            continue
        }

        if ($currentVersion -and ([version]$best) -le ([version]$currentVersion)) {
            Write-Host "    -> Already at latest compatible (or newer): $currentVersion"
            continue
        }

        Write-Host "    -> Updating to $best"

        if ($managedByProps -and $script:DirectoryPropsXml) {
            # zentrale Version pflegen
            Set-PropsPackageVersion -PackageId $id -Version $best

            # falls im csproj dennoch eine Version eingetragen ist, entfernen wir sie
            if ($pr.Version) {
                $pr.Version = $null
                $null = $pr.RemoveChild($pr.Version)
            }
            $attr = $pr.Attributes | Where-Object { $_.Name -eq "Version" }
            if ($attr) {
                $pr.Attributes.Remove($attr) | Out-Null
            }
        }
        else {
            # NICHT durch props verwaltet
            if (-not $currentVersion -and $script:DirectoryPropsXml) {
                # kein currentVersion im csproj, aber Props-Datei vorhanden -> neues zentrales Package anlegen
                Write-Host "    -> No local version, props exists -> adding to Directory.Packages.props"
                Set-PropsPackageVersion -PackageId $id -Version $best

                # sicherstellen, dass im csproj keine Version steht
                if ($pr.Version) {
                    $pr.Version = $null
                    $null = $pr.RemoveChild($pr.Version)
                }
                $attr = $pr.Attributes | Where-Object { $_.Name -eq "Version" }
                if ($attr) {
                    $pr.Attributes.Remove($attr) | Out-Null
                }
            }
            else {
                # Version im csproj pflegen
                if ($pr.Version) {
                    $pr.Version = $best
                }
                else {
                    $attr = $pr.Attributes | Where-Object { $_.Name -eq "Version" }
                    if ($attr) {
                        $attr.Value = $best
                    }
                    else {
                        $vNode = $xml.CreateElement("Version")
                        $vNode.InnerText = $best
                        $pr.AppendChild($vNode) | Out-Null
                    }
                }
            }
        }
    }

    $settings = New-Object System.Xml.XmlWriterSettings
    $settings.Encoding = New-Object System.Text.UTF8Encoding($false)  # ohne BOM
    $settings.OmitXmlDeclaration = $true
    $settings.Indent = $false

    $writer = [System.Xml.XmlWriter]::Create($CsprojPath, $settings)
    $xml.Save($writer)
    $writer.Close()

    Write-Host "  -> Saved $CsprojPath"
}

# ==============================================
# Main
# ==============================================
$SolutionRoot = (Resolve-Path $SolutionRoot).Path
Write-Host "Scanning solution root: $SolutionRoot"

$projects = Get-ChildItem -Path $SolutionRoot -Recurse -Filter *.csproj

foreach ($proj in $projects) {
    # Optional: Backup
    # Copy-Item $proj.FullName "$($proj.FullName).bak" -Force
    Update-ProjectPackages -CsprojPath $proj.FullName
}

# Directory.Packages.props ggf. am Ende speichern
if ($script:DirectoryPropsXml -and $script:DirectoryPropsPath) {
    $settings = New-Object System.Xml.XmlWriterSettings
    $settings.Encoding = New-Object System.Text.UTF8Encoding($false)
    $settings.OmitXmlDeclaration = $true
    $settings.Indent = $false

    $writer = [System.Xml.XmlWriter]::Create($script:DirectoryPropsPath, $settings)
    $script:DirectoryPropsXml.Save($writer)
    $writer.Close()

    Write-Host ""
    Write-Host "Updated Directory.Packages.props at: $($script:DirectoryPropsPath)"
}

Write-Host ""
Write-Host "Running dotnet restore..."
dotnet restore $SolutionRoot

Write-Host ""
Write-Host "Running dotnet build..."
dotnet build $SolutionRoot

Write-Host ""
Write-Host "Done."
