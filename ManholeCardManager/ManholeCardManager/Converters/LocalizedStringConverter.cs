using Microsoft.UI.Xaml.Data;
using ManholeCardManager.Services;
using System;

namespace ManholeCardManager.Converters
{
    /// <summary>
    /// ローカライズされた文字列を取得するコンバーター
    /// </summary>
    public class LocalizedStringConverter : IValueConverter
    {
        /// <summary>
        /// リソースキーを文字列に変換
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string resourceKey)
            {
                return LocalizationService.Instance.GetString(resourceKey);
            }
            return value;
        }

        /// <summary>
        /// 逆変換（使用なし）
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
