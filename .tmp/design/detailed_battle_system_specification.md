# GatchaSpire バトルシステム詳細仕様書

## 概要
GatchaSpireにおけるオートチェス型リアルタイムバトルシステムの詳細仕様。TeamFightTacticsを参考にした戦闘システムを定義する。

## 基本設計方針
- **リアルタイム制戦闘**: Unity Time.deltaTime使用、0.1秒固定間隔での戦闘処理
- **クールダウン制スキル**: 各スキル個別のクールタイム管理（Time.timeベース）
- **殲滅戦**: 相手チーム全滅まで戦闘継続
- **複雑シナジー**: 戦略的深度を提供する相互作用システム
- **TFT方式**: 配置時は自軍のみ、戦闘時に全体表示
- **端末性能非依存**: Unity時間システムによる一貫した戦闘速度

---

## 1. ボード設計

### 1.1 グリッドシステム
- **座標系**: 四角グリッド（初期実装）
- **将来対応**: ヘクスグリッド（実装可能であれば移行検討）

### 1.2 ボードサイズ
- **自軍エリア**: 7x4グリッド（28マス）
- **敵軍エリア**: 7x4グリッド（28マス）
- **戦闘フィールド全体**: 7x8グリッド（56マス）
- **最大配置キャラ数**: 8体固定（初期実装）

### 1.3 ボードレイアウト
```
四角グリッド座標系:
Y=7 [E] [E] [E] [E] [E] [E] [E]  ← 敵陣（後列）
Y=6 [E] [E] [E] [E] [E] [E] [E]  ← 敵陣
Y=5 [E] [E] [E] [E] [E] [E] [E]  ← 敵陣
Y=4 [E] [E] [E] [E] [E] [E] [E]  ← 敵陣（前列）
Y=3 [P] [P] [P] [P] [P] [P] [P]  ← 自陣（前列）
Y=2 [P] [P] [P] [P] [P] [P] [P]  ← 自陣
Y=1 [P] [P] [P] [P] [P] [P] [P]  ← 自陣
Y=0 [P] [P] [P] [P] [P] [P] [P]  ← 自陣（後列）
    X=0 X=1 X=2 X=3 X=4 X=5 X=6
```

### 1.4 UI表示方式
- **配置フェーズ**: 自軍エリア（7x4）のみ表示
- **戦闘フェーズ**: 全体フィールド（7x8）表示
- **スマホ対応**: TFT方式による画面最適化

---

## 2. 戦闘システム

### 2.1 基本戦闘フロー
```
戦闘開始 → 初期配置確認 → リアルタイム戦闘ループ → 勝敗判定 → 結果処理
```

#### リアルタイム戦闘ループ（Unity Time.deltaTime使用、0.1秒固定間隔）
1. **スキルクールダウン更新**
2. **キャラクター行動判定**
3. **移動処理**
4. **攻撃・スキル実行**
5. **勝利条件チェック**

#### 戦闘ループ実装例
```csharp
public class BattleManager : MonoBehaviour
{
    private float accumulator = 0f;
    private const float FIXED_TIMESTEP = 0.1f;
    
    private void Update()
    {
        if (!IsBattleActive) return;
        
        accumulator += Time.deltaTime;
        
        // 0.1秒ごとに確実に実行、フレームレート非依存
        while (accumulator >= FIXED_TIMESTEP)
        {
            ProcessBattleTick();
            accumulator -= FIXED_TIMESTEP;
        }
    }
    
    private void ProcessBattleTick()
    {
        UpdateSkillCooldowns();
        ProcessCharacterActions();
        ProcessMovement();
        ProcessAttacksAndSkills();
        CheckVictoryConditions();
    }
}
```

### 2.2 キャラクター行動システム

#### 基本パラメータ
```csharp
public class CombatCharacter
{
    public float AttackSpeed { get; set; }      // 基本攻撃間隔（秒）
    public float MovementSpeed { get; set; }    // 移動速度
    public int AttackRange { get; set; }        // 攻撃射程（マス数）
    public Vector2Int BoardPosition { get; set; } // 盤面座標
    
    // 行動制御（Time.timeベース）
    public float LastAttackTime { get; set; }
    public Dictionary<int, float> SkillLastUsedTime { get; set; }
    public Dictionary<int, float> SkillCooldownDurations { get; set; }
    public CombatState CurrentState { get; set; }
}

public enum CombatState
{
    Idle,           // 待機
    Moving,         // 移動中
    Attacking,      // 攻撃中
    CastingSkill,   // スキル詠唱中
    Stunned         // 行動不能
}
```

#### 行動優先度
1. **スキル発動**（クールダウン完了時）
2. **基本攻撃**（攻撃速度間隔経過時）
3. **移動**（攻撃対象への接近）

### 2.3 移動システム（TFT方式）

#### 移動ルール
- **移動条件**: 射程内に攻撃可能な敵がいない場合
- **移動距離**: 1マスずつ移動
- **移動方向**: 最も近い敵に向かって移動
- **移動制限**: 他のキャラクターがいるマスには移動不可

#### 距離計算（四角グリッド）
```csharp
public static int CalculateDistance(Vector2Int pos1, Vector2Int pos2)
{
    return Mathf.Abs(pos1.x - pos2.x) + Mathf.Abs(pos1.y - pos2.y); // マンハッタン距離
}
```

#### 移動アルゴリズム
```csharp
public Vector2Int CalculateNextMovePosition(Vector2Int currentPos, Vector2Int targetPos)
{
    Vector2Int direction = new Vector2Int(
        Mathf.Clamp(targetPos.x - currentPos.x, -1, 1),
        Mathf.Clamp(targetPos.y - currentPos.y, -1, 1)
    );
    
    Vector2Int nextPos = currentPos + direction;
    
    // 移動可能性チェック
    if (IsValidPosition(nextPos) && !IsOccupied(nextPos))
    {
        return nextPos;
    }
    
    return currentPos; // 移動不可の場合は現在位置を維持
}
```

### 2.4 攻撃対象選択システム

#### 対象選択優先度
1. **最近距離優先**: 最も近い敵を攻撃対象に選択
2. **同距離時の判定**: 
   - HP割合が低い敵を優先
   - 同HP割合時はランダム選択

#### 対象選択アルゴリズム
```csharp
public Character SelectTarget(Character attacker, List<Character> enemies)
{
    var validTargets = enemies.Where(e => 
        IsInRange(attacker.BoardPosition, e.BoardPosition, attacker.AttackRange) &&
        e.IsAlive
    ).ToList();
    
    if (!validTargets.Any()) return null;
    
    // 距離順でソート、同距離時はHP割合順
    return validTargets
        .OrderBy(e => CalculateDistance(attacker.BoardPosition, e.BoardPosition))
        .ThenBy(e => e.CurrentHP / (float)e.MaxHP)
        .ThenBy(e => Random.value)
        .First();
}
```

### 2.5 攻撃・射程システム

#### 射程の種類
- **近接攻撃**: 射程1（隣接マスのみ）
- **短距離攻撃**: 射程2-3
- **長距離攻撃**: 射程4-5
- **射線判定**: なし（複雑さを避けるため）

#### 攻撃処理
```csharp
public bool CanAttack(Character attacker, Character target)
{
    int distance = CalculateDistance(attacker.BoardPosition, target.BoardPosition);
    return distance <= attacker.AttackRange;
}

public void ProcessAttack(Character attacker, Character target)
{
    if (!CanAttack(attacker, target)) return;
    
    float damage = CalculateDamage(attacker, target);
    target.TakeDamage(damage);
    
    attacker.LastAttackTime = Time.time;
    
    // 攻撃エフェクト・ログ等の処理
    OnAttackPerformed?.Invoke(attacker, target, damage);
}
```

---

## 3. 勝利条件・時間管理

### 3.1 勝利条件
- **殲滅戦**: 相手チームのキャラクター数が0になった時点で勝利
- **判定タイミング**: 各キャラクターのHP0到達時に即座チェック

### 3.2 時間制限システム
- **基本戦闘時間**: 制限なし
- **長期化防止**: 30秒経過後に段階的ダメージ増加
- **ダメージ増加**: 10秒ごとに全キャラに現在HP5%のダメージ

#### 段階的ダメージシステム（Time.timeベース）
```csharp
public class BattleTimeManager
{
    private float battleStartTime;
    private float lastDamageTime;
    private const float DAMAGE_START_TIME = 30f; // 30秒後に開始
    private const float DAMAGE_INTERVAL = 10f;   // 10秒ごと
    private const float DAMAGE_PERCENT = 0.05f;  // 現在HP5%
    
    private void Start()
    {
        battleStartTime = Time.time; // 戦闘開始時刻を記録
        lastDamageTime = 0f;
    }
    
    public void Update()
    {
        float elapsedTime = Time.time - battleStartTime;
        
        if (elapsedTime > DAMAGE_START_TIME)
        {
            // 最後のダメージから10秒経過したかチェック
            if (Time.time - lastDamageTime >= DAMAGE_INTERVAL)
            {
                ApplyTimeoutDamage();
                lastDamageTime = Time.time;
            }
        }
    }
    
    private void ApplyTimeoutDamage()
    {
        foreach (var character in GetAllAliveCharacters())
        {
            float damage = character.CurrentHP * DAMAGE_PERCENT;
            character.TakeDamage(damage);
        }
        
        Debug.Log($"段階的ダメージを適用: {DAMAGE_PERCENT * 100}% (戦闘時間: {Time.time - battleStartTime:F1}秒)");
    }
}
```

---

## 4. 敵配置システム

### 4.1 敵プリセットシステム
```csharp
[CreateAssetMenu(fileName = "EnemyFormation", menuName = "GatchaSpire/EnemyFormation")]
public class EnemyFormationPreset : ScriptableObject
{
    [Header("基本情報")]
    public string formationName;
    public int recommendedRound;
    
    [Header("敵配置")]
    public List<EnemyPlacement> enemyPlacements;
}

[System.Serializable]
public class EnemyPlacement
{
    public CharacterData characterData;
    public Vector2Int position; // 敵エリア内の相対座標（0,0～6,3）
    public int level;
}
```

### 4.2 敵選択システム
- **プリセット管理**: 手動作成された固定配置パターン
- **選択方式**: ラウンド数に応じたプリセットからランダム選択
- **配置座標**: 敵エリア内の相対座標（Y=4～7の範囲）

#### 敵配置マネージャー
```csharp
public class EnemyFormationManager : MonoBehaviour
{
    [SerializeField] private List<EnemyFormationPreset> formationPresets;
    
    public EnemyFormationPreset SelectFormation(int currentRound)
    {
        var suitableFormations = formationPresets
            .Where(f => f.recommendedRound <= currentRound)
            .ToList();
            
        if (!suitableFormations.Any())
            return formationPresets.First();
            
        return suitableFormations[Random.Range(0, suitableFormations.Count)];
    }
    
    public List<Character> CreateEnemyTeam(EnemyFormationPreset formation)
    {
        var enemies = new List<Character>();
        
        foreach (var placement in formation.enemyPlacements)
        {
            var enemy = CreateCharacter(placement.characterData, placement.level);
            enemy.BoardPosition = ConvertToWorldPosition(placement.position);
            enemies.Add(enemy);
        }
        
        return enemies;
    }
    
    private Vector2Int ConvertToWorldPosition(Vector2Int relativePos)
    {
        // 敵エリア（Y=4～7）への座標変換
        return new Vector2Int(relativePos.x, relativePos.y + 4);
    }
}
```

---

## 5. 戦闘速度・アニメーション

### 5.1 Unity時間システムの活用

#### Time.timeベースの判定
```csharp
public class CharacterSkillManager
{
    // スキルクールダウン判定
    public bool IsSkillReady(int skillSlot)
    {
        if (!skillLastUsedTime.ContainsKey(skillSlot)) return true;
        
        float cooldownDuration = skillCooldownDurations[skillSlot];
        return Time.time - skillLastUsedTime[skillSlot] >= cooldownDuration;
    }
    
    // 攻撃間隔判定
    public bool CanAttack(Character character)
    {
        return Time.time - character.LastAttackTime >= character.AttackSpeed;
    }
    
    // スキル使用時の記録
    public void UseSkill(int skillSlot, float cooldownDuration)
    {
        skillLastUsedTime[skillSlot] = Time.time;
        skillCooldownDurations[skillSlot] = cooldownDuration;
    }
}
```

### 5.2 タイミング設定
- **基本攻撃速度**: キャラクター個別設定（おおむね2秒前後）
- **移動速度**: 1マス/秒（固定）
- **スキル詠唱時間**: スキルごとに個別設定
- **アニメーション時間**: 最小限（初期実装では簡素化）

### 5.3 戦闘速度制御（将来機能）
```csharp
public class BattleSpeedController
{
    public void SetBattleSpeed(float speed)
    {
        Time.timeScale = speed; // 1.0=通常、2.0=2倍速、0.5=半速
    }
    
    public void PauseBattle()
    {
        Time.timeScale = 0f; // 完全停止
    }
    
    public void ResumeBattle()
    {
        Time.timeScale = 1f; // 通常速度
    }
}
```

### 5.4 同時処理
- **複数キャラ行動**: 同時実行可能
- **競合回避**: 同一マスへの移動時は先着優先
- **攻撃順序**: 攻撃速度の早いキャラから処理

---

## 6. ダメージ計算システム

### 6.1 基本ダメージ計算式
```csharp
public float CalculateBasicDamage(Character attacker, Character target)
{
    float baseDamage = attacker.AttackPower;
    float defense = target.Defense;
    
    // 基本式（後で調整可能な設計）
    float finalDamage = baseDamage * (100f / (100f + defense));
    
    return Mathf.Max(1f, finalDamage); // 最低1ダメージ保証
}
```

### 6.2 拡張可能な設計
- **ダメージタイプ**: 物理・魔法・確定ダメージ
- **属性システム**: 火・水・土・風の相性
- **クリティカル**: 確率・倍率システム
- **バフ・デバフ**: ステータス修正値の適用

#### ダメージ計算インターフェース
```csharp
public interface IDamageCalculator
{
    float CalculateDamage(Character attacker, Character target, DamageType damageType);
}

public class StandardDamageCalculator : IDamageCalculator
{
    public float CalculateDamage(Character attacker, Character target, DamageType damageType)
    {
        // 基本計算ロジック
        // 将来的な拡張に対応
    }
}
```

---

## 7. 実装優先度

### Phase 1: 基本ボードシステム
1. **四角グリッド実装**
   - 7x8ボードの表示・管理
   - キャラクター配置システム
   - 座標系の基本実装

2. **基本戦闘ループ**
   - リアルタイム処理（0.1秒間隔）
   - キャラクター行動管理
   - 基本攻撃システム

3. **移動システム**
   - TFT方式移動
   - 距離計算・対象選択
   - 移動可能性判定

### Phase 2: 戦闘システム拡張
1. **敵プリセットシステム**
   - ScriptableObjectによる敵配置管理
   - ランダム選択機能
   - 敵チーム生成

2. **勝利条件・時間管理**
   - 殲滅戦判定
   - 段階的ダメージシステム
   - 戦闘結果処理

3. **UI基本実装**
   - 配置フェーズUI（簡素版）
   - 戦闘観戦UI（簡素版）
   - 基本的な情報表示

### Phase 3: システム統合・最適化
1. **スキルシステム統合**
   - 戦闘システムとの連携
   - クールダウン管理
   - スキル効果適用

2. **シナジーシステム統合**
   - 複雑な相互作用
   - 動的効果計算
   - バランス調整機能

3. **パフォーマンス最適化**
   - 処理負荷軽減
   - メモリ使用量最適化
   - フレームレート安定化

---

## 8. 技術的考慮事項

### 8.1 パフォーマンス
- **更新頻度**: Time.deltaTime累積による0.1秒固定間隔、フレームレート非依存
- **時間管理**: Time.timeベースによる端末性能非依存の一貫した動作
- **オブジェクトプール**: キャラクター・エフェクトの再利用
- **計算最適化**: 不要な距離計算の削減

### 8.2 拡張性
- **モジュラー設計**: 各システムの独立性確保
- **設定データ**: ScriptableObjectによる調整容易性
- **インターフェース**: 将来的な機能追加への対応

### 8.3 デバッグ・テスト
- **戦闘ログ**: 詳細な行動記録
- **可視化**: グリッド・射程・移動の表示
- **テストケース**: 自動テストによる品質保証

---

## 9. 注意事項

### 9.1 初期実装の簡素化
- **UI**: 最小限の機能に留める
- **アニメーション**: 基本的な表示のみ
- **エフェクト**: 後回し

### 9.2 将来的な拡張
- **ヘクスグリッド**: 実装可能性の検討
- **高度なUI**: ズーム・ミニマップ等
- **視覚効果**: 豊富なエフェクト・アニメーション

### 9.3 バランス調整
- **数値調整**: ScriptableObjectによる容易な変更
- **テストプレイ**: 実際のゲームプレイでの検証
- **データ収集**: 戦闘統計による改善指針

---

*このバトルシステム仕様書は、GatchaSpireのコアゲームプレイを決定する重要な要素です。実装時は段階的に機能を追加し、各段階でのテストとバランス調整を重視してください。*