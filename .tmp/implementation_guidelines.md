# GatchaSpire 実装ガイドライン

## 実装方針

### 基本原則
1. **段階的実装**: 小さなタスクを確実に完了
2. **テスト駆動**: 各機能の動作確認を必須とする
3. **エラーハンドリング**: 全ての操作に適切なエラー処理
4. **Unity特化**: Unityの特性を活かした設計
5. **保守性重視**: 将来の変更に対応しやすい構造

### 実装の流れ
```
タスク選択 → 実装 → 単体テスト → 統合テスト → チェック完了
```

---

## コーディング規約

### Unity特化パターン

#### MonoBehaviour 継承クラス
```csharp
public class ExampleManager : MonoBehaviour, IUnityGameSystem
{
    [Header("Settings")]
    [SerializeField] private bool enableDebugLogs = true;
    
    [Header("Unity Integration")]
    [SerializeField] private bool persistAcrossScenes = true;
    
    private static ExampleManager instance;
    public static ExampleManager Instance => instance;
    
    // IUnityGameSystem implementation required
    public bool RequiresUpdate => false;
    public int ExecutionOrder => 0;
    public bool PersistAcrossScenes => persistAcrossScenes;
    public bool RequiresMainThread => true;
    
    public void OnAwake() { /* Unity Awake処理 */ }
    public void OnStart() { /* Unity Start処理 */ }
    public void OnDestroy() { /* Unity OnDestroy処理 */ }
    
    // IGameSystem implementation required
    public void Initialize() { /* システム初期化 */ }
    public void Reset() { /* システムリセット */ }
    public void Update() { /* システム更新 */ }
    public void Shutdown() { /* システム終了 */ }
    
    public string GetSystemName() => "ExampleManager";
    public bool IsInitialized() => /* 初期化状態 */;
    public SystemPriority GetInitializationPriority() => SystemPriority.Medium;
    public List<string> GetDependencies() => new List<string>();
}
```

#### ScriptableObject データクラス
```csharp
[CreateAssetMenu(fileName = "ExampleData", menuName = "GatchaSpire/Example Data")]
public class ExampleData : ScriptableObject, IValidatable, IScriptableObjectData
{
    [Header("Basic Settings")]
    public string dataName;
    public string description;
    
    public ValidationResult Validate()
    {
        var result = new ValidationResult();
        
        if (string.IsNullOrEmpty(dataName))
            result.AddError("Data name is required");
            
        return result;
    }
    
    public void OnValidate()
    {
        var validation = Validate();
        if (!validation.IsValid)
        {
            validation.LogToConsole($"ExampleData ({name})");
        }
    }
    
    // IScriptableObjectData implementation
    public string GetDataName() => dataName;
    public string GetDataDescription() => description;
    public bool IsValid() => Validate().IsValid;
}
```

### エラーハンドリングパターン

#### 基本的なエラー報告
```csharp
public bool DoSomething(int value)
{
    try
    {
        if (value <= 0)
        {
            // GameSystemBaseを継承していること
            ReportWarning("ExampleManager", $"Invalid value: {value}");
            return false;
        }
        
        // 処理実行
        return true;
    }
    catch (Exception e)
    {
        ReportError("ExampleManager", "Failed to do something", e);
        return false;
    }
}
```

#### 開発設定の活用
```csharp
private void ApplyDevelopmentSettings()
{
    var devSettings = DevelopmentSettings.Instance;
    if (devSettings == null) return;
    
    enableDebugLogs = devSettings.enableAllDebugLogs;
    
    if (devSettings.infiniteGold)
    {
        // チート機能の適用
    }
}
```

---

## テスト戦略

### 単体テストパターン

#### 基本機能テスト
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
public void TestBasicFunctionality()
{
    Debug.Log("=== Testing Basic Functionality ===");
    
    // 正常系テスト
    bool result1 = DoSomething(10);
    Debug.Assert(result1, "Normal case should succeed");
    
    // 異常系テスト
    bool result2 = DoSomething(-5);
    Debug.Assert(!result2, "Invalid input should fail");
    
    Debug.Log("Basic functionality tests passed");
}
```

#### パフォーマンステスト
```csharp
[System.Diagnostics.Conditional("UNITY_EDITOR")]
public void PerformanceTest()
{
    var stopwatch = System.Diagnostics.Stopwatch.StartNew();
    
    for (int i = 0; i < 1000; i++)
    {
        DoSomething(i);
    }
    
    stopwatch.Stop();
    float averageTime = stopwatch.ElapsedMilliseconds / 1000.0f;
    
    Debug.Log($"Performance: {averageTime:F3}ms per operation");
    
    if (averageTime > 1.0f) // 1ms以上は警告
    {
        errorReporter?.ReportPerformanceError("ExampleManager", 
            "Slow operation detected", averageTime / 1000.0f);
    }
}
```

### 統合テストパターン

#### システム間連携テスト
```csharp
public void TestSystemIntegration()
{
    // 前提条件確認
    Debug.Assert(SystemA.Instance.IsInitialized(), "SystemA must be initialized");
    Debug.Assert(SystemB.Instance.IsInitialized(), "SystemB must be initialized");
    
    // 連携テスト実行
    SystemA.Instance.DoSomethingThatAffectsB();
    
    // 結果確認
    bool expected = SystemB.Instance.GetExpectedState();
    Debug.Assert(expected, "System integration failed");
    
    Debug.Log("System integration test passed");
}
```

---

## デバッグ支援

### ログ出力パターン
```csharp
private void LogDebugInfo(string message)
{
    if (!enableDebugLogs) return;
    
    var devSettings = DevelopmentSettings.Instance;
    if (devSettings != null && devSettings.enableAllDebugLogs)
    {
        Debug.Log($"[{GetSystemName()}] {message}");
    }
}
```

### エディタ専用機能
```csharp
#if UNITY_EDITOR
[System.Diagnostics.Conditional("UNITY_EDITOR")]
public void EditorOnlyFunction()
{
    // エディタでのみ実行される処理
}

[UnityEditor.MenuItem("GatchaSpire/Debug/Run Tests")]
private static void RunDebugTests()
{
    var manager = FindObjectOfType<ExampleManager>();
    manager?.TestBasicFunctionality();
}
#endif
```

---

## ファイル構成ルール

### スクリプトファイル配置
```
Assets/Scripts/
├── Core/                     # コアシステム
│   ├── Interfaces/           # 共通インターフェース
│   ├── Systems/              # システム管理
│   ├── Error/                # エラーハンドリング
│   └── Data/                 # 基本データクラス
├── Gameplay/                 # ゲームロジック
│   ├── Gold/                 # ゴールドシステム
│   ├── Characters/           # キャラクターシステム
│   ├── Gacha/                # ガチャシステム
│   └── Battle/               # 戦闘システム
├── UI/                       # UIシステム
├── Tests/                    # テストスクリプト
└── Editor/                   # エディタ拡張
```

### 命名規約
- **クラス名**: PascalCase (例: `GoldManager`)
- **メソッド名**: PascalCase (例: `GetCurrentGold()`)
- **フィールド名**: camelCase (例: `currentGold`)
- **定数名**: UPPER_SNAKE_CASE (例: `MAX_GOLD`)
- **プライベートフィールド**: camelCase (例: `isInitialized`)

---

## パフォーマンスガイドライン

### メモリ管理
```csharp
// ✅ Good: オブジェクトプールの活用
private Queue<GameObject> objectPool = new Queue<GameObject>();

// ❌ Bad: 毎回新しいオブジェクト生成
GameObject obj = Instantiate(prefab);
```

### ガベージコレクション対策
```csharp
// ✅ Good: 文字列連結の最適化
private StringBuilder stringBuilder = new StringBuilder();

// ❌ Bad: 文字列の頻繁な連結
string result = str1 + str2 + str3;
```

### Update メソッドの最適化
```csharp
// ✅ Good: 必要な時のみ更新
public bool RequiresUpdate => hasActiveOperations;

// ❌ Bad: 常に空のUpdate実行
public bool RequiresUpdate => true;
```

---

## 品質保証チェックリスト

### コード品質
- [ ] 全public メソッドにXMLドキュメント
- [ ] 適切な例外処理の実装
- [ ] Magic Numberの排除
- [ ] Null参照の安全な処理
- [ ] リソースの適切な解放

### Unity特有
- [ ] SerializeFieldの適切な使用
- [ ] Coroutineの適切な停止処理
- [ ] OnDestroyでのイベント購読解除
- [ ] DontDestroyOnLoadの適切な使用
- [ ] メモリリークの確認

### テスト
- [ ] 単体テストの実装
- [ ] 異常系テストの実装
- [ ] パフォーマンステストの実行
- [ ] 統合テストの確認
- [ ] エディタでの動作確認

### ドキュメント
- [ ] 実装内容の記録
- [ ] 既知の問題の記載
- [ ] 今後の改善点の整理
- [ ] 使用方法の説明

---

## トラブルシューティング

### よくある問題と解決方法

#### システム初期化エラー
**症状**: GameSystemCoordinatorでNullReferenceException
**原因**: Awakeの実行順序問題
**解決**: Script Execution Orderの設定

#### メモリリーク
**症状**: プレイ時間とともにメモリ使用量が増加
**原因**: イベントハンドラーの購読解除忘れ
**解決**: OnDestroyでの適切な購読解除

#### パフォーマンス問題
**症状**: フレームレートの低下
**原因**: Update メソッドでの重い処理
**解決**: 処理の分散化、コルーチンの活用

この ガイドラインに従うことで、一貫性のある高品質な実装が可能になります。