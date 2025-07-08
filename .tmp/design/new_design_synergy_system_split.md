# 新設計: シナジーシステム分割版

## 概要
機能13（シナジーシステム）のSynergyManagerを計算ロジック、効果適用、表示ロジックに分割し、それぞれの責任を明確化する。

## 分割クラス設計

### SynergyCalculator
シナジーの計算のみを担当するクラス。

**publicメソッド:**

**シナジー計算:**
- `List<SynergyEffect> CalculateAllSynergies(List<Character> party)` - 全シナジー計算
- `List<SynergyEffect> CalculateRaceSynergies(List<Character> party)` - 種族シナジー計算
- `List<SynergyEffect> CalculateClassSynergies(List<Character> party)` - クラスシナジー計算
- `Dictionary<SynergyType, int> CountSynergyUnits(List<Character> party)` - シナジー単位数計算

**シナジーレベル:**
- `int GetSynergyLevel(SynergyType type, int unitCount)` - シナジーレベル取得
- `bool IsSynergyActive(SynergyType type, int unitCount)` - シナジー有効判定
- `int GetNextThreshold(SynergyType type, int currentCount)` - 次の閾値取得
- `float GetSynergyProgress(SynergyType type, int currentCount)` - シナジー進捗取得

**詳細計算:**
- `StatModifier CalculateSynergyBonus(SynergyType type, int level)` - シナジーボーナス計算
- `List<SynergyType> GetActiveSynergyTypes(List<Character> party)` - 有効シナジータイプ取得
- `Dictionary<Character, List<SynergyType>> GetCharacterSynergies(List<Character> party)` - キャラクター別シナジー取得

**詳細説明:**
- 純粋な計算処理に特化
- パーティ構成の分析
- シナジー発動条件の判定
- 数値計算のみを担当

### SynergyEffectApplier
シナジー効果の適用のみを担当するクラス。

**publicメソッド:**

**効果適用:**
- `void ApplySynergyEffects(List<Character> party, List<SynergyEffect> effects)` - シナジー効果適用
- `void RemoveSynergyEffects(List<Character> party)` - シナジー効果除去
- `void UpdateSynergyEffects(List<Character> party, List<SynergyEffect> newEffects)` - シナジー効果更新

**個別効果管理:**
- `void ApplyEffectToCharacter(Character character, SynergyEffect effect)` - キャラクターへの効果適用
- `void RemoveEffectFromCharacter(Character character, SynergyEffect effect)` - キャラクターからの効果除去
- `List<SynergyEffect> GetActiveEffects(Character character)` - 有効効果取得

**効果状態管理:**
- `bool IsEffectActive(Character character, SynergyType type)` - 効果有効判定
- `float GetEffectStrength(Character character, SynergyType type)` - 効果強度取得
- `void ValidateEffectApplication(List<Character> party)` - 効果適用検証

**バッチ処理:**
- `void ApplyAllEffects(Dictionary<Character, List<SynergyEffect>> characterEffects)` - 全効果一括適用
- `void RefreshAllEffects(List<Character> party, List<SynergyEffect> effects)` - 全効果再適用

**詳細説明:**
- キャラクターステータスへの効果適用
- 効果の重複管理
- 効果の有効期間管理
- バトルシステムとの連携

### SynergyVisualizer
シナジーの可視化のみを担当するクラス。

**publicメソッド:**

**表示更新:**
- `void UpdateSynergyDisplay(List<SynergyEffect> effects)` - シナジー表示更新
- `void RefreshAllDisplays()` - 全表示リフレッシュ
- `void ClearSynergyDisplay()` - シナジー表示クリア

**ハイライト機能:**
- `void HighlightSynergyCharacters(SynergyType type)` - シナジーキャラクターハイライト
- `void ClearHighlights()` - ハイライトクリア
- `void HighlightPotentialSynergies(Character newCharacter, List<Character> currentParty)` - 潜在シナジーハイライト

**プレビュー機能:**
- `void ShowSynergyPreview(List<Character> party)` - シナジープレビュー表示
- `void HideSynergyPreview()` - シナジープレビュー非表示
- `void ShowCharacterAddPreview(Character character, List<Character> currentParty)` - キャラクター追加プレビュー

**アニメーション:**
- `void AnimateSynergyActivation(SynergyType type)` - シナジー発動アニメーション
- `void AnimateSynergyLevelUp(SynergyType type, int newLevel)` - シナジーレベルアップアニメーション
- `void AnimateEffectApplication(Character character, SynergyEffect effect)` - 効果適用アニメーション

**情報表示:**
- `void DisplaySynergyTooltip(SynergyType type, Vector2 position)` - シナジーツールチップ表示
- `void HideTooltip()` - ツールチップ非表示
- `string GetSynergyDisplayText(SynergyType type, int level)` - シナジー表示テキスト取得

**詳細説明:**
- UI表示の制御
- 視覚効果とアニメーション
- プレイヤーへの情報提供
- インタラクティブな表示機能

## 統合管理クラス

### SynergySystemManager
分割されたシナジーシステムを統合管理するクラス。

**publicメソッド:**

**統合操作:**
- `void UpdateSynergySystem(List<Character> party)` - シナジーシステム更新
- `void InitializeSynergySystem()` - シナジーシステム初期化
- `void ResetSynergySystem()` - シナジーシステムリセット

**情報取得:**
- `List<SynergyEffect> GetActiveSynergies()` - 有効シナジー取得
- `SynergyInfo GetSynergyInfo(SynergyType type)` - シナジー情報取得
- `SynergySystemStatus GetSystemStatus()` - システム状態取得

**設定管理:**
- `void SetVisualizationEnabled(bool enabled)` - 可視化有効設定
- `void SetAutoUpdate(bool enabled)` - 自動更新設定

**詳細説明:**
- 3つの分割クラスを統合調整
- 外部システムからの単一インターフェース
- システム全体の設定管理

## 新規サポートクラス

### SynergySystemStatus
シナジーシステムの状態を表すクラス。

**publicメソッド:**
- `int GetActiveSynergyCount()` - 有効シナジー数取得
- `List<SynergyType> GetActiveSynergyTypes()` - 有効シナジータイプ取得
- `float GetSystemEfficiency()` - システム効率取得
- `string GetStatusSummary()` - 状態サマリー取得

## 分割による利点

### 1. 関心の分離
- 計算ロジックと表示ロジックの分離
- 各クラスが単一の責任を持つ
- コードの理解が容易

### 2. テスタビリティの向上
- 計算ロジックを表示なしでテスト可能
- 表示機能を計算なしでテスト可能
- モックオブジェクトの作成が容易

### 3. 拡張性の向上
- 新しい表示方法の追加が容易
- 計算アルゴリズムの変更が表示に影響しない
- 効果適用ロジックの独立した改善

### 4. パフォーマンスの最適化
- 計算結果のキャッシュが容易
- 表示更新の最適化が独立して可能
- 必要な部分のみの更新が可能

## 削除されるクラス
- `SynergyManager`（機能13）

この機能は3つのクラス（`SynergyCalculator`、`SynergyEffectApplier`、`SynergyVisualizer`）と統合管理クラス（`SynergySystemManager`）に分割される。

## 残存するクラス（変更なし）
- `SynergyEffect`（機能13）
- `SynergyData`（機能13）
- `SynergyType`（機能13）
- `SynergyInfo`（機能13）

これらは独立したデータクラスとして適切な責任分割が保たれているため変更不要。