#!/bin/bash

# ManholeCardManager テスト実行スクリプト (Bash/Linux/macOS)

set -e

CONFIGURATION=${1:-Debug}
WATCH=${2:-false}
COVERAGE=${3:-false}

echo "🧪 ManholeCardManager テスト実行"
echo ""

TEST_PROJECT="ManholeCardManager/ManholeCardManager.Tests/ManholeCardManager.Tests.csproj"

# 依存関係を復元
echo "📦 依存関係を復元中..."
dotnet restore

# ビルド
echo ""
echo "🔨 ビルド中 ($CONFIGURATION)..."
dotnet build "$TEST_PROJECT" --configuration "$CONFIGURATION" --no-restore

# テスト実行
echo ""
echo "🧪 テスト実行中..."

test_args=(
    "test"
    "$TEST_PROJECT"
    "--configuration" "$CONFIGURATION"
    "--no-build"
    "--verbosity" "normal"
)

if [ "$COVERAGE" = "true" ]; then
    echo "📊 コードカバレッジを有効化"
    test_args+=(
        "/p:CollectCoverage=true"
        "/p:CoverletOutputFormat=opencover"
        "/p:CoverletOutput=./coverage/"
    )
fi

dotnet "${test_args[@]}"

if [ $? -eq 0 ]; then
    echo ""
    echo "✅ すべてのテストが成功しました！"
    if [ "$COVERAGE" = "true" ]; then
        echo ""
        echo "📊 カバレッジレポート:"
        echo "   ./coverage/opencover.xml"
    fi
else
    echo ""
    echo "❌ テスト実行に失敗しました"
    exit 1
fi
