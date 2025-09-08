using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.LogEndpoints;

/// <summary>
/// 获取日志列表的请求模型
/// </summary>
/// <param name="PageIndex">页码（从0开始）</param>
/// <param name="PageSize">每页大小</param>
/// <param name="Level">日志级别筛选</param>
/// <param name="StartTime">开始时间</param>
/// <param name="EndTime">结束时间</param>
/// <param name="Keyword">关键词搜索（在消息中搜索）</param>
/// <param name="CountTotal">是否计算总数</param>
public record GetLogsRequest(
    int PageIndex = 0,
    int PageSize = 20,
    string? Level = null,
    DateTime? StartTime = null,
    DateTime? EndTime = null,
    string? Keyword = null,
    bool CountTotal = true
);

/// <summary>
/// 获取日志列表的API端点
/// 该端点用于查询系统中的日志信息，支持分页、筛选和搜索
/// </summary>
[Tags("Logs")] // API文档标签，用于Swagger文档分组
public class GetLogsEndpoint(LogQuery logQuery) : Endpoint<GetLogsRequest, ResponseData<PagedData<LogItemDto>>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，用于查询日志信息
        Get("/api/logs");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和日志查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LogView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 根据请求参数查询日志数据并返回分页结果
    /// </summary>
    /// <param name="req">包含查询参数的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(GetLogsRequest req, CancellationToken ct)
    {
        // 调用日志查询服务获取分页数据
        var result = await logQuery.GetLogsAsync(
            pageIndex: req.PageIndex,
            pageSize: req.PageSize,
            level: req.Level,
            startTime: req.StartTime,
            endTime: req.EndTime,
            keyword: req.Keyword,
            countTotal: req.CountTotal,
            cancellationToken: ct
        );

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(new ResponseData<PagedData<LogItemDto>>(result), cancellation: ct);
    }
}

/// <summary>
/// 获取日志列表端点的API文档配置
/// 提供详细的API文档信息，包括参数说明和示例
/// </summary>
public class GetLogsEndpointSummary : Summary<GetLogsEndpoint>
{
    /// <summary>
    /// 构造函数，配置API文档信息
    /// </summary>
    public GetLogsEndpointSummary()
    {
        Summary = "获取日志列表";
        Description = "查询系统中的日志信息，支持分页、按级别筛选、时间范围筛选和关键词搜索";
        
        // 响应说明
        Response<ResponseData<PagedData<LogItemDto>>>(200, "成功获取日志列表");
        Response(401, "未授权访问");
        Response(403, "权限不足");
        Response(500, "服务器内部错误");
    }
}
