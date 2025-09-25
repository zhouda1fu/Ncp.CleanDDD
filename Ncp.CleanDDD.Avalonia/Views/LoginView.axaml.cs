using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ncp.CleanDDD.Avalonia.ViewModels;

namespace Ncp.CleanDDD.Avalonia.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
            // 设置运行时数据上下文
            DataContext = App.ServiceProvider.GetRequiredService<LoginViewModel>();
        }
    }
}
