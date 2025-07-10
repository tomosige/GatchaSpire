using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using GatchaSpire.Core.Systems;

namespace GatchaSpire.Core.Character
{
    /// <summary>
    /// キャラクターデータベース管理クラス
    /// データ読み込み、検索・フィルタ、データ整合性チェックを提供
    /// </summary>
    public class CharacterDatabase : GameSystemBase
    {
        [Header("データベース設定")]
        [SerializeField] private List<CharacterData> characterDataList = new List<CharacterData>();
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private bool validateOnLoad = true;

        // ランタイムデータ
        private Dictionary<int, CharacterData> characterById = new Dictionary<int, CharacterData>();
        private Dictionary<string, CharacterData> characterByName = new Dictionary<string, CharacterData>();
        private Dictionary<CharacterRarity, List<CharacterData>> charactersByRarity = new Dictionary<CharacterRarity, List<CharacterData>>();
        private Dictionary<CharacterClass, List<CharacterData>> charactersByClass = new Dictionary<CharacterClass, List<CharacterData>>();
        private Dictionary<CharacterRace, List<CharacterData>> charactersByRace = new Dictionary<CharacterRace, List<CharacterData>>();

        // シングルトンパターン
        private static CharacterDatabase _instance;
        public static CharacterDatabase Instance => _instance;

        protected override string SystemName => "CharacterDatabase";

        // プロパティ
        public int CharacterCount => characterDataList.Count;
        public List<CharacterData> AllCharacters => new List<CharacterData>(characterDataList);

        private void Awake()
        {
            OnAwake();
        }

        protected override void OnSystemInitialize()
        {
            // シングルトン設定
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                ReportWarning("CharacterDatabaseの複数インスタンスを検出しました。重複インスタンスを破棄します。");
                Destroy(gameObject);
                return;
            }
            
            if (autoLoadOnStart)
            {
                LoadAllCharacterData();
            }

            ReportInfo("キャラクターデータベースが初期化されました");
        }

        /// <summary>
        /// 全キャラクターデータを読み込み
        /// </summary>
        public void LoadAllCharacterData()
        {
            try
            {
                ReportInfo("キャラクターデータの読み込みを開始します");

                // Resourcesフォルダからも読み込み
                var resourceCharacters = Resources.LoadAll<CharacterData>("Characters");
                foreach (var character in resourceCharacters)
                {
                    if (!characterDataList.Contains(character))
                    {
                        characterDataList.Add(character);
                    }
                }

                BuildIndices();

                if (validateOnLoad)
                {
                    ValidateAllData();
                }

                ReportInfo($"キャラクターデータの読み込みが完了しました ({characterDataList.Count}体)");
            }
            catch (System.Exception e)
            {
                ReportError($"キャラクターデータの読み込みに失敗しました: {e.Message}", e);
            }
        }

        /// <summary>
        /// インデックスを構築
        /// </summary>
        private void BuildIndices()
        {
            characterById.Clear();
            characterByName.Clear();
            charactersByRarity.Clear();
            charactersByClass.Clear();
            charactersByRace.Clear();

            // レアリティ別リストの初期化
            foreach (CharacterRarity rarity in System.Enum.GetValues(typeof(CharacterRarity)))
            {
                charactersByRarity[rarity] = new List<CharacterData>();
            }

            // クラス別リストの初期化
            foreach (CharacterClass charClass in System.Enum.GetValues(typeof(CharacterClass)))
            {
                charactersByClass[charClass] = new List<CharacterData>();
            }

            // 種族別リストの初期化
            foreach (CharacterRace race in System.Enum.GetValues(typeof(CharacterRace)))
            {
                charactersByRace[race] = new List<CharacterData>();
            }

            // インデックス構築
            foreach (var character in characterDataList)
            {
                if (character == null)
                    continue;

                // ID別インデックス
                if (character.CharacterId > 0)
                {
                    if (characterById.ContainsKey(character.CharacterId))
                    {
                        ReportWarning($"重複したキャラクターID: {character.CharacterId} ({character.CharacterName})");
                    }
                    else
                    {
                        characterById[character.CharacterId] = character;
                    }
                }

                // 名前別インデックス
                if (!string.IsNullOrEmpty(character.CharacterName))
                {
                    if (characterByName.ContainsKey(character.CharacterName))
                    {
                        ReportWarning($"重複したキャラクター名: {character.CharacterName}");
                    }
                    else
                    {
                        characterByName[character.CharacterName] = character;
                    }
                }

                // 分類別インデックス
                charactersByRarity[character.Rarity].Add(character);
                charactersByClass[character.CharacterClass].Add(character);
                charactersByRace[character.Race].Add(character);
            }
        }

        /// <summary>
        /// IDでキャラクターデータを取得
        /// </summary>
        /// <param name="id">キャラクターID</param>
        /// <returns>キャラクターデータ</returns>
        public CharacterData GetCharacterById(int id)
        {
            return characterById.TryGetValue(id, out var character) ? character : null;
        }

        /// <summary>
        /// 名前でキャラクターデータを取得
        /// </summary>
        /// <param name="name">キャラクター名</param>
        /// <returns>キャラクターデータ</returns>
        public CharacterData GetCharacterByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return characterByName.TryGetValue(name, out var character) ? character : null;
        }

        /// <summary>
        /// レアリティでフィルタリング
        /// </summary>
        /// <param name="rarity">レアリティ</param>
        /// <returns>該当キャラクターリスト</returns>
        public List<CharacterData> GetCharactersByRarity(CharacterRarity rarity)
        {
            return charactersByRarity.TryGetValue(rarity, out var characters) ? 
                new List<CharacterData>(characters) : new List<CharacterData>();
        }

        /// <summary>
        /// クラスでフィルタリング
        /// </summary>
        /// <param name="characterClass">キャラクタークラス</param>
        /// <returns>該当キャラクターリスト</returns>
        public List<CharacterData> GetCharactersByClass(CharacterClass characterClass)
        {
            return charactersByClass.TryGetValue(characterClass, out var characters) ? 
                new List<CharacterData>(characters) : new List<CharacterData>();
        }

        /// <summary>
        /// 種族でフィルタリング
        /// </summary>
        /// <param name="race">種族</param>
        /// <returns>該当キャラクターリスト</returns>
        public List<CharacterData> GetCharactersByRace(CharacterRace race)
        {
            return charactersByRace.TryGetValue(race, out var characters) ? 
                new List<CharacterData>(characters) : new List<CharacterData>();
        }

        /// <summary>
        /// 複合条件でフィルタリング
        /// </summary>
        /// <param name="filter">フィルタ条件</param>
        /// <returns>該当キャラクターリスト</returns>
        public List<CharacterData> GetCharactersByFilter(CharacterFilter filter)
        {
            var results = characterDataList.AsEnumerable();

            if (filter.Rarities != null && filter.Rarities.Count > 0)
            {
                results = results.Where(c => filter.Rarities.Contains(c.Rarity));
            }

            if (filter.Classes != null && filter.Classes.Count > 0)
            {
                results = results.Where(c => filter.Classes.Contains(c.CharacterClass));
            }

            if (filter.Races != null && filter.Races.Count > 0)
            {
                results = results.Where(c => filter.Races.Contains(c.Race));
            }

            if (filter.Elements != null && filter.Elements.Count > 0)
            {
                results = results.Where(c => filter.Elements.Contains(c.Element));
            }

            if (filter.Roles != null && filter.Roles.Count > 0)
            {
                results = results.Where(c => filter.Roles.Contains(c.Role));
            }

            if (filter.MinBattlePower > 0)
            {
                results = results.Where(c => c.CalculateBaseBattlePower() >= filter.MinBattlePower);
            }

            if (filter.MaxBattlePower > 0)
            {
                results = results.Where(c => c.CalculateBaseBattlePower() <= filter.MaxBattlePower);
            }

            if (!string.IsNullOrEmpty(filter.NameContains))
            {
                results = results.Where(c => c.CharacterName.ToLower().Contains(filter.NameContains.ToLower()));
            }

            return results.ToList();
        }

        /// <summary>
        /// ランダムなキャラクターを取得
        /// </summary>
        /// <param name="count">取得数</param>
        /// <param name="filter">フィルタ条件</param>
        /// <returns>ランダムキャラクターリスト</returns>
        public List<CharacterData> GetRandomCharacters(int count, CharacterFilter filter = null)
        {
            var candidates = filter != null ? GetCharactersByFilter(filter) : characterDataList;
            
            if (candidates.Count == 0)
                return new List<CharacterData>();

            var shuffled = candidates.OrderBy(x => Random.value).ToList();
            return shuffled.Take(count).ToList();
        }

        /// <summary>
        /// レアリティ確率に基づくランダム取得
        /// </summary>
        /// <param name="rarityWeights">レアリティ別重み</param>
        /// <returns>ランダムキャラクター</returns>
        public CharacterData GetRandomCharacterByRarity(Dictionary<CharacterRarity, float> rarityWeights)
        {
            if (rarityWeights == null || rarityWeights.Count == 0)
                return null;

            // 重み付きランダム選択
            float totalWeight = rarityWeights.Values.Sum();
            float randomValue = Random.value * totalWeight;
            float currentWeight = 0f;

            foreach (var weight in rarityWeights)
            {
                currentWeight += weight.Value;
                if (randomValue <= currentWeight)
                {
                    var candidates = GetCharactersByRarity(weight.Key);
                    if (candidates.Count > 0)
                    {
                        return candidates[Random.Range(0, candidates.Count)];
                    }
                }
            }

            // フォールバック
            return characterDataList.Count > 0 ? characterDataList[Random.Range(0, characterDataList.Count)] : null;
        }

        /// <summary>
        /// 全データの整合性チェック
        /// </summary>
        public void ValidateAllData()
        {
            int errorCount = 0;
            int warningCount = 0;

            ReportInfo("データ整合性チェックを開始します");

            foreach (var character in characterDataList)
            {
                if (character == null)
                {
                    errorCount++;
                    ReportError("nullのキャラクターデータが含まれています");
                    continue;
                }

                var validation = character.Validate();
                if (validation.HasErrors)
                {
                    errorCount += validation.Errors.Count;
                    ReportError($"{character.CharacterName}: {string.Join(", ", validation.Errors)}");
                }

                if (validation.HasWarnings)
                {
                    warningCount += validation.Warnings.Count;
                    ReportWarning($"{character.CharacterName}: {string.Join(", ", validation.Warnings)}");
                }
            }

            var summary = $"データ整合性チェック完了: {errorCount}個のエラー, {warningCount}個の警告";
            if (errorCount > 0)
                ReportError(summary);
            else if (warningCount > 0)
                ReportWarning(summary);
            else
                ReportInfo(summary);
        }

        /// <summary>
        /// データベース統計情報を取得
        /// </summary>
        /// <returns>統計情報</returns>
        public DatabaseStatistics GetStatistics()
        {
            var stats = new DatabaseStatistics();
            
            stats.TotalCharacters = characterDataList.Count;
            
            // レアリティ別統計
            foreach (CharacterRarity rarity in System.Enum.GetValues(typeof(CharacterRarity)))
            {
                stats.CharactersByRarity[rarity] = GetCharactersByRarity(rarity).Count;
            }

            // クラス別統計
            foreach (CharacterClass charClass in System.Enum.GetValues(typeof(CharacterClass)))
            {
                stats.CharactersByClass[charClass] = GetCharactersByClass(charClass).Count;
            }

            // 戦闘力統計
            if (characterDataList.Count > 0)
            {
                var battlePowers = characterDataList.Select(c => c.CalculateBaseBattlePower()).ToList();
                stats.MinBattlePower = battlePowers.Min();
                stats.MaxBattlePower = battlePowers.Max();
                stats.AverageBattlePower = battlePowers.Average();
            }

            return stats;
        }

        /// <summary>
        /// デバッグ情報を取得
        /// </summary>
        /// <returns>デバッグ情報</returns>
        public string GetDebugInfo()
        {
            var stats = GetStatistics();
            var info = "=== キャラクターデータベース情報 ===\n";
            info += $"総キャラクター数: {stats.TotalCharacters}\n";
            info += $"戦闘力範囲: {stats.MinBattlePower} - {stats.MaxBattlePower}\n";
            info += $"平均戦闘力: {stats.AverageBattlePower:F1}\n";
            
            info += "\n=== レアリティ別分布 ===\n";
            foreach (var rarity in stats.CharactersByRarity)
            {
                info += $"{rarity.Key}: {rarity.Value}体\n";
            }

            return info;
        }
    }

    /// <summary>
    /// キャラクター検索フィルタ
    /// </summary>
    [System.Serializable]
    public class CharacterFilter
    {
        public List<CharacterRarity> Rarities = new List<CharacterRarity>();
        public List<CharacterClass> Classes = new List<CharacterClass>();
        public List<CharacterRace> Races = new List<CharacterRace>();
        public List<CharacterElement> Elements = new List<CharacterElement>();
        public List<CharacterRole> Roles = new List<CharacterRole>();
        public int MinBattlePower = 0;
        public int MaxBattlePower = 0;
        public string NameContains = "";
    }

    /// <summary>
    /// データベース統計情報
    /// </summary>
    [System.Serializable]
    public class DatabaseStatistics
    {
        public int TotalCharacters = 0;
        public Dictionary<CharacterRarity, int> CharactersByRarity = new Dictionary<CharacterRarity, int>();
        public Dictionary<CharacterClass, int> CharactersByClass = new Dictionary<CharacterClass, int>();
        public int MinBattlePower = 0;
        public int MaxBattlePower = 0;
        public double AverageBattlePower = 0;
    }
}