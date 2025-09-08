using Ncp.CleanDDD.Web.Endpoints.UserEndpoints;
using Ncp.CleanDDD.Web.Tests.Extensions;
using NetCorePal.Extensions.Dto;
using System.Net.Http.Headers;
using FastEndpoints.Testing;
using FastEndpoints;
using Ncp.CleanDDD.Domain.AggregatesModel.UserAggregate;

namespace Ncp.CleanDDD.Web.Tests.Base;

/// <summary>
/// Web测试基类，提供公共的认证和HTTP客户端功能
/// </summary>
public abstract class BaseWebTest : IDisposable
{
    protected readonly HttpClient Client;
    protected string? AuthToken;
    protected string? RefreshToken;
    protected UserId? TestUserId;

    protected BaseWebTest(MyWebApplicationFactory factory)
    {
        Client = factory.WithWebHostBuilder(builder => { builder.ConfigureServices(_ => { }); })
            .CreateClient();

        // 在构造函数中登录获取token
        LoginAndGetToken().GetAwaiter().GetResult();
    }

    public virtual void Dispose()
    {
        // 子类可以重写此方法进行特定的清理工作
    }

    /// <summary>
    /// 登录并获取认证令牌
    /// </summary>
    protected virtual async Task LoginAndGetToken()
    {
        const string json = $$"""
                              {
                                   "username": "{{AppDefaultCredentials.UserName}}",
                                   "password": "{{AppDefaultCredentials.Password}}"
                              }
                              """;
        var content = new StringContent(json);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var response = await Client.PostAsync("api/user/login", content);

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadFromNewtonsoftJsonAsync<ResponseData<LoginResponse>>();
            if (responseData?.Data != null)
            {
                AuthToken = responseData.Data.Token;
                RefreshToken = responseData.Data.RefreshToken;
                TestUserId = responseData.Data.UserId;

                // 设置认证头
                SetAuthHeader(true);
            }
        }
    }

    /// <summary>
    /// 设置HTTP客户端的认证头
    /// </summary>
    /// <param name="useAuth">是否使用认证</param>
    protected void SetAuthHeader(bool useAuth = true)
    {
        if (useAuth && AuthToken != null)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
        }
        else
        {
            Client.DefaultRequestHeaders.Authorization = null;
        }
    }

    /// <summary>
    /// 刷新认证令牌
    /// </summary>
    protected async Task RefreshAuthToken()
    {
        if (RefreshToken == null)
        {
            await LoginAndGetToken();
            return;
        }

        var refreshRequest = new RefreshTokenRequest(RefreshToken);
        var (response, result) = await Client.POSTAsync<RefreshTokenEndpoint, RefreshTokenRequest, ResponseData<RefreshTokenResponse>>(refreshRequest);

        if (response.IsSuccessStatusCode && result?.Data != null)
        {
            AuthToken = result.Data.Token;
            RefreshToken = result.Data.RefreshToken;
            SetAuthHeader(true);
        }
    }

    /// <summary>
    /// 执行需要认证的操作
    /// </summary>
    /// <param name="action">要执行的操作</param>
    /// <param name="useAuth">是否使用认证</param>
    protected async Task<T> ExecuteWithAuth<T>(Func<Task<T>> action, bool useAuth = true)
    {
        var originalAuthState = Client.DefaultRequestHeaders.Authorization;
        
        try
        {
            SetAuthHeader(useAuth);
            return await action();
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = originalAuthState;
        }
    }

    /// <summary>
    /// 执行需要认证的操作（无返回值）
    /// </summary>
    /// <param name="action">要执行的操作</param>
    /// <param name="useAuth">是否使用认证</param>
    protected async Task ExecuteWithAuth(Func<Task> action, bool useAuth = true)
    {
        var originalAuthState = Client.DefaultRequestHeaders.Authorization;
        
        try
        {
            SetAuthHeader(useAuth);
            await action();
        }
        finally
        {
            Client.DefaultRequestHeaders.Authorization = originalAuthState;
        }
    }
}
