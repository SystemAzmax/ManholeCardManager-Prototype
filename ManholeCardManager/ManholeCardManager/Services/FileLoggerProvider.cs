using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.IO;

namespace ManholeCardManager.Services
{
    /// <summary>
    /// ファイルログ出力用のロガープロバイダー
    /// </summary>
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _logDirectory;
        private readonly ConcurrentDictionary<string, SimpleFileLogger> _loggers
            = new ConcurrentDictionary<string, SimpleFileLogger>();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logDirectory">ログを保存するディレクトリパス</param>
        public FileLoggerProvider(string logDirectory)
        {
            _logDirectory = logDirectory ?? 
                throw new ArgumentNullException(nameof(logDirectory));
        }

        /// <summary>
        /// ロガーインスタンスを作成
        /// </summary>
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(
                categoryName,
                name => new SimpleFileLogger(name, _logDirectory));
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            _loggers.Clear();
        }
    }
}
