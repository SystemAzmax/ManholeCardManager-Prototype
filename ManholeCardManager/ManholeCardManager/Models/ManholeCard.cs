using System;

namespace ManholeCardManager.Models
{
    /// <summary>
    /// マンホールカードの情報を表すモデルクラス
    /// </summary>
    public class ManholeCard
    {
        /// <summary>
        /// カードID
        /// </summary>
        public int CardId { get; set; }

        /// <summary>
        /// 場所ID
        /// </summary>
        public int? LocationId { get; set; }

        /// <summary>
        /// カードデザイン画像パス
        /// </summary>
        public string? DesignImagePath { get; set; }

        /// <summary>
        /// 弾数
        /// </summary>
        public int SeriesNumber { get; set; } = 1;

        /// <summary>
        /// 発行年月日
        /// </summary>
        public DateTimeOffset? IssuedDate { get; set; }

        /// <summary>
        /// 登録日時
        /// </summary>
        public DateTimeOffset CreatedDate { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        public DateTimeOffset UpdatedDate { get; set; }
    }
}
