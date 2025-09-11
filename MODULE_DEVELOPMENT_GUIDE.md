# 模块开发规范指南

## 概述

本文档总结了开发新模块的标准流程和最佳实践，基于Ncp.CleanDDD架构模式（FastEndpoints + DDD + NetCorePal），确保代码质量和一致性。该框架在Clean Architecture基础上进行了优化，提供了更好的开发体验和性能。

## 1. 后端开发规范

### 1.1 文件组织结构
```
src/Ncp.CleanDDD.Domain/AggregatesModel/{ModuleName}Aggregate/
└── {ModuleName}.cs                    # 聚合根 (包含强类型ID和所有子实体)

src/Ncp.CleanDDD.Domain/DomainEvents/{ModuleName}Events/
├── {ModuleName}InfoChangedDomainEvent.cs
├── {ModuleName}StatusChangedDomainEvent.cs
└── {ModuleName}CreatedDomainEvent.cs

src/Ncp.CleanDDD.Infrastructure/EntityConfigurations/
└── {ModuleName}EntityTypeConfiguration.cs

src/Ncp.CleanDDD.Infrastructure/Repositories/
└── {ModuleName}Repository.cs          # 使用NetCorePal仓储接口统一实现

src/Ncp.CleanDDD.Web/Application/Commands/{ModuleName}Commands/
├── Create{ModuleName}Command.cs       # 创建命令 (record + 处理器)
├── Update{ModuleName}Command.cs       # 更新命令
├── Delete{ModuleName}Command.cs       # 删除命令
└── Update{ModuleName}StatusCommand.cs # 状态更新命令

src/Ncp.CleanDDD.Web/Application/Queries/
└── {ModuleName}Query.cs               # 查询服务 (包含所有DTO和查询方法)

src/Ncp.CleanDDD.Web/Endpoints/{ModuleName}Endpoints/
├── Get{ModuleName}.cs                 # 单个查询端点
├── GetAll{ModuleName}s.cs             # 列表查询端点
├── Create{ModuleName}.cs              # 创建端点 (包含Summary)
├── Update{ModuleName}.cs              # 更新端点
└── Delete{ModuleName}.cs              # 删除端点

src/Ncp.CleanDDD.Web/AppPermissions/
└── {ModuleName}Permissions.cs         # 权限定义

frontend/src/api/
└── {moduleName}.ts                    # API服务

frontend/src/views/
└── {ModuleName}s.vue                  # 管理页面 (复数命名)

frontend/src/components/
├── {ModuleName}FormDialog.vue         # 表单对话框
├── {ModuleName}ImportDialog.vue       # 导入对话框
└── {ModuleName}RoleAssignDialog.vue   # 角色分配对话框 (如适用)
```

### 1.2 核心规范

#### 1.2.1 强类型ID规范
- **GUID类型ID**：`public partial record {ModuleName}Id : IGuidStronglyTypedId;`
- **INT64类型ID**：`public partial record {ModuleName}Id : IInt64StronglyTypedId;`
- 强类型ID直接写在聚合根类文件顶部，不单独创建{ModuleName}Id.cs文件
- ID类型选择：新模块推荐使用GUID类型，需要与外部系统集成或性能要求极高时可使用Int64

#### 1.2.2 聚合根规范
```csharp
public partial record {ModuleName}Id : IGuidStronglyTypedId; // 或 IInt64StronglyTypedId

public class {ModuleName} : Entity<{ModuleName}Id>, IAggregateRoot
{
    // 无参构造函数 (EF Core)
    protected {ModuleName}() { }
    
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset CreatedAt { get; init; }
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);
    
    // 软删除字段 (NetCorePal值对象)
    public Deleted IsDeleted { get; private set; } = new Deleted(false);
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);
    
    // 导航属性 (如果有关联实体)
    public virtual ICollection<{RelatedEntity}> {RelatedEntities} { get; } = [];
    
    public {ModuleName}(string name, string description)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }
    
    public void Update{ModuleName}Info(string name, string description)
    {
        Name = name;
        Description = description;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
    
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = new DeletedTime(DateTimeOffset.UtcNow);
    }
    
    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
```

#### 1.2.3 命令设计规范（Record模式 + 处理器）
```csharp
// ✅ 推荐：使用record类型，包含所有必要参数
public record Create{ModuleName}Command(string Name, string Description, bool IsActive) : ICommand<{ModuleName}Id>;
public record Update{ModuleName}Command({ModuleName}Id Id, string Name, string Description) : ICommand;
public record Delete{ModuleName}Command({ModuleName}Id Id) : ICommand;
public record Update{ModuleName}StatusCommand({ModuleName}Id Id, bool IsActive) : ICommand;

// 命令处理器直接在同一文件中实现
public class Create{ModuleName}CommandHandler(I{ModuleName}Repository {moduleName}Repository) 
    : ICommandHandler<Create{ModuleName}Command, {ModuleName}Id>
{
    public async Task<{ModuleName}Id> Handle(Create{ModuleName}Command request, CancellationToken cancellationToken)
    {
        var {moduleName} = new {ModuleName}(request.Name, request.Description);
        if (!request.IsActive)
        {
            {moduleName}.SetActive(false);
        }
        
        await {moduleName}Repository.AddAsync({moduleName}, cancellationToken);
        return {moduleName}.Id;
    }
}
```

#### 1.2.4 命令验证器规范
```csharp
public class Create{ModuleName}CommandValidator : AbstractValidator<Create{ModuleName}Command>
{
    public Create{ModuleName}CommandValidator({ModuleName}Query {moduleName}Query)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("{模块名称}不能为空")
            .MaximumLength(100).WithMessage("{模块名称}长度不能超过100个字符")
            .MustAsync(async (name, ct) => !await {moduleName}Query.Does{ModuleName}NameExist(name, ct))
            .WithMessage(x => $"{模块名称}已存在，Name={x.Name}");
    }
}
```

#### 1.2.5 查询服务规范
```csharp
/// <summary>
/// {模块名}信息查询DTO
/// </summary>
public record {ModuleName}InfoQueryDto({ModuleName}Id Id, string Name, string Description, 
    bool IsActive, DateTimeOffset CreatedAt);

/// <summary>
/// {模块名}查询输入参数
/// </summary>
public class {ModuleName}QueryInput : PageRequest
{
    public string? Keyword { get; set; }
    public bool? IsActive { get; set; }
}

public class {ModuleName}Query(ApplicationDbContext applicationDbContext, IMemoryCache memoryCache) : IQuery
{
    private DbSet<{ModuleName}> {ModuleName}Set { get; } = applicationDbContext.{ModuleName}s;
    
    public async Task<bool> Does{ModuleName}Exist(string name, CancellationToken cancellationToken)
    {
        return await {ModuleName}Set.AsNoTracking()
            .AnyAsync(x => x.Name == name, cancellationToken);
    }
    
    public async Task<bool> Does{ModuleName}Exist({ModuleName}Id id, CancellationToken cancellationToken)
    {
        return await {ModuleName}Set.AsNoTracking()
            .AnyAsync(x => x.Id == id, cancellationToken);
    }
    
    public async Task<{ModuleName}InfoQueryDto?> Get{ModuleName}ByIdAsync({ModuleName}Id id, CancellationToken cancellationToken)
    {
        return await {ModuleName}Set.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new {ModuleName}InfoQueryDto(x.Id, x.Name, x.Description, x.IsActive, x.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
    
    public async Task<PagedData<{ModuleName}InfoQueryDto>> GetAll{ModuleName}sAsync({ModuleName}QueryInput input, CancellationToken cancellationToken)
    {
        var queryable = {ModuleName}Set.AsNoTracking();
        
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            queryable = queryable.Where(x => x.Name.Contains(input.Keyword!) || x.Description.Contains(input.Keyword!));
        }
        
        if (input.IsActive.HasValue)
        {
            queryable = queryable.Where(x => x.IsActive == input.IsActive);
        }
        
        return await queryable
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new {ModuleName}InfoQueryDto(x.Id, x.Name, x.Description, x.IsActive, x.CreatedAt))
            .ToPagedDataAsync(input, cancellationToken);
    }
}
```

#### 1.2.6 Endpoint端点规范（含Summary）
```csharp
/// <summary>
/// 创建{模块名}的请求模型
/// </summary>
public record Create{ModuleName}Request(string Name, string Description, bool IsActive = true);

/// <summary>
/// 创建{模块名}的响应模型
/// </summary>
public record Create{ModuleName}Response({ModuleName}Id Id, string Name, string Description);

/// <summary>
/// 创建{模块名}的API端点
/// </summary>
[Tags("{ModuleName}s")]
public class Create{ModuleName}Endpoint(IMediator mediator) : Endpoint<Create{ModuleName}Request, ResponseData<Create{ModuleName}Response>>
{
    public override void Configure()
    {
        Post("/api/{moduleName}s");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.{ModuleName}Create);
    }

    public override async Task HandleAsync(Create{ModuleName}Request req, CancellationToken ct)
    {
        var cmd = new Create{ModuleName}Command(req.Name, req.Description, req.IsActive);
        var result = await mediator.Send(cmd, ct);
        
        var response = new Create{ModuleName}Response(result, req.Name, req.Description);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建{模块名}端点的API文档配置
/// </summary>
public class Create{ModuleName}Summary : Summary<Create{ModuleName}Endpoint, Create{ModuleName}Request>
{
    public Create{ModuleName}Summary()
    {
        Summary = "创建{模块名}";
        Description = "在系统中创建新的{模块名}";
        Response<Create{ModuleName}Response>(200, "{模块名}创建成功");
        ExampleRequest = new Create{ModuleName}Request("示例名称", "示例描述", true);
        Responses[200] = "成功创建{模块名}";
        Responses[400] = "请求参数无效";
        Responses[401] = "未授权访问";
        Responses[403] = "权限不足";
        Responses[409] = "{模块名}名称已存在";
    }
}
```

#### 1.2.7 仓储规范（NetCorePal自动实现）
```csharp
// 仓储接口定义
public interface I{ModuleName}Repository : IRepository<{ModuleName}, {ModuleName}Id>
{
    // 基础CRUD操作已由NetCorePal自动提供，只需定义特殊查询方法
    Task<{ModuleName}?> GetBy{PropertyName}Async(string {propertyName}, CancellationToken cancellationToken = default);
    Task<bool> ExistsBy{PropertyName}Async(string {propertyName}, CancellationToken cancellationToken = default);
}

// 仓储实现（简化版）
public class {ModuleName}Repository(ApplicationDbContext dbContext) : Repository<{ModuleName}, {ModuleName}Id>(dbContext), I{ModuleName}Repository
{
    public async Task<{ModuleName}?> GetBy{PropertyName}Async(string {propertyName}, CancellationToken cancellationToken = default)
    {
        return await Set.FirstOrDefaultAsync(x => x.{PropertyName} == {propertyName}, cancellationToken);
    }
    
    public async Task<bool> ExistsBy{PropertyName}Async(string {propertyName}, CancellationToken cancellationToken = default)
    {
        return await Set.AnyAsync(x => x.{PropertyName} == {propertyName}, cancellationToken);
    }
}
```

### 1.3 权限定义规范
```csharp
// src/Ncp.CleanDDD.Web/AppPermissions/{ModuleName}Permissions.cs
public static class {ModuleName}Permissions
{
    public const string {ModuleName}View = "{ModuleName}View";
    public const string {ModuleName}Create = "{ModuleName}Create";
    public const string {ModuleName}Edit = "{ModuleName}Edit";
    public const string {ModuleName}Delete = "{ModuleName}Delete";
    public const string {ModuleName}Export = "{ModuleName}Export";
    public const string {ModuleName}Import = "{ModuleName}Import";
}
```

### 1.4 Entity Framework配置规范
```csharp
public class {ModuleName}EntityTypeConfiguration : IEntityTypeConfiguration<{ModuleName}>
{
    public void Configure(EntityTypeBuilder<{ModuleName}> builder)
    {
        builder.ToTable("{ModuleName}");
        
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.IsDeleted);
        
        // 软删除查询过滤器
        builder.HasQueryFilter(x => !x.IsDeleted.Value);
    }
}
```

### 1.5 代码片段工具

可以使用项目提供的代码片段快速生成代码：
- **NetCorePal仓储**: 使用 `ncprepo` 代码片段
- **Endpoint Summary**: 使用 `epsum` 代码片段  
- **Command Handler**: 使用 `cmd` 代码片段
- **Entity Configuration**: 使用 `efconfig` 代码片段

## 2. 前端开发规范

### 2.1 API服务规范

#### 2.1.1 API服务文件结构
```typescript
// frontend/src/api/{moduleName}.ts
import api from './index'
import type { BaseResponse, PaginationResponse } from '@/types'

// ==================== 类型定义 ====================

export interface {ModuleName} {
  id: string
  name: string
  description: string
  isActive: boolean
  createdAt: string
}

export interface Create{ModuleName}Request {
  name: string
  description: string
  isActive?: boolean
}

export interface Update{ModuleName}Request {
  id: string
  name: string
  description: string
}

export interface {ModuleName}QueryParams {
  pageIndex?: number
  pageSize?: number
  keyword?: string
  isActive?: boolean
}

// ==================== API 方法 ====================

export const {moduleName}Api = {
  /**
   * 获取{模块名}列表
   */
  getAll: (params: {ModuleName}QueryParams) => 
    api.get<PaginationResponse<{ModuleName}>>('/api/{moduleName}s', { params }),

  /**
   * 获取单个{模块名}
   */
  getById: (id: string) => 
    api.get<{ModuleName}>(`/api/{moduleName}s/${id}`),

  /**
   * 创建{模块名}
   */
  create: (data: Create{ModuleName}Request) => 
    api.post<{ModuleName}>('/api/{moduleName}s', data),

  /**
   * 更新{模块名}
   */
  update: (data: Update{ModuleName}Request) => 
    api.put('/api/{moduleName}s', data),

  /**
   * 删除{模块名}
   */
  delete: (id: string) => 
    api.delete(`/api/{moduleName}s/${id}`),

  /**
   * 批量删除{模块名}
   */
  batchDelete: (ids: string[]) => 
    api.delete('/api/{moduleName}s/batch', { data: { ids } })
}
```

### 2.2 页面布局模板

#### 2.2.1 现代化管理页面结构
```vue
<template>
  <div class="management-page">
    <!-- 页面头部 -->
    <div class="page-header">
      <div class="header-content">
        <div class="title-section">
          <h1>{模块名}管理</h1>
          <p class="subtitle">管理系统中的所有{模块名}</p>
        </div>
        <el-button 
          v-permission="[PERMISSIONS.{MODULE_NAME}_CREATE]"
          type="primary" 
          size="large"
          class="create-btn"
          @click="showCreateDialog"
          icon="Plus"
        >
          新建{模块名}
        </el-button>
      </div>
    </div>

    <div class="main-content">
      <!-- 搜索栏 -->
      <div class="search-section">
        <div class="section-header">
          <h2>
            <el-icon class="section-icon"><Search /></el-icon>
            搜索筛选
          </h2>
        </div>
        <div class="search-wrapper">
          <el-form :inline="true" :model="searchParams" class="search-form">
            <el-form-item label="搜索">
              <el-input
                v-model="searchParams.keyword"
                placeholder="请输入{模块名}名称"
                clearable
                class="search-input"
                :prefix-icon="Search"
                @keyup.enter="() => handleSearch()"
              />
            </el-form-item>
            <el-form-item label="状态">
              <el-select 
                v-model="searchParams.isActive" 
                placeholder="请选择状态" 
                clearable 
                class="status-select"
              >
                <el-option label="启用" :value="true" />
                <el-option label="禁用" :value="false" />
              </el-select>
            </el-form-item>
            <el-form-item>
              <div class="action-buttons">
                <el-button type="primary" class="search-btn" @click="() => handleSearch()" icon="Search">
                  搜索 
                </el-button>
                <el-button class="reset-btn" @click="handleReset" icon="Refresh">
                  重置
                </el-button>
              </div>
            </el-form-item>
          </el-form>
        </div>
      </div>

      <!-- 数据表格 -->
      <div class="table-section">
        <div class="section-header">
          <h2>
            <el-icon class="section-icon"><List /></el-icon>
            {模块名}列表
          </h2>
          <div class="header-actions">
            <el-tag type="info" class="count-tag">
              共 {{ pagination.total }} 个{模块名}
            </el-tag>
          </div>
        </div>
        
        <div class="table-wrapper">
          <el-table
            v-loading="loading"
            :data="data"
            class="data-table"
            stripe
            @selection-change="handleSelectionChange"
          >
            <el-table-column type="selection" width="55" />
            
            <el-table-column label="{模块名}信息" min-width="200">
              <template #default="{ row }">
                <div class="info-cell">
                  <div class="info-main">
                    <span class="name">{{ row.name }}</span>
                    <span class="description">{{ row.description }}</span>
                  </div>
                </div>
              </template>
            </el-table-column>

            <el-table-column label="状态" width="100" align="center">
              <template #default="{ row }">
                <el-tag :type="row.isActive ? 'success' : 'danger'" size="small">
                  {{ row.isActive ? '启用' : '禁用' }}
                </el-tag>
              </template>
            </el-table-column>

            <el-table-column label="创建时间" width="180" align="center">
              <template #default="{ row }">
                <div class="time-cell">
                  <el-icon size="14" color="#94a3b8"><Timer /></el-icon>
                  <span>{{ formatDate(row.createdAt) }}</span>
                </div>
              </template>
            </el-table-column>

            <el-table-column label="操作" width="200" fixed="right">
              <template #default="{ row }">
                <div class="action-buttons">
                  <el-button 
                    v-permission="[PERMISSIONS.{MODULE_NAME}_EDIT]"
                    size="small" 
                    type="primary" 
                    class="action-btn"
                    @click="handleEdit(row)"
                    icon="Edit"
                  >
                    编辑
                  </el-button>
                  <el-button 
                    v-permission="[PERMISSIONS.{MODULE_NAME}_DELETE]"
                    size="small" 
                    type="danger" 
                    class="action-btn"
                    @click="handleDelete(row)"
                    icon="Delete"
                  >
                    删除
                  </el-button>
                </div>
              </template>
            </el-table-column>
          </el-table>

          <!-- 分页 -->
          <div class="pagination-wrapper">
            <el-pagination
              v-model:current-page="pagination.pageIndex"
              v-model:page-size="pagination.pageSize"
              :total="pagination.total"
              :page-sizes="[10, 20, 50, 100]"
              layout="total, sizes, prev, pager, next, jumper"
              @update:page-size="handlePageSizeChange"
              @update:current-page="handlePageCurrentChange"
            />
          </div>
        </div>
      </div>
    </div>

    <!-- 表单对话框 -->
    <{ModuleName}FormDialog
      v-model="dialogVisible"
      :form-data="currentItem"
      :mode="dialogMode"
      @success="handleDialogSuccess"
    />
  </div>
</template>
```

#### 2.2.2 Vue组合式API脚本部分
```typescript
<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { Search, Edit, Delete, Plus, Timer, List, Refresh } from '@element-plus/icons-vue'
import { {moduleName}Api, type {ModuleName}, type {ModuleName}QueryParams } from '@/api/{moduleName}'
import { useTable } from '@/composables/useTable'
import { useConfirm } from '@/composables/useConfirm'
import { formatDate } from '@/utils'
import { PERMISSIONS } from '@/constants'
import {ModuleName}FormDialog from '@/components/{ModuleName}FormDialog.vue'

// ==================== 响应式数据 ====================
const dialogVisible = ref(false)
const dialogMode = ref<'create' | 'edit'>('create')
const currentItem = ref<Partial<{ModuleName}>>({})

const searchParams = reactive<{ModuleName}QueryParams>({
  keyword: '',
  isActive: undefined
})

// ==================== 表格管理 ====================
const {
  data,
  loading,
  pagination,
  selectedRows,
  handleSearch,
  handleReset,
  handlePageSizeChange,
  handlePageCurrentChange,
  handleSelectionChange,
  refresh
} = useTable(
  (params: {ModuleName}QueryParams) => {moduleName}Api.getAll(params),
  10 // 初始页面大小
)

// ==================== 事件处理 ====================
const showCreateDialog = () => {
  currentItem.value = {}
  dialogMode.value = 'create'
  dialogVisible.value = true
}

const handleEdit = (item: {ModuleName}) => {
  currentItem.value = { ...item }
  dialogMode.value = 'edit'
  dialogVisible.value = true
}

const handleDelete = async (item: {ModuleName}) => {
  await useConfirm(`确定要删除{模块名}"${item.name}"吗？`, '删除确认', 'warning')
  
  try {
    await {moduleName}Api.delete(item.id)
    ElMessage.success('删除成功')
    refresh()
  } catch (error) {
    ElMessage.error('删除失败')
  }
}

const handleDialogSuccess = () => {
  dialogVisible.value = false
  refresh()
}

// ==================== 搜索处理 ====================
const handleSearchClick = () => {
  handleSearch(searchParams)
}

const handleResetClick = () => {
  Object.assign(searchParams, {
    keyword: '',
    isActive: undefined
  })
  handleReset()
}

// ==================== 生命周期 ====================
onMounted(() => {
  refresh()
})
</script>
```

### 2.3 组件化规范

#### 2.3.1 表单对话框组件
```vue
<!-- {ModuleName}FormDialog.vue -->
<template>
  <el-dialog
    v-model="visible"
    :title="mode === 'create' ? '新建{模块名}' : '编辑{模块名}'"
    width="600px"
    destroy-on-close
    @close="handleClose"
  >
    <el-form
      ref="formRef"
      :model="form"
      :rules="rules"
      label-width="100px"
      label-position="left"
    >
      <el-form-item label="{模块名}名称" prop="name" required>
        <el-input
          v-model="form.name"
          placeholder="请输入{模块名}名称"
          maxlength="100"
          show-word-limit
        />
      </el-form-item>
      
      <el-form-item label="描述" prop="description">
        <el-input
          v-model="form.description"
          type="textarea"
          placeholder="请输入描述"
          :rows="3"
          maxlength="500"
          show-word-limit
        />
      </el-form-item>
      
      <el-form-item label="状态" prop="isActive">
        <el-switch
          v-model="form.isActive"
          active-text="启用"
          inactive-text="禁用"
        />
      </el-form-item>
    </el-form>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="handleClose">取消</el-button>
        <el-button type="primary" @click="handleSubmit" :loading="loading">
          {{ mode === 'create' ? '创建' : '更新' }}
        </el-button>
      </div>
    </template>
  </el-dialog>
</template>

<script setup lang="ts">
import { ref, reactive, watch, computed } from 'vue'
import { ElMessage, type FormInstance, type FormRules } from 'element-plus'
import { {moduleName}Api, type {ModuleName} } from '@/api/{moduleName}'

// Props & Emits
interface Props {
  modelValue: boolean
  formData?: Partial<{ModuleName}>
  mode: 'create' | 'edit'
}

const props = withDefaults(defineProps<Props>(), {
  formData: () => ({})
})

const emit = defineEmits<{
  'update:modelValue': [value: boolean]
  success: []
}>()

// 响应式数据
const formRef = ref<FormInstance>()
const loading = ref(false)

const visible = computed({
  get: () => props.modelValue,
  set: (value) => emit('update:modelValue', value)
})

const form = reactive({
  name: '',
  description: '',
  isActive: true
})

const rules: FormRules = {
  name: [
    { required: true, message: '请输入{模块名}名称', trigger: 'blur' },
    { min: 2, max: 100, message: '长度在 2 到 100 个字符', trigger: 'blur' }
  ]
}

// 监听表单数据变化
watch(() => props.formData, (data) => {
  if (data) {
    Object.assign(form, {
      name: data.name || '',
      description: data.description || '',
      isActive: data.isActive ?? true
    })
  }
}, { immediate: true, deep: true })

// 事件处理
const handleSubmit = async () => {
  if (!formRef.value) return
  
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  
  loading.value = true
  try {
    if (props.mode === 'create') {
      await {moduleName}Api.create(form)
      ElMessage.success('创建成功')
    } else {
      await {moduleName}Api.update({
        id: props.formData?.id!,
        ...form
      })
      ElMessage.success('更新成功')
    }
    emit('success')
  } catch (error) {
    ElMessage.error(props.mode === 'create' ? '创建失败' : '更新失败')
  } finally {
    loading.value = false
  }
}

const handleClose = () => {
  formRef.value?.resetFields()
  emit('update:modelValue', false)
}
</script>
```

### 2.4 样式规范

#### 2.4.1 样式组织原则
- **全局样式优先**：基础管理页面样式统一使用 `frontend/src/styles/common-page.css`
- **特有样式分离**：页面特有功能样式写在组件的 `<style scoped>` 中
- **避免重复**：不要重复定义全局样式中已有的样式
- **命名规范**：使用语义化的CSS类名，避免样式冲突

#### 2.4.2 全局样式使用规范
基础管理页面应使用以下全局样式类：

```scss
// 页面结构类
.management-page          // 页面容器
.page-header             // 页面头部
.main-content            // 主要内容区域
.search-section          // 搜索区域
.table-section           // 表格区域

// 通用组件类
.section-header          // 区域头部
.search-wrapper          // 搜索表单容器
.table-wrapper           // 表格容器
.data-table              // 数据表格
.entity-cell             // 实体信息单元格
.entity-avatar           // 实体头像
.entity-info             // 实体信息
.entity-name             // 实体名称
.entity-description      // 实体描述
.time-cell               // 时间单元格
.table-actions           // 表格操作按钮组
.table-action-btn        // 表格操作按钮
.pagination-wrapper      // 分页容器

// 对话框类
.management-dialog       // 管理对话框
.management-form         // 管理表单
.dialog-footer           // 对话框底部
.submit-btn              // 提交按钮

// 树组件类
.tree-wrapper            // 树容器
.tree-header             // 树头部
.tree-container          // 树滚动容器
.tree                    // 树组件
.tree-node               // 树节点
.tree-content            // 树节点内容
.tree-icon               // 树图标
.tree-name               // 树节点名称

// 通用标签类
.status-tag              // 状态标签
.count-tag               // 计数标签
.permission-count-tag    // 权限计数标签
```

#### 2.4.3 特有样式编写规范
当页面需要特有功能时，应在组件内编写scoped样式：

```vue
<style scoped>
/* ===== {页面名}页面特有样式 ===== */

/* 特有功能样式 - 如导入对话框 */
.import-dialog-content {
  max-height: 70vh;
  overflow-y: auto;
}

/* 特有表单样式 */
.special-form-item {
  margin-bottom: 24px;
}

/* 特有响应式设计 */
@media (max-width: 768px) {
  .special-mobile-style {
    display: block;
  }
}
</style>
```

#### 2.4.4 样式文件引用规范
```vue
<template>
  <!-- 使用全局样式类 -->
  <div class="management-page">
    <div class="page-header">
      <!-- 页面头部内容 -->
    </div>
    
    <!-- 使用特有样式类 -->
    <div class="special-feature-section">
      <!-- 特有功能内容 -->
    </div>
  </div>
</template>

<style scoped>
/* 只定义页面特有的样式，不重复全局样式 */
.special-feature-section {
  /* 特有样式定义 */
}
</style>
```

### 2.5 最佳实践

#### 2.5.1 开发规范
- **组合式API优先**：使用Vue 3 Composition API，提高代码复用性
- **TypeScript强类型**：所有组件和API调用使用TypeScript类型定义
- **Composables复用**：使用useTable、useConfirm等组合式函数提高开发效率
- **响应式设计**：所有页面必须支持移动端访问
- **样式复用优先**：优先使用全局样式，避免重复定义

#### 2.5.2 性能优化
- **异步加载**：使用动态import实现路由懒加载
- **图片优化**：使用WebP格式，设置合适的尺寸
- **API缓存**：合理使用缓存策略，减少网络请求
- **分页合理**：列表页面默认20条，支持10/20/50/100切换
- **样式优化**：减少CSS重复，使用全局样式提高渲染性能

#### 2.5.3 用户体验
- **加载状态**：所有异步操作显示loading状态
- **错误提示**：友好的错误信息和重试机制
- **操作确认**：危险操作（删除等）必须有确认提示
- **表单验证**：实时验证，明确的错误提示
- **一致性设计**：保持页面布局和交互的一致性

## 3. 开发流程

### 3.1 标准开发阶段
1. **需求分析与设计** - 明确业务需求，设计领域模型
2. **领域层开发** - 创建聚合根、值对象、领域事件
3. **基础设施层实现** - Entity配置、仓储实现
4. **应用层开发** - 命令、查询、处理器实现
5. **Web层API** - Endpoints端点和权限配置
6. **前端界面开发** - API服务、页面组件、样式
7. **测试与调试** - 单元测试、集成测试、端到端测试
8. **文档更新** - API文档、使用说明

### 3.2 开发检查清单

#### 3.2.1 后端开发检查项
- [ ] ✅ 强类型ID正确定义（GUID或Int64）
- [ ] ✅ 聚合根包含必要的业务方法和属性
- [ ] ✅ 软删除字段和更新时间字段已添加
- [ ] ✅ Entity Configuration配置完整（表名、索引、约束）
- [ ] ✅ 仓储接口和实现符合NetCorePal规范
- [ ] ✅ 命令使用record类型，包含验证器
- [ ] ✅ 查询服务包含所有必要的DTO和查询方法
- [ ] ✅ Endpoint配置正确的路由、认证和权限
- [ ] ✅ API文档Summary配置完整
- [ ] ✅ 权限常量已定义并在AppPermissions中注册

#### 3.2.2 前端开发检查项
- [ ] ✅ API服务文件类型定义完整
- [ ] ✅ 页面使用现代化布局和响应式设计
- [ ] ✅ 使用组合式API和TypeScript
- [ ] ✅ 表格支持搜索、筛选、分页功能
- [ ] ✅ 表单对话框组件化，支持创建和编辑模式
- [ ] ✅ 权限指令正确使用
- [ ] ✅ 错误处理和用户反馈完善
- [ ] ✅ 移动端适配良好
- [ ] ✅ 优先使用全局样式`common-page.css`，避免重复定义
- [ ] ✅ 特有功能样式使用scoped，命名语义化
- [ ] ✅ 遵循样式组织原则，保持代码简洁

### 3.3 框架优势总结

#### 3.3.1 Ncp.CleanDDD框架特点
- **NetCorePal集成**：强类型ID、值对象、仓储模式等开箱即用
- **FastEndpoints**：轻量级、高性能的API端点开发
- **DDD架构**：清晰的领域驱动设计分层
- **现代化前端**：Vue 3 + TypeScript + Element Plus
- **开发体验优化**：代码片段、自动化工具、规范统一

#### 3.3.2 相比传统MVC的改进
- **更好的性能**：FastEndpoints比Controller性能更优
- **类型安全**：强类型ID避免类型混淆
- **代码简洁**：record类型减少样板代码
- **组合式API**：Vue 3组合式API提高代码复用性
- **现代化UI**：响应式设计，移动端友好

### 3.4 注意事项

#### 3.4.1 开发注意点
- **命名一致性**：前后端API路径、权限名称保持一致
- **错误处理**：后端返回标准格式，前端统一处理
- **权限控制**：所有敏感操作必须添加权限验证
- **数据验证**：前后端双重验证，确保数据安全
- **性能考虑**：大量数据使用分页，避免一次性加载过多

#### 3.4.2 常见问题
- **强类型ID序列化**：确保JSON转换器正确配置
- **软删除查询**：QueryFilter自动过滤已删除数据
- **跨域问题**：开发环境正确配置CORS策略
- **权限缓存**：权限变更后及时清除相关缓存
- **数据库迁移**：字段变更需要创建新的Migration
- **样式冲突**：避免在组件中重复定义全局样式，优先使用`common-page.css`
- **响应式问题**：确保特有样式也包含对应的移动端适配

## 4. 总结

本规范基于Ncp.CleanDDD框架的最新优化，相比原版本有以下主要改进：

1. **架构优化**：采用FastEndpoints + NetCorePal + DDD的组合
2. **开发效率**：record类型、组合式API、自动化工具
3. **代码质量**：TypeScript全覆盖、强类型约束
4. **用户体验**：现代化UI设计、响应式布局
5. **可维护性**：清晰的分层架构、统一的代码规范

开发新模块时，请严格按照此规范执行，确保代码质量和项目一致性。遇到问题时，参考现有User、Role、OrganizationUnit等模块的实现。 