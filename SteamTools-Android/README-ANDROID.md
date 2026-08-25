# SteamTools Android - 独立开发环境

从 SteamTools v2.8.3 提取的 Android 端独立代码，用于 Android 开发。

## 项目结构

### Android 入口项目
- `src/ST.Client.Android.App.Modern/` — 现代 .NET 6 Android 应用入口 (net6.0-android)
- `src/ST.Client.Mobile.Droid.App/` — 遗留 Xamarin.Android 应用入口 (MonoAndroid v12.0)

### Android 核心库
- `src/ST.Client.Android/` — Android 客户端核心代码（被 Modern 项目以源码方式引用）
- `src/ST.Client.Android.ResSecrets/` — Android 资源密钥
- `src/ST.Client.Android.SecretKeys/` — Android 密钥定义
- `src/ST.Client.Android.V2Ray/` — V2Ray 绑定库（条件编译：V2RAY）
- `src/ST.Client.Android.Shadowsocks/` — Shadowsocks 绑定库（条件编译：SHADOWSOCKS）
- `src/ST.Client.Mobile/` — 移动端通用代码
- `src/ST.Client.Mobile.Droid/` — 移动端 Droid 共享库
- `src/ST.Client.Mobile.Droid.Design/` — Android UI 设计资源（布局、图标、主题）
- `src/ST.Client.Mobile.Droid.Resources/` — Android 资源文件
- `src/Common.ClientLib.Droid/` — Android 通用客户端库

### 共享依赖库
- `src/Common.CoreLib/` — 核心基础库
- `src/ST/` — 共享 ST 库
- `src/ST.Client/` — 客户端共享逻辑
- `src/ST.Client.ResSecrets/` — 客户端密钥
- `src/ST.Client.ReverseProxy/` — 反向代理抽象
- `src/ST.Client.ReverseProxy.Titanium/` — Titanium 反向代理实现
- `src/ST.Client.AppCenter/` — App Center 集成
- `src/ST.Services.CloudService/` — 云服务
- `src/ST.Services.CloudService.Models/` — 云服务模型
- `src/ST.Services.CloudService.ViewModels/` — 云服务 ViewModel
- `src/Repositories.sqlite-net-pcl/` — SQLite 数据仓库
- `src/Common.Essentials/` — 通用 Essentials
- `src/Common.Essentials.Xamarin/` — Xamarin Essentials
- `src/Common.ClientLib/` — 通用客户端库
- `src/Common.PinyinLib/` — 拼音库
- `src/Common.PinyinLib.TinyPinyin/` — TinyPinyin 实现
- `src/Common.AreaLib/` — 地区库
- `src/Microsoft.Net.Http.Headers/` — HTTP 头库

## 构建要求

- .NET SDK 6.0
- Android SDK（API 31+）
- Xamarin.Android 工作负载（遗留项目）
- .NET Android 工作负载（现代项目）

## 构建命令

```bash
# 构建现代 Android 项目
dotnet build src/ST.Client.Android.App.Modern/ST.Client.Android.App.Modern.csproj

# 构建遗留 Android 项目
dotnet build src/ST.Client.Mobile.Droid.App/ST.Client.Android.App.csproj
```

## 条件编译开关

- `V2RAY` — 启用 V2Ray 支持
- `SHADOWSOCKS` — 启用 Shadowsocks 支持
- `__XAMARIN_FORMS__` — 启用 Xamarin.Forms 页面

## 原始项目

https://github.com/BeyondDimension/SteamTools/tree/2.8.3
