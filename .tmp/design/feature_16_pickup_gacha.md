# 機能16: ピックアップガチャシステム

## 概要
限定ガチャチケットの管理、エリート敵/ボス撃破の報酬、選択式ガチャの実装を担当するシステム。

## 実装クラス設計

### PickupGachaManager
ピックアップガチャの管理を行うクラス。

**publicメソッド:**
- `void OpenPickupGacha(List<Character> candidates)` - ピックアップガチャ開始
- `Character SelectCharacter(int index)` - キャラクター選択
- `bool HasPickupTicket()` - ピックアップチケット所持判定
- `void ConsumePickupTicket()` - ピックアップチケット消費
- `List<Character> GetPickupCandidates()` - ピックアップ候補取得

**詳細説明:**
- 特別ガチャの実行制御
- 選択式インターフェース
- チケットの管理
- 候補キャラクターの提示

### PickupTicketManager
ピックアップチケットの管理を行うクラス。

**publicメソッド:**
- `void AddTicket(PickupTicketType type)` - チケット追加
- `bool ConsumeTicket(PickupTicketType type)` - チケット消費
- `int GetTicketCount(PickupTicketType type)` - チケット数取得
- `List<PickupTicketType> GetAvailableTickets()` - 利用可能チケット取得
- `void ResetTickets()` - チケットリセット

**詳細説明:**
- チケットの種類別管理
- 使用制限の制御
- 報酬システムとの連携
- ランごとのリセット

### PickupRewardSystem
ピックアップ報酬の管理を行うクラス。

**publicメソッド:**
- `void GrantEliteReward(EnemyType enemyType)` - エリート報酬付与
- `void GrantBossReward(BossType bossType)` - ボス報酬付与
- `PickupReward CalculateReward(EnemyInfo enemyInfo)` - 報酬計算
- `bool IsRewardEligible(EnemyInfo enemyInfo)` - 報酬対象判定
- `void ProcessRewardGrant(PickupReward reward)` - 報酬付与処理

**詳細説明:**
- 強敵撃破時の報酬付与
- 報酬の種類と数量決定
- 報酬付与条件の判定
- 報酬演出との連携