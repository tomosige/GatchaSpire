# データ設計書 - GatchaSpire

## 基本情報

- **プロジェクト名**: GatchaSpire
- **データ設計バージョン**: 1.0
- **作成日**: 2025-07-08
- **対象Unity バージョン**: 2021.3.22f1

---

## データ構造概要

### データ分類
1. **ゲーム設定データ** (ScriptableObject) - 読み取り専用
2. **プレイヤーデータ** (Save Data) - 読み書き可能
3. **一時データ** (Runtime Data) - セッション中のみ
4. **開発設定データ** (Development Settings) - デバッグ用

---

## ScriptableObject データ設計

### 1. CharacterData (キャラクターマスターデータ)

**概要**：
- **目的**: キャラクターの基本データとステータス成長を管理
- **成長システム**: レベル制（1-100）とレアリティ倍率による段階的成長
- **分類システム**: 種族・クラス・属性・役割による多軸分類
- **スキル連携**: Lv3/6/10でのスキル習得システム対応

**詳細仕様**：
- **CharacterData詳細仕様**: [`.tmp/design/detailed_character_data_specification.md`](design/detailed_character_data_specification.md)
- **データ構造・計算式**: 上記詳細仕様書の「1. データ構造詳細」を参照

**特徴**：
- **レアリティ倍率**: Common(1.0) → Legendary(2.0)の段階的成長
- **8ステータス**: HP/MP/Attack/Defense/Speed/Magic/Resistance/Luck
- **経験値計算**: 指数成長による適切なレベルアップ曲線
- **バリデーション**: 実行時データ整合性チェック
- **戦闘力計算**: 重み付きステータス合計による客観的評価

**実装状況**: 実装済み（Assets/Scripts/Core/Character/CharacterData.cs）

### 2. GachaSystemData (ガチャシステム設定)

**概要**：
- **目的**: ガチャシステムの全設定を統合管理
- **統合設計**: 基本ガチャ・アップグレード・天井・ピックアップの一元管理
- **段階的成長**: レベル別のガチャ性能向上システム
- **プレイヤー保護**: 天井システムによる最悪ケース保護

**詳細仕様**：
- **GachaSystemData詳細仕様**: [`.tmp/design/detailed_gacha_system_data_specification.md`](design/detailed_gacha_system_data_specification.md)
- **確率計算・バランス設計**: 上記詳細仕様書の「5. 主要メソッド詳細」を参照

**特徴**：
- **排出率制御**: レアリティ別の詳細確率設定とアップグレード連動
- **4種類のアップグレード効果**: 確率向上/コスト削減/同時排出/保証改善
- **天井システム**: 100回でEpic以上保証の標準設定
- **ピックアップ**: 特定キャラクターの出現率倍率システム
- **バリデーション**: 確率合計100%チェック等の整合性検証

**実装状況**: 実装済み（Assets/Scripts/Core/Gacha/GachaSystemData.cs）

### 3. SkillData (スキルマスターデータ)

**概要**：
- **目的**: キャラクターが習得するスキルの基本データ
- **習得方式**: レベル制（Lv3/6/10で計3個のスキル習得）
- **発動方式**: リアルタイム戦闘での個別クールダウン制
- **効果システム**: 複雑な継承システムによるスキル効果

**詳細仕様**：
- **スキルシステム詳細仕様**: [`.tmp/design/detailed_skill_system_specification.md`](design/detailed_skill_system_specification.md)
- **データ構造・実装指針**: 上記詳細仕様書の「3. SkillEffect詳細設計」を参照

**特徴**：
- **7x8戦闘フィールド対応**: 射程・範囲計算が戦闘フィールドに対応
- **Unity Time.time使用**: 端末性能に依存しないクールダウン管理
- **5種類のSkillEffect**: Damage/Heal/StatModifier/StatusEffect/Displacement
- **スケーリングシステム**: ステータス依存のスキル効果値計算
- **複雑発動条件**: HP閾値、敵数、シナジー発動等の条件システム

**実装状況**: 未実装（詳細仕様に基づく実装が必要）


### 4. SynergyData (シナジー設定)

**概要**：
- **目的**: シナジーシステムの設定とボーナス効果を管理
- **統合設計**: 種族・クラス・属性・役割による多軸シナジーシステム
- **段階的効果**: シナジー発動数に応じた段階的ボーナス適用
- **戦略性強化**: パーティ構築の戦略的選択肢を拡大

**詳細仕様**：
- **シナジーシステム詳細仕様**: [`.tmp/design/new_design_synergy_system_split.md`](design/new_design_synergy_system_split.md)
- **データ構造・計算式**: 上記詳細仕様書の「SynergyData」セクションを参照

**特徴**：
- **4種類のシナジータイプ**: 種族(Race)/クラス(Class)/属性(Element)/役割(Role)による分類
- **段階的発動システム**: 2体/4体/6体による3段階の効果強化
- **複合効果対応**: 複数シナジーの同時発動とスタック処理
- **動的計算**: パーティ構成変更時のリアルタイム効果更新
- **バリデーション**: シナジー設定の整合性検証とバランス調整支援

**実装状況**: 未実装（詳細仕様に基づく実装が必要）


### 5. DevelopmentSettings (開発設定)


---

## セーブデータ設計

### プレイヤーセーブデータ構造


---

## JSONデータフォーマット

### セーブデータファイル構造


### 設定ファイル構造

## ファイル配置・命名規約

### ScriptableObject配置
```
Assets/Resources/
├── Characters/          # キャラクターデータ
│   ├── Common/
│   ├── Rare/
│   ├── Epic/
│   └── Legendary/
├── Gacha/              # ガチャシステムデータ
├── Skills/             # スキルデータ
├── Synergies/          # シナジーデータ
└── Settings/           # 各種設定データ
    ├── DevelopmentSettings.asset
    └── BalanceSettings.asset
```

### セーブデータ配置

### 命名規約
```
ScriptableObject: PascalCase_descriptor.asset
例: CharacterData_Knight001.asset

JSON Files: snake_case.json
例: player_save_data.json

Class Names: PascalCase
Field Names: camelCase
Constants: UPPER_SNAKE_CASE
```

---

## セキュリティ・データ保護

### セーブデータ保護（簡易版）
```
暗号化: なし（オフラインゲーム、不正対策不要）
データ形式: 標準JSON
バックアップ: 前回セーブデータを1世代保持
```

### データ整合性（最小限）
```
ファイル破損検出: JSONパース失敗時のみ
復旧処理: バックアップファイルからの自動復元
```

---

## パフォーマンス考慮

### メモリ使用量目標
```
CharacterData: ~1KB/キャラクター
セーブデータ: ~50KB (100キャラクター所持時)
テンポラリデータ: ~10MB
```

### ロード時間目標
```
セーブデータロード: 1秒以内
ScriptableObjectロード: 2秒以内
初期化処理: 3秒以内
```

このデータ設計書に基づいて、一貫性のあるデータ管理システムを構築できます。