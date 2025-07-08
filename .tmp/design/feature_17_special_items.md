# 機能17: 特殊効果アイテムシステム

## 概要
不要キャラクターのアイテム変換、一回限りの消費アイテム、戦闘中の特殊効果適用を担当するシステム。

## 実装クラス設計

### SpecialItemManager
特殊効果アイテムの管理を行うクラス。

**publicメソッド:**
- `SpecialItem ConvertCharacterToItem(Character character)` - キャラクターアイテム変換
- `bool UseItem(SpecialItem item, BattleContext context)` - アイテム使用
- `List<SpecialItem> GetUsableItems(BattleContext context)` - 使用可能アイテム取得
- `void AddItem(SpecialItem item)` - アイテム追加
- `void RemoveItem(SpecialItem item)` - アイテム除去

**詳細説明:**
- キャラクターからアイテムへの変換
- 戦闘中のアイテム使用
- 使用制限の管理
- インベントリとの連携

### ItemConversionSystem
アイテム変換システムを管理するクラス。

**publicメソッド:**
- `SpecialItem GetConversionResult(Character character)` - 変換結果取得
- `List<SpecialItem> GetPossibleConversions(Character character)` - 変換候補取得
- `float GetConversionSuccessRate(Character character)` - 変換成功率取得
- `void ProcessConversion(Character character)` - 変換処理
- `ConversionPreview GetConversionPreview(Character character)` - 変換プレビュー取得

**詳細説明:**
- 変換アルゴリズムの実行
- 変換結果の決定
- 成功率の管理
- プレビュー機能の提供