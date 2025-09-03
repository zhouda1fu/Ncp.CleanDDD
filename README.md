# Ncp.CleanDDD

一个基于Clean Architecture和Domain-Driven Design (DDD)的现代化.NET Web应用程序框架，集成了Vue 3前端和完整的后端架构。

## 🏗️ 项目架构

本项目采用Clean Architecture和DDD设计模式，包含以下核心层次：

- **Domain Layer** (`Ncp.CleanDDD.Domain`) - 领域模型、聚合根、领域事件
- **Infrastructure Layer** (`Ncp.CleanDDD.Infrastructure`) - 数据访问、仓储实现、外部服务
- **Application Layer** (`Ncp.CleanDDD.Web`) - 应用服务、命令查询、端点实现
- **Frontend** (`frontend/`) - Vue 3 + TypeScript前端应用

## 🚀 快速开始

### 环境准备

#### 1. 数据库服务 (Docker)

```bash
# MySQL 数据库
docker run --restart always --name mysql \
  -v /mnt/d/docker/mysql/data:/var/lib/mysql \
  -e MYSQL_ROOT_PASSWORD=123456 \
  -p 3306:3306 -d mysql:latest

# RabbitMQ 消息队列
docker run --restart always -d --hostname node1 \
  --name rabbitmq -p 15672:15672 -p 5672:5672 \
  rabbitmq:3-management

# Redis 缓存
docker run --restart always --name redis \
  -v /mnt/d/docker/redis:/data \
  -p 6379:6379 -d redis:5.0.7 redis-server
```

#### 2. 后端启动

```bash
# 进入后端项目目录
cd src/Ncp.CleanDDD.Web

# 还原NuGet包
dotnet restore

# 运行数据库迁移
dotnet ef database update -p ../Ncp.CleanDDD.Infrastructure

# 启动应用
dotnet run
```

#### 3. 前端启动

```bash
# 进入前端目录
cd frontend

# 安装依赖
npm install

# 启动开发服务器
npm run dev
```

## 🛠️ 技术栈

### 后端技术
- **.NET 9** - 最新版本的.NET框架
- **ASP.NET Core** - Web应用框架
- **Entity Framework Core** - ORM框架
- **NetCorePal Cloud Framework** - 企业级应用框架
- **MediatR** - 中介者模式实现
- **FluentValidation** - 数据验证
- **CAP** - 分布式事务和消息队列
- **Swagger/OpenAPI** - API文档

### 前端技术
- **Vue 3** - 渐进式JavaScript框架
- **TypeScript** - 类型安全的JavaScript超集
- **Vite** - 现代前端构建工具
- **Vue Router** - 官方路由管理器
- **Pinia** - Vue状态管理库

### 基础设施
- **MySQL** - 关系型数据库
- **Redis** - 内存数据库和缓存
- **RabbitMQ** - 消息队列
- **Prometheus** - 监控和指标收集

## 📁 项目结构

```
Ncp.CleanDDD/
├── src/
│   ├── Ncp.CleanDDD.Domain/           # 领域层
│   │   ├── AggregatesModel/           # 聚合根模型
│   │   │   ├── UserAggregate/         # 用户聚合
│   │   │   ├── RoleAggregate/         # 角色聚合
│   │   │   └── OrganizationUnitAggregate/ # 组织单位聚合
│   │   └── DomainEvents/              # 领域事件
│   ├── Ncp.CleanDDD.Infrastructure/   # 基础设施层
│   │   ├── EntityConfigurations/      # EF Core实体配置
│   │   ├── Repositories/              # 仓储实现
│   │   └── Migrations/                # 数据库迁移
│   └── Ncp.CleanDDD.Web/             # 应用层
│       ├── Application/               # 应用服务
│       │   ├── Commands/              # 命令处理
│       │   ├── Queries/               # 查询处理
│       │   └── DomainEventHandlers/   # 领域事件处理器
│       ├── Endpoints/                 # API端点
│       └── AppPermissions/            # 权限定义
├── frontend/                          # Vue 3前端应用
│   ├── src/
│   │   ├── views/                     # 页面组件
│   │   ├── components/                # 通用组件
│   │   ├── stores/                    # 状态管理
│   │   └── router/                    # 路由配置
└── test/                              # 测试项目
```

## 🔧 开发工具配置

### IDE 代码片段配置

本模板提供了丰富的代码片段，帮助您快速生成常用的代码结构。

#### Visual Studio 配置

运行以下 PowerShell 命令自动安装代码片段：

```powershell
cd vs-snippets
.\Install-VSSnippets.ps1
```

或者手动安装：

1. 打开 Visual Studio
2. 转到 `工具` > `代码片段管理器`
3. 导入 `vs-snippets/NetCorePalTemplates.snippet` 文件

#### VS Code 配置

VS Code 的代码片段已预配置在 `.vscode/csharp.code-snippets` 文件中，打开项目时自动生效。

#### JetBrains Rider 配置

Rider 用户可以直接使用 `Ncp.CleanDDD.sln.DotSettings` 文件中的 Live Templates 配置。

### 可用的代码片段

#### NetCorePal (ncp) 快捷键
| 快捷键 | 描述 | 生成内容 |
|--------|------|----------|
| `ncpcmd` | NetCorePal 命令 | ICommand 实现(含验证器和处理器) |
| `ncpcmdres` | 命令(含返回值) | ICommand&lt;Response&gt; 实现 |
| `ncpar` | 聚合根 | Entity&lt;Id&gt; 和 IAggregateRoot |
| `ncprepo` | NetCorePal 仓储 | IRepository 接口和实现 |
| `ncpie` | 集成事件 | IntegrationEvent 和处理器 |
| `ncpdeh` | 域事件处理器 | IDomainEventHandler 实现 |
| `ncpiec` | 集成事件转换器 | IIntegrationEventConverter |
| `ncpde` | 域事件 | IDomainEvent 记录 |

#### Endpoint (ep) 快捷键
| 快捷键 | 描述 | 生成内容 |
|--------|------|----------|
| `epp` | FastEndpoint(NCP风格) | 完整的垂直切片实现 |
| `epreq` | 仅请求端点 | Endpoint&lt;Request&gt; |
| `epres` | 仅响应端点 | EndpointWithoutRequest&lt;Response&gt; |
| `epdto` | 端点 DTOs | Request 和 Response 类 |
| `epval` | 端点验证器 | Validator&lt;Request&gt; |
| `epmap` | 端点映射器 | Mapper&lt;Request, Response, Entity&gt; |
| `epfull` | 完整端点切片 | 带映射器的完整实现 |
| `epsum` | 端点摘要 | Summary&lt;Endpoint, Request&gt; |
| `epnoreq` | 无请求端点 | EndpointWithoutRequest |
| `epreqres` | 请求响应端点 | Endpoint&lt;Request, Response&gt; |
| `epdat` | 端点数据 | 静态数据类 |

更多详细配置请参考：[vs-snippets/README.md](vs-snippets/README.md)

## 🗄️ 数据库管理

### 数据库迁移

```bash
# 安装EF Core工具
dotnet tool install --global dotnet-ef --version 9.0.0

# 强制更新数据库
dotnet ef database update -p src/Ncp.CleanDDD.Infrastructure 

# 创建新迁移
dotnet ef migrations add InitialCreate -p src/Ncp.CleanDDD.Infrastructure 
```

### 数据库连接

确保在 `appsettings.json` 中配置正确的数据库连接字符串：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=NcpCleanDDD;User=root;Password=123456;"
  }
}
```

## 📊 代码分析可视化

框架提供了强大的代码流分析和可视化功能，帮助开发者直观地理解DDD架构中的组件关系和数据流向。

### 🎯 核心特性

+ **自动代码分析**：通过源生成器自动分析代码结构，识别控制器、命令、聚合根、事件等组件
+ **多种图表类型**：支持架构流程图、命令链路图、事件流程图、类图等多种可视化图表
+ **交互式HTML可视化**：生成完整的交互式HTML页面，内置导航和图表预览功能
+ **一键在线编辑**：集成"View in Mermaid Live"按钮，支持一键跳转到在线编辑器

### 🚀 快速开始

安装命令行工具来生成独立的HTML文件：

```bash
# 安装全局工具
dotnet tool install -g NetCorePal.Extensions.CodeAnalysis.Tools

# 进入项目目录并生成可视化文件
cd src/Ncp.CleanDDD.Web
netcorepal-codeanalysis generate --output architecture.html
```

### ✨ 主要功能

+ **交互式HTML页面**：
  + 左侧树形导航，支持不同图表类型切换
  + 内置Mermaid.js实时渲染
  + 响应式设计，适配不同设备
  + 专业的现代化界面

+ **一键在线编辑**：
  + 每个图表右上角的"View in Mermaid Live"按钮
  + 智能压缩算法优化URL长度
  + 自动跳转到[Mermaid Live Editor](https://mermaid.live/)
  + 支持在线编辑、导出图片、生成分享链接

### 📖 详细文档

完整的使用说明和示例请参考：

+ [代码流分析文档](https://netcorepal.github.io/netcorepal-cloud-framework/zh/code-analysis/code-flow-analysis/)
+ [代码分析工具文档](https://netcorepal.github.io/netcorepal-cloud-framework/zh/code-analysis/code-analysis-tools/)

## 📈 监控和性能

### Prometheus 监控

这里使用了`prometheus-net`作为与基础设施prometheus集成的监控方案，默认通过地址 `/metrics` 输出监控指标。

更多信息请参见：[https://github.com/prometheus-net/prometheus-net](https://github.com/prometheus-net/prometheus-net)

### 性能优化

- 使用EF Core的查询优化
- Redis缓存策略
- 异步编程模式
- 依赖注入优化

## 🧪 测试

项目包含完整的测试套件：

```bash
# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test test/Ncp.CleanDDD.Domain.Tests/
dotnet test test/Ncp.CleanDDD.Infrastructure.Tests/
dotnet test test/Ncp.CleanDDD.Web.Tests/
```

## 📚 学习资源

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [NetCorePal Cloud Framework](https://github.com/netcorepal/netcorepal-cloud-framework)
- [ASP.NET Core 文档](https://docs.microsoft.com/zh-cn/aspnet/core/)

## 🤝 贡献指南

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 📞 联系方式

保密

---



