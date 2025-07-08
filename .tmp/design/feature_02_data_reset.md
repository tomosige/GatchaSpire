# 機能02: データリセット機能

## 概要
ランごとの完全リセット、永続化要素の排除、一時的なデータのクリアを担当するシステム。

## 実装クラス設計

### DataResetManager
全てのゲームデータのリセットを管理するクラス。

**publicメソッド:**
- `void ResetAllRunData()` - ランに関する全データのリセット
- `void ResetCharacterData()` - キャラクターデータのリセット
- `void ResetResourceData()` - リソース（ゴールド等）のリセット
- `void ResetProgressData()` - 進行状況データのリセット
- `void PreservePermanentData()` - 永続化すべきデータの保護
- `void ValidateResetCompletion()` - リセット完了の検証

**詳細説明:**
- 各システムのリセット機能を統合的に管理
- リセット対象外のデータ（解放要素、設定等）を明確に区別
- メモリリークを防ぐため、参照の完全クリアを実行
- デバッグ用にリセット前後の状態を記録

### IPersistentData
永続化データを識別するインターフェース。

**publicメソッド:**
- `void SavePersistentData()` - 永続化データの保存
- `void LoadPersistentData()` - 永続化データの読み込み
- `bool IsPersistent()` - 永続化対象かどうかの判定

**詳細説明:**
- 永続化すべきデータクラスが実装するインターフェース
- 解放要素、設定、統計情報等が対象

### IResettableData
リセット可能データを識別するインターフェース。

**publicメソッド:**
- `void ResetData()` - データのリセット
- `void ValidateReset()` - リセット完了の検証
- `string GetDataTypeName()` - データタイプ名の取得

**詳細説明:**
- ランごとにリセットすべきデータクラスが実装
- キャラクターデータ、リソースデータ、進行状況等が対象

### MemoryManager
メモリ管理とガベージコレクションを担当するクラス。

**publicメソッド:**
- `void ForceGarbageCollection()` - 強制的なガベージコレクション実行
- `void UnloadUnusedAssets()` - 未使用アセットのアンロード
- `void ClearObjectPools()` - オブジェクトプールのクリア
- `long GetCurrentMemoryUsage()` - 現在のメモリ使用量取得

**詳細説明:**
- リセット時の確実なメモリ解放を担当
- Resources.UnloadUnusedAssets()の適切な実行タイミング制御
- オブジェクトプールの完全クリア