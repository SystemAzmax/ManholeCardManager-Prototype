using ManholeCardDataScraper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace ManholeCardDataScraper.Services
{
    /// <summary>
    /// スクレイピングデータをCSVに保存するサービス
    /// </summary>
    public class DatabaseSyncService
    {
        private readonly string _outputPath;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public DatabaseSyncService(string outputPath)
        {
            _outputPath = outputPath;
        }

        /// <summary>
        /// 配布場所情報をCSVに保存
        /// </summary>
        /// <param name="scrapedLocations">スクレイピングで取得した場所情報</param>
        /// <returns>保存された件数</returns>
        public async Task<int> SyncDistributionLocationsAsync(
            List<DistributionLocationData> scrapedLocations)
        {
            try
            {
                Console.WriteLine($"Saving {scrapedLocations.Count} locations to CSV...");
                Console.WriteLine($"Output path: {_outputPath}");

                var directory = Path.GetDirectoryName(_outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Console.WriteLine($"Created directory: {directory}");
                }

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Encoding = Encoding.UTF8
                };

                await using var writer = new StreamWriter(_outputPath, false, Encoding.UTF8);
                await using var csv = new CsvWriter(writer, config);

                // ヘッダーを書き込み
                csv.WriteField("LocationName");
                csv.WriteField("Description");
                csv.WriteField("CardImageUrl");
                csv.WriteField("IssueDate");
                csv.WriteField("DistributionLocation");
                csv.WriteField("DistributionHours");
                csv.WriteField("StockStatusUrl");
                csv.WriteField("FetchedDate");
                await csv.NextRecordAsync();

                // データを書き込み
                foreach (var location in scrapedLocations)
                {
                    csv.WriteField(location.LocationName);
                    csv.WriteField(location.Description);
                    csv.WriteField(location.CardImageUrl);
                    csv.WriteField(location.IssueDate);
                    csv.WriteField(location.DistributionLocation);
                    csv.WriteField(location.DistributionHours);
                    csv.WriteField(location.StockStatusUrl);
                    csv.WriteField(location.FetchedDate.ToString("yyyy-MM-dd HH:mm:ss"));
                    await csv.NextRecordAsync();
                }

                Console.WriteLine($"Successfully saved {scrapedLocations.Count} locations to CSV");
                return scrapedLocations.Count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving to CSV: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
