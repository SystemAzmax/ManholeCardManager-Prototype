using System;

namespace ManholeCardManager.Models
{
    /// <summary>
    /// カード取得履歴を表すモデルクラス
    /// </summary>
    public class AcquisitionHistory
    {
        /// <summary>
        /// 履歴ID
        /// </summary>
        public int HistoryId { get; set; }

        /// <summary>
        /// カードID
        /// </summary>
        public int CardId { get; set; }

        /// <summary>
        /// 取得日時
        /// </summary>
        public DateTimeOffset AcquisitionDate { get; set; }

        /// <summary>
        /// 取得場所のID
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// 備考
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// 登録日時
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }
    }
}
