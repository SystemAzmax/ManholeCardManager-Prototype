# GitHub Actions CI/CD パイプライン

このプロジェクトはGitHub Actionsを使用した自動CI/CDパイプラインを実装しています。

## 📋 ワークフロー一覧

### 1. **dotnet-test.yml** - ユニットテスト実行 ⭐ メイン
```yaml
実行トリガー:
  - push: main, develop, feature/** ブランチ
  - pull_request: main, develop ブランチ
```

**実行内容:**
- ✅ .NET 8のセットアップ
- ✅ 依存関係の復元
- ✅ Release構成でのビルド
- ✅ ユニットテスト実行（29テスト）
- ✅ テスト結果の保存・アップロード

**成果物:**
- Test Results (TRX形式) → Artifacts にアップロード

### 2. **coverage.yml** - コードカバレッジ分析
```yaml
実行トリガー:
  - push: main, develop ブランチ
  - pull_request: main, develop ブランチ
```

**実行内容:**
- ✅ OpenCover形式でのカバレッジ計測
- ✅ Codecov へのアップロード
- ✅ カバレッジレポート保存

**成果物:**
- Codecov バッジ
- カバレッジレポート → Artifacts にアップロード

### 3. **build.yml** - テストプロジェクトビルド検証
```yaml
実行トリガー:
  - push: すべてのブランチ
  - pull_request: main, develop ブランチ
```

**実行内容:**
- ✅ テストプロジェクトの Debug ビルド検証
- ✅ テストプロジェクトの Release ビルド検証

**注意:** WinUI プロジェクト（ManholeCardManager.csproj）のビルドは
複雑なため、テストプロジェクトに集約しています。

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

### Artifacts をダウンロード
1. PR の **Summary** ページ
2. 下部の **Artifacts** セクション
3. `test-results` をダウンロード
4. TRX ファイルをテストエクスプローラーで開く

### テスト結果の詳細確認
1. PR の **Checks** タブ
2. **.NET Tests** ワークフローを展開
3. **Run unit tests** ステップで詳細確認

## ✅ ワークフロー実行フロー

```
feature ブランチへの push
    ↓
GitHub Actions トリガー
    ↓
3つのワークフロー並列実行
├─ build.yml (テストプロジェクトビルド検証)
├─ dotnet-test.yml (ユニットテスト実行 ⭐)
└─ coverage.yml (コードカバレッジ分析)
    ↓
全て成功 → PR マージ可能
いずれか失敗 → 修正必要
```

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

### Q1: ワークフローが実行されない
**A:** `.github/workflows/` ファイルが存在し、`on:` セクションが設定されているか確認

```yaml
paths:
  - 'ManholeCardManager/**'
  - '.github/workflows/build.yml'
```

### Q2: テストが失敗する
**A:** ローカルで再現してみてください

```bash
# 同じ環境でテスト
dotnet test ManholeCardManager/ManholeCardManager.Tests/ --configuration Release

# ビルド確認
dotnet build ManholeCardManager/ManholeCardManager.Tests/ --configuration Release
```

### Q3: "The strategy configuration was canceled because build failed"
**A:** 通常はテストプロジェクトのビルドエラーです

**確認:**
```bash
# ローカルで再現
dotnet build ManholeCardManager/ManholeCardManager.Tests/

# 詳細ログで確認
dotnet build ManholeCardManager/ManholeCardManager.Tests/ --verbosity diagnostic
```

### Q4: WinUI アプリ のビルドワークフローが必要
**A:** `build.yml` をカスタマイズしてください（複雑なため別途対応推奨）

## 📈 カスタマイズ例

### 1. スケジュール実行（毎日テスト）
```yaml
on:
  schedule:
    - cron: '0 2 * * *'  # 毎日 02:00 UTC = JST 11:00
```

### 2. 複数 .NET バージョンでテスト
```yaml
strategy:
  matrix:
    dotnet-version: ['7.0.x', '8.0.x']
```

### 3. Slack 通知（テスト失敗時）
```yaml
- name: Notify Slack on failure
  if: failure()
  uses: slackapi/slack-github-action@v1
  with:
    webhook-url: ${{ secrets.SLACK_WEBHOOK }}
```

## 📚 リンク

- [GitHub Actions ドキュメント](https://docs.github.com/en/actions)
- [.NET Testing in Actions](https://github.com/actions/setup-dotnet)
- [Codecov](https://codecov.io)

## ⚙️ 現在の設定

| ワークフロー | トリガー | 内容 | OS |
|-----------|---------|------|-----|
| **dotnet-test.yml** ⭐ | push/PR | テスト実行（29テスト） | Windows |
| **coverage.yml** | push/PR | カバレッジ分析 | Windows |
| **build.yml** | push/PR | テストプロジェクトビルド検証 | Windows |

**推奨：** dotnet-test.yml がメインのワークフローです。build.yml と coverage.yml は補助的に使用してください。
