using ReactiveUI;
using System.Application.Mvvm;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;

namespace System.Application.UI.ViewModels
{
    /// <summary>
    /// 我的页面视图模型
    /// </summary>
    public partial class MyPageViewModel : PageViewModel
    {
        public static string DisplayName => AppResources.My;

        public MyPageViewModel()
        {
            preferenceButtons = new()
            {
                PreferenceButtonViewModel.Create(PreferenceButton.Settings, this),
                PreferenceButtonViewModel.Create(PreferenceButton.About, this),
            };

            NickName = AppResources.My;
        }

        string nickName = AppResources.My;
        public string NickName
        {
            get => nickName;
            set => this.RaiseAndSetIfChanged(ref nickName, value);
        }

        ObservableCollection<PreferenceButtonViewModel> preferenceButtons;
        /// <summary>
        /// 我的选项按钮组
        /// </summary>
        public ObservableCollection<PreferenceButtonViewModel> PreferenceButtons
        {
            get => preferenceButtons;
            set => this.RaiseAndSetIfChanged(ref preferenceButtons, value);
        }

        /// <summary>
        /// 我的选项按钮组唯一键
        /// </summary>
        public enum PreferenceButton
        {
            Settings = 1,
            About,
        }

        /// <summary>
        /// 我的选项按钮视图模型
        /// </summary>
        public sealed class PreferenceButtonViewModel : RIdTitleIconViewModel<PreferenceButton, ResIcon>, IReadOnlyItemViewGroup
        {
            PreferenceButtonViewModel()
            {
            }

            public int ItemViewGroup { get; set; }

            protected override string GetTitleById(PreferenceButton id)
            {
                var title = id switch
                {
                    PreferenceButton.Settings => AppResources.Settings,
                    PreferenceButton.About => AppResources.About,
                    _ => string.Empty,
                };
                return title;
            }

            protected override ResIcon GetIconById(PreferenceButton id)
            {
                var icon = id switch
                {
                    PreferenceButton.Settings => ResIcon.baseline_settings_black_24,
                    PreferenceButton.About => ResIcon.baseline_info_black_24,
                    _ => default,
                };
                return icon;
            }

            /// <summary>
            /// 是否需要已登录的用户
            /// </summary>
            public bool Authentication => false;

            /// <summary>
            /// 创建实例
            /// </summary>
            public static PreferenceButtonViewModel Create(PreferenceButton id, IDisposableHolder vm, int groupId = default)
            {
                PreferenceButtonViewModel r = new() { Id = id, ItemViewGroup = groupId, };
                r.OnBind(vm);
                return r;
            }
        }
    }
}
