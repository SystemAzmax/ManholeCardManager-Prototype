# GitHub Actions CI/CD セットアップガイド

## ✅ セットアップ完了状況

GitHub Actionsの自動CI/CDパイプラインがセットアップ済みです。

### 📁 作成されたファイル

```
.github/
├── workflows/
│   ├── dotnet-test.yml      # ユニットテスト実行
│   ├── coverage.yml         # コードカバレッジ分析
│   ├── build.yml            # ビルド検証
│   └── README.md            # ワークフロー説明
├── run-tests.ps1            # ローカルテスト実行（PowerShell）
└── run-tests.sh             # ローカルテスト実行（Bash）
```

## 🚀 すぐに使える機能

### 1. **自動テスト実行**
```
Push or Pull Request → テスト自動実行 → PR にコメント
```

### 2. **ビルド検証**
```
Debug/Release × x64 で自動ビルド検証
```

### 3. **コードカバレッジ分析**
```
テスト実行 → Codecov アップロード → レポート生成
```

## 📝 ローカル開発での使用

### PowerShell（Windows）

```powershell
# 基本的なテスト実行
.\run-tests.ps1

# Release構成でテスト
.\run-tests.ps1 -Configuration Release

# コードカバレッジ付き
.\run-tests.ps1 -Coverage

# ファイル変更を監視してテスト実行（開発中）
.\run-tests.ps1 -Watch

# 詳細出力
.\run-tests.ps1 -Verbose
```

### Bash（Linux/macOS）

```bash
# 基本的なテスト実行
./run-tests.sh

# Release構成でテスト
./run-tests.sh Release

# コードカバレッジ付き
./run-tests.sh Debug true
```

## 🔄 ワークフロー実行フロー

### 1. Push時
```
feature/my-feature にコミット
    ↓
GitHub Actions トリガー
    ↓
build.yml → dotnet-test.yml → coverage.yml
    ↓
✅ すべて成功 → マージ可能
❌ いずれか失敗 → 修正必要
```

### 2. Pull Request時
```
PR 作成
    ↓
GitHub Actions 自動実行
    ↓
Checks タブに結果表示
    ↓
"This branch has no conflicts"表示
    ↓
マージ可能
```

## 📊 テスト結果の確認

### GitHub UI

1. **リポジトリ** → **Actions** タブ
2. ワークフロー名をクリック
3. 実行履歴をクリック
4. **Unit Test Results** で詳細確認

### Pull Request

1. PR を開く
2. **Checks** タブ
3. 各ワークフローの結果確認
4. テスト失敗時は詳細を見てデバッグ

## ⚙️ カスタマイズ例

### 例1: スケジュール実行（毎晩テスト）

`.github/workflows/dotnet-test.yml` を編集：

```yaml
on:
  push:
    branches: [ main, develop ]
  schedule:
    - cron: '0 2 * * *'  # UTC 02:00 = JST 11:00
```

### 例2: 複数 .NET バージョンでテスト

```yaml
strategy:
  matrix:
    dotnet-version: ['7.0.x', '8.0.x']
```

### 例3: Slack 通知追加

```yaml
- name: Notify Slack on failure
  if: failure()
  uses: slackapi/slack-github-action@v1
  with:
    webhook-url: ${{ secrets.SLACK_WEBHOOK }}
```

## 🔐 Secrets（オプション）

Codecov や Slack 通知を使う場合：

1. **GitHub リポジトリ設定**
2. **Settings** → **Secrets and variables** → **Actions**
3. **New repository secret** をクリック
4. 必要なシークレットを追加

**Codecov:**
```
CODECOV_TOKEN: <codecov から取得>
```

**Slack:**
```
SLACK_WEBHOOK: <slack から取得>
```

## 📚 次のステップ

### 推奨対応
1. ✅ 最初のコミットを push
2. ✅ Actions タブでワークフロー実行確認
3. ✅ PR で自動テスト実行確認
4. ✅ テスト失敗時は修正してプッシュ

### 将来の拡張
- [ ] Codecov ダッシュボード統合
- [ ] 定期スケジュール実行
- [ ] Slack/Teams 通知
- [ ] 複数 .NET バージョンテスト
- [ ] コードスキャン（CodeQL）

## ❓ よくある質問

### Q1: ワークフローが実行されない
**A:** `.github/workflows/` ファイルが存在し、`on:` セクションが設定されているか確認

### Q2: テストローカルで成功、CI で失敗
**A:** ローカル環境と CI 環境の差を確認
```bash
# CI と同じ環境でテスト
dotnet test --configuration Release
```

### Q3: ワークフローを無効化したい
**A:** ワークフローファイルの `on:` セクションをコメントアウト

### Q4: テスト実行時間が長い
**A:** NuGet キャッシュを追加：
```yaml
- uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/packages.lock.json') }}
```

## 📞 サポート

- GitHub Actions 公式ドキュメント: https://docs.github.com/en/actions
- .NET テスト: https://docs.microsoft.com/en-us/dotnet/core/testing/
- Codecov: https://docs.codecov.io

---

✨ GitHub Actions パイプラインは完全にセットアップ済みです！
次のコミットで自動テスト実行をお試しください。
