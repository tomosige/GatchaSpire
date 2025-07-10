# Unity Project Guidelines - GatchaSpire

このドキュメントはUnityプロジェクト「GatchaSpire」の開発ガイドラインを定義します。以下の内容に従ってプロジェクトを進めてください。

## 最上位ルール

- 効率を最大化するため、**複数の独立したプロセスを実行する必要がある場合は、それらのツールを順次ではなく並行して呼び出す**
- **思考は英語で行う必要があります**。ただし、**日本語で回答する必要があります**

## プロジェクト情報

- **プロジェクト名**: GatchaSpire
- **ゲーム概要**: 一期一会のパーティ構築ローグライト
- **Unity バージョン**: 2021.3.22f1 LTS
- **プラットフォーム**: Android優先（API 26+、2GB RAM）、PC対応
- **現在の実装状況**:
  - **Phase 0**: 完了 - Unity基盤システムとエラーハンドリング
  - **Phase 1**: 進行中 - コアシステム（Step 1.2まで完了）
  - **Phase 2-5**: 未実装 - ゲームロジック、UI、最終統合

### プロジェクト構造
```
Assets/
├── Scripts/
│   ├── Core/                 # 基盤システム（完了）
│   │   ├── Character/        # キャラクターシステム（完了）
│   │   ├── Gold/             # ゴールドシステム（完了）
│   │   ├── Data/             # データ管理
│   │   ├── Error/            # エラーハンドリング（完了）
│   │   ├── Systems/          # システム基盤（完了）
│   │   ├── Unity/            # Unity統合（完了）
│   │   └── Interfaces/       # 共通インターフェース（完了）
│   ├── Editor/               # エディタ拡張（基本機能完了）
│   ├── Gameplay/             # ゲームプレイ（未実装）
│   ├── UI/                   # UI（未実装）
│   └── Utils/                # ユーティリティ（未実装）
├── Resources/                # データアセット統一管理
│   ├── Settings/
│   ├── Characters/           # キャラクターデータ（テスト用のみ）
│   ├── Gacha/                # ガチャシステムデータ（未実装）
│   └── Audio/                # 音声設定（未実装）
├── Prefab/                   # システムプレハブ
├── Scenes/                   # シーン
│   └── Tests/                # テスト用シーン（Phase0_UnityFoundationTest等）
├── Fonts/                    # フォント素材
└── Tests/                    # テストアセット
```

## Unity開発ルール

### コーディング規約
- **C#スクリプト**のコメントとドキュメントは**日本語**で記述する
- **XMLドキュメントコメント**（///）は日本語で記述する
- **コード内のコメント**（//）は日本語で記述する
- **絵文字は使用しない**
- 日本語記述時は不要なスペースを含めない
  - 例: ◯「Unity開発ガイド」× "Unity 開発 ガイド"

### Unity固有のルール
- **MonoBehaviourの継承**：IUnityGameSystemインターフェース実装を推奨
- **SerializeField**：publicフィールドの代わりに適切に使用する
- **Header属性**：Inspector表示のグループ化に使用（日本語記述）
- **エラーハンドリング**：UnityErrorHandlerシステムを統一的に使用
- **システム管理**：UnityGameSystemCoordinatorによる統一管理
- **DontDestroyOnLoad**：シーン間でのオブジェクト永続化に使用
- **ValidationResult**：ScriptableObjectの検証に使用
- **DevelopmentSettings**：デバッグ・チート機能の管理に使用

### パフォーマンス規約
- **Update()メソッド**：重い処理は避け、必要に応じてコルーチンやイベント駆動に変更
- **オブジェクトプーリング**：頻繁に生成/削除されるオブジェクトには適用を検討
- **テクスチャサイズ**：適切な解像度とフォーマットを選択
- **メモリ管理**：不要なアセットは適切にUnloadする

### ファイル命名規約
- **Csharpスクリプト**: PascalCase（例：PlayerController.cs）
- **プレハブ**: PascalCase（例：PlayerCharacter.prefab）
- **シーン**: PascalCase（例：MainMenu.scene）
- **アート素材**: snake_case（例：character_idle.png）
- **音声素材**: snake_case（例：bgm_battle.wav）

## プロジェクト目標

### ゲームコンセプト
- **ジャンル**: 一期一会のパーティ構築ローグライト
- **コアゲームプレイ**: Auto-chess風の5x5グリッドでのパーティ構築
- **リソース管理**: ゴールドをガチャかガチャアップグレードに投資するジレンマ
- **リセット方針**: 完全リセット型ローグライト（永続的アップグレードなし）
- **オフライン専用**: ネットワーク機能なし

### 技術的制約
- **メモリ制約**: 2GB RAM端末対応
- **ターゲットAPI**: Android API 26+
- **フレームレート**: 60fps維持
- **ロード時間**: 初期化3秒以内

### 実装プロセス
1. **各タスクの要件と設計は`.tmp/`フォルダで文書化済み**
2. **詳細なタスクリストは`.tmp/implementation_tasks_revised.md`で管理**
3. **実装ガイドラインは`.tmp/implementation_guidelines.md`で参照**
4. **データ設計は`.tmp/data_design.md`で統一管理**
5. **段階的実装（Phase 0 → Phase 1 → Phase 2-5）**
6. **各Step完了時に対応するチェックボックスを更新**
7. **各機能の動作確認テストを必須とする**
8. **変更をコミットしない。代わりに確認を求める**

### ブランチ戦略
- **main**: 安定版（Phase完了時のみマージ）
- **feature/**: 機能開発ブランチ（例: `feature/gacha-system`）
- **core-system**: 現在の開発ブランチ（Phase 1実装中）

### PR作成時の形式
- **タイトル**: 簡潔な要約
- **主な変更点**: 変更内容、注意点等を記述
- **テスト**: 実行したテストと結果
- **関連タスク**: 関連タスクのリンクまたは番号
- **その他**: 特別な注意事項

## Unity開発のプログラミングルール

- 絶対に必要でない限り、値をハードコーディングしない
- **コード実装前の確認ルール**：
  - 継承するクラス/インターフェースの全メンバーを確認し、必要なメソッド・プロパティを過不足なく実装すること
  - 参照するクラスの実際のプロパティ名・メソッド名を確認してから使用すること
  - 推測での実装は一切禁止。不明な点は該当ファイルを読んで確認すること
- **ScriptableObject**を使用してゲームデータを管理する
- **IUnityGameSystem**インターフェースを実装してシステム管理を統一する
- **UnityErrorHandler**を使用してエラーハンドリングを統一する
- **DevelopmentSettings**を活用してデバッグ・チート機能を管理する
- **nullチェック**を適切に行い、NullReferenceExceptionを防ぐ
- **コルーチン**の適切な停止処理を実装する
- **イベントシステム**を活用してコンポーネント間の結合を緩める
- **ValidationResult**を使用してScriptableObjectの検証を統一する
- その他はMicrosoftのC#コーディングガイドラインに準拠する

### オブジェクト初期化規約

- **単純なコレクション型**は宣言時に初期化してNullReferenceExceptionを防ぐ
  - `List<T>`, `Dictionary<TKey, TValue>`, `HashSet<T>`, `Queue<T>`, `Stack<T>`等
  - 例：`private List<string> items = new List<string>();`
- **複雑な依存関係を持つオブジェクト**はメソッド内で制御された初期化を行う
  - Unity固有オブジェクト（`GameObject`, `Component`等）
  - 外部依存のあるマネージャークラス
  - 設定に依存する初期化が必要なオブジェクト
  - インターフェース型のフィールド
- **理由**：単純なコレクションのnullエラーは予防可能だが、複雑なオブジェクトは適切な初期化タイミングとエラーハンドリングが重要

## Unity固有のタスク完了通知

- タスク完了時は必ず通知を送信する
- 「タスク完了」とは、ユーザーに回答を終了し、次の入力を待っている状態を指す
- フォーマット修正、リファクタリング、ドキュメント更新などの**軽微なタスクでも通知が必要**
- 以下の形式と`powershell.exe`を使用して通知を送信：
  - `powershell.exe -File 'C:\Users\tomot\Scripts\Notify-Toast.ps1' -message "${TASK_DESCRIPTION} completed!"'`
  - `${TASK_DESCRIPTION}`はタスクの要約である必要があります

## Unityツール使用ガイドライン

- **Context7 MCP**を使用して最新のUnity APIと機能について情報を取得する
- 隠しフォルダ（`.tmp`等）の検索には**Bashツール**を使用する
- **Unity MCPツール**を活用してUnityエディタと直接連携する
- コンソールログの確認とデバッグにUnity MCPの機能を使用する

## プロジェクト特有の注意事項

### プロジェクト特有の注意事項

- **GatchaSpire**は「ガチャ」をテーマにした一期一会のパーティ構築ローグライト
- **現在の実装状況**: Phase 0完了、Phase 1進行中（Step 1.2まで完了）
- **次の実装目標**: Step 1.3 ガチャシステム統合版の実装
- **重要な設計方針**:
  - 完全リセット型ローグライト（永続的アップグレードなし）
  - リソース管理ジレンマ（ガチャ vs ガチャアップグレード）
  - 5x5グリッドでのAuto-chess風パーティ構築
  - オフライン専用ゲーム
- **パフォーマンス制約**: 2GB RAM端末対応、60fps維持
- **エラーハンドリング**: UnityErrorHandlerによる統一的エラー管理
- **データ管理**: ScriptableObjectとJSONセーブデータの組み合わせ
- **テスト方針**: 各機能の動作確認テストを必須とする

## .tmpフォルダドキュメント優先度

### 最高優先度（すぐに理解が必要）
- **implementation_tasks_revised.md** - 実装タスクリスト
- **implementation_guidelines.md** - 実装ガイドライン
- **data_design.md** - データ設計書
- **technical_specifications.md** - 技術仕様書

### 高優先度（実装時に参照が必要）
- **design/new_design_gacha_system.md** - ガチャシステム設計
- **design/new_design_character_system.md** - キャラクターシステム設計
- **design/new_design_gold_system.md** - ゴールドシステム設計
- **implementation_plan_revised.md** - 実装計画（修正版）

### 中優先度（設計参考用）
- **design/overall_design.md** - 全体設計
- **design/revised_design_battle_system.md** - バトルシステム設計
- **design/revised_design_synergy_system.md** - シナジーシステム設計
- **design/revised_design_board_system.md** - ボードシステム設計
- **design/revised_design_inventory_system.md** - インベントリシステム設計
- **design/revised_design_ui_system.md** - UIシステム設計
- **design/revised_design_skill_system.md** - スキルシステム設計
- **design/revised_design_save_system.md** - セーブシステム設計
- **design/revised_design_error_handling.md** - エラーハンドリング設計
- **design/revised_design_development_support.md** - 開発支援設計
- **design/revised_design_unity_integration.md** - Unity統合設計
- **project_structure.md** - プロジェクト構造
- **feature_specifications.md** - 機能仕様書
- **game_design.md** - ゲーム設計書
- **system_architecture.md** - システムアーキテクチャ
- **coding_standards.md** - コーディング規約

### 低優先度（背景理解用）
- **user_stories.md** - ユーザーストーリー
- **test_plan.md** - テスト計画
- **performance_requirements.md** - パフォーマンス要件

---

*このドキュメントはプロジェクトの進行に応じて適宜更新されます。*
