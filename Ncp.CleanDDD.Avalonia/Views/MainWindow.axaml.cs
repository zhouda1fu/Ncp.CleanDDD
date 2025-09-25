using Avalonia.Controls;
using Ncp.CleanDDD.Avalonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace Ncp.CleanDDD.Avalonia.Views
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            // 设置数据上下文
            _viewModel = App.ServiceProvider.GetRequiredService<MainWindowViewModel>();
            DataContext = _viewModel;

            // 监听页面变化
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            // 初始化页面
            UpdateContent();
        }

      

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainWindowViewModel.CurrentPage))
            {
                UpdateContent();
            }
        }

        private void UpdateContent()
        {
            if (_viewModel == null) return;

            var pageName = _viewModel.CurrentPage;
            Control content = pageName switch
            {
                "Startup" => App.ServiceProvider.GetRequiredService<StartupView>(),
                "Login" => App.ServiceProvider.GetRequiredService<LoginView>(),
                "Users" => App.ServiceProvider.GetRequiredService<UsersView>(),
                "Roles" => App.ServiceProvider.GetRequiredService<RolesView>(),
                "OrganizationUnits" => App.ServiceProvider.GetRequiredService<OrganizationUnitsView>(),
                _ => new TextBlock { Text = $"未知页面: {pageName}" }
            };

            MainContent.Content = content;
        }
    }
}