using System.Collections.Generic;
using UnityEngine;

namespace GatchaSpire.Core
{
    /// <summary>
    /// バリデーション可能インターフェース
    /// ScriptableObjectやデータの検証機能を提供
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        /// データの検証を実行
        /// </summary>
        ValidationResult Validate();

        /// <summary>
        /// Unity OnValidate相当の処理
        /// </summary>
        void OnValidate();
    }

    /// <summary>
    /// エディタプレビュー可能インターフェース
    /// </summary>
    public interface IEditorPreviewable
    {
        /// <summary>
        /// エディタでのプレビュー実行
        /// </summary>
        void PreviewInEditor();

        /// <summary>
        /// プレビュー用テクスチャの取得
        /// </summary>
        Texture2D GetPreviewTexture();

        /// <summary>
        /// プレビュー説明文の取得
        /// </summary>
        string GetPreviewDescription();
    }

    /// <summary>
    /// バリデーション結果クラス
    /// </summary>
    [System.Serializable]
    public class ValidationResult
    {
        [SerializeField] private List<string> errors = new List<string>();
        [SerializeField] private List<string> warnings = new List<string>();

        /// <summary>
        /// エラーが存在しない場合にtrue
        /// </summary>
        public bool IsValid => errors.Count == 0;

        /// <summary>
        /// エラーリストの読み取り専用アクセス
        /// </summary>
        public IReadOnlyList<string> Errors => errors;

        /// <summary>
        /// 警告リストの読み取り専用アクセス
        /// </summary>
        public IReadOnlyList<string> Warnings => warnings;

        /// <summary>
        /// エラーを追加
        /// </summary>
        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
                errors.Add(error);
        }

        /// <summary>
        /// 警告を追加
        /// </summary>
        public void AddWarning(string warning)
        {
            if (!string.IsNullOrWhiteSpace(warning))
                warnings.Add(warning);
        }

        /// <summary>
        /// 結果のサマリーを取得
        /// </summary>
        public string GetSummary()
        {
            var summary = "";
            if (errors.Count > 0)
                summary += $"エラー: {string.Join(", ", errors)}";
            if (warnings.Count > 0)
            {
                if (!string.IsNullOrEmpty(summary)) summary += " ";
                summary += $"警告: {string.Join(", ", warnings)}";
            }
            return summary;
        }

        /// <summary>
        /// 結果をクリア
        /// </summary>
        public void Clear()
        {
            errors.Clear();
            warnings.Clear();
        }
    }
}