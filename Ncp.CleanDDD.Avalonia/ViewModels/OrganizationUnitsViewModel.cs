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
    public class OrganizationUnitsViewModel : ViewModelBase
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly ILogger<OrganizationUnitsViewModel> _logger;
        
        private ObservableCollection<OrganizationUnit> _organizationUnits = new();
        private ObservableCollection<OrganizationUnitTree> _organizationTree = new();
        private bool _isLoading;
        private string _searchName = string.Empty;
        private OrganizationUnit? _selectedOrganizationUnit;

        public OrganizationUnitsViewModel(IApiService apiService, IAuthService authService, ILogger<OrganizationUnitsViewModel> logger)
        {
            _apiService = apiService;
            _authService = authService;
            _logger = logger;

            // 初始化命令
            LoadOrganizationUnitsCommand = ReactiveCommand.CreateFromTask(LoadOrganizationUnitsAsync);
            LoadTreeCommand = ReactiveCommand.CreateFromTask(LoadTreeAsync);
            SearchCommand = ReactiveCommand.CreateFromTask(SearchAsync);
            CreateOrganizationUnitCommand = ReactiveCommand.Create(CreateOrganizationUnit);
            EditOrganizationUnitCommand = ReactiveCommand.Create<OrganizationUnit>(EditOrganizationUnit);
            DeleteOrganizationUnitCommand = ReactiveCommand.CreateFromTask<OrganizationUnit>(DeleteOrganizationUnitAsync);
            RefreshCommand = ReactiveCommand.CreateFromTask(LoadOrganizationUnitsAsync);

            // 自动加载数据
            LoadOrganizationUnitsCommand.Execute(null);
            LoadTreeCommand.Execute(null);
        }

        public ObservableCollection<OrganizationUnit> OrganizationUnits
        {
            get => _organizationUnits;
            set => this.RaiseAndSetIfChanged(ref _organizationUnits, value);
        }

        public ObservableCollection<OrganizationUnitTree> OrganizationTree
        {
            get => _organizationTree;
            set => this.RaiseAndSetIfChanged(ref _organizationTree, value);
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

        public OrganizationUnit? SelectedOrganizationUnit
        {
            get => _selectedOrganizationUnit;
            set => this.RaiseAndSetIfChanged(ref _selectedOrganizationUnit, value);
        }

        public ICommand LoadOrganizationUnitsCommand { get; }
        public ICommand LoadTreeCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateOrganizationUnitCommand { get; }
        public ICommand EditOrganizationUnitCommand { get; }
        public ICommand DeleteOrganizationUnitCommand { get; }
        public ICommand RefreshCommand { get; }

        // 权限检查
        public bool CanCreateOrganizationUnit => _authService.HasPermission("OrganizationUnitCreate");
        public bool CanEditOrganizationUnit => _authService.HasPermission("OrganizationUnitEdit");
        public bool CanDeleteOrganizationUnit => _authService.HasPermission("OrganizationUnitDelete");

        private async Task LoadOrganizationUnitsAsync()
        {
            try
            {
                IsLoading = true;

                var query = new OrganizationUnitQueryInput
                {
                    Name = string.IsNullOrWhiteSpace(SearchName) ? null : SearchName
                };

                var response = await _apiService.GetOrganizationUnitsAsync(query);
                
                if (response.Success && response.Data != null)
                {
                    OrganizationUnits.Clear();
                    foreach (var org in response.Data)
                    {
                        OrganizationUnits.Add(org);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载组织架构列表失败");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTreeAsync()
        {
            try
            {
                var response = await _apiService.GetOrganizationUnitTreeAsync();
                
                if (response.Success && response.Data != null)
                {
                    OrganizationTree.Clear();
                    foreach (var org in response.Data)
                    {
                        OrganizationTree.Add(org);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载组织架构树失败");
            }
        }

        private async Task SearchAsync()
        {
            await LoadOrganizationUnitsAsync();
        }

        private void CreateOrganizationUnit()
        {
            // TODO: 打开创建组织架构对话框
        }

        private void EditOrganizationUnit(OrganizationUnit organizationUnit)
        {
            SelectedOrganizationUnit = organizationUnit;
            // TODO: 打开编辑组织架构对话框
        }

        private async Task DeleteOrganizationUnitAsync(OrganizationUnit organizationUnit)
        {
            try
            {
                var response = await _apiService.DeleteOrganizationUnitAsync(organizationUnit.Id);
                if (response.Success)
                {
                    await LoadOrganizationUnitsAsync();
                    await LoadTreeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除组织架构失败");
            }
        }
    }
}
