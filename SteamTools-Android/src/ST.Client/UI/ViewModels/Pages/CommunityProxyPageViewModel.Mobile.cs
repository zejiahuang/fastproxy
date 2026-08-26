// ReSharper disable once CheckNamespace
using System.Application.Services;
using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    partial class CommunityProxyPageViewModel : IActionItem<CommunityProxyPageViewModel.ActionItem>
    {
        public enum ActionItem
        {
            ProxySettings = 1,
        }

        string IActionItem<ActionItem>.ToString2(ActionItem action) => ToString2(action);

        public static string ToString2(ActionItem action) => action switch
        {
            ActionItem.ProxySettings => AppResources.CommunityFix_ProxySettings,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        string IActionItem<ActionItem>.GetIcon(ActionItem action) => GetIcon(action);

        public static string GetIcon(ActionItem action) => action switch
        {
            _ => "baseline_settings_black_24",
        };

        public void MenuItemClick(ActionItem id)
        {
            switch (id)
            {
                case ActionItem.ProxySettings:
                    ProxySettingsCommand?.Invoke();
                    break;
            }
        }
    }
}
