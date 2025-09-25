using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Ncp.CleanDDD.Avalonia.Models;
using Ncp.CleanDDD.Avalonia.Services;

namespace Ncp.CleanDDD.Avalonia.ViewModels
{
    public class RolesViewModel : ViewModelBase
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<RolesViewModel> _logger;
        
        private ObservableCollection<Role> _roles = new();
        private ObservableCollection<Role> _selectedRoles = new();
        private bool _isLoading;
        private string _searchName = string.Empty;
        private Role? _selectedRole;

        public RolesViewModel(IApiService apiService, IAuthService authService, ILogger<RolesViewModel> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;

            // 初始化命令
            LoadRolesCommand = ReactiveCommand.CreateFromTask(LoadRolesAsync);
            SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync);
            CreateRoleCommand = ReactiveCommand.Create(CreateRole);
            EditRoleCommand = ReactiveCommand.Create<Role>(EditRole);
            DeleteRoleCommand = ReactiveCommand.CreateFromTask<Role>(DeleteRoleAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(LoadRolesAsync);

            // 自动加载数据
            LoadRolesCommand.Execute(null);
        }

        public ObservableCollection<Role> Roles
        {
            get => _roles;
            set => this.RaiseAndSetIfChanged(ref _roles, value);
        }

        public ObservableCollection<Role> SelectedRoles
        {
            get => _selectedRoles;
            set => this.RaiseAndSetIfChanged(ref _selectedRoles, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public string SearchName
        {
            get => _searchName;
            set => this.RaiseAndSetIfChanged(ref _searchName, value);
        }

        public Role? SelectedRole
        {
            get => _selectedRole;
            set => this.RaiseAndSetIfChanged(ref _selectedRole, value);
        }

        public ICommand LoadRolesCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateRoleCommand { get; }
        public ICommand EditRoleCommand { get; }
        public ICommand DeleteRoleCommand { get; }
        public ICommand RefreshCommand { get; }

        // 权限检查
        public bool CanCreateRole => _authService.HasPermission("RoleCreate");
        public bool CanEditRole => _authService.HasPermission("RoleEdit");
        public bool CanDeleteRole => _authService.HasPermission("RoleDelete");

        private async Task LoadRolesAsync()
        {
            try
            {
                IsLoading = true;

                var query = new RoleQueryInput
                {
                    PageIndex = 1,
                    PageSize = 100,
                    CountTotal = false,
                    Name = string.IsNullOrWhiteSpace(SearchName) ? null : SearchName
                };

                var response = await _apiService.GetRolesAsync(query);
                
                if (response.Success && response.Data != null)
                {
                    Roles.Clear();
                    foreach (var role in response.Data.Items)
                    {
                        Roles.Add(role);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载角色列表失败");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchAsync()
        {
            await LoadRolesAsync();
        }

        private void CreateRole()
        {
            // TODO: 打开创建角色对话框
        }

        private void EditRole(Role role)
        {
            SelectedRole = role;
            // TODO: 打开编辑角色对话框
        }

        private async Task DeleteRoleAsync(Role role)
        {
            try
            {
                var response = await _apiService.DeleteRoleAsync(role.RoleId);
                if (response.Success)
                {
                    await LoadRolesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除角色失败");
            }
        }
    }
}
