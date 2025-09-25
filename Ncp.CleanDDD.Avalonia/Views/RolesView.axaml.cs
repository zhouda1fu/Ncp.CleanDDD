using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ncp.CleanDDD.Avalonia.ViewModels;

namespace Ncp.CleanDDD.Avalonia.Views
{
    public partial class RolesView : UserControl
    {
        public RolesView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<RolesViewModel>();
        }
    }
}
