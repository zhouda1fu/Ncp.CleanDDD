using System;
using System.Collections.Generic;

namespace Ncp.CleanDDD.Avalonia.Models
{
    /// <summary>
    /// 用户模型
    /// </summary>
    public class User
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string RealName { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int Age { get; set; }
        public string OrganizationUnitId { get; set; } = string.Empty;
        public string OrganizationUnitName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public bool IsSelected { get; set; }

      
    }

    /// <summary>
    /// 用户查询输入参数
    /// </summary>
    public class UserQueryInput
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public bool CountTotal { get; set; } = true;
        public string? Keyword { get; set; }
        public int? Status { get; set; }
        public int? OrganizationUnitId { get; set; }
    }

    /// <summary>
    /// 分页数据响应
    /// </summary>
    public class PagedData<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }

    /// <summary>
    /// API响应基类
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int Code { get; set; }
        public T? Data { get; set; }
        public List<object> ErrorData { get; set; } = new();
    }
}
