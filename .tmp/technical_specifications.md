# 技術仕様書 - GatchaSpire

## 基本情報

- **プロジェクト名**: GatchaSpire
- **ジャンル**: ローグライクガチャゲーム
- **開発環境**: Unity 2021.3.22f1 LTS
- **作成日**: 2025-07-08

---

## プラットフォーム仕様

### 主要プラットフォーム
- **Android** (Primary Target)
  - 最小API レベル: API 26 (Android 8.0)
  - 推奨API レベル: API 30 (Android 11) 以上
  - アーキテクチャ: ARM64 (arm64-v8a)
  - 32bit対応: ARMv7 (armeabi-v7a) オプション

### 将来対応プラットフォーム
- **PC (Windows)**
  - OS: Windows 10 64bit 以上
  - DirectX: 11 以上
  - アーキテクチャ: x64

---

## Unity設定

### Unity バージョン
- **Unity 2021.3.22f1 LTS**
- LTS使用理由: 安定性とサポート期間の確保

### 必要パッケージ

#### コアパッケージ
```
com.unity.2d.sprite@1.0.0
com.unity.2d.tilemap@1.0.0
com.unity.ugui@1.0.0
```

#### UI・グラフィックス
```
com.unity.2d.animation@7.0.6
com.unity.2d.psdimporter@6.0.4
com.unity.addressables@1.19.19
```

#### プラットフォーム関連
```
com.unity.mobile.android-logcat@1.3.2
com.unity.mobile.notifications@2.0.2
```

#### 開発・デバッグ
```
com.unity.test-framework@1.1.33
com.unity.performance.profile-analyzer@1.1.1
```

#### オプション（検討中）
```
com.unity.analytics@3.8.1
com.unity.remote-config@3.1.3
```

### Player Settings

#### Android設定
```
Configuration: Release
Scripting Backend: IL2CPP
Api Compatibility Level: .NET Standard 2.1
Target Architectures: ARM64, ARMv7 (optional)
Multithreaded Rendering: Enabled
Graphics APIs: Vulkan, OpenGLES3, OpenGLES2
```

#### 共通設定
```
Color Space: Linear
Lightmap Encoding: Normal Quality
HDR Cube map Encoding: Normal Quality
Lightmap Streaming: Disabled
```

---

## パフォーマンス目標

### Android端末パフォーマンス

#### フレームレート目標
- **最低保証**: 30 FPS
- **推奨**: 60 FPS
- **測定条件**: 戦闘中の最大負荷時

#### メモリ使用量制限
- **総メモリ使用量**: 800MB 以下 (RAM 2GB端末で動作)
- **Unity Heap**: 300MB 以下
- **テクスチャメモリ**: 200MB 以下
- **オーディオメモリ**: 50MB 以下

#### ストレージ要件
- **初期APKサイズ**: 50MB 以下 (軽量ゲーム)
- **インストール後サイズ**: 150MB 以下
- **追加データ**: フリーアセット使用、最小限の容量

#### バッテリー消費
- **連続プレイ時間**: 2時間以上（標準的なスマートフォンで）
- **発熱対策**: CPU/GPU使用率の適切な制御

### PC端末パフォーマンス（将来対応）

#### 最小スペック
```
CPU: Intel i3-8100 / AMD Ryzen 3 2200G 相当
GPU: Intel UHD Graphics 630 / AMD Vega 8 相当
RAM: 4GB
ストレージ: 2GB
```

#### 推奨スペック
```
CPU: Intel i5-9400 / AMD Ryzen 5 3600 相当
GPU: GTX 1050 / RX 560 相当
RAM: 8GB
ストレージ: 4GB (SSD推奨)
```

---

## 品質設定

### グラフィック品質設定

#### Android Low Quality
```
Texture Quality: Half Resolution
Shadow Quality: Disabled
Anti Aliasing: Disabled
Soft Particles: Disabled
```

#### Android Medium Quality
```
Texture Quality: Full Resolution
Shadow Quality: Hard Shadows Only
Anti Aliasing: 2x Multi Sampling
Soft Particles: Enabled
```

#### Android High Quality
```
Texture Quality: Full Resolution
Shadow Quality: Soft Shadows
Anti Aliasing: 4x Multi Sampling
Soft Particles: Enabled
Realtime Reflection Probes: Enabled
```

---

## ビルド設定

### Android ビルド設定

#### Development Build
```
Development Build: Enabled
Script Debugging: Enabled
Deep Profiling: Disabled
Autoconnect Profiler: Enabled
```

#### Release Build
```
Development Build: Disabled
Script Debugging: Disabled
Stripping Level: Medium
Managed Code Stripping: Enabled
```

### アセット最適化

#### テクスチャ設定
```
Default Format: ASTC 8x8 (軽量化重視)
Quality: Low-Normal
Max Size: 1024x1024 (UI), 512x512 (Characters)
Generate Mip Maps: 必要最小限のみ
```

#### オーディオ設定
```
BGM: Vorbis, Quality 70, Mono/Stereo
SE: PCM (短音), Vorbis (長音)
Voice: Vorbis, Quality 50
```

---

## セキュリティ・認証

### Android セキュリティ
```
Proguard Minification: Enabled (Release)
Split Application Binary: Disabled (軽量化)
Use APK Expansion Files: 使用しない
```

### データ保護
```
Save Data Encryption: なし (オフラインゲーム)
Player Prefs Encryption: なし (不正対策不要)
```

---

## 開発・デバッグ設定

### ログレベル
```
Development: Debug, Info, Warning, Error
Staging: Info, Warning, Error  
Production: Warning, Error
```

### プロファイリング
```
Memory Profiler: Development Build時のみ
CPU Usage Profiler: Development Build時のみ
GPU Usage Profiler: Development Build時のみ
```

### デバッグ機能
```
Console Commands: Development Build時のみ
Debug UI: Development Build時のみ
Cheat Commands: Development Build時のみ
Performance Overlay: Development Build時のみ
```

---

## ネットワーク・データ通信

### オフライン機能
- **基本ゲームプレイ**: 完全オフライン対応
- **データ保存**: ローカルストレージのみ
- **ネットワーク通信**: 一切なし

### オンライン機能
- **対応予定なし**: 完全オフラインゲームとして設計

---

## 制約事項・注意点

### Android特有の制約
- **バックグラウンド制限**: アプリ非アクティブ時の処理制限
- **メモリ制限**: デバイス性能による動的調整
- **権限要求**: 最小限の権限のみ使用

### Unity 2021.3.22f1 既知の問題
- **IL2CPP**: 一部のリフレクション制限
- **Addressables**: 大量アセット時のロード時間

### パフォーマンス制約
- **ガベージコレクション**: 頻繁なオブジェクト生成の回避
- **Draw Call数**: 可能な限り削減（バッチング活用）
- **ポリゴン数**: キャラクター1体あたり1000ポリゴン以下推奨

---

## 更新・メンテナンス

### バージョン管理
```
Unity Version: 固定（2021.3.22f1）
Package Version: 定期的な更新チェック
```

### 監視項目
- **クラッシュ率**: 1%以下維持
- **フレームレート**: 目標値の維持
- **メモリ使用量**: 上限値の監視
- **ロード時間**: 3秒以内（シーン切り替え）

---

## 仕様確定項目

以下の項目について仕様が確定されました：

1. **Android最小API レベル**: API 26 (Android 8.0) 
2. **想定デバイス性能**: RAM 2GB端末での動作
3. **APK/インストールサイズ**: 軽量ゲーム向け（APK 50MB以下、インストール後150MB以下）
4. **セーブデータ**: ローカル保存のみ（完全オフラインゲーム）
5. **画像・音声素材**: フリーアセット使用、軽量化重視

これらの仕様に基づいて技術仕様書を最終調整済みです。