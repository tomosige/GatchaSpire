using UnityEngine;
using UnityEditor;

namespace GatchaSpire.Editor
{
    /// <summary>
    /// Player Settings の最適化設定
    /// </summary>
    public static class PlayerSettingsSetup
    {
        [MenuItem("GatchaSpire/Setup/Optimize Player Settings")]
        public static void OptimizePlayerSettings()
        {
            // 基本設定
            PlayerSettings.companyName = "GatchaSpire Team";
            PlayerSettings.productName = "GatchaSpire";
            PlayerSettings.bundleVersion = "0.1.0";

            // グラフィック設定
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.gpuSkinning = true;
            
            // Android設定
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // iOS設定
            PlayerSettings.iOS.targetOSVersionString = "12.0";
            PlayerSettings.iOS.requiresPersistentWiFi = false;

            // スクリプティング設定
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            
            // API互換性レベル
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard_2_0);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, ApiCompatibilityLevel.NET_Standard_2_0);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_Standard_2_0);

            // デバッグ設定
            #if DEVELOPMENT_BUILD
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
            #else
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            #endif

            // バンドル識別子
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.gatchaspire.game");
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.gatchaspire.game");

            Debug.Log("[PlayerSettingsSetup] Player Settingsの最適化が完了しました");
            
            // 設定を保存
            AssetDatabase.SaveAssets();
        }

        [MenuItem("GatchaSpire/Setup/Configure Graphics Settings")]
        public static void ConfigureGraphicsSettings()
        {
            // グラフィック設定の最適化
            var graphicsSettings = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;
            
            // Quality Settings
            QualitySettings.vSyncCount = 1;
            QualitySettings.antiAliasing = 4;
            QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
            
            Debug.Log("[PlayerSettingsSetup] Graphics Settingsの設定が完了しました");
        }
    }
}