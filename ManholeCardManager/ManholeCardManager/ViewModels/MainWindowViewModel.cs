using ManholeCardManager.Models;
using ManholeCardManager.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace ManholeCardManager.ViewModels
{
    /// <summary>
    /// メインウィンドウのビューモデル
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly LocalizationService _localizationService;
        private CardCollectionViewModel? _cardCollectionViewModel;
        private DistributionLocationViewModel? _distributionLocationViewModel;
        private bool _isInitialized;
        private bool _isLoading;
        private DatabaseService _databaseService = null!;

        /// <summary>
        /// カードコレクションビューモデル
        /// </summary>
        public CardCollectionViewModel? CardCollectionViewModel
        {
            get => _cardCollectionViewModel;
            set
            {
                if (_cardCollectionViewModel != value)
                {
                    _cardCollectionViewModel = value;
                    OnPropertyChanged(nameof(CardCollectionViewModel));
                }
            }
        }

        /// <summary>
        /// 配布場所ビューモデル
        /// </summary>
        public DistributionLocationViewModel? DistributionLocationViewModel
        {
            get => _distributionLocationViewModel;
            set
            {
                if (_distributionLocationViewModel != value)
                {
                    _distributionLocationViewModel = value;
                    OnPropertyChanged(nameof(DistributionLocationViewModel));
                }
            }
        }

        /// <summary>
        /// ローカライゼーションサービス
        /// </summary>
        public LocalizationService LocalizationService => _localizationService;

        /// <summary>
        /// 初期化済みフラグ
        /// </summary>
        public bool IsInitialized
        {
            get => _isInitialized;
            set
            {
                if (_isInitialized != value)
                {
                    _isInitialized = value;
                    OnPropertyChanged(nameof(IsInitialized));
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
        public MainWindowViewModel()
        {
            _localizationService = LocalizationService.Instance;
            _localizationService.PropertyChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(LocalizationService));
            };
        }

        /// <summary>
        /// アプリケーションを初期化
        /// </summary>
        public async Task InitializeAppAsync()
        {
            IsLoading = true;
            try
            {
                _databaseService = new DatabaseService();
                await _databaseService.InitializeAsync();

                // データベースが空の場合、サンプルデータを挿入
                await _databaseService.InsertSampleDataAsync();

                CardCollectionViewModel = new CardCollectionViewModel(_databaseService);
                await CardCollectionViewModel.InitializeAsync();

                DistributionLocationViewModel = new DistributionLocationViewModel(_databaseService);
                await DistributionLocationViewModel.InitializeAsync();

                IsInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Initialization error: {ex.Message}");
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
