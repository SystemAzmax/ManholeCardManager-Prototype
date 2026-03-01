using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ManholeCardManager.Services
{
    /// <summary>
    /// 画像のダウンロードとキャッシュを管理するサービスクラス
    /// </summary>
    public class ImageCacheService
    {
        private const string CACHE_FOLDER_NAME = "ImageCache";
        private const string DUMMY_IMAGE_NAME = "dummy_card.png";
        
        /// <summary>
        /// HttpClientのタイムアウト時間（秒）
        /// </summary>
        private const int HTTP_CLIENT_TIMEOUT_SECONDS = 30;
        
        private readonly string _cacheDirectory;
        private readonly string _dummyImagePath;
        
        /// <summary>
        /// 共有HttpClientインスタンス（シングルトン）
        /// スレッドセーフな静的フィールドで、アプリケーション全体で共有される。
        /// HttpClientは再利用することでコネクションプールとDNSキャッシュを活用し、
        /// パフォーマンスとメモリ効率を向上させる。
        /// 
        /// 参考: https://docs.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient
        /// </summary>
        private static readonly HttpClient _httpClient = new HttpClient 
        { 
            Timeout = TimeSpan.FromSeconds(HTTP_CLIENT_TIMEOUT_SECONDS)
        };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ImageCacheService()
        {
            try
            {
                var localFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                _cacheDirectory = Path.Combine(localFolder, CACHE_FOLDER_NAME);
            }
            catch (InvalidOperationException)
            {
                // Windows.Storage.ApplicationData にアクセスできない場合（UWP以外の環境など）
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var appFolder = Path.Combine(localAppData, "ManholeCardManager");
                _cacheDirectory = Path.Combine(appFolder, CACHE_FOLDER_NAME);
            }

            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }

            _dummyImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", DUMMY_IMAGE_NAME);
        }

        /// <summary>
        /// 画像を取得（キャッシュまたはダウンロード）
        /// </summary>
        /// <param name="imageUrl">画像URL</param>
        /// <returns>ローカルファイルパス</returns>
        public async Task<string> GetImageAsync(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
            {
                return _dummyImagePath;
            }

            try
            {
                var fileName = GetCacheFileName(imageUrl);
                var cachedPath = Path.Combine(_cacheDirectory, fileName);

                if (File.Exists(cachedPath))
                {
                    return cachedPath;
                }

                if (Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri))
                {
                    var imageData = await _httpClient.GetByteArrayAsync(uri);
                    await File.WriteAllBytesAsync(cachedPath, imageData);
                    return cachedPath;
                }

                return _dummyImagePath;
            }
            catch (HttpRequestException)
            {
                // ネットワークエラーやHTTPエラーの場合はダミー画像を返す
                return _dummyImagePath;
            }
            catch (IOException)
            {
                // ファイルの読み書きエラーの場合はダミー画像を返す
                return _dummyImagePath;
            }
            catch (OperationCanceledException)
            {
                // タイムアウト時はダミー画像を返す
                return _dummyImagePath;
            }
        }

        /// <summary>
        /// URLからキャッシュファイル名を生成
        /// </summary>
        /// <param name="url">画像URL</param>
        /// <returns>ファイル名</returns>
        private string GetCacheFileName(string url)
        {
            var hash = url.GetHashCode().ToString("X8");
            var extension = Path.GetExtension(url);
            if (string.IsNullOrEmpty(extension))
            {
                extension = ".png";
            }
            return $"{hash}{extension}";
        }

        /// <summary>
        /// キャッシュをクリア
        /// </summary>
        public void ClearCache()
        {
            try
            {
                if (Directory.Exists(_cacheDirectory))
                {
                    foreach (var file in Directory.GetFiles(_cacheDirectory))
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (IOException)
            {
                // ファイルが使用中またはアクセス不可の場合は無視
            }
            catch (UnauthorizedAccessException)
            {
                // アクセス権限がない場合は無視
            }
        }

        /// <summary>
        /// ダミー画像パスを取得
        /// </summary>
        /// <returns>ダミー画像のパス</returns>
        public string GetDummyImagePath()
        {
            return _dummyImagePath;
        }
    }
}
