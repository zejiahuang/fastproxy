using Android.Views;
using ReactiveUI;
using System.Application.Models;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using static System.Application.UI.Resx.AppResources;
using DynamicData;
using System.Application.UI.Activities;
using System.Application.Services;
using Android.Runtime;
using Android.App;
using Android.Content.PM;
using M = System.Application.UI.ViewModels.SettingsPageViewModel;
using V = Binding.fragment_settings;
using C = System.Application.UI.Controllers.SettingsController;

namespace System.Application.UI.Controllers
{
    internal sealed class SettingsController : ControllerBase<V, M>
    {
        public SettingsController(IHost host, V binding) : base(host, binding)
        {

        }

        public override M? OnCreateViewModel() => M.Instance;

        readonly Dictionary<View, ComboBoxHelper.ListPopupWindowWrapper<string>> comboBoxs = new();

        void SetIsAutoCheckUpdateChecked() => binding.swGeneralSettingsIsAutoCheckUpdate.Checked = GeneralSettings.IsAutoCheckUpdate.Value;

        void SetUpdateChannelText() => binding.tvGeneralSettingsUpdateChannelValue.Text = GeneralSettings.UpdateChannel.Value.ToString();

        void SetThemeText() => binding.tvUISettingsThemeValue.Text = ((AppTheme)UISettings.Theme.Value).ToString3();

        void SetCaptureScreenChecked() => binding.swGeneralSettingsCaptureScreen.Checked = GeneralSettings.CaptureScreen.Value;

        void SetSpeedTestChecked() => binding.swProxySettingsSpeedTest.Checked = ProxySettings.SpeedTestEnable.Value;

        void SetSpeedTestTimeoutText() => binding.tvProxySettingsSpeedTestTimeoutValue.Text = $"{ProxySettings.SpeedTestTimeoutMs.Value} ms";

        void SetSpeedTestTTLText() => binding.tvProxySettingsSpeedTestTTLValue.Text = FormatCacheTtl(ProxySettings.SpeedTestCacheTTLSeconds.Value);

        void SetSpeedTestWriteBackChecked() => binding.swProxySettingsSpeedTestWriteBack.Checked = ProxySettings.SpeedTestWriteBack.Value;

        void SetProgramStartupRunProxyChecked() => binding.swProxySettingsProgramStartupRunProxy.Checked = ProxySettings.ProgramStartupRunProxy.Value;

        void SetHttpToHttpsChecked() => binding.swProxySettingsHttpToHttps.Checked = ProxySettings.EnableHttpProxyToHttps.Value;

        void SetMasterDnsText() => binding.tvProxySettingsMasterDnsValue.Text = ProxySettings.ProxyMasterDns.Value ?? "223.5.5.5";

        static string FormatCacheTtl(int seconds)
        {
            if (seconds % 3600 == 0) return $"{seconds / 3600} 小时";
            if (seconds % 60 == 0) return $"{seconds / 60} 分钟";
            return $"{seconds} 秒";
        }

        void SetProxyLogChecked() => binding.swProxySettingsLog.Checked = GeneralSettings.ProxyLogEnable.Value;

        void SetDynamicColorChecked() => binding.swUISettingsDynamicColor.Checked = UISettings.UseDynamicColor.Value;

        void SetFloatingBarChecked() => binding.swUISettingsFloatingBar.Checked = UISettings.UseFloatingBar.Value;

        void SetBlurChecked() => binding.swUISettingsBlur.Checked = UISettings.UseBlur.Value;

        static readonly string[] ThemeColorNames = new[]
        {
            "默认", "粉色", "红色", "橙色", "琥珀", "黄色", "黄绿", "绿色",
            "青色", "蓝绿", "浅蓝", "蓝色", "靛蓝", "紫色", "深紫", "蓝灰", "棕色", "灰色",
        };

        static readonly int[] ThemeColorValues = new[]
        {
            unchecked((int)0xFF6750A4), unchecked((int)0xFFD81B60), unchecked((int)0xFFD32F2F), unchecked((int)0xFFF57C00),
            unchecked((int)0xFFFFB300), unchecked((int)0xFFFBC02D), unchecked((int)0xFF9E9D24), unchecked((int)0xFF388E3C),
            unchecked((int)0xFF00ACC1), unchecked((int)0xFF00897B), unchecked((int)0xFF039BE5), unchecked((int)0xFF1976D2),
            unchecked((int)0xFF3949AB), unchecked((int)0xFF8E24AA), unchecked((int)0xFF5E35B1), unchecked((int)0xFF546E7A),
            unchecked((int)0xFF6D4C41), unchecked((int)0xFF757575),
        };

        void SetThemeColorText() => binding.tvUISettingsThemeColorValue.Text = GetThemeColorName(UISettings.ThemeColor.Value);

        static string GetThemeColorName(int color)
        {
            for (int i = 0; i < ThemeColorValues.Length; i++)
            {
                if (ThemeColorValues[i] == color) return ThemeColorNames[i];
            }
            return "默认";
        }

        public override void OnCreate()
        {
            if (IsActivity)
            {
                SetSupportActionBarWithNavigationClick(true);
            }

#if IS_STORE_PACKAGE // 渠道包隐藏下载更新渠道，更新通过应用商店分发
            binding.layoutRootGeneralSettingsUpdateChannel.Visibility = ViewStates.Gone;
#endif

            R.Subscribe(() =>
            {
                if (IsActivity)
                {
                    Activity.Title = ViewModel.Name;
                }
                if (binding == null) return;
                binding.tvUISettings.Text = Settings_UI;
                binding.tvUISettingsLanguage.Text = Settings_Language;
                binding.tvUISettingsTheme.Text = Settings_Theme;
                binding.tvGeneralSettings.Text = Settings_General;
                binding.tvGeneralSettingsIsAutoCheckUpdate.Text = Settings_General_AutoCheckUpdate;
                binding.tvGeneralSettingsUpdateChannel.Text = Settings_General_UpdateChannel;
                binding.tvGeneralSettingsStorageSpace.Text = Settings_General_StorageSpace;
                binding.tvOSAppDetailsSettings.Text = Settings_General_AppDetailsSettings;
                binding.tvOSAppNotificationSettings.Text = Settings_General_AppNotificationSettings;
                if (comboBoxs.TryGetValue(binding.layoutRootUISettingsTheme, out var comboBoxUISettingsTheme))
                {
                    comboBoxUISettingsTheme.Items = M.GetThemes();
                }
                binding.tvGeneralSettingsCaptureScreen.Text = Settings_General_CaptureScreen;
                binding.tvGeneralSettingsCaptureScreenDesc.Text = Settings_General_CaptureScreen_Desc;
                binding.tvSystemSettings.Text = Settings_System;
                binding.tvProxySettings.Text = Settings_Proxy;
                binding.tvProxySettingsSpeedTest.Text = Settings_Proxy_SpeedTest;
                binding.tvProxySettingsSpeedTestDesc.Text = Settings_Proxy_SpeedTest_Desc;
                binding.tvProxySettingsLog.Text = Settings_Proxy_Log;
                binding.tvProxySettingsLogDesc.Text = Settings_Proxy_Log_Desc;
                binding.tvProxySettingsSpeedTestTimeout.Text = Settings_Proxy_SpeedTest_Timeout;
                binding.tvProxySettingsSpeedTestTTL.Text = Settings_Proxy_SpeedTest_TTL;
                binding.tvProxySettingsSpeedTestWriteBack.Text = Settings_Proxy_SpeedTest_WriteBack;
                binding.tvProxySettingsSpeedTestWriteBackDesc.Text = Settings_Proxy_SpeedTest_WriteBack_Desc;
                binding.tvProxySettingsProgramStartupRunProxy.Text = Settings_Proxy_ProgramStartupRunProxy;
                binding.tvProxySettingsProgramStartupRunProxyDesc.Text = Settings_Proxy_ProgramStartupRunProxy_Desc;
                binding.tvProxySettingsHttpToHttps.Text = Settings_Proxy_HttpToHttps;
                binding.tvProxySettingsHttpToHttpsDesc.Text = Settings_Proxy_HttpToHttps_Desc;
                binding.tvProxySettingsMasterDns.Text = Settings_Proxy_MasterDns;
                binding.tvAppearanceSettings.Text = Settings_Appearance;
                binding.tvUISettingsDynamicColor.Text = Settings_Appearance_DynamicColor;
                binding.tvUISettingsDynamicColorDesc.Text = Settings_Appearance_DynamicColor_Desc;
                binding.tvUISettingsFloatingBar.Text = Settings_Appearance_FloatingBar;
                binding.tvUISettingsFloatingBarDesc.Text = Settings_Appearance_FloatingBar_Desc;
                binding.tvUISettingsBlur.Text = Settings_Appearance_Blur;
                binding.tvUISettingsBlurDesc.Text = Settings_Appearance_Blur_Desc;
                binding.tvUISettingsThemeColor.Text = Settings_Appearance_ThemeColor;
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.SelectLanguage).SubscribeInMainThread(x =>
            {
                if (binding == null) return;
                binding.tvUISettingsLanguageValue.Text = x.Value;
            }).AddTo(this);

            comboBoxs.Add(binding.layoutRootUISettingsLanguage, ComboBoxHelper.Popup(Context, R.Languages.Select(x => x.Value).ToJavaList(), x =>
            {
                ViewModel!.SelectLanguage = R.Languages.FirstOrDefault(y => y.Value == x);
            }, binding.layoutUISettingsLanguage));
            comboBoxs.Add(binding.layoutRootUISettingsTheme, ComboBoxHelper.Popup(Context, M.GetThemes(), x =>
            {
                if (comboBoxs.TryGetValue(binding.layoutRootUISettingsTheme, out var comboBoxUISettingsTheme))
                {
                    var index = comboBoxUISettingsTheme.Items.IndexOf(x);
                    if (index >= 0)
                    {
                        UISettings.Theme.Value = (short)index;
                        SetThemeText();
                    }
                }
            }, binding.layoutUISettingsTheme));
            comboBoxs.Add(binding.layoutRootGeneralSettingsUpdateChannel, ComboBoxHelper.Popup(Context, Enum2.GetAllStrings<UpdateChannelType>(), x =>
            {
                if (!Enum.TryParse<UpdateChannelType>(x, out var value)) return;
                GeneralSettings.UpdateChannel.Value = value;
                SetUpdateChannelText();
            }, binding.layoutUISettingsTheme));

            comboBoxs.Add(binding.layoutRootUISettingsThemeColor, ComboBoxHelper.Popup(Context, ThemeColorNames.ToJavaList(), x =>
            {
                var index = Array.IndexOf(ThemeColorNames, x);
                if (index >= 0)
                {
                    UISettings.ThemeColor.Value = ThemeColorValues[index];
                    SetThemeColorText();
                    RestartActivityForThemeChange();
                }
            }, binding.layoutUISettingsThemeColor));

            var speedTestTimeouts = new[] { 1000, 2000, 3000, 5000, 8000 };
            comboBoxs.Add(binding.layoutRootProxySettingsSpeedTestTimeout, ComboBoxHelper.Popup(Context, speedTestTimeouts.Select(x => $"{x} ms").ToJavaList(), x =>
            {
                var ms = int.Parse(x.Split(' ')[0]);
                ProxySettings.SpeedTestTimeoutMs.Value = ms;
                SetSpeedTestTimeoutText();
            }, binding.layoutRootProxySettingsSpeedTestTimeout));

            var speedTestCacheTtls = new[] { 60, 300, 600, 1800, 3600 };
            comboBoxs.Add(binding.layoutRootProxySettingsSpeedTestTTL, ComboBoxHelper.Popup(Context, speedTestCacheTtls.Select(FormatCacheTtl).ToJavaList(), x =>
            {
                var seconds = speedTestCacheTtls[Array.IndexOf(speedTestCacheTtls.Select(FormatCacheTtl).ToArray(), x)];
                ProxySettings.SpeedTestCacheTTLSeconds.Value = seconds;
                SetSpeedTestTTLText();
            }, binding.layoutRootProxySettingsSpeedTestTTL));

            var masterDnsOptions = new[] { "223.5.5.5", "119.29.29.29", "8.8.8.8", "1.1.1.1" };
            comboBoxs.Add(binding.layoutRootProxySettingsMasterDns, ComboBoxHelper.Popup(Context, masterDnsOptions.ToJavaList(), x =>
            {
                ProxySettings.ProxyMasterDns.Value = x;
                SetMasterDnsText();
            }, binding.layoutRootProxySettingsMasterDns));
            SetOnClickListener(comboBoxs.Keys);
            SetOnClickListener(
                binding.layoutRootGeneralSettingsIsAutoCheckUpdate,
                binding.layoutRootGeneralSettingsStorageSpace,
                binding.layoutRootOSAppDetailsSettings,
                binding.layoutRootOSAppNotificationSettings,
                binding.layoutRootGeneralSettingsCaptureScreen,
                binding.layoutRootProxySettingsSpeedTest,
                binding.layoutRootProxySettingsLog,
                binding.layoutRootProxySettingsSpeedTestTimeout,
                binding.layoutRootProxySettingsSpeedTestTTL,
                binding.layoutRootProxySettingsSpeedTestWriteBack,
                binding.layoutRootProxySettingsProgramStartupRunProxy,
                binding.layoutRootProxySettingsHttpToHttps,
                binding.layoutRootProxySettingsMasterDns,
                binding.layoutRootUISettingsDynamicColor,
                binding.layoutRootUISettingsFloatingBar,
                binding.layoutRootUISettingsBlur);

            SetIsAutoCheckUpdateChecked();
            SetCaptureScreenChecked();
            SetUpdateChannelText();
            SetThemeText();
            SetSpeedTestChecked();
            SetProxyLogChecked();
            SetDynamicColorChecked();
            SetFloatingBarChecked();
            SetBlurChecked();
            SetThemeColorText();
            SetSpeedTestTimeoutText();
            SetSpeedTestTTLText();
            SetSpeedTestWriteBackChecked();
            SetProgramStartupRunProxyChecked();
            SetHttpToHttpsChecked();
            SetMasterDnsText();

            M.StartSizeCalcByCacheSize(x =>
            {
                if (binding == null) return;
                binding.tvGeneralSettingsStorageSpaceValue.Text = x;
            });
            M.StartSizeCalcByLogSize(x =>
            {
                if (binding == null) return;
                binding.tvGeneralSettingsStorageSpaceValue2.Text = x;
            });
        }

        public override void OnResume()
        {
            base.OnResume();
            var enabledNotification = INotificationService.Instance.AreNotificationsEnabled();
            binding!.swOSAppNotificationSettings.Checked = enabledNotification;
        }

        void RestartActivityForThemeChange()
        {
            try
            {
                if (IsActivity && Activity != null)
                {
                    Activity.Recreate();
                }
            }
            catch
            {
            }
        }

        public override bool OnClick(View view)
        {
            foreach (var item in comboBoxs)
            {
                if (view.Id == item.Key.Id)
                {
                    item.Value.Show();
                    return true;
                }
            }

            if (view.Id == Resource.Id.layoutRootOSAppDetailsSettings)
            {
                GoToPlatformPages.AppDetailsSettings(Context);
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootOSAppNotificationSettings)
            {
                GoToPlatformPages.NotificationSettings(Context);
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootGeneralSettingsIsAutoCheckUpdate)
            {
                GeneralSettings.IsAutoCheckUpdate.Value = !GeneralSettings.IsAutoCheckUpdate.Value;
                SetIsAutoCheckUpdateChecked();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootGeneralSettingsCaptureScreen)
            {
                GeneralSettings.CaptureScreen.Value = !GeneralSettings.CaptureScreen.Value;
                SetCaptureScreenChecked();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootProxySettingsSpeedTest)
            {
                ProxySettings.SpeedTestEnable.Value = !ProxySettings.SpeedTestEnable.Value;
                SetSpeedTestChecked();
                if (!ProxySettings.SpeedTestEnable.Value)
                    ILatencyTestService.Instance.ClearCache();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootProxySettingsLog)
            {
                GeneralSettings.ProxyLogEnable.Value = !GeneralSettings.ProxyLogEnable.Value;
                SetProxyLogChecked();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootProxySettingsSpeedTestWriteBack)
            {
                ProxySettings.SpeedTestWriteBack.Value = !ProxySettings.SpeedTestWriteBack.Value;
                SetSpeedTestWriteBackChecked();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootProxySettingsProgramStartupRunProxy)
            {
                ProxySettings.ProgramStartupRunProxy.Value = !ProxySettings.ProgramStartupRunProxy.Value;
                SetProgramStartupRunProxyChecked();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootProxySettingsHttpToHttps)
            {
                ProxySettings.EnableHttpProxyToHttps.Value = !ProxySettings.EnableHttpProxyToHttps.Value;
                SetHttpToHttpsChecked();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootUISettingsDynamicColor)
            {
                UISettings.UseDynamicColor.Value = !UISettings.UseDynamicColor.Value;
                SetDynamicColorChecked();
                RestartActivityForThemeChange();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootUISettingsFloatingBar)
            {
                UISettings.UseFloatingBar.Value = !UISettings.UseFloatingBar.Value;
                SetFloatingBarChecked();
                return true;
            }
            else if (view.Id == Resource.Id.layoutRootUISettingsBlur)
            {
                UISettings.UseBlur.Value = !UISettings.UseBlur.Value;
                SetBlurChecked();
                return true;
            }

            return base.OnClick(view);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (IsActivity)
            {
                // Fragment 中执行会导致闪退
                comboBoxs.Clear();
            }
        }
    }
}

namespace System.Application.UI.Fragments
{
#if __XAMARIN_FORMS__
    internal sealed class SettingsFragment : BaseMvcFragment<V, M, C>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_settings;
    }
#endif
}

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(SettingsActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
         LaunchMode = LaunchMode.SingleTask,
         ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class SettingsActivity : BaseMvcActivity<V, M, C>
    {
        protected override int? LayoutResource => Resource.Layout.activity_settings_not_binding;
    }
}