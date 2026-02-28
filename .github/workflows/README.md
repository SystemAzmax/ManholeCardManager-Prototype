# GitHub Actions CI/CD パイプライン

このプロジェクトはGitHub Actionsを使用した自動CI/CDパイプラインを実装しています。

## 📋 ワークフロー一覧

### 1. **dotnet-test.yml** - ユニットテスト実行
```yaml
実行トリガー:
  - push: main, develop, feature/** ブランチ
  - pull_request: main, develop ブランチ
```

**実行内容:**
- ✅ .NET 8のセットアップ
- ✅ 依存関係の復元
- ✅ Release構成でのビルド
- ✅ ユニットテスト実行
- ✅ テスト結果の保存・レポート

**成果物:**
- Test Results (TRX形式)
- PR に自動コメント

### 2. **coverage.yml** - コードカバレッジ分析
```yaml
実行トリガー:
  - push: main, develop ブランチ
  - pull_request: main, develop ブランチ
```

**実行内容:**
- ✅ OpenCover形式でのカバレッジ計測
- ✅ Codecov へのアップロード
- ✅ カバレッジレポート生成

**成果物:**
- Codecov バッジ
- カバレッジレポート

### 3. **build.yml** - ビルド検証
```yaml
実行トリガー:
  - push: すべてのブランチ
  - pull_request: main, develop ブランチ
```

**実行内容:**
- ✅ Debug & Release 構成でのビルド
- ✅ x64 プラットフォームでの検証

## 🚀 セットアップ手順

### 1. Codecov統合（オプション）
Codecovを使用する場合：

```bash
# 1. Codecov にログイン
# https://codecov.io

# 2. GitHub リポジトリを接続
# Settings > Connected Repositories

# 3. トークン取得（オプション、public repoは不要）
```

### 2. Secrets設定（オプション）
```bash
# GitHub リポジトリ Settings > Secrets and variables > Actions

# 必要なシークレット（オプション）
CODECOV_TOKEN  # Codecov プライベートリポジトリの場合
```

## 📊 ワークフロー実行状態の確認

### GitHub UI で確認
1. リポジトリのトップページ
2. **Actions** タブをクリック
3. ワークフロー実行履歴が表示されます

### コマンドラインで確認
```bash
# GitHub CLI がインストール済みの場合
gh run list
gh run view <run-id>
```

## 📝 テスト結果の見方

### Pull Request への自動コメント
テスト実行後、PRに以下の情報が自動でコメントされます：

```
✅ 29 passed
❌ 0 failed
⏭️ 0 skipped

テスト実行時間: 3.2 秒
```

### テスト結果の詳細確認
1. PR の **Checks** タブ
2. **Unit Test Results** を展開
3. 各テストの実行結果を確認

## 🔄 キャッシング（パフォーマンス最適化）

現在のワークフローは基本的な構成です。必要に応じて以下を追加可能：

```yaml
- name: Cache NuGet packages
  uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

## ❌ トラブルシューティング

### 1. ワークフローが実行されない
**原因:** paths が一致していない

**解決:**
```yaml
paths:
  - 'ManholeCardManager/**'
  - '.github/workflows/dotnet-test.yml'
```

### 2. テストが失敗する
**確認項目:**
```bash
# ローカルで再現
dotnet test ManholeCardManager/ManholeCardManager.Tests/

# ビルド確認
dotnet build --configuration Release
```

### 3. WinUI依存エラー
**原因:** WinUI3 は Windows 環境が必要

**解決:** ワークフローは `windows-latest` を使用（✅ 設定済み）

## 📈 カスタマイズ例

### 1. スケジュール実行（毎日テスト）
```yaml
on:
  schedule:
    - cron: '0 2 * * *'  # 毎日 02:00 UTC
```

### 2. 複数 .NET バージョンでテスト
```yaml
strategy:
  matrix:
    dotnet-version: ['7.0.x', '8.0.x']
```

### 3. Slack 通知
```yaml
- name: Slack Notification
  uses: slackapi/slack-github-action@v1
  if: failure()
```

## 📚 リンク

- [GitHub Actions ドキュメント](https://docs.github.com/en/actions)
- [.NET Testing in Actions](https://github.com/actions/setup-dotnet)
- [Codecov](https://codecov.io)

## ⚙️ 現在の設定

| ワークフロー | トリガー | テスト | ビルド |
|-----------|---------|--------|---------|
| dotnet-test.yml | push/PR | ✅ | ✅ |
| coverage.yml | push/PR | ✅ | - |
| build.yml | push/PR | - | ✅ |

全ワークフローが正常に機能しています！
