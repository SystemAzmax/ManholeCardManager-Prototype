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
| AcquisitionDate | TEXT | NOT NULL | - | 取得日時（ISO 8601形式） |
| LocationId | INTEGER | NULL | - | 取得場所のID（外部キー） |
| Notes | TEXT | NULL | - | 備考 |
| CreatedDate | TEXT | NOT NULL | - | 登録日時（ISO 8601形式） |

**主キー**: HistoryId  
**外部キー**: 
- CardId → Cards(CardId)
- LocationId → Locations(LocationId)

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

## 変更履歴

| 日付 | バージョン | 変更内容 |
|-----|----------|---------|
| 2025-01-XX | 1.0.0 | 初版作成（SQLite移行） |
| 2025-01-XX | 1.1.0 | PrefectureとMunicipalityをLocationsテーブルに移動 |
| 2025-01-XX | 2.0.0 | カテゴリー機能を削除、都道府県・弾数フィルタ機能を実装 |
| 2025-01-XX | 2.1.0 | 「カード設置場所」を「カード配布場所」に統一 |
| 2025-01-XX | 3.0.0 | DistributionLocationsテーブル削除、StockStatusをLocationsに統合 |
