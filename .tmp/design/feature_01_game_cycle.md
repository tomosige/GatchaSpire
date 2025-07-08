# 機能01: ゲームサイクル管理

## 概要
ラン開始/終了の管理、準備フェーズと戦闘フェーズの切り替え、タイトル画面への完全リセットを担当するコアシステム。

## 実装クラス設計

### GameCycleManager
ゲーム全体のサイクル管理を行うSingletonクラス。

**publicメソッド:**
- `void StartNewRun()` - 新しいランを開始
- `void EndCurrentRun(bool isCleared)` - 現在のランを終了（クリア/ゲームオーバー）
- `void TransitionToPreparationPhase()` - 準備フェーズへの遷移
- `void TransitionToBattlePhase()` - 戦闘フェーズへの遷移
- `void ReturnToTitle()` - タイトル画面への完全リセット
- `GamePhase GetCurrentPhase()` - 現在のフェーズを取得
- `int GetCurrentStage()` - 現在のステージ数を取得
- `void AdvanceToNextStage()` - 次のステージへ進行

**詳細説明:**
- UnityのSceneManager使用してシーン遷移を管理
- 各フェーズの状態をenumで管理（Title, Preparation, Battle, GameOver）
- イベントシステムを使用してフェーズ変更を各システムに通知
- PlayerPrefsを使用しない完全リセット機能
- MonoBehaviourを継承し、DontDestroyOnLoadで永続化

### GamePhase (enum)
ゲームの現在フェーズを表す列挙型。

**値:**
- `Title` - タイトル画面
- `Preparation` - 準備フェーズ
- `Battle` - 戦闘フェーズ
- `GameOver` - ゲームオーバー画面

### RunData
現在のランの情報を保持するデータクラス。

**publicメソッド:**
- `void Initialize()` - ランデータの初期化
- `void Reset()` - 全データのリセット
- `void SetStageCleared(int stageNumber)` - ステージクリア状態の設定
- `bool IsStageCleared(int stageNumber)` - ステージクリア状態の確認

**詳細説明:**
- 現在のステージ数、クリア状況、経過時間等を管理
- ScriptableObjectとして設計し、Inspector上での確認を可能にする
- ランの統計情報（戦闘数、ガチャ回数等）も記録