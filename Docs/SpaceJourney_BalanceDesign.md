# SpaceJourney 数値設計まとめ

最終確定: 2026-03-22  
※バランス調整余地あり（★マーク）の項目は実プレイで調整予定

---

## 1. ソウルステータス計算

### 1-1. MaxStat（転生内ステータス上限）

```
MaxStat[i] = MainStats[i] × 0.40
           + Guardian0[i] × 0.20
           + Guardian1[i] × 0.20
           + Guardian2[i] × 0.20
```

- 主魂1体＋守護霊3体の合計が100%
- 守護霊が少ない（またはない）場合はその分は0扱い

### 1-2. NowStat（転生内ステータス現在値）

```
age < 30 の場合:
  NowStat[i] = round(MaxStat[i] × (8/3 × age + 20) / 100)

age ≥ 30 の場合:
  NowStat[i] = MaxStat[i]
```

- age=0 → MaxStatの20%
- age=30 → MaxStatの100%（以降維持）

### 1-3. lv1Stat（ソウルLv1時の基礎ステータス）

```
lv1Stat = RankBaseStats[rank-1] × jobMul × TalentFactor × 0.1

RankBaseStats = [45, 55, 65, 75, 85, 95, 105, 115, 125, 135]
TalentFactor（才能C中間値） = (1.10 + 1.24) / 2 = 1.17
```

### 1-4. LvNのSoulStat

```
growthFactor = 1 + (6.25 - 1) × ((lv - 1) / (25 - 1))
SoulStat(rank, lv) = lv1Stat × growthFactor
```

---

## 2. ランクアップ設計

### 2-1. 合格ラインレベル（NormalLevels）

| ランクUP | 合格ラインLv | 備考 |
|---|---|---|
| Rank1→2 | Lv5 | |
| Rank2→3 | Lv7 | |
| Rank3→4 | Lv9 | |
| Rank4→5 | Lv11 | |
| Rank5→6 | Lv13 | |
| Rank6→7 | Lv15 | |
| Rank7→8 | Lv17 | |
| Rank8→9 | Lv19 | |
| Rank9→10 | Lv21 | |
| Rank10+ | Lv23 | Lv20がソウル通常上限。ボーナス込みで辛うじて届くライン |

**合格ラインLvとは：** そのランク・そのLvのNowStatがランクUP閾値に届く目安レベル。閾値ちょうどでは確率はほぼ0。statが高いほどランクUP確率が上がる。

### 2-2. ランクUP閾値計算

```csharp
// ReinSim.cs MeetsRankUpStatRequirements() 準拠

targetLv = NormalLevels[rankIndex]          // Easy/Medium
targetLv = NormalLevels[rankIndex] + 2      // Hard

s = (targetLv - 1) / (25 - 1)
growthFactor = 1 + (6.25 - 1) × s
lv1Stat = RankBaseStats[rankIndex] × jobMul × 1.17 × 0.1
threshold = round(lv1Stat × growthFactor)
```

**難易度によるチェック本数：**
- **Easy**（jobTier ≥ 50）：stat倍率上位2つが閾値を超えればOK
- **Medium**（jobTier ≥ 20）：上位4つ
- **Hard**（jobTier < 20）：上位4つ、かつ目標Lv+2分厳しい

### 2-3. ランクUP確率

```
ratio = NowStats[si] / threshold  ← ボトルネックstat（最小ratio）を使用

ratio < 1.0 → 確率0（閾値未満は発火不可）
ratio ≥ 1.0 → prob = min((ratio - 1.0)^1.5 × 0.35, 0.90)
```

| ratio | 確率/年 |
|---|---|
| 1.0（閾値ちょうど） | ≈0% |
| 1.2 | 3% |
| 1.5 | 12% |
| 2.0 | 35% |
| 3.0 | 90% |

### 2-4. ランクUPの基本フロー

```
Rank N ソウル Lv1スタート
    ↓ 同ランクエリアで戦ってレベル上げ
Rank N の合格ラインLvに到達
    ↓ NowStatが閾値に届き、転生シミュでランクUP確率が上がる
    ↓ 転生シミュ実行
Rank N+1 ソウル Lv1リセット
    ↓ 繰り返し
```

- ランクUPは転生シミュによってのみ発生
- 下ランクエリアで戦うことも可能だが効率は2倍悪い（後述）
- 就職（生業確定）時点でRank1スタート

---

## 3. EventFactor設計

### 3-1. 基本仕様

```
EventFactors[5]  // AT=0, DF=1, AGI=2, MAT=3, MDF=4
範囲: 0 〜 40pt（Unity: AddEventFactor でクランプ）
```

### 3-2. pt → 倍率変換

```csharp
// PtToMultiplier (ReinSim.cs 準拠)
maxBonus = 0.10 + (rank - 1) / 9 × 0.40
multiplier = 1.0 + (pt / 40) × maxBonus
```

| pt \ rank | Rank1 | Rank5 | Rank10 |
|---|---|---|---|
| 0pt | 1.000x | 1.000x | 1.000x |
| 20pt | 1.050x | 1.100x | 1.250x |
| 40pt | 1.100x | 1.200x | 1.500x |

### 3-3. 現在のEFバランス（最良パス時の合計pt）

| 合計pt | ジョブ |
|---|---|
| 35〜37pt | ゴルファー・消防士 |
| 29〜32pt | 警察官・剣術師範・傭兵・宇宙飛行士・ボディガード・裁判官・国際弁護人・詐欺師・カメラマン・弓道師範・MSF外科医・手品師・音楽家・カルト教祖・掘削技師・起業家・ロケットエンジニア |
| 17〜27pt | 自衛官・ヤクザ・救急救命士・狩人・占い師・僧侶・研究者・漁師・取り立て屋・配管工 |

★ 下位グループはランクチェーンへのEF設定が不足。要追加。

### 3-4. StatW（sイベントの発生重み）

sイベントに `StatW(STAT, +/-)` タグ付き。

```
+ イベント: weight × (0.5 + NowStats[si]/100)  ← stat高いほど出やすい
- イベント: weight × (1.5 - NowStats[si]/100)  ← stat高いほど出にくい
```

---

## 4. レベルアップ必要EXP

### 4-1. 基本式

```csharp
// SpaceJourneyStatMath.CalcBaseRequiredExpForLevel() 準拠
baseLevelUpExp = 100
levelUpExpFactor = 1.3
```

| Lv | Lv→Lv+1 | 累計EXP |
|---|---|---|
| 1 | 100 | 0 |
| 2 | 130 | 100 |
| 3 | 169 | 230 |
| 4 | 220 | 399 |
| 5 | 286 | 619 |
| 6 | 371 | 905 |
| 7 | 482 | 1,276 |
| 8 | 627 | 1,758 |
| 10 | 1,059 | 3,187 |
| 15 | 3,129 | 13,784 |
| 20 | 9,250 | 55,580 |
| 22 | 15,640 | 98,370（通常カーブ終点） |
| 23 | × 2.0ボーナス | |
| 24 | × 4.0ボーナス | |
| 25 | × 8.0ボーナス | |

**ソウルLv上限：25**（実質Lv20が通常上限、Lv21〜25は極端に重い）

### 4-2. ソウルジョブランク倍率

```
soulJobRankMul = 1.0 + 0.4 × (rank - 1)

Rank1: 1.0x / Rank2: 1.4x / Rank3: 1.8x / Rank5: 2.6x / Rank10: 4.6x
```

転生してランクが上がるほど、同じレベルアップに必要なEXPが増える。

---

## 5. 敵EXP設計

### 5-1. 基本値

```
baseEnemyExpRank1 = 50 ★
enemyRankExpFactor = 2.0

baseExp(rank) = 50 × 2^(rank-1)
```

| Rank | 雑魚EXP | エリート（×2） | 門番（×3） |
|---|---|---|---|
| 1 | 50 | 100 | 150 |
| 2 | 100 | 200 | 300 |
| 3 | 200 | 400 | 600 |
| 4 | 400 | 800 | 1,200 |
| 5 | 800 | 1,600 | 2,400 |

### 5-2. Rank1エリアのテンポ感

```
2体 → Lv2
5体 → Lv3
8体 → Lv4
13体 → Lv5（Rank1→2の合格ライン）
```

### 5-3. 同ランクエリア vs 1つ下エリアの効率比

```
同ランクエリアで合格ラインLvまで: 約12〜24体
1つ下エリアで同じことをする:     約2倍の体数が必要
```

→「下エリアで戦える。でも効率は半分」という設計。

---

## 6. エリア構造

```
Rank Nエリア
  ├ 雑魚キューブ（baseExp(N)）
  ├ エリートキューブ（baseExp(N) × 2）
  └ 門番キューブ（baseExp(N) × 3）
        ↓ 倒すと通行証
  Rank N+1エリアへ（経験値・ドロップが良い）
```

**門番はランクUPとは無関係。次のエリアへの鍵。**

ランクUPは転生シミュで行われ、エリア移動とは独立。

---

## 7. 最初の1体生成

### 7-1. フロー（SoulFactory.Create準拠）

```
SoulFactory.Create(rank=N, soulTendency=T)
  → ジョブ・才能・性別・名前・アイコン全部ランダム抽選
  → OneReinSoulData.CreateFromArgs()で転生データ1件（来歴なし）付き
  → SoulInstance完成

これを4体（主魂1+守護霊3）生成
```

### 7-2. 転生ありソウルを特定ランクで作りたい場合（敵データ用）

転生シミュでそのランクに到達するまで繰り返す。

```
ランクN指定の場合:
  ① rank=N のソウル4体を生成（転生なし）
  ② 転生シミュを実行
  ③ 結果がrank N以上なら採用
  ④ 届かなければ①に戻る（上限回数あり）
```

★ ただしRank1ソウルではNowStatが低くランクUPしにくいため、高ランク指定は多くの試行が必要になる可能性がある。敵データ専用の簡易生成メソッドを別途用意することも検討。

---

## 8. 未解決・要調整

| 項目 | 内容 |
|---|---|
| ★ baseEnemyExpRank1=50 | 実プレイで要調整 |
| ★ EFバランス下位グループ | 自衛官・救急救命士ほか7ジョブのランクチェーンにEF未設定 |
| ★ 門番の具体的なstat設定 | エリアランク×何レベル相当か未定 |
| ★ 高ランク敵データ生成 | 転生シミュ繰り返し方式の実装コスト |
| ★ DF系EFがほぼ0 | 設計上意図的か、漏れか未確認 |
