# 新設計: ボードシステム分割版

## 概要
機能10（5x5盤面システム）のBoardManagerを単一責任の原則に従って3つのクラスに分割する。

## 分割クラス設計

### BoardStateManager
盤面の状態管理のみを担当するクラス。

**publicメソッド:**

**基本配置操作:**
- `bool PlaceCharacter(Character character, Vector2Int position)` - キャラクター配置
- `bool MoveCharacter(Vector2Int fromPosition, Vector2Int toPosition)` - キャラクター移動
- `Character RemoveCharacter(Vector2Int position)` - キャラクター除去
- `Character GetCharacterAt(Vector2Int position)` - 指定位置のキャラクター取得

**状態取得:**
- `List<Character> GetAllPlacedCharacters()` - 配置済み全キャラクター取得
- `List<Vector2Int> GetOccupiedPositions()` - 占有位置一覧取得
- `List<Vector2Int> GetEmptyPositions()` - 空き位置一覧取得
- `bool IsPositionOccupied(Vector2Int position)` - 位置占有判定

**盤面管理:**
- `void ClearBoard()` - 盤面クリア
- `BoardState GetBoardState()` - 盤面状態取得
- `void SetBoardState(BoardState state)` - 盤面状態設定
- `bool IsValidPosition(Vector2Int position)` - 有効位置判定

**詳細説明:**
- 5x5グリッドの状態のみを管理
- キャラクター配置の物理的な管理
- 状態変更時のイベント通知
- 盤面の保存/復元機能

### PlacementValidator
配置の検証のみを担当するクラス。

**publicメソッド:**

**配置検証:**
- `bool ValidatePlacement(Character character, Vector2Int position, BoardStateManager boardState)` - 配置検証
- `bool ValidateMove(Vector2Int from, Vector2Int to, BoardStateManager boardState)` - 移動検証
- `ValidationResult GetValidationResult(Character character, Vector2Int position, BoardStateManager boardState)` - 詳細検証結果取得

**位置取得:**
- `List<Vector2Int> GetValidPositions(Character character, BoardStateManager boardState)` - 有効位置取得
- `List<Vector2Int> GetInvalidPositions(Character character, BoardStateManager boardState)` - 無効位置取得
- `List<Vector2Int> GetRecommendedPositions(Character character, BoardStateManager boardState)` - 推奨位置取得

**検証ルール:**
- `void AddValidationRule(IPlacementRule rule)` - 検証ルール追加
- `void RemoveValidationRule(IPlacementRule rule)` - 検証ルール除去
- `List<IPlacementRule> GetActiveRules()` - 有効ルール取得

**エラー情報:**
- `string GetValidationError(Character character, Vector2Int position, BoardStateManager boardState)` - 検証エラー取得
- `List<ValidationIssue> GetAllValidationIssues(BoardStateManager boardState)` - 全検証問題取得

**詳細説明:**
- 配置ルールの検証に特化
- ルールベースの拡張可能な検証システム
- 詳細なエラー情報の提供
- キャラクター固有の配置制限対応

### PlacementOptimizer
配置の最適化のみを担当するクラス。

**publicメソッド:**

**最適化提案:**
- `PlacementSuggestion GetOptimalPlacement(List<Character> party, EnemyInfo enemyInfo, BoardStateManager boardState)` - 最適配置提案
- `Dictionary<Character, Vector2Int> OptimizeCurrentPlacement(BoardStateManager boardState, EnemyInfo enemyInfo)` - 現在配置最適化
- `PlacementRating RatePlacement(BoardStateManager boardState, EnemyInfo enemyInfo)` - 配置評価

**位置分析:**
- `float CalculatePositionScore(Character character, Vector2Int position, BattleContext context)` - 位置スコア計算
- `List<Vector2Int> GetOptimalPositions(Character character, BattleContext context)` - 最適位置取得
- `PositionAnalysis AnalyzePosition(Vector2Int position, BattleContext context)` - 位置分析

**戦略分析:**
- `void AnalyzeSkillRanges(List<Character> party, BoardStateManager boardState)` - スキル範囲分析
- `void AnalyzeEnemyThreats(EnemyInfo enemyInfo, BoardStateManager boardState)` - 敵脅威分析
- `SynergyOptimization OptimizeSynergies(List<Character> party, BoardStateManager boardState)` - シナジー最適化

**設定管理:**
- `void SetOptimizationStrategy(OptimizationStrategy strategy)` - 最適化戦略設定
- `OptimizationStrategy GetCurrentStrategy()` - 現在戦略取得
- `void SaveOptimizationPreferences(OptimizationPreferences preferences)` - 最適化設定保存

**詳細説明:**
- AI支援による配置最適化
- 複数の最適化戦略をサポート
- 敵情報に基づく動的最適化
- プレイヤーの設定を考慮した提案

## 新規サポートクラス

### ValidationResult
検証結果を表すクラス。

**publicメソッド:**
- `bool IsValid()` - 有効性判定
- `List<string> GetErrors()` - エラー一覧取得
- `List<string> GetWarnings()` - 警告一覧取得
- `ValidationSeverity GetSeverity()` - 深刻度取得
- `string GetSummary()` - 結果サマリー取得

### IPlacementRule
配置ルールのインターフェース。

**publicメソッド:**
- `bool Validate(Character character, Vector2Int position, BoardStateManager boardState)` - ルール検証
- `string GetRuleName()` - ルール名取得
- `string GetRuleDescription()` - ルール説明取得
- `RulePriority GetPriority()` - ルール優先度取得

### OptimizationStrategy (enum)
最適化戦略を表す列挙型。

**値:**
- `Defensive` - 防御重視
- `Offensive` - 攻撃重視
- `Balanced` - バランス重視
- `SynergyFocused` - シナジー重視
- `CounterEnemy` - 敵対策重視

### PositionAnalysis
位置分析結果を表すクラス。

**publicメソッド:**
- `float GetDefensiveScore()` - 防御スコア取得
- `float GetOffensiveScore()` - 攻撃スコア取得
- `float GetSynergyScore()` - シナジースコア取得
- `List<string> GetPositionAdvantages()` - 位置利点取得
- `List<string> GetPositionDisadvantages()` - 位置欠点取得

## 分割による利点

### 1. 単一責任の原則の遵守
- 各クラスが明確な責任を持つ
- 変更理由が単一化される
- コードの理解が容易

### 2. テスタビリティの向上
- 各機能を独立してテスト可能
- モックオブジェクトの作成が容易
- 単体テストの粒度が適切

### 3. 拡張性の向上
- 新しい検証ルールの追加が容易
- 最適化アルゴリズムの変更が容易
- 独立した機能改善が可能

### 4. 保守性の向上
- バグの影響範囲が限定される
- 機能追加時の副作用が少ない
- コードレビューの対象が明確

## 削除されるクラス
- `BoardManager`（機能10）

この機能は3つのクラス（`BoardStateManager`、`PlacementValidator`、`PlacementOptimizer`）に分割される。

## 残存するクラス（変更なし）
- `BoardSlot`（機能10）
- `BoardLayout`（機能10）
- `BoardPosition`（機能10）
- `BoardZone`（機能10）

これらは独立したデータクラスやヘルパークラスとして適切な責任分割が保たれているため変更不要。