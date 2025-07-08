# GatchaSpire プロジェクト設計概要

## プロジェクト概要
**GatchaSpire**は「一期一会のパーティ構築ローグライト」をコンセプトとしたUnityゲームプロジェクトです。

### ゲームコンセプト
- **完全リセットのローグライト**: 毎回のプレイ（ラン）は完全にリセットされ、永続的な強化要素はありません
- **オートチェスライクなパーティ構築**: ガチャで獲得したキャラクターを5x5の盤面に配置
- **資源管理のジレンマ**: 限られたゴールドを「ガチャを引く」か「ガチャを強化する」かの選択
- **知識と経験の蓄積**: プレイヤーの「知識」と「経験」のみが次の挑戦に活かされる

### ゲームサイクル
1. **ラン開始**: 初期キャラクターと初期ゴールドでスタート
2. **準備フェーズ**: ガチャ、パーティ編成、キャラクター整理
3. **戦闘フェーズ**: 完全オートバトル
4. **ランの進行**: 準備→戦闘を繰り返してステージをクリア
5. **ラン終了**: 最終ボス撃破またはゲームオーバーで全リセット

## 設計ファイル構成

### コアシステム（統合設計）
- `new_design_gacha_system.md` - ガチャシステム統合版（機能04+05）
- `new_design_character_inventory.md` - キャラクター管理統合版（機能07+08+09）
- `new_design_battle_manager.md` - バトルシステム統合版（機能11）

### 分割設計
- `new_design_board_system_split.md` - ボードシステム分割版（機能10を3分割）
- `new_design_synergy_system_split.md` - シナジーシステム分割版（機能13を3分割）

### システム管理
- `new_design_system_coordinator.md` - 統合システム管理とインターフェース

### 個別機能（変更なし）
- `feature_01_game_cycle.md` - ゲームサイクル管理
- `feature_02_data_reset.md` - データリセット機能
- `feature_03_gold_system.md` - ゴールドシステム
- `feature_06_character_basic.md` - キャラクター基本システム
- `feature_12_skill_system.md` - スキルシステム
- `feature_14_placement_strategy.md` - 配置戦略システム
- `feature_15_ceiling_system.md` - 天井システム
- `feature_16_pickup_gacha.md` - ピックアップガチャシステム
- `feature_17_special_items.md` - 特殊効果アイテムシステム

## システム依存関係

### 初期化順序（SystemPriority）
1. **Critical**: GameSystemCoordinator
2. **High**: GoldSystem, CharacterDatabase, GachaSystemManager
3. **Medium**: CharacterInventoryManager, BoardStateManager, SynergyCalculator
4. **Low**: UI系システム、エフェクト系
5. **Lowest**: デバッグ系、統計系

### 主要な依存関係
- **GachaSystemManager** → GoldSystem
- **CharacterInventoryManager** → CharacterDatabase, GoldSystem
- **BattleManager** → SkillSystem, SynergyEffectApplier
- **SynergyEffectApplier** → SynergyCalculator

## 削除・統合されたクラス

### 統合されたクラス群
**ガチャ関連（→ GachaSystemManager）:**
- GachaManager, GachaUpgradeManager, GachaPool, GachaRateCalculator

**キャラクター管理関連（→ CharacterInventoryManager）:**
- CharacterFusionManager, CharacterSellManager, CharacterExpManager

**バトル関連（→ BattleManager）:**
- AutoBattleManager, BattleSimulator, BattleState

### 分割されたクラス群
**BoardManager（機能10）→ 3つに分割:**
- BoardStateManager（状態管理）
- PlacementValidator（検証）
- PlacementOptimizer（最適化）

**SynergyManager（機能13）→ 3つに分割:**
- SynergyCalculator（計算）
- SynergyEffectApplier（効果適用）
- SynergyVisualizer（表示）

## 未実装機能（機能18-31）

### UI/UXシステム
- **機能20**: GachaUI - ガチャインターフェース
- **機能21**: PartyFormationUI - パーティ編成インターフェース
- **機能22**: CharacterManagementUI - キャラクター管理インターフェース
- **機能23**: BattleUI - 戦闘インターフェース

### プログレッションシステム
- **機能18**: UnlockManager - 新キャラクター解放システム
- **機能19**: DifficultyManager - 高難易度モード

### データ管理システム
- **機能24**: CharacterDatabase - キャラクターデータベース
- **機能25**: BalanceManager - ゲームバランス管理
- **機能26**: SaveDataManager - セーブデータ管理

### オーディオ/ビジュアルシステム
- **機能27**: AudioManager - サウンドシステム
- **機能28**: EffectManager - エフェクトシステム
- **機能29**: AnimationManager - アニメーションシステム

### 設定/デバッグシステム
- **機能30**: SettingsManager - ゲーム設定
- **機能31**: DebugManager - デバッグ機能

## 設計原則

### 採用パターン
1. **Singletonパターン**: 唯一性が必要なシステム
2. **Observerパターン**: イベント通知システム
3. **Strategyパターン**: 戦略的な処理の切り替え
4. **Factoryパターン**: オブジェクトの生成処理

### 共通インターフェース
- **IGameSystem**: 全システム共通の基本操作
- **IResettableSystem**: リセット可能システム
- **IPersistentSystem**: 永続化システム

### Unity固有の考慮事項
- **MonoBehaviour継承**: 必要な場合のみ使用
- **ScriptableObject**: データ管理とバランス調整
- **SerializeField**: publicフィールドの代替
- **DontDestroyOnLoad**: シーン間でのオブジェクト永続化

## 実装の優先順位

### Phase 1: コアシステム
1. GameSystemCoordinator
2. GachaSystemManager
3. CharacterInventoryManager
4. 基本UI

### Phase 2: ゲームロジック
1. BattleManager
2. BoardStateManager
3. SynergyCalculator
4. SkillSystem

### Phase 3: 拡張機能
1. 配置最適化
2. 天井システム
3. 特殊アイテム
4. エフェクト・アニメーション

このドキュメントにより、新しいインスタンスでもプロジェクト全体を完全に理解できます。