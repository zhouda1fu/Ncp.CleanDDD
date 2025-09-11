import dayjs from 'dayjs'
import { DATE_FORMAT } from '@/constants'

/**
 * 日期格式化工具
 */
export class DateFormatter {
  /**
   * 格式化日期
   * @param date 日期
   * @param format 格式
   * @returns 格式化后的日期字符串
   */
  static format(date: string | Date | dayjs.Dayjs, format: string = DATE_FORMAT.DATETIME): string {
    if (!date) return ''
    return dayjs(date).format(format)
  }

  /**
   * 格式化为日期（不含时间）
   * @param date 日期
   * @returns 格式化后的日期字符串
   */
  static toDateString(date: string | Date | dayjs.Dayjs): string {
    return this.format(date, DATE_FORMAT.DATE)
  }

  /**
   * 格式化为日期时间
   * @param date 日期
   * @returns 格式化后的日期时间字符串
   */
  static toDateTimeString(date: string | Date | dayjs.Dayjs): string {
    return this.format(date, DATE_FORMAT.DATETIME)
  }

  /**
   * 格式化为时间
   * @param date 日期
   * @returns 格式化后的时间字符串
   */
  static toTimeString(date: string | Date | dayjs.Dayjs): string {
    return this.format(date, DATE_FORMAT.TIME)
  }

  /**
   * 相对时间
   * @param date 日期
   * @returns 相对时间字符串
   */
  static fromNow(date: string | Date | dayjs.Dayjs): string {
    if (!date) return ''
    return dayjs(date).fromNow()
  }

  /**
   * 计算年龄
   * @param birthDate 出生日期
   * @returns 年龄
   */
  static calculateAge(birthDate: string | Date): number {
    if (!birthDate) return 0
    
    const birth = dayjs(birthDate)
    const today = dayjs()
    
    let age = today.year() - birth.year()
    const monthDiff = today.month() - birth.month()
    
    // 如果还没过生日，年龄减1
    if (monthDiff < 0 || (monthDiff === 0 && today.date() < birth.date())) {
      age--
    }
    
    return age > 0 ? age : 0
  }
}

/**
 * 数字格式化工具
 */
export class NumberFormatter {
  /**
   * 格式化数字为千分位
   * @param num 数字
   * @returns 格式化后的字符串
   */
  static toThousands(num: number): string {
    return num.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',')
  }

  /**
   * 格式化文件大小
   * @param bytes 字节数
   * @returns 格式化后的文件大小
   */
  static formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 B'
    
    const k = 1024
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
    const i = Math.floor(Math.log(bytes) / Math.log(k))
    
    return `${parseFloat((bytes / Math.pow(k, i)).toFixed(2))} ${sizes[i]}`
  }

  /**
   * 格式化百分比
   * @param num 数字
   * @param decimals 小数位数
   * @returns 格式化后的百分比
   */
  static toPercentage(num: number, decimals = 2): string {
    return `${(num * 100).toFixed(decimals)}%`
  }
}

/**
 * 字符串格式化工具
 */
export class StringFormatter {
  /**
   * 截断字符串
   * @param str 字符串
   * @param length 长度
   * @param suffix 后缀
   * @returns 截断后的字符串
   */
  static truncate(str: string, length: number, suffix = '...'): string {
    if (str.length <= length) return str
    return str.substring(0, length) + suffix
  }

  /**
   * 首字母大写
   * @param str 字符串
   * @returns 首字母大写的字符串
   */
  static capitalize(str: string): string {
    if (!str) return ''
    return str.charAt(0).toUpperCase() + str.slice(1)
  }

  /**
   * 驼峰转下划线
   * @param str 字符串
   * @returns 下划线形式的字符串
   */
  static camelToUnderscore(str: string): string {
    return str.replace(/([A-Z])/g, '_$1').toLowerCase()
  }

  /**
   * 下划线转驼峰
   * @param str 字符串
   * @returns 驼峰形式的字符串
   */
  static underscoreToCamel(str: string): string {
    return str.replace(/_([a-z])/g, (_, letter) => letter.toUpperCase())
  }

  /**
   * 隐藏手机号中间四位
   * @param phone 手机号
   * @returns 隐藏后的手机号
   */
  static hidePhone(phone: string): string {
    if (!phone || phone.length !== 11) return phone
    return phone.replace(/(\d{3})\d{4}(\d{4})/, '$1****$2')
  }

  /**
   * 隐藏邮箱用户名部分
   * @param email 邮箱
   * @returns 隐藏后的邮箱
   */
  static hideEmail(email: string): string {
    if (!email || !email.includes('@')) return email
    const [username, domain] = email.split('@')
    if (username.length <= 2) return email
    return `${username.charAt(0)}***${username.slice(-1)}@${domain}`
  }
}
