# 新設計: バトルマネージャー統合版

## 概要
機能11（オートバトルシステム）のAutoBattleManager、BattleSimulator、BattleStateを統合し、バトル処理を一元管理する。

## 統合クラス設計

### BattleManager
バトルシステム全体を統合管理するメインクラス。

**publicメソッド:**

**バトル制御:**
- `void StartBattle(List<Character> playerTeam, List<Character> enemyTeam)` - 戦闘開始
- `void PauseBattle()` - 戦闘一時停止
- `void ResumeBattle()` - 戦闘再開
- `void StopBattle()` - 戦闘停止
- `bool IsBattleActive()` - 戦闘中判定
- `BattlePhase GetCurrentPhase()` - 現在のバトルフェーズ取得

**バトル情報:**
- `BattleResult GetBattleResult()` - 戦闘結果取得
- `float GetBattleProgress()` - 戦闘進行率取得
- `List<Character> GetAliveCharacters(BattleTeam team)` - 生存キャラクター取得
- `Character GetCurrentActor()` - 現在行動中キャラクター取得
- `BattleStatistics GetCurrentStatistics()` - 現在の戦闘統計取得

**バトル状態管理:**
- `void UpdateBattleState()` - 戦闘状態更新
- `bool IsBattleOver()` - 戦闘終了判定
- `int GetCurrentTurn()` - 現在ターン数取得
- `float GetTurnTimeRemaining()` - ターン残り時間取得

**詳細説明:**
- 旧AutoBattleManager、BattleSimulator、BattleStateの機能を統合
- 戦闘の制御、実行、状態管理を一元化
- AIシステムとスキルシステムとの統合インターフェース
- リアルタイム戦闘進行の管理

**privateメソッド（内部実装）:**
- `BattleResult SimulateBattle(BattleSetup setup)` - 戦闘シミュレーション実行
- `void ProcessTurn()` - ターン処理
- `void ProcessCharacterAction(Character character)` - キャラクター行動処理
- `void ApplyDamage(Character attacker, Character target, int damage)` - ダメージ適用
- `void CheckBattleEndConditions()` - 戦闘終了条件判定
- `void InitializeBattleState(List<Character> playerTeam, List<Character> enemyTeam)` - 戦闘状態初期化
- `Character CalculateNextActor()` - 次行動キャラクター計算
- `void UpdateTurnOrder()` - ターン順序更新

### BattleSetup
戦闘セットアップ情報を管理するクラス。

**publicメソッド:**
- `BattleSetup(List<Character> playerTeam, List<Character> enemyTeam, BattleConditions conditions)` - コンストラクタ
- `List<Character> GetPlayerTeam()` - プレイヤーチーム取得
- `List<Character> GetEnemyTeam()` - 敵チーム取得
- `BattleConditions GetBattleConditions()` - 戦闘条件取得
- `bool IsValid()` - セットアップ有効性判定
- `string GetSetupSummary()` - セットアップサマリー取得

**詳細説明:**
- 戦闘開始時の設定情報を管理
- 戦闘条件（天候、地形効果等）の適用
- セットアップの検証機能

### BattlePhase (enum)
戦闘フェーズを表す列挙型。

**値:**
- `Preparation` - 準備フェーズ
- `TurnStart` - ターン開始
- `CharacterAction` - キャラクター行動
- `EffectResolution` - 効果解決
- `TurnEnd` - ターン終了
- `BattleEnd` - 戦闘終了

### BattleStatistics
戦闘統計情報を管理するクラス。

**publicメソッド:**
- `void RecordDamage(Character attacker, Character target, int damage)` - ダメージ記録
- `void RecordSkillUse(Character caster, Skill skill)` - スキル使用記録
- `void RecordHeal(Character healer, Character target, int healAmount)` - 回復記録
- `int GetTotalDamageDealt(Character character)` - 総与ダメージ取得
- `int GetTotalDamageTaken(Character character)` - 総被ダメージ取得
- `int GetSkillUseCount(Character character, Skill skill)` - スキル使用回数取得
- `string GetStatisticsSummary()` - 統計サマリー取得

**詳細説明:**
- 戦闘中の行動統計を記録
- 戦闘結果の詳細分析
- バランス調整用のデータ収集

### BattleConditions
戦闘条件を管理するクラス。

**publicメソッド:**
- `void AddCondition(BattleConditionType type, object value)` - 条件追加
- `void RemoveCondition(BattleConditionType type)` - 条件除去
- `bool HasCondition(BattleConditionType type)` - 条件存在判定
- `T GetConditionValue<T>(BattleConditionType type)` - 条件値取得
- `void ApplyConditions(List<Character> allCharacters)` - 条件適用
- `string GetConditionsDescription()` - 条件説明取得

**詳細説明:**
- 特殊な戦闘条件の管理
- 天候、地形、イベント効果等
- 動的な条件変更への対応

## 統合による利点

### 1. 処理の効率化
- 戦闘状態の管理とシミュレーションが一体化
- 不要なデータコピーの削減
- リアルタイム処理の最適化

### 2. デバッグの容易性
- 戦闘処理が単一クラスに集約
- 状態遷移の追跡が容易
- バトルログの一元管理

### 3. 拡張性の向上
- 新しい戦闘ルールの追加が容易
- 特殊戦闘モードの実装が簡単
- AI改良時の影響範囲が明確

## 削除されるクラス
- `AutoBattleManager`（機能11）
- `BattleSimulator`（機能11）
- `BattleState`（機能11）

これらの機能は`BattleManager`に統合され、必要に応じてprivateメソッドとして実装される。

## 残存するクラス（変更なし）
- `BattleAI`（機能11）
- `BattleResult`（機能11）
- `BattleAction`（機能11）

これらは独立した機能として適切な責任分割が保たれているため変更不要。`BattleAI`は`BattleManager`から呼び出され、`BattleResult`と`BattleAction`はデータクラスとして継続使用される。