# マンホールカード配布場所データスクレイパー

## 概要

このツールは、下水道広報プラットホーム（GK-P）の公式サイト
（https://www.gk-p.jp/mhcard/?pref=zenkoku#mhcard_result）から
マンホールカード配布場所情報をスクレイピングし、
アプリケーションの SQLite データベースに同期します。

## 特徴

- **自動スクレイピング**：Webページから配布場所情報を自動取得
- **差分同期**：新規データのみを追加、既存データの更新にも対応
- **ハッシュベース検出**：コンテンツのハッシュで変更を検出
- **エラーハンドリング**：ネットワークエラーやDB エラーに対応

## 使用方法

### 基本的な実行方法

```bash
cd ManholeCardDataScraper
dotnet run
```

### 出力例

```
=== Manhole Card Data Scraper ===
Scraping distribution location data from GK-P

Fetching data from: https://www.gk-p.jp/mhcard/?pref=zenkoku#mhcard_result
Scraped 500 locations
Database initialized successfully
Starting scraping operation...
Added new location: 札幌市下水道科学館
Updated location: 北見市役所
Sync completed. Added/Updated: 45 locations

=== Scraping Complete ===
Total locations added/updated: 45
```

## システム要件

- .NET 8.0 以上
- Windows（ApplicationData.Current.LocalFolder を使用のため）
- インターネット接続（Webスクレイピング用）

## 依存パッケージ

- `HtmlAgilityPack`：HTML パースおよびスクレイピング
- `sqlite-net-pcl`：SQLite データベース操作
- `SQLitePCLRaw.bundle_green`：SQLite ネイティブライブラリ

## ファイル構成

```
ManholeCardDataScraper/
├── Program.cs                      # エントリーポイント
├── Models/
│   └── DistributionLocationData.cs # スクレイピングデータモデル
├── Services/
│   ├── WebScraperService.cs        # Webスクレイピング
│   ├── DatabaseSyncService.cs      # DB差分同期
│   └── LocationScraperService.cs   # メイン処理
└── ManholeCardDataScraper.csproj
```

## 実装詳細

### WebScraperService

- URL: `https://www.gk-p.jp/mhcard/?pref=zenkoku#mhcard_result`
- 解析対象：`table > tr > td` 要素
- ハッシュ生成：SHA256 でコンテンツをハッシュ化

### DatabaseSyncService

差分検出ロジック：

1. **新規判定**：`LocationName` が DB に存在しないか確認
2. **新規追加**：新しい場所を `CardLocation` テーブルに INSERT
3. **更新判定**：`Address` または `Description` の変更を検出
4. **更新処理**：変更があれば UPDATE を実行

### LocationScraperService

主な処理フロー：

```
1. データベース初期化
2. Webスクレイピング実行
3. 取得データで差分同期
4. リソース解放
```

## トラブルシューティング

### エラー：「Database not initialized」

→ `InitializeAsync()` が正常に完了しているか確認してください。

### エラー：「No table rows found」

→ Webサイトのレイアウトが変更された可能性があります。
   HTMLセレクタを確認してください。

### エラー：「Permission denied」

→ SQLite データベースファイルがロックされている可能性があります。
   メインアプリケーションを終了して再実行してください。

## 配布パッケージからの除外

このツールはコンソールアプリケーション（EXE）として独立しており、
WPF アプリケーションの配布パッケージには含まれません。

必要に応じて開発環境でのみ実行してください。

## コーディング規約に準拠

- XMLドキュメントコメント
- PascalCase（クラス・メソッド名）
- camelCase（変数・フィールド）
- 1行80文字以内（推奨）
- スペースインデント（タブなし）

## ライセンス

このツールはManholeCardManager プロジェクトの一部です。
