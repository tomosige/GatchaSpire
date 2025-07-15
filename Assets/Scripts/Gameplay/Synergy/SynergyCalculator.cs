using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GatchaSpire.Core.Character;

namespace GatchaSpire.Gameplay.Synergy
{
    /// <summary>
    /// シナジー計算システム
    /// パーティ構成からシナジー発動状況を計算する
    /// </summary>
    public class SynergyCalculator
    {
        /// <summary>
        /// シナジー計算結果
        /// </summary>
        public class SynergyCalculationResult
        {
            public SynergyData synergyData;
            public SynergyLevel activeSynergyLevel;
            public List<Character> synergyCharacters;
            public int characterCount;
            public bool isActive;
            
            public SynergyCalculationResult(SynergyData data)
            {
                synergyData = data;
                synergyCharacters = new List<Character>();
                characterCount = 0;
                isActive = false;
                activeSynergyLevel = null;
            }
        }
        
        /// <summary>
        /// 利用可能なシナジーデータリスト
        /// </summary>
        private List<SynergyData> availableSynergies;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="synergies">利用可能なシナジーデータ</param>
        public SynergyCalculator(List<SynergyData> synergies = null)
        {
            availableSynergies = synergies ?? new List<SynergyData>();
        }
        
        /// <summary>
        /// シナジーデータを設定
        /// </summary>
        /// <param name="synergies">シナジーデータリスト</param>
        public void SetSynergies(List<SynergyData> synergies)
        {
            availableSynergies = synergies ?? new List<SynergyData>();
        }
        
        /// <summary>
        /// パーティ構成からシナジーを計算
        /// </summary>
        /// <param name="characters">パーティキャラクターリスト</param>
        /// <returns>シナジー計算結果リスト</returns>
        public List<SynergyCalculationResult> CalculateSynergies(List<Character> characters)
        {
            var results = new List<SynergyCalculationResult>();
            
            if (characters == null || availableSynergies == null)
            {
                return results;
            }
            
            // 各シナジーについて計算
            foreach (var synergyData in availableSynergies)
            {
                if (synergyData == null || !synergyData.IsActive)
                {
                    continue;
                }
                
                var result = CalculateSingleSynergy(synergyData, characters);
                results.Add(result);
            }
            
            // 優先度でソート
            results.Sort((a, b) => b.synergyData.Priority.CompareTo(a.synergyData.Priority));
            
            return results;
        }
        
        /// <summary>
        /// 単一シナジーの計算
        /// </summary>
        /// <param name="synergyData">シナジーデータ</param>
        /// <param name="characters">キャラクターリスト</param>
        /// <returns>計算結果</returns>
        private SynergyCalculationResult CalculateSingleSynergy(SynergyData synergyData, List<Character> characters)
        {
            var result = new SynergyCalculationResult(synergyData);
            
            // シナジー対象キャラクターを抽出
            result.synergyCharacters = GetSynergyCharacters(synergyData, characters);
            result.characterCount = result.synergyCharacters.Count;
            
            // 発動可能な最大シナジーレベルを取得
            result.activeSynergyLevel = synergyData.GetMaxActiveSynergyLevel(result.characterCount);
            result.isActive = result.activeSynergyLevel != null;
            
            return result;
        }
        
        /// <summary>
        /// シナジー対象キャラクターを取得
        /// </summary>
        /// <param name="synergyData">シナジーデータ</param>
        /// <param name="characters">全キャラクターリスト</param>
        /// <returns>シナジー対象キャラクターリスト</returns>
        private List<Character> GetSynergyCharacters(SynergyData synergyData, List<Character> characters)
        {
            var synergyCharacters = new List<Character>();
            
            foreach (var character in characters)
            {
                if (character == null || character.CharacterData == null)
                {
                    continue;
                }
                
                // キャラクターがこのシナジーに該当するかチェック
                if (CharacterHasSynergy(character, synergyData))
                {
                    synergyCharacters.Add(character);
                }
            }
            
            return synergyCharacters;
        }
        
        /// <summary>
        /// キャラクターが指定シナジーを持つかチェック
        /// </summary>
        /// <param name="character">キャラクター</param>
        /// <param name="synergyData">シナジーデータ</param>
        /// <returns>該当する場合true</returns>
        private bool CharacterHasSynergy(Character character, SynergyData synergyData)
        {
            // 現在は CharacterData にシナジー情報がないため、暫定的に名前ベースで判定
            // TODO: CharacterData.synergyTypes 実装後に正式な判定に変更
            
            var characterName = character.CharacterData.CharacterName;
            var synergyId = synergyData.SynergyId;
            
            // 暫定：キャラクター名にシナジーIDが含まれているかで判定
            // 例: "TestRaceA_Knight" には "testracea" シナジーが含まれる
            return characterName.ToLower().Contains(synergyId.ToLower());
        }
        
        /// <summary>
        /// 特定シナジーの発動状況を取得
        /// </summary>
        /// <param name="synergyId">シナジーID</param>
        /// <param name="characters">キャラクターリスト</param>
        /// <returns>シナジー計算結果、見つからない場合はnull</returns>
        public SynergyCalculationResult GetSynergyResult(string synergyId, List<Character> characters)
        {
            if (string.IsNullOrEmpty(synergyId) || characters == null)
            {
                return null;
            }
            
            var synergyData = availableSynergies.FirstOrDefault(s => s.SynergyId == synergyId);
            if (synergyData == null)
            {
                return null;
            }
            
            return CalculateSingleSynergy(synergyData, characters);
        }
        
        /// <summary>
        /// アクティブなシナジー結果のみを取得
        /// </summary>
        /// <param name="characters">キャラクターリスト</param>
        /// <returns>アクティブなシナジー計算結果リスト</returns>
        public List<SynergyCalculationResult> GetActiveSynergies(List<Character> characters)
        {
            return CalculateSynergies(characters).Where(r => r.isActive).ToList();
        }
        
        /// <summary>
        /// シナジープレビュー（1キャラクター追加時の状況）
        /// </summary>
        /// <param name="currentCharacters">現在のキャラクターリスト</param>
        /// <param name="newCharacter">追加予定キャラクター</param>
        /// <returns>追加後のシナジー計算結果リスト</returns>
        public List<SynergyCalculationResult> PreviewSynergyWithNewCharacter(List<Character> currentCharacters, Character newCharacter)
        {
            if (newCharacter == null)
            {
                return CalculateSynergies(currentCharacters);
            }
            
            var previewCharacters = new List<Character>(currentCharacters ?? new List<Character>());
            previewCharacters.Add(newCharacter);
            
            return CalculateSynergies(previewCharacters);
        }
        
        /// <summary>
        /// デバッグ用：計算結果を文字列で出力
        /// </summary>
        /// <param name="results">計算結果リスト</param>
        /// <returns>デバッグ文字列</returns>
        public string GetDebugInfo(List<SynergyCalculationResult> results)
        {
            var info = "=== シナジー計算結果 ===\n";
            
            if (results == null || results.Count == 0)
            {
                info += "アクティブなシナジーはありません\n";
                return info;
            }
            
            foreach (var result in results)
            {
                info += $"{result.synergyData.DisplayName}: ";
                if (result.isActive)
                {
                    info += $"発動中 ({result.characterCount}体 - レベル{result.activeSynergyLevel.RequiredCount})\n";
                }
                else
                {
                    info += $"未発動 ({result.characterCount}体)\n";
                }
            }
            
            return info;
        }
    }
}