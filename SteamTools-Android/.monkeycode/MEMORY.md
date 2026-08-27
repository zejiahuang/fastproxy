# User Instruction Memory

This file records user instructions, preferences, and teachings for reference in future interactions.

## Format

### User Instruction Entry
User instruction entries should follow this format:

[User Instruction Summary]
- Date: [YYYY-MM-DD]
- Context: [Mentioned scenario or time]
- Instructions:
  - [Content of user teaching or instruction, described line by line]

### Project Knowledge Entry
Entries discovered by the Agent during task execution should follow this format:

[Project Knowledge Summary]
- Date: [YYYY-MM-DD]
- Context: Discovered by Agent while performing [specific task description]
- Category: [Operations & Deployment|Build Methods|Testing Methods|Troubleshooting & Debugging|Workflow & Collaboration|Environment Configuration]
- Instructions:
  - [Specific knowledge points, described line by line]

## Deduplication Strategy
- Before adding a new entry, check for similar or identical instructions.
- If a duplicate is found, skip the new entry or merge it with the existing one.
- When merging, update the context or date information.
- This helps avoid redundant entries and keeps the memory file tidy.

## Entries

Android 构建与打包
- Date: 2026-08-27
- Context: Discovered by Agent while 缩减 APK 体积并修复启动崩溃
- Category: Build Methods
- Instructions:
  - 构建入口脚本：`/workspace/SteamTools-Android/build-android.sh [modern|legacy|all] [-- 额外的 dotnet 参数]`，禁止用 `-p:DefineConstants` 覆盖，会丢掉 csproj 内的 `MVVM_VM` 等关键常量。
  - 构建前必须导出环境变量，devbox 默认 shell 中三者都不在环境里：`PATH=/usr/share/dotnet:$PATH`、`ANDROID_HOME=/opt/android-sdk`、`ANDROID_SDK_ROOT=/opt/android-sdk`。缺 dotnet 会报 `dotnet: not found`，缺 SDK 路径会报 `error XA5300: The Android SDK directory could not be found`。
  - 构建产物路径：`src/ST.Client.Android.App.Modern/bin/Release/net10.0-android/net.steampp.app-Signed.apk`。
  - `AndroidLinkMode` 必须保持 `None`。设为 `SdkOnly` 会裁掉仅通过反射到达的类型（NLog targets、ReactiveUI、DynamicData、DI），导致 `IApplication.InitLogDir` 之前就崩溃，主页不出现且不写日志。
  - 体积控制靠 `AndroidSupportedAbis=arm64-v8a` + `RuntimeIdentifiers=android-arm64` + `AndroidEnableAssemblyCompression`，可把 APK 从 113MB 降到约 38MB。
  - Release 构建耗时约 7 分钟，峰值内存约 2.5GB，环境总内存 8GB、2 核，须用 background terminal 执行并设置资源限制。增量构建（仅改少量 C# 文件）约 2.5 分钟。
  - 构建产生约 1.9 万条 StyleCop/Nullable 警告属正常基线，判断成败只看结尾的 `N Error(s)`。

Debug 配置与 Mock 客户端
- Date: 2026-08-27
- Context: Discovered by Agent while 清理功能移除后的遗留代码
- Category: Build Methods
- Instructions:
  - `MockCloudServiceClient`（及其 `.Accelerate.cs`）被 `#if (DEBUG && !UI_DEMO) || (!DEBUG && UI_DEMO)` 包裹，Release 构建完全跳过。删除云服务接口后若忘记同步它，Release 依旧通过而 Debug 编译失败。
  - 改动 `ST.Services.CloudService` 后应单独跑一次 `dotnet build src/ST.Services.CloudService/ST.Services.CloudService.csproj -c Debug` 验证，只需约 25 秒，无需等完整 APK 构建。

APK 分发方式
- Date: 2026-08-27
- Context: Discovered by Agent while 向用户交付安装包
- Category: Operations & Deployment
- Instructions:
  - 用 background terminal 在 `/workspace` 起 `python3 -m http.server 8000`，把 APK 拷到 `/workspace` 根目录后通过预览链接下载。
  - 该 HTTP 服务长期驻留，重新交付新包时只需覆盖 `/workspace/net.steampp.app-Signed.apk`，不必重启服务。

加速功能排障日志
- Date: 2026-08-27
- Context: Discovered by Agent while 排查社区加速节点不生效
- Category: Troubleshooting & Debugging
- Instructions:
  - 加速链路诊断日志统一使用 tag `CommunityProxy`，分组勾选状态在 `AccelerateProjectGroupAdapter` 打点，启动代理时的生效域名列表在 `CommunityFixFragment` 打点。
  - 加速数据来源为云端接口，失败时回退读取 `IOPath.AppDataDirectory/LOCAL_ACCELERATE.json`（MessagePack + lz4），排查“列表为空”时先确认该缓存文件。
