using System.ComponentModel;

namespace ManholeCardManager.Models
{
    /// <summary>
    /// 取得状況付きのカード情報
    /// </summary>
    public class CardWithAcquisitionStatus : INotifyPropertyChanged
    {
        private bool _isAcquired;
        private System.DateTimeOffset? _acquisitionDate;
        private string? _notes;
        private string _saveStatus = "none"; // none, saving, success, error

        /// <summary>
        /// カードID
        /// </summary>
        public int CardId { get; set; }

        /// <summary>
        /// 取得済みフラグ
        /// </summary>
        public bool IsAcquired
        {
            get => _isAcquired;
            set
            {
                if (_isAcquired != value)
                {
                    _isAcquired = value;
                    OnPropertyChanged(nameof(IsAcquired));
                    OnPropertyChanged(nameof(AcquisitionStatusDisplay));
                }
            }
        }

        /// <summary>
        /// 取得状況を表示用文字列で取得
        /// </summary>
        public string AcquisitionStatusDisplay => string.Empty;

        /// <summary>
        /// 都道府県
        /// </summary>
        public string? Prefecture { get; set; }

        /// <summary>
        /// 市町村
        /// </summary>
        public string? Municipality { get; set; }

        /// <summary>
        /// カードデザイン画像パス
        /// </summary>
        public string? DesignImagePath { get; set; }

        /// <summary>
        /// キャッシュされた画像の表示パス
        /// </summary>
        public string? CachedImagePath { get; set; }

        /// <summary>
        /// 弾数
        /// </summary>
        public int SeriesNumber { get; set; } = 1;

        /// <summary>
        /// 発行年月日
        /// </summary>
        public System.DateTimeOffset? IssuedDate { get; set; }

        /// <summary>
        /// 在庫状況
        /// </summary>
        public string? StockStatus { get; set; }

        /// <summary>
        /// 説明（配布場所の概要）
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 所在地表示用（都道府県 / 市町村）
        /// </summary>
        public string LocationDisplay
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrEmpty(Prefecture)) parts.Add(Prefecture);
                if (!string.IsNullOrEmpty(Municipality)) parts.Add(Municipality);
                return string.Join(" / ", parts);
            }
        }

        /// <summary>
        /// カード表示名
        /// </summary>
        public string CardTitleDisplay
        {
            get
            {
                var parts = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrEmpty(Prefecture)) parts.Add(Prefecture);
                if (!string.IsNullOrEmpty(Municipality)) parts.Add(Municipality);
                var title = string.Join(" ", parts);
                if (string.IsNullOrEmpty(title))
                {
                    return $"カード {CardId}";
                }

                return title;
            }
        }

        /// <summary>
        /// 発行日表示用
        /// </summary>
        public string IssuedDateDisplay
        {
            get
            {
                return IssuedDate?.ToString("yyyy/MM/dd") ?? "日付不明";
            }
        }

        /// <summary>
        /// 弾数表示用
        /// </summary>
        public string SeriesNumberDisplay
        {
            get
            {
                return $"第{SeriesNumber}弾";
            }
        }

        /// <summary>
        /// 概要表示用
        /// </summary>
        public string DescriptionDisplay
        {
            get
            {
                return string.IsNullOrEmpty(Description) ? "情報なし" : Description;
            }
        }

        /// <summary>
        /// 在庫数表示用
        /// </summary>
        public string StockCountDisplay
        {
            get
            {
                if (string.IsNullOrEmpty(StockStatus))
                {
                    return string.Empty;
                }

                // StockStatusから在庫数を抽出する処理
                // 例: "在庫あり (50枚)" のような形式を想定
                if (StockStatus.Contains("(") && StockStatus.Contains(")"))
                {
                    var startIndex = StockStatus.IndexOf("(") + 1;
                    var endIndex = StockStatus.IndexOf(")");
                    if (endIndex > startIndex)
                    {
                        return StockStatus.Substring(startIndex, endIndex - startIndex);
                    }
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// 取得日時
        /// </summary>
        public System.DateTimeOffset? AcquisitionDate
        {
            get => _acquisitionDate;
            set
            {
                if (_acquisitionDate != value)
                {
                    _acquisitionDate = value;
                    OnPropertyChanged(nameof(AcquisitionDate));
                    OnPropertyChanged(nameof(AcquisitionDateDisplay));
                }
            }
        }

        /// <summary>
        /// 取得日時表示用
        /// </summary>
        public string AcquisitionDateDisplay
        {
            get
            {
                if (AcquisitionDate == null)
                    return string.Empty;
                return AcquisitionDate.Value.ToString("yyyy-MM-ddTHH:mm:sszzz");
            }
        }

        /// <summary>
        /// 備考
        /// </summary>
        public string? Notes
        {
            get => _notes;
            set
            {
                if (_notes != value)
                {
                    _notes = value;
                    OnPropertyChanged(nameof(Notes));
                }
            }
        }

        /// <summary>
        /// 保存ステータス（none, saving, success, error）
        /// </summary>
        public string SaveStatus
        {
            get => _saveStatus;
            set
            {
                if (_saveStatus != value)
                {
                    _saveStatus = value;
                    OnPropertyChanged(nameof(SaveStatus));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティ変更通知
        /// </summary>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
