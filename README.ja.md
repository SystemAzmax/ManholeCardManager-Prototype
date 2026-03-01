# マンホールカードマネージャー (Manhole Card Manager)

[English](README.md) | 日本語

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![Windows App SDK](https://img.shields.io/badge/Windows%20App%20SDK-1.8-0078D4?logo=windows)](https://github.com/microsoft/WindowsAppSDK)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

マンホールカードのコレクションを管理するためのWindows デスクトップアプリケーションです。

## 📖 概要

マンホールカードマネージャーは、日本全国で配布されているマンホールカードの収集状況を管理できるアプリケーションです。カードコレクションビューと配布場所ビューの2つのビューを備え、効率的にコレクションを整理できます。

### 主な機能

- ✅ **カードコレクション管理**: 取得したマンホールカードを一覧表示・管理
- 📍 **配布場所一覧**: 全国の配布場所情報を都道府県・市区町村別に表示
- 🎯 **取得状況トラッキング**: カードの取得日や状態を記録
- 🌐 **多言語対応**: 日本語・英語のUI切り替えに対応
- 🗄️ **ローカルデータベース**: SQLiteによる軽量で高速なデータ管理
- 🖼️ **画像キャッシュ**: カード画像の効率的な表示

## 🚀 はじめに

### システム要件

- **OS**: Windows 10 (19041) 以降
- **ランタイム**: .NET 8.0
- **アーキテクチャ**: x64

### インストール

1. リポジトリをクローンします：
```bash
git clone https://github.com/SystemAzmax/ManholeCardManager-Prototype.git
cd ManholeCardManager
```

2. ソリューションをビルドします：
```bash
dotnet build
```

3. アプリケーションを実行します：
```bash
cd ManholeCardManager\ManholeCardManager
dotnet run
```

## 🏗️ プロジェクト構成

このリポジトリは以下の3つのプロジェクトで構成されています：

### 1. ManholeCardManager（メインアプリケーション）
WinUI 3 を使用したデスクトップアプリケーション。

**主要コンポーネント:**
- `ViewModels/`: MVVM パターンのビューモデル
  - `MainWindowViewModel.cs`: メインウィンドウのロジック
  - `CardCollectionViewModel.cs`: カードコレクションビューの管理
  - `DistributionLocationViewModel.cs`: 配布場所ビューの管理
- `Services/`: アプリケーションサービス
  - `DatabaseService.cs`: SQLite データベース操作
  - `LocalizationService.cs`: 多言語対応
  - `ImageCacheService.cs`: 画像キャッシュ管理
- `Models/`: データモデル
  - `ManholeCard.cs`: マンホールカード情報
  - `CardLocation.cs`: カード配布場所
  - `AcquisitionHistory.cs`: 取得履歴

### 2. ManholeCardDataScraper（データスクレイパー）
下水道広報プラットホーム（GK-P）から配布場所データを取得するツール。

詳細は [ManholeCardDataScraper/README.md](ManholeCardDataScraper/README.md) を参照してください。

### 3. ManholeCardManager.Tests（テストプロジェクト）
ユニットテストプロジェクト。

## 🎮 使い方

### 初回起動時

1. アプリケーションを起動すると、自動的にローカルデータベースが作成されます
2. データスクレイパーを実行して配布場所データを同期することをお勧めします

### カードの管理

1. **カードコレクションビュー**: 取得したカードを一覧表示
   - 各カードの画像、場所、取得日を表示
   - 弾数（シリーズ番号）でフィルタリング可能

2. **配布場所ビュー**: 配布場所を地域別に表示
   - 都道府県・市区町村で絞り込み
   - 各場所で配布されているカード情報を確認
   - 配布場所の住所や配布方法を表示

### 言語切り替え

設定から日本語/英語を切り替えることができます。

## 🔧 技術スタック

- **フレームワーク**: .NET 8.0
- **UI**: WinUI 3 (Windows App SDK 1.8)
- **データベース**: SQLite (Microsoft.Data.Sqlite)
- **アーキテクチャ**: MVVM パターン
- **ロギング**: Microsoft.Extensions.Logging

## 📊 データベーススキーマ

アプリケーションは以下の主要テーブルを使用します：

- `ManholeCards`: カード情報
- `CardLocations`: 配布場所情報
- `ManholeLocations`: マンホール設置場所
- `AcquisitionHistory`: カード取得履歴

## 🛠️ 開発

### ビルド

```bash
dotnet build
```

### テスト実行

```bash
dotnet test
```

### デバッグ実行

Visual Studio 2022 でソリューションを開き、F5 キーでデバッグを開始します。

## 📝 ライセンス

このプロジェクトは [MIT ライセンス](LICENSE) の下で公開されています。

## 🤝 コントリビューション

Issue や Pull Request を歓迎します！

1. このリポジトリをフォーク
2. フィーチャーブランチを作成 (`git checkout -b feature/amazing-feature`)
3. 変更をコミット (`git commit -m 'Add some amazing feature'`)
4. ブランチにプッシュ (`git push origin feature/amazing-feature`)
5. Pull Request を作成

## 📮 お問い合わせ

プロジェクトに関する質問や提案は、[Issues](https://github.com/SystemAzmax/ManholeCardManager-Prototype/issues) からお願いします。

## 🙏 謝辞

- マンホールカード配布場所データは [下水道広報プラットホーム（GK-P）](https://www.gk-p.jp/) より取得しています

---

**注**: このプロトタイプは開発中のため、機能や仕様が変更される可能性があります。
