using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ManholeCardManager.Models;
using ManholeCardManager.Services;
using ManholeCardManager.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ManholeCardManager
{
    /// <summary>
    /// メインウィンドウ
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;
        private DatabaseService? _databaseService;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            
            // ローカライズされたタイトルを設定
            var locService = LocalizationService.Instance;
            Title = locService.GetString("AppTitle");
            System.Diagnostics.Debug.WriteLine($"Window title set to: {Title}");
            
            this.Activated += MainWindow_Activated;
        }

        private bool _isInitialized = false;

        /// <summary>
        /// ウィンドウアクティブ時の初期化
        /// </summary>
        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs e)
        {
            if (_isInitialized) return;
            _isInitialized = true;

            try
            {
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing: {ex}");

                var dialog = new ContentDialog
                {
                    Title = "Error initializing database",
                    Content = ex.Message,
                    CloseButtonText = "OK",
                    XamlRoot = this.Content.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// 非同期初期化
        /// </summary>
        private async Task InitializeAsync()
        {
            _databaseService = new DatabaseService();
            await _databaseService.InitializeAsync();

            _viewModel = new MainWindowViewModel();

            // FrameworkElementのDataContextプロパティを設定
            (this.Content as FrameworkElement)!.DataContext = _viewModel;
            await _viewModel.InitializeAppAsync();

            UpdateUIText();
            LanguageComboBox.SelectedValue = _viewModel.LocalizationService.CurrentLanguage;
        }

        /// <summary>
        /// UI テキストを更新（言語切り替え時）
        /// </summary>
        private void UpdateUIText()
        {
            var locService = LocalizationService.Instance;

            Title = locService.GetString("AppTitle");
            AppTitleBlock.Text = locService.GetString("AppTitle");
            LanguageLabelBlock.Text = locService.GetString("Language");
            LoadingTip.Title = locService.GetString("Processing");
        }

        /// <summary>
        /// 新規カード追加ボタンクリック
        /// </summary>
        private async void AddCardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_databaseService == null) return;

            var newCard = new ManholeCard();

            if (_viewModel?.CardCollectionViewModel != null)
            {
                await _viewModel.CardCollectionViewModel.AddCardAsync(newCard);
            }
        }

        /// <summary>
        /// カード削除ボタンクリック
        /// </summary>
        private async void DeleteCardButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var card = button?.DataContext as ManholeCard;
            if (card == null || _databaseService == null) return;

            var deleteTitle = string.Empty;
            var location = await _databaseService.GetCardLocationAsync(card.CardId);
            
            if (location != null)
            {
                if (!string.IsNullOrEmpty(location.Prefecture))
                {
                    deleteTitle = location.Prefecture;
                }

                if (!string.IsNullOrEmpty(location.Municipality))
                {
                    deleteTitle = string.IsNullOrEmpty(deleteTitle)
                        ? location.Municipality
                        : $"{deleteTitle} {location.Municipality}";
                }
            }

            if (string.IsNullOrEmpty(deleteTitle))
            {
                deleteTitle = $"ID:{card.CardId}";
            }

            var deleteMessage = string.Format(
                LocalizationService.Instance.GetString("DeleteConfirmationMessage"),
                deleteTitle);

            var dialog = new ContentDialog
            {
                Title = LocalizationService.Instance.GetString("DeleteConfirmationTitle"),
                Content = deleteMessage,
                PrimaryButtonText = LocalizationService.Instance.GetString("DeleteConfirm"),
                CloseButtonText = LocalizationService.Instance.GetString("Cancel"),
                DefaultButton = ContentDialogButton.Close
            };

            if (this.Content is FrameworkElement root)
            {
                dialog.XamlRoot = root.XamlRoot;
            }

            var asyncOp = dialog.ShowAsync();
            asyncOp.AsTask().ContinueWith(async task =>
            {
                var result = await task;
                if (result == ContentDialogResult.Primary)
                {
                    if (_viewModel?.CardCollectionViewModel != null)
                    {
                        await _viewModel.CardCollectionViewModel.DeleteCardAsync(card);
                    }
                }
            });
        }

        /// <summary>
        /// 言語選択コンボボックス変更イベント
        /// </summary>
        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedValue is string languageCode)
            {
                LocalizationService.Instance.SetLanguage(languageCode);
                UpdateUIText();

                if (_viewModel?.CardCollectionViewModel?.Cards != null)
                {
                    _viewModel.CardCollectionViewModel.ReloadCards();
                }
            }
        }

        /// <summary>
        /// 在庫状況リンククリックイベント
        /// </summary>
        private async void StockStatusLink_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var url = button?.Tag as string;

                System.Diagnostics.Debug.WriteLine(
                    $"StockStatusLink_Click: URL = {url}");

                if (string.IsNullOrEmpty(url))
                {
                    System.Diagnostics.Debug.WriteLine(
                        "StockStatusLink_Click: URL is empty or null");
                    return;
                }

                // URLが正しい形式か確認
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"StockStatusLink_Click: Invalid URI format: {url}");
                    return;
                }

                System.Diagnostics.Debug.WriteLine(
                    $"StockStatusLink_Click: Opening URL: {uri}");

                var result = await Windows.System.Launcher.LaunchUriAsync(uri);
                System.Diagnostics.Debug.WriteLine(
                    $"StockStatusLink_Click: Launch result = {result}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Error opening URL: {ex}");
            }
        }
    }
}
