# データベース設計書

## 概要
このドキュメントは、マンホールカード管理システムのデータベース設計を説明します。

- **データベース種類**: SQLite
- **データベースファイル**: `manholecard.db`
- **保存場所**: `%LocalAppData%\ManholeCardManager\manholecard.db`

---

## テーブル一覧

| テーブル名 | 説明 |
|-----------|------|
| Cards | マンホールカード情報 |
| Locations | カード配布場所情報（都道府県・市町村・在庫状況を含む） |
| ManholeLocations | マンホール設置場所とカードの関連 |
| AcquisitionHistory | カード取得履歴 |

---

## テーブル定義

### 1. Cards（マンホールカード）

マンホールカードの基本情報を管理するテーブル

| カラム名 | データ型 | NULL | デフォルト | 説明 |
|---------|---------|------|-----------|------|
| CardId | INTEGER | NOT NULL | AUTOINCREMENT | カードID（主キー） |
| LocationId | INTEGER | NULL | - | 場所ID（外部キー） |
| DesignImagePath | TEXT | NULL | - | カードデザイン画像パス |
| SeriesNumber | INTEGER | NOT NULL | 1 | 弾数（バージョン番号） |
| IssuedDate | TEXT | NULL | - | 発行年月日（ISO 8601形式） |
| CreatedDate | TEXT | NOT NULL | - | 登録日時（ISO 8601形式） |
| UpdatedDate | TEXT | NOT NULL | - | 更新日時（ISO 8601形式） |

**主キー**: CardId  
**外部キー**: LocationId → Locations(LocationId)

---

### 2. Locations（カード配布場所）

カードの配布場所情報を管理するテーブル  
**都道府県と市町村、在庫状況の情報はこのテーブルで管理されます**

| カラム名 | データ型 | NULL | デフォルト | 説明 |
|---------|---------|------|-----------|------|
| LocationId | INTEGER | NOT NULL | AUTOINCREMENT | 場所ID（主キー） |
| CardId | INTEGER | NULL | - | カードID（外部キー） |
| LocationName | TEXT | NOT NULL | - | 場所名 |
| Prefecture | TEXT | NULL | - | **都道府県** |
| Municipality | TEXT | NULL | - | **市町村** |
| Address | TEXT | NULL | - | 住所 |
| Latitude | REAL | NULL | - | 緯度 |
| Longitude | REAL | NULL | - | 経度 |
| Description | TEXT | NULL | - | **説明（配布場所の概要・営業時間など）** |
| StockStatus | TEXT | NULL | - | **在庫状況** |
| CreatedDate | TEXT | NOT NULL | - | 登録日時（ISO 8601形式） |

**主キー**: LocationId  
**外部キー**: CardId → Cards(CardId)

---

### 3. ManholeLocations（マンホール設置場所関連）

マンホール設置場所とカードの関連を管理するテーブル  
**実際のマンホール設置場所（撮影スポット）の緯度経度を管理します**

| カラム名 | データ型 | NULL | デフォルト | 説明 |
|---------|---------|------|-----------|------|
| RelationId | INTEGER | NOT NULL | AUTOINCREMENT | 関連ID（主キー） |
| LocationId | INTEGER | NOT NULL | - | 場所ID（外部キー） |
| CardId | INTEGER | NOT NULL | - | カードID（外部キー） |
| Latitude | REAL | NULL | - | **マンホール設置場所の緯度** |
| Longitude | REAL | NULL | - | **マンホール設置場所の経度** |
| SpotName | TEXT | NULL | - | **マンホール設置場所の名称** |
| Description | TEXT | NULL | - | **マンホール設置場所の説明** |

**主キー**: RelationId  
**外部キー**: 
- LocationId → Locations(LocationId)
- CardId → Cards(CardId)

**補足**: 
- Locationsテーブルはカード配布場所（市役所、観光案内所など）
- ManholeLocationsテーブルは実際のマンホール設置場所（道路上の撮影スポット）
- 1つのカードに対して複数のマンホール設置場所が存在する場合があります

---

### 4. AcquisitionHistory（カード取得履歴）

ユーザーがカードを取得した履歴を管理するテーブル

| カラム名 | データ型 | NULL | デフォルト | 説明 |
|---------|---------|------|-----------|------|
| HistoryId | INTEGER | NOT NULL | AUTOINCREMENT | 履歴ID（主キー） |
| CardId | INTEGER | NOT NULL | - | カードID（外部キー） |
| **IsAcquired** | **INTEGER** | **NOT NULL** | **1** | **取得ステータス（1: 取得済み、0: 未取得）** |
| AcquisitionDate | TEXT | NOT NULL | - | 取得日時（ISO 8601形式） |
| LocationId | INTEGER | NULL | - | 取得場所のID（外部キー） |
| Notes | TEXT | NULL | - | 備考 |
| CreatedDate | TEXT | NOT NULL | - | 登録日時（ISO 8601形式） |

**主キー**: HistoryId  
**外部キー**: 
- CardId → Cards(CardId)
- LocationId → Locations(LocationId)

**補足**:
- **IsAcquired**: カードの取得/未取得状態を管理します
- 各カードの最新の取得ステータスは、CreatedDateで降順ソートして最初のレコードから取得します
- ユーザーが取得ステータスを切り替えるたびに、新しい履歴レコードが作成されます

---

## ER図（テキスト表現）

```
Cards (1) ─────< (N) ManholeLocations (N) >───── (1) Locations

Cards (1) ────< (N) AcquisitionHistory
                         │
                         │ (N)
                         ▼ (1)
                     Locations
```

---

## 主要な設計ポイント

### 1. データ正規化
- **都道府県（Prefecture）、市町村（Municipality）、在庫状況（StockStatus）はLocationsテーブルで管理**
- カードの基本情報と配布場所情報を分離し、第3正規形を実現

### 2. フィルタ機能
以下の検索メソッドがDatabaseServiceで提供されます：
- `SearchCardsAsync(string query)` - 都道府県・市町村で検索
- `GetCardsByPrefectureAsync(string prefecture)` - 都道府県で検索
- `GetCardsBySeriesNumberAsync(int seriesNumber)` - 弾数で検索
- `GetCardsByPrefectureAndSeriesAsync(string prefecture, int seriesNumber)` - 複合フィルタ

### 3. 日付データの管理
- すべての日付・日時データはISO 8601形式（YYYY-MM-DDTHH:mm:ss.fffzzz）で保存
- .NETの`DateTimeOffset`型と互換性あり

### 4. NULLの許容
- 必須項目以外はNULLを許容し、柔軟なデータ管理を実現

### 5. 主キーの設計
- すべてのテーブルでAUTOINCREMENTを使用し、自動採番

---

## インデックス推奨

パフォーマンス向上のため、以下のインデックス作成を推奨します：

```sql
-- Locationsテーブルの検索用
CREATE INDEX idx_locations_cardid ON Locations(CardId);
CREATE INDEX idx_locations_prefecture ON Locations(Prefecture);
CREATE INDEX idx_locations_municipality ON Locations(Municipality);

-- AcquisitionHistoryテーブルの検索用
CREATE INDEX idx_acquisitionhistory_cardid ON AcquisitionHistory(CardId);
```

---

## バックアップ・復元

### バックアップ
```bash
# データベースファイルをコピー
copy "%LocalAppData%\ManholeCardManager\manholecard.db" backup_location
```

### 復元
```bash
# バックアップファイルを元の場所にコピー
copy backup_location\manholecard.db "%LocalAppData%\ManholeCardManager\"
```

---

## データベースマイグレーション

### マイグレーション履歴

#### v3.1.0: AcquisitionHistoryテーブルにIsAcquiredカラム追加

**実行日**: 2025-01-XX  
**目的**: カードの取得/未取得ステータスを管理する機能を追加

**既存データベースへの手動マイグレーション方法**:

1. **SQLiteクライアントのインストール**
   - [DB Browser for SQLite](https://sqlitebrowser.org/) をダウンロードしてインストール
   - または、SQLiteコマンドラインツールを使用

2. **データベースファイルを開く**
   ```
   データベースファイルの場所:
   %LocalAppData%\ManholeCardManager\manholecard.db
   
   フルパス例:
   C:\Users\[ユーザー名]\AppData\Local\ManholeCardManager\manholecard.db
   ```

3. **バックアップの作成（重要！）**
   ```bash
   copy "%LocalAppData%\ManholeCardManager\manholecard.db" "%LocalAppData%\ManholeCardManager\manholecard_backup.db"
   ```

4. **マイグレーションSQLの実行**

   **DB Browser for SQLiteを使用する場合**:
   - データベースファイルを開く
   - 「SQL実行」タブを選択
   - 以下のSQLを貼り付けて実行

   **SQLiteコマンドラインを使用する場合**:
   ```bash
   sqlite3 "%LocalAppData%\ManholeCardManager\manholecard.db"
   ```

   **実行するSQL**:
   ```sql
   -- IsAcquiredカラムを追加（デフォルト値: 1 = 取得済み）
   ALTER TABLE AcquisitionHistory 
   ADD COLUMN IsAcquired INTEGER NOT NULL DEFAULT 1;
   
   -- テーブル構造の確認
   PRAGMA table_info(AcquisitionHistory);
   ```

5. **マイグレーション結果の確認**
   
   以下のカラムが表示されることを確認:
   ```
   cid | name             | type    | notnull | dflt_value | pk
   ----|------------------|---------|---------|------------|----
   0   | HistoryId        | INTEGER | 1       |            | 1
   1   | CardId           | INTEGER | 1       |            | 0
   2   | IsAcquired       | INTEGER | 1       | 1          | 0  ← 新しく追加されたカラム
   3   | AcquisitionDate  | TEXT    | 1       |            | 0
   4   | LocationId       | INTEGER | 0       |            | 0
   5   | Notes            | TEXT    | 0       |            | 0
   6   | CreatedDate      | TEXT    | 1       |            | 0
   ```

6. **アプリケーションの再起動**
   - マイグレーション完了後、アプリケーションを再起動
   - 取得ステータスの切り替え機能が正常に動作することを確認

**ロールバック方法**（問題が発生した場合）:
```bash
# バックアップから復元
copy "%LocalAppData%\ManholeCardManager\manholecard_backup.db" "%LocalAppData%\ManholeCardManager\manholecard.db"
```

**注意事項**:
- マイグレーション実行前には**必ずバックアップ**を作成してください
- 既存の履歴データには `IsAcquired = 1`（取得済み）が自動設定されます
- マイグレーション後は、アプリケーション側で自動的にマイグレーションチェックが行われます

---

## 変更履歴

| 日付 | バージョン | 変更内容 |
|-----|----------|---------|
| 2025-01-XX | 1.0.0 | 初版作成（SQLite移行） |
| 2025-01-XX | 1.1.0 | PrefectureとMunicipalityをLocationsテーブルに移動 |
| 2025-01-XX | 2.0.0 | カテゴリー機能を削除、都道府県・弾数フィルタ機能を実装 |
| 2025-01-XX | 2.1.0 | 「カード設置場所」を「カード配布場所」に統一 |
| 2025-01-XX | 3.0.0 | DistributionLocationsテーブル削除、StockStatusをLocationsに統合 |
| 2025-01-XX | 3.1.0 | AcquisitionHistoryテーブルにIsAcquiredカラム追加（カード取得記録機能） |
