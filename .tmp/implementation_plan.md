# GatchaSpire 実装計画

## 計画概要

### 実装方針
1. **段階的開発**: 小さな単位で実装→テスト→統合を繰り返す
2. **テスト駆動**: 各段階で動作確認可能なテストシーンを作成
3. **依存関係重視**: システム間の依存を考慮した実装順序
4. **手戻り最小化**: インターフェース先行で結合部を明確化

### 全体スケジュール（6段階構成）
- **Phase 0**: 基盤セットアップ（1-2日）
- **Phase 1**: コアシステム（3-5日）
- **Phase 2**: ゲームロジック（5-7日）
- **Phase 3**: UI統合（3-4日）
- **Phase 4**: 拡張機能（4-6日）
- **Phase 5**: 最終統合（2-3日）

---

## Phase 0: 基盤セットアップ

### 目標
プロジェクト基盤とインターフェースの確立

### 実装内容

#### Step 0.1: プロジェクト構造セットアップ
```
Assets/
├── Scripts/
│   ├── Core/                 # コアシステム
│   │   ├── Interfaces/       # 共通インターフェース
│   │   ├── Systems/          # システム管理
│   │   └── Data/             # データクラス
│   ├── Gameplay/             # ゲームロジック
│   │   ├── Characters/       # キャラクター関連
│   │   ├── Battle/           # 戦闘関連
│   │   ├── Gacha/           # ガチャ関連
│   │   └── Board/           # 盤面関連
│   ├── UI/                   # UIシステム
│   └── Utils/                # ユーティリティ
├── Data/                     # ScriptableObject
├── Scenes/                   # テストシーン
└── Tests/                    # テストコード
```

#### Step 0.2: 共通インターフェース実装
**ファイル**: `Scripts/Core/Interfaces/`
- `IGameSystem.cs` - システム基本インターフェース
- `IResettableSystem.cs` - リセット可能システム
- `IPersistentSystem.cs` - 永続化システム

**テスト**: インターフェースコンパイル確認

#### Step 0.3: システム管理基盤
**ファイル**: `Scripts/Core/Systems/GameSystemCoordinator.cs`
- 基本的なシステム登録・初期化機能
- 依存関係管理の基礎実装

**テスト**: 空のシステムの登録・初期化確認

---

## Phase 1: コアシステム

### 目標
基本的なリソース管理システムの構築

### 実装内容

#### Step 1.1: ゴールドシステム
**ファイル**: `Scripts/Gameplay/Gold/`
- `GoldManager.cs` - ゴールド管理
- `GoldCalculator.cs` - 計算ロジック
- `GoldTransaction.cs` - 取引履歴

**実装順序**:
1. GoldManagerの基本機能（Add/Spend/Get）
2. イベント通知システム
3. GoldCalculatorの計算ロジック
4. GoldTransactionの履歴機能

**テストシーン**: `GoldSystemTest.scene`
- ゴールド増減のテスト
- 不正な操作の防止確認
- イベント通知の確認

#### Step 1.2: キャラクター基本システム
**ファイル**: `Scripts/Gameplay/Characters/`
- `Character.cs` - キャラクターインスタンス
- `CharacterData.cs` - キャラクター設定データ（ScriptableObject）
- `CharacterDatabase.cs` - データベース
- `CharacterStats.cs` - ステータス構造体

**実装順序**:
1. CharacterDataとCharacterStatsの基本構造
2. Characterクラスの初期化とステータス管理
3. CharacterDatabaseの検索・フィルタ機能
4. レベルアップとステータス計算

**テストシーン**: `CharacterSystemTest.scene`
- キャラクター生成と初期化
- ステータス計算の確認
- データベース検索の確認

#### Step 1.3: ガチャシステム統合版
**ファイル**: `Scripts/Gameplay/Gacha/`
- `GachaSystemManager.cs` - 統合ガチャ管理
- `GachaSystemData.cs` - 設定データ（ScriptableObject）
- `GachaResult.cs` - 結果データ
- `GachaHistory.cs` - 履歴管理

**実装順序**:
1. 基本ガチャ機能（PullGacha）
2. ガチャアップグレード機能
3. レアリティ排出率計算
4. ゴールドシステムとの連携

**テストシーン**: `GachaSystemTest.scene`
- 基本ガチャの動作確認
- アップグレード効果の確認
- ゴールド消費の確認

---

## Phase 2: ゲームロジック

### 目標
キャラクター操作と戦闘システムの構築

#### Step 2.1: キャラクターインベントリ統合版
**ファイル**: `Scripts/Gameplay/Characters/Inventory/`
- `CharacterInventoryManager.cs` - 統合インベントリ管理
- `CharacterOperation.cs` - 操作データクラス
- `CharacterOperationResult.cs` - 操作結果
- `CharacterAvailabilityChecker.cs` - 利用可能性チェック

**実装順序**:
1. 基本的なキャラクター管理（追加・除去）
2. 売却機能とゴールドシステム連携
3. 合成機能の実装
4. 経験値化機能の実装
5. 統合操作インターフェース

**テストシーン**: `CharacterInventoryTest.scene`
- 各操作の動作確認
- 競合状態の防止確認
- ゴールドとの連携確認

#### Step 2.2: ボードシステム分割版
**ファイル**: `Scripts/Gameplay/Board/`
- `BoardStateManager.cs` - 盤面状態管理
- `PlacementValidator.cs` - 配置検証
- `PlacementOptimizer.cs` - 配置最適化（基本版）

**実装順序**:
1. BoardStateManagerの基本配置機能
2. PlacementValidatorの検証ルール
3. 基本的なPlacementOptimizer
4. 3クラス間の連携テスト

**テストシーン**: `BoardSystemTest.scene`
- 配置・移動・除去の確認
- 検証ルールの動作確認
- 最適化提案の基本動作

#### Step 2.3: スキルシステム
**ファイル**: `Scripts/Gameplay/Skills/`
- `SkillManager.cs`
- `Skill.cs`
- `SkillData.cs`（ScriptableObject）
- `SkillEffect.cs`

**実装順序**:
1. スキルの基本構造とデータ
2. スキル実行ロジック
3. 効果範囲の計算
4. クールダウンシステム

**テストシーン**: `SkillSystemTest.scene`
- スキル発動の確認
- 効果範囲の可視化
- クールダウンの動作確認

#### Step 2.4: シナジーシステム分割版
**ファイル**: `Scripts/Gameplay/Synergy/`
- `SynergyCalculator.cs` - 計算ロジック
- `SynergyEffectApplier.cs` - 効果適用
- `SynergyVisualizer.cs` - 表示制御
- `SynergySystemManager.cs` - 統合管理

**実装順序**:
1. SynergyCalculatorの計算ロジック
2. SynergyEffectApplierの効果適用
3. 基本的なSynergyVisualizer
4. SynergySystemManagerによる統合

**テストシーン**: `SynergySystemTest.scene`
- シナジー計算の確認
- 効果適用の確認
- 表示システムの動作確認

#### Step 2.5: バトルマネージャー統合版
**ファイル**: `Scripts/Gameplay/Battle/`
- `BattleManager.cs` - 統合バトル管理
- `BattleAI.cs` - AI行動決定
- `BattleSetup.cs` - 戦闘セットアップ
- `BattleResult.cs` - 戦闘結果

**実装順序**:
1. 基本的な戦闘フロー
2. ターン管理システム
3. AI行動決定ロジック
4. スキルシステムとの連携
5. 戦闘結果とゴールド報酬

**テストシーン**: `BattleSystemTest.scene`
- 基本戦闘の実行確認
- AI行動の確認
- システム間連携の確認

---

## Phase 3: UI統合

### 目標
ゲームロジックとUIの統合

#### Step 3.1: 基本UI基盤
**ファイル**: `Scripts/UI/Core/`
- `UIBase.cs` - UI基底クラス
- `UIManager.cs` - UI管理
- `UITransition.cs` - 画面遷移

#### Step 3.2: ガチャUI
**ファイル**: `Scripts/UI/Gacha/`
- `GachaUI.cs`
- ガチャシステムとの連携
- 基本的な演出

#### Step 3.3: パーティ編成UI
**ファイル**: `Scripts/UI/Formation/`
- `PartyFormationUI.cs`
- ドラッグ&ドロップ実装
- シナジー表示連携

#### Step 3.4: 戦闘UI
**ファイル**: `Scripts/UI/Battle/`
- `BattleUI.cs`
- リアルタイム状況表示
- 戦闘結果表示

**統合テストシーン**: `GameFlowTest.scene`
- 準備→戦闘→結果の一連の流れ
- UI操作の確認

---

## Phase 4: 拡張機能

### 目標
ゲーム体験を豊かにする機能の追加

#### Step 4.1: 天井システム
**ファイル**: `Scripts/Gameplay/Gacha/Ceiling/`
- ガチャシステムとの統合

#### Step 4.2: ピックアップガチャ
**ファイル**: `Scripts/Gameplay/Gacha/Pickup/`
- 特別ガチャの実装

#### Step 4.3: 配置最適化強化
- PlacementOptimizerの高度な機能
- 敵情報に基づく最適化

#### Step 4.4: 特殊アイテムシステム
**ファイル**: `Scripts/Gameplay/Items/`
- キャラクターからアイテムへの変換

---

## Phase 5: 最終統合

### 目標
完全なゲームループの実現

#### Step 5.1: ゲームサイクル管理
**ファイル**: `Scripts/Core/GameCycle/`
- 全システムとの統合
- ランのリセット機能

#### Step 5.2: データリセット機能
- 完全なリセット処理の実装

#### Step 5.3: 最終テスト
**シーン**: `MainGame.scene`
- 完全なゲームループの確認
- パフォーマンステスト

---

## 各段階のテスト戦略

### 単体テスト
- 各クラスの基本機能テスト
- 不正入力に対する防御テスト
- エッジケースの確認

### 統合テスト
- システム間の連携テスト
- データフローの確認
- イベント通知の確認

### シナリオテスト
- 実際のゲームプレイシナリオ
- ユーザー操作の模擬
- 異常系の処理確認

### パフォーマンステスト
- メモリ使用量の確認
- ガベージコレクションの影響
- フレームレートの維持

---

## 手戻り防止策

### インターフェース先行設計
- 各段階でインターフェースを先に確定
- 実装詳細の変更が他システムに影響しないよう分離

### 段階的統合
- 各ステップで動作確認を実施
- 問題発見時の影響範囲を最小化

### 継続的テスト
- 実装と並行してテストシーンを更新
- 既存機能の回帰テストを継続実行

### ドキュメント更新
- 実装に合わせて設計書を更新
- 変更履歴の記録

この計画により、大幅な手戻りなく段階的にシステムを構築できます。