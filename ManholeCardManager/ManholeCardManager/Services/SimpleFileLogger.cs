using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace ManholeCardManager.Services
{
    /// <summary>
    /// ファイルにログを出力するシンプルなロガー実装
    /// </summary>
    public class SimpleFileLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly string _logDirectory;
        private readonly object _lockObject = new object();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="categoryName">ロガーのカテゴリ名</param>
        /// <param name="logDirectory">ログディレクトリパス</param>
        public SimpleFileLogger(string categoryName, string logDirectory)
        {
            _categoryName = categoryName;
            _logDirectory = logDirectory;

            // ログディレクトリが存在しない場合は作成
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        /// <summary>
        /// ログレベルが有効かどうかを判定
        /// </summary>
        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        /// <summary>
        /// スコープを開始（このロガーでは未使用）
        /// </summary>
        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return new NoOpDisposable();
        }

        /// <summary>
        /// ログを出力
        /// </summary>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            try
            {
                var message = formatter(state, exception);
                var logEntry = FormatLogEntry(logLevel, _categoryName, message, exception);

                WriteToFile(logEntry);
            }
            catch
            {
                // ファイル書き込みエラーは無視（例外を発生させない）
            }
        }

        /// <summary>
        /// ログエントリをフォーマット
        /// </summary>
        private string FormatLogEntry(
            LogLevel logLevel,
            string categoryName,
            string message,
            Exception? exception)
        {
            var sb = new StringBuilder();
            sb.Append('[');
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.Append("] [");
            sb.Append(logLevel.ToString().ToUpperInvariant());
            sb.Append("] [");
            sb.Append(categoryName);
            sb.Append("] ");
            sb.Append(message);

            if (exception != null)
            {
                sb.AppendLine();
                sb.AppendLine("Exception:");
                sb.Append(exception);
            }

            return sb.ToString();
        }

        /// <summary>
        /// ファイルにログを書き込み
        /// </summary>
        private void WriteToFile(string logEntry)
        {
            lock (_lockObject)
            {
                var logFileName = $"app-{DateTime.Now:yyyy-MM-dd}.log";
                var logFilePath = Path.Combine(_logDirectory, logFileName);

                try
                {
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                }
                catch
                {
                    // ファイル書き込みエラーは無視
                }
            }
        }

        /// <summary>
        /// スコープ用のダミー Disposable
        /// </summary>
        private class NoOpDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}
