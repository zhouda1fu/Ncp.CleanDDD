using FastEndpoints;
using Ncp.CleanDDD.Domain.AggregatesModel.RoleAggregate;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;
using Ncp.CleanDDD.Web.Application.Commands.UserCommands;
using Ncp.CleanDDD.Web.Application.Queries;
using Ncp.CleanDDD.Domain.AggregatesModel.OrganizationUnitAggregate;
using Ncp.CleanDDD.Web.Utils;

namespace Ncp.CleanDDD.Web.Endpoints.UserEndpoints;

/// <summary>
/// 用户注册的请求模型
/// </summary>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
/// <param name="Password">密码</param>
/// <param name="Phone">电话号码</param>
/// <param name="RealName">真实姓名</param>
/// <param name="Status">用户状态</param>
/// <param name="Gender">性别</param>
/// <param name="Age">年龄</param>
/// <param name="BirthDate">出生日期</param>
/// <param name="OrganizationUnitId">组织单位ID（可选）</param>
/// <param name="OrganizationUnitName">组织单位名称（可选）</param>
/// <param name="RoleIds">要分配的角色ID列表</param>
public record RegisterRequest(string Name, string Email, string Password, string Phone, string RealName, int Status, string Gender, int Age, DateTimeOffset BirthDate, OrganizationUnitId? OrganizationUnitId, string? OrganizationUnitName, IEnumerable<RoleId> RoleIds);

/// <summary>
/// 用户注册的响应模型
/// </summary>
/// <param name="UserId">新创建的用户ID</param>
/// <param name="Name">用户名</param>
/// <param name="Email">邮箱地址</param>
public record RegisterResponse(UserId UserId, string Name, string Email);

/// <summary>
/// 用户注册的API端点
/// 该端点用于在系统中创建新的用户账户，支持角色分配和组织单位设置
/// </summary>
/// <param name="mediator">中介者模式接口，用于处理命令和查询</param>
/// <param name="roleQuery">角色查询服务，用于执行角色相关的查询操作</param>
[Tags("Users")] // API文档标签，用于Swagger文档分组
public class RegisterEndpoint(IMediator mediator, RoleQuery roleQuery) : Endpoint<RegisterRequest, ResponseData<RegisterResponse>>
{
    /// <summary>
    /// 配置端点的基本设置
    /// 包括HTTP方法、认证方案、权限要求等
    /// </summary>
    public override void Configure()
    {
        // 设置HTTP POST方法，用于创建新用户
        Post("/api/user/register");
        
        // 允许匿名访问，新用户无需认证即可注册
        AllowAnonymous();
    }

    /// <summary>
    /// 处理HTTP请求的核心方法
    /// 验证角色信息，处理密码哈希，创建用户账户并返回结果
    /// </summary>
    /// <param name="request">包含用户注册信息的请求对象</param>
    /// <param name="ct">取消令牌，用于支持异步操作的取消</param>
    /// <returns>异步任务</returns>
    public override async Task HandleAsync(RegisterRequest request, CancellationToken ct)
    {
        // 通过角色查询服务验证要分配的角色信息
        // 确保角色存在且可用于分配
        var rolesToBeAssigned = await roleQuery.GetAdminRolesForAssignmentAsync(request.RoleIds, ct);

        // 对用户密码进行哈希处理，确保安全性
        var passwordHash = PasswordHasher.HashPassword(request.Password);
        
        // 创建用户命令对象，包含所有用户信息和角色分配
        var cmd = new CreateUserCommand(
            request.Name,                // 用户名
            request.Email,               // 邮箱
            passwordHash,                // 密码哈希
            request.Phone,               // 电话
            request.RealName,            // 真实姓名
            request.Status,              // 状态
            request.Gender,              // 性别
            request.BirthDate,           // 出生日期
            request.OrganizationUnitId,  // 组织单位ID
            request.OrganizationUnitName, // 组织单位名称
            rolesToBeAssigned            // 要分配的角色列表
        );
        
        // 通过中介者发送命令，执行实际的用户创建业务逻辑
        // 返回新创建的用户ID
        var userId = await mediator.Send(cmd, ct);
        
        // 创建响应对象，包含新创建的用户信息
        var response = new RegisterResponse(
            userId,        // 用户ID
            request.Name,  // 用户名
            request.Email  // 邮箱
        );

        // 返回成功响应，使用统一的响应数据格式包装
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 用户注册端点的API文档配置
/// </summary>
public class RegisterSummary : Summary<RegisterEndpoint, RegisterRequest>
{
    public RegisterSummary()
    {
        Summary = "用户注册";
        Description = "在系统中创建新的用户账户，支持角色分配、组织单位设置和个人信息配置";
        Response<RegisterResponse>(200, "用户注册成功");
        ExampleRequest = new RegisterRequest(
            "john.doe",
            "john.doe@example.com",
            "SecurePassword123",
            "13800138000",
            "张三",
            1,
            "男",
            25,
            DateTimeOffset.Now.AddYears(-25),
            new OrganizationUnitId(1),
            "技术部",
            new[] { new RoleId(Guid.NewGuid()), new RoleId(Guid.NewGuid()) }
        );
        Responses[200] = "成功创建用户账户";
        Responses[400] = "请求参数无效";
        Responses[409] = "用户名或邮箱已存在";
        Responses[422] = "角色分配失败";
    }
}