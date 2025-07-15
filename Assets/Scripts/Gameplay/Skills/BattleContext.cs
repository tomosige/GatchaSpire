using UnityEngine;
using System.Collections.Generic;
using GatchaSpire.Core.Character;
using GatchaSpire.Gameplay.Battle;

namespace GatchaSpire.Gameplay.Skills
{
    /// <summary>
    /// スキル効果適用時の戦闘コンテキスト
    /// 軽量実装：TestBasicSkillEffectsに必要な最小限の情報のみ
    /// 将来のPhase II戦闘システム統合時に拡張予定
    /// </summary>
    [System.Serializable]
    public class BattleContext
    {
        [Header("戦闘管理")]
        [SerializeField] private BattleManager battleManager;
        [SerializeField] private float currentTime;

        [Header("戦闘参加者")]
        [SerializeField] private List<CombatCharacter> allCombatCharacters = new List<CombatCharacter>();

        /// <summary>
        /// 戦闘マネージャー（null可、テスト時は不要）
        /// </summary>
        public BattleManager BattleManager
        {
            get => battleManager;
            set => battleManager = value;
        }

        /// <summary>
        /// 現在の戦闘時間
        /// </summary>
        public float CurrentTime
        {
            get => currentTime;
            set => currentTime = value;
        }

        /// <summary>
        /// 全戦闘参加キャラクター
        /// </summary>
        public List<CombatCharacter> AllCombatCharacters
        {
            get => allCombatCharacters;
            set => allCombatCharacters = value ?? new List<CombatCharacter>();
        }

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
        public BattleContext()
        {
            battleManager = null;
            currentTime = 0f;
            allCombatCharacters = new List<CombatCharacter>();
        }

        /// <summary>
        /// 基本パラメータ指定コンストラクタ
        /// </summary>
        /// <param name="time">現在時刻</param>
        public BattleContext(float time) : this()
        {
            currentTime = time;
        }

        /// <summary>
        /// 完全パラメータ指定コンストラクタ
        /// </summary>
        /// <param name="manager">戦闘マネージャー</param>
        /// <param name="time">現在時刻</param>
        /// <param name="combatCharacters">戦闘参加キャラクター</param>
        public BattleContext(BattleManager manager, float time, List<CombatCharacter> combatCharacters) : this()
        {
            battleManager = manager;
            currentTime = time;
            allCombatCharacters = combatCharacters ?? new List<CombatCharacter>();
        }

        /// <summary>
        /// 指定キャラクターの戦闘データを取得
        /// </summary>
        /// <param name="character">対象キャラクター</param>
        /// <returns>戦闘データ（見つからない場合null）</returns>
        public CombatCharacter GetCombatCharacter(Character character)
        {
            if (character == null || allCombatCharacters == null)
                return null;

            foreach (var combatChar in allCombatCharacters)
            {
                if (combatChar.BaseCharacter == character)
                    return combatChar;
            }

            return null;
        }

        /// <summary>
        /// プレイヤーキャラクターの一覧を取得
        /// </summary>
        /// <returns>プレイヤーキャラクター一覧</returns>
        public List<CombatCharacter> GetPlayerCharacters()
        {
            var playerChars = new List<CombatCharacter>();
            
            if (allCombatCharacters != null)
            {
                foreach (var combatChar in allCombatCharacters)
                {
                    if (combatChar.IsPlayerCharacter)
                        playerChars.Add(combatChar);
                }
            }
            
            return playerChars;
        }

        /// <summary>
        /// 敵キャラクターの一覧を取得
        /// </summary>
        /// <returns>敵キャラクター一覧</returns>
        public List<CombatCharacter> GetEnemyCharacters()
        {
            var enemyChars = new List<CombatCharacter>();
            
            if (allCombatCharacters != null)
            {
                foreach (var combatChar in allCombatCharacters)
                {
                    if (!combatChar.IsPlayerCharacter)
                        enemyChars.Add(combatChar);
                }
            }
            
            return enemyChars;
        }

        /// <summary>
        /// 戦闘コンテキストの妥当性確認
        /// </summary>
        /// <returns>妥当性</returns>
        public bool IsValid()
        {
            // 最小限の要件：現在時刻が負でないこと
            return currentTime >= 0f;
        }

        /// <summary>
        /// 戦闘コンテキストの詳細情報
        /// </summary>
        /// <returns>詳細情報文字列</returns>
        public string GetDetailedInfo()
        {
            var info = $"=== BattleContext ===\n";
            info += $"戦闘時間: {currentTime:F2}秒\n";
            info += $"戦闘マネージャー: {(battleManager != null ? "あり" : "なし")}\n";
            info += $"戦闘参加者数: {(allCombatCharacters?.Count ?? 0)}体\n";
            
            if (allCombatCharacters != null && allCombatCharacters.Count > 0)
            {
                var playerCount = GetPlayerCharacters().Count;
                var enemyCount = GetEnemyCharacters().Count;
                info += $"  - プレイヤー: {playerCount}体\n";
                info += $"  - 敵: {enemyCount}体\n";
            }
            
            return info;
        }

        /// <summary>
        /// 戦闘コンテキストの文字列表現
        /// </summary>
        /// <returns>簡潔な情報</returns>
        public override string ToString()
        {
            int charCount = allCombatCharacters?.Count ?? 0;
            return $"BattleContext(時間:{currentTime:F1}秒, キャラ:{charCount}体)";
        }
    }
}