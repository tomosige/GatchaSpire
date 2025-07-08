# 機能15: 天井システム

## 概要
ガチャ回数のカウント、確定高レアリティ枠の管理、ランごとの天井リセットを担当するシステム。

## 実装クラス設計

### CeilingManager
天井システムの管理を行うクラス。

**publicメソッド:**
- `void IncrementPullCount()` - ガチャ回数増加
- `bool CheckCeilingReached()` - 天井到達判定
- `Character TriggerCeilingPull()` - 天井ガチャ実行
- `int GetCurrentPullCount()` - 現在ガチャ回数取得
- `int GetRemainingPullsToceiling()` - 天井までの残り回数取得
- `void ResetCeiling()` - 天井リセット

**詳細説明:**
- ガチャ回数の追跡
- 天井到達の自動判定
- 確定高レアリティの排出
- ランごとの完全リセット

### CeilingData
天井システムの設定データを管理するScriptableObject。

**publicメソッド:**
- `int GetCeilingThreshold()` - 天井閾値取得
- `List<CharacterRarity> GetCeilingRarities()` - 天井レアリティ取得
- `float GetCeilingRarityWeight(CharacterRarity rarity)` - 天井レアリティ重み取得
- `bool IsCeilingEnabled()` - 天井システム有効判定
- `string GetCeilingDescription()` - 天井説明取得

**詳細説明:**
- 天井発動条件の設定
- 確定レアリティの定義
- 重み付け抽選の設定
- バランス調整用パラメータ

### CeilingPullExecutor
天井ガチャの実行を担当するクラス。

**publicメソッド:**
- `Character ExecuteCeilingPull()` - 天井ガチャ実行
- `List<Character> GetCeilingCandidates()` - 天井候補取得
- `Character SelectCeilingCharacter(List<Character> candidates)` - 天井キャラクター選択
- `void ProcessCeilingEffects(Character character)` - 天井効果処理
- `CeilingPullResult GetLastCeilingResult()` - 最後の天井結果取得

**詳細説明:**
- 天井時の特別抽選
- 高レアリティ確定処理
- 天井効果の適用
- 結果の記録

### CeilingTracker
天井進捗の追跡を行うクラス。

**publicメソッド:**
- `void UpdateProgress(int pullCount)` - 進捗更新
- `float GetProgressPercentage()` - 進捗率取得
- `void SaveProgress()` - 進捗保存
- `void LoadProgress()` - 進捗読み込み
- `CeilingProgress GetCurrentProgress()` - 現在進捗取得

**詳細説明:**
- 進捗の可視化
- ランごとの進捗管理
- 進捗の永続化
- UI表示用の情報提供

### CeilingHistory
天井履歴を管理するクラス。

**publicメソッド:**
- `void RecordCeilingPull(Character character, int pullCount)` - 天井記録
- `List<CeilingRecord> GetCeilingHistory()` - 天井履歴取得
- `int GetTotalCeilingPulls()` - 総天井回数取得
- `Character GetLastCeilingCharacter()` - 最後の天井キャラクター取得
- `void ClearHistory()` - 履歴クリア

**詳細説明:**
- 天井発動履歴の記録
- 統計情報の生成
- 天井パターンの分析
- デバッグ用の詳細記録

### CeilingProgress
天井進捗を表すデータクラス。

**publicメソッド:**
- `int GetCurrentCount()` - 現在カウント取得
- `int GetTargetCount()` - 目標カウント取得
- `float GetProgressRatio()` - 進捗比率取得
- `bool IsNearCeiling()` - 天井接近判定
- `string GetProgressText()` - 進捗テキスト取得

**詳細説明:**
- 進捗状況の管理
- 天井接近の通知
- UI表示用の情報
- 進捗の可視化支援

### CeilingRecord
天井記録を表すデータクラス。

**publicメソッド:**
- `Character GetCharacter()` - キャラクター取得
- `int GetPullCount()` - ガチャ回数取得
- `DateTime GetTimestamp()` - 発動時刻取得
- `string GetRecordDescription()` - 記録説明取得
- `bool IsSpecialCeiling()` - 特別天井判定

**詳細説明:**
- 個別天井記録の管理
- 発動時の詳細情報
- 特別天井の識別
- 履歴分析用データ