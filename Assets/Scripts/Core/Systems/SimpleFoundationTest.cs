using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GatchaSpire.Core.Error;

namespace GatchaSpire.Core.Systems
{
    /// <summary>
    /// 簡潔な基盤システムテスト
    /// </summary>
    public class SimpleFoundationTest : GameSystemBase
    {
        [Header("テスト設定")]
        [SerializeField] private bool runTestsOnStart = true;

        protected override string SystemName => "SimpleFoundationTest";

        private List<string> testResults;

        protected override void OnSystemInitialize()
        {
            testResults = new List<string>();
            priority = SystemPriority.Lowest;
        }

        protected override void OnSystemStart()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunBasicTests());
            }
        }

        public IEnumerator RunBasicTests()
        {
            ReportInfo("基盤テストを開始します");
            testResults.Clear();

            // エラーハンドラーテスト
            TestErrorHandler();
            yield return new WaitForSeconds(0.1f);

            // システムコーディネーターテスト  
            TestSystemCoordinator();
            yield return new WaitForSeconds(0.1f);

            // 開発設定テスト
            TestDevelopmentSettings();
            yield return new WaitForSeconds(0.1f);

            // 結果表示
            ShowResults();
        }

        private void TestErrorHandler()
        {
            var handler = UnityErrorHandler.Instance;
            if (handler != null)
            {
                handler.ReportInfo("SimpleFoundationTest", "テストメッセージ");
                var history = handler.GetErrorHistory();
                testResults.Add($"✓ エラーハンドラー: 正常動作 (履歴数: {history.Count})");
            }
            else
            {
                testResults.Add("✗ エラーハンドラー: インスタンスが見つかりません");
            }
        }

        private void TestSystemCoordinator()
        {
            var coordinator = UnityGameSystemCoordinator.Instance;
            if (coordinator != null && coordinator.IsInitialized)
            {
                testResults.Add($"✓ システムコーディネーター: 正常動作 (システム数: {coordinator.SystemCount})");
            }
            else
            {
                testResults.Add("✗ システムコーディネーター: 初期化されていません");
            }
        }

        private void TestDevelopmentSettings()
        {
            var settings = Resources.Load<DevelopmentSettings>("DevelopmentSettings");
            if (settings != null)
            {
                var validation = settings.Validate();
                if (validation.IsValid)
                {
                    testResults.Add("✓ 開発設定: 正常動作");
                }
                else
                {
                    testResults.Add($"⚠ 開発設定: バリデーション警告 ({validation.Warnings.Count}件)");
                }
            }
            else
            {
                testResults.Add("✗ 開発設定: 設定ファイルが見つかりません");
            }
        }

        private void ShowResults()
        {
            ReportInfo("=== 基盤テスト結果 ===");
            foreach (var result in testResults)
            {
                ReportInfo(result);
            }

            var successCount = 0;
            foreach (var result in testResults)
            {
                if (result.StartsWith("✓")) successCount++;
            }

            var summary = $"テスト完了: {successCount}/{testResults.Count} 成功";
            if (successCount == testResults.Count)
            {
                ReportInfo($"🎉 {summary} - 基盤システムが正常に動作しています");
            }
            else
            {
                ReportWarning($"⚠️ {summary} - 一部に問題があります");
            }
        }

        [ContextMenu("Run Tests")]
        public void RunTestsManually()
        {
            if (Application.isPlaying)
            {
                StartCoroutine(RunBasicTests());
            }
            else
            {
                ReportWarning("テストは実行時のみ動作します");
            }
        }
    }
}