# クラス設計改善提案

## 統合すべきクラス群

### 1. ガチャ関連クラスの統合

**統合対象:**
- `GachaManager`（機能04）
- `GachaUpgradeManager`（機能05）
- `GachaPool`（機能04）
- `GachaRateCalculator`（機能04）

**統合理由:**
- 全てガチャシステムの中核機能
- 相互に密接に連携する
- 単一責任の原則に反しない範囲で統合可能

**統合後の提案:**
```csharp
public class GachaSystemManager
{
    // 基本ガチャ機能
    public List<Character> PullGacha(int pullCount = 1)
    public bool CanPullGacha()
    public int GetGachaCost()
    
    // アップグレード機能
    public bool UpgradeGacha()
    public bool CanUpgradeGacha()
    public int GetUpgradeCost()
    
    // 内部計算（privateメソッド化）
    private float CalculateRarityRate(CharacterRarity rarity, int gachaLevel)
    private Character GetRandomCharacter()
}
```

### 2. キャラクター管理クラスの統合

**統合対象:**
- `CharacterSellManager`（機能08）
- `CharacterExpManager`（機能09）
- `CharacterFusionManager`（機能07）

**統合理由:**
- 全てキャラクターの操作に関する機能
- 同一のキャラクターデータに対する異なる操作
- インベントリ管理の観点から統合が自然

**統合後の提案:**
```csharp
public class CharacterInventoryManager
{
    // 売却機能
    public bool SellCharacter(Character character)
    public int GetSellPrice(Character character)
    
    // 経験値化機能
    public bool FeedCharacterForExp(Character target, Character fodder)
    public int GetExpValue(Character character)
    
    // 合成機能
    public Character FuseCharacters(List<Character> characters)
    public bool CanFuseCharacters(List<Character> characters)
    
    // 共通機能
    public List<Character> GetAllCharacters()
    public void ValidateCharacterOperation(Character character)
}
```

### 3. バトル関連クラスの統合

**統合対象:**
- `AutoBattleManager`（機能11）
- `BattleSimulator`（機能11）
- `BattleState`（機能11）

**統合理由:**
- バトルの実行と状態管理は密接に関連
- 状態管理とシミュレーションは分離する必要性が低い
- 単一のバトルシステムとしての統合が適切

**統合後の提案:**
```csharp
public class BattleManager
{
    // 公開インターフェース
    public void StartBattle(List<Character> playerTeam, List<Character> enemyTeam)
    public BattleResult GetBattleResult()
    public void PauseBattle()
    
    // 内部処理（privateメソッド化）
    private BattleResult SimulateBattle(BattleSetup setup)
    private void ProcessTurn()
    private void UpdateBattleState()
}
```

## 分割すべきクラス群

### 1. BoardManagerの分割

**現在の問題:**
- `BoardManager`が配置、移動、検証を全て担当
- 責任が多すぎて単一責任の原則に違反

**分割提案:**
```csharp
// 配置状態の管理のみ
public class BoardStateManager
{
    public bool PlaceCharacter(Character character, Vector2Int position)
    public Character GetCharacterAt(Vector2Int position)
    public List<Character> GetAllPlacedCharacters()
}

// 配置の検証のみ
public class PlacementValidator
{
    public bool ValidatePlacement(Character character, Vector2Int position)
    public List<Vector2Int> GetValidPositions(Character character)
    public string GetValidationError(Character character, Vector2Int position)
}

// 配置の最適化のみ
public class PlacementOptimizer
{
    public PlacementSuggestion GetOptimalPlacement(List<Character> party)
    public float CalculatePositionScore(Character character, Vector2Int position)
}
```

### 2. SynergyManagerの分割

**現在の問題:**
- シナジー計算、効果適用、可視化が混在
- 計算ロジックと表示ロジックが分離されていない

**分割提案:**
```csharp
// シナジー計算のみ
public class SynergyCalculator
{
    public List<SynergyEffect> CalculateAllSynergies(List<Character> party)
    public int GetSynergyLevel(SynergyType type, int unitCount)
}

// シナジー効果の適用のみ
public class SynergyEffectApplier
{
    public void ApplySynergyEffects(List<Character> party, List<SynergyEffect> effects)
    public void RemoveSynergyEffects(List<Character> party)
}

// シナジー表示のみ
public class SynergyVisualizer
{
    public void UpdateSynergyDisplay(List<SynergyEffect> effects)
    public void HighlightSynergyCharacters(SynergyType type)
}
```

## 新たに必要なクラス

### 1. 全体調整クラス

**GameSystemCoordinator**
```csharp
public class GameSystemCoordinator
{
    // 各システム間の調整
    public void InitializeAllSystems()
    public void ResetAllSystems()
    public void ValidateSystemIntegrity()
    
    // システム間の依存関係管理
    public void RegisterSystemDependency(IGameSystem system, IGameSystem dependency)
    public void ResolveDependencies()
}
```

**理由:** 現在の設計では各システムが独立しすぎており、システム間の初期化順序や依存関係の管理が困難

### 2. 共通インターフェース

**IGameSystem**
```csharp
public interface IGameSystem
{
    void Initialize();
    void Reset();
    void Update();
    string GetSystemName();
    bool IsInitialized();
}
```

**理由:** 各システムの共通操作を統一し、システム管理を容易にする

## 統合・分割の指針

### 統合すべき場合
1. **機能的結合が強い** - 同一データに対する異なる操作
2. **実装の重複が多い** - 同じような処理を複数クラスで実装
3. **外部からの呼び出し頻度が高い** - 密接に連携する機能

### 分割すべき場合
1. **単一責任の原則に違反** - 複数の責任を持つクラス
2. **テスタビリティの問題** - 一部機能のみのテストが困難
3. **拡張性の問題** - 新機能追加時にクラスが肥大化

## 実装優先度

### 高優先度（すぐに統合すべき）
1. ガチャ関連クラス → `GachaSystemManager`
2. キャラクター管理クラス → `CharacterInventoryManager`

### 中優先度（リファクタリング時に検討）
1. バトル関連クラス → `BattleManager`
2. BoardManager → 3つのクラスに分割

### 低優先度（将来の拡張時に検討）
1. SynergyManager → 3つのクラスに分割
2. 共通インターフェースの導入