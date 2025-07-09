using UnityEngine;
using System.Collections.Generic;

namespace GatchaSpire.Core
{
    /// <summary>
    /// 開発用設定ScriptableObject
    /// デバッグ、パフォーマンス、バランス調整のための設定を一元管理
    /// </summary>
    [CreateAssetMenu(fileName = "DevelopmentSettings", menuName = "GatchaSpire/Settings/Development Settings")]
    public class DevelopmentSettings : ScriptableObject, IValidatable
    {
        [Header("デバッグ設定")]
        [SerializeField] private bool enableAllDebugLogs = true;
        [SerializeField] private bool showSystemHealth = false;
        [SerializeField] private bool pauseOnCriticalError = true;
        [SerializeField] private bool enableGodMode = false;
        [SerializeField] private bool enableDebugCommands = false;

        [Header("パフォーマンス設定")]
        [SerializeField] private bool skipAnimations = false;
        [SerializeField] private bool fastBattles = false;
        [SerializeField] private bool disableParticles = false;
        [SerializeField] private int targetFrameRate = 60;

        [Header("バランステスト設定")]
        [SerializeField, Range(0.1f, 10.0f)] private float globalGoldMultiplier = 1.0f;
        [SerializeField, Range(0.1f, 10.0f)] private float globalExpMultiplier = 1.0f;
        [SerializeField, Range(0.1f, 10.0f)] private float battleSpeedMultiplier = 1.0f;
        [SerializeField, Range(0.1f, 5.0f)] private float gachaRateMultiplier = 1.0f;

        [Header("チート設定")]
        [SerializeField] private bool unlockAllCharacters = false;
        [SerializeField] private bool infiniteGold = false;
        [SerializeField] private bool maxGachaLevel = false;
        [SerializeField] private bool skipTutorial = false;
        [SerializeField] private bool noBachaCost = false;


        [Header("UI設定")]
        [SerializeField] private bool showDebugUI = false;
        [SerializeField] private bool showPerformanceStats = false;
        [SerializeField] private bool enableQuickActions = false;

        // プロパティ
        public bool EnableAllDebugLogs => enableAllDebugLogs;
        public bool ShowSystemHealth => showSystemHealth;
        public bool PauseOnCriticalError => pauseOnCriticalError;
        public bool EnableGodMode => enableGodMode;
        public bool EnableDebugCommands => enableDebugCommands;

        public bool SkipAnimations => skipAnimations;
        public bool FastBattles => fastBattles;
        public bool DisableParticles => disableParticles;
        public int TargetFrameRate => targetFrameRate;

        public float GlobalGoldMultiplier => globalGoldMultiplier;
        public float GlobalExpMultiplier => globalExpMultiplier;
        public float BattleSpeedMultiplier => battleSpeedMultiplier;
        public float GachaRateMultiplier => gachaRateMultiplier;

        public bool UnlockAllCharacters => unlockAllCharacters;
        public bool InfiniteGold => infiniteGold;
        public bool MaxGachaLevel => maxGachaLevel;
        public bool SkipTutorial => skipTutorial;
        public bool NoGachaCost => noBachaCost;

        public bool ShowDebugUI => showDebugUI;
        public bool ShowPerformanceStats => showPerformanceStats;
        public bool EnableQuickActions => enableQuickActions;

        /// <summary>
        /// 設定のバリデーション
        /// </summary>
        public ValidationResult Validate()
        {
            var result = new ValidationResult();
            
            // 倍率設定の検証
            if (globalGoldMultiplier <= 0)
                result.AddError("ゴールド倍率は0より大きい値である必要があります");
            if (globalExpMultiplier <= 0)
                result.AddError("経験値倍率は0より大きい値である必要があります");
            if (battleSpeedMultiplier <= 0)
                result.AddError("戦闘速度倍率は0より大きい値である必要があります");
            if (gachaRateMultiplier <= 0)
                result.AddError("ガチャ確率倍率は0より大きい値である必要があります");

            // フレームレート設定の検証
            if (targetFrameRate < 30)
                result.AddWarning("目標フレームレートが30未満です。ユーザーエクスペリエンスが低下する可能性があります");
            if (targetFrameRate > 120)
                result.AddWarning("目標フレームレートが120を超えています。バッテリー消費が増加する可能性があります");

            // チート設定の警告
            if (infiniteGold || maxGachaLevel || unlockAllCharacters)
                result.AddWarning("チート機能が有効です。リリースビルドでは無効にしてください");

            // パフォーマンス設定の検証
            if (skipAnimations && fastBattles)
                result.AddWarning("アニメーションスキップと高速戦闘の両方が有効です。デバッグが困難になる可能性があります");

            return result;
        }

        /// <summary>
        /// Unity OnValidate
        /// </summary>
        public void OnValidate()
        {
            var validation = Validate();
            if (!validation.IsValid)
            {
                Debug.LogWarning($"[DevelopmentSettings] バリデーション問題: {validation.GetSummary()}");
            }

            // 設定変更時の自動適用
            ApplySettings();
        }

        /// <summary>
        /// 設定を実際のゲームに適用
        /// </summary>
        public void ApplySettings()
        {
            // フレームレート設定
            Application.targetFrameRate = targetFrameRate;
            
            if (enableAllDebugLogs)
                Debug.Log("[DevelopmentSettings] 開発設定を適用しました");
        }

        /// <summary>
        /// 全設定をデフォルトにリセット
        /// </summary>
        public void ResetToDefaults()
        {
            enableAllDebugLogs = true;
            showSystemHealth = false;
            pauseOnCriticalError = true;
            enableGodMode = false;

            skipAnimations = false;
            fastBattles = false;
            disableParticles = false;
            targetFrameRate = 60;

            globalGoldMultiplier = 1.0f;
            globalExpMultiplier = 1.0f;
            battleSpeedMultiplier = 1.0f;
            gachaRateMultiplier = 1.0f;

            unlockAllCharacters = false;
            infiniteGold = false;
            maxGachaLevel = false;
            skipTutorial = false;

            showDebugUI = false;
            showPerformanceStats = false;
            enableQuickActions = false;

            Debug.Log("[DevelopmentSettings] 設定をデフォルトにリセットしました");
        }

        /// <summary>
        /// 現在の設定の概要を取得
        /// </summary>
        public string GetSettingsSummary()
        {
            var summary = "=== 開発設定概要 ===\n";
            summary += $"デバッグログ: {enableAllDebugLogs}\n";
            summary += $"ゴッドモード: {enableGodMode}\n";
            summary += $"目標FPS: {targetFrameRate}\n";
            summary += $"ゴールド倍率: {globalGoldMultiplier:F1}x\n";
            summary += $"戦闘速度倍率: {battleSpeedMultiplier:F1}x\n";
            
            if (infiniteGold || maxGachaLevel || unlockAllCharacters)
            {
                summary += "--- チート有効 ---\n";
                if (infiniteGold) summary += "無限ゴールド\n";
                if (maxGachaLevel) summary += "最大ガチャレベル\n";
                if (unlockAllCharacters) summary += "全キャラクター解放\n";
            }

            return summary;
        }

        /// <summary>
        /// 設定をJSON文字列として出力
        /// </summary>
        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        /// <summary>
        /// JSON文字列から設定を読み込み
        /// </summary>
        public void FromJson(string json)
        {
            try
            {
                JsonUtility.FromJsonOverwrite(json, this);
                ApplySettings();
                Debug.Log("[DevelopmentSettings] JSONから設定を読み込みました");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DevelopmentSettings] JSON読み込みエラー: {e.Message}");
            }
        }
    }
}