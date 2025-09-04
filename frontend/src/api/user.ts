import api from './index'

// 用户登录
export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  refreshToken: string
  userId: string
  name: string
  email: string
  permissions: string
}

export const login = (data: LoginRequest) => {
  return api.post<LoginResponse>('/user/login', data)
}

// 用户注册
export interface RegisterRequest {
  name: string
  email: string
  password: string
  confirmPassword: string
  phone: string
  realName: string
  status: number
  gender: string
  age: number
  roleIds: string[]
  organizationUnitId: number
  organizationUnitName: string
  birthDate: string
  userId: string
}

export interface RegisterResponse {
  userId: string
  name: string
  email: string
}

export const register = (data: RegisterRequest) => {
  return api.post<RegisterResponse>('/user/register', data)
}

// 获取用户资料
export interface UserProfileResponse {
  userId: string
  name: string
  phone: string
  roles: string[]
  realName: string
  status: number
  email: string
  createdAt: string
  birthDate: string
  idType: string
  organizationUnitId: number
  organizationUnitName: string
  gender: string
}

export const getUserProfile = (userId: string) => {
  return api.get<UserProfileResponse>(`/user/profile/${userId}`)
}

// 更新用户信息
export interface UpdateUserRequest {
  userId: string
  name: string
  email: string
  phone: string
  realName: string
  status: number
  gender: string
  age: number
  organizationUnitId: number
  organizationUnitName: string
  birthDate: string
  password: string
}


export const updateUser = ( data: UpdateUserRequest) => {
  return api.put('/user/update', data)
}

// 重置密码
export const resetPassword = (userId: string) => {
  return api.put(`/user/password-reset`, { userId })
}

// 更新用户角色
export interface UpdateUserRolesRequest {
  roleIds: string[],
  userId: string
}

export const updateUserRoles = ( data: UpdateUserRolesRequest) => {
  return api.put(`/users/update-roles`, data)
}

// 获取用户列表
export interface GetUsersRequest {
  pageIndex: number
  pageSize: number
  keyword?: string
  status?: number
  organizationUnitId?: number
  countTotal:boolean
}

export interface UserInfo {
  userId: string;
  name: string;
  phone: string;
  roles: string[];
  realName: string;
  status: number;
  email: string;
  createdAt: string;
  gender: string;
  age: number;
  organizationUnitId: number;
  organizationUnitName: string;
  birthDate: string;
}

export interface GetUsersResponse {
  items: UserInfo[]
  total: number
  pageIndex: number
  pageSize: number
}

export const getUsers = (params: GetUsersRequest) => {
  return api.get<GetUsersResponse>('/users', { params })
}



// 删除用户
export const deleteUser = (userId: string) => {
  return api.delete(`/users/${userId}`)
}



// 下载用户导入模板
export const downloadUserTemplate = () => {
  // 通过Vite代理访问后端静态文件
  const templateUrl = '/Downloads/ExcelTemplates/users_template.xlsx'
  
  return fetch(templateUrl, {
    method: 'GET',
    headers: {
      'Accept': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    }
  }).then(response => {
    if (!response.ok) {
      throw new Error('模板文件下载失败')
    }
    return response.blob()
  })
}

// 批量导入用户
export interface BatchImportUsersRequest {
  file: File
}

export interface BatchImportUsersResponse {
  successCount: number
  failCount: number
  totalCount: number
  failedRows: {
    row: number
    errors: string[]
    data: any
  }[]
}

export interface BatchImportUsersData {
  file: File
  organizationUnitId?: number
  organizationUnitName?: string
  roleIds?: string[]
}

export const batchImportUsers = (data: BatchImportUsersData) => {
  const formData = new FormData()
  formData.append('file', data.file)

  if (data.organizationUnitId) {
    formData.append('organizationUnitId', data.organizationUnitId.toString())
  }
  if (data.organizationUnitName) {
    formData.append('organizationUnitName', data.organizationUnitName)
  }
  
  if (data.roleIds && data.roleIds.length > 0) {
    data.roleIds.forEach(roleId => {
      formData.append('roleIds', roleId)
    })
  }
  
  return api.post<BatchImportUsersResponse>('/users/batch-import', formData, {
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  })
}



