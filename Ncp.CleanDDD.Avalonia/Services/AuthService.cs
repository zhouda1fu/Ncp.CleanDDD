using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ncp.CleanDDD.Avalonia.Models;

namespace Ncp.CleanDDD.Avalonia.Services
{
    /// <summary>
    /// 认证服务实现
    /// </summary>
    public class AuthService : IAuthService, INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AuthService> _logger;
        private LoginResponse? _currentUser;
        private List<string> _permissions = new();

        public AuthService(IApiService apiService, ILogger<AuthService> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public LoginResponse? CurrentUser => _currentUser;

        public bool IsAuthenticated => _currentUser != null;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<bool> LoginAsync(LoginCredentials credentials)
        {
            try
            {
                var response = await _apiService.LoginAsync(credentials);
                
                if (response.Success && response.Data != null)
                {
                    _currentUser = response.Data;
                    
                    // 设置认证token到HttpClient
                    _apiService.SetAuthToken(_currentUser.Token);
                    
                    // 解析权限字符串
                    if (!string.IsNullOrEmpty(_currentUser.Permissions))
                    {
                        _permissions = _currentUser.Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.Trim())
                            .ToList();
                    }
                    
                    OnPropertyChanged(nameof(IsAuthenticated));
                    _logger.LogInformation("用户 {UserName} 登录成功", _currentUser.Name);
                    return true;
                }
                else
                {
                    _logger.LogWarning("登录失败: {Message}", response.Message);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登录过程中发生错误");
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                if (IsAuthenticated)
                {
                    await _apiService.LogoutAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "登出过程中发生错误");
            }
            finally
            {
                // 清除认证token
                _apiService.SetAuthToken(string.Empty);
                
                _currentUser = null;
                _permissions.Clear();
                OnPropertyChanged(nameof(IsAuthenticated));
                _logger.LogInformation("用户已登出");
            }
        }

        public bool HasPermission(string permission)
        {
            if (!IsAuthenticated)
                return false;

            return _permissions.Contains(permission);
        }

        public bool HasAllPermissions(params string[] permissions)
        {
            if (!IsAuthenticated || permissions == null || permissions.Length == 0)
                return false;

            return permissions.All(p => _permissions.Contains(p));
        }

        public bool HasAnyPermission(params string[] permissions)
        {
            if (!IsAuthenticated || permissions == null || permissions.Length == 0)
                return false;

            return permissions.Any(p => _permissions.Contains(p));
        }
    }
}
