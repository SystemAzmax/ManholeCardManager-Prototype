using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace ManholeCardManager.Converters
{
    /// <summary>
    /// 保存ステータスを可視性に変換するコンバーター
    /// </summary>
    public class SaveStatusToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// 値をプロパティに変換
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is not string status || parameter is not string expectedStatus)
            {
                return Visibility.Collapsed;
            }

            return status == expectedStatus ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 値をプロパティに逆変換
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
