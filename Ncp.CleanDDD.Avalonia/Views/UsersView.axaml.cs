using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ncp.CleanDDD.Avalonia.ViewModels;

namespace Ncp.CleanDDD.Avalonia.Views
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<UsersViewModel>();
        }
    }
}
