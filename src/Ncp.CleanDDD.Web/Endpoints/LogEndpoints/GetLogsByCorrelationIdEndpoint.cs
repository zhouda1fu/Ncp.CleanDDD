using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.CleanDDD.Web.Endpoints.LogEndpoints;

/// <summary>
/// 根据关联ID获取日志列表的API端点
/// 该端点用于查询特定关联ID下的所有日志记录，便于追踪请求的完整日志链路
/// </summary>
[Tags("Logs")] // API文档标签，用于Swagger文档分组
public class GetLogsByCorrelationIdEndpoint(LogQuery logQuery) : Endpoint<GetLogsByCorrelationIdRequest, ResponseData<IEnumerable<LogItemDto>>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP GET方法，用于查询特定关联ID的日志信息
        Get("/api/logs/correlation/{CorrelationId}");
        
        // 设置JWT Bearer认证方案，要求用户必须提供有效的JWT令牌
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        
        // 设置权限要求：用户必须同时拥有API访问权限和日志查看权限
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LogView);
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 根据关联ID查询相关的所有日志记录
    /// </summary>
    /// <param name="req">包含关联ID的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(GetLogsByCorrelationIdRequest req, CancellationToken ct)
    {
        // 调用日志查询服务根据关联ID获取日志数据
        var result = await logQuery.GetLogsByCorrelationIdAsync(req.CorrelationId, ct);

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(new ResponseData<IEnumerable<LogItemDto>>(result), cancellation: ct);
    }
}

/// <summary>
/// 根据关联ID获取日志列表的请求模型
/// </summary>
/// <param name="CorrelationId">关联ID，用于标识同一请求链路的所有日志</param>
public record GetLogsByCorrelationIdRequest(string CorrelationId);

/// <summary>
/// 根据关联ID获取日志列表端点的API文档配置
/// 提供详细的API文档信息，包括参数说明和示例
/// </summary>
public class GetLogsByCorrelationIdEndpointSummary : Summary<GetLogsByCorrelationIdEndpoint>
{
    /// <summary>
    /// 构造函数，配置API文档信息
    /// </summary>
    public GetLogsByCorrelationIdEndpointSummary()
    {
        Summary = "根据关联ID获取日志列表";
        Description = "查询特定关联ID下的所有日志记录，按时间顺序排列，便于追踪请求的完整日志链路";
        
        // 响应说明
        Response<ResponseData<IEnumerable<LogItemDto>>>(200, "成功获取关联ID下的日志列表");
        Response(401, "未授权访问");
        Response(403, "权限不足");
        Response(404, "未找到指定关联ID的日志");
        Response(500, "服务器内部错误");
    }
}
