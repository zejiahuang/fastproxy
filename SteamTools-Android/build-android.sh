#!/bin/bash
set -e

# SteamTools Android 构建脚本
# 用法: ./build-android.sh [modern|legacy|all] [--v2ray|--xamarin-forms] [-- $@]
#
# 条件编译说明:
#   - SHADOWSOCKS: 默认启用, 通过 csproj DefineConstants 管理
#   - V2RAY: 与 SHADOWSOCKS 互斥, 不能同时启用 (partial class VpnService 基类冲突)
#   - __XAMARIN_FORMS__: 可选, 与 Modern 默认配置不兼容 (排除 Renderers)
#   - EXCLUDE_ASF: 默认启用, 移除 ArchiSteamFarm 集成
#
# 注意: 不要使用 -p:DefineConstants 覆盖, 会丢失 csproj 内部的 MVVM_VM 等关键常量

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

MODE="${1:-modern}"
shift 2>/dev/null || true

# 解析可选标志
EXTRA_FLAGS=()
while [[ $# -gt 0 ]]; do
    case "$1" in
        --v2ray)
            EXTRA_FLAGS+=("-p:UseV2Ray=true")
            shift
            ;;
        --xamarin-forms)
            EXTRA_FLAGS+=("-p:UseXamarinForms=true")
            shift
            ;;
        --)
            shift
            EXTRA_FLAGS+=("$@")
            break
            ;;
        *)
            EXTRA_FLAGS+=("$1")
            shift
            ;;
    esac
done

build_modern() {
    echo "=== 构建现代 Android 项目 (net10.0-android) ==="
    dotnet build src/ST.Client.Android.App.Modern/ST.Client.Android.App.Modern.csproj \
        "${EXTRA_FLAGS[@]}" "$@"
}

build_legacy() {
    echo "=== 构建遗留 Xamarin.Android 项目 (MonoAndroid v12.0) ==="
    dotnet build src/ST.Client.Mobile.Droid.App/ST.Client.Android.App.csproj \
        "${EXTRA_FLAGS[@]}" "$@"
}

case "$MODE" in
    modern)
        build_modern
        ;;
    legacy)
        build_legacy
        ;;
    all)
        build_modern
        build_legacy
        ;;
    *)
        echo "用法: $0 [modern|legacy|all] [--v2ray] [--xamarin-forms]"
        exit 1
        ;;
esac
