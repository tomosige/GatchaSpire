# 機能14: 配置戦略システム

## 概要
前衛/中衛/後衛の役割分担、スキル効果範囲の最適化、敵の攻撃パターンに応じた配置を担当するシステム。

## 実装クラス設計

### PlacementStrategyManager
配置戦略の管理を行うクラス。

**publicメソッド:**
- `PlacementSuggestion GetOptimalPlacement(List<Character> party, EnemyInfo enemyInfo)` - 最適配置提案
- `void AnalyzeCurrentPlacement(List<Character> party)` - 現在配置分析
- `List<PlacementIssue> GetPlacementIssues(List<Character> party)` - 配置問題取得
- `PlacementRating RatePlacement(List<Character> party, EnemyInfo enemyInfo)` - 配置評価
- `void SavePlacementStrategy(string strategyName, PlacementPattern pattern)` - 配置戦略保存

**詳細説明:**
- AI支援による配置提案
- 現在配置の問題点分析
- 敵情報に基づく最適化
- 戦略パターンの管理

### RoleAssignmentSystem
キャラクターの役割分担を管理するクラス。

**publicメソッド:**
- `CharacterRole AssignRole(Character character, List<Character> party)` - 役割割り当て
- `List<Character> GetCharactersByRole(CharacterRole role, List<Character> party)` - 役割別キャラクター取得
- `bool IsRoleBalanced(List<Character> party)` - 役割バランス判定
- `RoleRequirement GetRoleRequirements(EnemyInfo enemyInfo)` - 役割要件取得
- `void OptimizeRoleDistribution(List<Character> party)` - 役割配分最適化

**詳細説明:**
- 自動役割割り当て
- パーティバランスの分析
- 敵に応じた役割要件
- 配置の最適化支援

### PositionOptimizer
配置位置の最適化を行うクラス。

**publicメソッド:**
- `Dictionary<Character, Vector2Int> OptimizePositions(List<Character> party, EnemyInfo enemyInfo)` - 位置最適化
- `float CalculatePositionScore(Character character, Vector2Int position, BattleContext context)` - 位置スコア計算
- `List<Vector2Int> GetOptimalPositions(Character character, BattleContext context)` - 最適位置取得
- `void ConsiderSkillRanges(List<Character> party)` - スキル範囲考慮
- `void ConsiderEnemyThreats(EnemyInfo enemyInfo)` - 敵脅威考慮

**詳細説明:**
- 数値的な配置最適化
- 位置価値の計算
- スキル範囲の考慮
- 敵攻撃パターンの分析

### PlacementSuggestion
配置提案を表すクラス。

**publicメソッド:**
- `Dictionary<Character, Vector2Int> GetSuggestedPositions()` - 提案位置取得
- `List<string> GetReasons()` - 提案理由取得
- `PlacementRating GetRating()` - 評価取得
- `void ApplySuggestion(BoardManager boardManager)` - 提案適用
- `string GetSuggestionSummary()` - 提案サマリー取得

**詳細説明:**
- AI配置提案の管理
- 提案理由の説明
- 評価システムとの連携
- 自動適用機能

### PlacementPattern
配置パターンを表すクラス。

**publicメソッド:**
- `void SavePattern(List<Character> party, string patternName)` - パターン保存
- `void LoadPattern(List<Character> party, BoardManager boardManager)` - パターン読み込み
- `bool IsPatternApplicable(List<Character> party)` - パターン適用可能判定
- `float GetPatternEffectiveness(EnemyInfo enemyInfo)` - パターン効果度取得
- `string GetPatternDescription()` - パターン説明取得

**詳細説明:**
- 配置パターンの記録
- 再利用可能な配置
- 適用可能性の判定
- 効果度の評価

### CharacterRole (enum)
キャラクターの役割を表す列挙型。

**値:**
- `Tank` - タンク（前衛守備）
- `Damage` - ダメージディーラー（攻撃）
- `Support` - サポート（支援）
- `Healer` - ヒーラー（回復）
- `Utility` - ユーティリティ（特殊）

### PlacementIssue
配置問題を表すクラス。

**publicメソッド:**
- `PlacementIssueType GetIssueType()` - 問題タイプ取得
- `List<Character> GetAffectedCharacters()` - 影響キャラクター取得
- `string GetIssueDescription()` - 問題説明取得
- `List<string> GetSolutions()` - 解決案取得
- `IssueSeverity GetSeverity()` - 深刻度取得

**詳細説明:**
- 配置問題の分類
- 影響範囲の特定
- 解決方法の提示
- 優先度の管理