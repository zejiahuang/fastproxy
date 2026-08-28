namespace System.Application.Settings;

public sealed partial class GeneralSettings : SettingsHost2<GeneralSettings>
{
    /// <summary>
    /// 自动检查更新
    /// </summary>
    public static SerializableProperty<bool> IsAutoCheckUpdate { get; }
        = GetProperty(defaultValue: true);

    /// <summary>
    /// 下载更新渠道
    /// </summary>
    public static SerializableProperty<UpdateChannelType> UpdateChannel { get; }
        = GetProperty(defaultValue: default(UpdateChannelType));

    /// <summary>
    /// 启用代理运行时日志（将 NLog 全局阈值降至 Information，使加速链路诊断日志持续写入本地文件）
    /// </summary>
    public static SerializableProperty<bool> ProxyLogEnable { get; }
        = GetProperty(defaultValue: true);
}
