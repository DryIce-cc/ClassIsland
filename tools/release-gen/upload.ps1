$ErrorActionPreference = "Stop"

Import-Module ./tools/release-gen/alist-utils.psm1

# Generate metadata & upload artifacts
Write-Output "Generating metadata & uploading artifacts"
#Copy-Item ./out_signed/* -Destination ./out/ -Recurse -Force

$version = $(git describe --abbrev=0 --tags)
$gitCommitId = $(git rev-parse HEAD)
$tagInfo = @{
    PrimaryVersion = $env:primaryVersion
    Channels = ConvertFrom-Json $env:channels
}
$versionInfo = @{
    Version = $version
    Title = $version
    DownloadInfos = @{}
    ChangeLogs = $(Get-Content ./doc/ChangeLogs/$($tagInfo.PrimaryVersion)/$version/App.md -Raw)
    Channels = @()
}
$versionInfo.Channels = $tagInfo.Channels

Write-Host "Version: $version"

if ($(Test-Path ./out) -eq $false) {
    mkdir out
}

$artifacts = Get-ChildItem -Path ./out_artifacts -Directory

foreach ($artifact in $artifacts) {
    if ($artifact.Name.Contains("out_app_") -ne $true) {
        continue
    }
    Copy-Item ./out_artifacts/$($artifact.Name)/* -Destination ./out/ -Recurse -Force
}

foreach ($artifact in $(Get-ChildItem ./out)) {
    Move-Item $artifact.FullName -Destination $artifact.FullName.Replace("out_app_", "ClassIsland_app_") -Force
}
# Move-Item ./out/out_app_windows_x64_full_wap.msix -Destination ./out/out_app_windows_x64_full_wap.appx
$artifacts = $(Get-ChildItem ./out)
$uploadJobs = @()
$hashSummary = "

***

> [!important]
> 下载时请注意核对文件 SHA256 是否正确。

| 文件名 | SHA256 |
| --- | --- |
"
$legaceyMD5Hashes = [ordered]@{}

foreach ($artifact in $artifacts){
    if ($artifact.Name.StartsWith("ClassIsland_app") -ne $true) {
        continue
    }
    $artifactId = [System.IO.Path]::GetFileNameWithoutExtension($artifact.Name).Split("_")[2..5] -join "_"
    Write-Output "Generating metadata for $artifactId"
    $deployMethod = [System.IO.Path]::GetFileNameWithoutExtension($artifact.Name).Split("_")[5]
    $deployMethodId = 0  # none
    if ($deployMethod -eq "singleFile") {
        $deployMethodId = 1
    }
    if ($deployMethod -eq "folder") {
        $deployMethodId = 2
    }
    if ($deployMethod -eq "wap") {
        $deployMethodId = 3
    }
    Write-Output "artifact ${artifactId}: deployMethod=${deployMethod}; deployMethodId=${deployMethodId}"
    $downloadInfo = @{
        "DeployMethod" = $deployMethodId
        "ArchiveDownloadUrls" = @{
            "main" = "https://get.classisland.tech/d/ClassIsland-Ningbo-S3/classisland/disturb/${version}/$($artifact.Name)"
            "github-origin" = "https://github.com/ClassIsland/ClassIsland/releases/download/${version}/$($artifact.Name)"
        }
        "ArchiveSHA256"= $(Get-FileHash $artifact -Algorithm SHA256).Hash
    }
    $taskInput = @{
        artifactId = $artifactId
        artifact = $artifact
        version = $version
    }
    $versionInfo.DownloadInfos[$artifactId] = $downloadInfo
    $hashSummary +=  "| $($artifact.Name) | ``$($downloadInfo.ArchiveSHA256)`` |`n"
    $legaceyMD5Hashes.Add($artifact.Name, (Get-FileHash $artifact -Algorithm MD5).Hash)
#    $uploadJobs += Start-ThreadJob -Name $artifact.FullName -StreamingHost $Host -ScriptBlock {
#        $inputs = $using:taskInput
#        Import-Module ./tools/release-gen/alist-utils.psm1
#        Write-Host "Uploading artifact $($inputs.artifactId)"
#        UploadFile $inputs.artifact.FullName "ClassIsland-Ningbo-S3/classisland/disturb/$($inputs.version)/"
#        Write-Host "Uploading artifact $($inputs.artifactId) COMPLETED"
#    }
}

#Wait-Job -Job $uploadJobs

#foreach ($job in $uploadJobs) {
#    Receive-Job -Job $job
#}

Copy-Item ./doc/ChangeLogs/$($tagInfo.PrimaryVersion)/$version/App.md -Destination ./out/ChangeLogs.md -Force

$hashSummary | Add-Content ./out/ChangeLogs.md
"<!-- CLASSISLAND_PKG_MD5 $(ConvertTo-Json $legaceyMD5Hashes -Compress) -->" | Add-Content ./out/ChangeLogs.md 

ConvertTo-Json $versionInfo -Depth 99 | Out-File ./out/index.json

# Upload metadata
#Write-Output "Uploading metadata"
#git config --global user.name 'classisland-bot'
#git config --global user.email 'elf-elysia.noreply@classisland.tech'
#git clone git@github.com:ClassIsland/metadata.git
#cd metadata
#
#git checkout -b metadata/disturb/$version
#if ($(Test-Path ./metadata/disturb/$version) -eq $false) {
#    mkdir ./metadata/disturb/$version
#}
#Copy-Item ../out/index.json -Destination ./metadata/disturb/$version/index.json -Force
#$globalIndex = ConvertFrom-Json (Get-Content ./metadata/disturb/index.json -Raw)
#$globalIndex.Versions = [System.Collections.ArrayList]@($globalIndex.Versions)
#$globalIndex.Versions.Add(@{
#    Version = $version
#    Title = $version
#    Channels = $tagInfo.Channels
#    VersionInfoUrl = "https://get.classisland.tech/d/ClassIsland-Ningbo-S3/classisland/disturb/${version}/index.json"
#})
#ConvertTo-Json $globalIndex -Depth 99 | Out-File ./metadata/disturb/index.json
#git add .
#git commit -m "metadata(disturb): release $version at https://github.com/ClassIsland/ClassIsland/commit/$gitCommitId"
#git push origin metadata/disturb/$version
#gh pr create -R ClassIsland/metadata -t "Add metadata for $version" -b "Add metadata for $version at https://github.com/ClassIsland/ClassIsland/commit/$gitCommitId" -B main -a HelloWRC
