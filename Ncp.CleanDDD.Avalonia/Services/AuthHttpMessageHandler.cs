using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Ncp.CleanDDD.Avalonia.Services
{
    /// <summary>
    /// 自动添加认证头的HTTP消息处理器
    /// </summary>
    public class AuthHttpMessageHandler : DelegatingHandler
    {
        private readonly ITokenProvider _tokenProvider;
        private readonly ILogger<AuthHttpMessageHandler> _logger;

        public AuthHttpMessageHandler(HttpMessageHandler innerHandler, ITokenProvider tokenProvider, ILogger<AuthHttpMessageHandler> logger) 
            : base(innerHandler)
        {
            _tokenProvider = tokenProvider;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 自动添加认证头
            var token = _tokenProvider.GetToken();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("为请求 {RequestUri} 添加认证头", request.RequestUri);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
