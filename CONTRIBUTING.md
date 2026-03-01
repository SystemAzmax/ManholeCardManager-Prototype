# Contributing to Manhole Card Manager

[English](#english) | [日本語](#日本語)

---

## English

First off, thank you for considering contributing to Manhole Card Manager! 🎉

### How Can I Contribute?

#### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When you create a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples** to demonstrate the steps
- **Describe the behavior you observed** and what you expected to see
- **Include screenshots** if relevant
- **Include your environment details**: OS version, .NET version, etc.

#### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description** of the suggested enhancement
- **Explain why this enhancement would be useful**
- **Provide examples** of how the feature would be used

#### Pull Requests

1. Fork the repository
2. Create a new branch from `master`:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. Make your changes:
   - Follow the existing code style
   - Add XML documentation comments for public APIs
   - Write or update tests as needed
   - Ensure all tests pass
4. Commit your changes:
   ```bash
   git commit -m "Add some feature"
   ```
5. Push to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```
6. Open a Pull Request

### Development Guidelines

#### Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Keep methods focused and concise
- Add comments for complex logic

#### Commit Messages

- Use the present tense ("Add feature" not "Added feature")
- Use the imperative mood ("Move cursor to..." not "Moves cursor to...")
- Limit the first line to 72 characters or less
- Reference issues and pull requests after the first line

#### Testing

- Write unit tests for new features
- Ensure existing tests pass before submitting PR
- Run tests locally:
  ```bash
  dotnet test
  ```

#### Building

```bash
dotnet build
```

### Project Structure

- `ManholeCardManager/` - Main WinUI 3 application
- `ManholeCardDataScraper/` - Data scraper tool
- `ManholeCardManager.Tests/` - Unit tests

### Questions?

Feel free to open an issue with the `question` label!

---

## 日本語

まず、マンホールカードマネージャーへの貢献を検討していただきありがとうございます！🎉

### どのように貢献できますか？

#### バグ報告

バグ報告を作成する前に、重複を避けるため既存のIssueを確認してください。バグ報告を作成する際は、できるだけ詳細を含めてください：

- **明確で説明的なタイトルを使用**
- **問題を再現するための正確な手順を説明**
- **手順を示す具体的な例を提供**
- **観察した動作と期待した動作を説明**
- **関連するスクリーンショットを含める**
- **環境の詳細を含める**: OSバージョン、.NETバージョンなど

#### 機能提案

機能改善の提案はGitHub Issueとして追跡されます。機能提案を作成する際は：

- **明確で説明的なタイトルを使用**
- **提案する機能の詳細な説明を提供**
- **なぜこの機能が有用なのか説明**
- **機能の使用例を提供**

#### プルリクエスト

1. リポジトリをフォーク
2. `master`から新しいブランチを作成：
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. 変更を加える：
   - 既存のコードスタイルに従う
   - パブリックAPIにXMLドキュメントコメントを追加
   - 必要に応じてテストを書く・更新する
   - すべてのテストが通ることを確認
4. 変更をコミット：
   ```bash
   git commit -m "Add some feature"
   ```
5. フォークにプッシュ：
   ```bash
   git push origin feature/your-feature-name
   ```
6. プルリクエストを開く

### 開発ガイドライン

#### コードスタイル

- C#のコーディング規約に従う
- 意味のある変数名・メソッド名を使用
- メソッドは簡潔に保つ
- 複雑なロジックにはコメントを追加

#### コミットメッセージ

- 現在形を使用（"Add feature"、"Added feature"ではない）
- 命令形を使用（"Move cursor to..."、"Moves cursor to..."ではない）
- 最初の行は72文字以内に制限
- 最初の行の後にIssueやPRを参照

#### テスト

- 新機能のユニットテストを書く
- PR提出前に既存テストが通ることを確認
- ローカルでテストを実行：
  ```bash
  dotnet test
  ```

#### ビルド

```bash
dotnet build
```

### プロジェクト構造

- `ManholeCardManager/` - メインのWinUI 3アプリケーション
- `ManholeCardDataScraper/` - データスクレイパーツール
- `ManholeCardManager.Tests/` - ユニットテスト

### 質問がありますか？

`question`ラベルでIssueを開いてください！
