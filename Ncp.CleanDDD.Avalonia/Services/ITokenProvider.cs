namespace Ncp.CleanDDD.Avalonia.Services
{
    /// <summary>
    /// Token提供者接口
    /// </summary>
    public interface ITokenProvider
    {
        /// <summary>
        /// 获取当前认证token
        /// </summary>
        string GetToken();
    }
}
