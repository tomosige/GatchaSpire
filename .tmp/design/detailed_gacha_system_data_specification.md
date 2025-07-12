# GatchaSpire GachaSystemData詳細仕様書

## 概要
GachaSystemDataは、GatchaSpireにおけるガチャシステムの全設定を管理するScriptableObjectです。基本ガチャ機能、アップグレードシステム、天井システム、ピックアップシステムを統合管理します。

## 基本設計方針
- **統合管理**: 旧GachaDataとGachaUpgradeDataの機能統合
- **段階的成長**: レベル別のガチャ性能向上システム
- **確率制御**: 詳細な排出率制御とバランス調整
- **プレイヤー保護**: 天井システムによる最悪ケース保護

---

## 1. 基本設定

### 1.1 システム識別
```csharp
[Header("基本設定")]
[SerializeField] private string gachaSystemId = "basic_gacha";  // ガチャID
[SerializeField] private string displayName = "基本ガチャ";       // 表示名
[SerializeField] private int baseCost = 100;                   // 基本コスト (ゴールド)
[SerializeField] private bool isActive = true;                 // 有効フラグ
```

**設計指針**：
- `gachaSystemId`は将来的な複数ガチャ対応を考慮
- `displayName`は多言語対応可能な設計
- `baseCost`はアップグレードにより変動

### 1.2 排出確率設定
```csharp
[Header("排出確率設定")]
[SerializeField] private GachaDropRate[] dropRates;            // レアリティ別排出率
[SerializeField] private float guaranteedRareRate = 5.0f;      // 保証レア以上確率
[SerializeField] private int guaranteedRareCount = 20;         // 保証までの回数
```

**GachaDropRate構造**：
```csharp
[System.Serializable]
public struct GachaDropRate
{
    public CharacterRarity rarity;       // レアリティ
    public float baseRate;               // 基本確率 (%)
    public float upgradeBonus;           // アップグレード時ボーナス (%/level)
    public int weight;                   // 抽選重み
}
```

**標準排出率設定例**：
- Common: 70% (基本), +0% (ボーナス)
- Uncommon: 25% (基本), +0.1%/level (ボーナス)
- Rare: 4.5% (基本), +0.15%/level (ボーナス)
- Epic: 0.45% (基本), +0.05%/level (ボーナス)
- Legendary: 0.05% (基本), +0.01%/level (ボーナス)

---

## 2. アップグレードシステム

### 2.1 アップグレード設定
```csharp
[Header("アップグレード設定")]
[SerializeField] private int maxUpgradeLevel = 10;             // 最大アップグレードレベル
[SerializeField] private GachaUpgrade[] upgrades;              // アップグレード効果配列
```

### 2.2 GachaUpgrade構造詳細
```csharp
[System.Serializable]
public struct GachaUpgrade
{
    public int level;                    // アップグレードレベル
    public int cost;                     // アップグレードコスト
    public string effectDescription;     // 効果説明
    public GachaUpgradeEffect[] effects; // 効果配列
}
```

### 2.3 アップグレード効果種類
```csharp
[System.Serializable]
public struct GachaUpgradeEffect
{
    public GachaUpgradeType type;        // 効果タイプ
    public CharacterRarity targetRarity; // 対象レアリティ
    public float value;                  // 効果値
}

public enum GachaUpgradeType
{
    RarityRateUp,        // レアリティ排出率向上
    CostReduction,       // ガチャコスト削減
    SimultaneousPull,    // 同時排出数増加
    GuaranteeImprovement // 保証システム強化
}
```

### 2.4 レベル別効果例
**レベル1**: コスト5%削減
**レベル2**: Rare以上確率+0.5%
**レベル3**: 同時排出数+1
**レベル5**: Epic以上確率+0.2%
**レベル7**: コスト10%削減（累積15%）
**レベル10**: Legendary確率+0.1%

---

## 3. 天井システム

### 3.1 天井設定
```csharp
[Header("天井システム")]
[SerializeField] private bool hasCeiling = true;               // 天井システム有効
[SerializeField] private int ceilingCount = 100;               // 天井回数
[SerializeField] private CharacterRarity ceilingRarity = CharacterRarity.Epic; // 天井時保証レアリティ
```

**天井システム動作**：
1. 連続でceilingCount回ガチャを引く
2. ceilingRarity以上が出なかった場合
3. 次回ガチャでceilingRarity以上を保証

### 3.2 天井カウント管理
- プレイヤーセーブデータで個別管理
- ceilingRarity以上が出現時にリセット
- ガチャレベルアップによる天井改善

---

## 4. ピックアップシステム

### 4.1 ピックアップ設定
```csharp
[Header("ピックアップ設定")]
[SerializeField] private bool hasPickup = false;               // ピックアップ有効
[SerializeField] private List<int> pickupCharacterIds;         // ピックアップキャラクターID配列
[SerializeField] private float pickupRate = 2.0f;              // ピックアップ追加確率倍率
```

**ピックアップ動作**：
1. 通常の排出率でレアリティ決定
2. 該当レアリティ内でピックアップキャラがいる場合
3. ピックアップ倍率を適用した重み付き抽選

---

## 5. 主要メソッド詳細

### 5.1 排出率計算
```csharp
public float GetBaseRarityRate(CharacterRarity rarity)
{
    var dropRate = dropRates.FirstOrDefault(r => r.rarity == rarity);
    return dropRate.baseRate;
}

public float GetLevelRarityBonus(int level, CharacterRarity rarity)
{
    var dropRate = dropRates.FirstOrDefault(r => r.rarity == rarity);
    return dropRate.upgradeBonus * level;
}
```

### 5.2 コスト計算
```csharp
public int GetUpgradeCost(int currentLevel)
{
    if (currentLevel >= maxUpgradeLevel) return 0;
    
    var upgrade = upgrades.FirstOrDefault(u => u.level == currentLevel + 1);
    return upgrade.cost;
}

public float GetCostReduction(int level)
{
    float totalReduction = 0f;
    
    for (int i = 1; i <= level; i++)
    {
        var upgrade = upgrades.FirstOrDefault(u => u.level == i);
        var costReductionEffect = upgrade.effects.FirstOrDefault(e => e.type == GachaUpgradeType.CostReduction);
        totalReduction += costReductionEffect.value;
    }
    
    return Mathf.Min(totalReduction, 0.5f); // 最大50%削減
}
```

### 5.3 同時排出数計算
```csharp
public int GetSimultaneousPullCount(int level)
{
    int pullCount = 1;
    
    for (int i = 1; i <= level; i++)
    {
        var upgrade = upgrades.FirstOrDefault(u => u.level == i);
        var simultaneousEffect = upgrade.effects.FirstOrDefault(e => e.type == GachaUpgradeType.SimultaneousPull);
        pullCount += (int)simultaneousEffect.value;
    }
    
    return pullCount;
}
```

---

## 6. バリデーションシステム

### 6.1 必須バリデーション
```csharp
public ValidationResult Validate()
{
    var result = new ValidationResult();
    
    // 基本設定検証
    if (string.IsNullOrEmpty(gachaSystemId))
        result.AddError("ガチャシステムIDが未設定です");
    
    if (baseCost <= 0)
        result.AddError("基本コストは正の値である必要があります");
    
    // 排出確率検証
    if (dropRates == null || dropRates.Length == 0)
        result.AddError("排出確率が設定されていません");
    else
    {
        float totalRate = dropRates.Sum(r => r.baseRate);
        if (Math.Abs(totalRate - 100f) > 0.01f)
            result.AddError($"排出確率の合計が100%ではありません（現在: {totalRate:F2}%）");
    }
    
    // アップグレード検証
    var duplicateLevels = upgrades.GroupBy(u => u.level).Where(g => g.Count() > 1);
    foreach (var level in duplicateLevels)
        result.AddError($"レベル {level.Key} のアップグレードが重複しています");
    
    return result;
}
```

### 6.2 バランスチェック
- 天井回数の妥当性（1-1000回）
- ピックアップ倍率の妥当性（1.5-5.0倍）
- アップグレードコストの指数的増加確認

---

## 7. 経済バランス設計

### 7.1 コスト設計指針
```
基本ガチャコスト: 100ゴールド
アップグレードコスト: level * 500ゴールド
最大コスト削減: 50%（アップグレード完全時）
実質最低コスト: 50ゴールド
```

### 7.2 確率設計指針
```
基本レア以上確率: 5%
最大レア以上確率: 15%（アップグレード完全時）
天井保証: 100回でEpic以上確定
Legendary期待値: 約200回
```

### 7.3 アップグレード進行設計
```
レベル1-3: 基本機能強化（コスト削減、確率向上）
レベル4-6: 利便性向上（同時排出、保証改善）
レベル7-10: 高確率化（高レア確率大幅向上）
```

---

## 8. 使用例とパターン

### 8.1 基本ガチャ設定例
```csharp
// 基本設定
gachaSystemId = "basic_gacha"
displayName = "基本ガチャ"
baseCost = 100
isActive = true

// 排出率設定
dropRates = [
    { Common, 70.0%, 0.0%/level, 700 },
    { Rare, 25.0%, 0.1%/level, 250 },
    { Epic, 4.5%, 0.15%/level, 45 },
    { Legendary, 0.5%, 0.05%/level, 5 }
]

// 天井設定
hasCeiling = true
ceilingCount = 100
ceilingRarity = Epic
```

### 8.2 イベントガチャ設定例
```csharp
// ピックアップ有効
hasPickup = true
pickupCharacterIds = [101, 102, 103] // イベントキャラクター
pickupRate = 2.5f // 2.5倍確率
```

---

## 9. 実装ガイドライン

### 9.1 新ガチャシステム作成手順
1. GachaSystemDataアセット作成
2. 基本設定・排出率調整
3. アップグレード効果設計
4. 天井・ピックアップ設定
5. バリデーション実行
6. 経済バランステスト

### 9.2 バランス調整指針
- Legendary期待値: 150-300回
- Epic期待値: 20-50回
- 天井到達率: プレイヤーの10-20%
- アップグレード完了期間: 1-2週間

### 9.3 パフォーマンス考慮
- 確率計算結果のキャッシュ
- 重み配列の事前計算
- バリデーション処理の最適化

---

*このGachaSystemData仕様書は、GatchaSpireにおけるガチャシステムの中核を定義します。実装時は段階的に機能を追加し、経済バランスの継続的な監視と調整を重視してください。*