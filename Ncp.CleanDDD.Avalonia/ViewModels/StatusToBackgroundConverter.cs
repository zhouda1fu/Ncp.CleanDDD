using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Ncp.CleanDDD.Avalonia.ViewModels
{
    public class StatusToBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                //return status.ToLower() switch
                //{
                //    "启用" or "active" or "1" => "#10b981", // 绿色
                //    "禁用" or "inactive" or "0" => "#ef4444", // 红色
                //    "待审核" or "pending" or "2" => "#f59e0b", // 橙色
                //    "已锁定" or "locked" or "3" => "#6b7280", // 灰色
                //    _ => "#6b7280" // 默认灰色
                //};

                return status switch
                {
                    1 => "#10b981",
                    0 => "#ef4444",
                    _ => "#6b7280" // 默认灰色
                };
            }
            return "#6b7280";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
