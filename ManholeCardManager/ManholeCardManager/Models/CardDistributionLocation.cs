using System;

namespace ManholeCardManager.Models
{
    /// <summary>
    /// 配布場所で配布されているカード情報
    /// </summary>
    public class CardDistributionLocation
    {
        /// <summary>
        /// 関連ID
        /// </summary>
        public int RelationId { get; set; }

        /// <summary>
        /// 配布場所ID
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// カードID
        /// </summary>
        public int CardId { get; set; }

        /// <summary>
        /// 配布時間
        /// </summary>
        public string? DistributionTime { get; set; }

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
