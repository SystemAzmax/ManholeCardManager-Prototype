using System;

namespace ManholeCardDataScraper.Models
{
    /// <summary>
    /// スクレイピングで取得したマンホールカード配布場所情報
    /// </summary>
    public class DistributionLocationData
    {
        /// <summary>
        /// 場所名
        /// </summary>
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// 弾数
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// マンホールカード画像URL
        /// </summary>
        public string? CardImageUrl { get; set; }

        /// <summary>
        /// 発行年月日
        /// </summary>
        public string? IssueDate { get; set; }

        /// <summary>
        /// 配布場所
        /// </summary>
        public string? DistributionLocation { get; set; }

        /// <summary>
        /// 配布時間
        /// </summary>
        public string? DistributionHours { get; set; }

        /// <summary>
        /// 在庫状況リンクURL
        /// </summary>
        public string? StockStatusUrl { get; set; }

        /// <summary>
        /// URLハッシュ（差分検出用）
        /// </summary>
        public string ContentHash { get; set; } = string.Empty;

        /// <summary>
        /// 取得日時
        /// </summary>
        public DateTimeOffset FetchedDate { get; set; } = DateTimeOffset.Now;
    }
}
