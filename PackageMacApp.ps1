#!/bin/bash
# 用法: ./package-mac.sh [版本号]
# 例如: ./package-mac.sh 2.0.23

echo "mac app publishing..."
set -e  # 遇到错误立即停止

# 1. 配置
APP_NAME="DualClock"
BUNDLE_ID="com.leison.dualclock"
VERSION=${1:-"2.0.0"}  # 从参数或默认值获取版本号

# 定义多个可能的发布目录（按优先级）
# 优先使用 WSL 本地目录（如果有），其次使用 Windows 挂载目录
POSSIBLE_PATHS=(
    "./publish"                              # WSL 本地目录
    "/mnt/e/Develop_Vs2022/DualClock/publish" # Windows 挂载目录
)

PUBLISH_BASE=""
for path in "${POSSIBLE_PATHS[@]}"; do
    if [ -d "$path" ] && ls "$path"/DualClock.*.osx-arm64-bundled 1>/dev/null 2>&1; then
        PUBLISH_BASE="$path"
        echo "✅ 找到发布目录: $PUBLISH_BASE"
        break
    fi
done

if [ -z "$PUBLISH_BASE" ]; then
    echo "❌ 未找到任何包含 DualClock.*.osx-arm64-bundled 的发布目录"
    echo "请检查路径: ${POSSIBLE_PATHS[@]}"
    exit 1
fi

# 2. 查找最新的 osx-arm64-bundled 目录
BUNDLED_DIR=$(ls -td "$PUBLISH_BASE"/DualClock.*.osx-arm64-bundled 2>/dev/null | head -1)
if [ -z "$BUNDLED_DIR" ]; then
    echo "❌ 未找到任何 DualClock.*.osx-arm64-bundled 目录"
    exit 1
fi
echo "📁 使用发布目录: $BUNDLED_DIR"

# 如果版本号未通过参数指定，则从文件夹名提取
if [ "$VERSION" = "2.0.0" ]; then
    FOLDER_NAME=$(basename "$BUNDLED_DIR")
    if [[ $FOLDER_NAME =~ DualClock\.([0-9]+\.[0-9]+\.[0-9]+)\.osx-arm64-bundled ]]; then
        VERSION="${BASH_REMATCH[1]}"
        echo "📌 从文件夹名提取版本号: $VERSION"
    else
        echo "⚠️ 无法从文件夹名提取版本号，使用默认: $VERSION"
    fi
fi

# 3. 创建临时工作目录（在 WSL 本地，保证权限）
WORK_DIR="$HOME/develop/DualClock/tmp_mac_pack"
rm -rf "$WORK_DIR"
mkdir -p "$WORK_DIR"
echo "📂 临时工作目录: $WORK_DIR"

# 4. 复制发布产物到临时目录（从 Windows 复制过来，获得 WSL 文件系统的权限）
APP_DIR="$WORK_DIR/$APP_NAME.app"
MACOS_DIR="$APP_DIR/Contents/MacOS"
RESOURCES_DIR="$APP_DIR/Contents/Resources"

mkdir -p "$MACOS_DIR"
mkdir -p "$RESOURCES_DIR"

echo "📦 正在复制发布产物..."
cp -r "$BUNDLED_DIR"/* "$MACOS_DIR/"

# 5. 查找并复制图标（如果存在）
ICON_SOURCE=""
if [ -f "$MACOS_DIR/Assets/icon.icns" ]; then
    ICON_SOURCE="$MACOS_DIR/Assets/icon.icns"
elif [ -f "$MACOS_DIR/icon.icns" ]; then
    ICON_SOURCE="$MACOS_DIR/icon.icns"
fi

ICON_FILE_NAME=""
if [ -n "$ICON_SOURCE" ]; then
    cp "$ICON_SOURCE" "$RESOURCES_DIR/"
    ICON_FILE_NAME=$(basename "$ICON_SOURCE")
    echo "✅ 已找到图标: $ICON_FILE_NAME"
else
    echo "⚠️ 未找到图标文件"
fi

# 6. 生成 Info.plist
cat > "$APP_DIR/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>$APP_NAME</string>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>
    <string>$BUNDLE_ID</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>$VERSION</string>
    <key>CFBundleVersion</key>
    <string>$VERSION</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>CFBundleIconFile</key>
    <string>$ICON_FILE_NAME</string>
</dict>
</plist>
EOF

# 7. 赋予执行权限（在 WSL 本地，有效）
chmod +x "$MACOS_DIR/$APP_NAME"

# 8. 打包为 .tar.gz（保留权限）
# 输出到 Windows 发布目录
OUTPUT_DIR="/mnt/e/Develop_Vs2022/DualClock/publish/packages"
mkdir -p "$OUTPUT_DIR"
TAR_NAME="$OUTPUT_DIR/$APP_NAME.$VERSION.macos-app.tar.gz"
tar -czf "$TAR_NAME" -C "$WORK_DIR" "$APP_NAME.app"

echo "✅ .app 已生成并打包: $TAR_NAME"
echo "📦 最终产物: $TAR_NAME"

# 9. 清理临时目录（可选）
#rm -rf "$WORK_DIR"
#echo "🧹 临时目录已清理"
