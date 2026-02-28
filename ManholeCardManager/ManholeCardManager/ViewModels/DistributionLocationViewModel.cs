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
    /// 配布場所一覧画面のビューモデル
    /// </summary>
    public class DistributionLocationViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private readonly ImageCacheService _imageCacheService;
        private ObservableCollection<DistributionLocationWithCards> _locations;
        private DistributionLocationWithCards? _selectedLocation;
        private string _searchText;
        private bool _isLoading;
        private ObservableCollection<string> _seriesNumbers;
        private string? _selectedSeriesNumber;

        /// <summary>
        /// 配布場所一覧
        /// </summary>
        public ObservableCollection<DistributionLocationWithCards> Locations
        {
            get => _locations;
            set
            {
                if (_locations != value)
                {
                    _locations = value;
                    OnPropertyChanged(nameof(Locations));
                }
            }
        }

        /// <summary>
        /// 選択された配布場所
        /// </summary>
        public DistributionLocationWithCards? SelectedLocation
        {
            get => _selectedLocation;
            set
            {
                if (_selectedLocation != value)
                {
                    _selectedLocation = value;
                    OnPropertyChanged(nameof(SelectedLocation));
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
                    _ = SearchLocations();
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
        /// 弾数フィルタのリスト
        /// </summary>
        public ObservableCollection<string> SeriesNumbers
        {
            get => _seriesNumbers;
            set
            {
                if (_seriesNumbers != value)
                {
                    _seriesNumbers = value;
                    OnPropertyChanged(nameof(SeriesNumbers));
                }
            }
        }

        /// <summary>
        /// 選択された弾数
        /// </summary>
        public string? SelectedSeriesNumber
        {
            get => _selectedSeriesNumber;
            set
            {
                if (_selectedSeriesNumber != value)
                {
                    _selectedSeriesNumber = value;
                    OnPropertyChanged(nameof(SelectedSeriesNumber));
                    _ = SearchLocations();
                }
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DistributionLocationViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _imageCacheService = new ImageCacheService();
            _locations = new ObservableCollection<DistributionLocationWithCards>();
            _searchText = string.Empty;
            _seriesNumbers = new ObservableCollection<string>();
        }

        /// <summary>
        /// データを初期化
        /// </summary>
        public async Task InitializeAsync()
        {
            IsLoading = true;
            try
            {
                await LoadSeriesNumbers();
                await LoadDistributionLocations();
                System.Diagnostics.Debug.WriteLine($"DistributionLocationViewModel initialized with {Locations.Count} locations");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 全配布場所を読み込む
        /// </summary>
        private async Task LoadDistributionLocations()
        {
            var locations = await _databaseService.GetAllDistributionLocationsWithCardsAsync();
            Locations.Clear();
            foreach (var location in locations.OrderBy(l => l.LocationName))
            {
                foreach (var card in location.DistributedCards)
                {
                    card.CachedImagePath = await _imageCacheService.GetImageAsync(card.DesignImagePath);
                }
                Locations.Add(location);
            }
        }

        /// <summary>
        /// 弾数リストを読み込む
        /// </summary>
        private async Task LoadSeriesNumbers()
        {
            SeriesNumbers.Clear();
            SeriesNumbers.Add(LocalizationService.Instance.GetString("AllSeries"));
            for (int i = 1; i <= 27; i++)
            {
                SeriesNumbers.Add($"第{i}弾");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 配布場所を検索
        /// </summary>
        public async Task SearchLocations()
        {
            IsLoading = true;
            try
            {
                var allLocations = await _databaseService.GetAllDistributionLocationsWithCardsAsync();
                var allSeriesText = LocalizationService.Instance.GetString("AllSeries");
                
                if (!string.IsNullOrEmpty(SearchText) || !string.IsNullOrEmpty(SelectedSeriesNumber) && SelectedSeriesNumber != allSeriesText)
                {
                    var filtered = allLocations.AsEnumerable();
                    
                    if (!string.IsNullOrEmpty(SearchText))
                    {
                        filtered = filtered.Where(l => 
                            l.LocationName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                            (l.Address?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));
                    }
                    
                    if (!string.IsNullOrEmpty(SelectedSeriesNumber) && SelectedSeriesNumber != allSeriesText)
                    {
                        filtered = filtered.Select(l => new DistributionLocationWithCards
                        {
                            LocationId = l.LocationId,
                            LocationName = l.LocationName,
                            Address = l.Address,
                            DistributedCards = new ObservableCollection<CardWithAcquisitionStatus>(
                                l.DistributedCards.Where(c => $"第{c.SeriesNumber}弾" == SelectedSeriesNumber))
                        }).Where(l => l.DistributedCards.Any());
                    }
                    
                    var filteredList = filtered.OrderBy(l => l.LocationName).ToList();
                    
                    Locations.Clear();
                    foreach (var location in filteredList)
                    {
                        foreach (var card in location.DistributedCards)
                        {
                            card.CachedImagePath = await _imageCacheService.GetImageAsync(card.DesignImagePath);
                        }
                        Locations.Add(location);
                    }
                }
                else
                {
                    await LoadDistributionLocations();
                }
            }
            finally
            {
                IsLoading = false;
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
