# 新設計: キャラクターインベントリ統合版

## 概要
機能07（キャラクター合成）、機能08（キャラクター売却）、機能09（キャラクター経験値）を統合し、キャラクター操作を一元管理する。

## 統合クラス設計

### CharacterInventoryManager
キャラクターの全操作を統合管理するメインクラス。

**publicメソッド:**

**合成機能:**
- `Character FuseCharacters(List<Character> characters)` - キャラクター合成実行
- `bool CanFuseCharacters(List<Character> characters)` - 合成可能判定
- `List<Character> GetFusableCharacters(Character baseCharacter)` - 合成可能キャラクター取得
- `FusionPreview GetFusionPreview(List<Character> characters)` - 合成プレビュー取得
- `void CheckAutoFusion()` - 自動合成チェック

**売却機能:**
- `bool SellCharacter(Character character)` - キャラクター売却実行
- `bool SellCharacters(List<Character> characters)` - 複数キャラクター売却
- `int GetSellPrice(Character character)` - 売却価格取得
- `int GetTotalSellPrice(List<Character> characters)` - 複数キャラクター売却価格取得
- `SellPreview GetSellPreview(List<Character> characters)` - 売却プレビュー取得

**経験値化機能:**
- `bool FeedCharacterForExp(Character target, Character fodder)` - キャラクター経験値化
- `bool FeedCharactersForExp(Character target, List<Character> fodders)` - 複数キャラクター経験値化
- `int GetExpValue(Character character)` - キャラクターの経験値価値取得
- `ExpFeedPreview GetExpFeedPreview(Character target, List<Character> fodders)` - 経験値化プレビュー取得

**統合管理機能:**
- `List<Character> GetAllCharacters()` - 全キャラクター取得
- `List<Character> GetAvailableCharacters()` - 操作可能キャラクター取得（配置中除外）
- `bool CanOperateCharacter(Character character, OperationType operation)` - キャラクター操作可能判定
- `void ValidateCharacterOperation(Character character, OperationType operation)` - 操作検証
- `CharacterOperationResult ExecuteOperation(CharacterOperation operation)` - 統合操作実行

**詳細説明:**
- 旧CharacterFusionManager、CharacterSellManager、CharacterExpManagerの機能を統合
- キャラクター操作の競合状態を防ぐ排他制御
- 操作履歴の統合管理
- ゴールドシステムとの一元連携

**privateメソッド（内部実装）:**
- `bool ValidateCharacterAvailability(Character character)` - キャラクター利用可能性検証
- `void UpdateCharacterStats(Character character)` - キャラクターステータス更新
- `void NotifyCharacterChange(Character character, OperationType operation)` - キャラクター変更通知
- `void RecordOperation(CharacterOperation operation)` - 操作履歴記録

### CharacterOperation
キャラクター操作を表すクラス。

**publicメソッド:**
- `CharacterOperation(OperationType type, List<Character> targets, Character subject)` - コンストラクタ
- `OperationType GetOperationType()` - 操作タイプ取得
- `List<Character> GetTargetCharacters()` - 対象キャラクター取得
- `Character GetSubjectCharacter()` - 主体キャラクター取得（合成・経験値化の場合）
- `bool IsValid()` - 操作有効性判定
- `string GetOperationDescription()` - 操作説明取得

**詳細説明:**
- 全キャラクター操作を統一的に表現
- 操作の検証と実行を分離
- undo/redo機能のための操作記録

### OperationType (enum)
キャラクター操作タイプを表す列挙型。

**値:**
- `Fusion` - キャラクター合成
- `Sell` - キャラクター売却
- `ExpFeed` - 経験値化
- `LevelUp` - レベルアップ
- `Move` - 配置移動

### CharacterOperationResult
キャラクター操作結果を表すクラス。

**publicメソッド:**
- `bool IsSuccess()` - 操作成功判定
- `Character GetResultCharacter()` - 結果キャラクター取得
- `int GetGoldChange()` - ゴールド変化量取得
- `List<Character> GetAffectedCharacters()` - 影響を受けたキャラクター取得
- `string GetResultSummary()` - 結果サマリー取得
- `OperationError GetError()` - エラー情報取得（失敗時）

**詳細説明:**
- 操作結果の統一的な表現
- UI演出への情報提供
- エラーハンドリングの統一化

### CharacterAvailabilityChecker
キャラクターの利用可能性をチェックするクラス。

**publicメソッド:**
- `bool IsAvailableForOperation(Character character, OperationType operation)` - 操作可能判定
- `List<Character> FilterAvailableCharacters(List<Character> characters, OperationType operation)` - 利用可能キャラクターフィルタ
- `string GetUnavailabilityReason(Character character, OperationType operation)` - 利用不可理由取得
- `void RegisterUnavailableCharacter(Character character, string reason)` - 利用不可キャラクター登録

**詳細説明:**
- 配置中、戦闘中等の状態を考慮した利用可能性判定
- 操作競合の防止
- わかりやすいエラーメッセージの提供

## 統合による利点

### 1. 操作の一貫性
- 全キャラクター操作が統一インターフェース
- 操作制限の一元管理
- エラーハンドリングの統一化

### 2. データ整合性の保証
- キャラクター状態の排他制御
- 操作の原子性保証
- 状態変更の一元通知

### 3. 拡張性の向上
- 新しいキャラクター操作の追加が容易
- 操作履歴機能の統一的実装
- バッチ処理の効率化

## 削除されるクラス
- `CharacterFusionManager`（機能07）
- `CharacterSellManager`（機能08）
- `CharacterExpManager`（機能09）
- `SellPriceCalculator`（機能08）
- `ExpValueCalculator`（機能09）

これらの機能は`CharacterInventoryManager`に統合され、必要に応じてprivateメソッドとして実装される。

## 残存するクラス（変更なし）
- `FusionPreview`（機能07）
- `SellPreview`（機能08）
- `ExpFeedPreview`（機能09）
- `FusionResult`（機能07）
- `SellHistory`（機能08）
- `ExpFeedHistory`（機能09）

これらは独立したデータクラスとして適切な責任分割が保たれているため変更不要。ただし、統合されたマネージャーから呼び出されるように調整される。