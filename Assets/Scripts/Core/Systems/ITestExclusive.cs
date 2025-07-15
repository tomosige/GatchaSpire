using System;
using System.Collections;

namespace GatchaSpire.Core.Systems
{
    /// <summary>
    /// テスト排他制御を行うテストクラス用インターフェース
    /// </summary>
    public interface ITestExclusive
    {
        /// <summary>
        /// テストクラス名
        /// </summary>
        string TestClassName { get; }
        
        /// <summary>
        /// テストの最大実行時間（秒）
        /// </summary>
        float MaxExecutionTimeSeconds { get; }
        
        /// <summary>
        /// テスト実行前の準備処理
        /// </summary>
        void OnTestPrepare();
        
        /// <summary>
        /// テスト実行後のクリーンアップ処理
        /// </summary>
        void OnTestCleanup();
        
        /// <summary>
        /// テストが強制終了された場合の処理
        /// </summary>
        void OnTestForceTerminated();
    }
}