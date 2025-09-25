using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Windows.Input;
using Ncp.CleanDDD.Avalonia.Services;
using Ncp.CleanDDD.Avalonia.Views;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private string _currentPage = "Startup";
        private bool _isLoggedIn;

        public MainWindowViewModel(IAuthService authService)
        {
            _authService = authService;
            
            // 初始化命令
            NavigateToUsersCommand = ReactiveCommand.Create(() => NavigateTo("Users"));
            NavigateToRolesCommand = ReactiveCommand.Create(() => NavigateTo("Roles"));
            NavigateToOrganizationUnitsCommand = ReactiveCommand.Create(() => NavigateTo("OrganizationUnits"));
            LogoutCommand = ReactiveCommand.CreateFromTask(LogoutAsync);
            
            // 监听认证状态变化
            _authService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IAuthService.IsAuthenticated))
                {
                    IsLoggedIn = _authService.IsAuthenticated;
                    if (!IsLoggedIn)
                    {
                        CurrentPage = "Login";
                    }
                    else
                    {
                        CurrentPage = "Users"; // 登录后默认显示用户管理页面
                    }
                }
            };
            
            IsLoggedIn = _authService.IsAuthenticated;
            // 初始化时根据登录状态切换页面
            CurrentPage = IsLoggedIn ? "Users" : "Login";
        }

        public string CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set => this.RaiseAndSetIfChanged(ref _isLoggedIn, value);
        }

        public ICommand NavigateToUsersCommand { get; }
        public ICommand NavigateToRolesCommand { get; }
        public ICommand NavigateToOrganizationUnitsCommand { get; }
        public ICommand LogoutCommand { get; }

        private void NavigateTo(string page)
        {
            CurrentPage = page;
        }

        private async Task LogoutAsync()
        {
            await _authService.LogoutAsync();
        }
    }
}
