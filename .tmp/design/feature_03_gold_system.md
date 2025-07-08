# 機能03: ゴールドシステム

## 概要
ゴールドの取得/消費、ガチャコストの管理、戦闘勝利時の報酬計算を担当するリソース管理システム。

## 実装クラス設計

### GoldManager
ゴールドの管理を行うシステムクラス。

**publicメソッド:**
- `void AddGold(int amount)` - ゴールドの追加
- `bool SpendGold(int amount)` - ゴールドの消費（成功/失敗を返す）
- `int GetCurrentGold()` - 現在のゴールド数取得
- `bool HasEnoughGold(int amount)` - 指定額の所持判定
- `void ResetGold()` - ゴールドのリセット
- `void SetInitialGold(int amount)` - 初期ゴールドの設定

**詳細説明:**
- ゴールドの増減に対してイベントを発火
- 負の値にならないよう制御
- デバッグ用のゴールド操作機能も含む
- UIに対してゴールド変更の通知を送信

### GoldCalculator
ゴールド計算を担当する静的クラス。

**publicメソッド:**
- `int CalculateBattleReward(int stageNumber, bool isPerfectWin)` - 戦闘報酬計算
- `int CalculateGachaCost(int gachaLevel)` - ガチャコストの計算
- `int CalculateUpgradeCost(int currentLevel)` - アップグレードコストの計算
- `int CalculateSellPrice(Character character)` - キャラクター売却価格の計算

**詳細説明:**
- ゲームバランスに関わる計算式を集約
- ScriptableObjectからバランス設定を読み込み
- ステージ進行に応じた動的な報酬計算
- デバッグ用の計算過程出力機能

### GoldTransaction
ゴールドの取引履歴を管理するクラス。

**publicメソッド:**
- `void RecordTransaction(GoldTransactionType type, int amount, string description)` - 取引記録
- `List<GoldTransactionRecord> GetTransactionHistory()` - 取引履歴取得
- `int GetTotalEarned()` - 総獲得ゴールド取得
- `int GetTotalSpent()` - 総消費ゴールド取得
- `void ClearHistory()` - 履歴のクリア

**詳細説明:**
- デバッグとバランス調整のための取引記録
- 取引タイプ（戦闘報酬、ガチャ、売却等）別の分類
- ランごとの詳細な収支分析が可能

### GoldTransactionType (enum)
ゴールド取引の種類を表す列挙型。

**値:**
- `BattleReward` - 戦闘報酬
- `GachaPull` - ガチャ実行
- `GachaUpgrade` - ガチャアップグレード
- `CharacterSell` - キャラクター売却
- `InitialGold` - 初期ゴールド
- `Debug` - デバッグ用操作

### GoldTransactionRecord
個別の取引記録を表すデータクラス。

**publicメソッド:**
- `GoldTransactionRecord(GoldTransactionType type, int amount, string description)` - コンストラクタ
- `DateTime GetTimestamp()` - 取引時刻取得
- `string GetFormattedRecord()` - フォーマットされた記録文字列取得

**詳細説明:**
- 取引の詳細情報を保持
- 時刻、金額、種類、説明を記録
- デバッグ表示用のフォーマット機能