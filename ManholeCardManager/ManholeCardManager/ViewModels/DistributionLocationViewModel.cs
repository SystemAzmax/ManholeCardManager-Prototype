using ManholeCardManager.Models;
using ManholeCardManager.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text.Json;

namespace ManholeCardManager.ViewModels
{
    /// <summary>
    /// 配布場所一覧画面のビューモデル
    /// </summary>
    public class DistributionLocationViewModel : INotifyPropertyChanged
    {
        private const int DEFAULT_MAX_SERIES_NUMBER = 99;
        private const string SERIES_SETTINGS_FILE_NAME = "series-settings.json";

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
            var maxSeriesNumber = await LoadMaxSeriesNumberAsync();

            SeriesNumbers.Clear();
            SeriesNumbers.Add(LocalizationService.Instance.GetString("AllSeries"));
            for (int i = 1; i <= maxSeriesNumber; i++)
            {
                SeriesNumbers.Add($"第{i}弾");
            }
        }

        /// <summary>
        /// 弾数設定ファイルから弾数の上限を読み込みます。
        /// </summary>
        /// <returns>有効な弾数上限。失敗時は既定値。</returns>
        private static async Task<int> LoadMaxSeriesNumberAsync()
        {
            var settingsPath = GetSeriesSettingsPath();

            try
            {
                if (!File.Exists(settingsPath))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Series settings file not found: {settingsPath}");
                    return DEFAULT_MAX_SERIES_NUMBER;
                }

                var json = await File.ReadAllTextAsync(settingsPath);
                var settings = JsonSerializer.Deserialize<SeriesSettings>(
                    json,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (settings?.MaxSeriesNumber > 0)
                {
                    return settings.MaxSeriesNumber;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Failed to load series settings: {ex.Message}");
            }

            return DEFAULT_MAX_SERIES_NUMBER;
        }

        /// <summary>
        /// 弾数設定ファイルの保存先パスを取得します。
        /// </summary>
        /// <returns>設定ファイルのフルパス。</returns>
        private static string GetSeriesSettingsPath()
        {
            var candidates = new[]
            {
                Path.Combine(
                    AppContext.BaseDirectory,
                    SERIES_SETTINGS_FILE_NAME),
                Path.Combine(
                    AppContext.BaseDirectory,
                    "ManholeCardManager",
                    SERIES_SETTINGS_FILE_NAME),
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    SERIES_SETTINGS_FILE_NAME)
            };

            foreach (var path in candidates)
            {
                if (File.Exists(path))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Using series settings: {path}");
                    return path;
                }
            }

            return candidates[0];
        }

        /// <summary>
        /// 弾数設定モデル
        /// </summary>
        private sealed class SeriesSettings
        {
            /// <summary>
            /// 弾数上限
            /// </summary>
            public int MaxSeriesNumber { get; set; }
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

        /// <summary>
        /// カードの取得ステータスを切り替える
        /// </summary>
        /// <param name="cardId">カードID</param>
        /// <param name="locationId">取得場所ID</param>
        public async Task ToggleCardAcquisitionAsync(int cardId, int locationId)
        {
            System.Diagnostics.Debug.WriteLine(
                $"ToggleCardAcquisitionAsync: CardId={cardId}, LocationId={locationId}");
                
            var newStatus = await _databaseService.ToggleCardAcquisitionStatusAsync(cardId, locationId);
            
            System.Diagnostics.Debug.WriteLine(
                $"ToggleCardAcquisitionAsync: New status from DB = {newStatus}");
            
            // 既存のカードオブジェクトを見つけて状態を更新
            foreach (var location in Locations)
            {
                var card = location.DistributedCards.FirstOrDefault(c => c.CardId == cardId);
                if (card != null)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"ToggleCardAcquisitionAsync: Found card, current IsAcquired = {card.IsAcquired}");
                        
                    card.IsAcquired = newStatus;
                    
                    System.Diagnostics.Debug.WriteLine(
                        $"ToggleCardAcquisitionAsync: Updated card {cardId} IsAcquired to {card.IsAcquired}");
                    return;
                }
            }
            
            System.Diagnostics.Debug.WriteLine(
                $"ToggleCardAcquisitionAsync: Card {cardId} not found in Locations");
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
