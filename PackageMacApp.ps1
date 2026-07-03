<#
.SYNOPSIS
    将 Avalonia 自包含发布产物打包成 macOS .app 应用包。
.DESCRIPTION
    从指定的发布目录（如 publish/DualClock.2.0.23.osx-arm64-bundled）复制文件，
    构建 .app 包，并生成 Info.plist。
.PARAMETER PublishDir
    发布产物的目录路径（例如 publish/DualClock.2.0.23.osx-arm64-bundled）。
    如果未指定，则自动查找 publish 下最新的 osx-arm64-bundled 文件夹。
.PARAMETER AppName
    应用名称（用于 .app 文件名和内部标识）。
    默认：DualClock。
.PARAMETER BundleId
    应用唯一标识符（反向域名）。
    默认：com.yourcompany.dualclock。
.PARAMETER Version
    应用版本号。
    默认：从文件夹名中提取，或使用 2.0.0。
.PARAMETER OutputDir
    生成的 .app 存放的父目录。
    默认：当前目录下的 packages。
.PARAMETER IconFile
    可选的 .icns 图标文件路径（将复制到 Resources）。
    默认：Assets/icon.icns（如果存在）。
.EXAMPLE
    .\PackageMacApp.ps1 -PublishDir "publish/DualClock.2.0.23.osx-arm64-bundled" -BundleId "com.example.dualclock"
.EXAMPLE
    .\PackageMacApp.ps1   # 自动查找最新的发布目录
#>

param(
    [string]$PublishDir,
    [string]$AppName = "DualClock",
    [string]$BundleId = "com.leison.dualclock",
    [string]$Version,
    [string]$OutputDir = "packages",
    [string]$IconFile = "Assets/icon.icns"
)

# 如果未指定发布目录，自动查找最新的 osx-arm64-bundled
if (-not $PublishDir) {
    $publishBase = "publish"
    if (Test-Path $publishBase) {
        $folders = Get-ChildItem -Path $publishBase -Directory | Where-Object { $_.Name -like "DualClock.*.osx-arm64-bundled" } | Sort-Object Name -Descending
        if ($folders.Count -gt 0) {
            $PublishDir = $folders[0].FullName
            Write-Host "🔍 自动选择最新的发布目录: $PublishDir" -ForegroundColor Cyan
        }
        else {
            Write-Error "未找到符合 'DualClock.*.osx-arm64-bundled' 的发布目录，请使用 -PublishDir 指定。"
            exit 1
        }
    }
    else {
        Write-Error "未找到 publish 文件夹，请先运行 publish.ps1 或使用 -PublishDir 指定。"
        exit 1
    }
}

# 确保 PublishDir 存在
if (-not (Test-Path $PublishDir)) {
    Write-Error "发布目录不存在: $PublishDir"
    exit 1
}

# 如果未指定版本号，尝试从文件夹名提取
if (-not $Version) {
    $folderName = Split-Path $PublishDir -Leaf
    if ($folderName -match 'DualClock\.(\d+\.\d+\.\d+)\.osx-arm64-bundled') {
        $Version = $matches[1]
    }
    else {
        $Version = "2.0.0"
    }
}

# 创建 .app 目录
$AppDir = Join-Path $OutputDir "$AppName.app"
$MacOSDir = Join-Path $AppDir "Contents/MacOS"
$ResourcesDir = Join-Path $AppDir "Contents/Resources"

# 1. 清理并创建目录
if (Test-Path $AppDir) { Remove-Item -Recurse -Force $AppDir }
New-Item -ItemType Directory -Path $MacOSDir -Force | Out-Null
New-Item -ItemType Directory -Path $ResourcesDir -Force | Out-Null

# 2. 复制所有发布文件到 MacOS（包括所有 dll、json、可执行文件）
Copy-Item -Path "$PublishDir\*" -Destination $MacOSDir -Recurse -Force

# 3. 如果提供了图标文件（.icns），复制到 Resources
# 查找图标文件
$iconSource = $null
$iconFileName = ""

if ($IconFile -and (Test-Path $IconFile)) {
    $iconSource = $IconFile
}
else {
    $candidate1 = Join-Path $PublishDir "Assets/icon.icns"
    if (Test-Path $candidate1) { $iconSource = $candidate1 }
    else {
        $candidate2 = Join-Path $PublishDir "icon.icns"
        if (Test-Path $candidate2) { $iconSource = $candidate2 }
    }
}

if ($iconSource) {
    Copy-Item -Path $iconSource -Destination $ResourcesDir -Force
    $iconFileName = Split-Path $iconSource -Leaf
    Write-Host "✅ 已找到图标: $iconFileName" -ForegroundColor Green
}
else {
    Write-Host "⚠️ 未找到图标文件，将使用默认图标" -ForegroundColor Yellow
}

# 4. 生成 Info.plist
$plistContent = @"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>$AppName</string>
    <key>CFBundleName</key>
    <string>$AppName</string>
    <key>CFBundleDisplayName</key>
    <string>$AppName</string>
    <key>CFBundleIdentifier</key>
    <string>$BundleId</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>$Version</string>
    <key>CFBundleVersion</key>
    <string>$Version</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>CFBundleIconFile</key>
    <string>$iconFileName</string>
</dict>
</plist>
"@
$plistPath = Join-Path $AppDir "Contents/Info.plist"
$plistContent | Out-File -FilePath $plistPath -Encoding utf8

# 5. 提示
Write-Host "✅ .app 包已生成于: $((Resolve-Path $AppDir).Path)" -ForegroundColor Green
Write-Host "⚠️  如果是在 Windows 上打包，请在 macOS 上执行以下命令赋予执行权限：" -ForegroundColor Yellow
Write-Host "    chmod +x '$((Resolve-Path $AppDir).Path)/Contents/MacOS/$AppName'" -ForegroundColor Cyan
Write-Host "然后双击 .app 文件即可运行。" -ForegroundColor Green