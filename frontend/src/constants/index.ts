// 用户状态常量
export const USER_STATUS = {
  DISABLED: 0,
  ENABLED: 1
} as const

// 性别常量
export const GENDER = {
  MALE: '男',
  FEMALE: '女'
} as const

// 权限常量
export const PERMISSIONS = {
  // 系统管理
  SYSTEM_ADMIN: 'SystemAdmin',
  SYSTEM_MONITOR: 'SystemMonitor',
  
  // 用户管理
  USER_VIEW: 'UserView',
  USER_CREATE: 'UserCreate',
  USER_EDIT: 'UserEdit',
  USER_DELETE: 'UserDelete',
  USER_MANAGEMENT: 'UserManagement',
  USER_ROLE_ASSIGN: 'UserRoleAssign',
  USER_RESET_PASSWORD: 'UserResetPassword',
  
  // 角色管理
  ROLE_VIEW: 'RoleView',
  ROLE_CREATE: 'RoleCreate',
  ROLE_EDIT: 'RoleEdit',
  ROLE_DELETE: 'RoleDelete',
  ROLE_MANAGEMENT: 'RoleManagement',
  
  // 组织架构管理
  ORG_VIEW: 'OrganizationUnitView',
  ORG_CREATE: 'OrganizationUnitCreate',
  ORG_EDIT: 'OrganizationUnitEdit',
  ORG_DELETE: 'OrganizationUnitDelete',
  ORG_MANAGEMENT: 'OrganizationUnitManagement',
  
  // 日志管理
  LOG_VIEW: 'LogView'
} as const

// 路由路径常量
export const ROUTES = {
  HOME: '/',
  LOGIN: '/login',
  REGISTER: '/register',
  USERS: '/users',
  ROLES: '/roles',
  ORGANIZATION_UNITS: '/organization-units',
  LOGS: '/logs',
  PROFILE: '/profile'
} as const

// 本地存储键常量
export const STORAGE_KEYS = {
  TOKEN: 'token',
  REFRESH_TOKEN: 'refreshToken',
  USER: 'user',
  USER_PERMISSIONS: 'userPermissions',
  THEME: 'theme',
  LANGUAGE: 'language'
} as const

// HTTP状态码常量
export const HTTP_STATUS = {
  OK: 200,
  CREATED: 201,
  NO_CONTENT: 204,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  CONFLICT: 409,
  INTERNAL_SERVER_ERROR: 500
} as const

// Element Plus 组件尺寸常量
export const COMPONENT_SIZE = {
  LARGE: 'large',
  DEFAULT: 'default',
  SMALL: 'small'
} as const

// Element Plus 类型常量
export const MESSAGE_TYPE = {
  SUCCESS: 'success',
  WARNING: 'warning',
  INFO: 'info',
  ERROR: 'error'
} as const

// 分页默认配置
export const PAGINATION_DEFAULTS = {
  PAGE_SIZE: 10,
  PAGE_SIZES: [10, 20, 50, 100],
  LAYOUT: 'total, sizes, prev, pager, next, jumper'
} as const

// 文件上传相关常量
export const UPLOAD = {
  MAX_FILE_SIZE: 10 * 1024 * 1024, // 10MB
  ALLOWED_EXTENSIONS: ['.xlsx', '.xls'],
  CHUNK_SIZE: 1024 * 1024 // 1MB
} as const

// 正则表达式常量
export const REGEX = {
  EMAIL: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
  PHONE: /^1[3-9]\d{9}$/,
  USERNAME: /^[a-zA-Z0-9_]+$/,
  PASSWORD: /^(?=.*[a-zA-Z])(?=.*\d).+$/
} as const

// 日期格式常量
export const DATE_FORMAT = {
  DATE: 'YYYY-MM-DD',
  DATETIME: 'YYYY-MM-DD HH:mm:ss',
  TIME: 'HH:mm:ss',
  MONTH: 'YYYY-MM',
  YEAR: 'YYYY'
} as const
