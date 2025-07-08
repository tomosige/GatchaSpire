# 新設計: ガチャシステム統合版

## 概要
機能04（ガチャシステム）と機能05（ガチャ強化システム）を統合し、ガチャ関連の全機能を一元管理する。

## 統合クラス設計

### GachaSystemManager
ガチャシステム全体を統合管理するメインクラス。

**publicメソッド:**

**基本ガチャ機能:**
- `List<Character> PullGacha(int pullCount = 1)` - ガチャ実行
- `bool CanPullGacha()` - ガチャ実行可能判定
- `int GetGachaCost()` - 現在のガチャコスト取得
- `GachaResult GetLastPullResult()` - 最後のガチャ結果取得

**ガチャアップグレード機能:**
- `bool UpgradeGacha()` - ガチャレベルアップ実行
- `bool CanUpgradeGacha()` - アップグレード可能判定
- `int GetUpgradeCost()` - アップグレードコスト取得
- `int GetCurrentLevel()` - 現在のガチャレベル取得
- `GachaUpgradePreview GetUpgradePreview()` - アップグレードプレビュー取得

**統合管理機能:**
- `void ResetGachaSystem()` - ガチャシステム全体のリセット
- `GachaSystemInfo GetSystemInfo()` - システム情報取得
- `void InitializeGachaSystem(GachaSystemData data)` - システム初期化

**詳細説明:**
- 旧GachaManager、GachaUpgradeManager、GachaPoolの機能を統合
- 内部でGachaRateCalculatorの計算処理を private メソッドとして実装
- ゴールドシステムとの連携を一元化
- 天井システムとの統合インターフェース

**privateメソッド（内部実装）:**
- `float CalculateRarityRate(CharacterRarity rarity)` - レアリティ排出率計算
- `Character SelectRandomCharacter()` - ランダムキャラクター選択
- `void UpdateGachaEffects()` - ガチャ効果更新
- `void ValidateGachaOperation()` - ガチャ操作検証

### GachaSystemData
ガチャシステムの統合設定データを管理するScriptableObject。

**publicメソッド:**
- `float GetBaseRarityRate(CharacterRarity rarity)` - 基本レアリティ排出率取得
- `int GetUpgradeCost(int currentLevel)` - レベル別アップグレードコスト取得
- `float GetLevelRarityBonus(int level, CharacterRarity rarity)` - レベル別レアリティボーナス取得
- `int GetBaseCost()` - 基本ガチャコスト取得
- `float GetCostReduction(int level)` - レベル別コスト削減率取得
- `int GetSimultaneousPullCount(int level)` - レベル別同時排出数取得

**詳細説明:**
- 旧GachaData、GachaUpgradeDataを統合
- 一つのScriptableObjectでガチャ関連の全設定を管理
- バランス調整の一元化

### GachaSystemInfo
ガチャシステムの現在状態を表すデータクラス。

**publicメソッド:**
- `int GetCurrentLevel()` - 現在レベル取得
- `float GetCurrentRarityRate(CharacterRarity rarity)` - 現在のレアリティ排出率取得
- `int GetCurrentCost()` - 現在のガチャコスト取得
- `int GetCurrentSimultaneousPulls()` - 現在の同時排出数取得
- `string GetSystemStatusSummary()` - システム状態サマリー取得

**詳細説明:**
- ガチャシステムの現在状態を一括取得
- UI表示用の統合情報
- デバッグ用の状態確認

## 統合による利点

### 1. シンプルな外部インターフェース
- ガチャ関連操作が単一クラスで完結
- 呼び出し側のコード簡素化
- 依存関係の削減

### 2. 内部処理の最適化
- 重複処理の排除
- データ共有の効率化
- 計算結果のキャッシュ最適化

### 3. 保守性の向上
- 関連機能の集約による理解しやすさ
- バグ修正時の影響範囲の明確化
- テストケースの統合

## 削除されるクラス
- `GachaManager`（機能04）
- `GachaUpgradeManager`（機能05）
- `GachaPool`（機能04）
- `GachaRateCalculator`（機能04）
- `GachaUpgradeCalculator`（機能05）

これらの機能は`GachaSystemManager`に統合され、必要に応じてprivateメソッドとして実装される。

## 残存するクラス（変更なし）
- `GachaResult`（機能04）
- `GachaHistory`（機能04）
- `GachaUpgradePreview`（機能05）
- `GachaUpgradeEffects`（機能05）

これらは独立したデータクラスとして適切な責任分割が保たれているため変更不要。