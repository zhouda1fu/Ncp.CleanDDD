using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Ncp.CleanDDD.Avalonia.ViewModels
{
    public class StatusToIconConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                //return status.ToLower() switch
                //{
                //    "启用" or "active" or "1" => "✅",
                //    "禁用" or "inactive" or "0" => "❌",
                //    "待审核" or "pending" or "2" => "⏳",
                //    "已锁定" or "locked" or "3" => "🔒",
                //    _ => "❓"
                //};

                return status switch
                {
                    1 => "✅",
                    0 => "❌",
                    _ => "❓"
                };
            }
            return "❓";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
