import api from './index'

export interface LogItem {
  id: number
  timestamp: string
  level: string
  message: string
  exception?: string | null
  properties?: string | null
  correlationId?: string | null
}

export interface GetLogsRequest {
  pageIndex: number
  pageSize: number
  level?: string
  startTime?: string
  endTime?: string
  keyword?: string
  countTotal: boolean
}

export interface PagedData<T> {
  items: T[]
  total: number
  pageIndex: number
  pageSize: number
}

export const getLogs = (params: GetLogsRequest) => {
  return api.get('/logs', { params })
}

export const getLogsByCorrelationId = (correlationId: string) => {
  return api.get(`/logs/correlation/${correlationId}`)
} 