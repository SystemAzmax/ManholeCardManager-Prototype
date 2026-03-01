using System.Collections.ObjectModel;

namespace ManholeCardManager.Models
{
    /// <summary>
    /// 配布場所に関連するカード情報を含むビューモデル
    /// </summary>
    public class DistributionLocationWithCards
    {
        /// <summary>
        /// 配布場所ID
        /// </summary>
        public int LocationId { get; set; }

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
        /// 説明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 緯度
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// 経度
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// この配布場所で配布されているカード
        /// </summary>
        public ObservableCollection<CardWithAcquisitionStatus> DistributedCards { get; set; } = 
            new ObservableCollection<CardWithAcquisitionStatus>();

        /// <summary>
        /// 配布カード数
        /// </summary>
        public int TotalCardCount => DistributedCards.Count;

        /// <summary>
        /// 取得済みカード数
        /// </summary>
        public int AcquiredCardCount
        {
            get
            {
                int count = 0;
                foreach (var card in DistributedCards)
                {
                    if (card.IsAcquired) count++;
                }
                return count;
            }
        }
    }
}
