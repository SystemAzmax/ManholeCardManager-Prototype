using ManholeCardManager.Models;
using ManholeCardManager.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;

namespace ManholeCardManager.ViewModels
{
    /// <summary>
    /// カードコレクション画面のビューモデル
    /// </summary>
    public class CardCollectionViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<ManholeCard> _cards;
        private ManholeCard? _selectedCard;
        private string _searchText;
        private bool _isLoading;

        /// <summary>
        /// カードコレクション
        /// </summary>
        public ObservableCollection<ManholeCard> Cards
        {
            get => _cards;
            set
            {
                if (_cards != value)
                {
                    _cards = value;
                    OnPropertyChanged(nameof(Cards));
                }
            }
        }

        /// <summary>
        /// 選択されたカード
        /// </summary>
        public ManholeCard? SelectedCard
        {
            get => _selectedCard;
            set
            {
                if (_selectedCard != value)
                {
                    _selectedCard = value;
                    OnPropertyChanged(nameof(SelectedCard));
                }
            }
        }

        /// <summary>
        /// 検索テキスト
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    _ = SearchCards();
                }
            }
        }

        /// <summary>
        /// ローディング状態
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CardCollectionViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _cards = new ObservableCollection<ManholeCard>();
            _searchText = string.Empty;
        }

        /// <summary>
        /// データを初期化
        /// </summary>
        public async Task InitializeAsync()
        {
            IsLoading = true;
            try
            {
                await LoadAllCards();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// カードを再読み込み（言語切り替え時）
        /// </summary>
        public void ReloadCards()
        {
            OnPropertyChanged(nameof(Cards));
        }

        /// <summary>
        /// 全カードを読み込む
        /// </summary>
        private async Task LoadAllCards()
        {
            var cards = await _databaseService.GetAllCardsAsync();
            Cards.Clear();
            foreach (var card in cards)
            {
                Cards.Add(card);
            }
        }

        /// <summary>
        /// カード名で検索
        /// </summary>
        public async Task SearchCards()
        {
            IsLoading = true;
            try
            {
                if (string.IsNullOrEmpty(SearchText))
                {
                    await LoadAllCards();
                }
                else
                {
                    var cards = await _databaseService.SearchCardsAsync(SearchText);
                    Cards.Clear();
                    foreach (var card in cards)
                    {
                        Cards.Add(card);
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// カードを追加
        /// </summary>
        public async Task AddCardAsync(ManholeCard card)
        {
            IsLoading = true;
            try
            {
                await _databaseService.InsertCardAsync(card);
                await LoadAllCards();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// カードを更新
        /// </summary>
        public async Task UpdateCardAsync(ManholeCard card)
        {
            IsLoading = true;
            try
            {
                await _databaseService.UpdateCardAsync(card);
                await LoadAllCards();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// カードを削除
        /// </summary>
        public async Task DeleteCardAsync(ManholeCard card)
        {
            IsLoading = true;
            try
            {
                await _databaseService.DeleteCardAsync(card.CardId);
                await LoadAllCards();
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティ変更イベント
        /// </summary>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
