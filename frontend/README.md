# CleanDDD 前端管理系统

基于 Vue 3 + TypeScript + Element Plus 构建的现代化管理系统前端。

## 技术栈

- **Vue 3** - 渐进式 JavaScript 框架
- **TypeScript** - JavaScript 的超集，提供类型安全
- **Element Plus** - 基于 Vue 3 的组件库
- **Vue Router** - Vue.js 官方路由管理器
- **Pinia** - Vue 的状态管理库
- **Axios** - HTTP 客户端
- **Vite** - 下一代前端构建工具

## 功能特性

- 🔐 用户认证与授权
- 👥 用户管理
- 🏷️ 角色管理
- 📊 仪表盘
- 📱 响应式设计
- 🎨 现代化 UI 设计

## 项目结构

```
frontend/
├── src/
│   ├── api/           # API 接口
│   ├── layouts/       # 布局组件
│   ├── router/        # 路由配置
│   ├── stores/        # 状态管理
│   ├── views/         # 页面组件
│   ├── App.vue        # 根组件
│   └── main.ts        # 入口文件
├── index.html         # HTML 模板
├── package.json       # 依赖配置
├── tsconfig.json      # TypeScript 配置
├── vite.config.ts     # Vite 配置
└── README.md          # 项目文档
```

## 快速开始

### 环境要求

- Node.js >= 16.0.0
- npm >= 8.0.0

### 安装依赖

```bash
cd frontend
npm install
```

### 开发环境运行

```bash
npm run dev
```

访问 http://localhost:3000 查看应用。

### 构建生产版本

```bash
npm run build
```

构建产物将输出到 `dist` 目录。

### 预览生产版本

```bash
npm run preview
```

## API 接口

项目已配置代理，开发环境下 API 请求会自动转发到后端服务（默认 http://localhost:5000）。

### 主要接口

- **用户认证**
  - `POST /api/user/login` - 用户登录
  - `POST /api/user/register` - 用户注册

- **用户管理**
  - `GET /api/user/profile/{userId}` - 获取用户资料
  - `PUT /api/user/{userId}` - 更新用户信息
  - `PUT /api/user/{userId}/roles` - 更新用户角色

- **角色管理**
  - `GET /api/roles` - 获取角色列表
  - `POST /api/roles` - 创建角色
  - `PUT /api/roles/{roleId}` - 更新角色

## 页面说明

### 登录页面 (`/login`)
- 用户登录界面
- 支持用户名/密码登录
- 表单验证

### 注册页面 (`/register`)
- 新用户注册界面
- 完整的注册表单
- 角色选择

### 仪表盘 (`/`)
- 系统概览
- 统计数据展示
- 最近活动

### 用户管理 (`/users`)
- 用户列表展示
- 用户增删改查
- 角色分配
- 搜索和筛选

### 角色管理 (`/roles`)
- 角色列表展示
- 角色增删改查
- 权限配置
- 搜索和筛选

### 个人资料 (`/profile`)
- 个人信息展示
- 资料编辑
- 密码修改

## 状态管理

使用 Pinia 进行状态管理，主要包含：

- **authStore** - 认证状态管理
  - 用户登录状态
  - Token 管理
  - 用户信息

## 路由配置

- 路由守卫确保需要认证的页面
- 自动重定向到登录页面
- 支持路由懒加载

## 样式设计

- 使用 Element Plus 组件库
- 响应式布局
- 现代化设计风格
- 中文本地化

## 开发规范

### 代码规范

- 使用 TypeScript 进行类型检查
- 遵循 Vue 3 Composition API 规范
- 使用 ESLint 进行代码检查

### 组件规范

- 使用 `<script setup>` 语法
- 组件命名使用 PascalCase
- 文件命名使用 kebab-case

### API 调用规范

- 统一使用 `@/api` 中的接口函数
- 错误处理统一在 axios 拦截器中处理
- 使用 TypeScript 接口定义请求/响应类型

## 部署说明

### 开发环境

1. 确保后端服务已启动（默认端口 5000）
2. 启动前端开发服务器：`npm run dev`
3. 访问 http://localhost:3000

### 生产环境

1. 构建项目：`npm run build`
2. 将 `dist` 目录部署到 Web 服务器
3. 配置后端 API 地址

## 常见问题

### 1. 依赖安装失败

```bash
# 清除缓存后重新安装
npm cache clean --force
rm -rf node_modules package-lock.json
npm install
```

### 2. 开发服务器启动失败

检查端口 3000 是否被占用，可以在 `vite.config.ts` 中修改端口配置。

### 3. API 请求失败

确保后端服务已启动，并检查 `vite.config.ts` 中的代理配置。

## 贡献指南

1. Fork 项目
2. 创建功能分支
3. 提交更改
4. 推送到分支
5. 创建 Pull Request

## 许可证

MIT License 