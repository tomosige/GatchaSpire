using System;

namespace GatchaSpire.Core.Character
{
    /// <summary>
    /// キャラクターのレアリティ
    /// </summary>
    [Serializable]
    public enum CharacterRarity
    {
        Common = 1,     // コモン
        Uncommon = 2,   // アンコモン
        Rare = 3,       // レア
        Epic = 4,       // エピック
        Legendary = 5   // レジェンダリー
    }

    /// <summary>
    /// キャラクターの種族
    /// </summary>
    [Serializable]
    public enum CharacterRace
    {
        Human = 1,      // 人間
        Elf = 2,        // エルフ
        Dwarf = 3,      // ドワーフ
        Orc = 4,        // オーク
        Beast = 5,      // 獣人
        Dragon = 6,     // ドラゴン
        Undead = 7,     // アンデッド
        Spirit = 8,     // 精霊
        Demon = 9,      // 悪魔
        Angel = 10      // 天使
    }

    /// <summary>
    /// キャラクターのクラス（職業）
    /// </summary>
    [Serializable]
    public enum CharacterClass
    {
        // 前衛系
        Warrior = 1,    // 戦士
        Knight = 2,     // 騎士
        Berserker = 3,  // バーサーカー
        Paladin = 4,    // パラディン
        
        // 後衛系
        Mage = 5,       // 魔法使い
        Archer = 6,     // 弓使い
        Priest = 7,     // 僧侶
        Wizard = 8,     // ウィザード
        
        // 中衛系
        Rogue = 9,      // 盗賊
        Assassin = 10,  // アサシン
        Ranger = 11,    // レンジャー
        Bard = 12,      // 吟遊詩人
        
        // 特殊系
        Summoner = 13,  // サモナー
        Necromancer = 14, // ネクロマンサー
        Druid = 15,     // ドルイド
        Monk = 16       // モンク
    }

    /// <summary>
    /// キャラクターの属性
    /// </summary>
    [Serializable]
    public enum CharacterElement
    {
        None = 0,       // 無属性
        Fire = 1,       // 火
        Water = 2,      // 水
        Earth = 3,      // 土
        Air = 4,        // 風
        Light = 5,      // 光
        Dark = 6,       // 闇
        Lightning = 7,  // 雷
        Ice = 8         // 氷
    }

    /// <summary>
    /// キャラクターの役割
    /// </summary>
    [Serializable]
    public enum CharacterRole
    {
        Tank = 1,       // タンク
        DPS = 2,        // ダメージディーラー
        Healer = 3,     // ヒーラー
        Support = 4,    // サポート
        Hybrid = 5      // 複合型
    }

    /// <summary>
    /// ステータス種別
    /// </summary>
    [Serializable]
    public enum StatType
    {
        HP = 1,         // ヒットポイント
        MP = 2,         // マジックポイント
        Attack = 3,     // 攻撃力
        Defense = 4,    // 防御力
        Speed = 5,      // 速度
        Magic = 6,      // 魔力
        Resistance = 7, // 魔法防御
        Luck = 8,       // 運
        Critical = 9,   // クリティカル率
        Accuracy = 10   // 命中率
    }
}