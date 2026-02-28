using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Extensions.Logging;
using ManholeCardManager.Services;
using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ManholeCardManager
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;
        private static ILoggerFactory? _loggerFactory;

        /// <summary>
        /// ロガーファクトリを取得
        /// </summary>
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_loggerFactory == null)
                {
                    InitializeLogging();
                }
                return _loggerFactory!;
            }
        }

        /// <summary>
        /// ロギングを初期化（コンソール + ファイル出力）
        /// </summary>
        private static void InitializeLogging()
        {
            // ログディレクトリパスを取得
            var logDirectory = GetLogDirectory();

            // ロガーファクトリを作成
            var factory = new LoggerFactory();

            // ファイル出力を追加（メインの出力）
            factory.AddProvider(new FileLoggerProvider(logDirectory));

            _loggerFactory = factory;
        }

        /// <summary>
        /// ログディレクトリパスを取得
        /// </summary>
        private static string GetLogDirectory()
        {
            try
            {
                // UWP環境の場合はApplicationData.Currentを使用
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                var logsPath = Path.Combine(localFolder, "logs");
                return logsPath;
            }
            catch (InvalidOperationException)
            {
                // UWP以外の環境の場合はLocalApplicationDataを使用
                var localAppData = Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData);
                var appFolder = Path.Combine(localAppData, "ManholeCardManager");
                var logsPath = Path.Combine(appFolder, "logs");
                return logsPath;
            }
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            _window.Activate();
        }
    }
}
