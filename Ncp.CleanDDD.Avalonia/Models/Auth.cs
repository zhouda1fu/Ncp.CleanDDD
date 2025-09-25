using System;
using System.Collections.Generic;

namespace Ncp.CleanDDD.Avalonia.Models
{
    /// <summary>
    /// 登录凭据
    /// </summary>
    public class LoginCredentials
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// 登录响应
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Permissions { get; set; } = string.Empty;
    }

    /// <summary>
    /// 用户状态枚举
    /// </summary>
    public enum UserStatus
    {
        Disabled = 0,
        Enabled = 1
    }

    /// <summary>
    /// 性别枚举
    /// </summary>
    public enum Gender
    {
        Male = 0,
        Female = 1
    }
}
