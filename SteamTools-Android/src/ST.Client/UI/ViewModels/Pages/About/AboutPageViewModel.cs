using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class AboutPageViewModel
    {
        public static AboutPageViewModel Instance { get; } = new();

        public ReactiveCommand<Unit, Unit> CheckUpdateCommand { get; }

        public ReactiveCommand<string, Unit> OpenBrowserCommand { get; }

        public ICommand UIDCommand { get; }

        public AboutPageViewModel()
        {
            OpenBrowserCommand = ReactiveCommand.CreateFromTask<string>(x => Browser2.OpenAsync(x));

            CheckUpdateCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                await IApplicationUpdateService.Instance.CheckUpdateAsync(showIsExistUpdateFalse: true);
            });

            UIDCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var uid = UserService.Current.User?.Id;
                if (uid.HasValue)
                {
                    await IApplication.CopyToClipboardAsync(uid.Value.ToString());
                }
                else
                {
                    Toast.Show(AppResources.YouNeedSignInToGetUID);
                }
            });

            if (!IApplication.IsDesktopPlatform)
            {
                preferenceButtons = new(Enum2.GetAll<PreferenceButton>().Select(x => PreferenceButtonViewModel.Create(x, this)));
            }
        }

        public override void Activation()
        {
            base.Activation();
        }

        public override void Deactivation()
        {
            base.Deactivation();
        }

        public string VersionDisplay => $"{ThisAssembly.VersionDisplay} for {DeviceInfo2.OSName()} ({RuntimeInformation.ProcessArchitecture.ToString().ToLower()})";

        public string LabelVersionDisplay => ThisAssembly.IsAlphaRelease ? "Alpha Version:" : (ThisAssembly.IsBetaRelease ? "Beta Version:" : "Current Version:");

        public static string Copyright
        {
            get
            {
                // https://www.w3cschool.cn/html/html-copyright.html
                int startYear = 2020, thisYear = 2021;
                var nowYear = DateTime.Now.Year;
                if (nowYear < thisYear) nowYear = thisYear;
                return $"© {startYear}{(nowYear == startYear ? startYear : "-" + nowYear)} {ThisAssembly.AssemblyCompany}. All Rights Reserved.";
            }
        }

        public const string Zhengye = "Zhengye";
        public const string 沙中金 = "沙中金";
        public const string EspRoy = "EspRoy";

        //public ICommand ContributorsCommand { get; } = ReactiveCommand.CreateFromTask<string?>(async (p, _) =>
        //{
        //    switch (p)
        //    {
        //        case 沙中金:
        //            await Email2.ComposeAsync(new() { To = new() { "" } });
        //            break;
        //        case EspRoy:
        //            await Email2.ComposeAsync(new() { To = new() { "" } });
        //            break;
        //    }
        //});

        #region Urls

        public static string RmbadminSteamLink => SteamApiUrls.MY_PROFILE_URL;

        public static string RmbadminLink => UrlConstants.GitHub_User_Rmbadmin;

        public static string AigioLLink => UrlConstants.GitHub_User_AigioL;

        public static string MossimosLink => UrlConstants.GitHub_User_Mossimos;

        public static string RmbadminEmailLink => UrlConstants.Rmbadmin_Email;

        public static string PrivacyLink => UrlConstants.OfficialWebsite_Privacy;

        public static string AgreementLink => UrlConstants.OfficialWebsite_Agreement;

        public static string OfficialLink => UrlConstants.OfficialWebsite;

        public static string SourceCodeLink => UrlConstants.GitHub_Repository;

        public static string UserSupportLink => UrlConstants.OfficialWebsite_Contact;

        public static string BugReportLink => UrlConstants.GitHub_Issues;

        public static string FAQLink => UrlConstants.OfficialWebsite_Faq;

        public static string ChangeLogLink => UrlConstants.OfficialWebsite_Changelog;

        public static string LicenseLink => UrlConstants.License_GPLv3;

        public static string MicrosoftStoreReviewLink => UrlConstants.MicrosoftStoreReviewLink;

        #endregion

        public string AppName => ThisAssembly.DisplayTrademark;

        public string FormerAppName => string.Format(Title_2_, AppResources.About_FormerName);
    }
}
