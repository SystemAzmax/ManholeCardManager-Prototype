using ManholeCardManager.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ManholeCardManager.Services
{
    /// <summary>
    /// SQLiteデータベースでデータを管理するサービスクラス
    /// </summary>
    public class DatabaseService
    {
        private const string DATABASE_NAME = "manholecard.db";
        private string _databasePath = string.Empty;
        private readonly string? _customBasePath;

        /// <summary>
        /// コンストラクタ（デフォルト）
        /// </summary>
        public DatabaseService()
        {
        }

        /// <summary>
        /// コンストラクタ（カスタムパス指定）
        /// </summary>
        /// <param name="basePath">データベースフォルダのベースパス</param>
        public DatabaseService(string basePath)
        {
            _customBasePath = basePath;
        }

        /// <summary>
        /// データベース接続文字列を取得
        /// </summary>
        private string GetConnectionString()
        {
            return $"Data Source={_databasePath}";
        }

        /// <summary>
        /// データベースを初期化
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                if (_customBasePath != null)
                {
                    _databasePath = Path.Combine(_customBasePath, DATABASE_NAME);
                }
                else
                {
                    try
                    {
                        _databasePath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, DATABASE_NAME);
                    }
                    catch
                    {
                        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                        var appFolder = Path.Combine(localAppData, "ManholeCardManager");
                        if (!Directory.Exists(appFolder))
                        {
                            Directory.CreateDirectory(appFolder);
                        }
                        _databasePath = Path.Combine(appFolder, DATABASE_NAME);
                    }
                }

                await CreateTablesAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// テーブルを作成
        /// </summary>
        private async Task CreateTablesAsync()
        {
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var createTablesCommand = connection.CreateCommand();
            createTablesCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS Cards (
                    CardId INTEGER PRIMARY KEY AUTOINCREMENT,
                    LocationId INTEGER,
                    DesignImagePath TEXT,
                    SeriesNumber INTEGER DEFAULT 1,
                    IssuedDate TEXT,
                    CreatedDate TEXT NOT NULL,
                    UpdatedDate TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS Locations (
                    LocationId INTEGER PRIMARY KEY AUTOINCREMENT,
                    CardId INTEGER,
                    LocationName TEXT NOT NULL,
                    Prefecture TEXT,
                    Municipality TEXT,
                    Address TEXT,
                    Latitude REAL,
                    Longitude REAL,
                    Description TEXT,
                    StockStatus TEXT,
                    CreatedDate TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS ManholeLocations (
                    RelationId INTEGER PRIMARY KEY AUTOINCREMENT,
                    LocationId INTEGER NOT NULL,
                    CardId INTEGER NOT NULL,
                    Latitude REAL,
                    Longitude REAL,
                    SpotName TEXT,
                    Description TEXT
                );

                CREATE TABLE IF NOT EXISTS AcquisitionHistory (
                    HistoryId INTEGER PRIMARY KEY AUTOINCREMENT,
                    CardId INTEGER NOT NULL,
                    AcquisitionDate TEXT NOT NULL,
                    LocationId INTEGER,
                    Notes TEXT,
                    CreatedDate TEXT NOT NULL
                );
            ";

            await createTablesCommand.ExecuteNonQueryAsync();

            // データベースの状態をログ出力
            await LogDatabaseStatusAsync(connection);
        }

        /// <summary>
        /// データベースの状態をログ出力
        /// </summary>
        private async Task LogDatabaseStatusAsync(SqliteConnection connection)
        {
            var countCommand = connection.CreateCommand();
            countCommand.CommandText = @"
                SELECT 
                    (SELECT COUNT(*) FROM Cards) as CardsCount,
                    (SELECT COUNT(*) FROM Locations) as LocationsCount,
                    (SELECT COUNT(*) FROM AcquisitionHistory) as AcquisitionHistoryCount
            ";

            using var reader = await countCommand.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                System.Diagnostics.Debug.WriteLine($"Database Status:");
                System.Diagnostics.Debug.WriteLine($"  Cards: {reader.GetInt32(0)}");
                System.Diagnostics.Debug.WriteLine($"  Locations: {reader.GetInt32(1)}");
                System.Diagnostics.Debug.WriteLine($"  AcquisitionHistory: {reader.GetInt32(2)}");
                System.Diagnostics.Debug.WriteLine($"  Database Path: {_databasePath}");
            }
        }

        /// <summary>
        /// 全カードを取得
        /// </summary>
        public async Task<List<ManholeCard>> GetAllCardsAsync()
        {
            var cards = new List<ManholeCard>();
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Cards";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cards.Add(new ManholeCard
                {
                    CardId = reader.GetInt32(0),
                    LocationId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    DesignImagePath = reader.IsDBNull(2) ? null : reader.GetString(2),
                    SeriesNumber = reader.GetInt32(3),
                    IssuedDate = reader.IsDBNull(4) ? null : DateTimeOffset.Parse(reader.GetString(4)),
                    CreatedDate = DateTimeOffset.Parse(reader.GetString(5)),
                    UpdatedDate = DateTimeOffset.Parse(reader.GetString(6))
                });
            }

            return cards;
        }

        /// <summary>
        /// カードで検索
        /// </summary>
        public async Task<List<ManholeCard>> SearchCardsAsync(string query)
        {
            var cards = new List<ManholeCard>();
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT DISTINCT c.* FROM Cards c
                LEFT JOIN Locations l ON c.CardId = l.CardId
                WHERE l.Prefecture LIKE @query OR l.Municipality LIKE @query
            ";
            command.Parameters.AddWithValue("@query", $"%{query}%");

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cards.Add(new ManholeCard
                {
                    CardId = reader.GetInt32(0),
                    LocationId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    DesignImagePath = reader.IsDBNull(2) ? null : reader.GetString(2),
                    SeriesNumber = reader.GetInt32(3),
                    IssuedDate = reader.IsDBNull(4) ? null : DateTimeOffset.Parse(reader.GetString(4)),
                    CreatedDate = DateTimeOffset.Parse(reader.GetString(5)),
                    UpdatedDate = DateTimeOffset.Parse(reader.GetString(6))
                });
            }

            return cards;
        }

        /// <summary>
        /// 都道府県でカードを検索
        /// </summary>
        public async Task<List<ManholeCard>> GetCardsByPrefectureAsync(string prefecture)
        {
            var cards = new List<ManholeCard>();
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT c.* FROM Cards c
                INNER JOIN Locations l ON c.CardId = l.CardId
                WHERE l.Prefecture = @prefecture
            ";
            command.Parameters.AddWithValue("@prefecture", prefecture);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cards.Add(new ManholeCard
                {
                    CardId = reader.GetInt32(0),
                    LocationId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    DesignImagePath = reader.IsDBNull(2) ? null : reader.GetString(2),
                    SeriesNumber = reader.GetInt32(3),
                    IssuedDate = reader.IsDBNull(4) ? null : DateTimeOffset.Parse(reader.GetString(4)),
                    CreatedDate = DateTimeOffset.Parse(reader.GetString(5)),
                    UpdatedDate = DateTimeOffset.Parse(reader.GetString(6))
                });
            }

            return cards;
        }

        /// <summary>
        /// 弾数でカードを検索
        /// </summary>
        public async Task<List<ManholeCard>> GetCardsBySeriesNumberAsync(int seriesNumber)
        {
            var cards = new List<ManholeCard>();
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Cards
                WHERE SeriesNumber = @seriesNumber
            ";
            command.Parameters.AddWithValue("@seriesNumber", seriesNumber);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cards.Add(new ManholeCard
                {
                    CardId = reader.GetInt32(0),
                    LocationId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    DesignImagePath = reader.IsDBNull(2) ? null : reader.GetString(2),
                    SeriesNumber = reader.GetInt32(3),
                    IssuedDate = reader.IsDBNull(4) ? null : DateTimeOffset.Parse(reader.GetString(4)),
                    CreatedDate = DateTimeOffset.Parse(reader.GetString(5)),
                    UpdatedDate = DateTimeOffset.Parse(reader.GetString(6))
                });
            }

            return cards;
        }

        /// <summary>
        /// 都道府県と弾数で検索（複合フィルタ）
        /// </summary>
        public async Task<List<ManholeCard>> GetCardsByPrefectureAndSeriesAsync(
            string prefecture, int seriesNumber)
        {
            var cards = new List<ManholeCard>();
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT c.* FROM Cards c
                INNER JOIN Locations l ON c.CardId = l.CardId
                WHERE l.Prefecture = @prefecture AND c.SeriesNumber = @seriesNumber
            ";
            command.Parameters.AddWithValue("@prefecture", prefecture);
            command.Parameters.AddWithValue("@seriesNumber", seriesNumber);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cards.Add(new ManholeCard
                {
                    CardId = reader.GetInt32(0),
                    LocationId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    DesignImagePath = reader.IsDBNull(2) ? null : reader.GetString(2),
                    SeriesNumber = reader.GetInt32(3),
                    IssuedDate = reader.IsDBNull(4) ? null : DateTimeOffset.Parse(reader.GetString(4)),
                    CreatedDate = DateTimeOffset.Parse(reader.GetString(5)),
                    UpdatedDate = DateTimeOffset.Parse(reader.GetString(6))
                });
            }

            return cards;
        }

        /// <summary>
        /// カードを追加
        /// </summary>
        public async Task<int> InsertCardAsync(ManholeCard card)
        {
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Cards (LocationId, DesignImagePath, SeriesNumber, IssuedDate, CreatedDate, UpdatedDate)
                VALUES (@locationId, @designImagePath, @seriesNumber, @issuedDate, @createdDate, @updatedDate);
                SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue("@locationId", (object?)card.LocationId ?? DBNull.Value);
            command.Parameters.AddWithValue("@designImagePath", (object?)card.DesignImagePath ?? DBNull.Value);
            command.Parameters.AddWithValue("@seriesNumber", card.SeriesNumber);
            command.Parameters.AddWithValue("@issuedDate", (object?)card.IssuedDate?.ToString("O") ?? DBNull.Value);
            command.Parameters.AddWithValue("@createdDate", DateTimeOffset.Now.ToString("O"));
            command.Parameters.AddWithValue("@updatedDate", DateTimeOffset.Now.ToString("O"));

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// カードを更新
        /// </summary>
        public async Task<int> UpdateCardAsync(ManholeCard card)
        {
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE Cards 
                SET LocationId = @locationId, 
                    DesignImagePath = @designImagePath, 
                    SeriesNumber = @seriesNumber, 
                    IssuedDate = @issuedDate, 
                    UpdatedDate = @updatedDate
                WHERE CardId = @cardId
            ";

            command.Parameters.AddWithValue("@cardId", card.CardId);
            command.Parameters.AddWithValue("@locationId", (object?)card.LocationId ?? DBNull.Value);
            command.Parameters.AddWithValue("@designImagePath", (object?)card.DesignImagePath ?? DBNull.Value);
            command.Parameters.AddWithValue("@seriesNumber", card.SeriesNumber);
            command.Parameters.AddWithValue("@issuedDate", (object?)card.IssuedDate?.ToString("O") ?? DBNull.Value);
            command.Parameters.AddWithValue("@updatedDate", DateTimeOffset.Now.ToString("O"));

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// カードを削除
        /// </summary>
        public async Task<int> DeleteCardAsync(int cardId)
        {
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var deleteCard = connection.CreateCommand();
            deleteCard.CommandText = "DELETE FROM Cards WHERE CardId = @cardId";
            deleteCard.Parameters.AddWithValue("@cardId", cardId);
            
            return await deleteCard.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// カードの位置情報を取得
        /// </summary>
        public async Task<List<CardLocation>> GetCardLocationsAsync(int cardId)
        {
            var locations = new List<CardLocation>();
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM Locations WHERE CardId = @cardId";
            command.Parameters.AddWithValue("@cardId", cardId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                locations.Add(new CardLocation
                {
                    LocationId = reader.GetInt32(0),
                    CardId = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                    LocationName = reader.GetString(2),
                    Prefecture = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Municipality = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Address = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Latitude = reader.IsDBNull(6) ? null : reader.GetDouble(6),
                    Longitude = reader.IsDBNull(7) ? null : reader.GetDouble(7),
                    Description = reader.IsDBNull(8) ? null : reader.GetString(8),
                    StockStatus = reader.IsDBNull(9) ? null : reader.GetString(9),
                    CreatedDate = DateTimeOffset.Parse(reader.GetString(10))
                });
            }

            return locations;
        }

        /// <summary>
        /// カードIDから最初のLocationを取得
        /// </summary>
        public async Task<CardLocation?> GetCardLocationAsync(int cardId)
        {
            var locations = await GetCardLocationsAsync(cardId);
            return locations.FirstOrDefault();
        }

        /// <summary>
        /// マンホール設置場所情報を取得
        /// </summary>
        /// <param name="cardId">カードID</param>
        /// <returns>マンホール設置場所リスト</returns>
        public async Task<List<ManholeLocation>> GetManholeLocationsByCardIdAsync(int cardId)
        {
            var manholeLocations = new List<ManholeLocation>();
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM ManholeLocations WHERE CardId = @cardId";
            command.Parameters.AddWithValue("@cardId", cardId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                manholeLocations.Add(new ManholeLocation
                {
                    RelationId = reader.GetInt32(0),
                    LocationId = reader.GetInt32(1),
                    CardId = reader.GetInt32(2),
                    Latitude = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                    Longitude = reader.IsDBNull(4) ? null : reader.GetDouble(4),
                    SpotName = reader.IsDBNull(5) ? null : reader.GetString(5),
                    Description = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }

            return manholeLocations;
        }

        /// <summary>
        /// マンホール設置場所を追加
        /// </summary>
        /// <param name="manholeLocation">マンホール設置場所情報</param>
        /// <returns>挿入されたRelationId</returns>
        public async Task<int> InsertManholeLocationAsync(ManholeLocation manholeLocation)
        {
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ManholeLocations (LocationId, CardId, Latitude, Longitude, SpotName, Description)
                VALUES (@locationId, @cardId, @latitude, @longitude, @spotName, @description);
                SELECT last_insert_rowid();
            ";

            command.Parameters.AddWithValue("@locationId", manholeLocation.LocationId);
            command.Parameters.AddWithValue("@cardId", manholeLocation.CardId);
            command.Parameters.AddWithValue("@latitude", (object?)manholeLocation.Latitude ?? DBNull.Value);
            command.Parameters.AddWithValue("@longitude", (object?)manholeLocation.Longitude ?? DBNull.Value);
            command.Parameters.AddWithValue("@spotName", (object?)manholeLocation.SpotName ?? DBNull.Value);
            command.Parameters.AddWithValue("@description", (object?)manholeLocation.Description ?? DBNull.Value);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        /// <summary>
        /// マンホール設置場所を更新
        /// </summary>
        /// <param name="manholeLocation">マンホール設置場所情報</param>
        /// <returns>更新された行数</returns>
        public async Task<int> UpdateManholeLocationAsync(ManholeLocation manholeLocation)
        {
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                UPDATE ManholeLocations
                SET LocationId = @locationId,
                    CardId = @cardId,
                    Latitude = @latitude,
                    Longitude = @longitude,
                    SpotName = @spotName,
                    Description = @description
                WHERE RelationId = @relationId
            ";

            command.Parameters.AddWithValue("@relationId", manholeLocation.RelationId);
            command.Parameters.AddWithValue("@locationId", manholeLocation.LocationId);
            command.Parameters.AddWithValue("@cardId", manholeLocation.CardId);
            command.Parameters.AddWithValue("@latitude", (object?)manholeLocation.Latitude ?? DBNull.Value);
            command.Parameters.AddWithValue("@longitude", (object?)manholeLocation.Longitude ?? DBNull.Value);
            command.Parameters.AddWithValue("@spotName", (object?)manholeLocation.SpotName ?? DBNull.Value);
            command.Parameters.AddWithValue("@description", (object?)manholeLocation.Description ?? DBNull.Value);

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// マンホール設置場所を削除
        /// </summary>
        /// <param name="relationId">関連ID</param>
        /// <returns>削除された行数</returns>
        public async Task<int> DeleteManholeLocationAsync(int relationId)
        {
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM ManholeLocations WHERE RelationId = @relationId";
            command.Parameters.AddWithValue("@relationId", relationId);

            return await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// 配布場所のカードを取得
        /// </summary>
        public async Task<List<ManholeCard>> GetCardsByLocationAsync(int locationId)
        {
            var cards = new List<ManholeCard>();
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM Cards c
                INNER JOIN Locations l ON c.CardId = l.CardId
                WHERE l.LocationId = @locationId
            ";
            command.Parameters.AddWithValue("@locationId", locationId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                cards.Add(new ManholeCard
                {
                    CardId = reader.GetInt32(0),
                    LocationId = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    DesignImagePath = reader.IsDBNull(2) ? null : reader.GetString(2),
                    SeriesNumber = reader.GetInt32(3),
                    IssuedDate = reader.IsDBNull(4) ? null : DateTimeOffset.Parse(reader.GetString(4)),
                    CreatedDate = DateTimeOffset.Parse(reader.GetString(5)),
                    UpdatedDate = DateTimeOffset.Parse(reader.GetString(6))
                });
            }

            return cards;
        }

        /// <summary>
        /// 取得履歴を追加
        /// </summary>
        public async Task InsertAcquisitionHistoryAsync(AcquisitionHistory history)
        {
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO AcquisitionHistory (CardId, AcquisitionDate, LocationId, Notes, CreatedDate)
                VALUES (@cardId, @acquisitionDate, @locationId, @notes, @createdDate)
            ";

            command.Parameters.AddWithValue("@cardId", history.CardId);
            command.Parameters.AddWithValue("@acquisitionDate", DateTimeOffset.Now.ToString("O"));
            command.Parameters.AddWithValue("@locationId", (object?)history.LocationId ?? DBNull.Value);
            command.Parameters.AddWithValue("@notes", (object?)history.Notes ?? DBNull.Value);
            command.Parameters.AddWithValue("@createdDate", DateTimeOffset.Now.ToString("O"));

            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// カードの取得履歴を取得
        /// </summary>
        public async Task<AcquisitionHistory?> GetAcquisitionHistoryAsync(int cardId)
        {
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM AcquisitionHistory WHERE CardId = @cardId LIMIT 1";
            command.Parameters.AddWithValue("@cardId", cardId);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new AcquisitionHistory
                {
                    HistoryId = reader.GetInt32(0),
                    CardId = reader.GetInt32(1),
                    AcquisitionDate = DateTimeOffset.Parse(reader.GetString(2)),
                    LocationId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    Notes = reader.IsDBNull(4) ? null : reader.GetString(4),
                    CreatedDate = DateTimeOffset.Parse(reader.GetString(5))
                };
            }

            return null;
        }

        /// <summary>
        /// 全配布場所とそのカードを取得
        /// </summary>
        public async Task<List<DistributionLocationWithCards>> GetAllDistributionLocationsWithCardsAsync()
        {
            var locations = new Dictionary<int, DistributionLocationWithCards>();

            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT 
                    l.LocationId, 
                    l.LocationName,
                    l.Address,
                    l.Prefecture,
                    l.Municipality,
                    l.Description,
                    l.StockStatus,
                    c.CardId, 
                    c.DesignImagePath, 
                    c.SeriesNumber,
                    c.IssuedDate,
                    ah.CardId as HasAcquired
                FROM Locations l
                INNER JOIN Cards c ON l.CardId = c.CardId
                LEFT JOIN AcquisitionHistory ah ON c.CardId = ah.CardId
                ORDER BY l.LocationName, c.SeriesNumber
            ";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var locationId = reader.GetInt32(0);

                if (!locations.ContainsKey(locationId))
                {
                    locations[locationId] = new DistributionLocationWithCards
                    {
                        LocationId = locationId,
                        LocationName = reader.GetString(1),
                        Address = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Prefecture = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Municipality = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Description = reader.IsDBNull(5) ? null : reader.GetString(5),
                        DistributedCards = new System.Collections.ObjectModel.ObservableCollection<CardWithAcquisitionStatus>()
                    };
                }

                locations[locationId].DistributedCards.Add(new CardWithAcquisitionStatus
                {
                    CardId = reader.GetInt32(7),
                    DesignImagePath = reader.IsDBNull(8) ? null : reader.GetString(8),
                    SeriesNumber = reader.GetInt32(9),
                    IssuedDate = reader.IsDBNull(10) ? null : DateTimeOffset.Parse(reader.GetString(10)),
                    Prefecture = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Municipality = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Description = reader.IsDBNull(5) ? null : reader.GetString(5),
                    StockStatus = reader.IsDBNull(6) ? null : reader.GetString(6),
                    IsAcquired = !reader.IsDBNull(11)
                });
            }

            System.Diagnostics.Debug.WriteLine($"Loaded {locations.Count} distribution locations from database");
            return locations.Values.ToList();
        }

        /// <summary>
        /// サンプルデータを挿入（テスト用）
        /// </summary>
        public async Task InsertSampleDataAsync()
        {
            try
            {
                using var connection = new SqliteConnection(GetConnectionString());
                await connection.OpenAsync();

                // データが既に存在する場合はスキップ
                var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = "SELECT COUNT(*) FROM Cards";
                var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                if (count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("Sample data already exists, skipping insertion");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("Inserting sample data...");
                var now = DateTimeOffset.Now.ToString("O");

                // サンプル配布場所とカードを追加（1件ずつ）
                var location1 = connection.CreateCommand();
                location1.CommandText = @"
                    INSERT INTO Locations (LocationName, Prefecture, Municipality, Address, Description, StockStatus, CreatedDate)
                    VALUES ('東京都庁', '東京都', '新宿区', '東京都新宿区西新宿2-8-1', '平日 9:00-17:00', 'https://example.com/stock/tokyo', @now)
                ";
                location1.Parameters.AddWithValue("@now", now);
                await location1.ExecuteNonQueryAsync();
                System.Diagnostics.Debug.WriteLine("Location 1 inserted");

                var location2 = connection.CreateCommand();
                location2.CommandText = @"
                    INSERT INTO Locations (LocationName, Prefecture, Municipality, Address, Description, StockStatus, CreatedDate)
                    VALUES ('大阪市役所', '大阪府', '大阪市', '大阪府大阪市北区中之島1-3-20', '月～金 8:30-17:30', 'https://example.com/stock/osaka', @now)
                ";
                location2.Parameters.AddWithValue("@now", now);
                await location2.ExecuteNonQueryAsync();

                var location3 = connection.CreateCommand();
                location3.CommandText = @"
                    INSERT INTO Locations (LocationName, Prefecture, Municipality, Address, Description, StockStatus, CreatedDate)
                    VALUES ('札幌市役所', '北海道', '札幌市', '北海道札幌市中央区北1条西2丁目', '9:00-17:00（土日祝除く）', '在庫なし', @now)
                ";
                location3.Parameters.AddWithValue("@now", now);
                await location3.ExecuteNonQueryAsync();
                System.Diagnostics.Debug.WriteLine("Location 3 inserted");

                // サンプルカードを追加（LocationIdを設定）
                var card1 = connection.CreateCommand();
                card1.CommandText = @"
                    INSERT INTO Cards (LocationId, SeriesNumber, CreatedDate, UpdatedDate)
                    VALUES (1, 1, @now, @now)
                ";
                card1.Parameters.AddWithValue("@now", now);
                await card1.ExecuteNonQueryAsync();
                System.Diagnostics.Debug.WriteLine("Card 1 inserted");

                var card2 = connection.CreateCommand();
                card2.CommandText = @"
                    INSERT INTO Cards (LocationId, SeriesNumber, CreatedDate, UpdatedDate)
                    VALUES (2, 1, @now, @now)
                ";
                card2.Parameters.AddWithValue("@now", now);
                await card2.ExecuteNonQueryAsync();
                System.Diagnostics.Debug.WriteLine("Card 2 inserted");

                var card3 = connection.CreateCommand();
                card3.CommandText = @"
                    INSERT INTO Cards (LocationId, SeriesNumber, CreatedDate, UpdatedDate)
                    VALUES (3, 2, @now, @now)
                ";
                card3.Parameters.AddWithValue("@now", now);
                await card3.ExecuteNonQueryAsync();
                System.Diagnostics.Debug.WriteLine("Card 3 inserted");

                System.Diagnostics.Debug.WriteLine("Sample data inserted successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inserting sample data: {ex.Message}");
                throw;
            }
        }
    }
}
