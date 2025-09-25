using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ncp.CleanDDD.Avalonia.ViewModels;

namespace Ncp.CleanDDD.Avalonia.Views
{
    public partial class OrganizationUnitsView : UserControl
    {
        public OrganizationUnitsView()
        {
            InitializeComponent();
            DataContext = App.ServiceProvider.GetRequiredService<OrganizationUnitsViewModel>();
        }
    }
}
