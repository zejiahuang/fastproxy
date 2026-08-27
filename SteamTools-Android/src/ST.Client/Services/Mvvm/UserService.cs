using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Application.Models;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.Application.Services
{
    public sealed class UserService : ReactiveObject
    {
        static UserService? mCurrent;

        public static UserService Current => mCurrent ?? new();

        public Task ShowWindow(CustomWindow windowName) => Task.CompletedTask;

        public void NavigateUserCenterPage() { }

        public Task SignOutAsync(System.Func<Task<IApiResponse>>? apiCall = null, string? message = null) => Task.CompletedTask;

        public void SignOut() { }

        public Task SignIn() => Task.CompletedTask;

        public Task DelAccountAsync() => Task.CompletedTask;

        public Task SignOutUserManagerAsync() => Task.CompletedTask;

        [Reactive]
        public UserInfoDTO? User { get; set; }

        public bool IsAuthenticated => User != null;

        [Reactive]
        public SteamUser? CurrentSteamUser { get; set; }

        public object? AvatarPath { get; set; }

        [Reactive]
        public bool HasPhoneNumber { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public void RefreshCurrentUser(CurrentUser? currentUser) { }

        public Task SaveUserAsync(UserInfoDTO user) => Task.CompletedTask;

        public Task RefreshUserAsync(UserInfoDTO? user, bool refreshCurrentUser = true) => Task.CompletedTask;

        public Task RefreshUserAsync(bool refreshCurrentUser = true) => Task.CompletedTask;

        public Task RefreshUserAvatarAsync() => Task.CompletedTask;

        public Task UpdateCurrentUserPhoneNumberAsync(string phoneNumber) => Task.CompletedTask;

        public Task UnbundleAccountAfterUpdateAsync(FastLoginChannel channel) => Task.CompletedTask;

        UserService() { mCurrent = this; }
    }
}
