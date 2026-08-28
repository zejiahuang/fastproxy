using System.Collections.Generic;

namespace System.Application.Settings;

public sealed partial class UISettings : SettingsHost2<UISettings>
{
    /// <summary>
    /// 主题
    /// </summary>
    public static SerializableProperty<short> Theme { get; }
        = GetProperty(defaultValue: (short)0);

    /// <summary>
    /// 语言
    /// </summary>
    public static SerializableProperty<string> Language { get; }
        = GetProperty(defaultValue: string.Empty);

    /// <summary>
    /// 不再提示的消息框数组
    /// </summary>
    public static SerializableProperty<HashSet<MessageBox.DontPromptType>?> DoNotShowMessageBoxs { get; }
        = GetProperty<HashSet<MessageBox.DontPromptType>?>(defaultValue: null, autoSave: false);

    /// <summary>
    /// 是否显示广告
    /// </summary>
    public static SerializableProperty<bool> IsShowAdvertise { get; }
        = GetProperty(defaultValue: true);

    #region InstallerX 外观设置项

    /// <summary>
    /// 启用动态取色（Android 12+ 壁纸取色，Material You）
    /// </summary>
    public static SerializableProperty<bool> UseDynamicColor { get; }
        = GetProperty(defaultValue: false);

    /// <summary>
    /// 主题色种子（18 色预设索引，ARGB 色值），用于基于种子色的动态主题
    /// </summary>
    public static SerializableProperty<int> ThemeColor { get; }
        = GetProperty(defaultValue: unchecked((int)0xFF6750A4));

    /// <summary>
    /// 启用悬浮胶囊底栏
    /// </summary>
    public static SerializableProperty<bool> UseFloatingBar { get; }
        = GetProperty(defaultValue: true);

    /// <summary>
    /// 启用模糊效果
    /// </summary>
    public static SerializableProperty<bool> UseBlur { get; }
        = GetProperty(defaultValue: false);

    #endregion
}

//static void EnableDesktopBackground_ValueChanged(object? sender, ValueChangedEventArgs<bool> e)
//{
//    if (e.NewValue)
//    {
//        IApplication.Instance.SetDesktopBackgroundWindow();
//    }
//    else
//    {
//        INativeWindowApiService.Instance.ResetWallerpaper();
//    }
//}

//static void Theme_ValueChanged(object sender, ValueChangedEventArgs<short> e)
//{
//    // 当前 Avalonia App 主题切换存在问题
//    //if (OperatingSystem2.Application.UseAvalonia()) return;
//    if (e.NewValue != e.OldValue)
//    {
//        var value = (AppTheme)e.NewValue;
//        if (value.IsDefined())
//        {
//            IApplication.Instance.Theme = value;
//        }
//    }
//}