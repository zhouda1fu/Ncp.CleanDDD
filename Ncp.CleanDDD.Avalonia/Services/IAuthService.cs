using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Ncp.CleanDDD.Avalonia.Models;

namespace Ncp.CleanDDD.Avalonia.Services
{
    /// <summary>
    /// 认证服务接口
    /// </summary>
    public interface IAuthService : INotifyPropertyChanged
    {
        /// <summary>
        /// 当前用户信息
        /// </summary>
        LoginResponse? CurrentUser { get; }

        /// <summary>
        /// 是否已登录
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// 登录
        /// </summary>
        Task<bool> LoginAsync(LoginCredentials credentials);

        /// <summary>
        /// 登出
        /// </summary>
        Task LogoutAsync();

        /// <summary>
        /// 检查权限
        /// </summary>
        bool HasPermission(string permission);

        /// <summary>
        /// 检查多个权限（需要全部拥有）
        /// </summary>
        bool HasAllPermissions(params string[] permissions);

        /// <summary>
        /// 检查多个权限（拥有任意一个即可）
        /// </summary>
        bool HasAnyPermission(params string[] permissions);
    }
}
