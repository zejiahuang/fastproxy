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

[Project Knowledge Summary]
- Date: 2026-08-28
- Context: Discovered by Agent while fixing "加速页图标全都没显示" bug
- Category: Troubleshooting & Debugging
- Instructions:
  - 加速页图标（分组 /icon/<group>、条目 /icon/entry/<id>）来自第三方服务 https://abhuang.dpdns.org，返回的是 image/svg+xml（Simple Icons 单 path 格式），Picasso 无法解码 SVG，导致图标全部不显示。
  - 解决方式：在 ImageLoader.cs 实现了自包含的 Simple Icons SVG->Bitmap 光栅化器（SvgPathUtil，解析 viewBox/fill/path d，支持 M/L/H/V/C/S/Q/T/A/Z 及弧线），新增 SetImageSourceSvg(ImageView, url, resId) 异步抓取 SVG 文本后渲染，分组/条目适配器改用该方法。
  - 新增外观/代理设置项的通用排查链路：先核对设置项被下游真正读取（SpeedTestTimeoutMs/TTL/WriteBack/EnableHttpProxyToHttps/ProxyMasterDns/ProgramStartupRunProxy 均被 LatencyTestServiceImpl / TitaniumReverseProxyServiceImpl / ProxyService 读取），再核对 SerializableProperty autoSave 是否落盘（默认 autoSave=true，曾发现 ProxyMasterDns 误设 autoSave:false 导致改了不持久化）。

[Project Knowledge Summary]
- Date: 2026-08-28
- Context: Discovered by Agent while cleaning up orphan resource files
- Category: Build Methods
- Instructions:
  - Design 模块（ST.Client.Mobile.Droid.Design/ui）的 res 资源链接架构：ST.Tools.AndroidResourceLink/Program.cs 会重写 ST.Client.Android.csproj 中 `<!--ST.Tools.AndroidResourceLink-->` 标记段，把 res 下除 `__dont_link` 文件名和 ignoreArray 外的所有文件以 AndroidBoundLayout/AndroidResource 条目链入；Modern csproj（ST.Client.Android.App.Modern）的资源条目是手工维护的，删资源文件后必须同步从三个 csproj（Modern/ST.Client.Android/ST.Client.Mobile.Droid）移除对应条目，否则 MSBuild 报找不到文件。
  - 清理孤儿资源的方法：以布局文件名做全源宽松 grep（含 .cs/.xml/.csproj/.json，排除 obj/build 与自身文件），零命中即孤儿；注意生成 Binding.*.g.cs 只说明布局被链接，不代表被代码使用。
