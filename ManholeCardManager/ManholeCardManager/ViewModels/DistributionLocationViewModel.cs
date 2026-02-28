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
        private ObservableCollection<DistributionLocationWithCards> _locations;
        private DistributionLocationWithCards? _selectedLocation;
        private string _searchText;
        private bool _isLoading;

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
        /// コンストラクタ
        /// </summary>
        public DistributionLocationViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _locations = new ObservableCollection<DistributionLocationWithCards>();
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
                Locations.Add(location);
            }
        }

        /// <summary>
        /// 配布場所を検索
        /// </summary>
        public async Task SearchLocations()
        {
            IsLoading = true;
            try
            {
                if (string.IsNullOrEmpty(SearchText))
                {
                    await LoadDistributionLocations();
                }
                else
                {
                    var allLocations = await _databaseService.GetAllDistributionLocationsWithCardsAsync();
                    var filtered = allLocations
                        .Where(l => l.LocationName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                   (l.Address?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                        .OrderBy(l => l.LocationName)
                        .ToList();

                    Locations.Clear();
                    foreach (var location in filtered)
                    {
                        Locations.Add(location);
                    }
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
