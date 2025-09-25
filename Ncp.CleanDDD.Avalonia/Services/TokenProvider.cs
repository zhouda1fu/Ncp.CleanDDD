namespace Ncp.CleanDDD.Avalonia.Services
{
    /// <summary>
    /// Token提供者实现
    /// </summary>
    public class TokenProvider : ITokenProvider
    {
        private string _token = string.Empty;

        /// <summary>
        /// 设置认证token
        /// </summary>
        /// <param name="token">JWT token</param>
        public void SetToken(string token)
        {
            _token = token ?? string.Empty;
        }

        /// <summary>
        /// 获取当前认证token
        /// </summary>
        public string GetToken()
        {
            return _token;
        }
    }
}
