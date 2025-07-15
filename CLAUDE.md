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


### プロジェクト構造
```
Assets/
├── Scripts/
│   ├── Core/                 # 基盤システム
│   │   ├── Character/        # キャラクターシステム
│   │   ├── Gold/             # ゴールドシステム
│   │   ├── Data/             # データ管理
│   │   ├── Error/            # エラーハンドリング
│   │   ├── Systems/          # システム基盤
│   │   ├── Unity/            # Unity統合
│   │   └── Interfaces/       # 共通インターフェース
│   ├── Editor/               # エディタ拡張
│   ├── Gameplay/             # ゲームプレイ
│   ├── UI/                   # UI
│   └── Utils/                # ユーティリティ
├── Resources/                # データアセット統一管理
│   ├── Settings/             # 設定データ
│   ├── Characters/           # キャラクターデータ
│   ├── Gacha/                # ガチャシステムデータ
│   └── Audio/                # 音声設定
├── Prefab/                   # システムプレハブ
├── Scenes/                   # シーン
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
- **コアゲームプレイ**: Auto-chess風の7x8戦闘フィールド（自軍7x4配置エリア）でのパーティ構築
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
3. **実装ガイドラインは類似する既存実装を参照**
4. **データ設計は`.tmp/data_design.md`で統一管理**
5. **段階的実装（Phase 0 → Phase 1 → Phase 2-5）**
6. **各Step完了時に対応するチェックボックスを更新**
7. **各機能の動作確認テストを必須とする**
8. **変更をコミットしない。代わりに確認を求める**

## Unity開発のプログラミングルール

- /home/tomosige/workspace/tomosige/GatchaSpire/.tmp/coding_standards.md を遵守すること
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
- **重要な設計方針**:
  - 完全リセット型ローグライト（永続的アップグレードなし）
  - リソース管理ジレンマ（ガチャ vs ガチャアップグレード）
  - 7x8戦闘フィールド（自軍7x4配置エリア）でのAuto-chess風パーティ構築
  - オフライン専用ゲーム
- **パフォーマンス制約**: 2GB RAM端末対応、60fps維持
- **エラーハンドリング**: UnityErrorHandlerによる統一的エラー管理
- **データ管理**: ScriptableObjectとJSONセーブデータの組み合わせ
- **テスト方針**: 各機能の動作確認テストを必須とする

## .tmpフォルダドキュメント優先度

### 最高優先度（すぐに理解が必要）
- **memories.md** - その日行った作業の備忘録
- **implementation_tasks_revised.md** - 実装タスクリスト
- **data_design.md** - データ設計書
- **technical_specifications.md** - 技術仕様書

### 高優先度（実装時に参照が必要）
- **design/detailed_skill_system_specification.md** - スキルシステム詳細仕様
- **design/detailed_battle_system_specification.md** - バトルシステム詳細仕様
- **design/new_design_gacha_system.md** - ガチャシステム設計
- **design/new_design_character_system.md** - キャラクターシステム設計
- **design/new_design_gold_system.md** - ゴールドシステム設計


### 作業日誌
/home/tomosige/workspace/tomosige/GatchaSpire/.tmp/memories.md
作業の区切りごとに上のドキュメントに記録を付ける。
起動したときにmemories.mdをすべて読むこと。

---

*このドキュメントはプロジェクトの進行に応じて適宜更新されます。*
