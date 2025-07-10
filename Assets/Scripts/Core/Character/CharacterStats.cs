using System;
using System.Collections.Generic;
using UnityEngine;

namespace GatchaSpire.Core.Character
{
    /// <summary>
    /// キャラクターのステータス管理構造体
    /// 基本ステータス、修正値、計算メソッドを含む
    /// </summary>
    [Serializable]
    public struct CharacterStats
    {
        [Header("基本ステータス")]
        [SerializeField] private int baseHP;
        [SerializeField] private int baseMP;
        [SerializeField] private int baseAttack;
        [SerializeField] private int baseDefense;
        [SerializeField] private int baseSpeed;
        [SerializeField] private int baseMagic;
        [SerializeField] private int baseResistance;
        [SerializeField] private int baseLuck;

        [Header("修正値")]
        [SerializeField] private Dictionary<StatType, int> statModifiers;

        // プロパティ（基本値のみ）
        public int BaseHP => baseHP;
        public int BaseMP => baseMP;
        public int BaseAttack => baseAttack;
        public int BaseDefense => baseDefense;
        public int BaseSpeed => baseSpeed;
        public int BaseMagic => baseMagic;
        public int BaseResistance => baseResistance;
        public int BaseLuck => baseLuck;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="hp">HP</param>
        /// <param name="mp">MP</param>
        /// <param name="attack">攻撃力</param>
        /// <param name="defense">防御力</param>
        /// <param name="speed">速度</param>
        /// <param name="magic">魔力</param>
        /// <param name="resistance">魔法防御</param>
        /// <param name="luck">運</param>
        public CharacterStats(int hp, int mp, int attack, int defense, int speed, int magic, int resistance, int luck)
        {
            baseHP = hp;
            baseMP = mp;
            baseAttack = attack;
            baseDefense = defense;
            baseSpeed = speed;
            baseMagic = magic;
            baseResistance = resistance;
            baseLuck = luck;
            statModifiers = new Dictionary<StatType, int>();
        }

        /// <summary>
        /// ステータス修正値を追加
        /// </summary>
        /// <param name="statType">ステータス種別</param>
        /// <param name="value">修正値</param>
        public void AddModifier(StatType statType, int value)
        {
            if (statModifiers == null)
                statModifiers = new Dictionary<StatType, int>();

            if (statModifiers.ContainsKey(statType))
                statModifiers[statType] += value;
            else
                statModifiers[statType] = value;
        }

        /// <summary>
        /// ステータス修正値を設定
        /// </summary>
        /// <param name="statType">ステータス種別</param>
        /// <param name="value">修正値</param>
        public void SetModifier(StatType statType, int value)
        {
            if (statModifiers == null)
                statModifiers = new Dictionary<StatType, int>();

            statModifiers[statType] = value;
        }

        /// <summary>
        /// ステータス修正値を削除
        /// </summary>
        /// <param name="statType">ステータス種別</param>
        public void RemoveModifier(StatType statType)
        {
            statModifiers?.Remove(statType);
        }

        /// <summary>
        /// 全ての修正値をクリア
        /// </summary>
        public void ClearModifiers()
        {
            statModifiers?.Clear();
        }

        /// <summary>
        /// 修正値を含む最終ステータスを取得
        /// </summary>
        /// <param name="statType">ステータス種別</param>
        /// <returns>最終ステータス値</returns>
        public int GetFinalStat(StatType statType)
        {
            int baseStat = GetBaseStat(statType);
            int modifier = GetModifier(statType);
            return Mathf.Max(0, baseStat + modifier);
        }

        /// <summary>
        /// 基本ステータス値を取得
        /// </summary>
        /// <param name="statType">ステータス種別</param>
        /// <returns>基本ステータス値</returns>
        public int GetBaseStat(StatType statType)
        {
            return statType switch
            {
                StatType.HP => baseHP,
                StatType.MP => baseMP,
                StatType.Attack => baseAttack,
                StatType.Defense => baseDefense,
                StatType.Speed => baseSpeed,
                StatType.Magic => baseMagic,
                StatType.Resistance => baseResistance,
                StatType.Luck => baseLuck,
                _ => 0
            };
        }

        /// <summary>
        /// ステータス修正値を取得
        /// </summary>
        /// <param name="statType">ステータス種別</param>
        /// <returns>修正値</returns>
        public int GetModifier(StatType statType)
        {
            if (statModifiers == null)
                return 0;

            return statModifiers.TryGetValue(statType, out int value) ? value : 0;
        }

        /// <summary>
        /// 基本ステータス値を設定
        /// </summary>
        /// <param name="statType">ステータス種別</param>
        /// <param name="value">設定値</param>
        public void SetBaseStat(StatType statType, int value)
        {
            switch (statType)
            {
                case StatType.HP:
                    baseHP = value;
                    break;
                case StatType.MP:
                    baseMP = value;
                    break;
                case StatType.Attack:
                    baseAttack = value;
                    break;
                case StatType.Defense:
                    baseDefense = value;
                    break;
                case StatType.Speed:
                    baseSpeed = value;
                    break;
                case StatType.Magic:
                    baseMagic = value;
                    break;
                case StatType.Resistance:
                    baseResistance = value;
                    break;
                case StatType.Luck:
                    baseLuck = value;
                    break;
            }
        }

        /// <summary>
        /// レベルに基づくステータス成長を適用
        /// </summary>
        /// <param name="level">現在のレベル</param>
        /// <param name="growthRates">成長率（ステータス種別ごと）</param>
        public void ApplyLevelGrowth(int level, Dictionary<StatType, float> growthRates)
        {
            if (growthRates == null || level <= 1)
                return;

            int levelGrowth = level - 1;

            foreach (var growth in growthRates)
            {
                StatType statType = growth.Key;
                float rate = growth.Value;
                
                int baseValue = GetBaseStat(statType);
                int growthValue = Mathf.RoundToInt(baseValue * rate * levelGrowth);
                
                AddModifier(statType, growthValue);
            }
        }

        /// <summary>
        /// ステータス合計値を計算
        /// </summary>
        /// <returns>全ステータスの合計</returns>
        public int GetTotalStats()
        {
            int total = 0;
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                total += GetFinalStat(statType);
            }
            return total;
        }

        /// <summary>
        /// 戦闘力を計算
        /// </summary>
        /// <returns>戦闘力値</returns>
        public int CalculateBattlePower()
        {
            // 各ステータスに重み付けして戦闘力を計算
            float battlePower = 0f;
            
            battlePower += GetFinalStat(StatType.HP) * 1.0f;
            battlePower += GetFinalStat(StatType.MP) * 0.8f;
            battlePower += GetFinalStat(StatType.Attack) * 1.5f;
            battlePower += GetFinalStat(StatType.Defense) * 1.2f;
            battlePower += GetFinalStat(StatType.Speed) * 1.1f;
            battlePower += GetFinalStat(StatType.Magic) * 1.3f;
            battlePower += GetFinalStat(StatType.Resistance) * 1.0f;
            battlePower += GetFinalStat(StatType.Luck) * 0.5f;

            return Mathf.RoundToInt(battlePower);
        }

        /// <summary>
        /// 別のステータスと比較
        /// </summary>
        /// <param name="other">比較対象</param>
        /// <returns>戦闘力の差</returns>
        public int CompareBattlePower(CharacterStats other)
        {
            return CalculateBattlePower() - other.CalculateBattlePower();
        }

        /// <summary>
        /// ステータスが有効かチェック
        /// </summary>
        /// <returns>有効な場合true</returns>
        public bool IsValid()
        {
            return baseHP > 0 && baseMP >= 0 && baseAttack >= 0 && baseDefense >= 0;
        }

        /// <summary>
        /// デバッグ用文字列
        /// </summary>
        /// <returns>ステータス情報の文字列</returns>
        public override string ToString()
        {
            return $"HP:{GetFinalStat(StatType.HP)} MP:{GetFinalStat(StatType.MP)} " +
                   $"ATK:{GetFinalStat(StatType.Attack)} DEF:{GetFinalStat(StatType.Defense)} " +
                   $"SPD:{GetFinalStat(StatType.Speed)} MAG:{GetFinalStat(StatType.Magic)} " +
                   $"RES:{GetFinalStat(StatType.Resistance)} LUK:{GetFinalStat(StatType.Luck)} " +
                   $"(BP:{CalculateBattlePower()})";
        }

        /// <summary>
        /// 詳細なステータス情報を取得
        /// </summary>
        /// <returns>詳細情報文字列</returns>
        public string GetDetailedInfo()
        {
            var info = "=== ステータス詳細 ===\n";
            
            foreach (StatType statType in Enum.GetValues(typeof(StatType)))
            {
                int baseStat = GetBaseStat(statType);
                int modifier = GetModifier(statType);
                int finalStat = GetFinalStat(statType);
                
                info += $"{statType}: {baseStat}";
                if (modifier != 0)
                    info += $" + {modifier}";
                info += $" = {finalStat}\n";
            }
            
            info += $"総戦闘力: {CalculateBattlePower()}";
            return info;
        }
    }
}