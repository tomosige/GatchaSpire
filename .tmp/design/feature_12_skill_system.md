# 機能12: スキルシステム

## 概要
スキルの発動タイミング、効果範囲の判定、スキル連携の処理を担当するシステム。

## 実装クラス設計

### SkillManager
スキルシステム全体の管理を行うクラス。

**publicメソッド:**
- `void ExecuteSkill(Character caster, Skill skill, BattleState state)` - スキル実行
- `bool CanUseSkill(Character caster, Skill skill, BattleState state)` - スキル使用可能判定
- `List<Character> GetValidTargets(Character caster, Skill skill, BattleState state)` - 有効対象取得
- `void UpdateSkillCooldowns(Character character)` - スキルクールダウン更新
- `void ProcessSkillEffects(List<SkillEffect> effects, BattleState state)` - スキル効果処理

**詳細説明:**
- スキルの実行制御
- 対象選択の管理
- クールダウンシステム
- 効果の適用順序制御

### Skill
個別のスキルを表するクラス。

**publicメソッド:**
- `void Initialize(SkillData data)` - スキル初期化
- `List<SkillEffect> GetEffects()` - スキル効果取得
- `SkillTargetType GetTargetType()` - 対象タイプ取得
- `int GetCooldown()` - クールダウン取得
- `int GetManaCost()` - マナコスト取得
- `string GetDescription()` - スキル説明取得

**詳細説明:**
- スキルの基本情報管理
- 効果の定義
- 使用条件の管理
- UI表示用の情報提供

### SkillData
スキルの設定データを管理するScriptableObject。

**publicメソッド:**
- `List<SkillEffect> GetEffectsByLevel(int level)` - レベル別効果取得
- `SkillRange GetRange()` - スキル範囲取得
- `SkillTargetType GetTargetType()` - 対象タイプ取得
- `int GetCooldownByLevel(int level)` - レベル別クールダウン取得
- `string GetDescriptionByLevel(int level)` - レベル別説明取得

**詳細説明:**
- スキルの基本設定
- レベル別の効果変化
- 範囲と対象の定義
- バランス調整用パラメータ

### SkillEffect
スキル効果を表すクラス。

**publicメソッド:**
- `void Apply(Character target, Character caster, BattleState state)` - 効果適用
- `bool IsValid(Character target, Character caster, BattleState state)` - 効果有効性判定
- `SkillEffectType GetEffectType()` - 効果タイプ取得
- `float GetDuration()` - 効果継続時間取得
- `string GetEffectDescription()` - 効果説明取得

**詳細説明:**
- 個別効果の実装
- 効果の適用処理
- 継続効果の管理
- 効果の説明生成

### SkillRange
スキル効果範囲を管理するクラス。

**publicメソッド:**
- `List<Vector2Int> GetAffectedPositions(Vector2Int origin, Vector2Int direction)` - 影響範囲取得
- `bool IsInRange(Vector2Int origin, Vector2Int target)` - 範囲内判定
- `List<Character> GetTargetsInRange(Vector2Int origin, List<Character> candidates)` - 範囲内対象取得
- `void VisualizeRange(Vector2Int origin)` - 範囲可視化
- `SkillRangeType GetRangeType()` - 範囲タイプ取得

**詳細説明:**
- 範囲攻撃の対象判定
- 範囲パターンの定義
- 可視化システムとの連携
- 複雑な範囲形状の対応

### SkillComboSystem
スキル連携システムを管理するクラス。

**publicメソッド:**
- `void CheckForCombos(List<Skill> recentSkills, BattleState state)` - 連携チェック
- `void TriggerCombo(SkillCombo combo, BattleState state)` - 連携発動
- `List<SkillCombo> GetAvailableCombos(Character character, BattleState state)` - 利用可能連携取得
- `void RegisterComboCondition(SkillComboCondition condition)` - 連携条件登録
- `bool IsComboActive(SkillCombo combo)` - 連携有効判定

**詳細説明:**
- スキル連携の検出
- 連携効果の発動
- 連携条件の管理
- 戦略的な連携構築支援

### SkillTargetType (enum)
スキル対象タイプを表す列挙型。

**値:**
- `SingleEnemy` - 単体敵
- `MultipleEnemies` - 複数敵
- `AllEnemies` - 全敵
- `SingleAlly` - 単体味方
- `MultipleAllies` - 複数味方
- `AllAllies` - 全味方
- `Self` - 自分
- `Area` - 範囲指定