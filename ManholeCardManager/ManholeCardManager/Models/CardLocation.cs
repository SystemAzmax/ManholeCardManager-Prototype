using System;

namespace ManholeCardManager.Models
{
    /// <summary>
    /// カード配布場所の情報を表すモデルクラス
    /// </summary>
    public class CardLocation
    {
        /// <summary>
        /// 場所ID
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// カードID
        /// </summary>
        public int CardId { get; set; }

        /// <summary>
        /// 場所名
        /// </summary>
        public string LocationName { get; set; } = string.Empty;

        /// <summary>
        /// 都道府県
        /// </summary>
        public string? Prefecture { get; set; }

        /// <summary>
        /// 市町村
        /// </summary>
        public string? Municipality { get; set; }

        /// <summary>
        /// 住所
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 緯度
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// 経度
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// 説明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 在庫状況
        /// </summary>
        public string? StockStatus { get; set; }

        /// <summary>
        /// 登録日時
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }
    }
}
