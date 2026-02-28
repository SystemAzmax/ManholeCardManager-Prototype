using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ManholeCardManager.Services
{
    /// <summary>
    /// ローカライゼーション管理サービス
    /// </summary>
    public class LocalizationService : INotifyPropertyChanged
    {
        private static LocalizationService? _instance;
        private string _currentLanguage = "ja-JP";
        private Dictionary<string, Dictionary<string, string>> _resources;

        /// <summary>
        /// サポートする言語のリスト
        /// </summary>
        public List<LanguageInfo> SupportedLanguages { get; }

        /// <summary>   
        /// 現在の言語
        /// </summary>
        public string CurrentLanguage
        {
            get => _currentLanguage;
            set
            {
                if (_currentLanguage != value)
                {
                    _currentLanguage = value;
                    OnPropertyChanged(nameof(CurrentLanguage));
                }
            }
        }

        /// <summary>
        /// シングルトンインスタンス取得
        /// </summary>
        public static LocalizationService Instance
        {
            get
            {
                _instance ??= new LocalizationService();
                return _instance;
            }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        private LocalizationService()
        {
            SupportedLanguages = new List<LanguageInfo>
            {
                new LanguageInfo { LanguageCode = "ja-JP", LanguageName = "日本語" },
                new LanguageInfo { LanguageCode = "en-US", LanguageName = "English" }
            };

            // リソース辞書を初期化
            _resources = new Dictionary<string, Dictionary<string, string>>
            {
                ["ja-JP"] = new Dictionary<string, string>
                {
                    ["AppTitle"] = "マンホールカード取得管理",
                    ["AppDescription"] = "マンホールカード取得状況を管理するアプリケーション",
                    ["CardSearch"] = "カード検索",
                    ["SearchPlaceholder"] = "カード名で検索...",
                    ["Category"] = "カテゴリ",
                    ["AcquisitionCount"] = "取得カード数:",
                    ["AddNewCard"] = "新規追加",
                    ["DeleteButton"] = "削除",
                    ["DeleteConfirmationTitle"] = "削除確認",
                    ["DeleteConfirmationMessage"] = "「{0}」を削除してもよろしいですか？",
                    ["DeleteConfirm"] = "削除",
                    ["Cancel"] = "キャンセル",
                    ["Language"] = "言語",
                    ["SelectLanguage"] = "言語を選択",
                    ["Settings"] = "設定",
                    ["NewCard"] = "新しいカード",
                    ["CardDescription"] = "説明を入力してください",
                    ["Processing"] = "処理中...",
                    ["DistributionLocations"] = "配布場所一覧",
                    ["DistributedCards"] = "配布カード:",
                    ["AcquisitionStatus"] = "取得状況",
                    ["Acquired"] = "◯",
                    ["NotAcquired"] = "　"
                },
                ["en-US"] = new Dictionary<string, string>
                {
                    ["AppTitle"] = "Manhole Card Acquisition Manager",
                    ["AppDescription"] = "An application to manage your manhole card acquisition status",
                    ["CardSearch"] = "Card Search",
                    ["SearchPlaceholder"] = "Search by card name...",
                    ["Category"] = "Category",
                    ["AcquisitionCount"] = "Acquired Cards:",
                    ["AddNewCard"] = "Add New",
                    ["DeleteButton"] = "Delete",
                    ["DeleteConfirmationTitle"] = "Delete Confirmation",
                    ["DeleteConfirmationMessage"] = "Are you sure you want to delete \"{0}\"?",
                    ["DeleteConfirm"] = "Delete",
                    ["Cancel"] = "Cancel",
                    ["Language"] = "Language",
                    ["SelectLanguage"] = "Select Language",
                    ["Settings"] = "Settings",
                    ["NewCard"] = "New Card",
                    ["CardDescription"] = "Enter a description",
                    ["Processing"] = "Processing...",
                    ["DistributionLocations"] = "Distribution Locations",
                    ["DistributedCards"] = "Distributed Cards:",
                    ["AcquisitionStatus"] = "Acquisition Status",
                    ["Acquired"] = "◯",
                    ["NotAcquired"] = "　"
                }
            };

            System.Diagnostics.Debug.WriteLine($"LocalizationService initialized with {_resources[_currentLanguage].Count} strings");
        }

        /// <summary>
        /// リソース文字列を取得
        /// </summary>
        public string GetString(string resourceKey)
        {
            try
            {
                if (_resources.ContainsKey(_currentLanguage) && 
                    _resources[_currentLanguage].ContainsKey(resourceKey))
                {
                    var result = _resources[_currentLanguage][resourceKey];
                    System.Diagnostics.Debug.WriteLine($"GetString('{resourceKey}') = '{result}'");
                    return result;
                }
                
                System.Diagnostics.Debug.WriteLine($"Resource key not found: {resourceKey}");
                return resourceKey;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting resource '{resourceKey}': {ex.Message}");
                return resourceKey;
            }
        }

        /// <summary>
        /// 言語を変更
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            if (CurrentLanguage != languageCode)
            {
                CurrentLanguage = languageCode;
                OnPropertyChanged(nameof(CurrentLanguage));
                ReloadResources();
            }
        }

        /// <summary>
        /// 全てのリソース文字列をリロード（言語変更時に呼び出し）
        /// </summary>
        public void ReloadResources()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AllResources"));
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

    /// <summary>
    /// 言語情報
    /// </summary>
    public class LanguageInfo
    {
        /// <summary>
        /// 言語コード（ja-JP, en-US等）
        /// </summary>
        public string LanguageCode { get; set; } = string.Empty;

        /// <summary>
        /// 言語の表示名
        /// </summary>
        public string LanguageName { get; set; } = string.Empty;
    }
}
