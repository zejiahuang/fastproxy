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
  - 构建入口脚本：`/workspace/SteamTools-Android/build-android.sh [modern|legacy|all]`，禁止用 `-p:DefineConstants` 覆盖，会丢掉 csproj 内的 `MVVM_VM` 等关键常量。
  - 构建产物路径：`src/ST.Client.Android.App.Modern/bin/Release/net10.0-android/net.steampp.app-Signed.apk`。
  - `AndroidLinkMode` 必须保持 `None`。设为 `SdkOnly` 会裁掉仅通过反射到达的类型（NLog targets、ReactiveUI、DynamicData、DI），导致 `IApplication.InitLogDir` 之前就崩溃，主页不出现且不写日志。
  - 体积控制靠 `AndroidSupportedAbis=arm64-v8a` + `RuntimeIdentifiers=android-arm64` + `AndroidEnableAssemblyCompression`，可把 APK 从 113MB 降到约 38MB。
  - Release 构建耗时约 7 分钟，峰值内存约 2.5GB，环境总内存 8GB、2 核，须用 background terminal 执行并设置资源限制。

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
