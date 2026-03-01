using ManholeCardManager.Models;
using ManholeCardManager.Services;
using ManholeCardManager.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ManholeCardManager
{
    /// <summary>
    /// メインウィンドウ
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private MainWindowViewModel? _viewModel;
        private DatabaseService? _databaseService;
        private readonly ILogger _logger = App.LoggerFactory.CreateLogger("MainWindow");

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // ウィンドウアイコンを設定
            SetWindowIcon();

            // ローカライズされたタイトルを設定
            var locService = LocalizationService.Instance;
            Title = locService.GetString("AppTitle");
            
            this.Activated += MainWindow_Activated;
        }

        /// <summary>
        /// ウィンドウアイコンを設定
        /// </summary>
        private void SetWindowIcon()
        {
            try
            {
                IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                // シンプルにAppIcon.icoを設定
                appWindow.SetIcon("Assets\\AppIcon.ico");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ウィンドウアイコンの設定に失敗しました。");
            }
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
                _logger.LogError(ex, "ウィンドウの初期化に失敗しました。");
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
            var logger = App.LoggerFactory.CreateLogger<DatabaseService>();
            _databaseService = new DatabaseService(logger);
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
            SeriesNumberFilterComboBox.PlaceholderText = locService.GetString("SeriesNumberFilter");
            DistributionLocationsLabel.Text = locService.GetString("DistributionLocations");
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

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                if (_viewModel?.CardCollectionViewModel != null)
                {
                    await _viewModel.CardCollectionViewModel.DeleteCardAsync(card);
                }
            }
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
        /// 弾数フィルタコンボボックス変更イベント
        /// </summary>
        private void SeriesNumberFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        /// <summary>
        /// 弾数フィルタコンボボックスドロップダウン開始イベント。
        /// ドロップダウンのスクロールを最下部に移動します。
        /// </summary>
        private async void SeriesNumberFilterComboBox_DropDownOpened(
            object sender, object e)
        {
            if (sender is not ComboBox comboBox
                || comboBox.Items.Count <= 0
                || comboBox.XamlRoot == null)
                return;

            // Popupのレンダリング完了を待つ
            await Task.Delay(50);

            DispatcherQueue.TryEnqueue(() =>
            {
                try
                {
                    ScrollToBottomOfDropDown(comboBox);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "ドロップダウンのスクロールに失敗しました。");
                }
            });
        }

        /// <summary>
        /// ComboBoxのドロップダウンPopup内の
        /// ScrollViewerを最下部へスクロールします。
        /// </summary>
        /// <param name="comboBox">
        /// 対象のComboBox
        /// </param>
        private static void ScrollToBottomOfDropDown(
            ComboBox comboBox)
        {
            var popups = VisualTreeHelper
                .GetOpenPopupsForXamlRoot(comboBox.XamlRoot);

            foreach (var popup in popups)
            {
                var sv = FindDescendant<ScrollViewer>(
                    popup.Child);
                if (sv != null)
                {
                    sv.ChangeView(
                        null,
                        sv.ScrollableHeight,
                        null,
                        false);
                    return;
                }
            }
        }

        /// <summary>
        /// ビジュアルツリーから指定型の子孫要素を探します。
        /// </summary>
        /// <typeparam name="T">探索対象の型</typeparam>
        /// <param name="parent">探索開始の親要素</param>
        /// <returns>
        /// 見つかった要素。見つからない場合はnull。
        /// </returns>
        private static T? FindDescendant<T>(
            DependencyObject parent) where T : DependencyObject
        {
            var count = VisualTreeHelper
                .GetChildrenCount(parent);

            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper
                    .GetChild(parent, i);

                if (child is T found)
                {
                    return found;
                }

                var result = FindDescendant<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
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

                if (string.IsNullOrEmpty(url))
                {
                    return;
                }

                // URLが正しい形式か確認
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    return;
                }

                var result = await Windows.System.Launcher.LaunchUriAsync(uri);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "URLを開く際にエラーが発生しました。");
            }
        }

        /// <summary>
        /// 取得ステータスクリックイベント
        /// </summary>
        private async void AcquisitionStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var card = button?.DataContext as CardWithAcquisitionStatus;
                
                if (card == null)
                {
                    return;
                }

                if (_viewModel?.DistributionLocationViewModel == null)
                {
                    return;
                }

                // 配布場所を特定
                var locationWithCards = _viewModel.DistributionLocationViewModel.Locations
                    .FirstOrDefault(l => l.DistributedCards.Any(c => c.CardId == card.CardId));

                if (locationWithCards != null)
                {
                    await _viewModel.DistributionLocationViewModel.ToggleCardAcquisitionAsync(
                        card.CardId, locationWithCards.LocationId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "カード取得ステータスの切り替えに失敗しました。");
            }
        }

        /// <summary>
        /// 取得日時ピッカーボタンクリックイベント
        /// </summary>
        private async void AcquisitionDatePicker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var card = button?.DataContext as CardWithAcquisitionStatus;
                if (card == null) return;

                var contentDialog = new ContentDialog
                {
                    Title = "取得日時を選択してください",
                    PrimaryButtonText = "OK",
                    CloseButtonText = "キャンセル",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.Content.XamlRoot
                };

                var stackPanel = new StackPanel { Spacing = 10 };
                var now = DateTimeOffset.Now;

                var datePickerLabel = new TextBlock
                {
                    Text = "日付",
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    FontSize = 12
                };
                var datePicker = new CalendarDatePicker
                {
                    Date = card.AcquisitionDate?.DateTime.Date ?? now.Date
                };

                var timePickerLabel = new TextBlock
                {
                    Text = "時刻",
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    FontSize = 12,
                    Margin = new Thickness(0, 10, 0, 0)
                };
                var timePicker = new TimePicker
                {
                    Time = card.AcquisitionDate?.DateTime.TimeOfDay ?? now.TimeOfDay,
                    ClockIdentifier = "24HourClock"
                };

                stackPanel.Children.Add(datePickerLabel);
                stackPanel.Children.Add(datePicker);
                stackPanel.Children.Add(timePickerLabel);
                stackPanel.Children.Add(timePicker);

                contentDialog.Content = stackPanel;

                var result = await contentDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    if (datePicker.Date.HasValue)
                    {
                        var selectedDate = datePicker.Date.Value.DateTime.Date;
                        var selectedTime = timePicker.Time;
                        var combinedDateTime = selectedDate.Add(selectedTime);

                        // ISO 8601 拡張形式のタイムゾーン付き日時として設定
                        var dateTimeOffset = new DateTimeOffset(
                            combinedDateTime,
                            TimeZoneInfo.Local.GetUtcOffset(combinedDateTime));

                        card.AcquisitionDate = dateTimeOffset;

                        // 未保存状態に設定（黒丸を表示）
                        card.SaveStatus = "unsaved";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得日時ピッカーの処理に失敗しました。");
            }
        }

        /// <summary>
        /// 取得日時と備考を保存するボタンクリックイベント
        /// </summary>
        private async void SaveAcquisitionData_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var card = button?.DataContext as CardWithAcquisitionStatus;
            if (card == null || _databaseService == null) return;

            try
            {
                // 保存中状態に変更
                card.SaveStatus = "saving";

                // テキストボックスから DateTimeOffset をパース（必要に応じて）
                var acquisitionDate = card.AcquisitionDate;
                if (!string.IsNullOrEmpty(card.AcquisitionDateDisplay))
                {
                    if (DateTimeOffset.TryParse(card.AcquisitionDateDisplay, out var parsedDate))
                    {
                        acquisitionDate = parsedDate;
                    }
                }

                // データベースに保存
                await _databaseService.UpdateAcquisitionHistoryAsync(
                    card.CardId,
                    acquisitionDate,
                    card.Notes);

                // 保存成功状態に変更
                card.SaveStatus = "success";

                // 2秒後に未編集状態に戻す
                _ = ResetSaveStatusAsync(card, 2000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得日時データの保存に失敗しました。CardId={CardId}", card.CardId);
                // 失敗状態に変更（バツが消えない）
                card.SaveStatus = "error";
            }
        }

        /// <summary>
        /// 保存ステータスをリセット（未編集状態に戻す）
        /// </summary>
        private async Task ResetSaveStatusAsync(CardWithAcquisitionStatus card, int delayMilliseconds)
        {
            await Task.Delay(delayMilliseconds);
            card.SaveStatus = "none";
        }

        /// <summary>
        /// 取得日時テキストボックス変更イベント
        /// </summary>
        private void AcquisitionDateTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            try
            {
                if (sender is TextBox textBox &&
                    textBox.DataContext is CardWithAcquisitionStatus card)
                {
                    // 保存中以外の場合、未保存状態に設定（黒丸を表示）
                    // 失敗状態でも編集時は黒丸が優先表示される
                    if (card.SaveStatus != "saving")
                    {
                        card.SaveStatus = "unsaved";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "取得日時テキストボックスの変更処理に失敗しました。");
            }
        }

        /// <summary>
        /// 備考テキストボックス変更イベント
        /// </summary>
        private void NotesTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            try
            {
                if (sender is TextBox textBox &&
                    textBox.DataContext is CardWithAcquisitionStatus card)
                {
                    // 保存中以外の場合、未保存状態に設定（黒丸を表示）
                    // 失敗状態でも編集時は黒丸が優先表示される
                    if (card.SaveStatus != "saving")
                    {
                        card.SaveStatus = "unsaved";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "備考テキストボックスの変更処理に失敗しました。");
            }
        }
    }
}
