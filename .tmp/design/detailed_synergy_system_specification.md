# シナジーシステム詳細仕様書 - GatchaSpire

## 基本情報

- **作成日**: 2025-07-14
- **バージョン**: 1.1
- **対象Unity**: 2021.3.22f1
- **実装フェーズ**: Phase 2 - Step 2.4

---

## 1. システム概要

### 1.1 目的と役割
- **戦略的選択肢の拡大**: キャラクター配置の組み合わせによる戦術的深み
- **パズル的要素の強化**: 1キャラクター複数シナジー保有による構築の複雑性
- **段階的パワーアップ**: 同じシナジーでも体数により効果が強化される仕組み

### 1.2 設計方針
- **重複なし**: 1キャラクターは各シナジーに1回のみカウント
- **複数保有**: 1キャラクターが2-3個のシナジーを保有可能
- **段階的発動**: 体数に応じて効果が段階的に強化
- **複合効果**: 1つのシナジーレベルが複数の効果を持つ
- **シンプル第一**: 初期実装では相性システムは導入しない

---

## 2. シナジー設計の基本原則

### 2.1 シナジー設計で考慮すべき要素
シナジーを設計する際は、以下の3つの要素のみを考慮する：

1. **発動条件**: 何体必要なのか（2体/4体/6体など）
2. **効果段階**: 何段階あるのか（通常2-3段階）
3. **各レベルの効果**: それぞれのレベルごとの具体的な効果

### 2.2 効果タイプ

#### A. ステータス修正型（Phase 2.4で実装）
- **処理タイミング**: 戦闘開始時に計算、戦闘中は固定値
- **実装方法**: CharacterStatsの修正値として適用
- **計算方式**: 
  - 固定値: 序盤有利（例: 攻撃力+100）
  - 割合: 終盤有利（例: 攻撃力+20%）
- **適用範囲**: シナジーによって異なる（そのシナジー保有者のみ/全体）

#### B. 発動能力型（Phase 2.5以降で実装）
- **処理タイミング**: 特定条件で動的発動
- **実装方法**: イベント駆動、SkillEffect系と連携
- **発動条件例**: 戦闘開始時、時間経過、ダメージ時、死亡時など

---

## 3. 具体例

### 3.1 ナイトシナジー

1. **発動条件**: ナイト2体/4体/6体
2. **効果段階**: 3段階
3. **各レベルの効果**:

```
ナイト2体:
- 効果1(ステータス修正型): 戦闘開始時、全ナイトの防御+30（固定値）

ナイト4体:
- 効果1(ステータス修正型): 戦闘開始時、全ナイトの防御+60

ナイト6体:
- 効果1(ステータス修正型): 戦闘開始時、全ナイトの防御+100
```

### 3.2 メイジシナジー

1. **発動条件**: メイジ3体/6体
2. **効果段階**: 2段階
3. **各レベルの効果**:

```
メイジ3体:
- 効果1(ステータス修正型): 戦闘開始時、全メイジの魔力+40（固定値）

メイジ6体:
- 効果1(ステータス修正型): 戦闘開始時、全メイジの魔力+80
- 効果2(ステータス修正型): 戦闘開始時、全メイジの魔法防御+50
```

### 3.3 エルフシナジー

1. **発動条件**: エルフ2体/4体/6体
2. **効果段階**: 3段階
3. **各レベルの効果**:

```
エルフ2体:
- 効果1(ステータス修正型): 戦闘開始時、全エルフの速度+20（固定値）

エルフ4体:
- 効果1(ステータス修正型): 戦闘開始時、全エルフの速度+40
- 効果2(ステータス修正型): 戦闘開始時、全エルフのクリティカル率+15%

エルフ6体:
- 効果1(ステータス修正型): 戦闘開始時、全エルフの速度+60
- 効果2(ステータス修正型): 戦闘開始時、全エルフのクリティカル率+25%
```

---

## 4. データ構造設計

### 4.1 SynergyData（ScriptableObject）

```csharp
[CreateAssetMenu(fileName = "SynergyData", menuName = "GatchaSpire/Synergy Data")]
public class SynergyData : ScriptableObject, IValidatable
{
    [Header("基本情報")]
    public string synergyId;           // 一意識別子
    public string displayName;         // 表示名
    public string description;         // 説明文
    public SynergyType synergyType;    // 種族/クラス/属性/役割
    
    [Header("発動条件")]
    public SynergyCondition condition; // 発動に必要な条件
    
    [Header("効果レベル")]
    public SynergyLevel[] synergyLevels; // 各段階の効果定義
    
    [Header("UI設定")]
    public Sprite synergyIcon;         // アイコン
    public Color synergyColor;         // テーマカラー
    
    [Header("バランス設定")]
    public int priority;               // 計算優先度
    public bool isActive;              // 有効/無効
}
```

### 4.2 SynergyLevel（効果レベル）

```csharp
[System.Serializable]
public class SynergyLevel
{
    [Header("発動条件")]
    public int requiredCount;          // 必要キャラクター数
    
    [Header("効果リスト")]
    public List<SynergyEffect> effects; // 複数効果の定義
    
    [Header("表示設定")]
    public string levelDescription;    // レベル説明文
    public Color levelColor;           // レベル別カラー
}
```

### 4.3 SynergyEffect（効果基底クラス）

```csharp
public abstract class SynergyEffect : ScriptableObject
{
    [Header("基本設定")]
    public string effectId;            // 効果識別子
    public string effectName;          // 効果名
    public string effectDescription;   // 効果説明
    
    [Header("適用設定")]
    public SynergyTarget targetType;   // 適用対象（自分のみ/シナジー保有者/全体）
    public SynergyEffectType effectType; // ステータス修正型/発動能力型
    
    // 派生クラスで実装
    public abstract void ApplyEffect(List<Character> targets, BattleContext context);
    public abstract void RemoveEffect(List<Character> targets, BattleContext context);
    public abstract bool CanApply(BattleContext context);
}
```

### 4.4 具体的な効果クラス

```csharp
// ステータス修正型
[CreateAssetMenu(menuName = "GatchaSpire/Synergy Effects/Stat Modifier")]
public class SynergyStatModifierEffect : SynergyEffect
{
    [Header("ステータス修正")]
    public StatType statType;          // 修正するステータス
    public StatModifierType modifierType; // 固定値/割合
    public float modifierValue;        // 修正値
    public bool isPermanent;           // 永続かどうか
}

// 発動能力型
[CreateAssetMenu(menuName = "GatchaSpire/Synergy Effects/Trigger Ability")]
public class SynergyTriggerAbilityEffect : SynergyEffect
{
    [Header("発動条件")]
    public TriggerCondition triggerCondition; // 発動条件
    public float triggerChance;        // 発動確率
    public float cooldownTime;         // クールダウン時間
    
    [Header("能力効果")]
    public List<SkillEffect> abilityEffects; // 発動する能力効果
    public GameObject effectPrefab;    // エフェクトプレハブ
}
```

---

## 5. 実装指針

### 5.1 システム統合

#### SynergyCalculator（計算システム）
- **役割**: パーティ構成からシナジー発動状況を計算
- **処理**: キャラクター配置変更時のリアルタイム更新
- **最適化**: キャッシュ機能、差分更新による高速化

#### SynergyEffectApplier（効果適用システム）
- **役割**: 計算されたシナジーを実際のキャラクターに適用
- **連携**: CharacterStats、SkillSystem との統合
- **管理**: 効果の重複、除去、更新処理

#### SynergyVisualizer（表示システム）
- **役割**: シナジー状態のUI表示
- **機能**: 発動状況、プレビュー、ハイライト
- **実装**: Phase 3のUI統合で詳細化

### 5.2 既存システムとの連携

#### CharacterData との連携
```csharp
// CharacterData に追加予定
[Header("シナジー")]
public List<SynergyType> synergyTypes; // 所属シナジー（2-3個）
```

#### BattleSystem との連携
- **戦闘開始時**: ステータス修正型効果の一括適用
- **戦闘中**: 発動能力型効果のイベント処理
- **戦闘終了時**: 効果のクリーンアップ

#### BoardSystem との連携
- **配置変更時**: シナジー計算の即座更新
- **プレビュー**: 配置前のシナジー状況表示

---

---

## 4. 実装優先度

### 4.1 Phase 2.4 実装内容

#### 必須実装
1. **SynergyData, SynergyLevel, SynergyEffect基底クラス**
2. **SynergyStatModifierEffect（ステータス修正型）**
3. **SynergyCalculator（基本計算システム）**
4. **SynergyEffectApplier（基本適用システム）**
5. **基本テストケース（5-10種類のシナジー）**

#### 後回し実装
1. **SynergyTriggerAbilityEffect（発動能力型）** → Phase 2.5
2. **SynergyVisualizer（表示システム）** → Phase 3

### 4.2 Phase 2.4 実装用シナジー例

#### 基本実装用シナジー
1. **ナイト2/4/6体**: 防御力固定値上昇
2. **メイジ3/6体**: 魔力固定値上昇  
3. **エルフ2/4/6体**: 速度とクリティカル率上昇
4. **ウォリアー2/4体**: 攻撃力固定値上昇
5. **ドラゴン4/6体**: 全ステータス上昇

---

## 5. 将来拡張

### 5.1 発動能力型シナジー（Phase 2.5以降）
- 時間経過による追加効果
- ダメージ時の反射効果
- 死亡時の効果転移

### 5.2 相性システム（Phase 4以降）
- シナジー間の組み合わせボーナス
- 対立シナジーの設計

---

このシナジーシステム仕様により、GatchaSpireの戦略性とパズル要素を強化し、キャラクター配置の戦術的深みを提供します。