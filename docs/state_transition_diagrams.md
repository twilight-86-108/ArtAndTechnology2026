# 状態遷移図

**境界生物 / Liminal Creature**  
バージョン: 1.1 ｜ 作成日: 2026年4月  
変更履歴: v1.0→v1.1 MECEレビュー反映（パフォーマンス低下遷移、形態段階分岐、凡例、画面遷移修正等）

---

## 目次

1. [本書の位置づけ](#1-本書の位置づけ)
2. [図の一覧](#2-図の一覧)
   - 2.1 [凡例](#21-凡例)
   - 2.2 [互換性に関する注記](#22-互換性に関する注記)
3. [STD-01: 体験フロー全体](#3-std-01-体験フロー全体)
4. [STD-02: 生き物ライフサイクル](#4-std-02-生き物ライフサイクル)
5. [STD-03: 関係性状態遷移](#5-std-03-関係性状態遷移)
6. [STD-04: 行動優先度遷移](#6-std-04-行動優先度遷移)
7. [STD-05: 越境状態遷移（複数人対応）](#7-std-05-越境状態遷移複数人対応)
8. [STD-06: 画面遷移](#8-std-06-画面遷移)
9. [図間の関係](#9-図間の関係)

---

## 1. 本書の位置づけ

本書は、プロジェクト設計資料「今後の成果物」における成果物2「状態遷移図」に相当する。

生き物行動仕様書（v1.2）の15章に含まれるMermaid図を正式版として清書・拡充し、新規に行動優先度遷移（STD-04）および越境状態遷移（STD-05）を追加したものである。

### 参照ドキュメント

| ドキュメント | 参照箇所 |
|------------|---------|
| プロジェクト設計資料 | 3章（コンセプト）、6章（画面設計）、7章（アーキテクチャ） |
| 生き物行動仕様書 v1.2 | 全章（本書の各図と対応章を図ごとに明記） |

---

## 2. 図の一覧

| 図ID | 図名 | 対象 | 行動仕様書との対応 |
|------|------|------|-----------------|
| STD-01 | 体験フロー全体 | システム全体の状態遷移 | 9章（体験フェーズ連動）、12章（異常系） |
| STD-02 | 生き物ライフサイクル | 個体の生成〜消滅 | 2章（基本定義）、4章（形態と成長） |
| STD-03 | 関係性状態遷移 | 体験者との関係性の変化 | 8章（関係性状態遷移）、10章（通じ合い判定） |
| STD-04 | 行動優先度遷移 | フレームごとの行動選択フロー | 6章（行動ルール）、7章（反応システム） |
| STD-05 | 越境状態遷移 | 複数人時の生き物の所属変化 | 14章（複数人対応拡張方針） |
| STD-06 | 画面遷移 | UI画面間の遷移 | プロジェクト設計資料6章（画面設計） |

---

### 2.1 凡例

本書の状態遷移図は以下の表記規則に従う。

| 表記 | 意味 |
|------|------|
| `[*] -->` | 初期遷移（開始点からの進入） |
| `A --> B : ラベル` | 状態Aから状態Bへの遷移。ラベルは遷移条件 |
| `A --> A : ラベル` | 自己遷移（同じ状態に留まるが内部処理が発生） |
| `state A { ... }` | 複合状態（内部に子状態を持つ） |
| `note` | 補足説明（遷移条件の詳細や設計意図） |
| `\n` | ラベル内の改行（条件が複数ある場合） |

遷移ラベルの形式: `トリガー条件（補足パラメータ）`

### 2.2 互換性に関する注記

- 本書のMermaid図はMermaid v10以降を前提とする
- STD-03内のスコア推移グラフ（`xychart-beta`）はMermaid v10.6以降で対応。未対応レンダラーではテキスト表示になる。その場合は5.3の表を参照のこと

---

## 3. STD-01: 体験フロー全体

システム起動から体験の開始・進行・終了・リセットまでの全体フローを定義する。

### 3.1 遷移図

```mermaid
stateDiagram-v2
    [*] --> Standby : アプリ起動

    state Standby {
        [*] --> WaitingForPerson
        WaitingForPerson : 待機画面（SCR-01）表示
        WaitingForPerson : 待機中の生き物アニメーション
    }

    Standby --> Guidance : 人物検出（信頼度 > 閾値）
    
    state Guidance {
        [*] --> ShowingSilhouette
        ShowingSilhouette : 誘導画面（SCR-02）表示
        ShowingSilhouette : シルエット薄く表示
        ShowingSilhouette : 安定検出カウント開始
    }

    Guidance --> Standby : 検出消失（即時）
    Guidance --> Experience : 3秒安定検出

    state Experience {
        [*] --> Introduction

        state Introduction {
            [*] --> SpawningParticles
            SpawningParticles : 0〜10秒
            SpawningParticles : パーティクル生成開始（20個/秒）
            SpawningParticles : 生成速度補正 ×0.5
        }

        Introduction --> Discovery : 10秒経過

        state Discovery {
            [*] --> FormingLarvae
            FormingLarvae : 10〜25秒
            FormingLarvae : パーティクル集合→幼生化
            FormingLarvae : 生成速度 10個/秒
        }

        Discovery --> Exploration : 25秒経過

        state Exploration {
            [*] --> GrowingAdults
            GrowingAdults : 25〜40秒
            GrowingAdults : 幼生→成体成長促進
            GrowingAdults : 即時反応強度 ×1.2
            GrowingAdults : 30秒で軽ブースト（×1.5）開始
        }

        Exploration --> Connection : 通じ合いトリガー成立
        Exploration --> Connection : 40秒経過（強ブースト ×3.0）

        state Connection {
            [*] --> BondingMoment
            BondingMoment : 通じ合い演出中
            BondingMoment : トリガー個体が「見つめる」
            BondingMoment : 他個体が集合
            BondingMoment : 48秒で未発動なら強制発動
        }

        Connection --> Afterglow : 50秒経過

        state Afterglow {
            [*] --> GentleGathering
            GentleGathering : 50〜60秒
            GentleGathering : 全生き物が輪郭近くに集合
            GentleGathering : 動き穏やか、即時反応 ×0.3
        }
    }

    Experience --> Lingering : 人物離脱
    Experience --> Lingering : 60秒経過

    state Lingering {
        [*] --> Gathering
        Gathering : 集合（0〜3秒）
        Gathering --> Huddling : 3秒
        Huddling : 寄り添い（3〜7秒）
        Huddling --> Dissolving : 7秒
        Dissolving : 消散（7〜10秒）
        Dissolving : 個体が順にフェードアウト
    }

    Lingering --> Standby : 10秒経過（全個体消滅）

    Experience --> ContourFlicker : 輪郭消失（0.5秒未満）
    ContourFlicker --> Experience : 復帰（最後の輪郭位置で待機）

    Experience --> ContourLost : 輪郭消失（0.5秒〜3秒）
    ContourLost --> Experience : 復帰（スコア凍結中）
    ContourLost --> Lingering : 3秒超過

    Experience --> TargetSwitch : 対象者切替検出
    TargetSwitch --> Experience : 2秒遷移完了\n（スコア ×0.8で引き継ぎ）

    Standby --> ManualReset : F5キー
    Experience --> ManualReset : F5キー
    Lingering --> ManualReset : F5キー
    ManualReset --> Standby : 全個体フェードアウト（0.5秒）

    Experience --> SoftReset : F6キー
    SoftReset --> Experience : スコアのみリセット\n（個体は維持）

    Experience --> PerfDegraded : FPS < 30
    PerfDegraded --> Experience : FPS回復（≧ 30）

    state PerfDegraded {
        [*] --> ReduceLoad
        ReduceLoad : パーティクル生成レート 50%削減
        ReduceLoad : 群れ更新を2フレームに1回
        ReduceLoad : 適応ウィンドウ 5秒→3秒
        ReduceLoad --> FurtherReduce : FPS未回復（5秒）
        FurtherReduce : パーティクル上限 50%削減
    }

    Guidance --> Guidance : 対象者切替\n（検出カウントリセットして再開始）
```

### 3.2 状態説明

| 状態 | 説明 | 進入条件 | 退出条件 |
|------|------|---------|---------|
| Standby | 待機。人物を待つ | アプリ起動 / 余韻終了 / リセット | 人物検出 |
| Guidance | 誘導。立ち位置を案内 | 人物検出 | 安定検出3秒 / 検出消失 |
| Introduction | 導入期。パーティクル生成 | 誘導完了 | 10秒経過 |
| Discovery | 発見期。幼生化 | 10秒経過 | 25秒経過 |
| Exploration | 探索期。成体化・反応探索 | 25秒経過 | 通じ合いトリガー / 40秒 |
| Connection | 通じ合い期。最重要瞬間 | トリガー成立 | 50秒経過 |
| Afterglow | 余韻期。穏やかな集合 | 50秒経過 | 60秒経過 / 人物離脱 |
| Lingering | 退場余韻。生き物が散る | 体験終了 | 10秒経過 |
| ContourFlicker | 輪郭短時間消失 | 0.5秒未満の消失 | 復帰 / 0.5秒超過 |
| ContourLost | 輪郭中時間消失 | 0.5秒〜3秒の消失 | 復帰 / 3秒超過 |
| TargetSwitch | 対象者切替中 | 対象者変更検出 | 2秒遷移完了 |
| ManualReset | 手動リセット | F5キー | フェードアウト完了 |
| SoftReset | ソフトリセット | F6キー | 即時（スコアリセットのみ） |
| PerfDegraded | パフォーマンス低下中 | FPS＜30 | FPS回復（≧30） |

> **補足**: Guidance中に対象者が切り替わった場合（別の人が前に立つ等）、安定検出カウントをリセットして再計測する。体験は開始されない。

---

## 4. STD-02: 生き物ライフサイクル

個々の生き物（パーティクル→幼生→成体）の状態遷移を定義する。

### 4.1 遷移図

```mermaid
stateDiagram-v2
    [*] --> Particle : スポーン条件成立\n（輪郭上の重み付きサンプリング）

    state Particle {
        [*] --> Drifting
        Drifting : 輪郭上を漂流
        Drifting : 行動ロジックなし
        Drifting : 個性パラメータなし
        Drifting --> Clustering : 近隣パーティクルと集合開始\n（半径30px以内に8〜12個）
        Clustering : 集合中（2秒カウント）
        Clustering --> Drifting : 集合が解散
    }

    Particle --> Larva : 集合閾値到達\n＋ 集合2秒持続
    Particle --> [*] : タイムアウト（10秒）
    Particle --> [*] : 体験終了
    Particle --> [*] : 画面外に到達

    state Larva {
        [*] --> LarvaActive
        LarvaActive : 脈動しながら活動
        LarvaActive : 個性パラメータ付与
        LarvaActive : 関係性状態: Wary で開始
        LarvaActive : 反応カウント蓄積中
        LarvaActive --> LarvaReacting : 体験者の動きに反応
        LarvaReacting : 反応実行（カウント+1）
        LarvaReacting --> LarvaActive : 反応完了
    }

    Larva --> Adult : 経過5秒以上\n＋ 反応3回以上

    note left of Larva
        成長促進イベントにより
        閾値が一時的に緩和される
        場合がある
    end note

    Larva --> [*] : 体験終了
    Larva --> Adult : 通じ合いトリガー時\n即時成長

    state Adult {
        [*] --> AdultActive
        AdultActive : 成体として活動
        AdultActive : 全行動ルール適用
        AdultActive : 関係性状態遷移あり
        AdultActive : 形態バリエーション適用
    }

    Adult --> LingeringState : 余韻期開始 / 人物離脱

    state LingeringState {
        [*] --> GatheringToContour
        GatheringToContour : 最後の輪郭位置に集合（0〜3秒）
        GatheringToContour --> HuddlingTogether : 3秒
        HuddlingTogether : 寄り添い・脈動（3〜7秒）
        HuddlingTogether --> DissolvingToParticles : 7秒
        DissolvingToParticles : パーティクルに還って散る
    }

    LingeringState --> [*] : フェードアウト完了

    Adult --> [*] : 手動リセット（F5）
    Adult --> TargetTransition : 対象者切替

    state TargetTransition {
        [*] --> MovingToNewContour
        MovingToNewContour : 新輪郭へ補間移動（2秒）
        MovingToNewContour : 即時反応無効
        MovingToNewContour : スコア ×0.8 に減衰
    }

    TargetTransition --> Adult : 新輪郭到着

    Particle --> [*] : パフォーマンス低下時\n強制削減（上限50%化）
    Larva --> [*] : 手動リセット（F5）
```

### 4.2 状態説明

| 状態 | 描画負荷単位 | 行動ロジック | 個性 | 関係性 |
|------|------------|------------|------|--------|
| Particle/Drifting | 1 | なし（VFX制御） | なし | なし |
| Particle/Clustering | 1 | なし（物理演算のみ） | なし | なし |
| Larva | 5 | 基本移動・即時反応 | あり | Waryで開始 |
| Adult | 10 | 全行動ルール | あり | 4状態遷移 |
| LingeringState | 10→0 | 余韻専用行動 | 維持 | 凍結 |
| TargetTransition | 10 | 補間移動のみ | 維持 | 減衰 |

---

## 5. STD-03: 関係性状態遷移

体験者と個々の生き物の間の関係性変化を定義する。幼生化時点で開始し、成体まで継続する。

### 5.1 遷移図

```mermaid
stateDiagram-v2
    [*] --> Wary : 幼生化時点で開始\n（スコア = 0.0）

    state Wary {
        [*] --> WaryIdle
        WaryIdle : 輪郭に密着（0〜10px）
        WaryIdle : 自ら近づかない
        WaryIdle : 急な動きで大きく逃避
        WaryIdle : スコア範囲: 0.00〜0.24
    }

    Wary --> Curious : スコア ≧ 0.25\n（穏やかな動き蓄積）\n目安: 5〜10秒

    state Curious {
        [*] --> CuriousIdle
        CuriousIdle : 輪郭から少し離れて観察（10〜50px）
        CuriousIdle : 動きに追従し始める
        CuriousIdle : 時折自ら近づく
        CuriousIdle : スコア範囲: 0.25〜0.49
    }

    Curious --> Wary : 非常に急な動き\n（スコアが0.25未満に低下）\n※1段階のみ後退

    Curious --> Friendly : スコア ≧ 0.50\n（反応回数＞閾値B\n＋ 逃避なし3秒）\n目安: 8〜15秒

    state Friendly {
        [*] --> FriendlyIdle
        FriendlyIdle : 自由に泳ぎ回る（10〜60px）
        FriendlyIdle : 動きに積極的に反応
        FriendlyIdle : 遊ぶような行動
        FriendlyIdle : スコア範囲: 0.50〜0.74
    }

    Friendly --> Curious : 非常に急な動き\n（スコアが0.50未満に低下）\n※1段階のみ後退

    Friendly --> Bonded : 通じ合いトリガー成立\n（スコア ≧ 0.75\n＋ 穏やか2秒持続\n＋ 25秒以上経過）

    state Bonded {
        [*] --> BondedIdle
        BondedIdle : 体験者の近くに留まる（0〜80px）
        BondedIdle : 動きに寄り添う
        BondedIdle : 特別な発光・反応
        BondedIdle : スコア範囲: 0.75〜1.00
        BondedIdle : ※後退なし（不可逆）
    }

    note right of Wary
        スコア増加要因:
        ・穏やかな動き: +0.02/秒 × curiosity
        ・静止: +0.005/秒
        ・同期: +0.05/秒
        
        スコア減少要因:
        ・急な動き: -0.1/回 × timidity
    end note

    note right of Bonded
        保証メカニズム:
        ・30秒: 増加レート ×1.5
        ・40秒: 増加レート ×3.0
        ・48秒: 強制的にBondedへ
    end note

    note left of Friendly
        対象者切替時:
        スコア ×0.8 に減衰
        → 状態が後退する場合あり
        例: Friendly(0.52) → 0.42 → Curious
        ただしBondedからの後退はなし
    end note
```

### 5.2 遷移条件の詳細

| 遷移 | スコア条件 | 追加条件 | 個性の影響 |
|------|----------|---------|----------|
| Wary→Curious | ≧ 0.25 | なし | curiosity高 → 増加レート ×1.5 |
| Curious→Friendly | ≧ 0.50 | 反応回数＞閾値B、逃避なし3秒 | — |
| Friendly→Bonded | ≧ 0.75 | 通じ合いトリガー全条件（10章参照） | curiosity高 → 先行到達 |
| Curious→Wary | ＜ 0.25 | 急な動き発生 | timidity高 → 減少 ×1.5 |
| Friendly→Curious | ＜ 0.50 | 急な動き発生 | timidity高 → 減少 ×1.5 |
| Bonded→（なし） | — | 後退不可 | — |

### 5.3 スコア推移シミュレーション（典型パターン）

```mermaid
xychart-beta
    title "関係性スコア推移（典型的な体験）"
    x-axis "経過時間（秒）" [0, 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 55, 60]
    y-axis "スコア" 0 --> 1.0
    line "好奇心高い個体" [0, 0.10, 0.22, 0.35, 0.48, 0.55, 0.62, 0.70, 0.78, 0.85, 0.90, 0.92, 0.95]
    line "標準個体" [0, 0.06, 0.14, 0.22, 0.30, 0.38, 0.44, 0.50, 0.58, 0.72, 0.80, 0.85, 0.88]
    line "臆病な個体" [0, 0.04, 0.10, 0.08, 0.16, 0.24, 0.30, 0.36, 0.42, 0.60, 0.75, 0.80, 0.84]
```

> 上図は理想的な体験を想定したシミュレーション。臆病な個体は15秒付近で急な動きにより一時後退する例を含む。30秒以降は保証メカニズムのブーストにより全個体がBondedに到達。

#### xychart未対応環境向けテーブル

| 秒 | 好奇心高 | 標準 | 臆病 | フェーズ | 備考 |
|----|---------|------|------|---------|------|
| 0 | 0.00 | 0.00 | 0.00 | 導入 | 全個体Wary |
| 10 | 0.22 | 0.14 | 0.10 | 発見 | — |
| 15 | 0.35 | 0.22 | 0.08 | 発見 | 臆病個体が急な動きで後退 |
| 25 | 0.48→**Curious** | 0.30→**Curious** | 0.24 | 探索開始 | 好奇心高・標準がCuriousに |
| 30 | 0.55 | 0.38 | 0.30→**Curious** | 探索 | 軽ブースト(×1.5)開始 |
| 40 | 0.70 | 0.50→**Friendly** | 0.42 | 通じ合い期 | 強ブースト(×3.0)開始 |
| 45 | 0.78→**Bonded** | 0.72 | 0.60→**Friendly** | 通じ合い期 | 好奇心高が通じ合い到達 |
| 50 | 0.90 | 0.80→**Bonded** | 0.75→**Bonded** | 余韻 | 全個体Bonded |

---

## 6. STD-04: 行動優先度遷移

各フレームにおける行動選択のフローを定義する。生き物（幼生・成体）ごとに毎フレーム評価される。

### 6.1 遷移図（フレーム単位の評価フロー）

```mermaid
stateDiagram-v2
    [*] --> EvaluateFrame : 毎フレーム

    state EvaluateFrame {
        [*] --> CheckFormStage

        state CheckFormStage {
            [*] --> DetermineStage
            DetermineStage : 形態段階を判定
        }

        CheckFormStage --> LarvaEval : 幼生
        CheckFormStage --> CheckEmergency : 成体

        state LarvaEval {
            [*] --> LarvaActions
            LarvaActions : 利用可能: 緊急反応、即時反応、輪郭追従
            LarvaActions : 利用不可: 通じ合い演出、自律行動
            LarvaActions : 群れ行動: 制限付き（分離のみ）
        }

        LarvaEval --> CheckEmergency : 緊急反応評価へ

        state CheckEmergency {
            [*] --> DetectSuddenMotion
            DetectSuddenMotion : 急な動き検出\n（速度 > suddenThreshold）
        }

        CheckEmergency --> ExecuteFlee : 急な動き検出\n＋ 減衰後強度 > 0
        CheckEmergency --> CheckConnection : 急な動きなし / 減衰で無効

        state ExecuteFlee {
            [*] --> Fleeing
            Fleeing : 逃避行動実行
            Fleeing : 逃避距離 = 80px × timidity変調
            Fleeing : 持続 0.5秒
        }

        state CheckConnection {
            [*] --> EvalConnectionTrigger
            EvalConnectionTrigger : 通じ合いトリガー判定\n（スコア≧0.7, 穏やか2秒, 25秒経過）
        }

        CheckConnection --> ExecuteConnection : トリガー成立
        CheckConnection --> CheckRelationship : トリガー未成立

        state ExecuteConnection {
            [*] --> ConnectionBehavior
            ConnectionBehavior : 通じ合い演出行動
            ConnectionBehavior : 「見つめる」/ 集合 / 特別発光
        }

        state CheckRelationship {
            [*] --> EvalRelationState
            EvalRelationState : 関係性状態に基づく行動判定\n（接近 / 回避 / 観察）
        }

        CheckRelationship --> ExecuteRelationship : 関係性行動あり
        CheckRelationship --> CheckAdaptation : 関係性行動なし

        state ExecuteRelationship {
            [*] --> RelationBehavior
            RelationBehavior : 関係性状態に応じた行動
            RelationBehavior : Wary→密着, Curious→観察, Friendly→遊び, Bonded→寄り添い
        }

        state CheckAdaptation {
            [*] --> EvalAdaptivePattern
            EvalAdaptivePattern : 適応パターン判定\n（慣れ / 興味 / 同期）
        }

        CheckAdaptation --> ExecuteAdaptation : パターン検出
        CheckAdaptation --> CheckAutonomous : パターンなし

        state ExecuteAdaptation {
            [*] --> AdaptiveBehavior
            AdaptiveBehavior : 適応反応実行
            AdaptiveBehavior : 慣れ→反応弱化
            AdaptiveBehavior : 興味→注目・接近
            AdaptiveBehavior : 同期→リズム追従
        }

        state CheckAutonomous {
            [*] --> EvalSelfAction
            EvalSelfAction : 自律行動判定\n（遊泳 / 探索 / 休息）
        }

        CheckAutonomous --> ExecuteAutonomous : 自律行動発動
        CheckAutonomous --> ExecuteContourFollow : 自律行動なし（デフォルト）

        state ExecuteAutonomous {
            [*] --> AutonomousBehavior
            AutonomousBehavior : 遊泳 / 探索 / 休息
            AutonomousBehavior : energy で頻度変調
        }

        state ExecuteContourFollow {
            [*] --> ContourFollowing
            ContourFollowing : 輪郭追従（最低優先度）
            ContourFollowing : アンカーポイントに沿って移動
        }
    }

    ExecuteFlee --> ApplyMovement
    ExecuteConnection --> ApplyMovement
    ExecuteRelationship --> ApplyMovement
    ExecuteAdaptation --> ApplyMovement
    ExecuteAutonomous --> ApplyMovement
    ExecuteContourFollow --> ApplyMovement

    state ApplyMovement {
        [*] --> BlendWithFlock
        BlendWithFlock : Boids力をブレンド\n（分離・整列・結合・輪郭引力）
        BlendWithFlock --> ApplySmoothDamp
        ApplySmoothDamp : SmoothDampで平滑化
        ApplySmoothDamp --> ClampToScreen
        ClampToScreen : 画面境界チェック\n（ソフトバウンダリ 50px）
        ClampToScreen --> UpdatePosition
        UpdatePosition : 位置・回転更新
    }

    ApplyMovement --> UpdateState

    state UpdateState {
        [*] --> UpdateRelationScore
        UpdateRelationScore : 関係性スコア更新
        UpdateRelationScore --> UpdateAdaptBuffer
        UpdateAdaptBuffer : 適応バッファ更新\n（動き特徴量蓄積）
        UpdateAdaptBuffer --> EmitRenderData
        EmitRenderData : CreatureRenderData出力
    }

    UpdateState --> [*] : フレーム完了
```

### 6.2 優先度のまとめ

| 優先度 | 行動 | 評価順 | 割り込み可否 | 幼生 | 成体 |
|--------|------|--------|------------|------|------|
| 1（最高） | 緊急反応（逃避） | 最初に評価 | 全行動を中断して実行 | ○ | ○ |
| 2 | 通じ合い演出 | 緊急なしの場合 | 関係性以下を上書き | × | ○ |
| 3 | 関係性行動 | 通じ合いなしの場合 | 適応以下を上書き | △（接近/回避のみ） | ○ |
| 4 | 適応反応 | 関係性行動なしの場合 | 自律以下を上書き | ○ | ○ |
| 5 | 自律行動 | 適応なしの場合 | 輪郭追従を上書き | × | ○ |
| 6（最低） | 輪郭追従 | 他の行動がない場合 | デフォルト | ○ | ○ |

> すべての行動出力は ApplyMovement でBoids力とブレンドされ、SmoothDampで平滑化される。このため、行動の切り替わりは滑らかに見える。

---

## 7. STD-05: 越境状態遷移（複数人対応）

複数人の体験者がいる場合の、生き物の所属と越境の状態遷移を定義する。フェーズ2（1人版安定後）で実装予定。

> **適用条件**: 本図はSTD-01がExperience状態にあるときのみアクティブ。Standby/Guidance/Lingering中は越境ロジックは評価されない。

### 7.1 遷移図

```mermaid
stateDiagram-v2
    [*] --> Home : 生成時\n（所属体験者の輪郭上でスポーン）

    state Home {
        [*] --> HomeActive
        HomeActive : 所属体験者の輪郭で通常行動
        HomeActive : 関係性スコア: relationships[homeTargetId]
        HomeActive : 全行動ルール適用
    }

    Home --> BoundaryOpen : 所属元と他体験者の\n輪郭間距離 < 100px

    state BoundaryOpen {
        [*] --> Aware
        Aware : 境界開放を認知
        Aware : 他体験者の輪郭を知覚可能に
        Aware --> Approaching : curiosity高い個体が先導\n（curiosity > 0.5）
        Approaching : 2つの輪郭の中間地帯へ移動
        Approaching : 所属元の関係性スコアは維持
    }

    BoundaryOpen --> Home : 輪郭間距離 ≧ 100px\n（境界閉鎖→帰巣）

    BoundaryOpen --> Crossing : 他体験者の輪郭に接触

    state Crossing {
        [*] --> CrossingActive
        CrossingActive : 越境先の輪郭で活動
        CrossingActive : 越境先との関係性スコア蓄積開始
        CrossingActive : relationships[otherTargetId]
        CrossingActive : 所属元のスコアは凍結（増減なし）
        CrossingActive --> CrossingBonding : 越境先でも通じ合い条件成立可能
        CrossingBonding : 越境先の体験者とも「通じ合い」
    }

    Crossing --> Returning : 境界閉鎖\n（輪郭間距離 ≧ 100px）

    state Returning {
        [*] --> ReturningHome
        ReturningHome : 所属元の輪郭へ帰巣
        ReturningHome : 1〜2秒かけて移動
        ReturningHome : 越境先のスコアは凍結（リセットしない）
    }

    Returning --> Home : 所属元の輪郭に到着

    Crossing --> Home : 越境先の体験者が離脱\n（越境先消失→即帰巣）

    Home --> [*] : 体験終了 / リセット
    Crossing --> [*] : 体験終了 / リセット

    note right of Crossing
        越境によるコンセプト体験:
        ・「自分の境界から生まれた生き物が他者に渡る」
        ・「他者の生き物が自分の輪郭に来る」
        ・「2人の間の空間で境界が溶ける」
    end note

    note left of BoundaryOpen
        越境の優先度:
        1. curiosity高い個体が先導
        2. sociability高い個体が追従
        3. timidity高い個体は境界付近で留まる
    end note
```

### 7.2 越境時の関係性データ

```mermaid
stateDiagram-v2
    state "生き物の関係性マップ" as RelMap {
        state "relationships[homeTargetId]" as HomeRel {
            [*] --> Active_Home
            Active_Home : スコア蓄積中
            Active_Home --> Frozen_Home : 越境開始
            Frozen_Home : スコア凍結
            Frozen_Home --> Active_Home : 帰巣完了
        }

        state "relationships[otherTargetId]" as OtherRel {
            [*] --> Inactive
            Inactive : 未接触
            Inactive --> Active_Other : 越境開始
            Active_Other : スコア蓄積中
            Active_Other --> Frozen_Other : 帰巣開始
            Frozen_Other : スコア凍結（リセットしない）
            Frozen_Other --> Active_Other : 再越境
        }
    }
```

### 7.3 状態説明

| 状態 | 所属 | 追従先 | スコア計算 |
|------|------|--------|----------|
| Home | 元の体験者 | 元の体験者 | homeスコア蓄積中 |
| BoundaryOpen/Aware | 元の体験者 | 元の体験者 | homeスコア蓄積中 |
| BoundaryOpen/Approaching | 元の体験者 | 中間地帯 | homeスコア維持 |
| Crossing | 元の体験者（変わらない） | 越境先の体験者 | otherスコア蓄積中、homeスコア凍結 |
| Returning | 元の体験者 | 元の体験者（帰巣中） | 両スコア凍結 |

---

## 8. STD-06: 画面遷移

UI画面間の遷移を定義する。プロジェクト設計資料6.9の画面遷移を正式図として清書。

### 8.1 遷移図

```mermaid
stateDiagram-v2
    [*] --> SCR_01

    state "SCR-01: 待機画面" as SCR_01 {
        [*] --> WaitIdle
        WaitIdle : タイトル「境界生物」表示
        WaitIdle : 待機中の生き物アニメーション
        WaitIdle : サブテキスト表示
    }

    SCR_01 --> SCR_02 : 人物検出

    state "SCR-02: 誘導画面" as SCR_02 {
        [*] --> GuideDisplay
        GuideDisplay : 体験者のシルエット（薄い）
        GuideDisplay : 誘導テキスト（数秒で消える）
        GuideDisplay : 立ち位置の目安表示
    }

    SCR_02 --> SCR_01 : 検出消失（即時）
    SCR_02 --> SCR_03 : 3秒安定検出

    state "SCR-03: 体験画面" as SCR_03 {
        [*] --> ExperienceDisplay
        ExperienceDisplay : 7層レイヤー構成
        ExperienceDisplay : フェーズに応じた視覚変化
        ExperienceDisplay : ※STD-01の体験フローと連動
    }

    SCR_03 --> SCR_04 : 人物離脱 / 60秒経過

    state "SCR-04: 余韻画面" as SCR_04 {
        [*] --> LingeringDisplay
        LingeringDisplay : シルエット消失
        LingeringDisplay : 生き物が散っていく
        LingeringDisplay : 次の体験者を待つ雰囲気
    }

    SCR_04 --> SCR_01 : 10秒経過

    state "SCR-05: 管理画面（オーバーレイ）" as SCR_05 {
        [*] --> AdminPanel
        AdminPanel : パラメータ調整
        AdminPanel : プリセット切替
        AdminPanel : ステータス表示（FPS/メモリ/GPU）
    }

    state "SCR-06: デバッグ画面（オーバーレイ）" as SCR_06 {
        [*] --> DebugPanel
        DebugPanel : 骨格・輪郭・関係性状態表示
        DebugPanel : 生き物ID・ヒートマップ
        DebugPanel : パフォーマンス詳細
    }

    SCR_01 --> SCR_05 : 管理者操作
    SCR_02 --> SCR_05 : 管理者操作
    SCR_03 --> SCR_05 : 管理者操作
    SCR_04 --> SCR_05 : 管理者操作

    SCR_01 --> SCR_06 : デバッグ切替
    SCR_02 --> SCR_06 : デバッグ切替
    SCR_03 --> SCR_06 : デバッグ切替
    SCR_04 --> SCR_06 : デバッグ切替

    note right of SCR_05
        オーバーレイ表示
        閉じると呼び出し元の画面に復帰
        （背面の画面遷移は継続）
    end note

    note right of SCR_06
        オーバーレイ表示
        トグルで表示/非表示切替
        （背面の画面遷移は継続）
    end note
```

> **補足**: SCR-05/SCR-06はオーバーレイのため、背面の画面（SCR-01〜04）の状態遷移は影響を受けない。閉じた場合はオーバーレイが消えるだけで、呼び出し元の画面がそのまま表示される。Mermaid上は遷移先を省略しているが、実装上は「元画面に復帰」となる。

### 8.2 画面とシステム状態の対応

| 画面 | 対応するシステム状態（STD-01） | 表示レイヤー |
|------|---------------------------|------------|
| SCR-01 | Standby | 背景＋待機アニメ |
| SCR-02 | Guidance | 背景＋シルエット＋誘導テキスト |
| SCR-03 | Experience全フェーズ | 7層フルレンダリング |
| SCR-04 | Lingering | 背景＋生き物（フェードアウト中） |
| SCR-05 | 任意（オーバーレイ） | 元画面の上にパネル |
| SCR-06 | 任意（オーバーレイ） | 元画面の上にデバッグ情報 |

---

## 9. 図間の関係

各図がカバーする範囲と相互の依存関係を示す。

```mermaid
flowchart TD
    STD01["STD-01\n体験フロー全体\n（システム状態）"]
    STD02["STD-02\n生き物ライフサイクル\n（個体の生死）"]
    STD03["STD-03\n関係性状態遷移\n（対体験者の関係）"]
    STD04["STD-04\n行動優先度遷移\n（フレーム単位の行動選択）"]
    STD05["STD-05\n越境状態遷移\n（複数人時の所属変化）"]
    STD06["STD-06\n画面遷移\n（UI表示）"]

    STD01 -->|"フェーズが決定"| STD02
    STD01 -->|"フェーズ補正を提供"| STD04
    STD01 -->|"表示画面を決定"| STD06
    STD02 -->|"個体の存在が前提"| STD03
    STD02 -->|"個体の存在が前提"| STD04
    STD03 -->|"関係性状態が行動を変調"| STD04
    STD03 -->|"通じ合い条件を供給"| STD01
    STD05 -->|"越境中は追従先が変化"| STD04
    STD05 -->|"越境中のスコアルール"| STD03
    STD06 -->|"管理画面のパラメータ変更"| STD04
    STD01 -->|"PerfDegraded時に負荷制限"| STD02

    style STD01 fill:#e1f5fe
    style STD02 fill:#f3e5f5
    style STD03 fill:#fff3e0
    style STD04 fill:#e8f5e9
    style STD05 fill:#fce4ec
    style STD06 fill:#f5f5f5
```

### 依存関係の読み方

| 関係 | 説明 |
|------|------|
| STD-01 → STD-02 | 体験フェーズが生き物の生成・成長速度を制御する |
| STD-01 → STD-02（PerfDegraded） | パフォーマンス低下時にパーティクル強制削減を指示する |
| STD-01 → STD-04 | フェーズ別パラメータ補正が行動優先度の評価に影響する |
| STD-01 → STD-06 | システム状態がどの画面を表示するかを決定する |
| STD-02 → STD-03 | 関係性は幼生以降の個体にのみ存在する |
| STD-02 → STD-04 | 行動ロジックは幼生・成体にのみ適用される。形態段階で利用可能な行動が異なる |
| STD-03 → STD-04 | 関係性状態（Wary/Curious/Friendly/Bonded）が行動パターンを変調する |
| STD-03 → STD-01 | 通じ合いトリガーが体験フローのフェーズ遷移を駆動する |
| STD-05 → STD-04 | 越境中は追従先の輪郭が変わり、行動の基準点が変化する |
| STD-05 → STD-03 | 越境中は越境先の体験者に対する関係性スコアが蓄積される |
| STD-06 → STD-04 | 管理画面（SCR-05）でのパラメータ変更が行動パラメータに即時反映される |

---

*以上*
