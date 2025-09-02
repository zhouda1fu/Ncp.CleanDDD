import dayjs from 'dayjs'
import duration from 'dayjs/plugin/duration'
import relativeTime from 'dayjs/plugin/relativeTime'
import 'dayjs/locale/zh-cn'

// 扩展 dayjs 功能
dayjs.extend(duration)
dayjs.extend(relativeTime)

// 设置中文语言
dayjs.locale('zh-cn')

export default dayjs 