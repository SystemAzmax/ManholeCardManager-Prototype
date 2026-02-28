#!/usr/bin/env pwsh

<#
.SYNOPSIS
    ローカルテスト実行スクリプト

.DESCRIPTION
    開発環境でテストを実行するための便利なスクリプト

.EXAMPLE
    .\run-tests.ps1
    .\run-tests.ps1 -Configuration Release
    .\run-tests.ps1 -Watch
#>

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [switch]$Watch,

    [switch]$Coverage,

    [switch]$Verbose
)

$ErrorActionPreference = 'Stop'

Write-Host "🧪 ManholeCardManager テスト実行" -ForegroundColor Cyan
Write-Host ""

# テストプロジェクトパス
$TestProject = "ManholeCardManager/ManholeCardManager.Tests/ManholeCardManager.Tests.csproj"

# ビルド前に復元
Write-Host "📦 依存関係を復元中..." -ForegroundColor Yellow
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ 復元に失敗しました" -ForegroundColor Red
    exit 1
}

# ビルド
Write-Host ""
Write-Host "🔨 ビルド中 ($Configuration)..." -ForegroundColor Yellow
dotnet build $TestProject --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ ビルドに失敗しました" -ForegroundColor Red
    exit 1
}

# テスト実行
Write-Host ""
Write-Host "🧪 テスト実行中..." -ForegroundColor Yellow

$testArgs = @(
    'test',
    $TestProject,
    '--configuration', $Configuration,
    '--no-build',
    '--verbosity', ($Verbose ? 'detailed' : 'normal'),
    '--logger', 'console;verbosity=normal'
)

# カバレッジ有効
if ($Coverage) {
    Write-Host "📊 コードカバレッジを有効化" -ForegroundColor Yellow
    $testArgs += @(
        '/p:CollectCoverage=true',
        '/p:CoverletOutputFormat=opencover',
        '/p:CoverletOutput=./coverage/'
    )
}

# ウォッチモード
if ($Watch) {
    Write-Host "👁️  ウォッチモード有効（ファイル変更を監視）" -ForegroundColor Yellow
    Write-Host ""
    
    while ($true) {
        Write-Host "$(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') テスト実行中..." -ForegroundColor Cyan
        dotnet @testArgs
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ テスト成功" -ForegroundColor Green
        } else {
            Write-Host "❌ テスト失敗" -ForegroundColor Red
        }
        
        Write-Host ""
        Write-Host "ファイルの変更を監視中... (Ctrl+C で終了)" -ForegroundColor Gray
        Read-Host
    }
}
else {
    # 単発実行
    dotnet @testArgs
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "❌ テスト実行に失敗しました" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    Write-Host "✅ すべてのテストが成功しました！" -ForegroundColor Green
    
    if ($Coverage) {
        Write-Host ""
        Write-Host "📊 カバレッジレポート:" -ForegroundColor Cyan
        Write-Host "   ./coverage/opencover.xml"
    }
}
