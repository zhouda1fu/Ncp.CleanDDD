import api from './index'
import type { PaginationRequest, PaginationResponse } from '@/types'

// 日志项接口
export interface LogItem {
  id: number
  timestamp: string
  level: string
  message: string
  exception?: string | null
  properties?: string | null
  correlationId?: string | null
}

// 日志查询请求
export interface GetLogsRequest extends PaginationRequest {
  level?: string
  startTime?: string
  endTime?: string
  keyword?: string
}

// 日志级别枚举
export enum LogLevel {
  TRACE = 'Trace',
  DEBUG = 'Debug',
  INFO = 'Information',
  WARN = 'Warning',
  ERROR = 'Error',
  FATAL = 'Fatal'
}

// ==================== API 方法 ====================

/**
 * 获取日志列表
 */
export const getLogs = (params: GetLogsRequest) => {
  return api.get<PaginationResponse<LogItem>>('/logs', { params })
}

/**
 * 根据关联ID获取日志
 */
export const getLogsByCorrelationId = (correlationId: string) => {
  return api.get<LogItem[]>(`/logs/correlation/${correlationId}`)
}

/**
 * 获取日志统计信息
 */
export const getLogStatistics = (startTime?: string, endTime?: string) => {
  return api.get<{
    total: number
    levels: Record<string, number>
  }>('/logs/statistics', {
    params: { startTime, endTime }
  })
}

/**
 * 清理过期日志
 */
export const cleanupLogs = (beforeDate: string) => {
  return api.delete('/logs/cleanup', {
    params: { beforeDate }
  })
} 