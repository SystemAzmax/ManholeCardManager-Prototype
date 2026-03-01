using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace ManholeCardManager.Converters
{
    /// <summary>
    /// 取得状況に基づいて色を返すコンバーター
    /// </summary>
    public class AcquisitionStatusColorConverter : IValueConverter
    {
        /// <summary>
        /// 取得状況をブラシに変換
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isAcquired)
            {
                // 取得済み = 緑, 未取得 = グレー
                return isAcquired 
                    ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 76, 175, 80))  // 緑
                    : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 189, 189, 189)); // グレー
            }
            return new SolidColorBrush(Windows.UI.Color.FromArgb(255, 189, 189, 189));
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
