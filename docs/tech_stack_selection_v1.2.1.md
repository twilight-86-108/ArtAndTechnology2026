# 技術スタック選定書

**境界生物 / Liminal Creature**  
バージョン: 1.2.1 ｜ 作成日: 2026年4月  
変更履歴:  
- v1.0→v1.1 MECEレビュー反映（カメラ入力・輪郭抽出・推論パイプライン・テスト方針・ライセンス・ログ等を追加）  
- v1.1→v1.2 ランタイム/開発時の明確な分離、MLオフラインパイプライン追加、Python環境定義  
- v1.2→v1.2.1 展示運用フロー検証によるMECE補完（ビルド・リリースプロセス、現場キャリブレーション、自動復帰、モデル切替）

---

## 目次

### Part A: 全体方針
1. [本書の位置づけ](#1-本書の位置づけ)
2. [選定方針](#2-選定方針)
3. [技術スタック全体像](#3-技術スタック全体像)

### Part B: ランタイム（実行時）
4. [Unity バージョン](#4-unity-バージョン)
5. [レンダリングパイプライン](#5-レンダリングパイプライン)
6. [Unity 公式パッケージ（ランタイム）](#6-unity-公式パッケージランタイム)
7. [カメラ入力・輪郭抽出](#7-カメラ入力輪郭抽出)
8. [セグメンテーション・AI推論（ランタイム）](#8-セグメンテーションai推論ランタイム)
9. [VFX Graph 連携方針](#9-vfx-graph-連携方針)
10. [サードパーティライブラリ（ランタイム）](#10-サードパーティライブラリランタイム)
11. [C# アーキテクチャ方針](#11-c-アーキテクチャ方針)
12. [設定管理・ホットリロード](#12-設定管理ホットリロード)
13. [ログ・監視](#13-ログ監視)
14. [ハードウェア構成・展示運用](#14-ハードウェア構成展示環境)
    - 14.3 [展示現場キャリブレーション](#143-展示現場キャリブレーション)
    - 14.4 [長時間稼働・自動復帰](#144-長時間稼働自動復帰)

### Part C: 開発時
15. [開発環境・ツール](#15-開発環境ツール)
    - 15.4 [ビルド・リリースプロセス](#154-ビルドリリースプロセス)
16. [MLオフラインパイプライン（Python）](#16-mlオフラインパイプラインpython)
17. [テスト方針](#17-テスト方針)

### Part D: 横断事項
18. [ライセンス一覧](#18-ライセンス一覧)
19. [依存関係マップ](#19-依存関係マップ)
20. [リスクと代替案](#20-リスクと代替案)
21. [バージョンロック表](#21-バージョンロック表)

---

# Part A: 全体方針

## 1. 本書の位置づけ

プロジェクト設計資料「今後の成果物」における成果物3「技術スタック選定書（Unity詳細）」に相当する。プロジェクト設計資料1.3の技術選定（確定）を具体的なパッケージ・バージョンレベルに落とし込む。

本書はランタイム（展示時に実行されるアプリケーション）と開発時（開発中にのみ使用するツール・環境）を明確に分離して記載する。

### 参照ドキュメント

| ドキュメント | 参照箇所 |
|------------|---------|
| プロジェクト設計資料 | 1.3（技術選定）、7章（アーキテクチャ設計）|
| 生き物行動仕様書 v1.2 | 13章（入力処理層インターフェース）、16章（パラメータ一覧） |
| 非機能要件一覧 | NF-101〜108（パフォーマンス）、NF-501（ホットリロード）、NF-601〜604（セキュリティ） |
| 機能要件一覧 | ID 111〜118（セグメンテーション精度・ファインチューニング関連） |

---

## 2. 選定方針

| 方針 | 理由 |
|------|------|
| 安定性重視 | 展示作品のため、長時間稼働の信頼性が最優先（NF-201: 8時間連続稼働） |
| LTSバージョン採用 | 発表が2027年7月中旬。開発中のバージョン変更リスクを排除 |
| 最小依存 | 一人開発のため、学習コスト・メンテコストを最小化。使わない依存を入れない |
| GPU 8GB制約 | セグメンテーション推論＋VFXレンダリングが同一GPUで動作（ランタイム）。ファインチューニングも同一マシン（開発時） |
| オフライン完結 | ネットワーク依存のパッケージ・ライセンス認証は避ける（NF-603） |
| ランタイム/開発時分離 | 展示アプリにはPython等の開発時依存を含めない。成果物（.onnxファイル等）のみをUnityに取り込む |

---

## 3. 技術スタック全体像

> **v1.2追加**: ランタイムと開発時の技術領域を俯瞰する図。

```
┌─────────────────────────────────────────────────────┐
│                   開発時（Dev-time）                   │
│                                                       │
│  ┌──────────────────┐    ┌─────────────────────────┐  │
│  │  Unity開発環境     │    │  MLパイプライン（Python） │  │
│  │  ・Unity Editor   │    │  ・PyTorch              │  │
│  │  ・IDE (VS/Rider) │    │  ・ONNX変換             │  │
│  │  ・Test Framework │    │  ・精度評価              │  │
│  │  ・Profiler       │    │  ・ファインチューニング   │  │
│  │  ・NaughtyAttr    │    │  ・量子化               │  │
│  │  ・Git / Git LFS  │    │  ・データ収集・前処理    │  │
│  └────────┬─────────┘    └──────────┬──────────────┘  │
│           │                         │                  │
│           │    .onnx / .sentis      │                  │
│           │  ←─────────────────────-┘                  │
└───────────┼─────────────────────────────────────────-──┘
            │
            ▼
┌───────────────────────────────────────────────────────┐
│              ランタイム（Runtime / 展示時）              │
│                                                        │
│  Unity 6.3 LTS スタンドアロン .exe                       │
│  ┌──────────┐ ┌───────────┐ ┌───────────────────────┐  │
│  │ URP      │ │VFX Graph  │ │ Inference Engine      │  │
│  │ Shader   │ │パーティクル│ │ セグメンテーション推論  │  │
│  │ Graph    │ │生き物描画  │ │ (.onnx/.sentisを読込)  │  │
│  └──────────┘ └───────────┘ └───────────────────────┘  │
│  ┌──────────┐ ┌───────────┐ ┌───────────────────────┐  │
│  │WebCam    │ │行動ロジック│ │ 設定 (JSON)           │  │
│  │Texture   │ │Burst/Jobs │ │ ホットリロード         │  │
│  └──────────┘ └───────────┘ └───────────────────────┘  │
└────────────────────────────────────────────────────────┘
```

---

# Part B: ランタイム（実行時）

## 4. Unity バージョン

### 4.1 選定: Unity 6.3 LTS (6000.3.x)

| 項目 | 内容 |
|------|------|
| バージョン | Unity 6.3 LTS (6000.3.x) |
| サポート期限 | 2027年12月まで（発表時期の2027年7月を十分にカバー） |
| 選定理由 | Unity 6系の最新LTS。VFX Graph / Shader Graphの最新機能が利用可能。6.0 LTSはサポートが2026年10月で終了するため不適 |

### 4.2 不採用バージョンと理由

| バージョン | 不採用理由 |
|-----------|----------|
| Unity 6.0 LTS | サポートが2026年10月で終了。発表時期（2027年7月）にサポート切れ |
| Unity 2022 LTS | Unity 6系のVFX Graph改善・Shader Graph機能拡張等の恩恵を受けられない |
| Unity 6.2 (Update) | LTSではないため、次のリリースでサポートが終了する可能性 |

### 4.3 レンダリングバックエンド

| 項目 | 選定 | 理由 |
|------|------|------|
| Graphics API | DirectX 12 (Primary) / DirectX 11 (Fallback) | Windows Standalone前提。DX12はCompute Shader性能が優位 |
| Color Space | Linear | 正確な発光・ブレンド処理のため |

---

## 5. レンダリングパイプライン

### 5.1 選定: URP (Universal Render Pipeline)

| 項目 | 内容 |
|------|------|
| パイプライン | URP |
| 選定理由 | 8GB GPU制約下でのパフォーマンス最適化に適している。VFX Graphとの互換性が確保されている |

### 5.2 不採用パイプラインと理由

| パイプライン | 不採用理由 |
|------------|----------|
| HDRP | GPU負荷が高い。8GB GPU + セグメンテーションモデル同時実行で余裕がない |
| Built-in RP | VFX Graphのフル機能が利用できない |

### 5.3 URP設定方針

| 設定 | 値 | 理由 |
|------|-----|------|
| HDR | 有効 | 発光エフェクト（Bloom）に必要 |
| Anti-aliasing | MSAA 2x | 品質と負荷のバランス |
| Post-processing | Bloom, Color Grading のみ | 最小限に絞ってGPU負荷抑制 |
| Shadow | 無効 | 本作品には不要 |
| Depth Texture | 無効（暫定） | 現時点で深度バッファを使うシェーダーは未確定。必要になった場合のみ有効化 |

---

## 6. Unity 公式パッケージ（ランタイム）

ランタイムのビルドに含まれるパッケージのみを記載。開発時専用パッケージは15章参照。

### 6.1 コアパッケージ

| パッケージ | 用途 | 対応アーキテクチャ層 |
|-----------|------|-------------------|
| **Visual Effect Graph** (com.unity.visualeffectgraph) | パーティクル描画、生き物の視覚表現（9章参照） | プレゼンテーション層 |
| **Shader Graph** (com.unity.shadergraph) | カスタムシェーダー（発光、軌跡、シルエット処理） | プレゼンテーション層 |
| **Unity Inference Engine** (com.unity.ai.inference) ※旧Sentis | セグメンテーションモデルのONNX推論（8章参照） | 入力処理層 |
| **Input System** (com.unity.inputsystem) | 管理者ホットキー（F5/F6等） | 基盤層 |
| **TextMeshPro** (com.unity.textmeshpro) | UIテキスト（待機画面、誘導、デバッグ表示） | プレゼンテーション層 |

### 6.2 補助パッケージ

| パッケージ | 用途 | 対応アーキテクチャ層 |
|-----------|------|-------------------|
| **UI Toolkit** (com.unity.ui) | 管理画面（SCR-05）、デバッグ画面（SCR-06） | プレゼンテーション層 |
| **Burst Compiler** (com.unity.burst) | 輪郭解析・Boids計算のJob最適化（11.5参照） | ビジネスロジック層 |
| **Collections** (com.unity.collections) | NativeArray等、Burst/Jobs連携用データ構造 | ビジネスロジック層 |
| **Mathematics** (com.unity.mathematics) | Burst互換の数学演算 | ビジネスロジック層 |

### 6.3 不採用パッケージと理由

| パッケージ | 不採用理由 |
|-----------|----------|
| Cinemachine | カメラは固定。ダイナミックなカメラワーク不要 |
| Unity Audio (新Audio Foundation) | 音響は推奨要件（優先度低）。必要になった場合は後から追加 |
| Addressables | アセットは少量。StreamingAssetsで十分 |
| Netcode for GameObjects | オフラインスタンドアロン。ネットワーク不要 |

---

## 7. カメラ入力・輪郭抽出

### 7.1 カメラ入力

| 項目 | 選定 | 理由 |
|------|------|------|
| API | **WebCamTexture**（Unity標準） | 追加依存なしでWebカメラにアクセス可能 |
| 解像度 | 1280×720 (720p) | NF-303準拠 |
| フレームレート | 30fps | NF-304準拠。レンダリング60fpsとは独立 |
| テクスチャフォーマット | RGBA32 | Inference Engine / Compute Shaderとの互換性 |

#### カメラ→推論のデータフロー

```
WebCamTexture (720p, 30fps)
  → GPU上でリサイズ（Compute Shader / Graphics.Blit）
  → Inference Engine入力テンソル（256×256 or 512×512）
  → 推論実行（8.4参照）
  → セグメンテーションマスク（RenderTexture）
  → 輪郭抽出（7.2参照）
```

### 7.2 輪郭抽出アルゴリズム

| 項目 | 選定 | 理由 |
|------|------|------|
| アルゴリズム | **Marching Squares**（改良版） | バイナリマスクからの等値線抽出に適切。Burstで高速化しやすい |
| 実行環境 | **CPU (Burst + Job System)** | 輪郭点列はCPU側で必要（行動ロジックで使用） |
| 平滑化 | Chaikin's Corner Cutting（2パス） | ID 113準拠 |
| 出力 | Vector2[] (正規化座標 0.0〜1.0) | 行動仕様書13章 InputData.contourPoints に準拠 |

#### 不採用アルゴリズムと理由

| アルゴリズム | 不採用理由 |
|------------|----------|
| OpenCV Canny + FindContours | OpenCV for Unityは有料。ネイティブOpenCVはビルド複雑性増 |
| GPU Compute Shaderによる輪郭検出 | 順序付き点列の生成がGPU上では困難 |
| Sobel/Laplacianエッジ検出 | エッジ画像→順序付き輪郭点列の変換が別途必要 |

### 7.3 曲率計算

スポーン位置選定（行動仕様書5章）に使用。Burst Jobで輪郭抽出と同一フレーム内に実行。

```
曲率 = 隣接3点 (P[i-k], P[i], P[i+k]) の角度変化 / 点間距離
k = 曲率計算の窓幅（デフォルト: 3点）
```

---

## 8. セグメンテーション・AI推論（ランタイム）

本章はランタイムで動作する推論パイプラインを定義する。モデルの学習・変換は16章（MLオフラインパイプライン）を参照。

### 8.1 推論ランタイムの選定

| 項目 | 内容 |
|------|------|
| 主要ランタイム | **Unity Inference Engine**（旧Sentis） |
| 選定理由 | Unity公式のONNX推論エンジン。GPU推論対応。Compute Shaderバックエンドでレンダリングとのテンソル共有が可能 |
| バックエンド | GPUCompute（DX12 Compute Shader） |

### 8.2 セグメンテーションモデル候補

| モデル | 入力解像度 | モデルサイズ | GPU VRAM目安 | 特徴 | 導入フェーズ |
|--------|----------|-----------|------------|------|-----------|
| **MediaPipe SelfieSegmenter** | 256×256 | ~4MB | ~0.3GB | 最速導入。MediaPipe Unity Pluginで動作 | フェーズ1（4〜5月） |
| **BodyPix** (keijiro/BodyPixSentis) | 可変 | ~5-20MB | ~0.5GB | Inference Engine最適化済み。即座にUnity動作 | フェーズ1並行 |
| **MODNet** | 512×512 | ~25MB | ~1GB | 軽量高速。背景マット特化 | フェーズ2（5〜6月） |
| **RMBG-2.0** (BRIA AI) | 1024×1024 | ~170MB | ~2GB | 高精度だがGPU負荷高 | フェーズ2（最終手段） |
| **ファインチューニング済みモデル** | 要検証 | 要検証 | 要検証 | 16章のパイプラインで生成 | フェーズ3（必要な場合） |

#### 選定判断基準

| 基準 | 閾値 |
|------|------|
| 推論速度 | 1フレームあたり16ms以下（60fps）。許容上限33ms（30fps） |
| VRAM使用量 | モデル単体で2GB以下（レンダリングに4GB確保） |
| 明るい環境での精度 | 500lux以上でIoU 0.85以上 |
| 輪郭の安定性 | フリッカー頻度 < 5% |

### 8.3 MediaPipe Unity Plugin

| 項目 | 内容 |
|------|------|
| ライブラリ | homuler/MediaPipeUnityPlugin |
| 対応MediaPipe | 0.10.22 |
| 推論モード | CPU（Windows/macOS。GPU推論は非対応） |
| 利用するTask API | Image Segmenter、Pose Landmarker（ID 107） |

> **注意**: CPU推論のためボトルネックになる可能性あり。Inference Engine（GPU推論）との性能比較が必要。

### 8.4 推論パイプライン設計

| 項目 | 方針 |
|------|------|
| 実行頻度 | カメラフレームレートに同期（30fps）。レンダリング60fpsとは非同期 |
| 実行方式 | 非同期推論（UniTask）。前フレーム結果でレンダリング継続 |
| レイテンシ | 1フレーム遅延を許容（33ms + α） |
| テンソル変換 | GPU上で完結。WebCamTexture → Compute Shader → Inference Engine |
| 結果取り出し | AsyncGPUReadback → CPU側で輪郭抽出 |

```
フレーム N:
  [GPU] WebCamTexture更新
  [GPU] Compute Shader: リサイズ+正規化 → 入力テンソル
  [GPU] Inference Engine: 推論開始（非同期）

フレーム N+1:
  [GPU] 推論完了 → マスクRenderTexture確定
  [GPU→CPU] AsyncGPUReadback: マスクデータ取得
  [CPU] Burst Job: Marching Squares → 輪郭点列
  [CPU] Burst Job: 曲率計算・平滑化
  [CPU] 行動ロジックに InputData として提供

  ※推論が間に合わない場合は前フレームの結果を継続使用
```

### 8.5 モデル切替メカニズム

> **v1.2.1追加**: フォールバック戦略（20.2）を実行する際のモデル切替方法。

| 方法 | 説明 |
|------|------|
| **設定ファイルによる切替（推奨）** | settings.json の `segmentation.modelPath` にモデルファイル名を指定。アプリ起動時に読み込み |
| 管理画面からの切替 | SCR-05にモデル選択ドロップダウンを設置。変更時にInference Engineのモデルを再読み込み（2〜5秒のロード時間） |
| 再ビルド不要 | モデルファイルはStreamingAssetsに配置するため、.onnxファイルの差し替えだけで切替可能 |

```
StreamingAssets/
  ├── models/
  │   ├── selfie_segmenter.onnx    # Level 1
  │   ├── bodypix_mobilenet.onnx   # Level 2
  │   ├── modnet.onnx              # Level 3
  │   ├── modnet_finetuned.onnx    # Level 4（ファインチューニング済み）
  │   └── rmbg2_int8.onnx          # Level 5（量子化済み）
  └── Config/
      └── settings.json            # "segmentation.modelPath": "models/bodypix_mobilenet.onnx"
```

---

## 9. VFX Graph 連携方針

### 9.1 役割分担

| VFX Graphが担当 | C#が担当 |
|----------------|---------|
| パーティクルの描画・アニメーション | パーティクルの生成位置・タイミング決定 |
| 発光・軌跡・モーフィングの視覚表現 | 生き物の行動ロジック・状態管理 |
| 色調変化・シェーダーパラメータの補間 | 個性パラメータ・関係性スコアの計算 |

### 9.2 C# → VFX Graph パラメータ注入

| パラメータ名（VFX側） | 型 | 供給元（C#側） |
|---------------------|-----|-------------|
| CreaturePositions | GraphicsBuffer | 全個体の位置配列 |
| CreatureRotations | GraphicsBuffer | 全個体の回転配列 |
| CreatureStates | GraphicsBuffer | 関係性状態・形態段階のエンコード |
| GlowIntensity | float | 通じ合い演出時の発光強度 |
| PhaseMultiplier | float | 体験フェーズごとの演出係数 |
| ContourTexture | Texture2D | セグメンテーションマスク |

### 9.3 VFXイベントの活用

| イベント | トリガー条件 | VFX側の反応 |
|---------|-----------|-----------|
| OnSpawn | パーティクル生成時 | 輪郭上の指定位置にバースト |
| OnConnection | 通じ合いトリガー発動 | 特別な発光・集合演出 |
| OnPhaseChange | フェーズ遷移時 | 色調・速度の段階的変化 |

---

## 10. サードパーティライブラリ（ランタイム）

ランタイムビルドに含まれるもののみ記載。

### 10.1 採用

| ライブラリ | 用途 | ライセンス | 導入方法 |
|-----------|------|----------|---------|
| **UniTask** (Cysharp) | AI推論の非同期待機、フレーム間の非同期処理 | MIT | UPM Git URL |
| **Newtonsoft.Json** (com.unity.nuget.newtonsoft-json) | 設定ファイル（JSON）の読み書き | MIT | Unity Package Manager |

### 10.2 不採用と理由

| ライブラリ | 不採用理由 |
|-----------|----------|
| UniRx / R3 | 一人開発で学習コスト対効果が低い。UniTaskで十分 |
| VContainer / Zenject | プロジェクト規模に対してオーバーキル |
| DOTween | VFX Graph + Shader Graphで制御 |
| OpenCV for Unity | 有料。Marching Squares自前実装で対応 |

---

## 11. C# アーキテクチャ方針

### 11.1 全体方針

| 項目 | 方針 |
|------|------|
| アーキテクチャ | 4層構造（プロジェクト設計資料7.2準拠）。層間はインターフェースで結合 |
| 状態管理 | 有限状態マシン（自前実装） |
| イベント通知 | C# event / Action<T>。ScriptableObjectベースのイベントチャネル併用 |
| 非同期処理 | async/await + UniTask |
| 高負荷処理 | Burst + Job System（対象は11.5参照） |
| 設定管理 | ScriptableObject＋JSON（12章参照） |

### 11.2 コーディング規約

| 項目 | 規約 |
|------|------|
| 命名 | Microsoft C# Coding Conventions準拠 |
| クラス・メソッド | PascalCase |
| フィールド（private） | _camelCase |
| フィールド（serialized） | camelCase（[SerializeField]） |
| 定数 | PascalCase |
| 名前空間 | LiminalCreature.{Layer}.{Module} |

### 11.3 プロジェクト構成

```
Assets/
├── Scripts/
│   ├── Core/           # アプリケーションコア、状態管理、イベント管理
│   ├── Input/          # カメラ入力、セグメンテーション、輪郭抽出、動き解析
│   ├── Creatures/      # 生き物管理、生成、行動、個性、成長
│   ├── Experience/     # 体験フロー制御、フェーズ管理、通じ合い判定
│   ├── Rendering/      # 背景、シルエット、生き物、エフェクト描画
│   ├── UI/             # 画面管理、管理パネル、デバッグパネル
│   ├── Audio/          # 音響管理（将来対応）
│   ├── Persistence/    # 痕跡保存、設定保存
│   └── Utils/          # ログ、パフォーマンス監視、数学ユーティリティ
├── Tests/              # テスト（15章参照）
│   ├── EditMode/
│   └── PlayMode/
├── Editor/             # Editor専用スクリプト（本番ビルド非含有）
├── VFX/                # VFX Graphアセット
├── Shaders/            # カスタムシェーダー（Shader Graph + HLSL）
├── Resources/Config/   # 設定ファイル（JSON）
├── StreamingAssets/     # MLモデルファイル
└── Plugins/            # ネイティブプラグイン（MediaPipe等）
```

### 11.4 主要な設計パターン

| パターン | 適用箇所 | 理由 |
|---------|---------|------|
| State Pattern | 体験フロー制御、生き物の関係性状態 | STD-01, STD-03の状態遷移を直接実装 |
| Observer / Event | フェーズ遷移通知、通じ合いトリガー | 層間の疎結合 |
| Object Pool | パーティクル、生き物エンティティ | GC回避（8時間連続稼働のメモリ安定性） |
| Strategy | 行動優先度評価 | STD-04の行動カテゴリを差し替え可能に |
| Singleton | AppManager（1つのみ） | 体験フローの単一制御点。最小限に使用 |

### 11.5 Burst + Job System 適用対象

| 処理 | Job化 | 理由 |
|------|------|------|
| Marching Squares（輪郭抽出） | ○ | マスク画像の全ピクセル走査 |
| 輪郭平滑化・曲率計算 | ○ | 輪郭点列の配列操作 |
| Boids力計算 | ○ | N体問題の並列化 |
| 動き特徴量算出 | ○ | 輪郭点列の差分計算 |
| 関係性スコア更新 | × | 個体数が少ない（最大8体） |
| 行動選択（STD-04） | × | 条件分岐が多い |
| 通じ合い判定 | × | 単一判定。コード量が少ない |

---

## 12. 設定管理・ホットリロード

### 12.1 設定の2層構造

| 層 | 形式 | 用途 | 変更タイミング |
|----|------|------|-------------|
| デフォルト設定 | ScriptableObject | 開発時のベースライン値 | ビルド前 |
| ランタイム設定 | JSON | 展示現場でのパラメータ調整。デフォルト値を上書き | 実行中（ホットリロード） |

### 12.2 ホットリロードの仕組み

```
1. JSONファイル (StreamingAssets/Config/settings.json) を監視
   → FileSystemWatcher または ポーリング（5秒間隔）
2. ファイル変更検出時、JSONを読み込み
3. パラメータ差分を検出し、イベント通知
4. 各モジュールがパラメータ更新
```

管理画面（SCR-05）からの変更も同じJSONファイルに書き出す。

---

## 13. ログ・監視

### 13.1 ログ出力

| 項目 | 方針 |
|------|------|
| ライブラリ | Unity標準 `Debug.Log` + カスタムラッパー |
| ログレベル | Error / Warning / Info / Debug の4段階 |
| 出力先 | ファイル（StreamingAssets/Logs/）＋ Unity Console |
| ファイルローテーション | 1時間ごとに分割。24時間保持 |
| リリースビルド | Debug レベルのログは除外（Conditional属性） |

### 13.2 パフォーマンスモニタ

| 指標 | 閾値 |
|------|------|
| FPS | < 30で警告 |
| メモリ増加量 | 1時間で100MB以上増加で警告（NF-202） |
| 推論時間 | > 16msで警告 |

デバッグ画面（SCR-06）にリアルタイム表示。

---

## 14. ハードウェア構成（展示環境）

### 14.1 展示環境

| コンポーネント | 要件 | 備考 |
|-------------|------|------|
| PC | ラップトップ、GPU VRAM 8GB | 開発機 = 展示機 |
| GPU | NVIDIA GeForce RTX 3060 Laptop相当以上 | DX12対応必須 |
| Webカメラ | 720p以上、30fps以上 | USB接続 |
| モニター/プロジェクター | HDMI出力 | 1920×1080想定 |
| スピーカー | 任意 | 音響は推奨レベル |

### 14.2 GPU VRAMバジェット（ランタイム）

| 用途 | 割当 | 備考 |
|------|------|------|
| セグメンテーションモデル | 1〜2GB | モデル選定により変動 |
| VFX Graph | 2GB | パーティクル500単位＋エフェクト |
| URP レンダリング | 2GB | FHD + HDR + Bloom |
| テクスチャ・シェーダー | 1GB | — |
| 予備 | 1GB | 安全マージン |
| **合計** | **8GB** | プロトタイプ完成後に実測・再配分 |

### 14.3 展示現場キャリブレーション

> **v1.2.1追加**: NF-801（設営30分以内）を実現するための現場調整手順と技術選定。

展示会場ごとに照明条件・背景・カメラ位置が異なるため、現場でのキャリブレーションが必要。管理画面（SCR-05）からワンボタンで実行できる設計とする。

#### キャリブレーション項目

| 項目 | 方法 | 技術実装 | 所要時間 |
|------|------|---------|---------|
| 背景キャリブレーション（ID 114） | 人がいない状態で「背景取得」ボタン押下。数フレームの平均画像を背景基準として保存 | WebCamTextureから10フレーム取得→平均化→RenderTextureとして保持 | 5秒 |
| セグメンテーション閾値調整 | 管理者が立ってシルエットを確認しながら信頼度閾値をスライダーで調整 | SCR-05のスライダー → settings.json に即時保存 | 1〜2分 |
| 検出距離・範囲の確認 | 管理者がブース内を歩き、検出可否をデバッグ画面（SCR-06）で確認 | 骨格表示・信頼度表示をリアルタイムで確認 | 2〜3分 |
| 明るさ適応 | 自動。背景キャリブレーション時の平均輝度からカメラ露出を推定 | WebCamTextureの輝度ヒストグラム解析 | 自動 |
| プリセット適用 | 照明条件に合わせた事前定義プリセット（「明るい」「標準」「暗い」）を選択 | SCR-05のプリセットボタン → 閾値群を一括変更 | 即時 |

#### キャリブレーションデータの保存

```
StreamingAssets/Config/
  ├── settings.json          # パラメータ設定（ホットリロード対象）
  └── calibration/
      ├── background.png     # 背景基準画像
      └── calibration.json   # キャリブレーション結果（閾値、輝度情報）
```

起動時にキャリブレーションデータが存在すれば自動読み込み。存在しなければ初回キャリブレーションを誘導する。

### 14.4 長時間稼働・自動復帰

> **v1.2.1追加**: ID 906（自動復帰）の実装アプローチ。

#### アプリケーション内部の復帰（Unity側）

| 異常 | 検出方法 | 復帰動作 |
|------|---------|---------|
| カメラ切断 | WebCamTexture.isPlaying == false | エラー表示 → カメラ再接続を5秒ごとにリトライ |
| FPS持続低下 | 10秒間の平均FPS < 20 | パーティクル上限を段階的に削減。改善しなければ体験をリセット |
| 推論エラー | Inference Engine の例外捕捉 | 前フレームの結果を継続使用。3回連続エラーでモデル再読み込み |
| メモリ閾値超過 | 定期計測（60秒間隔）でVRAM/RAM監視 | ログ警告 → Object Pool強制回収 → 改善しなければアプリ再起動 |

#### 外部ウォッチドッグ（Unity外）

Unityアプリ自体がクラッシュした場合の復帰には、外部プロセスが必要。

| 方法 | 実装 | 備考 |
|------|------|------|
| **バッチスクリプト（推奨）** | .batファイルでアプリを起動し、終了コードを監視。異常終了時に再起動 | 最もシンプル。追加依存なし |
| Windowsタスクスケジューラ | 定期的にプロセス存在を確認し、なければ起動 | 設定が複雑だが確実 |

#### ウォッチドッグスクリプト（watchdog.bat）

```bat
@echo off
:loop
echo [%date% %time%] Starting LiminalCreature...
start /wait LiminalCreature.exe -monitor 1 -screen-fullscreen 1
echo [%date% %time%] Application exited with code %errorlevel%
echo Restarting in 5 seconds...
timeout /t 5 /nobreak
goto loop
```

展示時はこの.batをスタートアップに登録する。正常終了（管理者による意図的な終了）の場合はバッチスクリプト自体をCtrl+Cで停止する。

---

# Part C: 開発時

## 15. 開発環境・ツール

### 15.1 エディタ・IDE

| ツール | 用途 |
|--------|------|
| Visual Studio 2022 / Rider | C# 開発 |
| Unity Editor 6.3 LTS | シーン編集、VFX/Shader Graph |

### 15.2 開発時専用Unityパッケージ

本番ビルドに含まれない、開発支援のみに使用するパッケージ。

| パッケージ | 用途 | 制限方法 |
|-----------|------|---------|
| **Unity Test Framework** (com.unity.test-framework) | ユニット/統合テスト | テストアセンブリのみ |
| **Profile Analyzer** (com.unity.performance.profile-analyzer) | パフォーマンス分析 | エディタ専用 |
| **NaughtyAttributes** | Inspector拡張 | Editor専用Assembly Definition |

### 15.3 バージョン管理

| ツール | 設定 |
|--------|------|
| Git | ソースコード管理 |
| Git LFS | VFX Graphアセット、MLモデルファイル（.onnx, .sentis）、テクスチャ |
| .gitignore | Unity標準 + Library/, Temp/, Build/, Logs/ |

### 15.4 ビルド・リリースプロセス

> **v1.2.1追加**: 展示用.exeに必要な設定を追加。

| 項目 | 内容 |
|------|------|
| ターゲット | Windows Standalone (x86_64) |
| 出力形式 | .exe（スタンドアロン） |
| スクリプトバックエンド | IL2CPP（本番）/ Mono（開発時） |

#### 展示用ビルド設定

| 設定 | 値 | 理由 |
|------|-----|------|
| 解像度 | 1920×1080 固定 | 展示用モニター/プロジェクターに合わせる |
| ウィンドウモード | Exclusive Fullscreen | ボーダーレスだとOS通知が表示される可能性 |
| ディスプレイ選択 | 起動引数 `-monitor N` で指定 | ラップトップ画面と外部出力を区別 |
| VSync | ON (1) | 画面ティアリング防止 |
| ターゲットフレームレート | 60fps | Application.targetFrameRate = 60 |
| スプラッシュスクリーン | Unity Personal: ロゴ表示後に待機画面へ遷移 | Personal版の場合ロゴは必須 |
| カーソル | 非表示 + ロック | Cursor.visible = false |
| スクリーンセーバー抑制 | Application.runInBackground = true | 8時間稼働中にスリープさせない |

#### Windows自動起動設定

展示時にPCの電源投入だけでアプリケーションが起動する構成:

```
方法: Windowsスタートアップフォルダにショートカット配置
  shell:startup/LiminalCreature.lnk
  → "LiminalCreature.exe" -monitor 1 -screen-fullscreen 1

補助設定:
  ・Windows自動ログオン設定（netplwiz）
  ・Windows Update の一時停止（展示期間中）
  ・通知の無効化（集中モード）
  ・電源プラン: スリープなし
```

#### リリースチェックリスト

| # | 確認項目 | 備考 |
|---|---------|------|
| 1 | IL2CPPビルドが正常に完了するか | Monoビルドとの動作差異確認 |
| 2 | StreamingAssetsにモデルファイルが含まれているか | .onnx or .sentis |
| 3 | StreamingAssets/Config/settings.json が同梱されているか | デフォルト設定 |
| 4 | Licensesフォルダが同梱されているか | MIT/Apache 2.0表示義務 |
| 5 | フルスクリーン起動が正常に動作するか | 外部ディスプレイで確認 |
| 6 | 30分以上の連続稼働テスト | メモリリーク、FPS安定性 |
| 7 | F5/F6ホットキーが動作するか | 管理者操作の確認 |
| 8 | ログファイルが正常に出力されるか | StreamingAssets/Logs/ |

---

## 16. MLオフラインパイプライン（Python）

> **v1.2追加**: ランタイムには含まれない、開発時のみ使用するML関連の技術スタック。

### 16.1 概要と目的

| 目的 | 内容 | 対応機能要件 |
|------|------|------------|
| セグメンテーション精度評価 | 展示環境条件でのモデル比較・定量評価 | ID 111 |
| モデル変換 | PyTorch/TF → ONNX → .sentis | — |
| ファインチューニング | 展示環境に特化した精度改善 | ID 116, 117 |
| モデル軽量化 | 量子化・プルーニングによるVRAM/速度最適化 | ID 118 |
| データ収集・前処理 | 学習データの整備 | ID 116 |

### 16.2 Python環境

| 項目 | 選定 | 理由 |
|------|------|------|
| Python | 3.10〜3.11 | PyTorch 2.x系の安定サポート範囲 |
| パッケージ管理 | **venv** + pip | 最小構成。conda不要 |
| 仮想環境名 | `liminal-ml` | Unity プロジェクトとは別ディレクトリに配置 |

### 16.3 Pythonパッケージ

| パッケージ | バージョン目安 | 用途 |
|-----------|-------------|------|
| **PyTorch** | 2.x + CUDA 11.8/12.1 | ファインチューニング、モデル定義 |
| **torchvision** | PyTorch対応版 | データ前処理、モデルバックボーン |
| **onnx** | 1.16+ | ONNXモデルの検証・操作 |
| **onnxruntime** | 1.18+ | ONNX推論テスト（Pythonでの精度検証） |
| **onnx-simplifier** | 0.4+ | ONNXグラフの最適化・簡素化 |
| **opencv-python** | 4.x | 画像前処理、マスク操作、IoU計算 |
| **numpy** | 1.x | 数値計算 |
| **Pillow** | 10.x | 画像読み書き |
| **matplotlib** | 3.x | 精度評価結果の可視化 |
| **tqdm** | 4.x | 学習進捗表示 |

### 16.4 ディレクトリ構成

```
liminal-creature/                   # リポジトリルート
├── UnityProject/                   # Unity プロジェクト
│   ├── Assets/
│   ├── Packages/
│   └── ...
├── ml/                             # MLパイプライン（Python）
│   ├── requirements.txt            # pip install -r requirements.txt
│   ├── data/
│   │   ├── raw/                    # 収集した生画像
│   │   ├── masks/                  # アノテーション（セグメンテーションマスク）
│   │   └── processed/             # 前処理済みデータセット
│   ├── models/
│   │   ├── pretrained/            # ダウンロードした事前学習済みモデル
│   │   ├── finetuned/             # ファインチューニング済みモデル (.pth)
│   │   └── exported/              # ONNX / .sentis エクスポート済み
│   ├── scripts/
│   │   ├── evaluate.py            # 精度評価（IoU, フリッカー率）
│   │   ├── finetune.py            # ファインチューニング実行
│   │   ├── export_onnx.py         # PyTorch → ONNX 変換
│   │   ├── quantize.py            # モデル量子化（INT8/UINT8）
│   │   ├── simplify_onnx.py       # ONNX最適化
│   │   ├── capture_data.py        # Webカメラからの学習データ収集
│   │   └── prepare_dataset.py     # データ前処理・分割
│   ├── notebooks/
│   │   └── analysis.ipynb         # 精度分析・可視化
│   └── configs/
│       └── finetune_config.yaml   # 学習ハイパーパラメータ
└── docs/                           # 設計ドキュメント
```

### 16.5 ファインチューニングワークフロー

```
Step 1: データ収集
  ・別環境でWebカメラ映像を録画（scripts/capture_data.py）
  ・照明条件を展示環境に近づける（300〜1000lux）
  ・公開データセット（Supervisely Person Dataset等）で補完
  ・目標: 500〜2000枚程度

Step 2: アノテーション
  ・事前学習済みモデルで自動アノテーション → 手動補正
  ・ツール: LabelMe or CVAT（セグメンテーションマスク作成）

Step 3: ファインチューニング
  ・ベースモデル: MODNet or BodyPix（軽量モデルを選択）
  ・学習率: 1e-4（事前学習済み重みからの微調整）
  ・バッチサイズ: 2〜4（8GB VRAM制約）
  ・エポック数: 20〜50
  ・データ拡張: 明るさ変動、コントラスト変動、水平反転

Step 4: 精度評価（scripts/evaluate.py）
  ・IoU（Intersection over Union）
  ・境界精度（Boundary F1 Score）
  ・フリッカー率（フレーム間マスク変動量）
  ・推論速度（onnxruntime でプロファイル）

Step 5: ONNX変換（scripts/export_onnx.py）
  ・torch.onnx.export() でONNXエクスポート
  ・opset version: 15以下（Inference Engine互換）
  ・onnx-simplifier で最適化

Step 6: 量子化（必要な場合）（scripts/quantize.py）
  ・onnxruntime の quantize_dynamic / quantize_static
  ・INT8量子化でモデルサイズ・推論速度を改善
  ・量子化後の精度劣化を evaluate.py で確認

Step 7: Unity取り込み
  ・.onnx ファイルを UnityProject/Assets/StreamingAssets/ にコピー
  ・Inference Engine で読み込みテスト
  ・ランタイムでの推論速度・精度を確認
```

### 16.6 8GB GPU でのファインチューニング制約と対策

ラップトップGPU（VRAM 8GB）でファインチューニングを行う場合の制約:

| 制約 | 対策 |
|------|------|
| バッチサイズが小さい（2〜4） | Gradient Accumulation で実効バッチサイズを拡大（4ステップ蓄積 → 実効8〜16） |
| 入力解像度が制限される | 256×256 or 512×512 で学習。高解像度モデルは対象外 |
| 大型モデル（RMBG-2.0等）は学習不可 | 軽量モデル（MODNet, BodyPix）のみ対象 |
| 学習とUnity開発の同時実行不可 | 学習中はUnity Editorを閉じる。学習は夜間バッチ実行推奨 |
| メモリ不足で学習がクラッシュ | Mixed Precision（torch.cuda.amp）でVRAM使用量を約40%削減 |
| 学習速度が遅い | 小規模データセットで微調整（フルスクラッチ学習はしない） |

#### Mixed Precision設定例

```python
scaler = torch.cuda.amp.GradScaler()
for batch in dataloader:
    optimizer.zero_grad()
    with torch.cuda.amp.autocast():
        output = model(batch['image'])
        loss = criterion(output, batch['mask'])
    scaler.scale(loss).backward()
    scaler.step(optimizer)
    scaler.update()
```

### 16.7 セグメンテーション精度評価スクリプト仕様

Unity側でも使用する評価基準と同一の指標をPython側で計測する。

| 指標 | 計算方法 | 合格基準 |
|------|---------|---------|
| IoU (mIoU) | 予測マスクとGTマスクの積集合/和集合 | ≧ 0.85 |
| Boundary F1 | 輪郭ピクセルの一致度（距離閾値3px） | ≧ 0.80 |
| フリッカー率 | 連続フレーム間のマスク差分面積の平均 | < 5% |
| 推論速度 | onnxruntime プロファイル | < 16ms/frame |

### 16.8 成果物の受け渡し

MLパイプラインからUnityへ渡す成果物は以下のみ。Python環境自体はランタイムに含まない。

| 成果物 | ファイル形式 | 配置先 |
|--------|-----------|--------|
| セグメンテーションモデル | .onnx or .sentis | UnityProject/Assets/StreamingAssets/ |
| 精度評価レポート | .md or .html | docs/ |
| 量子化済みモデル（使用時） | .onnx | UnityProject/Assets/StreamingAssets/ |

---

## 17. テスト方針

### 17.1 テスト種別

| テスト種別 | フレームワーク | 対象 | 実行タイミング |
|-----------|-------------|------|-------------|
| ユニットテスト（EditMode） | Unity Test Framework | 行動ロジック、スコア計算、輪郭解析、個性生成 | コミット前 |
| 統合テスト（PlayMode） | Unity Test Framework | 体験フロー遷移、通じ合い判定、フェーズ連動 | 週次 |
| パフォーマンステスト | Unity Profiler + カスタム計測 | FPS、メモリリーク、推論速度 | プロトタイプ後、定期 |
| ML精度テスト | Python (scripts/evaluate.py) | セグメンテーション精度（IoU, フリッカー率） | モデル変更時 |
| 展示環境テスト | 手動 | セグメンテーション精度、照明条件、長時間稼働 | 展示前リハーサル |

### 17.2 テスト優先度

| 優先度 | 対象 | 理由 |
|--------|------|------|
| 高 | 関係性スコア計算 | 通じ合い到達に直結 |
| 高 | 通じ合い保証メカニズム | 48秒強制発動の安全弁 |
| 高 | ML精度評価 | コア体験の成立条件 |
| 中 | Boids計算 | Burst Job化後の正しさ |
| 中 | 体験フェーズ遷移 | 時間ベース遷移の正確性 |
| 低 | VFXパラメータ注入 | 目視確認で十分 |

---

# Part D: 横断事項

## 18. ライセンス一覧

### 18.1 ランタイム依存

| パッケージ | ライセンス | 表示義務 |
|-----------|----------|---------|
| Unity Engine | Unity EULA | Personalプランの場合、起動時ロゴ表示 |
| URP / VFX Graph / Shader Graph等 | Unity Companion License | なし |
| Inference Engine | Unity Companion License | なし |
| UniTask | MIT | LICENSE表示推奨 |
| Newtonsoft.Json (Unity配布) | MIT | LICENSE表示推奨 |
| MediaPipe Unity Plugin | MIT | LICENSE表示推奨 |
| MediaPipe (本体) | Apache 2.0 | NOTICE表示必要 |

### 18.2 開発時専用

| パッケージ | ライセンス | 備考 |
|-----------|----------|------|
| NaughtyAttributes | MIT | ビルド非含有 |
| Unity Test Framework | Unity Companion License | ビルド非含有 |
| PyTorch | BSD-3 | 開発環境のみ |
| onnxruntime | MIT | 開発環境のみ |
| OpenCV (Python) | Apache 2.0 | 開発環境のみ |
| 公開データセット | 各データセットに依存 | 要確認（Supervisely: CC BY等） |

### 18.3 展示時のライセンス表示方針

- ランタイム依存のLICENSE/NOTICEファイルをアプリケーション同梱ディレクトリ（/Licenses/）に配置
- 管理画面（SCR-05）に「ライセンス情報」ボタンを設置

---

## 19. 依存関係マップ

### 19.1 ランタイム

```
Unity 6.3 LTS スタンドアロン .exe
├── URP (com.unity.render-pipelines.universal)
│   ├── Shader Graph
│   └── Post-processing (URP内蔵)
├── VFX Graph (com.unity.visualeffectgraph)
├── Inference Engine (com.unity.ai.inference)
│   └── .onnx / .sentis モデルファイル ← [MLパイプラインから供給]
├── Input System (com.unity.inputsystem)    ← ホットキーのみ
├── Burst / Collections / Mathematics
├── TextMeshPro / UI Toolkit
├── UniTask (Cysharp)
├── Newtonsoft.Json
└── [Optional] MediaPipe Unity Plugin (homuler)
    └── MediaPipe Native Libraries (CPU, Apache 2.0)
```

### 19.2 開発時

```
開発用ラップトップ
├── Unity Editor 6.3 LTS
│   ├── Unity Test Framework
│   ├── Profile Analyzer
│   └── NaughtyAttributes (Editor専用)
├── IDE (Visual Studio 2022 / Rider)
├── Git / Git LFS
└── Python 3.10〜3.11 (venv: liminal-ml)
    ├── PyTorch 2.x + CUDA
    ├── onnx / onnxruntime / onnx-simplifier
    ├── opencv-python
    └── numpy / Pillow / matplotlib / tqdm
```

### 19.3 開発時→ランタイムの受け渡し

```
[Python MLパイプライン]           [Unity ランタイム]
  finetune.py                      
  → model.pth                     
  export_onnx.py                   
  → model.onnx    ─── コピー ──→  StreamingAssets/model.onnx
  quantize.py                      
  → model_int8.onnx ─ コピー ──→  StreamingAssets/model_int8.onnx
                                    
  evaluate.py                      
  → report.md     ─── コピー ──→  docs/evaluation_report.md
```

---

## 20. リスクと代替案

### 20.1 技術リスクと対策

| リスク | 影響 | 対策 |
|--------|------|------|
| MediaPipe Unity PluginがUnity 6.3で動作しない | プロトタイプ遅延 | BodyPixSentis（Inference Engine上）を並行準備 |
| Inference Engineが特定ONNX OPをサポートしない | モデル選択肢が狭まる | opsetバージョン事前確認。onnx-simplifierで最適化 |
| GPU 8GBでモデル＋VFXが収まらない | フレームレート低下 | モデル量子化。VFX品質の動的調整 |
| 8GB GPUでファインチューニングが困難 | 精度改善できない | Mixed Precision + Gradient Accumulation。それでも不足なら軽量モデルのみ対象 |
| VFX Graphで複雑な行動制御が困難 | 表現力の制約 | 行動ロジックはC#、VFXは視覚のみ（9章の役割分担） |
| 8時間連続稼働でメモリリーク | 展示中にクラッシュ | Object Pool。Profiler定期計測。自動復帰（ID 906） |
| AsyncGPUReadbackの遅延が想定超 | 輪郭データの遅延増大 | Readback前倒し。最悪ケースでは推論15fpsに削減 |
| ファインチューニング用データが不足 | モデル精度が上がらない | 公開データセット併用。データ拡張で補完 |

### 20.2 セグメンテーションの段階的フォールバック

```
Level 1: MediaPipe SelfieSegmenter（最速導入）
  ↓ 精度不足
Level 2: BodyPixSentis（Inference Engine GPU推論）
  ↓ 精度不足
Level 3: MODNet ONNX → Inference Engine
  ↓ 精度不足
Level 4: Level 2 or 3 のファインチューニング（16章パイプライン）
  ↓ 精度不足 or VRAM不足
Level 5: RMBG-2.0 + モデル量子化
  ↓ それでもダメな場合
Level 6: 背景差分 + MediaPipe のハイブリッド（精度補完）
```

> **v1.2変更**: ファインチューニングをLevel 4として独立させた。

---

## 21. バージョンロック表

### 21.1 ランタイム

| パッケージ / ツール | バージョン | ロック理由 |
|------------------|----------|----------|
| Unity Editor | 6.3 LTS (6000.3.x) | LTSの安定性 |
| URP | 17.x（Unity 6.3同梱版） | Unity同梱版に準拠 |
| VFX Graph | 17.x（Unity 6.3同梱版） | 同上 |
| Shader Graph | 17.x（Unity 6.3同梱版） | 同上 |
| Inference Engine | 2.4.x | Unity 6.3対応の最新安定版 |
| Burst | Unity 6.3同梱版 | 同上 |
| Input System | Unity 6.3同梱版 | 同上 |
| MediaPipe Unity Plugin | 最新stable（要Unity 6.3動作確認） | 互換性確認後にロック |
| UniTask | 2.x latest | 確認後にコミットハッシュ固定 |
| Newtonsoft.Json | 3.x（Unity公式配布版） | Unity同梱版に準拠 |

### 21.2 開発時

| パッケージ / ツール | バージョン | ロック理由 |
|------------------|----------|----------|
| Unity Test Framework | Unity 6.3同梱版 | 同上 |
| NaughtyAttributes | latest stable | Editor専用 |
| Python | 3.10〜3.11 | PyTorch互換性 |
| PyTorch | 2.x + CUDA 11.8 or 12.1 | GPU 8GB互換 |
| onnxruntime | 1.18+ | opset 15サポート |
| onnx-simplifier | 0.4+ | — |

> **運用ルール**: 開発期間中のパッケージ更新は、バグ修正パッチ（x.x.N）のみ許可。マイナー・メジャー更新は行わない。

---

*以上*
