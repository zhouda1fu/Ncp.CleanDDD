using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ncp.CleanDDD.Avalonia.Services;
using Ncp.CleanDDD.Avalonia.ViewModels;
using Ncp.CleanDDD.Avalonia.Views;
using System;
using System.Net.Http;

namespace Ncp.CleanDDD.Avalonia
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; } = null!;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // 配置依赖注入
            ConfigureServices();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // 添加日志
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            });

            // 添加HTTP客户端
            services.AddHttpClient<IApiService, ApiService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7058"); // 根据实际API地址调整
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // 添加服务
            services.AddSingleton<IAuthService, AuthService>();

            // 添加视图模型
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<UsersViewModel>();
            services.AddTransient<RolesViewModel>();
            services.AddTransient<OrganizationUnitsViewModel>();

            // 添加视图
            services.AddTransient<StartupView>();
            services.AddTransient<LoginView>();
            services.AddTransient<UsersView>();
            services.AddTransient<RolesView>();
            services.AddTransient<OrganizationUnitsView>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}