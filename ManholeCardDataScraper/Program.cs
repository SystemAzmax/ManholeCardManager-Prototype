using ManholeCardDataScraper.Services;
using System;
using System.Threading.Tasks;

namespace ManholeCardDataScraper
{
    /// <summary>
    /// マンホールカード配布場所データスクレイパー
    /// このプログラムは、マンホールカード配布場所情報をWebから
    /// スクレイピングしてDBに同期します。
    /// </summary>
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Manhole Card Data Scraper ===");
            Console.WriteLine("Scraping distribution location data from GK-P");
            Console.WriteLine();

            var scraperService = new LocationScraperService();

            try
            {
                await scraperService.InitializeAsync();
                var result = await scraperService.ExecuteAsync();

                Console.WriteLine();
                Console.WriteLine("=== Scraping Complete ===");
                Console.WriteLine($"Total locations added/updated: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
            finally
            {
                await scraperService.DisposeAsync();
            }
        }
    }
}
