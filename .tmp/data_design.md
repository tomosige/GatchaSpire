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

```csharp
[CreateAssetMenu(fileName = "CharacterData", menuName = "GatchaSpire/Character Data")]
public class CharacterData : ScriptableObject, IValidatable, IScriptableObjectData
{
    [Header("基本情報")]
    public string characterId;           // 一意ID (例: "char_001")
    public string characterName;         // 表示名
    public string description;           // 説明文
    public Sprite characterIcon;         // アイコン画像
    public Sprite characterArt;          // キャラクター画像
    
    [Header("レアリティ・分類")]
    public CharacterRarity rarity;       // レアリティ (Common, Rare, Epic, Legendary)
    public CharacterRace race;           // 種族 (Human, Elf, Dwarf, Beast, etc.)
    public CharacterClass characterClass; // クラス (Warrior, Mage, Archer, etc.)
    
    [Header("基本ステータス")]
    public CharacterStats baseStats;     // レベル1時のステータス
    public CharacterStats growthStats;   // レベルアップ時の成長値
    public int maxLevel;                 // 最大レベル (デフォルト: 100)
    
    [Header("スキル")]
    public SkillData[] skills;           // 所持スキル配列
    public int[] skillUnlockLevels;      // スキル解放レベル
    
    [Header("経済関連")]
    public int sellPrice;                // 売却価格
    public int fusionExpValue;           // 合成時の経験値変換値
    public int upgradeCost;              // 強化コスト
    
    [Header("バランス調整")]
    public float rarityMultiplier;       // レアリティ補正値
    public bool isLimitedCharacter;      // 限定キャラクターフラグ
    public string[] synergyTags;         // シナジータグ配列
}

[System.Serializable]
public struct CharacterStats
{
    public int health;          // HP
    public int attack;          // 攻撃力
    public int defense;         // 防御力
    public int speed;           // 素早さ
    public int magic;           // 魔力
    public int resistance;      // 魔法防御
    public int accuracy;        // 命中率
    public int critical;        // クリティカル率
}
```

### 2. GachaSystemData (ガチャシステム設定)

```csharp
[CreateAssetMenu(fileName = "GachaSystemData", menuName = "GatchaSpire/Gacha System Data")]
public class GachaSystemData : ScriptableObject, IValidatable
{
    [Header("基本設定")]
    public string gachaSystemId;         // ガチャID
    public string displayName;           // 表示名
    public int baseCost;                 // 基本コスト (ゴールド)
    public bool isActive;                // 有効フラグ
    
    [Header("排出確率設定")]
    public GachaDropRate[] dropRates;    // レアリティ別排出率
    public float guaranteedRareRate;     // 保証レア以上確率
    public int guaranteedRareCount;      // 保証までの回数
    
    [Header("アップグレード設定")]
    public int maxUpgradeLevel;          // 最大アップグレードレベル
    public GachaUpgrade[] upgrades;      // アップグレード効果配列
    
    [Header("天井システム")]
    public bool hasCeiling;              // 天井システム有効
    public int ceilingCount;             // 天井回数
    public CharacterRarity ceilingRarity; // 天井時保証レアリティ
    
    [Header("ピックアップ設定")]
    public bool hasPickup;               // ピックアップ有効
    public string[] pickupCharacterIds;  // ピックアップキャラクターID配列
    public float pickupRate;             // ピックアップ追加確率
}

[System.Serializable]
public struct GachaDropRate
{
    public CharacterRarity rarity;       // レアリティ
    public float baseRate;               // 基本確率 (%)
    public float upgradeBonus;           // アップグレード時ボーナス (%)
    public int weight;                   // 抽選重み
}

[System.Serializable]
public struct GachaUpgrade
{
    public int level;                    // アップグレードレベル
    public int cost;                     // アップグレードコスト
    public string effectDescription;     // 効果説明
    public GachaUpgradeEffect[] effects; // 効果配列
}

[System.Serializable]
public struct GachaUpgradeEffect
{
    public GachaUpgradeType type;        // 効果タイプ
    public CharacterRarity targetRarity; // 対象レアリティ
    public float value;                  // 効果値
}
```

### 3. SkillData (スキルマスターデータ)

```csharp
[CreateAssetMenu(fileName = "SkillData", menuName = "GatchaSpire/Skill Data")]
public class SkillData : ScriptableObject, IValidatable
{
    [Header("基本情報")]
    public string skillId;               // スキルID
    public string skillName;             // スキル名
    public string description;           // 説明
    public Sprite skillIcon;             // アイコン
    
    [Header("スキル設定")]
    public SkillType skillType;          // スキルタイプ (Active, Passive, Auto)
    public SkillTarget targetType;       // 対象タイプ (Self, Enemy, All, etc.)
    public int cooldown;                 // クールダウン (ターン)
    public int energyCost;               // エネルギーコスト
    
    [Header("効果設定")]
    public SkillEffect[] effects;        // スキル効果配列
    public SkillLevelData[] levelData;   // レベル別データ
    
    [Header("発動条件")]
    public SkillCondition[] conditions;  // 発動条件配列
    public float baseProbability;        // 基本発動確率
}

[System.Serializable]
public struct SkillLevelData
{
    public int level;                    // スキルレベル
    public float damageMultiplier;       // ダメージ倍率
    public float effectPower;            // 効果力
    public int duration;                 // 効果持続時間
    public string levelDescription;      // レベル別説明
}
```

### 4. SynergyData (シナジー設定)

```csharp
[CreateAssetMenu(fileName = "SynergyData", menuName = "GatchaSpire/Synergy Data")]
public class SynergyData : ScriptableObject, IValidatable
{
    [Header("基本設定")]
    public string synergyId;             // シナジーID
    public string synergyName;           // シナジー名
    public string description;           // 説明
    public Sprite synergyIcon;           // アイコン
    
    [Header("発動条件")]
    public SynergyCondition condition;   // 発動条件 (Race, Class, Tag)
    public SynergyRequirement[] requirements; // 必要数配列
    
    [Header("効果設定")]
    public SynergyEffect[] effects;      // シナジー効果配列
    public bool stackable;               // 重複可能フラグ
    public int maxStacks;                // 最大重複数
}

[System.Serializable]
public struct SynergyRequirement
{
    public int requiredCount;            // 必要キャラクター数
    public SynergyLevel level;           // シナジーレベル
    public string levelDescription;      // レベル説明
}

[System.Serializable]
public struct SynergyEffect
{
    public SynergyEffectType type;       // 効果タイプ
    public StatType targetStat;          // 対象ステータス
    public float value;                  // 効果値
    public bool isPercentage;            // パーセンテージフラグ
    public SynergyTarget target;         // 効果対象
}
```

### 5. DevelopmentSettings (開発設定)

```csharp
[CreateAssetMenu(fileName = "DevelopmentSettings", menuName = "GatchaSpire/Development Settings")]
public class DevelopmentSettings : ScriptableObject, IValidatable
{
    [Header("デバッグ設定")]
    public bool enableAllDebugLogs;      // 全デバッグログ有効
    public bool showPerformanceInfo;     // パフォーマンス情報表示
    public bool enableErrorDetails;      // エラー詳細表示
    public bool showSystemStatus;        // システム状態表示
    
    [Header("ゲームバランス調整")]
    public float goldMultiplier;         // ゴールド獲得倍率
    public float expMultiplier;          // 経験値獲得倍率
    public bool skipTutorial;            // チュートリアルスキップ
    public bool fastAnimations;          // アニメーション高速化
    
    [Header("チート機能")]
    public bool infiniteGold;            // 無限ゴールド
    public bool maxLevel;                // レベル最大
    public bool allCharactersUnlocked;   // 全キャラクター解放
    public bool noGachaCost;             // ガチャコスト無料
    
    [Header("テスト設定")]
    public bool autoSave;                // 自動セーブ
    public int saveInterval;             // セーブ間隔 (秒)
    public bool validateOnLoad;          // ロード時検証
    public bool enablePerformanceTests;  // パフォーマンステスト有効
    
    [Header("UI設定")]
    public bool showDebugPanel;          // デバッグパネル表示
    public bool showFPS;                 // FPS表示
    public bool showMemoryUsage;         // メモリ使用量表示
    public DebugUIPosition debugUIPosition; // デバッグUI位置
}
```

---

## セーブデータ設計

### プレイヤーセーブデータ構造

```csharp
[System.Serializable]
public class PlayerSaveData
{
    [Header("基本情報")]
    public string playerId;              // プレイヤーID
    public string playerName;            // プレイヤー名
    public int playerLevel;              // プレイヤーレベル
    public int totalPlayTime;            // 総プレイ時間 (秒)
    public string lastSaveTime;          // 最終セーブ時刻
    public int saveDataVersion;          // セーブデータバージョン
    
    [Header("リソース")]
    public long currentGold;             // 現在のゴールド
    public int totalGoldEarned;          // 累計獲得ゴールド
    public int totalGoldSpent;           // 累計消費ゴールド
    
    [Header("キャラクター")]
    public List<CharacterSaveData> ownedCharacters; // 所持キャラクター
    public int maxCharacterSlots;        // 最大所持数
    public string[] favoriteCharacters;  // お気に入りキャラクター
    
    [Header("ガチャ")]
    public List<GachaSaveData> gachaStatus; // ガチャ状態
    public int totalGachaCount;          // 総ガチャ回数
    public GachaHistory gachaHistory;    // ガチャ履歴
    
    [Header("パーティ編成")]
    public PartyFormation currentParty;  // 現在のパーティ
    public List<PartyFormation> savedParties; // 保存パーティ
    
    [Header("進行状況")]
    public int currentStage;             // 現在ステージ
    public int highestStage;             // 最高到達ステージ
    public bool[] completedAchievements; // 達成済み実績
    
    [Header("設定")]
    public PlayerSettings playerSettings; // プレイヤー設定
    public StatisticsData statistics;    // 統計データ
}

[System.Serializable]
public class CharacterSaveData
{
    public string characterId;           // キャラクターID
    public int currentLevel;             // 現在レベル
    public int currentExp;               // 現在経験値
    public bool isLocked;                // ロックフラグ
    public string obtainedDate;          // 獲得日時
    public int enhancementLevel;         // 強化レベル
    public bool[] unlockedSkills;        // 解放済みスキル
    public CharacterStats currentStats;  // 現在ステータス
}

[System.Serializable]
public class GachaSaveData
{
    public string gachaSystemId;         // ガチャシステムID
    public int upgradeLevel;             // アップグレードレベル
    public int remainingCeilingCount;    // 天井までの残り回数
    public int consecutiveCount;         // 連続回数
    public string lastPullTime;          // 最終実行時刻
    public bool[] purchasedUpgrades;     // 購入済みアップグレード
}

[System.Serializable]
public class PartyFormation
{
    public string formationName;         // 編成名
    public string[,] characterGrid;      // 5x5キャラクター配置 (characterId)
    public bool isActive;                // アクティブフラグ
    public string lastModified;          // 最終更新時刻
}
```

---

## JSONデータフォーマット

### セーブデータファイル構造

```json
{
    "playerSaveData": {
        "playerId": "player_12345",
        "playerName": "プレイヤー名",
        "playerLevel": 25,
        "totalPlayTime": 7200,
        "lastSaveTime": "2025-07-08T10:30:00Z",
        "saveDataVersion": 1,
        "currentGold": 15000,
        "ownedCharacters": [
            {
                "characterId": "char_001",
                "currentLevel": 10,
                "currentExp": 250,
                "isLocked": false,
                "obtainedDate": "2025-07-01T12:00:00Z",
                "enhancementLevel": 0,
                "unlockedSkills": [true, false, false],
                "currentStats": {
                    "health": 120,
                    "attack": 45,
                    "defense": 30,
                    "speed": 25,
                    "magic": 15,
                    "resistance": 20,
                    "accuracy": 85,
                    "critical": 10
                }
            }
        ],
        "gachaStatus": [
            {
                "gachaSystemId": "basic_gacha",
                "upgradeLevel": 2,
                "remainingCeilingCount": 50,
                "consecutiveCount": 5,
                "lastPullTime": "2025-07-08T09:15:00Z",
                "purchasedUpgrades": [true, true, false]
            }
        ]
    },
    "metadata": {
        "version": "1.0",
        "platform": "Android",
        "created": "2025-07-08T10:30:00Z"
    }
}
```

### 設定ファイル構造

```json
{
    "gameSettings": {
        "masterDataVersion": "1.0.0",
        "requiredUnityVersion": "2021.3.22f1",
        "supportedPlatforms": ["Android", "PC"],
        "defaultLanguage": "ja",
        "autoSaveInterval": 60
    },
    "balanceParameters": {
        "goldDropRate": {
            "baseRate": 1.0,
            "stageMultiplier": 1.1,
            "difficultyMultiplier": 1.2
        },
        "expGainRate": {
            "baseRate": 1.0,
            "levelPenalty": 0.95,
            "bonusThreshold": 10
        },
        "gachaRates": {
            "common": 70.0,
            "rare": 25.0,
            "epic": 4.5,
            "legendary": 0.5
        }
    }
}
```

---

## バランスパラメータ定義

### 経済バランス

#### ゴールド関連
```
初期ゴールド: 1,000
基本ガチャコスト: 100
アップグレードコスト: level * 500
売却価格: 基本価格 * rarityMultiplier * 0.3
```

#### 経験値関連
```
基本経験値テーブル: level^2 * 10
レベルアップ必要経験値: nextLevel * 100
合成時経験値変換: sellPrice * 2
```

### キャラクターバランス

#### レアリティ補正値
```
Common: 1.0
Rare: 1.5
Epic: 2.5
Legendary: 5.0
```

#### ステータス成長率
```
基本成長: baseStats * 0.1 per level
レアリティボーナス: growth * rarityMultiplier
最大レベル: 100 (全レアリティ共通)
```

### ガチャバランス

#### 基本排出率
```
Common: 70%
Rare: 25%
Epic: 4.5%
Legendary: 0.5%
```

#### 天井システム
```
天井回数: 100回
保証レアリティ: Epic以上
ピックアップ倍率: 2.0x
```

---

## データ整合性・バリデーション

### バリデーションルール

#### CharacterData
- characterId: 必須、一意、英数字のみ
- characterName: 必須、50文字以内
- rarity: 有効な列挙値
- baseStats: 全て正の値
- skills.Length == skillUnlockLevels.Length

#### GachaSystemData
- dropRates: 合計確率100%
- upgrades: レベル順でソート済み
- ceilingCount: 1以上1000以下

### データマイグレーション

#### バージョン1.0 → 1.1 (例)
```csharp
public void MigrateFrom1_0To1_1(PlayerSaveData data)
{
    // 新フィールドのデフォルト値設定
    if (data.saveDataVersion == 1)
    {
        data.maxCharacterSlots = 100;
        data.favoriteCharacters = new string[5];
        data.saveDataVersion = 2;
    }
}
```

---

## ファイル配置・命名規約

### ScriptableObject配置
```
Assets/Data/
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
```
Android: /Android/data/com.company.gatchaspire/files/
PC: %USERPROFILE%/AppData/LocalLow/CompanyName/GatchaSpire/

save_data.json          # メインセーブデータ
backup_save_data.json   # バックアップ
settings.json           # プレイヤー設定
statistics.json         # 統計データ
```

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