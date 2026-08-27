using Android.Runtime;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Activities;
using System.Application.UI.Adapters;
using System.Application.UI.ViewModels;
using static System.Application.UI.ViewModels.MyPageViewModel;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(MyFragment))]
    internal sealed class MyFragment : BaseFragment<fragment_my, MyPageViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_my;

        protected override MyPageViewModel? OnCreateViewModel() => Instance;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            ViewModel!.WhenAnyValue(x => x.NickName).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.tvNickName.Text = value;
            }).AddTo(this);

            var adapter = new LargePreferenceButtonAdapter<PreferenceButtonViewModel, PreferenceButton>(ViewModel!.PreferenceButtons);
            adapter.ItemClick += (_, e) =>
            {
                switch (e.Current.Id)
                {
                    case PreferenceButton.Settings:
                        this.StartActivity<SettingsActivity>();
                        break;
                    case PreferenceButton.About:
                        this.StartActivity<AboutActivity>();
                        break;
                }
            };
            binding.rvPreferenceButtons.SetLinearLayoutManager();
            binding.rvPreferenceButtons.AddVerticalGroupItemDecoration(binding.rvPreferenceButtons.PaddingTop);
            binding.rvPreferenceButtons.SetAdapter(adapter);
        }
    }
}
