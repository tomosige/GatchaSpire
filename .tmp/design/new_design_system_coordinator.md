# 新設計: システム統合管理

## 概要
リファクタリング提案で新たに必要とされた全体調整クラスと共通インターフェースの設計。

## 統合管理クラス設計

### GameSystemCoordinator
全システムの統合調整を担当するクラス。

**publicメソッド:**

**システム初期化:**
- `void InitializeAllSystems()` - 全システム初期化
- `void InitializeSystem(IGameSystem system)` - 個別システム初期化
- `bool AreAllSystemsInitialized()` - 全システム初期化完了判定
- `List<string> GetInitializationErrors()` - 初期化エラー取得

**システムリセット:**
- `void ResetAllSystems()` - 全システムリセット
- `void ResetSystem(IGameSystem system)` - 個別システムリセット
- `void ResetSystemsExcept(List<IGameSystem> exceptions)` - 例外指定リセット

**依存関係管理:**
- `void RegisterSystemDependency(IGameSystem system, IGameSystem dependency)` - システム依存関係登録
- `void ResolveDependencies()` - 依存関係解決
- `List<IGameSystem> GetDependencyOrder()` - 依存順序取得
- `bool ValidateDependencies()` - 依存関係検証

**システム監視:**
- `void ValidateSystemIntegrity()` - システム整合性検証
- `SystemHealth GetSystemHealth()` - システム健康状態取得
- `void MonitorSystemPerformance()` - システムパフォーマンス監視
- `SystemReport GenerateSystemReport()` - システムレポート生成

**ライフサイクル管理:**
- `void UpdateAllSystems()` - 全システム更新
- `void ShutdownAllSystems()` - 全システムシャットダウン
- `void RegisterSystem(IGameSystem system)` - システム登録
- `void UnregisterSystem(IGameSystem system)` - システム登録解除

**詳細説明:**
- 各システムの初期化順序制御
- システム間の依存関係管理
- 統合的なエラーハンドリング
- システム全体の状態監視

## 共通インターフェース設計

### IGameSystem
全ゲームシステムが実装すべき基本インターフェース。

**publicメソッド:**
- `void Initialize()` - システム初期化
- `void Reset()` - システムリセット
- `void Update()` - システム更新
- `void Shutdown()` - システムシャットダウン
- `string GetSystemName()` - システム名取得
- `bool IsInitialized()` - 初期化状態取得
- `SystemPriority GetInitializationPriority()` - 初期化優先度取得
- `List<string> GetDependencies()` - 依存システム名取得

**詳細説明:**
- 全システム共通の操作を定義
- 統一的なライフサイクル管理
- 依存関係の明示化

### IResettableSystem
リセット可能なシステム用の拡張インターフェース。

**publicメソッド:**
- `void PrepareForReset()` - リセット準備
- `void PostResetCleanup()` - リセット後クリーンアップ
- `bool CanReset()` - リセット可能判定
- `ResetScope GetResetScope()` - リセット範囲取得

### IPersistentSystem
永続化が必要なシステム用のインターフェース。

**publicメソッド:**
- `void SavePersistentData()` - 永続データ保存
- `void LoadPersistentData()` - 永続データ読み込み
- `bool HasPersistentData()` - 永続データ存在判定
- `void ClearPersistentData()` - 永続データクリア

## サポートクラス設計

### SystemHealth
システム健康状態を表すクラス。

**publicメソッド:**
- `SystemHealthStatus GetOverallStatus()` - 全体ステータス取得
- `Dictionary<string, SystemHealthStatus> GetSystemStatuses()` - システム別ステータス取得
- `List<string> GetHealthIssues()` - 健康問題取得
- `int GetHealthScore()` - 健康スコア取得
- `string GetHealthSummary()` - 健康サマリー取得

### SystemReport
システムレポートを管理するクラス。

**publicメソッド:**
- `void AddSystemInfo(string systemName, SystemInfo info)` - システム情報追加
- `string GenerateTextReport()` - テキストレポート生成
- `void SaveReportToFile(string filePath)` - レポートファイル保存
- `Dictionary<string, object> GetReportData()` - レポートデータ取得

### SystemPriority (enum)
システム初期化優先度を表す列挙型。

**値:**
- `Critical` - 最高優先度（基盤システム）
- `High` - 高優先度（コアシステム）
- `Medium` - 中優先度（ゲームロジック）
- `Low` - 低優先度（UI・演出）
- `Lowest` - 最低優先度（デバッグ・統計）

### ResetScope (enum)
リセット範囲を表す列挙型。

**値:**
- `Complete` - 完全リセット
- `RunData` - ランデータのみ
- `UserPreferences` - ユーザー設定のみ
- `Cache` - キャッシュデータのみ

## 実装システムでの適用例

### GachaSystemManager (IGameSystem実装例)
```csharp
public class GachaSystemManager : IGameSystem, IResettableSystem
{
    public void Initialize()
    {
        // ガチャデータ読み込み
        // 初期化処理
    }
    
    public void Reset()
    {
        // ガチャレベルリセット
        // 進行状況リセット
    }
    
    public string GetSystemName() => "GachaSystem";
    public SystemPriority GetInitializationPriority() => SystemPriority.High;
    public List<string> GetDependencies() => new List<string> { "GoldSystem" };
}
```

### CharacterInventoryManager (IGameSystem実装例)
```csharp
public class CharacterInventoryManager : IGameSystem, IResettableSystem
{
    public void Initialize()
    {
        // キャラクターデータベース初期化
    }
    
    public void Reset()
    {
        // 所持キャラクターリセット
    }
    
    public string GetSystemName() => "CharacterInventory";
    public SystemPriority GetInitializationPriority() => SystemPriority.Medium;
    public List<string> GetDependencies() => new List<string> { "CharacterDatabase", "GoldSystem" };
}
```

## 統合管理による利点

### 1. 初期化順序の保証
- 依存関係に基づく適切な初期化順序
- 初期化失敗時の適切なエラーハンドリング
- システム間の競合状態の回避

### 2. 統一的なライフサイクル管理
- 全システムの一貫した操作
- リセット処理の確実な実行
- メモリリークの防止

### 3. デバッグとメンテナンスの向上
- システム状態の一元監視
- 問題の早期発見
- 統合的なレポート機能

### 4. 拡張性の確保
- 新システム追加時の統一的な管理
- 依存関係の自動解決
- システム間の疎結合の維持

## 使用方法

### 初期化フロー
```csharp
void Start()
{
    var coordinator = GameSystemCoordinator.Instance;
    
    // システム登録
    coordinator.RegisterSystem(new GachaSystemManager());
    coordinator.RegisterSystem(new CharacterInventoryManager());
    coordinator.RegisterSystem(new BattleManager());
    
    // 依存関係解決と初期化
    coordinator.ResolveDependencies();
    coordinator.InitializeAllSystems();
}
```

### ランリセット
```csharp
void StartNewRun()
{
    var coordinator = GameSystemCoordinator.Instance;
    coordinator.ResetAllSystems();
}
```