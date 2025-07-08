# 機能06: キャラクター基本システム

## 概要
キャラクターのステータス管理、レアリティシステム、種族/クラスの分類を担当するシステム。

## 実装クラス設計

### Character
個別のキャラクターインスタンスを表すクラス。

**publicメソッド:**
- `void Initialize(CharacterData data)` - キャラクター初期化
- `void LevelUp()` - レベルアップ処理
- `void AddExperience(int exp)` - 経験値追加
- `int GetCurrentLevel()` - 現在レベル取得
- `CharacterStats GetCurrentStats()` - 現在ステータス取得
- `bool CanLevelUp()` - レベルアップ可能判定

**詳細説明:**
- CharacterDataをベースにインスタンス化
- レベルアップによるステータス変化
- 経験値システムとの連携
- 戦闘での一時的なステータス変化管理

### CharacterData
キャラクターの基本データを定義するScriptableObject。

**publicメソッド:**
- `CharacterStats GetBaseStats()` - 基本ステータス取得
- `CharacterStats GetStatsAtLevel(int level)` - レベル別ステータス取得
- `List<Skill> GetSkillsAtLevel(int level)` - レベル別スキル取得
- `string GetDescription()` - キャラクター説明取得
- `Sprite GetPortrait()` - ポートレート画像取得

**詳細説明:**
- 全キャラクターの基本設定を管理
- レベル成長曲線の定義
- スキル習得レベルの設定
- UI表示用のリソース管理

### CharacterStats
キャラクターのステータス情報を保持する構造体。

**publicメソッド:**
- `CharacterStats(int hp, int attack, int defense, int speed)` - コンストラクタ
- `CharacterStats ApplyModifier(StatModifier modifier)` - 修正値適用
- `CharacterStats Add(CharacterStats other)` - ステータス加算
- `CharacterStats Multiply(float multiplier)` - ステータス倍率適用
- `string GetFormattedStats()` - フォーマットされたステータス文字列取得

**詳細説明:**
- 基本ステータス（HP、攻撃力、防御力、素早さ）
- ステータス計算の共通処理
- バフ・デバフ効果の適用
- UI表示用のフォーマット機能

### CharacterRarity (enum)
キャラクターのレアリティを表す列挙型。

**値:**
- `Common` - コモン（1星）
- `Uncommon` - アンコモン（2星）
- `Rare` - レア（3星）
- `Epic` - エピック（4星）
- `Legendary` - レジェンダリー（5星）

### CharacterRace (enum)
キャラクターの種族を表す列挙型。

**値:**
- `Human` - 人間
- `Elf` - エルフ
- `Dwarf` - ドワーフ
- `Orc` - オーク
- `Dragon` - ドラゴン
- `Demon` - デーモン

### CharacterClass (enum)
キャラクターのクラスを表す列挙型。

**値:**
- `Warrior` - 戦士
- `Mage` - 魔法使い
- `Archer` - 弓使い
- `Healer` - 回復職
- `Assassin` - 暗殺者
- `Tank` - タンク

### CharacterDatabase
全キャラクターデータを管理するデータベースクラス。

**publicメソッド:**
- `CharacterData GetCharacterById(int id)` - IDによるキャラクター取得
- `List<CharacterData> GetCharactersByRarity(CharacterRarity rarity)` - レアリティ別キャラクター取得
- `List<CharacterData> GetCharactersByRace(CharacterRace race)` - 種族別キャラクター取得
- `List<CharacterData> GetCharactersByClass(CharacterClass characterClass)` - クラス別キャラクター取得
- `List<CharacterData> GetAllCharacters()` - 全キャラクター取得

**詳細説明:**
- 全キャラクターの一元管理
- 検索・フィルタリング機能
- キャラクターの動的読み込み
- エディタ拡張でのキャラクター管理UI