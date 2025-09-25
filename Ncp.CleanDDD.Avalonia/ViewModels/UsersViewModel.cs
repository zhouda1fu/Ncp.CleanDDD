using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Ncp.CleanDDD.Avalonia.Models;
using Ncp.CleanDDD.Avalonia.Services;

namespace Ncp.CleanDDD.Avalonia.ViewModels
{
    public class UsersViewModel : ViewModelBase
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<UsersViewModel> _logger;
        
        private ObservableCollection<User> _users = new();
        private ObservableCollection<User> _selectedUsers = new();
        private bool _isLoading;
        private string _searchKeyword = string.Empty;
        private int? _statusFilter;
        private int? _organizationUnitFilter;
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalCount;
        private User? _selectedUser;

        public UsersViewModel(IApiService apiService, IAuthService authService, ILogger<UsersViewModel> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;

            // 初始化命令
            LoadUsersCommand = ReactiveCommand.CreateFromTask(LoadUsersAsync);
            SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync);
            CreateUserCommand = ReactiveCommand.Create(CreateUser);
            EditUserCommand = ReactiveCommand.Create<User>(EditUser);
            DeleteUserCommand = ReactiveCommand.CreateFromTask<User>(DeleteUserAsync);
            ResetPasswordCommand = ReactiveCommand.CreateFromTask<User>(ResetPasswordAsync);
            AssignRolesCommand = ReactiveCommand.Create<User>(AssignRoles);
            BatchDeleteCommand = ReactiveCommand.CreateFromTask(BatchDeleteAsync);
            BatchResetPasswordCommand = ReactiveCommand.CreateFromTask(BatchResetPasswordAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(LoadUsersAsync);
            PageChangedCommand = ReactiveCommand.Create<int>(OnPageChanged);

            // 自动加载数据
            LoadUsersCommand.Execute(null);
        }

        public ObservableCollection<User> Users
        {
            get => _users;
            set => this.RaiseAndSetIfChanged(ref _users, value);
        }

        public ObservableCollection<User> SelectedUsers
        {
            get => _selectedUsers;
            set => this.RaiseAndSetIfChanged(ref _selectedUsers, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => this.RaiseAndSetIfChanged(ref _isLoading, value);
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set => this.RaiseAndSetIfChanged(ref _searchKeyword, value);
        }

        public int? StatusFilter
        {
            get => _statusFilter;
            set => this.RaiseAndSetIfChanged(ref _statusFilter, value);
        }

        public int? OrganizationUnitFilter
        {
            get => _organizationUnitFilter;
            set => this.RaiseAndSetIfChanged(ref _organizationUnitFilter, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public int PageSize
        {
            get => _pageSize;
            set => this.RaiseAndSetIfChanged(ref _pageSize, value);
        }

        public int TotalCount
        {
            get => _totalCount;
            set => this.RaiseAndSetIfChanged(ref _totalCount, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set => this.RaiseAndSetIfChanged(ref _selectedUser, value);
        }

        public ICommand LoadUsersCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ResetPasswordCommand { get; }
        public ICommand AssignRolesCommand { get; }
        public ICommand BatchDeleteCommand { get; }
        public ICommand BatchResetPasswordCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand PageChangedCommand { get; }

        // 权限检查
        public bool CanCreateUser => _authService.HasPermission("UserCreate");
        public bool CanEditUser => _authService.HasPermission("UserEdit");
        public bool CanDeleteUser => _authService.HasPermission("UserDelete");
        public bool CanAssignRoles => _authService.HasPermission("UserRoleAssign");
        public bool CanResetPassword => _authService.HasPermission("UserResetPassword");

        private async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;

                var query = new UserQueryInput
                {
                    PageIndex = CurrentPage,
                    PageSize = PageSize,
                    CountTotal = true,
                    Keyword = string.IsNullOrWhiteSpace(SearchKeyword) ? null : SearchKeyword,
                    Status = StatusFilter,
                    OrganizationUnitId = OrganizationUnitFilter
                };

                var response = await _apiService.GetUsersAsync(query);
                
                if (response.Success && response.Data != null)
                {
                    Users.Clear();
                    foreach (var user in response.Data.Items)
                    {
                        Users.Add(user);
                    }
                    TotalCount = response.Data.Total;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载用户列表失败");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchAsync()
        {
            CurrentPage = 1;
            await LoadUsersAsync();
        }

        private void CreateUser()
        {
            // TODO: 打开创建用户对话框
        }

        private void EditUser(User user)
        {
            SelectedUser = user;
            // TODO: 打开编辑用户对话框
        }

        private async Task DeleteUserAsync(User user)
        {
            try
            {
                var response = await _apiService.DeleteUserAsync(user.UserId);
                if (response.Success)
                {
                    await LoadUsersAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户失败");
            }
        }

        private async Task ResetPasswordAsync(User user)
        {
            try
            {
                var response = await _apiService.ResetPasswordAsync(user.UserId);
                if (response.Success)
                {
                    // TODO: 显示成功消息
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重置密码失败");
            }
        }

        private void AssignRoles(User user)
        {
            SelectedUser = user;
            // TODO: 打开角色分配对话框
        }

        private async Task BatchDeleteAsync()
        {
            try
            {
                foreach (var user in SelectedUsers.ToList())
                {
                    await _apiService.DeleteUserAsync(user.UserId);
                }
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量删除用户失败");
            }
        }

        private async Task BatchResetPasswordAsync()
        {
            try
            {
                foreach (var user in SelectedUsers.ToList())
                {
                    await _apiService.ResetPasswordAsync(user.UserId);
                }
                // TODO: 显示成功消息
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量重置密码失败");
            }
        }

        private void OnPageChanged(int page)
        {
            CurrentPage = page;
            LoadUsersCommand.Execute(null);
        }
    }
}
