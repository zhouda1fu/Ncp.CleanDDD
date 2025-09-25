# Ncp.CleanDDD Avalonia 前端应用

这是一个基于 Avalonia UI 的桌面应用程序，用于替代原有的 Vue.js 前端，提供用户管理、角色管理和组织架构管理功能。

## 功能特性

### 🔐 用户认证
- 用户登录/登出
- JWT Token 认证
- 权限验证

### 👥 用户管理
- 用户列表查看
- 用户创建/编辑/删除
- 用户搜索和筛选
- 批量操作（删除、重置密码）
- 角色分配

### 🛡️ 角色管理
- 角色列表查看
- 角色创建/编辑/删除
- 权限分配
- 角色搜索

### 🏢 组织架构管理
- 组织架构树形显示
- 组织架构列表管理
- 组织架构创建/编辑/删除
- 层级关系管理

## 技术栈

- **UI框架**: Avalonia UI 11.3.5
- **MVVM模式**: ReactiveUI
- **依赖注入**: Microsoft.Extensions.DependencyInjection
- **HTTP客户端**: System.Net.Http.Json
- **日志**: Microsoft.Extensions.Logging
- **JSON序列化**: Newtonsoft.Json

## 项目结构

```
Ncp.CleanDDD.Avalonia/
├── Models/                 # 数据模型
│   ├── User.cs
│   ├── Role.cs
│   ├── OrganizationUnit.cs
│   └── Auth.cs
├── Services/              # 服务层
│   ├── IApiService.cs
│   ├── ApiService.cs
│   ├── IAuthService.cs
│   └── AuthService.cs
├── ViewModels/            # 视图模型
│   ├── ViewModelBase.cs
│   ├── MainWindowViewModel.cs
│   ├── LoginViewModel.cs
│   ├── UsersViewModel.cs
│   ├── RolesViewModel.cs
│   └── OrganizationUnitsViewModel.cs
├── Views/                 # 视图
│   ├── MainWindow.axaml
│   ├── LoginView.axaml
│   ├── UsersView.axaml
│   ├── RolesView.axaml
│   └── OrganizationUnitsView.axaml
├── Converters/            # 转换器
│   └── PageToViewConverter.cs
└── Assets/               # 资源文件
```

## 配置说明

### API 端点配置

在 `App.axaml.cs` 中配置后端 API 地址：

```csharp
services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7000"); // 修改为实际API地址
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
```

### 权限配置

应用支持以下权限验证：
- `UserCreate`, `UserEdit`, `UserDelete`, `UserView`, `UserRoleAssign`, `UserResetPassword`
- `RoleCreate`, `RoleEdit`, `RoleDelete`, `RoleView`
- `OrganizationUnitCreate`, `OrganizationUnitEdit`, `OrganizationUnitDelete`, `OrganizationUnitView`

## 运行说明

### 前提条件
- .NET 9.0 SDK
- 后端 API 服务运行在配置的地址上

### 启动应用
```bash
cd Ncp.CleanDDD.Avalonia
dotnet run
```

### 构建发布版本
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

## 主要功能说明

### 1. 登录功能
- 支持用户名/密码登录
- 自动保存认证状态
- 登录失败显示错误信息

### 2. 用户管理
- 分页显示用户列表
- 支持按用户名、邮箱搜索
- 支持按状态、组织架构筛选
- 支持批量删除和重置密码
- 支持用户角色分配

### 3. 角色管理
- 显示角色列表和权限信息
- 支持角色创建、编辑、删除
- 支持权限分配管理

### 4. 组织架构管理
- 左侧树形结构显示组织层级
- 右侧列表显示组织详情
- 支持组织的增删改查操作

## 开发说明

### 添加新功能
1. 在 `Models/` 中定义数据模型
2. 在 `Services/` 中实现业务逻辑
3. 在 `ViewModels/` 中创建视图模型
4. 在 `Views/` 中创建 XAML 视图
5. 在 `App.axaml.cs` 中注册服务

### 样式定制
应用使用 Fluent Design 主题，可以通过修改 XAML 中的样式来自定义外观。

## 注意事项

1. 确保后端 API 服务正常运行
2. 检查 API 地址配置是否正确
3. 确保用户具有相应的权限才能访问对应功能
4. 应用支持 Windows、macOS 和 Linux 平台

## 故障排除

### 常见问题
1. **无法连接到后端**: 检查 API 地址配置和网络连接
2. **权限不足**: 确保用户具有相应的权限
3. **数据加载失败**: 检查后端 API 是否正常响应

### 日志查看
应用使用控制台日志，可以在控制台中查看详细的错误信息。
