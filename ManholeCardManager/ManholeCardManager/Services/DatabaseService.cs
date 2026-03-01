using ManholeCardManager.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        
        // Cardsテーブルのカラムインデックス（SELECT * FROM Cardsの順序に対応）
        private const int CARD_ID_INDEX = 0;
        private const int LOCATION_ID_INDEX = 1;
        private const int DESIGN_IMAGE_PATH_INDEX = 2;
        private const int SERIES_NUMBER_INDEX = 3;
        private const int ISSUED_DATE_INDEX = 4;
        private const int CREATED_DATE_INDEX = 5;
        private const int UPDATED_DATE_INDEX = 6;
        
        // Locationsテーブルのカラムインデックス
        private const int LOCATION_LOCATION_ID_INDEX = 0;
        private const int LOCATION_CARD_ID_INDEX = 1;
        private const int LOCATION_NAME_INDEX = 2;
        private const int LOCATION_PREFECTURE_INDEX = 3;
        private const int LOCATION_MUNICIPALITY_INDEX = 4;
        private const int LOCATION_ADDRESS_INDEX = 5;
        
        // GetAllDistributionLocationsWithCardsAsyncのSELECT結果のカラムインデックス
        private const int DIST_LOCATION_ID_INDEX = 0;
        private const int DIST_LOCATION_NAME_INDEX = 1;
        private const int DIST_ADDRESS_INDEX = 2;
        private const int DIST_PREFECTURE_INDEX = 3;
        private const int DIST_MUNICIPALITY_INDEX = 4;
        private const int DIST_DESCRIPTION_INDEX = 5;
        private const int DIST_STOCK_STATUS_INDEX = 6;
        private const int DIST_CARD_ID_INDEX = 7;
        private const int DIST_DESIGN_IMAGE_PATH_INDEX = 8;
        private const int DIST_SERIES_NUMBER_INDEX = 9;
        private const int DIST_ISSUED_DATE_INDEX = 10;
        private const int DIST_IS_ACQUIRED_INDEX = 11;
        private const int DIST_ACQUISITION_DATE_INDEX = 12;
        private const int DIST_NOTES_INDEX = 13;
        
        // PRAGMAテーブルのカラムインデックス
        private const int PRAGMA_COLUMN_NAME_INDEX = 1;
        
        private string _databasePath = string.Empty;
        private readonly string? _customBasePath;
        private readonly ILogger<DatabaseService>? _logger;

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
        /// コンストラクタ（ロガー指定）
        /// </summary>
        /// <param name="logger">ロガーインスタンス</param>
        public DatabaseService(ILogger<DatabaseService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// コンストラクタ（カスタムパスとロガー指定）
        /// </summary>
        /// <param name="basePath">データベースフォルダのベースパス</param>
        /// <param name="logger">ロガーインスタンス</param>
        public DatabaseService(string basePath, ILogger<DatabaseService> logger)
        {
            _customBasePath = basePath;
            _logger = logger;
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
                _logger?.LogInformation("データベース初期化を開始します。");

                if (_customBasePath != null)
                {
                    _databasePath = Path.Combine(_customBasePath, DATABASE_NAME);
                    _logger?.LogDebug(
                        "カスタムパスを使用します: {DatabasePath}",
                        _databasePath);
                }
                else
                {
                    try
                    {
                        _databasePath = Path.Combine(
                            Windows.Storage.ApplicationData.Current.LocalFolder.Path,
                            DATABASE_NAME);
                        _logger?.LogDebug(
                            "Windows.Storage.ApplicationData を使用します: {DatabasePath}",
                            _databasePath);
                    }
                    catch (InvalidOperationException ex)
                    {
                        _logger?.LogWarning(
                            ex,
                            "Windows.Storage.ApplicationData にアクセスできません。" +
                            "LocalApplicationData を使用します。");
                        // Windows.Storage.ApplicationData にアクセスできない場合（UWP以外の環境など）
                        var localAppData = Environment.GetFolderPath(
                            Environment.SpecialFolder.LocalApplicationData);
                        var appFolder = Path.Combine(localAppData, "ManholeCardManager");
                        if (!Directory.Exists(appFolder))
                        {
                            Directory.CreateDirectory(appFolder);
                        }
                        _databasePath = Path.Combine(appFolder, DATABASE_NAME);
                        _logger?.LogDebug(
                            "LocalApplicationData を使用します: {DatabasePath}",
                            _databasePath);
                    }
                }

                await CreateTablesAsync();
                _logger?.LogInformation("データベース初期化が完了しました。");
            }
            catch (InvalidOperationException ex)
            {
                _logger?.LogError(
                    ex,
                    "データベース初期化中に InvalidOperationException が発生しました。");
                throw;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "データベース初期化中に SqliteException が発生しました。");
                throw;
            }
        }

        /// <summary>
        /// テーブルを作成
        /// </summary>
        private async Task CreateTablesAsync()
        {
            try
            {
                _logger?.LogInformation("テーブル作成を開始します。");

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
                    IsAcquired INTEGER NOT NULL DEFAULT 1,
                    AcquisitionDate TEXT NOT NULL,
                    LocationId INTEGER,
                    Notes TEXT,
                    CreatedDate TEXT NOT NULL
                );
            ";

                await createTablesCommand.ExecuteNonQueryAsync();
                _logger?.LogDebug("すべてのテーブルが作成されました。");

                // 既存テーブルに IsAcquired カラムを追加（マイグレーション）
                await MigrateAcquisitionHistoryTableAsync(connection);
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "テーブル作成中に SqliteException が発生しました。");
                throw;
            }
        }

        /// <summary>
        /// AcquisitionHistoryテーブルにIsAcquiredカラムを追加するマイグレーション
        /// </summary>
        private async Task MigrateAcquisitionHistoryTableAsync(SqliteConnection connection)
        {
            try
            {
                _logger?.LogInformation("AcquisitionHistory テーブルのマイグレーションを確認します。");

                // カラムの存在確認
                var checkColumnCommand = connection.CreateCommand();
                checkColumnCommand.CommandText = "PRAGMA table_info(AcquisitionHistory)";
                
                bool hasIsAcquiredColumn = false;
                var columnList = new System.Collections.Generic.List<string>();
                
                using (var reader = await checkColumnCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var columnName = reader.GetString(PRAGMA_COLUMN_NAME_INDEX);
                        columnList.Add(columnName);
                        if (columnName == "IsAcquired")
                        {
                            hasIsAcquiredColumn = true;
                        }
                    }
                }

                // カラムが存在しない場合は追加
                if (!hasIsAcquiredColumn)
                {
                    _logger?.LogInformation("IsAcquired カラムが見つかりません。追加します。");

                    var alterTableCommand = connection.CreateCommand();
                    alterTableCommand.CommandText = @"
                        ALTER TABLE AcquisitionHistory 
                        ADD COLUMN IsAcquired INTEGER NOT NULL DEFAULT 1;
                    ";
                    await alterTableCommand.ExecuteNonQueryAsync();
                    _logger?.LogInformation("IsAcquired カラムが追加されました。");
                }
                else
                {
                    _logger?.LogDebug("IsAcquired カラムは既に存在します。");
                }
            }
            catch (SqliteException ex)
            {
                _logger?.LogWarning(
                    ex,
                    "マイグレーション実行中に SqliteException が発生しました。" +
                    "テーブルが既に存在するか、スキーマ関連のエラーと考えられます。");
            }
        }

        /// <summary>
        /// データベースの状態をログ出力
        /// </summary>
        private async Task LogDatabaseStatusAsync(SqliteConnection connection)
        {
            try
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
                    var cardsCount = reader.GetInt32(0);
                    var locationsCount = reader.GetInt32(1);
                    var acquisitionHistoryCount = reader.GetInt32(2);

                    _logger?.LogInformation(
                        "データベース統計情報: " +
                        "カード件数={CardsCount}, " +
                        "配布場所件数={LocationsCount}, " +
                        "取得履歴件数={AcquisitionHistoryCount}",
                        cardsCount,
                        locationsCount,
                        acquisitionHistoryCount);
                }
            }
            catch (SqliteException ex)
            {
                _logger?.LogWarning(
                    ex,
                    "データベース統計情報の取得に失敗しました。");
            }
        }

        /// <summary>
        /// SQLiteリーダーからManholeCardを生成
        /// </summary>
        private ManholeCard ReadManholeCard(Microsoft.Data.Sqlite.SqliteDataReader reader)
        {
            return new ManholeCard
            {
                CardId = reader.GetInt32(CARD_ID_INDEX),
                LocationId = reader.IsDBNull(LOCATION_ID_INDEX) ? null : reader.GetInt32(LOCATION_ID_INDEX),
                DesignImagePath = reader.IsDBNull(DESIGN_IMAGE_PATH_INDEX) ? null : reader.GetString(DESIGN_IMAGE_PATH_INDEX),
                SeriesNumber = reader.GetInt32(SERIES_NUMBER_INDEX),
                IssuedDate = reader.IsDBNull(ISSUED_DATE_INDEX) ? null : DateTimeOffset.Parse(reader.GetString(ISSUED_DATE_INDEX)),
                CreatedDate = DateTimeOffset.Parse(reader.GetString(CREATED_DATE_INDEX)),
                UpdatedDate = DateTimeOffset.Parse(reader.GetString(UPDATED_DATE_INDEX))
            };
        }

        /// <summary>
        /// 全カードを取得
        /// </summary>
        public async Task<List<ManholeCard>> GetAllCardsAsync()
        {
            var cards = new List<ManholeCard>();
            try
            {
                using var connection = new SqliteConnection(GetConnectionString());
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Cards";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    cards.Add(ReadManholeCard(reader));
                }

                _logger?.LogDebug("全カード取得: {CardCount} 件", cards.Count);
                return cards;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "全カード取得中に SqliteException が発生しました。");
                throw;
            }
        }

        /// <summary>
        /// カードで検索
        /// </summary>
        public async Task<List<ManholeCard>> SearchCardsAsync(string query)
        {
            var cards = new List<ManholeCard>();
            try
            {
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
                    cards.Add(ReadManholeCard(reader));
                }

                _logger?.LogDebug(
                    "カード検索: クエリ={Query}, 結果件数={ResultCount}",
                    query,
                    cards.Count);
                return cards;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "カード検索中に SqliteException が発生しました。クエリ={Query}",
                    query);
                throw;
            }
        }

        /// <summary>
        /// 都道府県でカードを検索
        /// </summary>
        public async Task<List<ManholeCard>> GetCardsByPrefectureAsync(string prefecture)
        {
            var cards = new List<ManholeCard>();
            try
            {
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
                    cards.Add(ReadManholeCard(reader));
                }

                _logger?.LogDebug(
                    "都道府県別カード取得: {Prefecture}={CardCount} 件",
                    prefecture,
                    cards.Count);
                return cards;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "都道府県別カード取得中に SqliteException が発生しました。Prefecture={Prefecture}",
                    prefecture);
                throw;
            }
        }

        /// <summary>
        /// 弾数でカードを検索
        /// </summary>
        public async Task<List<ManholeCard>> GetCardsBySeriesNumberAsync(int seriesNumber)
        {
            var cards = new List<ManholeCard>();
            try
            {
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
                    cards.Add(ReadManholeCard(reader));
                }

                _logger?.LogDebug(
                    "弾数別カード取得: Series={SeriesNumber}={CardCount} 件",
                    seriesNumber,
                    cards.Count);
                return cards;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "弾数別カード取得中に SqliteException が発生しました。SeriesNumber={SeriesNumber}",
                    seriesNumber);
                throw;
            }
        }

        /// <summary>
        /// 都道府県と弾数で検索（複合フィルタ）
        /// </summary>
        public async Task<List<ManholeCard>> GetCardsByPrefectureAndSeriesAsync(
            string prefecture, int seriesNumber)
        {
            var cards = new List<ManholeCard>();
            try
            {
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
                    cards.Add(ReadManholeCard(reader));
                }

                _logger?.LogDebug(
                    "複合フィルタカード取得: Prefecture={Prefecture}, Series={SeriesNumber}={CardCount} 件",
                    prefecture,
                    seriesNumber,
                    cards.Count);
                return cards;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "複合フィルタカード取得中に SqliteException が発生しました。" +
                    "Prefecture={Prefecture}, SeriesNumber={SeriesNumber}",
                    prefecture,
                    seriesNumber);
                throw;
            }
        }

        /// <summary>
        /// カードを追加
        /// </summary>
        public async Task<int> InsertCardAsync(ManholeCard card)
        {
            try
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
                var insertedId = Convert.ToInt32(result);
                _logger?.LogInformation(
                    "カード挿入: CardId={CardId}, Series={SeriesNumber}",
                    insertedId,
                    card.SeriesNumber);
                return insertedId;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "カード挿入中に SqliteException が発生しました。Series={SeriesNumber}",
                    card.SeriesNumber);
                throw;
            }
        }

        /// <summary>
        /// カードを更新
        /// </summary>
        public async Task<int> UpdateCardAsync(ManholeCard card)
        {
            try
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

                var result = await command.ExecuteNonQueryAsync();
                _logger?.LogInformation(
                    "カード更新: CardId={CardId}（更新行数={RowCount}）",
                    card.CardId,
                    result);
                return result;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "カード更新中に SqliteException が発生しました。CardId={CardId}",
                    card.CardId);
                throw;
            }
        }

        /// <summary>
        /// カードを削除
        /// </summary>
        public async Task<int> DeleteCardAsync(int cardId)
        {
            try
            {
                using var connection = new SqliteConnection(GetConnectionString());
                await connection.OpenAsync();

                var deleteCard = connection.CreateCommand();
                deleteCard.CommandText = "DELETE FROM Cards WHERE CardId = @cardId";
                deleteCard.Parameters.AddWithValue("@cardId", cardId);
                
                var result = await deleteCard.ExecuteNonQueryAsync();
                _logger?.LogInformation(
                    "カード削除: CardId={CardId}（削除行数={RowCount}）",
                    cardId,
                    result);
                return result;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "カード削除中に SqliteException が発生しました。CardId={CardId}",
                    cardId);
                throw;
            }
        }

        /// <summary>
        /// カードの位置情報を取得
        /// </summary>
        public async Task<List<CardLocation>> GetCardLocationsAsync(int cardId)
        {
            var locations = new List<CardLocation>();
            try
            {
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

                _logger?.LogDebug(
                    "カード位置情報取得: CardId={CardId}={LocationCount} 件",
                    cardId,
                    locations.Count);
                return locations;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "カード位置情報取得中に SqliteException が発生しました。CardId={CardId}",
                    cardId);
                throw;
            }
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
            try
            {
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

                _logger?.LogDebug(
                    "マンホール設置場所取得: CardId={CardId}={LocationCount} 件",
                    cardId,
                    manholeLocations.Count);
                return manholeLocations;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "マンホール設置場所取得中に SqliteException が発生しました。CardId={CardId}",
                    cardId);
                throw;
            }
        }

        /// <summary>
        /// マンホール設置場所を追加
        /// </summary>
        /// <param name="manholeLocation">マンホール設置場所情報</param>
        /// <returns>挿入されたRelationId</returns>
        public async Task<int> InsertManholeLocationAsync(ManholeLocation manholeLocation)
        {
            try
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
                var insertedId = Convert.ToInt32(result);
                _logger?.LogInformation(
                    "マンホール設置場所挿入: RelationId={RelationId}, CardId={CardId}",
                    insertedId,
                    manholeLocation.CardId);
                return insertedId;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "マンホール設置場所挿入中に SqliteException が発生しました。CardId={CardId}",
                    manholeLocation.CardId);
                throw;
            }
        }

        /// <summary>
        /// マンホール設置場所を更新
        /// </summary>
        /// <param name="manholeLocation">マンホール設置場所情報</param>
        /// <returns>更新された行数</returns>
        public async Task<int> UpdateManholeLocationAsync(ManholeLocation manholeLocation)
        {
            try
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

                var result = await command.ExecuteNonQueryAsync();
                _logger?.LogInformation(
                    "マンホール設置場所更新: RelationId={RelationId}（更新行数={RowCount}）",
                    manholeLocation.RelationId,
                    result);
                return result;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "マンホール設置場所更新中に SqliteException が発生しました。RelationId={RelationId}",
                    manholeLocation.RelationId);
                throw;
            }
        }

        /// <summary>
        /// マンホール設置場所を削除
        /// </summary>
        /// <param name="relationId">関連ID</param>
        /// <returns>削除された行数</returns>
        public async Task<int> DeleteManholeLocationAsync(int relationId)
        {
            try
            {
                using var connection = new SqliteConnection(GetConnectionString());
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM ManholeLocations WHERE RelationId = @relationId";
                command.Parameters.AddWithValue("@relationId", relationId);

                var result = await command.ExecuteNonQueryAsync();
                _logger?.LogInformation(
                    "マンホール設置場所削除: RelationId={RelationId}（削除行数={RowCount}）",
                    relationId,
                    result);
                return result;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "マンホール設置場所削除中に SqliteException が発生しました。RelationId={RelationId}",
                    relationId);
                throw;
            }
        }

        /// <summary>
        /// 配布場所のカードを取得
        /// </summary>
        public async Task<List<ManholeCard>> GetCardsByLocationAsync(int locationId)
        {
            var cards = new List<ManholeCard>();
            try
            {
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
                    cards.Add(ReadManholeCard(reader));
                }

                _logger?.LogDebug(
                    "配布場所別カード取得: LocationId={LocationId}={CardCount} 件",
                    locationId,
                    cards.Count);
                return cards;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "配布場所別カード取得中に SqliteException が発生しました。LocationId={LocationId}",
                    locationId);
                throw;
            }
        }

        /// <summary>
        /// 取得履歴を追加
        /// </summary>
        public async Task InsertAcquisitionHistoryAsync(AcquisitionHistory history)
        {
            try
            {
                using var connection = new SqliteConnection(GetConnectionString());
                await connection.OpenAsync();

                var command = connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO AcquisitionHistory (CardId, IsAcquired, AcquisitionDate, LocationId, Notes, CreatedDate)
                VALUES (@cardId, @isAcquired, @acquisitionDate, @locationId, @notes, @createdDate)
            ";

                command.Parameters.AddWithValue("@cardId", history.CardId);
                command.Parameters.AddWithValue("@isAcquired", history.IsAcquired ? 1 : 0);
                command.Parameters.AddWithValue("@acquisitionDate", history.AcquisitionDate.ToString("O"));
                command.Parameters.AddWithValue("@locationId", (object?)history.LocationId ?? DBNull.Value);
                command.Parameters.AddWithValue("@notes", (object?)history.Notes ?? DBNull.Value);
                command.Parameters.AddWithValue("@createdDate", history.CreatedDate.ToString("O"));

                await command.ExecuteNonQueryAsync();
                _logger?.LogInformation(
                    "取得履歴挿入: CardId={CardId}, IsAcquired={IsAcquired}",
                    history.CardId,
                    history.IsAcquired);
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "取得履歴挿入中に SqliteException が発生しました。CardId={CardId}",
                    history.CardId);
                throw;
            }
        }

        /// <summary>
        /// 取得履歴を読み込み
        /// </summary>
        public async Task<AcquisitionHistory?> GetAcquisitionHistoryAsync(int cardId)
        {
            using var connection = new SqliteConnection(GetConnectionString());
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT HistoryId, CardId, IsAcquired, AcquisitionDate, LocationId, Notes, CreatedDate 
                FROM AcquisitionHistory 
                WHERE CardId = @cardId
                ORDER BY CreatedDate DESC
                LIMIT 1
            ";
            command.Parameters.AddWithValue("@cardId", cardId);

            try
            {
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var history = new AcquisitionHistory
                    {
                        HistoryId = reader.GetInt32(0),
                        CardId = reader.GetInt32(1),
                        IsAcquired = reader.GetInt32(2) == 1,
                        AcquisitionDate = DateTimeOffset.Parse(reader.GetString(3)),
                        LocationId = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                        Notes = reader.IsDBNull(5) ? null : reader.GetString(5),
                        CreatedDate = DateTimeOffset.Parse(reader.GetString(6))
                    };
                    _logger?.LogDebug(
                        "取得履歴読み込み: CardId={CardId}, IsAcquired={IsAcquired}",
                        history.CardId,
                        history.IsAcquired);
                    return history;
                }
            }
            catch (SqliteException ex)
            {
                _logger?.LogWarning(
                    ex,
                    "取得履歴読み込み中に SqliteException が発生しました。CardId={CardId}",
                    cardId);
            }

            return null;
        }

        /// <summary>
        /// カードの取得ステータスを切り替える
        /// </summary>
        /// <param name="cardId">カードID</param>
        /// <param name="locationId">取得場所ID（オプション）</param>
        /// <returns>新しい取得ステータス</returns>
        public async Task<bool> ToggleCardAcquisitionStatusAsync(int cardId, int? locationId = null)
        {
            try
            {
                using var connection = new SqliteConnection(GetConnectionString());
                await connection.OpenAsync();

                var existingHistory = await GetAcquisitionHistoryAsync(cardId);
                bool newStatus;

                if (existingHistory == null)
                {
                    newStatus = true;
                }
                else
                {
                    newStatus = !existingHistory.IsAcquired;
                }

                await InsertAcquisitionHistoryAsync(new AcquisitionHistory
                {
                    CardId = cardId,
                    IsAcquired = newStatus,
                    AcquisitionDate = DateTimeOffset.Now,
                    LocationId = locationId ?? existingHistory?.LocationId,
                    CreatedDate = DateTimeOffset.Now
                });

                _logger?.LogInformation(
                    "カード取得ステータス変更: CardId={CardId}, NewStatus={NewStatus}",
                    cardId,
                    newStatus);
                return newStatus;
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "カード取得ステータス変更中に SqliteException が発生しました。CardId={CardId}",
                    cardId);
                throw;
            }
        }

        /// <summary>
        /// 全配布場所とそのカードを取得
        /// </summary>
        public async Task<List<DistributionLocationWithCards>> GetAllDistributionLocationsWithCardsAsync()
        {
            var locations = new Dictionary<int, DistributionLocationWithCards>();

            try
            {
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
                    COALESCE(latest_ah.IsAcquired, 0) as IsAcquired,
                    latest_ah.AcquisitionDate,
                    latest_ah.Notes
                FROM Locations l
                INNER JOIN Cards c ON l.CardId = c.CardId
                LEFT JOIN (
                    SELECT CardId, IsAcquired, AcquisitionDate, Notes
                    FROM AcquisitionHistory
                    WHERE (CardId, CreatedDate) IN (
                        SELECT CardId, MAX(CreatedDate)
                        FROM AcquisitionHistory
                        GROUP BY CardId
                    )
                ) latest_ah ON c.CardId = latest_ah.CardId
                ORDER BY l.LocationName, c.SeriesNumber
            ";

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var locationId = reader.GetInt32(DIST_LOCATION_ID_INDEX);
                    var cardId = reader.GetInt32(DIST_CARD_ID_INDEX);
                    var isAcquired = reader.GetInt32(DIST_IS_ACQUIRED_INDEX) == 1;
                    var acquisitionDate = reader.IsDBNull(DIST_ACQUISITION_DATE_INDEX) ? (DateTimeOffset?)null : DateTimeOffset.Parse(reader.GetString(DIST_ACQUISITION_DATE_INDEX));
                    var notes = reader.IsDBNull(DIST_NOTES_INDEX) ? null : reader.GetString(DIST_NOTES_INDEX);

                    if (!locations.ContainsKey(locationId))
                    {
                        locations[locationId] = new DistributionLocationWithCards
                        {
                            LocationId = locationId,
                            LocationName = reader.GetString(DIST_LOCATION_NAME_INDEX),
                            Address = reader.IsDBNull(DIST_ADDRESS_INDEX) ? null : reader.GetString(DIST_ADDRESS_INDEX),
                            Prefecture = reader.IsDBNull(DIST_PREFECTURE_INDEX) ? null : reader.GetString(DIST_PREFECTURE_INDEX),
                            Municipality = reader.IsDBNull(DIST_MUNICIPALITY_INDEX) ? null : reader.GetString(DIST_MUNICIPALITY_INDEX),
                            Description = reader.IsDBNull(DIST_DESCRIPTION_INDEX) ? null : reader.GetString(DIST_DESCRIPTION_INDEX)
                        };
                    }

                    locations[locationId].DistributedCards.Add(new CardWithAcquisitionStatus
                    {
                        CardId = cardId,
                        DesignImagePath = reader.IsDBNull(DIST_DESIGN_IMAGE_PATH_INDEX) ? null : reader.GetString(DIST_DESIGN_IMAGE_PATH_INDEX),
                        SeriesNumber = reader.GetInt32(DIST_SERIES_NUMBER_INDEX),
                        IssuedDate = reader.IsDBNull(DIST_ISSUED_DATE_INDEX) ? null : DateTimeOffset.Parse(reader.GetString(DIST_ISSUED_DATE_INDEX)),
                        Prefecture = reader.IsDBNull(DIST_PREFECTURE_INDEX) ? null : reader.GetString(DIST_PREFECTURE_INDEX),
                        Municipality = reader.IsDBNull(DIST_MUNICIPALITY_INDEX) ? null : reader.GetString(DIST_MUNICIPALITY_INDEX),
                        Description = reader.IsDBNull(DIST_DESCRIPTION_INDEX) ? null : reader.GetString(DIST_DESCRIPTION_INDEX),
                        StockStatus = reader.IsDBNull(DIST_STOCK_STATUS_INDEX) ? null : reader.GetString(DIST_STOCK_STATUS_INDEX),
                        IsAcquired = isAcquired,
                        AcquisitionDate = acquisitionDate,
                        Notes = notes
                    });
                }

                _logger?.LogInformation(
                    "全配布場所取得: {LocationCount} 件（カード総数={CardCount} 件）",
                    locations.Count,
                    locations.Values.Sum(l => l.DistributedCards.Count));
                return locations.Values.ToList();
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "全配布場所取得中に SqliteException が発生しました。");
                throw;
            }
        }

        /// <summary>
        /// サンプルデータを挿入（テスト用）
        /// </summary>
        public async Task InsertSampleDataAsync()
        {
            try
            {
                _logger?.LogInformation("サンプルデータ挿入を開始します。");

                using var connection = new SqliteConnection(GetConnectionString());
                await connection.OpenAsync();

                // データが既に存在する場合はスキップ
                var checkCommand = connection.CreateCommand();
                checkCommand.CommandText = "SELECT COUNT(*) FROM Locations";
                var count = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());
                if (count > 0)
                {
                    _logger?.LogInformation("サンプルデータは既に存在します。スキップします。");
                    return;
                }

                var now = DateTimeOffset.Now.ToString("O");

                // サンプル配布場所とカードを追加（1件ずつ）
                var location1 = connection.CreateCommand();
                location1.CommandText = @"
                    INSERT INTO Locations (LocationName, Prefecture, Municipality, Address, Description, StockStatus, CreatedDate)
                    VALUES ('東京都庁', '東京都', '新宿区', '東京都新宿区西新宿2-8-1', '平日 9:00-17:00', 'https://example.com/stock/tokyo', @now)
                ";
                location1.Parameters.AddWithValue("@now", now);
                await location1.ExecuteNonQueryAsync();

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

                // サンプルカードを追加（LocationIdを設定）
                var card1 = connection.CreateCommand();
                card1.CommandText = @"
                    INSERT INTO Cards (LocationId, SeriesNumber, CreatedDate, UpdatedDate)
                    VALUES (1, 1, @now, @now)
                ";
                card1.Parameters.AddWithValue("@now", now);
                await card1.ExecuteNonQueryAsync();

                var card2 = connection.CreateCommand();
                card2.CommandText = @"
                    INSERT INTO Cards (LocationId, SeriesNumber, CreatedDate, UpdatedDate)
                    VALUES (2, 1, @now, @now)
                ";
                card2.Parameters.AddWithValue("@now", now);
                await card2.ExecuteNonQueryAsync();

                var card3 = connection.CreateCommand();
                card3.CommandText = @"
                    INSERT INTO Cards (LocationId, SeriesNumber, CreatedDate, UpdatedDate)
                    VALUES (3, 2, @now, @now)
                ";
                card3.Parameters.AddWithValue("@now", now);
                await card3.ExecuteNonQueryAsync();

                _logger?.LogInformation(
                    "サンプルデータ挿入完了: 3つの配布場所と3枚のカードを追加しました。");
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "サンプルデータ挿入中に SqliteException が発生しました。");
                throw;
            }
        }

        /// <summary>
        /// カード取得履歴を更新または追加
        /// </summary>
        /// <param name="cardId">カードID</param>
        /// <param name="acquisitionDate">取得日時（ISO 8601 拡張形式）</param>
        /// <param name="notes">備考</param>
        public async Task UpdateAcquisitionHistoryAsync(
            int cardId,
            DateTimeOffset? acquisitionDate,
            string? notes)
        {
            try
            {
                _logger?.LogInformation(
                    "カード取得履歴を更新または追加します: CardId={CardId}",
                    cardId);

                using var connection = new SqliteConnection(GetConnectionString());
                await connection.OpenAsync();

                // 既存の取得履歴を確認
                var checkQuery = "SELECT COUNT(*) FROM AcquisitionHistory WHERE CardId = @cardId";
                using var checkCommand = new SqliteCommand(checkQuery, connection);
                checkCommand.Parameters.AddWithValue("@cardId", cardId);
                var result = await checkCommand.ExecuteScalarAsync();
                var exists = result != null && (long)result > 0;

                if (exists)
                {
                    // 既存レコードを更新（最新の1件のみを更新）
                    var updateQuery = @"
                        UPDATE AcquisitionHistory
                        SET AcquisitionDate = @acquisitionDate,
                            Notes = @notes
                        WHERE HistoryId = (
                            SELECT HistoryId FROM AcquisitionHistory
                            WHERE CardId = @cardId
                            ORDER BY CreatedDate DESC
                            LIMIT 1
                        )
                    ";
                    using var updateCommand = new SqliteCommand(updateQuery, connection);
                    updateCommand.Parameters.AddWithValue("@cardId", cardId);
                    updateCommand.Parameters.AddWithValue(
                        "@acquisitionDate",
                        acquisitionDate?.ToString("O") ?? (object)DBNull.Value);
                    updateCommand.Parameters.AddWithValue(
                        "@notes",
                        notes ?? (object)DBNull.Value);

                    await updateCommand.ExecuteNonQueryAsync();
                    _logger?.LogInformation(
                        "カード取得履歴を更新しました: CardId={CardId}",
                        cardId);
                }
                else
                {
                    // 新規レコードを挿入
                    var insertQuery = @"
                        INSERT INTO AcquisitionHistory
                        (CardId, IsAcquired, AcquisitionDate, Notes, CreatedDate)
                        VALUES (@cardId, 1, @acquisitionDate, @notes, @createdDate)
                    ";
                    using var insertCommand = new SqliteCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@cardId", cardId);
                    insertCommand.Parameters.AddWithValue(
                        "@acquisitionDate",
                        acquisitionDate?.ToString("O") ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue(
                        "@notes",
                        notes ?? (object)DBNull.Value);
                    insertCommand.Parameters.AddWithValue(
                        "@createdDate",
                        DateTimeOffset.UtcNow.ToString("O"));

                    await insertCommand.ExecuteNonQueryAsync();
                    _logger?.LogInformation(
                        "カード取得履歴を新規追加しました: CardId={CardId}",
                        cardId);
                }
            }
            catch (SqliteException ex)
            {
                _logger?.LogError(
                    ex,
                    "カード取得履歴更新中に SqliteException が発生しました。CardId={CardId}",
                    cardId);
                throw;
            }
        }
    }
}
