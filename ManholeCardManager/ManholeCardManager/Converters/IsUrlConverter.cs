using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace ManholeCardManager.Converters
{
    /// <summary>
    /// 文字列がURLかどうかを判定するコンバーター
    /// </summary>
    public sealed class IsUrlConverter : IValueConverter
    {
        /// <summary>
        /// 値がURLかどうかを判定
        /// </summary>
        public object Convert(
            object value,
            Type targetType,
            object parameter,
            string language)
        {
            var isUrl = false;
            if (value is string str)
            {
                isUrl = Uri.TryCreate(str, UriKind.Absolute, out _);
            }

            // パラメーターがfalseの場合は反転
            if (parameter is string paramStr && paramStr == "false")
            {
                isUrl = !isUrl;
            }

            return isUrl ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// 逆変換（未実装）
        /// </summary>
        public object ConvertBack(
            object value,
            Type targetType,
            object parameter,
            string language)
        {
            throw new NotImplementedException();
        }
    }
}
