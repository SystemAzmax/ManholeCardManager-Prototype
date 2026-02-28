using HtmlAgilityPack;
using ManholeCardDataScraper.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ManholeCardDataScraper.Services
{
    /// <summary>
    /// Webスクレイピングを行うサービス
    /// </summary>
    public class WebScraperService
    {
        private readonly HttpClient _httpClient;
        private const string TARGET_URL = 
            "https://www.gk-p.jp/mhcard/?pref=zenkoku#mhcard_result";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public WebScraperService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        }

        /// <summary>
        /// マンホールカード配布場所情報をスクレイピング
        /// </summary>
        /// <returns>配布場所情報のリスト</returns>
        public async Task<List<DistributionLocationData>> 
            ScrapeDistributionLocationsAsync()
        {
            try
            {
                Console.WriteLine("Fetching data from: " + TARGET_URL);
                var html = await _httpClient.GetStringAsync(TARGET_URL);

                Console.WriteLine($"HTML length: {html.Length} characters");

                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var locations = new List<DistributionLocationData>();

                // より詳細なセレクタでテーブルを探す
                var tables = doc.DocumentNode.SelectNodes("//table");
                Console.WriteLine($"Found {tables?.Count ?? 0} table(s)");

                if (tables == null || tables.Count == 0)
                {
                    Console.WriteLine("Warning: No tables found in HTML");
                    
                    // HTMLの一部を出力してデバッグ
                    Console.WriteLine("HTML snippet (first 500 chars):");
                    Console.WriteLine(html.Substring(0, Math.Min(500, html.Length)));
                    
                    return locations;
                }

                // 各テーブルの行をチェック
                foreach (var table in tables)
                {
                    var rows = table.SelectNodes(".//tr");
                    if (rows == null || rows.Count <= 1)
                        continue;

                    Console.WriteLine($"Processing table with {rows.Count} rows");

                    // ヘッダー行をスキップ
                    foreach (var row in rows.Skip(1))
                    {
                        var cells = row.SelectNodes(".//td");
                        if (cells == null || cells.Count == 0)
                            continue;

                        var location = ParseLocationFromRow(cells);
                        if (location != null)
                        {
                            locations.Add(location);
                            Console.WriteLine($"  Parsed: {location.LocationName}");
                        }
                    }
                }

                Console.WriteLine($"Scraped {locations.Count} locations");
                return locations;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping locations: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// テーブル行からロケーション情報を抽出
        /// </summary>
        private DistributionLocationData? ParseLocationFromRow(
            HtmlNodeCollection cells)
        {
            try
            {
                Console.WriteLine($"    Parsing row with {cells.Count} cells");

                if (cells.Count < 2)
                {
                    Console.WriteLine("    Skipped: Not enough cells");
                    return null;
                }

                // 各セルから情報を抽出（td要素の順序に従って取得）
                var locationName = cells.Count > 0 
                    ? cells[0].InnerText.Trim() 
                    : string.Empty;

                // cells[1]はAddress（使用しない）

                // 弾数（Description）
                var description = cells.Count > 2
                    ? cells[2].InnerText.Trim()
                    : string.Empty;

                // 画像URLを取得（行全体からimg要素を探す）
                var cardImageUrl = string.Empty;
                foreach (var cell in cells)
                {
                    var imgNode = cell.SelectSingleNode(".//img");
                    if (imgNode != null)
                    {
                        cardImageUrl = imgNode.GetAttributeValue("src", string.Empty);
                        if (!string.IsNullOrEmpty(cardImageUrl))
                        {
                            Console.WriteLine($"    Found image in cell: {cardImageUrl}");
                            // 相対URLの場合は絶対URLに変換
                            if (!cardImageUrl.StartsWith("http"))
                            {
                                var baseUri = new Uri(TARGET_URL);
                                cardImageUrl = new Uri(baseUri, cardImageUrl).ToString();
                            }
                            break;
                        }
                    }
                }

                // 発行年月日を取得
                var issueDate = cells.Count > 3
                    ? cells[3].InnerText.Trim()
                    : string.Empty;

                // 配布場所を取得
                var distributionLocation = cells.Count > 4
                    ? cells[4].InnerText.Trim()
                    : string.Empty;

                // 配布時間を取得
                var distributionHours = cells.Count > 5
                    ? cells[5].InnerText.Trim()
                    : string.Empty;

                // 在庫状況リンクURLを取得（行全体から「こちら」リンクを探す）
                var stockStatusUrl = string.Empty;
                foreach (var cell in cells)
                {
                    var linkNode = cell.SelectSingleNode(".//a");
                    if (linkNode != null)
                    {
                        var linkText = linkNode.InnerText.Trim();
                        if (linkText.Contains("こちら") || linkText.Contains("在庫"))
                        {
                            stockStatusUrl = linkNode.GetAttributeValue("href", string.Empty);
                            if (!string.IsNullOrEmpty(stockStatusUrl))
                            {
                                Console.WriteLine($"    Found stock link: {stockStatusUrl}");
                                // 相対URLの場合は絶対URLに変換
                                if (!stockStatusUrl.StartsWith("http"))
                                {
                                    var baseUri = new Uri(TARGET_URL);
                                    stockStatusUrl = new Uri(baseUri, stockStatusUrl).ToString();
                                }
                                break;
                            }
                        }
                    }
                }

                Console.WriteLine($"    LocationName: '{locationName}'");
                Console.WriteLine($"    Description: '{description}'");
                Console.WriteLine($"    CardImageUrl: '{cardImageUrl}'");
                Console.WriteLine($"    IssueDate: '{issueDate}'");
                Console.WriteLine($"    DistributionLocation: '{distributionLocation}'");
                Console.WriteLine($"    DistributionHours: '{distributionHours}'");
                Console.WriteLine($"    StockStatusUrl: '{stockStatusUrl}'");

                if (string.IsNullOrWhiteSpace(locationName))
                {
                    Console.WriteLine("    Skipped: Empty location name");
                    return null;
                }

                var rawContent = $"{locationName}|{description}|" +
                    $"{cardImageUrl}|{issueDate}|{distributionLocation}|" +
                    $"{distributionHours}|{stockStatusUrl}";
                var contentHash = GenerateHash(rawContent);

                return new DistributionLocationData
                {
                    LocationName = locationName,
                    Description = description,
                    CardImageUrl = cardImageUrl,
                    IssueDate = issueDate,
                    DistributionLocation = distributionLocation,
                    DistributionHours = distributionHours,
                    StockStatusUrl = stockStatusUrl,
                    ContentHash = contentHash,
                    FetchedDate = DateTimeOffset.Now
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Error parsing row: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// コンテンツのハッシュを生成（差分検出用）
        /// </summary>
        private string GenerateHash(string content)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(content));
                return Convert.ToBase64String(hashBytes);
            }
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
