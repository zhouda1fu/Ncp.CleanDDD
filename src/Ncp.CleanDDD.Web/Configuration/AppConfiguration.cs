namespace Ncp.CleanDDD.Web.Configuration
{
    public class AppConfiguration
    {
        public string Secret { get; set; } = string.Empty;
        public int TokenExpiryInMinutes { get; set; }
    }
}
