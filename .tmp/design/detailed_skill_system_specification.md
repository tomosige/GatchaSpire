# GatchaSpire スキルシステム詳細仕様書

## 概要
GatchaSpireにおけるスキルシステムの詳細仕様。レベル制スキル習得、個別クールダウン、複雑なスキル効果システムを定義する。

## 基本設計方針
- **レベル制スキル習得**: Lv3/6/10で計3個のスキルを習得
- **個別クールダウン制**: 各スキルが独立したクールタイムを持つ
- **リアルタイム戦闘**: クールダウンがリアルタイムで減少
- **合成レベルジャンプ対応**: 複数レベル上昇時もスキル習得を保証

---

## 1. キャラクタースキル習得システム

### 1.1 スキル習得レベル
```csharp
public static readonly int[] SKILL_UNLOCK_LEVELS = { 3, 6, 10 };
```

| レベル | スキル種類 | 特徴 |
|--------|------------|------|
| Lv3 | 基本スキル | 単体攻撃、小回復等の基本能力 |
| Lv6 | 戦術スキル | 範囲攻撃、バフ・デバフ等の戦術能力 |
| Lv10 | 必殺技 | 強力効果、固有能力等の決定打 |

### 1.2 CharacterSkillProgression クラス
```csharp
public class CharacterSkillProgression
{
    public int Level { get; set; }
    public Dictionary<int, Skill> UnlockedSkills { get; set; } = new();
    
    // スキル習得レベル（固定）
    public static readonly int[] SKILL_UNLOCK_LEVELS = { 3, 6, 10 };
    
    public bool CanUnlockSkill(int level) => SKILL_UNLOCK_LEVELS.Contains(level);
    public int GetSkillSlot(int level) => Array.IndexOf(SKILL_UNLOCK_LEVELS, level);
}
```

---

## 2. 個別クールダウンシステム

### 2.1 スキルクールダウン管理
```csharp
public class CharacterSkillManager
{
    public Dictionary<int, float> SkillCooldowns { get; set; } = new();
    public Dictionary<int, float> SkillLastUsedTime { get; set; } = new();
    
    public bool IsSkillReady(int skillSlot, float currentTime)
    {
        if (!SkillLastUsedTime.ContainsKey(skillSlot)) return true;
        
        float cooldown = SkillCooldowns.GetValueOrDefault(skillSlot, 0f);
        return currentTime - SkillLastUsedTime[skillSlot] >= cooldown;
    }
    
    public void UseSkill(int skillSlot, float currentTime, float cooldownTime)
    {
        SkillLastUsedTime[skillSlot] = currentTime;
        SkillCooldowns[skillSlot] = cooldownTime;
    }
}
```

### 2.2 クールダウン更新処理
- **更新間隔**: 0.1秒
- **管理方式**: 最終使用時刻を記録し、経過時間で判定
- **同時発動**: 複数スキルのクールダウンが同時に完了可能

---

## 3. SkillEffect詳細設計

### 3.1 基本SkillEffectクラス
```csharp
public abstract class SkillEffect
{
    // 基本プロパティ
    public SkillEffectType EffectType { get; set; }
    public string EffectName { get; set; }
    public string Description { get; set; }
    
    // 数値効果
    public float BaseValue { get; set; }           // 基本効果値
    public float ScalingRatio { get; set; }        // スケーリング係数
    public ScalingAttribute ScalingSource { get; set; } // スケール元ステータス
    
    // 対象・範囲
    public SkillTargetType TargetType { get; set; }
    public int MaxTargets { get; set; }            // 最大対象数
    public SkillRange Range { get; set; }
    
    // 継続効果
    public float Duration { get; set; }            // 効果継続時間（秒）
    public bool IsPermanent { get; set; }          // 永続効果かどうか
    public float TickInterval { get; set; }        // DoT/HoTの間隔
    
    // 確率・条件
    public float SuccessChance { get; set; } = 1.0f; // 成功確率（0.0-1.0）
    public List<SkillCondition> TriggerConditions { get; set; }
    
    // スタック・重複
    public bool CanStack { get; set; }             // 効果重複可能か
    public int MaxStacks { get; set; } = 1;        // 最大スタック数
    public StackBehavior StackType { get; set; }   // スタック時の挙動
    
    // 視覚効果
    public string EffectAnimationId { get; set; }  // エフェクトアニメーション
    public Color EffectColor { get; set; }         // エフェクト色
    public bool ShowFloatingText { get; set; }     // ダメージ表示等
    
    // 抽象メソッド
    public abstract void Apply(Character target, Character caster, BattleContext context);
    public abstract bool CanApply(Character target, Character caster, BattleContext context);
}
```

### 3.2 具体的なSkillEffect種類

#### DamageEffect（ダメージ効果）
```csharp
public class DamageEffect : SkillEffect
{
    public DamageType DamageType { get; set; }     // 物理/魔法
    public bool IgnoreDefense { get; set; }        // 防御無視
    public float CriticalChance { get; set; }      // クリティカル確率
    public float CriticalMultiplier { get; set; }  // クリティカル倍率
}
```

#### HealEffect（回復効果）
```csharp
public class HealEffect : SkillEffect
{
    public bool CanOverheal { get; set; }          // 最大HP超過回復
    public float OverhealDecayRate { get; set; }   // 超過HP減衰率
}
```

#### StatModifierEffect（ステータス変更効果）
```csharp
public class StatModifierEffect : SkillEffect
{
    public StatType TargetStat { get; set; }       // 対象ステータス
    public ModifierType ModifierType { get; set; } // 加算/乗算
    public bool IsDebuff { get; set; }             // デバフかどうか
}
```

#### StatusEffect（状態異常効果）
```csharp
public class StatusEffect : SkillEffect
{
    public StatusType StatusType { get; set; }     // 毒、麻痺、沈黙等
    public bool RemoveOnDamage { get; set; }       // ダメージで解除
    public int RemoveOnActionCount { get; set; }   // 行動回数で解除
}
```

#### DisplacementEffect（位置操作効果）
```csharp
public class DisplacementEffect : SkillEffect
{
    public DisplacementType MovementType { get; set; } // ノックバック、引き寄せ等
    public int Distance { get; set; }              // 移動距離
    public bool CanMoveOverObstacles { get; set; } // 障害物越え可能
}
```

### 3.3 補助列挙型

#### ScalingAttribute（スケーリング元ステータス）
```csharp
public enum ScalingAttribute
{
    Attack, MagicAttack, Defense, MagicDefense, 
    MaxHP, CurrentHP, MissingHP, Level
}
```

#### StackBehavior（スタック挙動）
```csharp
public enum StackBehavior
{
    Refresh,        // 継続時間リセット
    Extend,         // 継続時間延長
    Intensify,      // 効果値増加
    Independent     // 独立効果
}
```

#### DamageType（ダメージ種類）
```csharp
public enum DamageType
{
    Physical, Magical, True, // 物理、魔法、確定
    Fire, Water, Earth, Air  // 属性ダメージ
}
```

#### StatusType（状態異常種類）
```csharp
public enum StatusType
{
    Poison, Burn, Bleed,     // DoT系
    Stun, Silence, Root,     // 行動阻害
    Charm, Fear, Sleep,      // 制御系
    Shield, Immunity, Regen  // 防御系
}
```

#### DisplacementType（位置操作種類）
```csharp
public enum DisplacementType
{
    Knockback, PullToward, Teleport, Swap
}
```

---

## 4. レベルアップ・スキル習得ロジック

### 4.1 安全なレベルアップ処理
```csharp
public class Character
{
    public int Level { get; private set; } = 1;
    public Dictionary<int, Skill> UnlockedSkills { get; private set; } = new();
    
    // スキル習得レベル定義
    public static readonly int[] SKILL_UNLOCK_LEVELS = { 3, 6, 10 };
    
    /// <summary>
    /// レベルアップ処理（合成による複数レベル上昇対応）
    /// </summary>
    public List<SkillUnlockResult> LevelUp(int targetLevel)
    {
        var unlockResults = new List<SkillUnlockResult>();
        
        if (targetLevel <= Level)
        {
            Debug.LogWarning($"対象レベル{targetLevel}は現在レベル{Level}以下です");
            return unlockResults;
        }
        
        int oldLevel = Level;
        Level = targetLevel;
        
        // 旧レベルから新レベルまでの間で習得可能なスキルをチェック
        foreach (int unlockLevel in SKILL_UNLOCK_LEVELS)
        {
            // 旧レベルより上で、新レベル以下のスキル習得レベルをすべて処理
            if (unlockLevel > oldLevel && unlockLevel <= targetLevel)
            {
                var result = TryUnlockSkill(unlockLevel);
                if (result != null)
                {
                    unlockResults.Add(result);
                }
            }
        }
        
        return unlockResults;
    }
    
    /// <summary>
    /// 指定レベルでのスキル習得試行
    /// </summary>
    private SkillUnlockResult TryUnlockSkill(int unlockLevel)
    {
        int skillSlot = GetSkillSlotByLevel(unlockLevel);
        
        if (skillSlot == -1)
        {
            Debug.LogError($"レベル{unlockLevel}は有効なスキル習得レベルではありません");
            return null;
        }
        
        // 既に習得済みかチェック
        if (UnlockedSkills.ContainsKey(skillSlot))
        {
            Debug.LogWarning($"スキルスロット{skillSlot}は既に習得済みです");
            return null;
        }
        
        // キャラクターデータからスキルを取得
        var skillData = CharacterData.GetSkillByLevel(unlockLevel);
        if (skillData == null)
        {
            Debug.LogError($"レベル{unlockLevel}のスキルデータが見つかりません");
            return null;
        }
        
        // スキル習得実行
        var skill = new Skill(skillData);
        UnlockedSkills[skillSlot] = skill;
        
        return new SkillUnlockResult
        {
            UnlockLevel = unlockLevel,
            SkillSlot = skillSlot,
            UnlockedSkill = skill,
            CharacterName = CharacterData.characterName
        };
    }
    
    /// <summary>
    /// レベルからスキルスロットを取得
    /// </summary>
    private int GetSkillSlotByLevel(int level)
    {
        return Array.IndexOf(SKILL_UNLOCK_LEVELS, level);
    }
}
```

### 4.2 スキル習得結果の管理
```csharp
public class SkillUnlockResult
{
    public int UnlockLevel { get; set; }
    public int SkillSlot { get; set; }
    public Skill UnlockedSkill { get; set; }
    public string CharacterName { get; set; }
    
    public override string ToString()
    {
        return $"{CharacterName}がレベル{UnlockLevel}でスキル「{UnlockedSkill.SkillName}」を習得しました";
    }
}
```

### 4.3 テストケース

#### 通常レベルアップ
```csharp
// レベル2→3（スキル1個習得）
var results = character.LevelUp(3);
Assert.AreEqual(1, results.Count);
Assert.AreEqual(3, results[0].UnlockLevel);
```

#### ジャンプレベルアップ
```csharp
// レベル2→4（スキル1個習得、レベル3をスキップしない）
var results = character.LevelUp(4);
Assert.AreEqual(1, results.Count);
Assert.AreEqual(3, results[0].UnlockLevel);

// レベル1→7（スキル2個習得）
var results = character.LevelUp(7);
Assert.AreEqual(2, results.Count);
Assert.AreEqual(3, results[0].UnlockLevel);
Assert.AreEqual(6, results[1].UnlockLevel);

// レベル5→10（スキル1個習得）
var results = character.LevelUp(10);
Assert.AreEqual(1, results.Count);
Assert.AreEqual(10, results[0].UnlockLevel);
```

---

## 5. キャラクター合成との統合

### 5.1 CharacterInventoryManagerとの統合
```csharp
public class CharacterInventoryManager
{
    /// <summary>
    /// キャラクター合成処理
    /// </summary>
    public CharacterFusionResult FuseCharacters(int baseCharacterId, List<int> materialCharacterIds)
    {
        var baseCharacter = GetCharacter(baseCharacterId);
        var targetLevel = CalculateFusionLevel(baseCharacter, materialCharacterIds);
        
        // レベルアップとスキル習得
        var skillUnlockResults = baseCharacter.LevelUp(targetLevel);
        
        // 合成結果作成
        return new CharacterFusionResult
        {
            ResultCharacter = baseCharacter,
            SkillUnlocks = skillUnlockResults,
            NewLevel = targetLevel,
            ExpGained = targetLevel - baseCharacter.Level
        };
    }
}
```

---

## 6. スキル効果の計算例

### 6.1 ダメージ計算
```csharp
public float CalculateDamage(Character caster, Character target)
{
    float baseDamage = BaseValue;
    
    // スケーリング計算
    if (ScalingSource != ScalingAttribute.None)
    {
        float scalingStat = caster.GetStat(ScalingSource);
        baseDamage += scalingStat * ScalingRatio;
    }
    
    // 防御計算（物理ダメージの場合）
    if (DamageType == DamageType.Physical && !IgnoreDefense)
    {
        float defense = target.GetStat(StatType.Defense);
        baseDamage *= (100f / (100f + defense));
    }
    
    // クリティカル判定
    if (Random.value <= CriticalChance)
    {
        baseDamage *= CriticalMultiplier;
    }
    
    return baseDamage;
}
```

---

## 7. 実装優先度

### Phase 1（基本スキルシステム）
1. レベル制スキル習得システム
2. 個別クールダウン管理
3. 基本的なSkillEffect（ダメージ、回復）
4. スキル習得ロジックのテスト

### Phase 2（拡張スキル効果）
1. ステータス変更効果
2. 状態異常システム
3. 位置操作効果
4. 複雑な発動条件

### Phase 3（高度な機能）
1. スキル効果のスタッキング
2. 特殊効果システム
3. スキル連携機能
4. 視覚効果統合

---

## 8. 注意事項

### 8.1 パフォーマンス考慮
- スキル効果の計算は0.1秒間隔で実行される
- 大量の継続効果がある場合の最適化が必要
- SkillEffectオブジェクトのプーリングを検討

### 8.2 バランス調整
- スキル効果値はScriptableObjectで管理し、実行時調整可能
- 強力すぎるスキル組み合わせの制限機能
- テストプレイでの数値調整の容易性

### 8.3 拡張性
- 新しいSkillEffectタイプの追加が容易
- カスタムスキル効果の実装サポート
- MOD対応のためのインターフェース設計

---

*このスキルシステム仕様書は、GatchaSpireの戦略性と深度を決定する重要な要素です。実装時は段階的に機能を追加し、各段階でのテストとバランス調整を重視してください。*