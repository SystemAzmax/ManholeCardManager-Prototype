namespace ManholeCardManager.Models
{
    /// <summary>
    /// マンホール設置場所とカードの関連情報を表すモデルクラス
    /// </summary>
    public class ManholeLocation
    {
        /// <summary>
        /// 関連ID
        /// </summary>
        public int RelationId { get; set; }

        /// <summary>
        /// 場所ID
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// カードID
        /// </summary>
        public int CardId { get; set; }

        /// <summary>
        /// マンホール設置場所の緯度
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// マンホール設置場所の経度
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// マンホール設置場所の名称
        /// </summary>
        public string? SpotName { get; set; }

        /// <summary>
        /// マンホール設置場所の説明
        /// </summary>
        public string? Description { get; set; }
    }
}
