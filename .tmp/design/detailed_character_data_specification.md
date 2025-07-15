# GatchaSpire CharacterData詳細仕様書

## 概要
CharacterDataは、GatchaSpireにおけるキャラクターの基本データを管理するScriptableObjectです。レベル制成長、ステータス計算、レアリティシステム、分類管理を担当します。

## 基本設計方針
- **レアリティベース成長**: レアリティによる基本性能差とステータス倍率
- **レベル制成長システム**: 1-100レベルでの段階的成長
- **多軸分類システム**: 種族・クラス・属性・役割による多角的分類
- **バランス調整容易性**: ScriptableObjectによる実行時調整対応

---

## 1. データ構造詳細

### 1.1 基本情報
```csharp
[Header("基本情報")]
[SerializeField] private string characterName = "";        // 表示名
[SerializeField] private string description = "";          // 説明文
[SerializeField] private int characterId = 0;              // 一意ID（int型）
```

**設計指針**：
- `characterId`はマスターデータ全体で一意
- `characterName`は多言語対応を考慮した設計
- `description`はUI表示とフレーバーテキスト両対応

### 1.2 分類システム
```csharp
[Header("分類")]
[SerializeField] private CharacterRarity rarity = CharacterRarity.Common;
[SerializeField] private CharacterRace race = CharacterRace.Human;
[SerializeField] private CharacterClass characterClass = CharacterClass.Warrior;
[SerializeField] private CharacterElement element = CharacterElement.None;
[SerializeField] private CharacterRole role = CharacterRole.DPS;
```

**分類枠組み**：
- **CharacterRarity**: Common/Uncommon/Rare/Epic/Legendary（5段階）
- **CharacterRace**: Human/Elf/Dwarf/Orc/Dragon/Demon（6種族）
- **CharacterClass**: Warrior/Mage/Archer/Healer/Assassin/Tank（6クラス）
- **CharacterElement**: None/Fire/Water/Earth/Air（5属性）
- **CharacterRole**: DPS/Tank/Support/Healer（4役割）

### 1.3 レベルシステム
```csharp
[Header("レベルシステム")]
[SerializeField] private int baseLevel = 1;                // 初期レベル
[SerializeField] private int maxLevel = 100;               // 最大レベル
[SerializeField] private int expToNextLevel = 100;         // 次レベル必要経験値
[SerializeField] private float expGrowthRate = 1.2f;       // 経験値成長率
```

**経験値計算公式**：
```
レベルnから(n+1)への必要経験値 = expToNextLevel * (expGrowthRate ^ (n - baseLevel))
累計必要経験値 = Σ(i=baseLevel to targetLevel-1) expToNextLevel * (expGrowthRate ^ (i - baseLevel))
```

### 1.4 ステータスシステム
```csharp
[Header("基本ステータス")]
[SerializeField] private CharacterStats baseStats = new CharacterStats(100, 50, 10, 8, 12, 6, 5, 8);

[Header("成長率（レベルアップ時の増加率）")]
[SerializeField, Range(0f, 1f)] private float hpGrowthRate = 0.1f;
[SerializeField, Range(0f, 1f)] private float mpGrowthRate = 0.08f;
[SerializeField, Range(0f, 1f)] private float attackGrowthRate = 0.12f;
[SerializeField, Range(0f, 1f)] private float defenseGrowthRate = 0.1f;
[SerializeField, Range(0f, 1f)] private float speedGrowthRate = 0.05f;
[SerializeField, Range(0f, 1f)] private float magicGrowthRate = 0.08f;
[SerializeField, Range(0f, 1f)] private float resistanceGrowthRate = 0.06f;
[SerializeField, Range(0f, 1f)] private float luckGrowthRate = 0.03f;
```

**ステータス計算公式**：
```
レベルnでのステータス = baseStat * (1 + growthRate * (n - baseLevel)) * rarityMultiplier
```

**レアリティ倍率**：
- Common: 1.0
- Uncommon: 1.1
- Rare: 1.25
- Epic: 1.5
- Legendary: 2.0

---

## 2. CharacterStats構造体

### 2.1 ステータス定義
```csharp
[System.Serializable]
public struct CharacterStats
{
    public int BaseHP;          // HP（生命力）
    public int BaseMP;          // MP（マジックポイント）
    public int BaseAttack;      // 攻撃力
    public int BaseDefense;     // 防御力
    public int BaseSpeed;       // 素早さ
    public int BaseMagic;       // 魔力
    public int BaseResistance;  // 魔法防御
    public int BaseLuck;        // 運
}
```

### 2.2 ステータス用途
- **HP**: 戦闘での生存力、最大HP
- **MP**: スキル使用コスト、最大MP
- **Attack**: 物理攻撃のダメージ基準
- **Defense**: 物理攻撃の軽減率計算
- **Speed**: 行動順序、回避率影響
- **Magic**: 魔法攻撃のダメージ基準
- **Resistance**: 魔法攻撃の軽減率計算
- **Luck**: クリティカル率、アイテムドロップ率影響

### 2.3 戦闘力計算
```csharp
public int CalculateBattlePower()
{
    // 重み付き合計による戦闘力算出
    return (BaseHP * 2) + (BaseAttack * 4) + (BaseDefense * 3) + 
           (BaseSpeed * 2) + (BaseMagic * 4) + (BaseResistance * 3) + 
           (BaseLuck * 1);
}
```

---

## 3. アセット管理

### 3.1 アート素材
```csharp
[Header("アート素材")]
[SerializeField] private Sprite characterIcon;      // アイコン画像（64x64推奨）
[SerializeField] private Sprite characterPortrait;  // ポートレート画像（256x256推奨）
[SerializeField] private GameObject characterModel; // 3Dモデル（戦闘表示用）
```

### 3.2 経済設定
```csharp
[Header("コスト設定")]
[SerializeField] private int gachaCost = 100;       // ガチャコスト
[SerializeField] private int upgradeCost = 50;      // アップグレードコスト
[SerializeField] private int sellPrice = 30;        // 売却価格
```

**コスト設計指針**：
- レアリティが高いほど高コスト
- `sellPrice = gachaCost * 0.3`を基準
- `upgradeCost = gachaCost * 0.5`を基準

---

## 4. バリデーションシステム

### 4.1 必須バリデーション
```csharp
public ValidationResult Validate()
{
    var result = new ValidationResult();
    
    // 基本情報検証
    if (string.IsNullOrEmpty(characterName))
        result.AddError("キャラクター名が設定されていません");
    
    if (characterId <= 0)
        result.AddError("キャラクターIDは1以上である必要があります");
    
    // レベル設定検証
    if (baseLevel <= 0 || maxLevel <= baseLevel)
        result.AddError("レベル設定が無効です");
    
    // ステータス検証
    if (!baseStats.IsValid())
        result.AddError("基本ステータスが無効です");
    
    return result;
}
```

### 4.2 バランスチェック
- 戦闘力が極端に高い/低い場合の警告
- 成長率が異常な値の警告
- アセット未設定の警告

---

## 5. レベル計算メソッド詳細

### 5.1 経験値からレベル計算
```csharp
public int CalculateLevelFromExp(int currentExp)
{
    int level = baseLevel;
    int requiredExp = 0;
    
    while (level < maxLevel)
    {
        int expForNextLevel = Mathf.RoundToInt(expToNextLevel * Mathf.Pow(expGrowthRate, level - baseLevel));
        
        if (currentExp < requiredExp + expForNextLevel)
            break;
        
        requiredExp += expForNextLevel;
        level++;
    }
    
    return level;
}
```

### 5.2 レベルから必要経験値計算
```csharp
public int CalculateExpForLevel(int level)
{
    if (level <= baseLevel) return 0;
    
    int totalExp = 0;
    for (int i = baseLevel; i < level; i++)
    {
        totalExp += Mathf.RoundToInt(expToNextLevel * Mathf.Pow(expGrowthRate, i - baseLevel));
    }
    return totalExp;
}
```

---

## 6. 使用例とパターン

### 6.1 レアリティ別設定例
**Common（コモン）設定例**：
```
baseStats: HP=80, Attack=30, Defense=20
gachaCost: 100, sellPrice: 30
growthRate: 全て0.08-0.12程度
```

**Legendary（レジェンダリー）設定例**：
```
baseStats: HP=150, Attack=60, Defense=40
gachaCost: 500, sellPrice: 150
growthRate: 全て0.15-0.20程度
```

### 6.2 役割別ステータス傾向
- **Tank**: HP・Defense重視、Attack・Speed控えめ
- **DPS**: Attack・Magic重視、Defense・HP控えめ
- **Support**: Speed・Luck重視、バランス型
- **Healer**: Magic・MP重視、Defense・Attack控えめ

---

## 7. 実装ガイドライン

### 7.1 新キャラクター作成手順
1. CharacterDataアセット作成
2. 基本情報・分類設定
3. ステータス・成長率調整
4. アート素材アサイン
5. バリデーション実行
6. バランステスト

### 7.2 バランス調整指針
- 同レアリティ内での戦闘力差は±20%以内
- レアリティ間の戦闘力比は約1.5倍
- 役割による特化は明確に差別化

### 7.3 パフォーマンス考慮
- ステータス計算結果のキャッシュ
- 頻繁なアクセスメソッドの最適化
- バリデーション処理の軽量化

---

*このCharacterData仕様書は、GatchaSpireにおけるキャラクター設計の中核を定義します。実装時は段階的に機能を追加し、各段階でのバランステストを重視してください。*