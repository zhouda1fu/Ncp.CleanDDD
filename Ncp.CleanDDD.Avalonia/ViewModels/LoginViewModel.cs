using ReactiveUI;
using System;
using System.Reactive;
using System.Windows.Input;
using Ncp.CleanDDD.Avalonia.Models;
using Ncp.CleanDDD.Avalonia.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ncp.CleanDDD.Avalonia.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginViewModel> _logger;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _isLoading;
        private string _errorMessage = string.Empty;

        public LoginViewModel(IAuthService authService, ILogger<LoginViewModel> logger)
        {
            _authService = authService;
            _logger = logger;

            LoginCommand = ReactiveCommand.CreateFromTask(LoginAsync, this.WhenAnyValue(x => x.Username, x => x.Password, (u, p) => !string.IsNullOrWhiteSpace(u) && !string.IsNullOrWhiteSpace(p)));
        }

        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var credentials = new LoginCredentials
                {
                    Username = Username,
                    Password = Password
                };

                var success = await _authService.LoginAsync(credentials);
                
                if (!success)
                {
                    ErrorMessage = "用户名或密码错误";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录过程中发生错误");
                ErrorMessage = "登录失败，请稍后重试";
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
