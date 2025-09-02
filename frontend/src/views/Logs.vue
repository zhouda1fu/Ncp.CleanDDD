<template>
  <div class="logs-page">
    <div class="page-header">
      <div class="header-content">
        <div class="title-section">
          <h1>系统日志</h1>
          <p class="subtitle">查看后端运行日志，支持按级别/时间/关键字筛选</p>
        </div>
      </div>
    </div>

    <div class="main-content">
      <div class="search-section">
        <div class="section-header">
          <h2>
            <el-icon class="section-icon"><Search /></el-icon>
            搜索筛选
          </h2>
        </div>
        <div class="search-wrapper">
          <el-form :inline="true" :model="searchForm" class="search-form">
            <el-form-item label="级别">
              <el-select v-model="searchForm.level" placeholder="全部" clearable class="level-select">
                <el-option v-for="l in levels" :key="l" :label="l" :value="l" />
              </el-select>
            </el-form-item>
            <el-form-item label="时间区间">
              <el-date-picker
                v-model="timeRange"
                type="datetimerange"
                range-separator="至"
                start-placeholder="开始时间"
                end-placeholder="结束时间"
                value-format="YYYY-MM-DDTHH:mm:ss"
              />
            </el-form-item>
            <el-form-item label="关键字">
              <el-input v-model="searchForm.keyword" placeholder="消息或异常关键字" clearable class="search-input" @keyup.enter="handleSearch" />
            </el-form-item>
            <el-form-item>
              <div class="action-buttons">
                <el-button type="primary" class="search-btn" @click="handleSearch">
                  <el-icon><Search /></el-icon>
                  搜索
                </el-button>
                <el-button class="reset-btn" @click="handleReset">
                  <el-icon><Refresh /></el-icon>
                  重置
                </el-button>
              </div>
            </el-form-item>
          </el-form>
        </div>
      </div>

      <div class="table-section">
        <div class="section-header">
          <div class="header-left">
            <h2>
              <el-icon class="section-icon"><Document /></el-icon>
              日志列表
            </h2>
            <el-tag type="info" class="count-tag">{{ pagination.total }} 条</el-tag>
          </div>
        </div>

        <div class="table-wrapper">
          <el-table
            v-loading="loading"
            :data="logs"
            class="logs-table"
            :header-cell-style="{ backgroundColor: '#f8fafc', color: '#374151', fontWeight: '600' }"
            stripe
            row-key="id"
            :expand-row-keys="expandedRowKeys"
          >
            <el-table-column prop="timestamp" label="时间" width="190" align="center">
              <template #default="{ row }">
                <div class="time-cell">
                  <el-icon class="time-icon"><Clock /></el-icon>
                  <span>{{ formatDate(row.timestamp) }}</span>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="level" label="级别" width="100" align="center">
              <template #default="{ row }">
                <el-tag :type="levelType(row.level)" size="small" class="level-tag">{{ row.level }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="correlationId" label="关联ID" width="120" align="center">
              <template #default="{ row }">
                <el-button v-if="row.correlationId" size="small" type="primary" plain @click="showCorrelationLogs(row.correlationId)" class="correlation-btn">
                  {{ row.correlationId?.substring(0, 8) }}...
                </el-button>
                <span v-else class="empty-text">-</span>
              </template>
            </el-table-column>
            <el-table-column prop="message" label="消息" min-width="300">
              <template #default="{ row }">
                <div class="message-cell" :title="row.message">{{ row.message }}</div>
              </template>
            </el-table-column>
            <el-table-column label="异常" min-width="120" align="center">
              <template #default="{ row }">
                <el-button v-if="row.exception" size="small" type="danger" @click="toggleExpand(row.id)" plain>
                  查看异常
                </el-button>
                <span v-else class="empty-text">-</span>
              </template>
            </el-table-column>
            <el-table-column label="属性" min-width="120" align="center">
              <template #default="{ row }">
                <el-button v-if="row.properties" size="small" type="info" @click="toggleExpandProps(row.id)" plain>
                  查看属性
                </el-button>
                <span v-else class="empty-text">-</span>
              </template>
            </el-table-column>
            <el-table-column type="expand">
              <template #default="{ row }">
                <div class="expand-content">
                  <div v-if="row.exception" class="exception-block">
                    <div class="expand-title">异常详情</div>
                    <pre class="exception-pre">{{ row.exception }}</pre>
                  </div>
                  <div v-if="row.properties" class="props-block">
                    <div class="expand-title">属性</div>
                    <pre class="props-pre">{{ row.properties }}</pre>
                  </div>
                  <div v-if="!row.exception && !row.properties" class="empty-expand">无附加信息</div>
                </div>
              </template>
            </el-table-column>
          </el-table>

          <div v-if="logs.length === 0 && !loading" class="empty-state">
            <el-icon class="empty-icon"><DocumentRemove /></el-icon>
            <p>暂无日志数据</p>
          </div>

          <div class="pagination-wrapper">
            <el-pagination
              v-model:current-page="pagination.pageIndex"
              v-model:page-size="pagination.pageSize"
              :total="pagination.total"
              :page-sizes="[10, 20, 50, 100]"
              layout="total, sizes, prev, pager, next, jumper"
              class="pagination"
              @update:page-size="handleSizeChange"
              @update:current-page="handleCurrentChange"
            />
          </div>
        </div>
      </div>

      <!-- 关联日志弹窗 -->
      <el-dialog
        v-model="correlationDialogVisible"
        title="关联请求日志"
        width="80%"
        :close-on-click-modal="false"
        class="correlation-dialog"
      >
        <div v-loading="correlationLoading" class="correlation-content">
          <div class="correlation-info">
            <el-alert
              title="这些日志记录属于同一个HTTP请求"
              :description="`关联ID: ${selectedCorrelationId}`"
              type="info"
              show-icon
              :closable="false"
            />
          </div>

          <el-table
            :data="correlationLogs"
            class="correlation-table"
            :header-cell-style="{ backgroundColor: '#f8fafc', color: '#374151', fontWeight: '600' }"
            stripe
            row-key="id"
            :expand-row-keys="correlationExpandedRowKeys"
          >
            <el-table-column prop="timestamp" label="时间" width="190" align="center">
              <template #default="{ row }">
                <div class="time-cell">
                  <el-icon class="time-icon"><Clock /></el-icon>
                  <span>{{ formatDate(row.timestamp) }}</span>
                </div>
              </template>
            </el-table-column>
            <el-table-column prop="level" label="级别" width="100" align="center">
              <template #default="{ row }">
                <el-tag :type="levelType(row.level)" size="small" class="level-tag">{{ row.level }}</el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="message" label="消息" min-width="400">
              <template #default="{ row }">
                <div class="message-cell" :title="row.message">{{ row.message }}</div>
              </template>
            </el-table-column>
            <el-table-column label="异常" width="120" align="center">
              <template #default="{ row }">
                <el-button v-if="row.exception" size="small" type="danger" @click="toggleCorrelationExpand(row.id)" plain>
                  查看异常
                </el-button>
                <span v-else class="empty-text">-</span>
              </template>
            </el-table-column>
            <el-table-column type="expand">
              <template #default="{ row }">
                <div class="expand-content">
                  <div v-if="row.exception" class="exception-block">
                    <div class="expand-title">异常详情</div>
                    <pre class="exception-pre">{{ row.exception }}</pre>
                  </div>
                  <div v-if="row.properties" class="props-block">
                    <div class="expand-title">属性</div>
                    <pre class="props-pre">{{ row.properties }}</pre>
                  </div>
                  <div v-if="!row.exception && !row.properties" class="empty-expand">无附加信息</div>
                </div>
              </template>
            </el-table-column>
          </el-table>

          <div v-if="correlationLogs.length === 0 && !correlationLoading" class="empty-state">
            <el-icon class="empty-icon"><DocumentRemove /></el-icon>
            <p>暂无相关日志数据</p>
          </div>
        </div>
      </el-dialog>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, reactive, onMounted } from 'vue'
import { Search, Refresh, Document, DocumentRemove, Clock } from '@element-plus/icons-vue'
import { getLogs, getLogsByCorrelationId, type LogItem } from '@/api/log'

const levels = ['Verbose', 'Debug', 'Information', 'Warning', 'Error', 'Fatal']

const loading = ref(false)
const logs = ref<LogItem[]>([])
const expandedRowKeys = ref<number[]>([])

// 关联日志弹窗相关
const correlationDialogVisible = ref(false)
const correlationLoading = ref(false)
const correlationLogs = ref<LogItem[]>([])
const selectedCorrelationId = ref('')
const correlationExpandedRowKeys = ref<number[]>([])

const searchForm = reactive({
  level: '' as string | undefined,
  keyword: ''
})
const timeRange = ref<string[] | null>(null)

const pagination = reactive({
  pageIndex: 1,
  pageSize: 10,
  total: 0
})

const loadLogs = async () => {
  loading.value = true
  try {
    const resp = await getLogs({
      pageIndex: pagination.pageIndex,
      pageSize: pagination.pageSize,
      level: searchForm.level || undefined,
      startTime: timeRange.value?.[0],
      endTime: timeRange.value?.[1],
      keyword: searchForm.keyword || undefined,
      countTotal: true
    })
    logs.value = resp.data.items
    pagination.total = resp.data.total
    expandedRowKeys.value = []
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  pagination.pageIndex = 1
  loadLogs()
}

const handleReset = () => {
  searchForm.level = undefined
  searchForm.keyword = ''
  timeRange.value = null
  pagination.pageIndex = 1
  loadLogs()
}

const handleSizeChange = (size: number) => {
  pagination.pageSize = size
  pagination.pageIndex = 1
  loadLogs()
}

const handleCurrentChange = (page: number) => {
  pagination.pageIndex = page
  loadLogs()
}

const levelType = (level: string) => {
  switch (level?.toLowerCase()) {
    case 'fatal': return 'danger'
    case 'error': return 'danger'
    case 'warning': return 'warning'
    case 'information': return 'success'
    case 'debug': return 'info'
    default: return ''
  }
}

const formatDate = (iso: string) => new Date(iso).toLocaleString('zh-CN')

const toggleExpand = (id: number) => {
  const index = expandedRowKeys.value.indexOf(id)
  if (index > -1) {
    expandedRowKeys.value.splice(index, 1)
  } else {
    expandedRowKeys.value.push(id)
  }
}
const toggleExpandProps = (id: number) => {
  const index = expandedRowKeys.value.indexOf(id)
  if (index > -1) {
    expandedRowKeys.value.splice(index, 1)
  } else {
    expandedRowKeys.value.push(id)
  }
}

const showCorrelationLogs = async (correlationId: string) => {
  if (!correlationId) return

  selectedCorrelationId.value = correlationId
  correlationDialogVisible.value = true
  correlationLoading.value = true

  try {
    const resp = await getLogsByCorrelationId(correlationId)
    correlationLogs.value = resp.data
    correlationExpandedRowKeys.value = []
  } catch (error) {
    console.error('加载关联日志失败:', error)
    correlationLogs.value = []
  } finally {
    correlationLoading.value = false
  }
}

const toggleCorrelationExpand = (id: number) => {
  const index = correlationExpandedRowKeys.value.indexOf(id)
  if (index > -1) {
    correlationExpandedRowKeys.value.splice(index, 1)
  } else {
    correlationExpandedRowKeys.value.push(id)
  }
}

onMounted(() => {
  loadLogs()
})
</script>

<style scoped>
.logs-page { min-height: 80vh; background: #f5f7fa; }
.page-header { background: linear-gradient(135deg, #334155 0%, #0f172a 100%); color: #fff; padding: 1.5rem 2rem 2.25rem; }
.header-content { max-width: 1400px; margin: 0 auto; }
.title-section h1 { margin: 0 0 .5rem 0; font-size: 28px; font-weight: 700; }
.subtitle { margin: 0; opacity:.85 }
.main-content { max-width: 1400px; margin: -1.5rem auto 0; padding: 0 2rem 2rem; display: flex; flex-direction: column; gap: 1.25rem; }
.search-section, .table-section { background: white; border-radius: 16px; box-shadow: 0 10px 30px rgba(0,0,0,.08); overflow: hidden; }
.section-header { padding: 1rem 1.25rem; background: #f8fafc; border-bottom: 1px solid #e2e8f0; display:flex; align-items:center; justify-content:space-between; }
.section-icon{ color:#6366f1 }
.search-wrapper,.table-wrapper{ padding: 1rem 1.25rem; }
.search-form{ display:flex; align-items:center; gap: .75rem; flex-wrap: wrap; }
.level-select,.search-input{ width: 180px; }
.action-buttons{ display:flex; gap:.5rem; flex-wrap: wrap; }
.count-tag{ margin-left:.5rem }
.logs-table{ border-radius:8px; overflow:hidden }
.time-cell{ display:flex; align-items:center; justify-content:center; gap:.35rem }
.time-icon{ color:#9ca3af }
.message-cell{ overflow:hidden; text-overflow:ellipsis; white-space:nowrap }
.expand-content{ padding: .5rem 1rem }
.exception-pre,.props-pre{ background:#0a0a0a; color:#d1d5db; padding:.75rem; border-radius:8px; white-space:pre-wrap }
.exception-block{ margin-bottom:.75rem }
.empty-expand{ color:#94a3b8 }
.empty-text{ color:#9ca3af }
.correlation-btn{ color:#3b82f6 }
.correlation-dialog .el-dialog__body{ padding:1rem 1.5rem }
.correlation-content{ min-height:400px }
.correlation-info{ margin-bottom:1rem }
.correlation-table{ border-radius:8px; overflow:hidden }
.pagination-wrapper{ margin-top: 1rem; display:flex; justify-content:center; padding-top: 1rem; border-top:1px solid #f1f5f9 }
</style> 