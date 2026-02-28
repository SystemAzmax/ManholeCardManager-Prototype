using ManholeCardManager.Models;
using ManholeCardManager.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ManholeCardDataScraper.Services
{
    /// <summary>
    /// マンホールカード配布場所をスクレイピング・CSVに保存するメインサービス
    /// </summary>
    public class LocationScraperService
    {
        private readonly WebScraperService _scraperService;
        private readonly string _outputCsvPath;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public LocationScraperService()
        {
            _scraperService = new WebScraperService();
            
            // 出力先のパスを設定
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var outputDir = Path.Combine(localAppData, "ManholeCardManager", "ScrapedData");
            _outputCsvPath = Path.Combine(outputDir, "distribution_locations.csv");
        }

        /// <summary>
        /// サービスを初期化
        /// </summary>
        public Task InitializeAsync()
        {
            Console.WriteLine("Service initialized");
            Console.WriteLine($"Output CSV path: {_outputCsvPath}");
            return Task.CompletedTask;
        }

        /// <summary>
        /// スクレイピングとCSV保存を実行
        /// </summary>
        /// <returns>保存された配布場所の件数</returns>
        public async Task<int> ExecuteAsync()
        {
            try
            {
                Console.WriteLine("Starting scraping operation...");
                var locations = 
                    await _scraperService.ScrapeDistributionLocationsAsync();

                if (locations.Count == 0)
                {
                    Console.WriteLine("No locations scraped");
                    return 0;
                }

                Console.WriteLine($"Scraped {locations.Count} locations");

                var syncService = new DatabaseSyncService(_outputCsvPath);
                var savedCount = 
                    await syncService.SyncDistributionLocationsAsync(locations);

                Console.WriteLine($"Operation completed. Saved: {savedCount} locations");
                Console.WriteLine($"CSV file location: {_outputCsvPath}");
                
                return savedCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during execution: {ex.Message}");
                throw;
            }
            finally
            {
                _scraperService.Dispose();
            }
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
