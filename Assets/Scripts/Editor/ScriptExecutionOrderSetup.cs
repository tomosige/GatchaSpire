using UnityEngine;
using UnityEditor;
using GatchaSpire.Core;
using GatchaSpire.Core.Error;

namespace GatchaSpire.Editor
{
    /// <summary>
    /// スクリプト実行順序の自動設定
    /// </summary>
    [InitializeOnLoad]
    public static class ScriptExecutionOrderSetup
    {
        static ScriptExecutionOrderSetup()
        {
            SetupExecutionOrder();
        }

        [MenuItem("GatchaSpire/Setup/Configure Script Execution Order")]
        public static void SetupExecutionOrder()
        {
            // 実行順序設定
            var executionOrders = new[]
            {
                (typeof(UnityErrorHandler), -200),
                (typeof(UnityGameSystemCoordinator), -100),
                (typeof(DevelopmentSettings), -50)
            };

            bool changed = false;
            foreach (var (type, order) in executionOrders)
            {
                var script = FindScriptFromType(type);
                if (script != null)
                {
                    var currentOrder = MonoImporter.GetExecutionOrder(script);
                    if (currentOrder != order)
                    {
                        MonoImporter.SetExecutionOrder(script, order);
                        changed = true;
                        Debug.Log($"[ScriptExecutionOrderSetup] {type.Name}の実行順序を{order}に設定しました");
                    }
                }
            }

            if (changed)
            {
                AssetDatabase.Refresh();
                Debug.Log("[ScriptExecutionOrderSetup] スクリプト実行順序の設定が完了しました");
            }
            else
            {
                Debug.Log("[ScriptExecutionOrderSetup] スクリプト実行順序は既に正しく設定されています");
            }
        }

        private static MonoScript FindScriptFromType(System.Type type)
        {
            var guids = AssetDatabase.FindAssets($"t:MonoScript {type.Name}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.GetClass() == type)
                {
                    return script;
                }
            }
            return null;
        }
    }
}