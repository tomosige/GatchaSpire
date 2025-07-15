using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GatchaSpire.Core.Character.Operations
{
    /// <summary>
    /// キャラクター操作の実行結果を表す基底クラス
    /// </summary>
    [Serializable]
    public abstract class CharacterOperationResult
    {
        [Header("結果基本情報")]
        [SerializeField] protected string operationId = "";
        [SerializeField] protected DateTime executionTime = DateTime.Now;
        [SerializeField] protected float executionDurationMs = 0f;

        [Header("メッセージ")]
        [SerializeField] protected string message = "";
        [SerializeField] protected List<string> warnings = new List<string>();

        [Header("処理結果")]
        [SerializeField] protected List<string> affectedCharacterIds = new List<string>();

        // プロパティ
        public string OperationId => operationId;
        public DateTime ExecutionTime => executionTime;
        public float ExecutionDurationMs => executionDurationMs;
        public string Message => message;
        public List<string> Warnings => new List<string>(warnings);
        public List<string> AffectedCharacterIds => new List<string>(affectedCharacterIds);

        // 便利プロパティ
        public bool HasWarnings => warnings.Any();
        public int TotalAffectedCount => affectedCharacterIds.Count;

        /// <summary>
        /// 結果オブジェクトのコンストラクタ
        /// </summary>
        /// <param name="opId">操作ID</param>
        /// <param name="msg">メッセージ</param>
        protected CharacterOperationResult(string opId, string msg = "")
        {
            operationId = opId ?? "";
            message = msg ?? "";
            executionTime = DateTime.Now;
        }

        /// <summary>
        /// メッセージを設定
        /// </summary>
        /// <param name="msg">メッセージ</param>
        public void SetMessage(string msg)
        {
            message = msg ?? "";
        }

        /// <summary>
        /// 実行時間を設定
        /// </summary>
        /// <param name="durationMs">実行時間（ミリ秒）</param>
        public void SetExecutionDuration(float durationMs)
        {
            executionDurationMs = Mathf.Max(0f, durationMs);
        }

        /// <summary>
        /// 警告を追加
        /// </summary>
        /// <param name="warning">警告メッセージ</param>
        public void AddWarning(string warning)
        {
            if (string.IsNullOrEmpty(warning))
            {
                return;
            }

            if (!warnings.Contains(warning))
            {
                warnings.Add(warning);
            }
        }

        /// <summary>
        /// 影響を受けたキャラクターを追加
        /// </summary>
        /// <param name="characterId">キャラクターID</param>
        public void AddAffectedCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                return;
            }

            if (!affectedCharacterIds.Contains(characterId))
            {
                affectedCharacterIds.Add(characterId);
            }
        }

        /// <summary>
        /// 結果の詳細情報を取得
        /// 派生クラスでオーバーライドして追加情報を含める
        /// </summary>
        /// <returns>詳細情報</returns>
        public virtual string GetDetailedInfo()
        {
            var info = $"=== {GetType().Name} ===\n";
            info += $"操作ID: {operationId}\n";
            info += $"実行時刻: {executionTime:yyyy/MM/dd HH:mm:ss}\n";
            info += $"実行時間: {executionDurationMs:F2}ms\n";
            info += $"メッセージ: {message}\n";
            info += $"影響キャラクター数: {affectedCharacterIds.Count}\n";

            if (warnings.Any())
            {
                info += "\n=== 警告 ===\n";
                info += string.Join("\n", warnings.Select((w, index) => $"{index + 1}. {w}")) + "\n";
            }

            if (affectedCharacterIds.Any())
            {
                info += "\n=== 影響を受けたキャラクター ===\n";
                info += string.Join("\n", affectedCharacterIds.Select((id, index) => $"{index + 1}. {id}")) + "\n";
            }

            return info;
        }

        /// <summary>
        /// 結果の簡潔な情報を取得
        /// </summary>
        /// <returns>簡潔な情報</returns>
        public override string ToString()
        {
            return $"{GetType().Name} - {message} ({affectedCharacterIds.Count}件処理)";
        }
    }
}