#
#   请在 PowerShell 7 中运行此脚本
#   如没有 PowerShell 7，请先安装 PowerShell 7
#   或者修改脚本，使得它符合你当前的 PowerShell 版本
#


# 1. 读取和写入 Patch 号 (防止 PS7 文件锁)
$patchFile = Resolve-Path "version_patch.txt" -ErrorAction SilentlyContinue
$patch = 0
if ($patchFile -and (Test-Path $patchFile)) {
    $patch = [int]([System.IO.File]::ReadAllText($patchFile).Trim())
}
$patch++
[System.IO.File]::WriteAllText((New-Item -Path "version_patch.txt" -Force), $patch.ToString())

# 2. 版本号定义
$major = 2
$minor = 0
$version = "$major.$minor.$patch"
$assemblyVersion = "$major.$minor.$patch.0"


# 3. 清理旧包
       if (Test-Path  "publish") { Remove-Item -Recurse -Force  "publish" }


# 4. 按照运行时发布
    $runtimes = @("win-x64", "linux-x64", "linux-arm64")
    #$runtimes = @( "linux-x64", "linux-arm64")

foreach ($rid in $runtimes) {
        Write-Host "`n=== 正在发布 $rid ===" -ForegroundColor Cyan
    # 4.1 定义输出文件夹
        $baseFolder    = "DualClock.$version.$rid"
        $bundledFolder = "$baseFolder-bundled"
        $baseOutput    = "publish\$baseFolder"
        $bundledOutput = "publish\$bundledFolder"
        $baseZip       = "publish\$baseFolder.zip"
        $bundledZip    = "publish\$bundledFolder.zip"

    # 4.2 准备基础参数
        $baseArgs = @(
            "publish", ".\DualClock\DualClock.csproj", 
            "-c", "Release", 
            "-r", $rid,
            "-p:Version=$version",
            "-p:AssemblyVersion=$assemblyVersion",
            "-p:FileVersion=$assemblyVersion",
            "-p:InformationalVersion=$version"
        )

        # 4.2.1 纯净 Base 版
        $baseLaunchArgs = $baseArgs + @(
            "--self-contained", "false", 
            "-p:SelfContained=false", 
            "-p:PublishSelfContained=false", 
            "-p:PublishSingleFile=true",
            "-p:PublishReadyToRun=false", 
            "--output", $baseOutput
        )
        # 4.2.2 Bundled 版
        $bundledLaunchArgs = $baseArgs + @(
            "--self-contained", "true", 
            "-p:SelfContained=true", 
            "-p:PublishSelfContained=true", 
            "-p:PublishSingleFile=true",                        
            "-p:IncludeNativeLibrariesForSelfContained=true",  
            "-p:PublishReadyToRun=false",
            "--output", $bundledOutput
        )
    # 4.3 发布
        Write-Host "开始发布 Base 版..." -ForegroundColor Cyan
        dotnet @baseLaunchArgs

        Write-Host "开始发布 Bundled 版..." -ForegroundColor Cyan
        dotnet @bundledLaunchArgs

    # 4.4 打包 ZIP
        Compress-Archive -Path "$baseOutput\*" -DestinationPath $baseZip -Force
        Compress-Archive -Path "$bundledOutput\*" -DestinationPath $bundledZip -Force

    # 4.5 GitHub Release
      # Write-Host "发布到 github ..." -ForegroundColor Cyan
      # $tag = "v$version"
      # git tag -f -m "Release DualClock $version" $tag
      # git push origin $tag --force
      #
      # gh release create $tag `
      #      $baseZip `
      #      $bundledZip `
      #      --title "DualClock $version" `
      #      --notes "Release of DualClock $version with both self-contained and dependent builds."
}