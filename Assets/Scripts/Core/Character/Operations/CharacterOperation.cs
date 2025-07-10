using System;
using UnityEngine;

namespace GatchaSpire.Core.Character.Operations
{
    /// <summary>
    /// キャラクター操作の基底クラス
    /// 全ての操作に共通する基本的な機能を提供
    /// </summary>
    [Serializable]
    public abstract class CharacterOperation
    {
        [Header("操作基本情報")]
        [SerializeField] protected string operationId;
        [SerializeField] protected DateTime requestTime;
        [SerializeField] protected string requesterId;

        [Header("実行制御")]
        [SerializeField] protected bool validateBeforeExecution = true;
        [SerializeField] protected int maxRetries = 3;
        [SerializeField] protected float timeoutSeconds = 30f;
        [SerializeField] protected bool isUrgent = false;

        // プロパティ
        public string OperationId => operationId;
        public DateTime RequestTime => requestTime;
        public string RequesterId => requesterId;
        public bool ValidateBeforeExecution => validateBeforeExecution;
        public int MaxRetries => maxRetries;
        public float TimeoutSeconds => timeoutSeconds;
        public bool IsUrgent => isUrgent;

        /// <summary>
        /// 基底クラスコンストラクタ
        /// </summary>
        /// <param name="requester">リクエスター名</param>
        protected CharacterOperation(string requester = "system")
        {
            operationId = Guid.NewGuid().ToString();
            requestTime = DateTime.Now;
            requesterId = requester;
        }

        /// <summary>
        /// 操作が有効かどうかを検証
        /// 派生クラスで具体的な検証ロジックを実装
        /// </summary>
        /// <returns>有効な場合true</returns>
        public abstract bool IsValid();

        /// <summary>
        /// 操作が期限切れかチェック
        /// </summary>
        /// <returns>期限切れの場合true</returns>
        public bool IsExpired()
        {
            var elapsed = DateTime.Now - requestTime;
            return elapsed.TotalSeconds > timeoutSeconds;
        }

        /// <summary>
        /// タイムアウト時間を設定
        /// </summary>
        /// <param name="seconds">タイムアウト秒数</param>
        public void SetTimeout(float seconds)
        {
            timeoutSeconds = Mathf.Max(1f, seconds);
        }

        /// <summary>
        /// 最大リトライ回数を設定
        /// </summary>
        /// <param name="retries">リトライ回数</param>
        public void SetMaxRetries(int retries)
        {
            maxRetries = Mathf.Max(0, retries);
        }

        /// <summary>
        /// 緊急フラグを設定
        /// </summary>
        /// <param name="urgent">緊急の場合true</param>
        public void SetUrgent(bool urgent)
        {
            isUrgent = urgent;
        }

        /// <summary>
        /// 実行前検証の有効/無効を設定
        /// </summary>
        /// <param name="validate">検証する場合true</param>
        public void SetValidateBeforeExecution(bool validate)
        {
            validateBeforeExecution = validate;
        }

        /// <summary>
        /// 操作の詳細情報を取得
        /// 派生クラスでオーバーライドして追加情報を含める
        /// </summary>
        /// <returns>詳細情報</returns>
        public virtual string GetDetailedInfo()
        {
            var info = $"=== {GetType().Name} ===\n";
            info += $"ID: {operationId}\n";
            info += $"リクエスト時刻: {requestTime:yyyy/MM/dd HH:mm:ss}\n";
            info += $"リクエスター: {requesterId}\n";
            info += $"検証有効: {validateBeforeExecution}\n";
            info += $"最大リトライ: {maxRetries}\n";
            info += $"タイムアウト: {timeoutSeconds}秒\n";
            info += $"緊急フラグ: {isUrgent}\n";
            info += $"有効性: {IsValid()}\n";
            info += $"期限切れ: {IsExpired()}\n";
            return info;
        }

        /// <summary>
        /// 操作の簡潔な情報を取得
        /// </summary>
        /// <returns>簡潔な情報</returns>
        public override string ToString()
        {
            return $"{GetType().Name} - {operationId}";
        }
    }
}