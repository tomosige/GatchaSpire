# GatchaSpire Unity プロジェクト コーディング規約

このドキュメントは、GatchaSpire Unityプロジェクトにおけるコーディング規約を定義します。既存のコードベースとの整合性を保つため、実際の実装を分析して策定されました。

## 1. 命名規約

### 1.1 クラス、インターフェース、構造体
- **PascalCase**を使用
- 例: `Character`, `GoldManager`, `UnityGameSystemCoordinator`
- インターフェースには`I`を接頭辞として使用
- 例: `IGameSystem`, `IUnityGameSystem`, `IErrorHandler`

### 1.2 メソッド
- **PascalCase**を使用
- 動詞で開始し、目的を明確に表現
- 例: `AddExperience`, `InitializeFromData`, `RegisterUnitySystem`

### 1.3 プロパティ
- **PascalCase**を使用
- 読み取り専用プロパティを優先
- 例: `CurrentLevel`, `InstanceId`, `SystemName`

### 1.4 フィールド
- **camelCase**を使用
- 例: `currentLevel`, `currentGold`, `isInitialized`

### 1.5 定数
- **UPPER_SNAKE_CASE**を使用
- 例: `GOLD_SAVE_KEY`, `MAX_LEVEL`

### 1.6 名前空間
- **PascalCase**を使用
- プロジェクト名から開始
- 例: `GatchaSpire.Core.Character`, `GatchaSpire.Core.Gold`

## 2. コメント規約

### 2.1 XMLドキュメントコメント
- **日本語で記述**
- 全てのpublicメンバーに必須
- `<summary>`, `<param>`, `<returns>`等を適切に使用

```csharp
/// <summary>
/// 経験値を追加してレベルアップを処理
/// </summary>
/// <param name="exp">追加経験値</param>
/// <returns>レベルアップした回数</returns>
public int AddExperience(int exp)
```

### 2.2 実装コメント
- **日本語で記述**
- 実装の背景や理由を説明
- 複雑なロジックには必須

```csharp
// レベルアップした場合はステータスを再計算
if (levelUps > 0)
{
    ApplyLevelGrowth();
}
```

### 2.3 ヘッダーコメント
- Unity Inspector表示用のヘッダーは日本語で記述
- システム設定、基本情報等、意味のあるグループ化

```csharp
[Header("基本情報")]
[SerializeField] private string instanceId;
```

## 3. Unity固有の規約

### 3.1 SerializeField属性
- privateフィールドをInspectorに表示する場合に使用
- publicフィールドは避ける

```csharp
[SerializeField] private int initialGold = 100;
```

### 3.2 Header属性
- Inspector表示のグループ化に使用
- 日本語で記述

```csharp
[Header("ゴールドシステム設定")]
[SerializeField] private int initialGold = 100;
```

### 3.3 DefaultExecutionOrder属性
- 実行順序が重要なシステムに使用
- 数値が小さいほど早く実行

```csharp
[DefaultExecutionOrder(-100)]
public class UnityGameSystemCoordinator : MonoBehaviour
```

## 4. ログ出力規約

### 4.1 ログメッセージ
- **日本語で記述**
- システム名をブラケットで囲んで表示
- 例: `[GoldManager]`, `[Character]`

```csharp
Debug.Log($"[{SystemName}] システムを初期化しました");
```

### 4.2 ログレベル
- 情報: `Debug.Log`
- 警告: `Debug.LogWarning`
- エラー: `Debug.LogError`
- 独自のエラーハンドリングシステムを併用

```csharp
ReportInfo("ゴールドシステムが初期化されました");
ReportWarning("DevelopmentSettingsが見つかりません");
ReportError("システムの初期化に失敗しました", exception);
```

## 5. クラス構成規約

### 5.1 フィールド配置順序
1. 定数
2. 静的フィールド
3. インスタンスフィールド（SerializeField）
4. プロパティ
5. コンストラクタ
6. Unity生命周期メソッド
7. publicメソッド
8. protectedメソッド
9. privateメソッド

### 5.2 アクセス修飾子
- 最小限のアクセス権限を付与
- publicプロパティは読み取り専用を優先
- internal は同一アセンブリ内での連携に使用

## 6. エラーハンドリング規約

### 6.1 例外処理
- 必要に応じてtry-catch文を使用
- 独自のエラーハンドリングシステムを活用

```csharp
try
{
    OnSystemInitialize();
    isInitialized = true;
}
catch (System.Exception e)
{
    errorHandler?.ReportCritical(SystemName, "システムの初期化に失敗しました", e);
    throw;
}
```

### 6.2 null チェック
- 重要なオブジェクトには必須
- 適切なエラーメッセージを出力

```csharp
if (data == null)
{
    Debug.LogError("[Character] CharacterDataがnullです");
    return;
}
```

## 7. 初期化規約

### 7.1 コレクションの初期化
- 宣言時に初期化してNullReferenceExceptionを防ぐ
- 単純なコレクション型に適用

```csharp
private Dictionary<StatType, int> permanentBoosts = new Dictionary<StatType, int>();
private List<string> unlockedAbilities = new List<string>();
```

### 7.2 複雑なオブジェクト初期化
- メソッド内で制御された初期化
- Unity固有オブジェクトや外部依存のあるオブジェクト

```csharp
protected override void OnSystemInitialize()
{
    _transactionHistory = new GoldTransactionHistory();
    _calculator = new GoldCalculator();
}
```

## 8. インターフェース設計規約

### 8.1 インターフェースの粒度
- 単一責任の原則を適用
- 適切な抽象度を保つ

### 8.2 システムインターフェース
- `IGameSystem`: 基本的なゲームシステム
- `IUnityGameSystem`: Unity特化システム
- `IPersistentSystem`: 永続化システム
- `IResettableSystem`: リセット可能システム

## 9. パフォーマンス考慮事項

### 9.1 Update処理
- 重い処理は避ける
- 必要に応じてコルーチンやイベント駆動に変更

### 9.2 メモリ管理
- 適切なオブジェクトの生成と破棄
- 不要なアセットのUnload

### 9.3 文字列操作
- 頻繁な文字列結合は避ける
- StringBuilderやstring interpolationを活用

## 10. 設定と構成

### 10.1 設定ファイル
- ScriptableObjectを使用
- Resources フォルダで管理
- 例: `DevelopmentSettings.asset`

### 10.2 依存関係
- 依存関係の明示的な管理
- `DependencyResolver`を活用

## 11. テストとデバッグ

### 11.1 デバッグ機能
- 開発設定による制御
- デバッグ専用メソッドの提供

```csharp
public void SetGold(int amount)
{
    if (_developmentSettings == null || !_developmentSettings.EnableDebugCommands)
    {
        ReportWarning("デバッグコマンドが無効です");
        return;
    }
    // デバッグ処理
}
```

### 11.2 検証機能
- 入力値の検証
- 状態の整合性チェック

## 12. 特記事項

### 12.1 絵文字の使用
- コードやコメントでは絵文字を使用しない
- ドキュメンテーションでも基本的に避ける

### 12.2 日本語記述
- 不要なスペースを含めない
- 例: ◯「Unity開発ガイド」× "Unity 開発 ガイド"

### 12.3 シングルトンパターン
- 必要最小限の使用
- スレッドセーフではない実装

```csharp
private static GoldManager _instance;
public static GoldManager Instance => _instance;
```

---

このコーディング規約は、既存のコードベースとの整合性を保つために策定されています。新しい機能の追加や既存コードの修正時は、これらの規約に従って実装してください。