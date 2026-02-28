# ManholeCardManager.Tests

Phase1テスト項目の自動化テストプロジェクトです。

## テスト概要

### ✅ 実装済みテスト

#### Services
- **SimpleFileLoggerTests** (S-050～S-054)
  - ログ出力（Information、Error with Exception）
  - IsEnabled判定（None、Information）
  - ログディレクトリ自動作成
  - 複数ログレベル対応
  - 日付ベースファイル名

#### Models
- **CardWithAcquisitionStatusTests** (M-010～M-013)
  - PropertyChanged通知（IsAcquired、AcquisitionDate、SaveStatus）
  - SaveStatus初期値確認
  - AcquisitionStatusDisplay
  - AcquisitionDateDisplay

- **ManholeCardTests** (M-001～M-003)
  - デフォルト値確認
  - プロパティの設定と取得
  - IssuedDateのnull許容

- **DistributionLocationWithCardsTests** (M-030～M-032)
  - TotalCardCount
  - AcquiredCardCount
  - DistributedCards初期値

- **AcquisitionHistoryTests** (M-040～M-041)
  - プロパティの設定と取得
  - Notesのnull許容

### テスト実行方法

```powershell
# すべてのテストを実行
dotnet test

# 詳細出力で実行
dotnet test --verbosity normal

# 特定のテストクラスのみ実行
dotnet test --filter "FullyQualifiedName~SimpleFileLoggerTests"
```

### テスト結果

- **合計**: 29テスト
- **成功**: 29テスト
- **失敗**: 0テスト
- **スキップ**: 0テスト

## テストフレームワーク

- **xUnit** 2.9.3
- **Moq** 4.20.72 (モッキング)
- **FluentAssertions** 6.12.2 (アサーション)
- **Microsoft.Data.Sqlite** 8.0.11

## プロジェクト構造

```
ManholeCardManager.Tests/
├── Services/
│   └── SimpleFileLoggerTests.cs
├── Models/
│   └── ModelsTests.cs
└── ManholeCardManager.Tests.csproj
```

## 注意事項

### WinUI依存の制限

以下のクラスはWinUI 3 APIに依存しているため、標準的な.NET 8テストプロジェクトでは直接テストできません：

- `DatabaseService` (Windows.Storage API使用)
- `ImageCacheService` (Windows.Storage API使用)
- ViewModels (UI スレッド依存)

これらのテストを自動化するには以下のアプローチが必要です：

1. **依存性注入のリファクタリング**: Windows API を抽象化し、モック可能にする
2. **統合テスト**: WinUI環境で実行される統合テストプロジェクトを作成
3. **手動テスト**: Phase1_結合テスト.csvに従った手動テスト

## 今後の拡張

### Phase 2: UI自動化テスト
- WinAppDriver を使用したUI自動化
- Coded UIテストまたはAppium

### Phase 3: 統合テスト
- DatabaseServiceの統合テスト（テスト用WinUI環境）
- ViewModelの統合テスト

### 優先度「高」の残タスク
- **S-001～S-020**: DatabaseServiceテスト（WinUI環境が必要）
- **I-020～I-021、VM-025**: 取得ステータス切替の統合テスト
- **I-030～I-035**: 保存フローの統合テスト

## コーディング規約遵守

このテストコードは `.github/copilot-instructions.md` のコーディング規約に準拠しています：

✅ メソッドにXMLドキュメントコメントを記載  
✅ PascalCase（クラス・メソッド）、camelCase（変数）の命名規則  
✅ 適切なインデント（スペース使用）  
✅ 例外処理の適切な実装  
✅ コードの再利用性（ヘルパーメソッド）

## CI/CD統合

GitHub Actionsでの自動実行例:

```yaml
- name: Run Tests
  run: dotnet test --configuration Release --logger trx --results-directory ./TestResults
```
