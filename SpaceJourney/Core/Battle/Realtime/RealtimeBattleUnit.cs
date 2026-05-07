using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// リアルタイム戦闘での1ユニットの挙動。各3Dモデルにアタッチ。
    /// 最小プロトタイプ: 基本攻撃 + 職別距離移動のみ。
    /// </summary>
    public class RealtimeBattleUnit : MonoBehaviour
    {
        public enum AttackRangeCategory
        {
            Close = 0,    // 1 mass
            Mid = 1,      // 2 mass
            Far = 2,      // 5 mass
            MaxFar = 3,   // 6 mass
        }

        // ロジックデータ参照
        public SpaceJourneyUnit unit;
        public int ownerSide;  // 0=味方, 1=敵
        public string displayName;

        // 戦闘ビヘイビア設定
        public AttackRangeCategory preferredRange = AttackRangeCategory.Close;
        public float walkSpeed = 1.0f;  // 世界単位/秒
        public float basicAttackDamage = 10f;
        public float basicAttackCooldownSec = 0.8f;
        [Tooltip("攻撃アニメーションの拘束時間 (この間は移動しない、秒)")]
        public float attackAnimDurationSec = 0.7f;
        [Tooltip("攻撃アニメ終了後、回転/AIPath 移動を再開するまでの猶予 (秒)。ルートモーションはこの間も有効")]
        public float postAttackLockSec = 0.3f;
        [Header("ターゲット優先度リスト")]
        [Tooltip("上から評価し dominant を決定。dominant が変わった時のみ再選択 (振動防止)。空ならデフォルト (敵最寄)。")]
        public List<RealtimeTargetEntry> targetList = new();

        [Header("優先度行動リスト")]
        [Tooltip("上から順に条件を評価し、最初にマッチしたアクションを実行。空なら既定ルール適用。")]
        public List<RealtimeActionEntry> actionList = new();

        [Header("スキル (index 0 が基礎スキル、ActionEntry から index 参照)")]
        public List<RealtimeSkillDefinition> skills = new();

        [Header("再評価間隔 (秒)")]
        [Tooltip("ターゲットリスト再評価の周期。AGI で短縮想定、starter から上書き")]
        public float decisionIntervalSec = 1.0f;

        // ===== Skill Timeline Editor のテストプレイ用 =====
        // true: AI 自動更新を停止し、DebugCastSkill 経由でのみスキルが発動する。
        // ProcessPendingEvents は通常通り動くので、放たれた矢/エフェクトは継続して飛ぶ。
        [System.NonSerialized] public bool testPlayManualMode = false;

        [Tooltip("スキルCTグローバル倍率 (starter から設定、AGI込み)")]
        public float skillCooldownMul = 1.0f;

        [Header("ガード (装備盾から設定。騎士専用想定)")]
        [Tooltip("被弾時にガードが発動する基本確率 (0-1)。0.2=20%。装備盾から設定される。")]
        [Range(0f, 1f)] public float guardChance = 0f;
        [Tooltip("ガード発動時のダメージ軽減率 (0-1)。0.3=30%軽減。")]
        [Range(0f, 1f)] public float guardMitigation = 0f;
        [Tooltip("ガード発動時に追加判定するパッシブ (盾の副次効果)。盾の guardChance で判定")]
        public List<RealtimePassiveDefinition> onGuardEffects = new List<RealtimePassiveDefinition>();

        [Header("装備中武器からの有効パッシブ")]
        [Tooltip("装備武器の weaponPassives が実行時にここに積まれる。")]
        public List<RealtimePassiveDefinition> activePassives = new List<RealtimePassiveDefinition>();

        // ===== Buff 系一時状態 =====
        // 戦士 rank 5 Charge: 次の通常攻撃ダメ 1.5x、1回消費
        [HideInInspector] public bool chargeActive = false;

        // ===== ボディスキル パッシブ (rank 取得で常時有効) =====
        // 戦士 rank 3 追加攻撃: 通常攻撃ヒット時に同 target に 0.3x の追撃
        [HideInInspector] public bool hasExtraAttack = false;
        // ランサー rank 3 遠突: 通常攻撃 (Thrust) の range/length に +1m
        [HideInInspector] public bool hasLongThrust = false;
        // ランサー rank 5 ひるみ突き: 通常攻撃ヒット時に flinch 付与
        [HideInInspector] public bool hasFlinchBasic = false;
        // アーチャー rank 3 貫通射撃: 通常攻撃が直線上の敵を全員貫通
        [HideInInspector] public bool hasPierceShot = false;
        [Tooltip("貫通射撃 (rank3) 発動時、矢に親子で取り付ける VFX (= 矢を芯に螺旋する竜巻)。null なら表示なし")]
        public GameObject pierceShotVfxPrefab;
        [Tooltip("pierceShotVfxPrefab の矢ローカル回転 (デフォルト Euler(90,0,0) = tornado の縦軸を矢の forward に合わせる)")]
        public Vector3 pierceShotVfxLocalEuler = new Vector3(90f, 0f, 0f);
        [Tooltip("pierceShotVfxPrefab の矢ローカルスケール乗数 (1=プレハブ既定)")]
        public float pierceShotVfxScale = 1f;
        // アーチャー rank 5 トライアロー (Tri-Arrow): 通常攻撃が扇状 ±15° (合計 30°) に 3 本同時発射、各 0.9x
        [HideInInspector] public bool hasTripleArrow = false;

        // ===== Passive 由来 状態 (AlwaysWhileActive で装備武器から設定) =====
        [HideInInspector] public float basicAttackMul = 1.0f;
        [HideInInspector] public float skillMul = 1.0f;
        [HideInInspector] public float cooldownMul = 1.0f;  // CT 倍率 (0.9x 等)
        [HideInInspector] public float healMul = 1.0f;
        [HideInInspector] public float rangeBonus = 0.0f;   // 全スキル共通: rangeMax + rangeBonus
        [HideInInspector] public float basicRangeBonus = 0.0f; // 通常攻撃 (skills[0]) のみ追加で +basicRangeBonus (竜牙穿ち)
        [HideInInspector] public float rangeMul = 1.0f;     // 全スキル共通: rangeMax × rangeMul (魔王の射程 1.2 倍 等)
        [HideInInspector] public float shapeSizeBonus = 0.0f; // length/width + amount(m)
        [HideInInspector] public float basicShapeMul = 1.0f; // 通常攻撃 (skills[0]) shape の length/width 倍率 (太陽の膨張 1.5 倍)
        [HideInInspector] public bool basicKindOverrideValid = false;
        [HideInInspector] public SkillDamageKind basicKindOverride = SkillDamageKind.Physical;
        [HideInInspector] public bool basicAttackDisabled = false;
        [HideInInspector] public int multiShotCount = 1;

        // ===== Passive 評価用 追跡フィールド =====
        private int _basicAttackHitCount = 0;
        private RealtimeBattleUnit _lastHitTarget = null;
        private int _sameTargetHitStreak = 0;
        private int _lastDealtDamage = 0;  // OnDealDamage 直前にセット。 LifestealPercent / HealAlliesOnHitPercent から参照
        private int _damagedPendingCounter = 0; // OnDamagedThenNextHit 用フラグ
        private float _lastHpPercentForTrigger = 100f; // OnSelfHpBelowPercent 閾値跨ぎ検出
        private float _nextHitDamageReductionRatio = 0f; // NextHitDamageReduction 効果の残量
        private bool _battleStartFired = false;

        private class PassiveRuntimeState
        {
            public int triggerCount = 0;
            public float nextReadyTime = 0f;
            // OnNthBasicAttackHit 用 window-random 状態
            public int windowAttackCount = 0;       // 周期内 何発目か
            public int windowFireAtOffset = 0;      // 周期内 何発目で発動するか (1..N)、 0=未選択
            public bool windowFired = false;        // 今周期で既に発動したか
        }
        private readonly System.Collections.Generic.Dictionary<RealtimePassiveDefinition, PassiveRuntimeState> _passiveStates
            = new System.Collections.Generic.Dictionary<RealtimePassiveDefinition, PassiveRuntimeState>();

        // ConditionalCast (timeline event) ごとの window-random 状態
        private class WindowState
        {
            public int attackCount = 0;
            public int fireAtOffset = 0;
            public bool fired = false;
        }
        private readonly System.Collections.Generic.Dictionary<RealtimeSkillEvent, WindowState> _conditionalCastWindowStates
            = new System.Collections.Generic.Dictionary<RealtimeSkillEvent, WindowState>();

        // 内部状態
        private float nextAttackTime = 0f;
        // ContinuousMover.IsAnchored から参照されるため internal に
        internal float attackingUntil = 0f; // この時刻まで移動禁止
        private float[] skillNextReadyTime; // skills と同じ長さで各スキルCT管理
        internal float nextDecisionTime = 0f;
        private int lastDominantIndex = -1;
        private RealtimeBattleUnit forcedTarget;
        private float forcedUntil;
        private readonly List<PendingSkillEvent> pendingEvents = new();
        private RealtimeSkillDefinition activeCastSkill;

        // ===== 行動妨害共通仕様 (仕様書 §21 / project_disruption_spec.md) =====
        // ノックバック / スタン / フリーズ等を喰らった unit に対し、 進行中 SO の CT 判定 (DealDamage 前=払戻、 後=通常) と硬直管理。
        private struct CurrentCastInfo
        {
            public bool valid;
            public RealtimeSkillDefinition skill;
            public int skillIndex;           // -1 = sub-skill (CT entry なし)
            public float refundCtTime;       // 払戻時に skillNextReadyTime[idx] を戻す値 (= 通常は BT で 即時使用可)
            public float animEndTime;
            public bool hasFiredDealDamage;
        }
        private CurrentCastInfo currentCast;
        internal float disruptedUntil = 0f;
        private GameObject activeAttachment;
        private GameObject[] skillAttachments;
        private RealtimeBattleUnit lastAttacker;  // 最後に自分を攻撃したユニット
        private float lastAttackerUntil;          // 有効期限
        // 一時的に非表示にした装備武器 (UnequipWeapon イベント) — cast 終了で復帰
        private readonly List<GameObject> hiddenWeapons = new();

        [Header("ボディサイズ")]
        [Tooltip("自分の半径 (m)。重なり判定・経路探索で使用")]
        public float bodyRadius = 0.45f;

        // ガード強制 (Knight Defend スキル等で一時的に guardChance=100% にする)
        private float guardForceUntil = 0f;
        public bool IsGuardForced => BT < guardForceUntil;
        public void ForceGuardFor(float durationSec)
        {
            guardForceUntil = BT + Mathf.Max(0f, durationSec);
        }

        /// <summary>
        /// 被弾時のガード判定。発動すれば mitigation (軽減率) を返し true。
        /// Defend スキル中は 100% 発動だが、盾の副次効果 (onGuardEffects) は
        /// 盾本来の guardChance で別途判定する (仕様: 足早/炎上は盾の 20% で発動)。
        /// </summary>
        public bool TryRollGuard(RealtimeBattleUnit attacker, out float mitigation)
        {
            mitigation = 0f;
            float baseChance = Mathf.Clamp01(guardChance);
            float effectiveChance = IsGuardForced ? 1f : baseChance;
            if (effectiveChance <= 0f) return false;
            if (UnityEngine.Random.value >= effectiveChance) return false;

            mitigation = Mathf.Clamp01(guardMitigation);

            // 盾の副次効果 (足早/炎上等) — 盾本来の guardChance で判定
            if (onGuardEffects != null && onGuardEffects.Count > 0 && baseChance > 0f)
            {
                for (int i = 0; i < onGuardEffects.Count; i++)
                {
                    var fx = onGuardEffects[i];
                    if (fx == null) continue;
                    if (UnityEngine.Random.value < baseChance)
                    {
                        ApplyOnGuardEffect(fx, attacker);
                    }
                }
            }
            return true;
        }

        /// <summary>盾の onGuardEffects 1 個を適用。対応する effect のみ実装、他は TODO。</summary>
        private void ApplyOnGuardEffect(RealtimePassiveDefinition fx, RealtimeBattleUnit attacker)
        {
            if (manager != null)
                manager.BattleLog.Add($"[{BT:F2}s] guard-passive {fx.displayName} ({fx.effect}) on {displayName}");
            switch (fx.effect)
            {
                case PassiveEffect.ApplyStatusEffectSelf:
                    if (unit != null)
                        unit.ApplyStatusEffect(fx.statusEffectType, fx.statusEffectValue,
                            fx.effectDurationSec > 0 ? fx.effectDurationSec : 3);
                    break;
                case PassiveEffect.ApplyStatusEffectTarget:
                    if (attacker != null && attacker.unit != null)
                        attacker.unit.ApplyStatusEffect(fx.statusEffectType, fx.statusEffectValue,
                            fx.effectDurationSec > 0 ? fx.effectDurationSec : 3);
                    break;
                case PassiveEffect.MoveSpeedMultiplier:
                    // effectAmount% 分 walkSpeed を短時間 UP (デフォルト 3 秒)
                    StartCoroutine(TempMoveSpeedBoost(fx.effectAmount, fx.effectDurationSec > 0 ? fx.effectDurationSec : 3f));
                    break;
                // 他 Phase B で実装
            }
        }

        private System.Collections.IEnumerator TempMoveSpeedBoost(float percent, float durationSec)
        {
            float mul = 1f + (percent / 100f);
            float orig = walkSpeed;
            walkSpeed = orig * mul;
            yield return new WaitForSeconds(durationSec);
            walkSpeed = orig;
        }

        // ============================================================
        //  Passive evaluator (Phase B-2)
        // ============================================================

        /// <summary>
        /// 装備武器の activePassives をトリガ種別で評価。発動条件を満たしたら ApplyPassiveEffect。
        /// ctxTarget: 攻撃命中系なら被害者、被弾系なら攻撃者、self系なら null
        /// ctxAttacker: 被弾系の攻撃者 (別で渡したい場合)
        /// </summary>
        public void EvaluatePassives(PassiveTrigger trigger,
                                      RealtimeBattleUnit ctxTarget = null,
                                      RealtimeBattleUnit ctxAttacker = null)
        {
            if (activePassives == null || activePassives.Count == 0) return;
            float now = BT;
            foreach (var p in activePassives)
            {
                if (p == null) continue;
                if (p.trigger != trigger) continue;

                // トリガ個別条件
                if (!MatchesTriggerParams(p, trigger)) continue;

                // ランタイム状態
                if (!_passiveStates.TryGetValue(p, out var st))
                {
                    st = new PassiveRuntimeState();
                    _passiveStates[p] = st;
                }

                // 最大発動回数
                if (p.maxTriggersPerBattle >= 0 && st.triggerCount >= p.maxTriggersPerBattle) continue;
                // クールダウン
                if (p.cooldownSec > 0f && now < st.nextReadyTime) continue;
                // 発動確率
                if (p.procChance < 1f && UnityEngine.Random.value >= p.procChance) continue;

                // 発動
                st.triggerCount++;
                st.nextReadyTime = now + Mathf.Max(0f, p.cooldownSec);
                ApplyPassiveEffect(p, ctxTarget, ctxAttacker);
            }
        }

        /// <summary>被弾時の passive トリガ処理を self で実行。攻撃者の ApplySkillEffect から呼ばれる。</summary>
        public void OnDamagedByPassive(RealtimeBattleUnit attacker)
        {
            // OnDamaged 発火
            EvaluatePassives(PassiveTrigger.OnDamaged, ctxAttacker: attacker);
            _damagedPendingCounter++; // OnDamagedThenNextHit 用

            // OnSelfHpBelowPercent: 閾値跨ぎ検出
            if (unit != null && unit.MaxHp > 0 && activePassives != null)
            {
                float curPct = 100f * unit.CurrentHp / unit.MaxHp;
                foreach (var p in activePassives)
                {
                    if (p == null || p.trigger != PassiveTrigger.OnSelfHpBelowPercent) continue;
                    float threshold = p.triggerFloatParam;
                    if (_lastHpPercentForTrigger > threshold && curPct <= threshold)
                    {
                        // 閾値を下回った瞬間 → EvaluatePassives 経由でロール/回数制限
                        EvaluatePassives(PassiveTrigger.OnSelfHpBelowPercent, ctxAttacker: attacker);
                        break; // 1 フレームに 1 回のみ
                    }
                }
                _lastHpPercentForTrigger = curPct;
            }
        }

        /// <summary>trigger 固有のパラメータ条件 (N 回目、連続 N 回 等) を判定。</summary>
        private bool MatchesTriggerParams(RealtimePassiveDefinition p, PassiveTrigger trigger)
        {
            switch (trigger)
            {
                case PassiveTrigger.OnNthBasicAttackHit:
                    // window-random: N 攻撃を 1 周期として、 周期内のランダムな 1 タイミングで発動。
                    // 発動後も周期は最後まで進める (発動済の周期で再発動はしない)。 周期境界で 2 連発の余地あり。
                    // 期待値: 平均 1 発動 / N 攻撃。
                    {
                        int n = Mathf.Max(1, p.triggerIntParam);
                        if (!_passiveStates.TryGetValue(p, out var st))
                        {
                            st = new PassiveRuntimeState();
                            _passiveStates[p] = st;
                        }
                        if (st.windowFireAtOffset == 0)
                            st.windowFireAtOffset = UnityEngine.Random.Range(1, n + 1);
                        st.windowAttackCount++;
                        bool fire = !st.windowFired && st.windowAttackCount == st.windowFireAtOffset;
                        if (fire) st.windowFired = true;
                        // 周期完了 → 次の周期開始 (新しい K を引く)
                        if (st.windowAttackCount >= n)
                        {
                            st.windowAttackCount = 0;
                            st.windowFireAtOffset = UnityEngine.Random.Range(1, n + 1);
                            st.windowFired = false;
                        }
                        return fire;
                    }
                case PassiveTrigger.OnConsecutiveHitSameTarget:
                    return _sameTargetHitStreak == Mathf.Max(1, p.triggerIntParam);
                case PassiveTrigger.OnDamagedThenNextHit:
                    return _damagedPendingCounter > 0;
                case PassiveTrigger.OnSelfHpBelowPercent:
                    return true; // 閾値跨ぎは発火側で事前判定、ここは常に true
                default:
                    return true;
            }
        }

        /// <summary>passive の effect を実際に適用。基本 effect 7 種のみ実装、他は TODO ログ。</summary>
        private void ApplyPassiveEffect(RealtimePassiveDefinition p,
                                         RealtimeBattleUnit ctxTarget,
                                         RealtimeBattleUnit ctxAttacker)
        {
            if (manager != null)
            {
                manager.BattleLog.Add($"[{BT:F2}s] passive {p.displayName} ({p.effect}) on {displayName}");
            }

            switch (p.effect)
            {
                case PassiveEffect.ApplyStatusEffectSelf:
                    if (unit != null)
                    {
                        int dur = p.effectDurationSec > 0 ? p.effectDurationSec : 5;
                        unit.ApplyStatusEffect(p.statusEffectType, p.statusEffectValue, dur);
                    }
                    break;

                case PassiveEffect.ApplyStatusEffectTarget:
                    var t1 = ctxTarget ?? ctxAttacker;
                    if (t1 != null && t1.unit != null)
                    {
                        int dur = p.effectDurationSec > 0 ? p.effectDurationSec : 5;
                        t1.unit.ApplyStatusEffect(p.statusEffectType, p.statusEffectValue, dur);
                    }
                    break;

                case PassiveEffect.StatusEffectOnHitTarget:
                    if (ctxTarget != null && ctxTarget.unit != null)
                    {
                        int dur = p.effectDurationSec > 0 ? p.effectDurationSec : 3;
                        ctxTarget.unit.ApplyStatusEffect(p.statusEffectType, Mathf.Max(1, p.statusEffectValue), dur);
                    }
                    break;

                case PassiveEffect.ExtraDamageThisHit:
                    if (ctxTarget != null && ctxTarget.unit != null && ctxTarget.IsAlive())
                    {
                        // 攻撃直後に追加で amount% 分の atFinal ダメを与える (簡易)
                        int baseAt = unit != null ? unit.AtFinal : 10;
                        int extra = Mathf.Max(1, Mathf.RoundToInt(baseAt * (p.effectAmount / 100f)));
                        int before = ctxTarget.unit.CurrentHp;
                        ProcessAutoRevive(ctxTarget, ref extra);
                        ProcessSurviveLethal(ctxTarget, ref extra);
                        ctxTarget.unit.TakeDamage(extra);
                        ctxTarget.OnHit(ctxTarget.unit.IsDead, this);
                        manager?.OnAttack(this, ctxTarget, extra, before, ctxTarget.unit.CurrentHp);
                    }
                    break;

                case PassiveEffect.HealSelf:
                    if (unit != null)
                    {
                        int heal = Mathf.Max(1, Mathf.RoundToInt(p.effectAmount));
                        unit.Heal(heal);
                    }
                    break;

                case PassiveEffect.GuardChanceBoost:
                    guardChance = Mathf.Clamp01(guardChance + (p.effectAmount / 100f));
                    break;

                case PassiveEffect.NextHitDamageReduction:
                    _nextHitDamageReductionRatio = Mathf.Clamp01(p.effectAmount / 100f);
                    break;

                // ===== Phase B-4-a: 状態フィールド系 =====
                case PassiveEffect.BasicAttackMul:
                    basicAttackMul *= 1f + (p.effectAmount / 100f);
                    break;
                case PassiveEffect.SkillMul:
                    skillMul *= 1f + (p.effectAmount / 100f);
                    break;
                case PassiveEffect.CooldownMultiplierSelf:
                    cooldownMul *= 1f + (p.effectAmount / 100f);
                    break;
                case PassiveEffect.HealMultiplier:
                    healMul *= 1f + (p.effectAmount / 100f);
                    break;
                case PassiveEffect.RangeMultiplier:
                    rangeBonus += p.effectAmount;  // amount を m 加算 (legacy 名: 実体は flat add)
                    break;
                case PassiveEffect.BasicAttackRangeBonus:
                    basicRangeBonus += p.effectAmount;  // 通常攻撃のみ +amount(m)
                    break;
                case PassiveEffect.RangePercentMul:
                    rangeMul *= 1f + (p.effectAmount / 100f);  // amount=20 で 1.2 倍
                    break;

                case PassiveEffect.LifestealPercent:
                    // 与ダメ amount% を自己回復 (吸血、 野武士の呼吸)
                    if (unit != null && _lastDealtDamage > 0)
                    {
                        int heal = Mathf.Max(1, Mathf.RoundToInt(_lastDealtDamage * (p.effectAmount / 100f)));
                        unit.Heal(heal);
                        if (manager != null)
                            manager.BattleLog.Add($"  → Lifesteal: {heal} HP 自己回復");
                    }
                    break;

                case PassiveEffect.HealAlliesOnHitPercent:
                    // 攻撃時 与ダメ amount% を味方全体回復 (大樹の加護)
                    if (manager != null && _lastDealtDamage > 0)
                    {
                        int heal = Mathf.Max(1, Mathf.RoundToInt(_lastDealtDamage * (p.effectAmount / 100f)));
                        int healed = 0;
                        foreach (var u in manager.AllUnits)
                        {
                            if (u == null || !u.IsAlive() || u.ownerSide != ownerSide || u.unit == null) continue;
                            u.unit.Heal(heal);
                            healed++;
                        }
                        manager.BattleLog.Add($"  → 味方 {healed} 体に {heal} HP 回復");
                    }
                    break;

                case PassiveEffect.BasicAttackShapeMultiplier:
                    basicShapeMul *= 1f + (p.effectAmount / 100f);  // amount=50 で 1.5 倍
                    break;

                case PassiveEffect.WalkSpeedMul:
                    // walkSpeed に恒久乗算 (竜王の槍 = Lancer 1.1 × 1.10 = 1.21 ≈ Warrior 1.2)。
                    // 下流の MoveToDistance / MaintainDistance / MoveAwayToDistance は毎フレ
                    // mover.maxSpeed = walkSpeed を再代入するので、 walkSpeed を書き換えるだけで反映される。
                    walkSpeed *= 1f + (p.effectAmount / 100f);
                    if (mover != null) mover.maxSpeed = walkSpeed;  // 静止中も即時反映
                    break;
                case PassiveEffect.AllDamageMul:
                    // basic + skill 両方に乗算 (竜骨 +5% 等)
                    {
                        float allMul = 1f + (p.effectAmount / 100f);
                        basicAttackMul *= allMul;
                        skillMul *= allMul;
                    }
                    break;
                case PassiveEffect.DeTauntOnDamaged:
                    // 攻撃者が自分をターゲットしてれば、 別の味方に切り替えさせる (シャドウハンター 影の回避)
                    if (ctxAttacker != null && manager != null && ctxAttacker.CurrentTarget == this)
                    {
                        RealtimeBattleUnit alt = null;
                        float bestDist = float.MaxValue;
                        foreach (var u in manager.AllUnits)
                        {
                            if (u == null || u == this || !u.IsAlive() || u.ownerSide != ownerSide) continue;
                            float d = Vector3.Distance(u.transform.position, ctxAttacker.transform.position);
                            if (d < bestDist) { bestDist = d; alt = u; }
                        }
                        if (alt != null)
                        {
                            ctxAttacker.SwitchTargetTo(alt);
                            manager.BattleLog.Add($"  → 影の回避: {ctxAttacker.displayName} のターゲットを {displayName} → {alt.displayName} に切替");
                        }
                    }
                    break;
                case PassiveEffect.AttackShapeOverride:
                    // shape size 系 (Line/Fan/Circle/Diamond 等の lengthMass/widthMass) +amount
                    shapeSizeBonus += p.effectAmount;
                    // 直撃型 (Single / CircleAtTarget = 弓矢 / 着弾型 projectile) も射程 +amount
                    rangeBonus += p.effectAmount;
                    break;
                case PassiveEffect.DamageKindOverride:
                    basicKindOverrideValid = true;
                    basicKindOverride = (SkillDamageKind)Mathf.RoundToInt(p.effectAmount);
                    break;
                case PassiveEffect.DisableBasicAttack:
                    basicAttackDisabled = true;
                    break;
                case PassiveEffect.MultiShot:
                    multiShotCount = Mathf.Max(1, Mathf.RoundToInt(p.effectAmount));
                    break;

                // ===== Phase B-4-b: 瞬間発動系 =====
                case PassiveEffect.ExtraHitOnSameTarget:
                    // 発動契機の対象に追加 1 ヒット (amount%)
                    if (ctxTarget != null && ctxTarget.unit != null && ctxTarget.IsAlive())
                    {
                        int baseAt = unit != null ? unit.AtFinal : 10;
                        int extra = Mathf.Max(1, Mathf.RoundToInt(baseAt * (p.effectAmount / 100f)));
                        int before = ctxTarget.unit.CurrentHp;
                        ProcessAutoRevive(ctxTarget, ref extra);
                        ProcessSurviveLethal(ctxTarget, ref extra);
                        ctxTarget.unit.TakeDamage(extra);
                        ctxTarget.OnHit(ctxTarget.unit.IsDead, this);
                        manager?.OnAttack(this, ctxTarget, extra, before, ctxTarget.unit.CurrentHp);
                    }
                    break;

                case PassiveEffect.ExtraAoeHit:
                    // 対象中心に半径 1.5m AoE、amount% 追加ダメ (三星剣 / 朱雀の槍)
                    if (ctxTarget != null && manager != null)
                    {
                        float radius = 1.5f;
                        int baseAt = unit != null ? unit.AtFinal : 10;
                        int extra = Mathf.Max(1, Mathf.RoundToInt(baseAt * (p.effectAmount / 100f)));
                        foreach (var u in manager.AllUnits)
                        {
                            if (u == null || !u.IsAlive() || u.ownerSide == ownerSide) continue;
                            if (Vector3.Distance(u.transform.position, ctxTarget.transform.position) > radius) continue;
                            int before = u.unit.CurrentHp;
                            int eachDmg = extra;
                            ProcessAutoRevive(u, ref eachDmg);
                            ProcessSurviveLethal(u, ref eachDmg);
                            u.unit.TakeDamage(eachDmg);
                            u.OnHit(u.unit.IsDead, this);
                            manager?.OnAttack(this, u, eachDmg, before, u.unit.CurrentHp);
                        }
                    }
                    break;

                case PassiveEffect.PullEnemies:
                    // 対象を自分方向へ amount m 引き寄せ
                    if (ctxTarget != null)
                    {
                        Vector3 dir = transform.position - ctxTarget.transform.position;
                        dir.y = 0;
                        if (dir.sqrMagnitude > 0.01f)
                        {
                            dir.Normalize();
                            MovePreventOverlap(ctxTarget, dir, Mathf.Max(0.5f, p.effectAmount));
                        }
                    }
                    break;

                case PassiveEffect.KnockbackAoE:
                    // 自前方 AoE (半径 2m 扱い) 内の敵を前方へ amount m 押し戻す
                    if (manager != null)
                    {
                        float radius = 2f;
                        foreach (var u in manager.AllUnits)
                        {
                            if (u == null || !u.IsAlive() || u.ownerSide == ownerSide) continue;
                            Vector3 delta = u.transform.position - transform.position;
                            delta.y = 0;
                            if (delta.magnitude > radius) continue;
                            ApplyKnockback(u, Mathf.Max(0.5f, p.effectAmount));
                        }
                    }
                    break;

                case PassiveEffect.FocusMark:
                    // 近場 (2m 以内) の味方 1 人のターゲットを、自分が攻撃した敵に強制変更 (3秒)
                    if (ctxTarget != null && manager != null)
                    {
                        RealtimeBattleUnit nearest = null;
                        float nearestDist = float.MaxValue;
                        foreach (var ally in manager.AllUnits)
                        {
                            if (ally == null || !ally.IsAlive() || ally == this) continue;
                            if (ally.ownerSide != ownerSide) continue;
                            float d = Vector3.Distance(ally.transform.position, transform.position);
                            if (d > 2f) continue;
                            if (d < nearestDist) { nearestDist = d; nearest = ally; }
                        }
                        if (nearest != null)
                        {
                            nearest.ForceTarget(ctxTarget, 3f);
                            if (manager != null) manager.BattleLog.Add($"  → FocusMark: {nearest.displayName} → {ctxTarget.displayName}");
                        }
                    }
                    break;

                case PassiveEffect.ExtendEnemyCooldowns:
                    // 対象の CT 中スキル全てを effectAmount 秒延長 (ウエイトカッター 時断ち)
                    if (ctxTarget != null) ctxTarget.ExtendActiveCooldowns(p.effectAmount);
                    if (manager != null && ctxTarget != null)
                        manager.BattleLog.Add($"  → {ctxTarget.displayName} の CT 中スキルを {p.effectAmount:F1}s 延長");
                    break;

                case PassiveEffect.AutoReviveOnce:
                    // BattleStart で AutoRevive を付与 (致死ダメを 1 回だけ HP value% で復活)
                    // SurviveLethal (HP1残し) とは別物。十字架の加護用
                    if (unit != null)
                    {
                        int pct = Mathf.Max(1, Mathf.RoundToInt(p.effectAmount));
                        unit.ApplyStatusEffect(StatusEffectType.AutoRevive, pct, 9999);
                    }
                    break;

                case PassiveEffect.RandomBuffSelf:
                    // ランダムな BuffAt / BuffDf / BuffAgi / BuffMat / BuffMdf を 10% UP、指定秒数
                    if (unit != null)
                    {
                        var options = new StatusEffectType[] {
                            StatusEffectType.BuffAt, StatusEffectType.BuffDf,
                            StatusEffectType.BuffAgi, StatusEffectType.BuffMat,
                            StatusEffectType.BuffMdf,
                        };
                        var pick = options[UnityEngine.Random.Range(0, options.Length)];
                        int dur = p.effectDurationSec > 0 ? p.effectDurationSec : 3;
                        int val = p.statusEffectValue > 0 ? p.statusEffectValue : Mathf.Max(1, Mathf.RoundToInt(p.effectAmount));
                        unit.ApplyStatusEffect(pick, val, dur);
                        if (manager != null) manager.BattleLog.Add($"  → RandomBuff: {pick}+{val} ({dur}s)");
                    }
                    break;

                case PassiveEffect.GoldDropChance:
                    // RT 戦闘内では log のみ。RPG 外側のドロップ集計で拾う想定
                    if (manager != null && ctxTarget != null)
                        manager.BattleLog.Add($"  → GoldDrop ({p.effectAmount:F0}%) on {ctxTarget.displayName}");
                    break;

                case PassiveEffect.MoveSpeedMultiplier:
                    StartCoroutine(TempMoveSpeedBoost(p.effectAmount, p.effectDurationSec > 0 ? p.effectDurationSec : 3f));
                    break;

                case PassiveEffect.AttackSpeedMultiplier:
                    // CT 倍率と同意義なので cooldownMul に合流
                    cooldownMul *= 1f + (-p.effectAmount / 100f); // 速くしたい amount=20 なら *0.8
                    break;

                default:
                    // Phase B-4 で実装予定: FocusMark/PullEnemies/KnockbackAoE/MultiShot/
                    // RangeMultiplier/MoveSpeed/AttackSpeed/DamageKindOverride/HealMultiplier/
                    // BasicAttackMul/SkillMul/CooldownMultiplierSelf/RandomBuffSelf/GoldDropChance/
                    // DisableBasicAttack/AttackShapeOverride/ExtraAoeHit/AutoReviveOnce/
                    // CooldownResetEnemyBasic
                    if (manager != null)
                        manager.BattleLog.Add($"  [TODO] effect {p.effect} not implemented yet");
                    break;
            }
        }

        /// <summary>
        /// 武器を装備する。盾なら guard 系に反映、武器パッシブを activePassives に追加、
        /// modelPrefab を指定ボーン配下にスポーン。
        /// </summary>
        public void EquipWeapon(RealtimeWeaponDefinition w)
        {
            if (w == null) return;

            // 盾固有
            if (w.kind == WeaponKind.Shield)
            {
                guardChance = w.guardChance;
                guardMitigation = w.guardMitigation;
                if (onGuardEffects == null) onGuardEffects = new List<RealtimePassiveDefinition>();
                if (w.onGuardEffects != null)
                {
                    foreach (var fx in w.onGuardEffects)
                        if (fx != null && !onGuardEffects.Contains(fx)) onGuardEffects.Add(fx);
                }
            }

            // 武器パッシブ蓄積
            if (activePassives == null) activePassives = new List<RealtimePassiveDefinition>();
            if (w.weaponPassives != null)
            {
                foreach (var p in w.weaponPassives)
                    if (p != null && !activePassives.Contains(p)) activePassives.Add(p);
            }

            // 武器スキル差し替え (skillId 一致で skills[i] 置換、 一致なしなら append)
            // 例: DragonFang.weaponSkills に ArrowShot_DragonFang (skillId="ArrowShot") を入れておくと、
            //   Archer の skills[0] (= ArrowShot) が ArrowShot_DragonFang に上書きされ、
            //   anim / VFX / 射程 / timeline 全部その SO の値で動く。
            if (w.weaponSkills != null && w.weaponSkills.Count > 0)
            {
                if (skills == null) skills = new List<RealtimeSkillDefinition>();
                foreach (var ws in w.weaponSkills)
                {
                    if (ws == null) continue;
                    int replaceIdx = -1;
                    if (!string.IsNullOrEmpty(ws.skillId))
                    {
                        for (int i = 0; i < skills.Count; i++)
                        {
                            if (skills[i] != null && skills[i].skillId == ws.skillId)
                            {
                                replaceIdx = i;
                                break;
                            }
                        }
                    }
                    if (replaceIdx >= 0)
                    {
                        Debug.Log($"[EquipWeapon] {name}: skills[{replaceIdx}] '{skills[replaceIdx].skillId}' → '{ws.name}' (id={ws.skillId})");
                        skills[replaceIdx] = ws;
                    }
                    else
                    {
                        Debug.Log($"[EquipWeapon] {name}: append skill '{ws.name}' (id={ws.skillId}) at index {skills.Count}");
                        skills.Add(ws);
                    }
                }
                // skillNextReadyTime / skillCooldownMul の配列長を skills 数に合わせる
                if (skillNextReadyTime == null || skillNextReadyTime.Length < skills.Count)
                {
                    var newCT = new float[skills.Count];
                    if (skillNextReadyTime != null)
                        System.Array.Copy(skillNextReadyTime, newCT, skillNextReadyTime.Length);
                    skillNextReadyTime = newCT;
                }
            }

            // 3D モデル装着
            GameObject weaponGo = null;
            if (w.modelPrefab != null)
            {
                string boneName = w.ResolveBoneName();
                var bone = ResolveAttachTransform(boneName);
                if (bone == null)
                {
                    Debug.LogWarning($"[EquipWeapon] {name}: bone '{boneName}' not found for {w.weaponId}");
                    return;
                }
                weaponGo = Instantiate(w.modelPrefab, bone);
                weaponGo.name = $"EquippedWeapon_{w.weaponId}";
                weaponGo.transform.localPosition = w.attachPositionOffset;
                weaponGo.transform.localRotation = Quaternion.Euler(w.attachEulerOffset);
                // attachScale がデフォルト (1,1,1) なら prefab 側の scale を尊重、
                // 明示的に違う値が設定されてる場合のみ上書き
                if (w.attachScale != Vector3.one)
                    weaponGo.transform.localScale = w.attachScale;

                // TrailTip 自動生成 (trailTipLocalPosition が指定されてる時のみ)
                if (w.trailTipLocalPosition != Vector3.zero)
                {
                    var tipGo = new GameObject("TrailTip");
                    tipGo.transform.SetParent(weaponGo.transform, false);
                    tipGo.transform.localPosition = w.trailTipLocalPosition;
                    tipGo.transform.localRotation = Quaternion.identity;
                }
            }

            // 両手持ち：左手 IK セットアップ
            ApplyLeftHandIK(w, weaponGo);
        }

        /// <summary>
        /// 武器側に leftHandGripName が設定されていれば、その Transform を左手 IK ターゲットに設定。
        /// 空 or 武器なし → IK 解除（前装備の残留防止）。
        /// </summary>
        private void ApplyLeftHandIK(RealtimeWeaponDefinition w, GameObject weaponGo)
        {
            var animatorComp = GetComponentInChildren<Animator>();
            if (animatorComp == null)
            {
                Debug.LogWarning($"[EquipWeapon-IK] {name}: Animator not found, IK skipped");
                return;
            }

            if (weaponGo != null && !string.IsNullOrEmpty(w.leftHandGripName))
            {
                var grip = FindChildRecursive(weaponGo.transform, w.leftHandGripName);
                if (grip == null)
                {
                    Debug.LogWarning($"[EquipWeapon-IK] {name}: leftHandGrip '{w.leftHandGripName}' not found in {w.weaponId}");
                    ClearLeftHandIK(animatorComp);
                    return;
                }
                var ik = animatorComp.GetComponent<WeaponLeftHandIK>();
                if (ik == null) ik = animatorComp.gameObject.AddComponent<WeaponLeftHandIK>();
                ik.leftHandTarget = grip;
                ik.weight = 1f;
                Debug.Log($"[EquipWeapon-IK] {name}: IK enabled for {w.weaponId} → {grip.name} on Animator {animatorComp.gameObject.name} (humanoid={animatorComp.isHuman})");
            }
            else
            {
                ClearLeftHandIK(animatorComp);
            }
        }

        private void ClearLeftHandIK(Animator animatorComp)
        {
            var ik = animatorComp.GetComponent<WeaponLeftHandIK>();
            if (ik != null) ik.leftHandTarget = null;
        }

        private static Transform FindChildRecursive(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var found = FindChildRecursive(root.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        // 戦闘時刻ゲッター (倍速対応: Time.time / Time.deltaTime の代わりに BT/BDT を使う)
        private float BT => manager != null ? manager.BattleTime : 0f;
        private float BDT => manager != null ? manager.BattleDeltaTime : Time.deltaTime;

        private struct PendingSkillEvent
        {
            public float fireTime;
            public RealtimeSkillEvent ev;
            public RealtimeSkillDefinition skill;
            public RealtimeBattleUnit primaryTarget;
            public List<RealtimeBattleUnit> hitTargets;
            public Barricade barricadeTarget;       // 非 null なら DealDamage はバリケードに ApplyFixedHit (近接 Slash 用)
            public Vector3 castOrigin;
            public Vector3 castAim;
            public Vector3 castDir;
            public float damageMul; // トライアローの側矢などで 0.9 倍にする用 (default 1.0)
        }
        private RealtimeBattleUnit currentTarget;
        // 基本スキル射程内に敵バリケードがあれば優先攻撃 (Warrior/Knight 等の近接職向け)。
        // currentTarget (RealtimeBattleUnit) と独立に管理。
        private Barricade currentBarricadeTarget;
        internal RealtimeBattleManager manager;
        private JobAnimator anim;
        private bool wasMoving;
        private bool _isTurning;       // 方向転換中フラグ (Walk アニメ流して回転を見せる用)
        private bool _turnRight;       // 旋回方向 (true=右回り、false=左回り)
        private bool _useMoveDirFacing; // 速度ヒステリシスで管理: true=進行方向を向く、 false=target を向く

        // 連続座標移動コントローラ (ContinuousMover)
        public SteraCube.SpaceJourney.Realtime.Pathfinding.ContinuousMover mover;

        // 生死状態切替用 (occupant table 廃止後は _prevAlive のみ使用)
        private bool _prevAlive = true;

        // フィールド境界 (RealtimeBattleStarter から設定される)
        public BattleFieldVisualizer fieldVisualizer;

        // 範囲マップ (manager 側で設定される)
        public static float RangeCloseMass = 1f;
        public static float RangeMidMass = 2f;
        public static float RangeFarMass = 5f;
        public static float RangeMaxFarMass = 6f;

        public void Setup(SpaceJourneyUnit u, int side, string name,
                          AttackRangeCategory range, float speed,
                          RealtimeBattleManager mgr)
        {
            // 古い subscribe を必ず外してから貼り直し (使い回し時の多重登録防止)
            if (unit != null) unit.OnDisruptApplied -= OnDisruptAppliedHandler;

            unit = u;
            ownerSide = side;
            displayName = name;
            preferredRange = range;
            walkSpeed = speed;
            manager = mgr;
            basicAttackDamage = u != null ? u.AtFinal : 10f;

            // 行動妨害系 (Hardcc + Charm + Silence) 付与時に OnDisrupted を呼ぶ subscribe
            // (Knockback と同じく 進行中SOキャンセル + DealDamage 前=CT払戻+硬直0、後=残アニメ秒硬直)
            if (u != null) u.OnDisruptApplied += OnDisruptAppliedHandler;

            anim = GetComponent<JobAnimator>();
            if (anim == null) anim = gameObject.AddComponent<JobAnimator>();
            anim.RefreshParams();

            // ContinuousMover 参照キャッシュ
            mover = GetComponent<SteraCube.SpaceJourney.Realtime.Pathfinding.ContinuousMover>();
            if (mover == null)
                mover = gameObject.AddComponent<SteraCube.SpaceJourney.Realtime.Pathfinding.ContinuousMover>();
            mover.Init(this);
            mover.maxSpeed = walkSpeed;
            mover.simulateMovement = false;

            // スキル CT 配列を用意 (opening CT 分先送り)
            skillNextReadyTime = new float[skills != null ? skills.Count : 0];
            for (int i = 0; i < skillNextReadyTime.Length; i++)
            {
                var s = skills[i];
                skillNextReadyTime[i] = BT + (s != null ? s.openingCooldownSec : 0f);
            }

            // ボディスキル パッシブ flag を job に応じて設定 (rank 3/5 で習得想定、現状は常時有効)
            ApplyBodyJobPassives(u);

            // スキル毎の手装備アタッチ (弓射の矢 等) を事前配置
            SpawnHandAttachments();
        }

        /// <summary>job ID に応じて rank 3/5 のパッシブを有効化 (現状は常時 ON)。
        /// 将来的には Soul の rank に応じて条件分岐する想定。</summary>
        private void ApplyBodyJobPassives(SpaceJourneyUnit u)
        {
            // BodyJob (ScriptableObject 参照) は [NonSerialized] cachedBodyJob 経由で
            // Resolve(db) が呼ばれた後でないと null になるため、Body.BodyJobId (serialized string)
            // を直接使う。
            string jobId = u?.Body?.BodyJobId ?? u?.Body?.BodyJob?.bodyJobId ?? "";
            // BodyJob SO の rank3PassiveMarkers を activePassives に注入 (ConditionalCast 条件用)
            var bodyJob = u?.Body?.BodyJob;
            // cachedBodyJob ([NonSerialized]) が Resolve 未実行で null の場合、 DB 経由で取得 (ResolveSkillsForUnit と同パターン)
            if (bodyJob == null && !string.IsNullOrEmpty(jobId))
            {
                bodyJob = MasterDatabase.Instance?.GetBodyJobById(jobId);
            }
            if (bodyJob != null && bodyJob.rank3PassiveMarkers != null)
            {
                if (activePassives == null) activePassives = new List<RealtimePassiveDefinition>();
                foreach (var marker in bodyJob.rank3PassiveMarkers)
                {
                    if (marker != null && !activePassives.Contains(marker))
                        activePassives.Add(marker);
                }
                if (manager != null && bodyJob.rank3PassiveMarkers.Count > 0)
                    Debug.Log($"[ApplyBodyJobPassives] {name} ({jobId}): added {bodyJob.rank3PassiveMarkers.Count} rank3 markers to activePassives");
            }
            switch (jobId)
            {
                case "Warrior":
                    hasExtraAttack = true;  // rank 3 (legacy flag、 SkillTestPlayBootstrap 互換用に残置)
                    break;
                case "Lancer":
                    hasLongThrust = true;  // rank 3 (range +1m)
                    rangeBonus += 1f;
                    shapeSizeBonus += 1f;
                    hasFlinchBasic = true; // rank 5
                    break;
                case "Archer":
                    hasPierceShot = true;  // rank 3
                    hasTripleArrow = true; // rank 5
                    break;
                // Knight: Provoke / Defend / Barricade はスキル枠で扱うので flag 不要
                // Mage: IceBall / HealingWave はスキル枠で扱うので flag 不要
            }
        }

        /// <summary>skills に handAttachmentPrefab があるものを該当ボーン配下に事前配置 (初期非表示)</summary>
        private void SpawnHandAttachments()
        {
            if (skills == null) return;
            skillAttachments = new GameObject[skills.Count];
            for (int i = 0; i < skills.Count; i++)
            {
                var s = skills[i];
                if (s == null || s.handAttachmentPrefab == null) continue;
                string boneName = string.IsNullOrEmpty(s.handAttachmentBoneName) ? "Weapon_Root_R" : s.handAttachmentBoneName;
                // ResolveAttachTransform が Weapon_Root_R まで自動 fallback してくれる
                var bone = ResolveAttachTransform(boneName);
                if (bone == null)
                {
                    Debug.LogWarning($"[RealtimeSkill] handAttachment bone '{boneName}' not found and couldn't auto-create on {name}");
                    continue;
                }
                var att = Instantiate(s.handAttachmentPrefab, bone);
                att.transform.localPosition = s.handAttachmentOffset;
                att.transform.localRotation = Quaternion.Euler(s.handAttachmentEulerOffset);
                att.transform.localScale = Vector3.one;
                att.SetActive(false);
                skillAttachments[i] = att;
                // Keyframe 駆動が優先、無ければ AttachmentRotator で 2 点補間
                if (s.attachmentKeyframes != null && s.attachmentKeyframes.Count > 0)
                {
                    var drv = att.AddComponent<AttachmentCurveDriver>();
                    drv.keyframes = s.attachmentKeyframes;
                }
                else
                {
                    bool hasRotAnim = s.handAttachmentRotationEndTime > 0.001f
                        && s.handAttachmentEulerStart != s.handAttachmentEulerOffset;
                    if (hasRotAnim)
                    {
                        var rot = att.AddComponent<AttachmentRotator>();
                        rot.startLocalEuler = s.handAttachmentEulerStart;
                        rot.endLocalEulerOffset = s.handAttachmentEulerOffset;
                        rot.duration = s.handAttachmentRotationEndTime;
                        rot.caster = this;
                    }
                }
            }
        }

        [Tooltip("希望距離 (m)。>0 なら preferredRange カテゴリを上書き。")]
        public float preferredDistanceOverride = 0f;

        public float GetPreferredRangeWorld()
        {
            if (preferredDistanceOverride > 0f) return preferredDistanceOverride;
            return preferredRange switch
            {
                AttackRangeCategory.Close => RangeCloseMass,
                AttackRangeCategory.Mid => RangeMidMass,
                AttackRangeCategory.Far => RangeFarMass,
                AttackRangeCategory.MaxFar => RangeMaxFarMass,
                _ => 1f,
            };
        }

        public bool IsAlive() => unit != null && !unit.IsDead;
        public RealtimeBattleUnit CurrentTarget => currentTarget;

        // ─── Snapshot 用 readonly getter (RemoteControl /battle/snapshot 用) ──
        public bool IsCasting => BT < attackingUntil;
        public float CastingRemaining => Mathf.Max(0f, attackingUntil - BT);
        public float FlinchRemaining => Mathf.Max(0f, flinchUntil - BT);
        public bool IsDisrupted => BT < disruptedUntil;
        public float DisruptedRemaining => Mathf.Max(0f, disruptedUntil - BT);
        public int CurrentCastSkillIndex => currentCast.valid ? currentCast.skillIndex : -1;
        public string CurrentCastSkillName =>
            (currentCast.valid && currentCast.skill != null) ? currentCast.skill.skillName : null;
        public int SkillCtCount => skillNextReadyTime != null ? skillNextReadyTime.Length : 0;
        public float GetSkillCtRemaining(int index)
        {
            if (skillNextReadyTime == null) return 0f;
            if (index < 0 || index >= skillNextReadyTime.Length) return 0f;
            return Mathf.Max(0f, skillNextReadyTime[index] - BT);
        }

        private void Update()
        {
            // Pending イベント処理 (死亡しても発火継続: 既に放たれた矢は届く)
            ProcessPendingEvents();

            // 行動妨害共通仕様: pendingEvents 全消化 & anim 終了で currentCast を確定クリア
            if (currentCast.valid && BT >= currentCast.animEndTime)
            {
                bool stillHasEvents = false;
                for (int i = 0; i < pendingEvents.Count; i++)
                {
                    if (pendingEvents[i].skill == currentCast.skill) { stillHasEvents = true; break; }
                }
                if (!stillHasEvents) currentCast.valid = false;
            }

            // UnequipWeapon で隠した武器を cast 終了で復帰
            if (hiddenWeapons.Count > 0 && BT >= attackingUntil)
            {
                RestoreHiddenWeapons();
            }

            // 生死遷移時に RVO + Layer を同期 (死亡で他 unit から邪魔されなくなる、復活で復帰)
            SyncAliveState();

            if (manager == null || manager.IsFinished)
            {
                if (mover != null) mover.simulateMovement = false;
                return;
            }
            if (manager.IsCountdown) return; // カウントダウン中は unit AI 停止
            if (!IsAlive()) return;
            if (testPlayManualMode) return; // Skill Timeline Editor: 手動 Replay のみ受け付ける

            // 行動妨害共通仕様: 硬直中は AI 判断 / 移動を停止
            if (BT < disruptedUntil)
            {
                if (mover != null) mover.simulateMovement = false;
                return;
            }

            // 初回 Update: BattleStart と AlwaysWhileActive 系 passive を発火
            if (!_battleStartFired)
            {
                _battleStartFired = true;
                _lastHpPercentForTrigger = unit != null && unit.MaxHp > 0
                    ? (100f * unit.CurrentHp / unit.MaxHp) : 100f;
                EvaluatePassives(PassiveTrigger.BattleStart);
                EvaluatePassives(PassiveTrigger.AlwaysWhileActive);
            }

            // ターゲット維持: 挑発優先 → targetList 再評価 (1秒毎 or target死亡)
            MaintainCurrentTarget();
            // 基本スキル射程内に敵バリケードがあれば優先攻撃 (Warrior/Knight 等)
            UpdateBarricadeTarget();
            // 通常 currentTarget が null でもバリケードが居れば BasicAttack 可能なので continue
            if (currentTarget == null && currentBarricadeTarget == null) return;

            Vector3 myPos = transform.position;
            Vector3 targetPos = currentTarget.transform.position;
            Vector3 toTarget = targetPos - myPos;
            toTarget.y = 0f;
            // 連続座標ベースの Euclid 距離 (XZ 平面)
            float dist = new Vector2(toTarget.x, toTarget.z).magnitude;
            float preferred = GetPreferredRangeWorld();

            // 攻撃アニメ中 + postAttackLockSec 猶予まで「アニメ中」として扱う
            // (回転再開・終端付近の変な向きジャンプを防ぐ)
            // さらに anim.IsBusyState() (Animator が Attack/Damage 等の非移動 state) も加味:
            // SkillDefinition.castAnimSec が短くて attackingUntil が早く切れても、
            // 実 Animator State が Attack のままなら移動を抑止する。
            // ただし Animator の戻し漏れで永久ロックを防ぐため、 attackingUntil 超過後 1.5s で
            // IsBusyState 由来のロックは強制解除 (= castAnimSec を信じる)。
            const float animSafetyTimeoutSec = 1.5f;
            bool animBusy = anim != null && anim.IsBusyState();
            bool isAttacking = (BT < attackingUntil + postAttackLockSec)
                            || (animBusy && BT < attackingUntil + animSafetyTimeoutSec);
            bool isFlinching = IsFlinching;

            bool moving = false;
            if (isAttacking || isFlinching)
            {
                // 攻撃アニメ中 / 被弾怯み中: 完全停止
                if (mover != null) mover.simulateMovement = false;
            }
            else
            {
                var list = (actionList != null && actionList.Count > 0)
                    ? actionList
                    : DefaultActionList();

                foreach (var entry in list)
                {
                    if (!EvalCondition(entry, dist)) continue;
                    moving = ExecuteAction(entry, toTarget, dist, preferred, out bool committed);
                    if (!committed) continue;
                    break;
                }
            }

            // 向き制御 (Phase 2 改: ヒステリシス付き moveDir / target 切替):
            //   - 攻撃中 / のけぞり中 → 何もしない
            //   - 移動中 (velocity 充分) → 進行方向 (= 迂回時は迂回方向、 直行時は target 方向)
            //   - 停止中 → target 方向
            //   - 切替振動防止: 移動 ON 判定は |v|>0.5*walkSpeed、 OFF 判定は |v|<0.2*walkSpeed
            if (!isAttacking && !isFlinching)
            {
                float vMag = (mover != null) ? mover.velocity.magnitude : 0f;
                float walkRef = Mathf.Max(0.1f, walkSpeed);
                if (_useMoveDirFacing)
                {
                    if (vMag < 0.2f * walkRef) _useMoveDirFacing = false;
                }
                else
                {
                    if (vMag > 0.5f * walkRef) _useMoveDirFacing = true;
                }

                Vector3 moveDir = Vector3.zero;
                if (_useMoveDirFacing && mover != null)
                {
                    Vector3 v = mover.velocity; v.y = 0f;
                    if (v.sqrMagnitude > 0.0001f) moveDir = v.normalized;
                }

                Vector3 facingDir;
                if (moveDir.sqrMagnitude > 0.01f)
                {
                    facingDir = moveDir;
                }
                else if (toTarget.sqrMagnitude > 0.01f)
                {
                    facingDir = toTarget.normalized;
                }
                else
                {
                    facingDir = transform.forward;
                }
                Quaternion faceRot = Quaternion.LookRotation(facingDir, Vector3.up);
                // 方向転換中フラグ: 静止中で target に向き直してる時だけ (WalkLeft/WalkRight 用)
                _isTurning = !_useMoveDirFacing && Quaternion.Angle(transform.rotation, faceRot) > 5f;
                if (_isTurning)
                {
                    Vector3 fwd = transform.forward;
                    Vector3 toFace = faceRot * Vector3.forward;
                    float crossY = fwd.x * toFace.z - fwd.z * toFace.x;
                    _turnRight = crossY < 0f;
                }
                transform.rotation = Quaternion.Slerp(transform.rotation, faceRot, 6f * BDT);
            }
            else
            {
                _isTurning = false;
            }

            // フィールド境界クランプ (5x5 × 2 side = 幅5×奥行10)
            ClampToField();

            // moving 判定: 「destination 方向への前進量」(EffectiveProgressSpeed) で walk/idle 切替。
            // 横滑りや無効な押し合いでは progress=0 → idle に落ちる。
            // しきい値 0.3 m/s 以下 → 実質「destination に向かって進めていない」扱い。
            if (mover != null)
            {
                bool moverActive = mover.simulateMovement || mover.IsInTransit;
                moving = moverActive && !mover.reachedDestination && mover.EffectiveProgressSpeed > 0.3f;
            }

            // アニメ反映
            // 1. 方向転換中: WalkLeft/WalkRight の専用 state があればそれを再生 (CrossFade)。
            //    無ければ Walk アニメ (moving=true) で fallback。
            // 2. 通常移動: SetMoving で Walk/Idle 切替。
            if (_isTurning && !moving)
            {
                bool playedTurn = anim != null && anim.PlayTurn(_turnRight);
                if (playedTurn)
                {
                    // CrossFade で WalkLeft/WalkRight 直接再生 → SetMoving(false) で安定
                    if (wasMoving)
                    {
                        wasMoving = false;
                        // SetMoving は PlayTurn 内で呼ばれてる
                    }
                    moving = false;
                }
                else
                {
                    moving = true;
                }
            }
            if (moving != wasMoving)
            {
                wasMoving = moving;
                anim?.SetMoving(moving);
            }
        }

        private void ClampToField()
        {
            if (fieldVisualizer == null) return;
            float w = fieldVisualizer.gridWidth * fieldVisualizer.cellSize;
            float d = fieldVisualizer.gridDepth * fieldVisualizer.cellSize;
            Vector3 center = (fieldVisualizer.playerCube.position + fieldVisualizer.enemyCube.position) * 0.5f;
            center.y = 0;

            Vector3 fwd = fieldVisualizer.Forward;
            Vector3 right = fieldVisualizer.RightAxis;

            Vector3 pos = transform.position;
            Vector3 rel = pos - center;
            rel.y = 0;

            float fDot = Vector3.Dot(rel, fwd);
            float rDot = Vector3.Dot(rel, right);

            // フィールド全体 (敵陣侵入可)
            fDot = Mathf.Clamp(fDot, -d, d);
            rDot = Mathf.Clamp(rDot, -w * 0.5f, w * 0.5f);
            Vector3 clamped = center + fwd * fDot + right * rDot;
            clamped.y = pos.y;
            // 0.5m 以上大きくズレた時だけ安全ネットとして clamp + mover に位置同期
            if ((clamped - pos).sqrMagnitude > 0.25f)
            {
                transform.position = clamped;
                if (mover != null) mover.Teleport(clamped, true);
            }
        }

        /// <summary>指定位置が他ユニットと重なるか (最低距離以内か)。MovePreventOverlap 用。
        /// ignoreUnit を指定するとそのユニットは衝突判定から除外 (ノックバック/Pull の caster を除外する用途)。
        /// ignoreSet を指定するとその全 unit を衝突判定から除外 (cascade knockback で BOX 内仲間同士を擦り抜ける用途)。</summary>
        private bool WouldOverlap(Vector3 pos, RealtimeBattleUnit ignoreUnit = null, HashSet<RealtimeBattleUnit> ignoreSet = null)
        {
            if (manager == null) return false;
            foreach (var u in manager.AllUnits)
            {
                if (u == null || u == this || u == ignoreUnit || !u.IsAlive()) continue;
                if (ignoreSet != null && ignoreSet.Contains(u)) continue;
                float minDist = GetMinDistance(u);
                Vector3 toU = u.transform.position - pos;
                toU.y = 0f;
                if (toU.sqrMagnitude < minDist * minDist) return true;
            }
            return false;
        }

        /// <summary>自分と相手の bodyRadius の合計 = 最小中心間距離</summary>
        private float GetMinDistance(RealtimeBattleUnit other)
        {
            if (other == null) return bodyRadius;
            return bodyRadius + other.bodyRadius;
        }

        [Tooltip("怯みデフォルト秒数 (ApplyFlinch の引数省略時に使う)")]
        public float flinchDurationSec = 0.4f;
        // ContinuousMover.IsAnchored から参照されるため internal に
        internal float flinchUntil = 0f;
        public bool IsFlinching => BT < flinchUntil;

        /// <summary>Barricade を発動可能か pre-check (CT 消費前)。
        /// 物理的に置けるか (Barricade.CanPlace が Ok) のみ判定する。
        /// 「奥に敵が居るか」は action list 側 condition で表現する (コード側で重複 check しない)。</summary>
        private bool CanPlaceMeaningfulBarricade()
        {
            return TryFindBarricadePlacement(out _, out _);
        }

        /// <summary>真直 0° で failed なら ±10°, ±20°, ±30° と角度を振って通る placement を探す。
        /// 通った角度で center / facing を返す。 Phase 2 の facing 固定で field 端の Knight が
        /// axis-aligned wall corners 落ちて FieldBound になる問題への対処。</summary>
        private bool TryFindBarricadePlacement(out Vector3 outCenter, out Vector3 outFacing)
        {
            outCenter = transform.position;
            outFacing = transform.forward;
            if (manager == null) return false;
            Vector3 fwd0 = transform.forward; fwd0.y = 0f;
            if (fwd0.sqrMagnitude < 0.001f) fwd0 = Vector3.forward;
            fwd0.Normalize();
            float radius = (mover != null) ? mover.radius : 0.36f;
            // 0°, +10, -10, +20, -20, +30, -30 の順で試す
            float[] angles = { 0f, 10f, -10f, 20f, -20f, 30f, -30f };
            foreach (var a in angles)
            {
                Vector3 facing = (a == 0f) ? fwd0 : Quaternion.AngleAxis(a, Vector3.up) * fwd0;
                Vector3 center = transform.position + facing * 1.5f;
                center.y = transform.position.y;
                if (Barricade.CanPlace(center, facing, this, manager, radius) == Barricade.PlacementResult.Ok)
                {
                    outCenter = center;
                    outFacing = facing;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Knight rank 5 Barricade: caster の前 1.5m に 3m 幅の壁を設置。
        /// 位置検証 (フィールド境界 / 既存壁衝突 / 味方衝突 / 敵押し出し可否) して OK なら配置、敵を押し出す。
        /// Python tools/battle_sim/unit.py._place_barricade の 1:1 移植。</summary>
        public bool PlaceBarricade()
        {
            if (manager == null) return false;
            // 設置位置: caster の forward 方向 1.5m 前 (角度サーチで通った placement を採用)
            if (!TryFindBarricadePlacement(out Vector3 center, out Vector3 facing))
            {
                manager.BattleLog.Add($"[{BT:F2}s] {displayName} Barricade 設置失敗 (no valid angle)");
                return false;
            }
            float radius = (mover != null) ? mover.radius : 0.36f;
            var bar = Barricade.Spawn(this, center, facing, BT);
            manager.barricades.Add(bar);
            manager.RebuildBarricadeMap();   // GridBarricadeMap の blocked セルを更新
            Barricade.PushEnemiesAside(manager, bar, radius);
            manager.BattleLog.Add($"[{BT:F2}s] {displayName} Barricade 設置 (HP {bar.hp}, 5s)");
            // 全ユニットの target/path を即時再評価 (バリケード出現で迂回開始)
            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                u.nextDecisionTime = 0f;
                if (u.mover != null) u.mover.simulateMovement = false;  // 旧 destination 解除、次フレで AI が方向再決定
            }
            return true;
        }

        /// <summary>怯み攻撃を受けた時のみ呼ぶ。指定秒数の間 Damage アニメ + 完全行動不能 (移動/向き変更/攻撃 全て NG)。</summary>
        public void ApplyFlinch(float durationSec = -1f)
        {
            if (durationSec <= 0f) durationSec = flinchDurationSec;
            flinchUntil = BT + durationSec;
            anim?.PlayDamage();
            // 進行中の transit を含めて移動を完全停止
            if (mover != null) mover.StopMovement();
        }

        /// <summary>被弾時に外部から呼ぶ。attacker 情報で lastAttacker を更新 (ターゲット切替用)。
        /// Damage アニメは再生しない (怯み攻撃なら別途 ApplyFlinch を呼ぶ)。</summary>
        public void OnHit(bool died, RealtimeBattleUnit attacker = null)
        {
            if (died) anim?.PlayDie();
            if (attacker != null && attacker != this && attacker.IsAlive())
            {
                lastAttacker = attacker;
                lastAttackerUntil = BT + 3f; // 3 秒間有効
                nextDecisionTime = 0f; // 即 target 再評価
            }
        }

        [Header("死亡演出")]
        [Tooltip("死亡アニメ本体の長さ (秒)。これ + deathFadeDelaySec の経過後に盤面から消去される")]
        public float deathAnimDurationSec = 1.5f;
        [Tooltip("死亡アニメ終了後、消去までの猶予 (秒)")]
        public float deathFadeDelaySec = 1.0f;
        private float _deathTime = -1f;          // 死亡瞬間の BT
        private bool _deathRemoved = false;       // 消去済フラグ

        /// <summary>生死が切り替わった瞬間に同期する。
        /// 死亡: mover 停止。占有はアニメ終了 + delay 経過まで維持 (障害物扱い)、その後解放して GameObject 非アクティブ化。
        /// 復活: 移動再開可能に。</summary>
        private void SyncAliveState()
        {
            bool alive = IsAlive();
            if (_prevAlive != alive)
            {
                if (mover != null)
                {
                    if (alive)
                    {
                        mover.simulateMovement = false; // 復活時は idle スタート
                        // 復活: もし以前消去済なら GameObject 復帰 + 占有再登録
                        if (_deathRemoved)
                        {
                            gameObject.SetActive(true);
                            _deathRemoved = false;
                        }
                    }
                    else
                    {
                        mover.StopMovement();
                        _deathTime = BT;          // 死亡開始時刻を記録
                        _deathRemoved = false;
                    }
                }
                _prevAlive = alive;
            }

            // 死亡から所定時間経過したら盤面から消去 (occupant 解放 + GameObject 非表示)
            if (!alive && !_deathRemoved && _deathTime > 0f)
            {
                float elapsed = BT - _deathTime;
                if (elapsed >= deathAnimDurationSec + deathFadeDelaySec)
                {
                    if (mover != null)
                    {
                        // mover を無効化 (死亡消去後は移動不要)
                        mover.Detach();
                        mover.enabled = false;
                    }
                    gameObject.SetActive(false);
                    _deathRemoved = true;
                }
            }
        }

        // ─────────────────────────────────
        // 優先度アクション評価
        // ─────────────────────────────────

        /// <summary>デフォルト行動リスト (Inspector で override なしの場合)。
        /// skills[2], skills[1] を優先して、CT空+射程内なら発動。基礎(0)がフォールバック。
        /// Self系/Heal系/Move系 等の「敵が射程内」では発動しないスキルは手動でactionList設定必要。</summary>
        private static readonly List<RealtimeActionEntry> _defaultList = new()
        {
            new RealtimeActionEntry {
                condition = RealtimeCondition.CanCastSkill, conditionSkillIndex = 2,
                action = RealtimeAction.CastSkill, actionSkillIndex = 2,
                label = "skills[2] が使えるなら発動"
            },
            new RealtimeActionEntry {
                condition = RealtimeCondition.CanCastSkill, conditionSkillIndex = 1,
                action = RealtimeAction.CastSkill, actionSkillIndex = 1,
                label = "skills[1] が使えるなら発動"
            },
            new RealtimeActionEntry(RealtimeCondition.CanBasicAttack, RealtimeAction.BasicAttack, "基礎攻撃"),
            new RealtimeActionEntry(RealtimeCondition.Always, RealtimeAction.MoveToOwnRange, "射程まで接近"),
        };

        private List<RealtimeActionEntry> DefaultActionList() => _defaultList;

        /// <summary>
        /// 職別距離カテゴリ (基本攻撃の射程)。preferredRange から逆算。
        /// </summary>
        public float GetAttackRangeMass()
        {
            return GetPreferredRangeWorld();
        }

        private bool EvalCondition(RealtimeActionEntry entry, float dist)
        {
            var cond = entry.condition;
            switch (cond)
            {
                case RealtimeCondition.Always: return true;
                case RealtimeCondition.CanBasicAttack:
                    // skills[0] があればそのCT/射程を使う、なければ内蔵
                    if (skills != null && skills.Count > 0 && skills[0] != null)
                    {
                        var bs = skills[0];
                        float effRange = bs.shape.rangeMax * rangeMul + rangeBonus + basicRangeBonus;
                        return IsSkillReady(0) && dist >= bs.shape.rangeMin && dist <= effRange;
                    }
                    return BT >= nextAttackTime && dist <= GetAttackRangeMass();
                case RealtimeCondition.TargetWithinClose:   return dist <= RangeCloseMass;
                case RealtimeCondition.TargetWithinMid:     return dist <= RangeMidMass;
                case RealtimeCondition.TargetWithinFar:     return dist <= RangeFarMass;
                case RealtimeCondition.TargetWithinMaxFar:  return dist <= RangeMaxFarMass;
                case RealtimeCondition.TargetOutsideClose:  return dist > RangeCloseMass;
                case RealtimeCondition.TargetOutsideMid:    return dist > RangeMidMass;
                case RealtimeCondition.TargetOutsideFar:    return dist > RangeFarMass;
                case RealtimeCondition.HpBelow50:
                    return unit != null && unit.CurrentHp * 2 < unit.MaxHp;
                case RealtimeCondition.TargetHpBelow30:
                    return currentTarget != null && currentTarget.unit != null
                        && currentTarget.unit.CurrentHp * 10 < currentTarget.unit.MaxHp * 3;

                case RealtimeCondition.SkillCooldownReady:
                    return IsSkillReady(entry.conditionSkillIndex);
                case RealtimeCondition.TargetInSkillRange:
                    return IsTargetInSkillRange(entry.conditionSkillIndex);
                case RealtimeCondition.CanCastSkill:
                    return IsSkillReady(entry.conditionSkillIndex) && IsTargetInSkillRange(entry.conditionSkillIndex);
                case RealtimeCondition.EnemiesHitCountGe:
                    return CountSkillHits(entry.conditionSkillIndex) >= Mathf.RoundToInt(entry.conditionParam);
                case RealtimeCondition.AnyEnemyInSkillRange:
                    return AnyEnemyInSkillRange(entry.conditionSkillIndex);
                case RealtimeCondition.CanCastSkillAny:
                    return IsSkillReady(entry.conditionSkillIndex) && AnyEnemyInSkillRange(entry.conditionSkillIndex);
                case RealtimeCondition.TargetOutsideSkillRange:
                    return currentTarget != null && !IsTargetInSkillRange(entry.conditionSkillIndex);
                case RealtimeCondition.SelfHpBelowPercent:
                    return unit != null && unit.CurrentHp * 100f < unit.MaxHp * entry.conditionParam;
                case RealtimeCondition.AllyHpBelowPercent:
                    return FindAllyHpBelow(entry.conditionParam) != null;
                case RealtimeCondition.CurrentTargetIsAlly:
                    return currentTarget != null && currentTarget.ownerSide == ownerSide;
                case RealtimeCondition.CurrentTargetIsEnemy:
                    return currentTarget != null && currentTarget.ownerSide != ownerSide;
                case RealtimeCondition.FragileAllyTargetedByEnemy:
                    return IsFragileAllyTargetedByEnemy();
                case RealtimeCondition.WasAttackedRecently:
                    return IsLastAttackerValid();
                case RealtimeCondition.AttackedAndNoEnemyInBasicRange:
                    return IsLastAttackerValid() && !AnyEnemyInPreferredRange();
                case RealtimeCondition.AttackerCloserThanCurrentTarget:
                    return IsAttackerCloserThanCurrentTarget();
                case RealtimeCondition.NoLongRangeAttack:
                    return !HasLongRangeAttackSkill();
                case RealtimeCondition.EnemyInContact:
                    return IsEnemyInContact();
            }
            return false;
        }

        /// <summary>3m 以上の射程で使える攻撃スキル (Attack タイプ・敵対象) を 1 つでも持っていれば true。
        /// basicAttackDisabled (秘術封印 / アルカナマキシマス等) のとき skills[0] は除外する。</summary>
        private bool HasLongRangeAttackSkill()
        {
            if (skills == null) return false;
            for (int i = 0; i < skills.Count; i++)
            {
                var s = skills[i];
                if (s == null) continue;
                if (i == 0 && basicAttackDisabled) continue;     // 基本攻撃無効化されてれば skill[0] 除外
                if (s.skillType != RealtimeSkillType.Attack) continue;
                if (s.targetSide != RealtimeTargetSide.Enemy) continue;
                if (s.shape != null && s.shape.rangeMax >= 3f) return true;
            }
            return false;
        }

        private bool IsLastAttackerValid()
        {
            return lastAttacker != null && lastAttacker.IsAlive() && BT < lastAttackerUntil;
        }

        /// <summary>被弾直近 AND lastAttacker が現ターゲットより近い AND currentTarget が基本スキル(skills[0])射程外。
        /// (currentTarget が射程内なら攻撃継続が優先で、attacker への切替は行わない)</summary>
        private bool IsAttackerCloserThanCurrentTarget()
        {
            if (!IsLastAttackerValid()) return false;
            if (currentTarget == null || !currentTarget.IsAlive()) return true; // target 無し → attacker 優先
            // currentTarget が基本スキル射程内なら切替不要 (そのまま攻撃継続)
            if (IsTargetInSkillRange(0)) return false;
            float dTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            float dAttacker = Vector3.Distance(transform.position, lastAttacker.transform.position);
            return dAttacker < dTarget;
        }

        /// <summary>接敵判定 (RealtimeCondition.EnemyInContact 用)。
        /// 自分中心 EnemyContactRadius 球内に敵が 1 体以上居れば true。
        /// ContinuousMover の path obstacle 検出 (進行方向 forward 1.0m 通路) より
        /// わずかに広い (1.05m) 球で覆うことで、敵が遠くから近づく時に
        /// 必ず「接敵 → ターゲット切替」が「ルート上 → 迂回」より先に発火するのを保証する。
        /// 進行方向に依存しない (横や後ろの敵も接敵扱い) ことで、押し合い stuck の
        /// 解消をターゲット切替経由で行う。</summary>
        public const float EnemyContactRadius = 1.05f;
        private bool IsEnemyInContact()
        {
            if (manager == null) return false;
            Vector3 myPos = transform.position; myPos.y = 0f;
            int side = ownerSide;
            const float r2 = EnemyContactRadius * EnemyContactRadius;
            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive() || u.ownerSide == side) continue;
                Vector3 rel = u.transform.position - myPos; rel.y = 0f;
                if (rel.sqrMagnitude <= r2) return true;
            }
            return false;
        }

        /// <summary>preferredRange (職のお約束距離: Warrior=1, Lancer=2等) 以内に敵がいるか</summary>
        private bool AnyEnemyInPreferredRange()
        {
            if (manager == null) return false;
            float maxR = GetPreferredRangeWorld();
            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                if (u.ownerSide == ownerSide) continue;
                float d = Vector3.Distance(transform.position, u.transform.position);
                if (d <= maxR) return true;
            }
            return false;
        }

        // ─────────────────────────────────
        // ターゲット維持ロジック
        // ─────────────────────────────────

        /// <summary>挑発 (外部から呼ぶ)。指定時間、taunter を強制ターゲット化。</summary>
        /// <summary>基本攻撃 (skills[0]) の CT を即時リセット。CooldownResetEnemyBasic 用。</summary>
        public void ResetBasicAttackCooldown()
        {
            nextAttackTime = BT;
            if (skillNextReadyTime != null && skillNextReadyTime.Length > 0)
                skillNextReadyTime[0] = BT;
        }

        /// <summary>致死ダメチェック: SurviveLethal を持ってて致死ダメなら HP1 残しで耐える。効果消費 + log 出力。
        /// (不屈 skill_nd_tenacity 用。HP%復活が欲しい場合は AutoRevive を使う)</summary>
        private static void ProcessSurviveLethal(RealtimeBattleUnit tgt, ref int dmg)
        {
            if (tgt?.unit == null || dmg <= 0) return;
            if (dmg >= tgt.unit.CurrentHp && tgt.unit.HasActiveEffect(StatusEffectType.SurviveLethal))
            {
                dmg = Mathf.Max(0, tgt.unit.CurrentHp - 1);
                tgt.unit.ConsumeActiveEffect(StatusEffectType.SurviveLethal);
                tgt.manager?.BattleLog.Add($"  → {tgt.displayName} ☆不屈! HP1 で耐えた");
            }
        }

        /// <summary>致死ダメチェック: AutoRevive を持ってて致死ダメなら HP value% で復活する。効果消費 + log 出力。
        /// (十字架の加護 WP_Cross_01 用。HP1残しが欲しい場合は SurviveLethal を使う)</summary>
        private static void ProcessAutoRevive(RealtimeBattleUnit tgt, ref int dmg)
        {
            if (tgt?.unit == null || dmg <= 0) return;
            if (dmg >= tgt.unit.CurrentHp && tgt.unit.HasActiveEffect(StatusEffectType.AutoRevive))
            {
                int pct = Mathf.Max(1, tgt.unit.GetActiveEffectValue(StatusEffectType.AutoRevive));
                int reviveHp = Mathf.Max(1, Mathf.RoundToInt(tgt.unit.MaxHp * pct / 100f));
                dmg = Mathf.Max(0, tgt.unit.CurrentHp - reviveHp);
                tgt.unit.ConsumeActiveEffect(StatusEffectType.AutoRevive);
                tgt.manager?.BattleLog.Add($"  → {tgt.displayName} ✝復活! HP {reviveHp} ({pct}%)");
            }
        }

        /// <summary>現在 CT 中 (skillNextReadyTime[i] > BT) のスキルすべての残り CT を seconds 秒延長。
        /// 既に ready のスキルや nextAttackTime が ready の場合は影響しない (ウエイトカッター 時断ち用)。</summary>
        public void ExtendActiveCooldowns(float seconds)
        {
            if (seconds <= 0f) return;
            if (skillNextReadyTime != null)
            {
                for (int i = 0; i < skillNextReadyTime.Length; i++)
                {
                    if (skillNextReadyTime[i] > BT)
                        skillNextReadyTime[i] += seconds;
                }
            }
            if (nextAttackTime > BT) nextAttackTime += seconds;
        }

        /// <summary>ターゲットを外部から強制的に切替 (シャドウハンター de-taunt 用)。 nextDecisionTime を即時化して
        /// 次フレで再評価させる。 強制 (forcedTarget) ではなく通常 currentTarget を上書きするため、 attacker の
        /// 通常の MaintainCurrentTarget で再選定される可能性あり (1 撃避けたい用途には十分)。</summary>
        public void SwitchTargetTo(RealtimeBattleUnit newTarget)
        {
            currentTarget = newTarget;
            nextDecisionTime = 0f;
        }

        public void ForceTarget(RealtimeBattleUnit taunter, float durationSec)
        {
            forcedTarget = taunter;
            forcedUntil = BT + durationSec;
            nextDecisionTime = 0f; // 即時再評価
        }

        private void MaintainCurrentTarget()
        {
            // 攻撃アニメ中 / のけぞり中はターゲット切替を凍結
            // (攻撃発動時に向きを固定したまま振り終えるため、anim 中に対象が死亡しても切替えない)
            // 解除されたフレームで targetGone (=対象死亡) なら直後の判定で即時再選択ルートに入るので
            // anim 終了後の切替は確実に発生する。
            // anim.IsBusyState() でも Animator 上の Attack 状態を見て凍結判定する。
            // ただし Animator の戻し漏れで永久ロックを防ぐため、 attackingUntil 超過後 1.5s で
            // IsBusyState 由来のロックは強制解除 (Update 側と整合)。
            const float animSafetyTimeoutSec = 1.5f;
            bool animBusy = anim != null && anim.IsBusyState();
            bool isAttacking = (BT < attackingUntil + postAttackLockSec)
                            || (animBusy && BT < attackingUntil + animSafetyTimeoutSec);
            if (isAttacking || IsFlinching) return;

            // 1. 挑発: 強制ターゲット固定 (有効期間中)
            if (forcedTarget != null && forcedTarget.IsAlive() && BT < forcedUntil)
            {
                currentTarget = forcedTarget;
                return;
            }
            if (forcedTarget != null)
            {
                // 挑発解除
                forcedTarget = null;
                nextDecisionTime = 0f; // 解除時に即再評価
            }

            // 2. 再評価タイミング判定
            bool targetGone = currentTarget == null || !currentTarget.IsAlive();
            bool timeToReEval = BT >= nextDecisionTime;
            if (!targetGone && !timeToReEval) return; // 維持

            // 3. targetList 上から評価して、条件マッチ AND 有効候補が居る最初の entry を採用。
            // (rangeFilterSkillIndex で候補ゼロなら次のルールへフォールスルー)
            var list = GetEffectiveTargetList();
            int dominantIdx = -1;
            RealtimeTargetEntry dominant = null;
            RealtimeBattleUnit picked = null;
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (!EvalTargetCondition(e)) continue;
                var t = SelectTargetByEntry(e);
                if (t == null) continue;  // 候補無し → 次優先度へ
                dominantIdx = i;
                dominant = e;
                picked = t;
                break;
            }

            nextDecisionTime = BT + decisionIntervalSec;

            if (dominant == null)
            {
                currentTarget = null;
                lastDominantIndex = -1;
                return;
            }

            // 4. dominant が前回と同じで currentTarget が生きてれば維持 (振動防止)
            if (dominantIdx == lastDominantIndex && currentTarget != null && currentTarget.IsAlive())
            {
                return;
            }

            // 5. 新 dominant or target 死亡 → 再選択
            currentTarget = picked;
            lastDominantIndex = dominantIdx;
        }

        /// <summary>基本スキル (skills[0]) を barricade に発動。timeline の DealDamage 時刻で ApplyFixedHit (1 ダメ固定)。
        /// status/knockback は barricade に効かない (DealDamage 経路で barricadeTarget を見て ApplyFixedHit のみ走る)。</summary>
        private bool CastBasicAttackOnBarricade(Barricade target)
        {
            if (target == null || !target.IsActive) return false;
            if (skills == null || skills.Count == 0) return false;
            var s = skills[0];
            if (s == null) return false;
            if (BT < skillNextReadyTime[0]) return false;

            // 向き合わせ
            Vector3 toB = target.center - transform.position;
            toB.y = 0f;
            if (toB.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(toB.normalized, Vector3.up);

            anim?.PlayAttackForSkill(s);
            skillNextReadyTime[0] = BT + s.cooldownSec * skillCooldownMul * cooldownMul;
            attackingUntil = BT + s.castAnimSec;
            activeCastSkill = s;
            BeginCastInfo(s, 0); // 行動妨害共通仕様

            // timeline 経由で発動 (DealDamage 時刻に barricade hit、PlaySound などもそのまま発火)
            if (s.timeline != null && s.timeline.Count > 0)
            {
                foreach (var ev in s.timeline)
                {
                    if (ev == null) continue;
                    pendingEvents.Add(new PendingSkillEvent
                    {
                        fireTime = BT + Mathf.Max(0f, ev.timeSec),
                        ev = ev,
                        skill = s,
                        primaryTarget = null,
                        hitTargets = null,
                        barricadeTarget = ev.kind == RealtimeSkillEventKind.DealDamage ? target : null,
                        castOrigin = transform.position,
                        castAim = target.center,
                        castDir = toB.sqrMagnitude > 0.01f ? toB.normalized : transform.forward,
                        damageMul = 1f,
                    });
                }
            }
            else
            {
                // timeline 無しの場合は即時 1 ダメ
                int b4 = target.hp;
                int dlt = target.ApplyFixedHit();
                manager?.OnAttackBarricade(this, target, dlt, b4, target.hp);
            }
            return true;
        }

        /// <summary>基本スキル (skills[0]) 射程内に敵側バリケードがあれば currentBarricadeTarget にセット。
        /// CanBasicAttack / ExecuteAction.BasicAttack でこちらを優先攻撃。
        /// 主に Warrior/Knight (近接 Slash) で機能。Archer/Mage は DealDamage 経路でバリケードに自動衝突する。</summary>
        private void UpdateBarricadeTarget()
        {
            currentBarricadeTarget = null;
            if (manager?.barricades == null || manager.barricades.Count == 0) return;
            if (skills == null || skills.Count == 0) return;
            var basicSkill = skills[0];
            if (basicSkill == null) return;
            float reachMax = GetShapeMaxReach(basicSkill.shape);
            Barricade closest = null;
            float closestDist = float.MaxValue;
            Vector3 myPos = transform.position;
            foreach (var b in manager.barricades)
            {
                if (b == null || !b.IsActive) continue;
                // 自陣バリケードは攻撃しない
                if (b.owner != null && b.owner.ownerSide == ownerSide) continue;
                float d = b.DistanceTo(myPos);
                if (d > reachMax) continue;
                if (d < closestDist)
                {
                    closestDist = d;
                    closest = b;
                }
            }
            currentBarricadeTarget = closest;
        }

        private static readonly List<RealtimeTargetEntry> _defaultTargetList = new()
        {
            new RealtimeTargetEntry {
                condition = RealtimeCondition.Always,
                targetSide = RealtimeTargetSide.Enemy,
                targetSelect = RealtimeTargetSelect.Nearest,
                label = "default: 敵最寄"
            }
        };

        private List<RealtimeTargetEntry> GetEffectiveTargetList()
        {
            return (targetList != null && targetList.Count > 0) ? targetList : _defaultTargetList;
        }

        /// <summary>
        /// TargetEntry 用の条件評価 (ActionEntry とは別: 距離計算不要なので簡略版)。
        /// TargetInSkillRange 等の「現在ターゲット」依存条件は原則使わない想定。
        /// </summary>
        private bool EvalTargetCondition(RealtimeTargetEntry e)
        {
            float tdist = (currentTarget != null)
                ? Vector3.Distance(transform.position, currentTarget.transform.position)
                : float.MaxValue;
            switch (e.condition)
            {
                case RealtimeCondition.Always: return true;
                case RealtimeCondition.SelfHpBelowPercent:
                    return unit != null && unit.CurrentHp * 100f < unit.MaxHp * e.conditionParam;
                case RealtimeCondition.AllyHpBelowPercent:
                    return FindAllyHpBelow(e.conditionParam) != null;
                case RealtimeCondition.SkillCooldownReady:
                    return IsSkillReady(e.conditionSkillIndex);
                case RealtimeCondition.TargetWithinClose:
                    return currentTarget != null && tdist <= RangeCloseMass;
                case RealtimeCondition.TargetWithinMid:
                    return currentTarget != null && tdist <= RangeMidMass;
                case RealtimeCondition.TargetWithinFar:
                    return currentTarget != null && tdist <= RangeFarMass;
                case RealtimeCondition.TargetOutsideClose:
                    return currentTarget != null && tdist > RangeCloseMass;
                case RealtimeCondition.TargetOutsideMid:
                    return currentTarget != null && tdist > RangeMidMass;
                case RealtimeCondition.TargetOutsideFar:
                    return currentTarget != null && tdist > RangeFarMass;
                case RealtimeCondition.TargetOutsideSkillRange:
                    return currentTarget != null && !IsTargetInSkillRange(e.conditionSkillIndex);
                case RealtimeCondition.AnyEnemyInSkillRange:
                    return AnyEnemyInSkillRange(e.conditionSkillIndex);
                case RealtimeCondition.CanCastSkillAny:
                    return IsSkillReady(e.conditionSkillIndex) && AnyEnemyInSkillRange(e.conditionSkillIndex);
                case RealtimeCondition.FragileAllyTargetedByEnemy:
                    return IsFragileAllyTargetedByEnemy();
                case RealtimeCondition.WasAttackedRecently:
                    return IsLastAttackerValid();
                case RealtimeCondition.AttackedAndNoEnemyInBasicRange:
                    return IsLastAttackerValid() && !AnyEnemyInPreferredRange();
                case RealtimeCondition.AttackerCloserThanCurrentTarget:
                    return IsAttackerCloserThanCurrentTarget();
                case RealtimeCondition.NoLongRangeAttack:
                    return !HasLongRangeAttackSkill();
                case RealtimeCondition.EnemyInContact:
                    return IsEnemyInContact();
            }
            return false;
        }

        /// <summary>shape ごとの実効リーチ (m)。shapeSizeBonus / rangeBonus / rangeMul を加算する。
        /// 注: basicRangeBonus は呼び出し側で skill index が判別できないので含めない (基本攻撃用射程は CanBasicAttack / IsTargetInSkillRange 側で別途加算)。</summary>
        private float GetShapeMaxReach(RealtimeSkillShape sh)
        {
            if (sh == null) return 1f;
            float bonus = shapeSizeBonus;
            switch (sh.shape)
            {
                case RealtimeTargetShape.Single:
                case RealtimeTargetShape.CircleAtTarget:
                    return sh.rangeMax * rangeMul + rangeBonus; // 着弾型は rangeBonus 優先
                case RealtimeTargetShape.CircleAtSelf:
                case RealtimeTargetShape.Fan:
                    return sh.widthMass + bonus;
                case RealtimeTargetShape.Line:
                case RealtimeTargetShape.Square:
                case RealtimeTargetShape.CrossAtSelf:
                    return sh.lengthMass + bonus;
                case RealtimeTargetShape.Diamond:
                    return Mathf.Max(sh.widthMass, sh.lengthMass) + bonus;
                default: return sh.rangeMax * rangeMul + rangeBonus;
            }
        }

        /// <summary>TargetEntry の side/select/filter で1体選ぶ</summary>
        private RealtimeBattleUnit SelectTargetByEntry(RealtimeTargetEntry e)
        {
            if (manager == null) return null;
            if (e.targetSide == RealtimeTargetSide.Self) return this;
            // LastAttacker は特別扱い (lastAttacker 参照、生存かつ有効期限内)
            if (e.targetSelect == RealtimeTargetSelect.LastAttacker)
            {
                return IsLastAttackerValid() ? lastAttacker : null;
            }

            var candidates = new List<RealtimeBattleUnit>();
            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                switch (e.targetSide)
                {
                    case RealtimeTargetSide.Enemy:
                        if (u.ownerSide == ownerSide) continue; break;
                    case RealtimeTargetSide.Ally:
                        if (u.ownerSide != ownerSide || u == this) continue; break;
                    case RealtimeTargetSide.AnyAllyIncludingSelf:
                        if (u.ownerSide != ownerSide) continue; break;
                }
                if (e.jobFilter != null && e.jobFilter.Length > 0)
                {
                    string jid = u.unit?.Body?.BodyJobId;
                    bool match = false;
                    foreach (var j in e.jobFilter)
                        if (j != null && j.bodyJobId == jid) { match = true; break; }
                    if (!match) continue;
                }
                // rangeFilterSkillIndex が指定されてれば、その skill の射程内の敵だけ通す
                if (e.rangeFilterSkillIndex >= 0)
                {
                    var s = GetSkill(e.rangeFilterSkillIndex);
                    if (s != null)
                    {
                        float d = Vector3.Distance(transform.position, u.transform.position);
                        float maxReach = GetShapeMaxReach(s.shape);
                        if (d < s.shape.rangeMin || d > maxReach) continue;
                    }
                }
                // rangeFilterMaxDist (絶対距離フィルタ) が指定されていればその範囲内のみ
                if (e.rangeFilterMaxDist > 0f)
                {
                    float d = Vector3.Distance(transform.position, u.transform.position);
                    if (d > e.rangeFilterMaxDist) continue;
                }
                candidates.Add(u);
            }
            if (candidates.Count == 0) return null;

            // requireRoute: LineOfSight で経路確保できる候補のみに絞る (連続移動版)
            if (e.requireRoute && manager != null)
            {
                var bm = manager.barricadeMap;
                if (bm != null)
                {
                    var routable = new List<RealtimeBattleUnit>();
                    foreach (var u in candidates)
                    {
                        if (bm.LineOfSight(transform.position, u.transform.position))
                            routable.Add(u);
                    }
                    if (routable.Count > 0) candidates = routable;
                    // 全部到達不能なら絞らずに元の candidates を残す (詰みより無理にでも狙う)
                }
            }

            // target sticky: 現 target が候補の中に居る場合、ヒステリシス付きで優先する。
            // 同距離・微差で switch して向きが左右に振れるのを防ぐ。
            // 新候補が現 target より閾値以上 worse でないと switch しない。
            const float stickyHysteresis = 1.5f; // 1.5 cell 分以上良くないと switch しない
            RealtimeBattleUnit candidate;
            switch (e.targetSelect)
            {
                case RealtimeTargetSelect.Self: return this;
                case RealtimeTargetSelect.Random:
                    return candidates[Random.Range(0, candidates.Count)];
                case RealtimeTargetSelect.LowestHp:
                    candidate = BestBy(candidates, u => u.unit != null ? (float)u.unit.CurrentHp / Mathf.Max(1, u.unit.MaxHp) : 1f, false);
                    return PreferCurrentIfSimilar(candidate, candidates, u => u.unit != null ? (float)u.unit.CurrentHp / Mathf.Max(1, u.unit.MaxHp) : 1f, false, 0.05f);
                case RealtimeTargetSelect.HighestHp:
                    candidate = BestBy(candidates, u => u.unit != null ? (float)u.unit.CurrentHp / Mathf.Max(1, u.unit.MaxHp) : 1f, true);
                    return PreferCurrentIfSimilar(candidate, candidates, u => u.unit != null ? (float)u.unit.CurrentHp / Mathf.Max(1, u.unit.MaxHp) : 1f, true, 0.05f);
                case RealtimeTargetSelect.Farthest:
                    candidate = BestBy(candidates, u => Vector3.Distance(transform.position, u.transform.position), true);
                    return PreferCurrentIfSimilar(candidate, candidates, u => Vector3.Distance(transform.position, u.transform.position), true, stickyHysteresis);
                default: // Nearest
                    candidate = BestBy(candidates, u => Vector3.Distance(transform.position, u.transform.position), false);
                    return PreferCurrentIfSimilar(candidate, candidates, u => Vector3.Distance(transform.position, u.transform.position), false, stickyHysteresis);
            }
        }

        /// <summary>新候補と現 target の差が threshold 以下なら現 target を維持 (sticky)。</summary>
        private RealtimeBattleUnit PreferCurrentIfSimilar(
            RealtimeBattleUnit newBest,
            List<RealtimeBattleUnit> candidates,
            System.Func<RealtimeBattleUnit, float> metric,
            bool higherIsBetter,
            float threshold)
        {
            if (currentTarget == null || !currentTarget.IsAlive()) return newBest;
            if (!candidates.Contains(currentTarget)) return newBest;
            if (newBest == currentTarget) return currentTarget;
            float curScore = metric(currentTarget);
            float newScore = metric(newBest);
            // higherIsBetter=true のとき newScore > curScore なら新が良い、その差が閾値超えてれば switch
            // higherIsBetter=false (Nearest 等) なら newScore < curScore なら新が良い
            float improvement = higherIsBetter ? (newScore - curScore) : (curScore - newScore);
            return improvement >= threshold ? newBest : currentTarget;
        }

        private bool ExecuteAction(RealtimeActionEntry entry, Vector3 toTarget, float dist, float preferred, out bool committed)
        {
            committed = true; // 既定: 発火完了 (break)
            var act = entry.action;

            // 「実際に destination へ前進している間」だけ攻撃系を抑止。
            // 詰まって IDLE 同然 (EffectiveProgressSpeed ≈ 0) のときは攻撃発動を許す。
            // これでバリケードや味方に押し合って動けない時でも skill / 基本攻撃が撃てる。
            bool isAttackAction = act == RealtimeAction.BasicAttack || act == RealtimeAction.CastSkill;
            bool actuallyMoving = mover != null && mover.IsInTransit && mover.EffectiveProgressSpeed > 0.3f;
            if (isAttackAction && actuallyMoving)
            {
                committed = false;
                return false; // 次の優先度 (移動アクション等) にフォールスルー
            }

            switch (act)
            {
                case RealtimeAction.Wait:
                    return false;

                case RealtimeAction.BasicAttack:
                    // DisableBasicAttack (アルカナマキシマス等) 有効なら basic 攻撃をスキップ
                    if (basicAttackDisabled) { committed = false; break; }
                    // 隣接バリケード優先 (Warrior/Knight 等の近接職)
                    if (currentBarricadeTarget != null && currentBarricadeTarget.IsActive)
                    {
                        committed = CastBasicAttackOnBarricade(currentBarricadeTarget);
                        if (committed && mover != null) mover.StopMovement();
                        if (committed) return false;
                        // 失敗 (CT 中等) なら通常 currentTarget の攻撃に fall-through
                    }
                    // skills[0] があれば新スキルとして発動、なければ内蔵処理
                    if (skills != null && skills.Count > 0 && skills[0] != null)
                    {
                        committed = CastSkill(0); // 失敗なら次優先度へ fall-through
                        if (committed && mover != null) mover.StopMovement();
                    }
                    else if (BT >= nextAttackTime && dist <= GetAttackRangeMass())
                    {
                        FireBasicAttack(currentTarget);
                        nextAttackTime = BT + basicAttackCooldownSec;
                        attackingUntil = BT + attackAnimDurationSec;
                        if (mover != null) mover.StopMovement(); // 攻撃発動と同フレで即停止
                    }
                    else
                    {
                        committed = false;
                    }
                    return false;

                case RealtimeAction.CastSkill:
                    committed = CastSkill(entry.actionSkillIndex);
                    if (committed && mover != null) mover.StopMovement();
                    return false;

                case RealtimeAction.MoveToTarget:
                    return MoveToDistance(toTarget, dist, 0.1f);

                case RealtimeAction.MoveToLowestHpAlly:
                    {
                        var ally = FindAllyHpBelow(100f); // HP最少味方を取得
                        if (ally == null) return false;
                        Vector3 toAlly = ally.transform.position - transform.position;
                        toAlly.y = 0;
                        return MoveToDistance(toAlly, toAlly.magnitude, 0.5f);
                    }

                case RealtimeAction.MoveToOwnRange:
                {
                    // 緊急近接切替: 3m+ で使える攻撃スキル無し AND 敵 2m 内 AND この MoveToOwnRange が no-op になる
                    // (= 既に preferred 距離以内) → fall-through で次優先度 (近接接近) に切替。
                    // wouldNoOp ガードで preferred より外側 (= 通常通り近づくべきケース) は影響受けないようにしている。
                    // 例: Mage with ArcanaMaximus, preferred=4m, dist=2m → wouldNoOp=true → fall-through
                    //     Warrior preferred=1m, dist=2m → wouldNoOp=false → 通常移動 (1m まで詰める)
                    float wouldNoOpThreshold = preferred + (mover != null && mover.simulateMovement ? HoldEpsilonIn : HoldEpsilonOut);
                    bool wouldNoOp = dist <= wouldNoOpThreshold;
                    if (!HasLongRangeAttackSkill() && dist <= 2f && wouldNoOp)
                    {
                        committed = false; return false;
                    }
                    return MoveToDistance(toTarget, dist, preferred);
                }
                case RealtimeAction.MoveToOwnRangeKeep: return MaintainDistance(toTarget, dist, preferred);
                case RealtimeAction.MoveToCloseRange:   return MoveToDistance(toTarget, dist, RangeCloseMass);
                case RealtimeAction.MoveToMidRange:     return MoveToDistance(toTarget, dist, RangeMidMass);
                case RealtimeAction.MoveToFarRange:     return MoveToDistance(toTarget, dist, RangeFarMass);
                case RealtimeAction.MoveToMaxFarRange:  return MoveToDistance(toTarget, dist, RangeMaxFarMass);
                case RealtimeAction.MoveAwayToClose:    return MoveAwayToDistance(toTarget, dist, RangeCloseMass);
                case RealtimeAction.MoveAwayToMid:      return MoveAwayToDistance(toTarget, dist, RangeMidMass);
                case RealtimeAction.MoveAwayToFar:      return MoveAwayToDistance(toTarget, dist, RangeFarMass);
            }
            return false;
        }

        /// <summary>target の世界座標を返す。連続移動では transform.position 直返し。</summary>
        private Vector3 GetStableTargetWorldPos(RealtimeBattleUnit t)
        {
            if (t == null) return Vector3.zero;
            return t.transform.position;
        }

        // 移動再追跡 / 停止のヒステリシス定数。
        // HoldEpsilonIn 負値 → 移動中に preferred-0.15 まで詰めて停止。
        // これで停止後 (dist=preferred-0.15) → target が ~0.15m 動くまで再追跡しない (zigzag 抑制)。
        // HoldEpsilonOut = 0 → 停止中は preferred を 1mm でも超えたら即再追跡。
        // 旧値 0.40 は「preferred==rangeMax 職」(Warrior/Slash 等) で死帯 (1.0〜1.40m) を発生させ、
        // ノックバック後に dist が死帯に入ると「攻撃も移動もしない」 lock 状態になる致命バグの原因だった。
        // HoldEpsilonIn の -0.15m が hysteresis 役なので外側 buffer は不要。
        private const float HoldEpsilonIn = -0.15f;
        private const float HoldEpsilonOut = 0f;

        /// <summary>ターゲットとの距離を targetDist 以下まで詰める。
        /// Python _move_to_distance のヒステリシス方式 1:1 移植。
        /// 停止中なら (targetDist + HoldEpsilonOut) 以内は動かない。
        /// 移動中なら (targetDist + HoldEpsilonIn) 以内で停止。</summary>
        private bool MoveToDistance(Vector3 toTarget, float dist, float targetDist)
        {
            if (currentTarget == null || mover == null) return false;

            float threshold = targetDist + (mover.simulateMovement ? HoldEpsilonIn : HoldEpsilonOut);
            if (dist <= threshold)
            {
                mover.simulateMovement = false;
                return false;
            }

            // destination は target が 0.3m 以上動いたときのみ更新。毎フレ書き換えると
            // ContinuousMover の direction 計算が target 位置の微変動で振動するため。
            Vector3 newDest = GetStableTargetWorldPos(currentTarget);
            if ((newDest - mover.destination).sqrMagnitude > 0.09f)
                mover.destination = newDest;
            mover.endReachedDistance = targetDist;
            mover.maxSpeed = walkSpeed;
            mover.ignoreOccupant = null;
            mover.simulateMovement = true;
            return !mover.reachedDestination;
        }

        /// <summary>ターゲット距離を targetDist 以下 0.3m 以内に維持。</summary>
        private bool MaintainDistance(Vector3 toTarget, float dist, float targetDist)
        {
            const float tolerance = 0.3f;
            float maxDist = targetDist;
            float minDist = targetDist - tolerance;
            if (dist >= minDist && dist <= maxDist)
            {
                if (mover != null) mover.simulateMovement = false;
                return false;
            }
            if (toTarget.sqrMagnitude < 0.01f || mover == null) return false;

            if (dist > maxDist && currentTarget != null)
            {
                mover.destination = GetStableTargetWorldPos(currentTarget);
                mover.endReachedDistance = maxDist;
                mover.maxSpeed = walkSpeed;
                mover.ignoreOccupant = null;
                mover.simulateMovement = true;
                return true;
            }
            // 後退: 反対方向の point を destination に
            Vector3 awayPos = transform.position - toTarget.normalized * (minDist - dist);
            mover.destination = awayPos;
            mover.endReachedDistance = 0.1f;
            mover.maxSpeed = walkSpeed;
            mover.ignoreOccupant = null;
            mover.simulateMovement = true;
            return true;
        }

        /// <summary>ターゲットから targetDist 以上離れる。</summary>
        private bool MoveAwayToDistance(Vector3 toTarget, float dist, float targetDist)
        {
            if (dist >= targetDist)
            {
                if (mover != null) mover.simulateMovement = false;
                return false;
            }
            if (toTarget.sqrMagnitude < 0.01f || mover == null) return false;
            Vector3 awayPos = transform.position - toTarget.normalized * (targetDist - dist);
            mover.destination = awayPos;
            mover.endReachedDistance = 0.1f;
            mover.maxSpeed = walkSpeed;
            mover.ignoreOccupant = null;
            mover.simulateMovement = true;
            return true;
        }

        private RealtimeBattleUnit FindNearestEnemy()
        {
            if (manager == null) return null;
            RealtimeBattleUnit best = null;
            float bestDist = float.MaxValue;
            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                if (u.ownerSide == ownerSide) continue;
                float d = Vector3.Distance(transform.position, u.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = u;
                }
            }
            return best;
        }

        private void FireBasicAttack(RealtimeBattleUnit target)
        {
            if (target == null || !target.IsAlive()) return;
            if (unit == null || target.unit == null) return;

            int at = unit.AtFinal;
            int df = target.unit.DfFinal;
            int dmg = Mathf.Max(1, Mathf.RoundToInt(at - df * 0.5f));

            int before = target.unit.CurrentHp;
            ProcessAutoRevive(target, ref dmg);
            ProcessSurviveLethal(target, ref dmg);
            target.unit.TakeDamage(dmg);
            int after = target.unit.CurrentHp;

            anim?.PlayAttack();
            target.OnHit(target.unit.IsDead, this);

            manager?.OnAttack(this, target, dmg, before, after);
            SpawnDamagePopup(target.transform.position, dmg, DamagePopupSpawner.PopupKind.Damage);
        }

        private void SpawnDamagePopup(Vector3 pos, int value, DamagePopupSpawner.PopupKind kind)
        {
            if (manager == null) return;
            var sp = manager.damagePopup;
            if (sp == null)
            {
                // 無ければ自動生成 (最低限の表示保証)
                var go = new GameObject("DamagePopupSpawner(Auto)");
                go.transform.SetParent(manager.transform);
                sp = go.AddComponent<DamagePopupSpawner>();
                // プレハブを自動ロード (フォントが崩れないよう、プレハブ経由推奨)
#if UNITY_EDITOR
                sp.prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<DamagePopup>(
                    "Assets/0SteraCube/Prefabs/UI/DamagePopup.prefab");
#endif
                manager.damagePopup = sp;
            }
            sp.Spawn(pos, value, kind);
        }

        // ─────────────────────────────────
        // スキル関連ヘルパー
        // ─────────────────────────────────

        private bool IsSkillReady(int index)
        {
            if (skillNextReadyTime == null || index < 0 || index >= skillNextReadyTime.Length) return false;
            return BT >= skillNextReadyTime[index];
        }

        private RealtimeSkillDefinition GetSkill(int index)
        {
            if (skills == null || index < 0 || index >= skills.Count) return null;
            return skills[index];
        }

        /// <summary>現在ターゲットがスキルの射程内かチェック (rangeMul / rangeBonus / basicRangeBonus 込み)</summary>
        private bool IsTargetInSkillRange(int index)
        {
            var s = GetSkill(index);
            if (s == null) return false;
            // Self target (バフ等) は常に範囲内
            if (s.targetSide == RealtimeTargetSide.Self) return true;
            if (currentTarget == null) return false;
            float d = Vector3.Distance(transform.position, currentTarget.transform.position);
            float extra = (index == 0) ? basicRangeBonus : 0f;
            float effRange = s.shape.rangeMax * rangeMul + rangeBonus + extra;
            return d >= s.shape.rangeMin && d <= effRange;
        }

        /// <summary>いまスキル i を撃てば何体当たるか (現在ターゲット方向で計算)</summary>
        private int CountSkillHits(int index)
        {
            var s = GetSkill(index);
            if (s == null || manager == null) return 0;
            if (currentTarget == null) return 0;
            var hits = CollectSkillTargets(s, currentTarget);
            return hits.Count;
        }

        /// <summary>currentTarget に縛られず、敵が skills[i] の射程内に 1 体でも居るか。
        /// (Archer の「弓射程内に敵居れば currentTarget=隣接敵でも矢を撃つ」用)</summary>
        private bool AnyEnemyInSkillRange(int index)
        {
            var s = GetSkill(index);
            if (s == null || manager == null) return false;
            float maxReach = GetShapeMaxReach(s.shape);
            float minReach = s.shape.rangeMin;
            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                if (u.ownerSide == ownerSide) continue;
                float d = Vector3.Distance(transform.position, u.transform.position);
                if (d >= minReach && d <= maxReach) return true;
            }
            return false;
        }

        /// <summary>弓/魔法/槍 の味方が敵に狙われているか</summary>
        private bool IsFragileAllyTargetedByEnemy()
        {
            if (manager == null) return false;
            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                if (u.ownerSide == ownerSide) continue; // 敵側のみ調査
                var t = u.CurrentTarget;
                if (t == null || !t.IsAlive()) continue;
                if (t.ownerSide != ownerSide) continue; // 敵の target が味方
                string jid = t.unit?.Body?.BodyJobId;
                if (jid == "Archer" || jid == "Mage" || jid == "Lancer") return true;
            }
            return false;
        }

        /// <summary>HP が percent% 未満の味方 (最少 HP) を返す。なければ null</summary>
        private RealtimeBattleUnit FindAllyHpBelow(float percent)
        {
            if (manager == null) return null;
            RealtimeBattleUnit best = null;
            float bestRatio = float.MaxValue;
            foreach (var u in manager.AllUnits)
            {
                if (u == null || u == this || !u.IsAlive()) continue;
                if (u.ownerSide != ownerSide) continue;
                if (u.unit == null || u.unit.MaxHp <= 0) continue;
                float ratio = (float)u.unit.CurrentHp / u.unit.MaxHp * 100f;
                if (ratio >= percent) continue;
                if (ratio < bestRatio)
                {
                    bestRatio = ratio;
                    best = u;
                }
            }
            return best;
        }

        // ===== Skill Timeline Editor のテストプレイ用 公開ラッパー =====

        /// <summary>テストプレイで指定 index のスキルを強制発動 (CT 残・AI 停止 を無視せず CastSkill を直接呼ぶ)。</summary>
        public bool DebugCastSkill(int index) => CastSkill(index);

        /// <summary>テストプレイで Replay 前に CT/拘束/保留イベントをリセットする。</summary>
        public void DebugResetForReplay()
        {
            if (skillNextReadyTime != null)
            {
                for (int i = 0; i < skillNextReadyTime.Length; i++) skillNextReadyTime[i] = 0f;
            }
            attackingUntil = 0f;
            pendingEvents.Clear();
        }

        /// <summary>キャスト中アニメ拘束時間が残っているか (Replay 連打防止判定用)。</summary>
        public bool DebugIsCasting() => BT < attackingUntil;

        /// <summary>スキル発動。成功したら true、CT未達/対象なしで no-op なら false (fall-through)。</summary>
        private bool CastSkill(int index)
        {
            var s = GetSkill(index);
            if (s == null) return false;
            if (!IsSkillReady(index)) return false;  // CT 残り

            // Move スキル: 自己瞬間移動 (shape.rangeMax を距離として使用)
            if (s.skillType == RealtimeSkillType.Move)
            {
                ApplySelfMove(s);
                skillNextReadyTime[index] = BT + s.cooldownSec * skillCooldownMul * cooldownMul;
                attackingUntil = BT + s.castAnimSec;
                anim?.PlayAttackForSkill(s);
                BeginCastInfo(s, index); // 行動妨害共通仕様
                return true;
            }

            // Barricade pre-check: 設置不能 (フィールド境界 / 既存壁衝突 / 味方衝突 / 敵押し出し不能) や
            // 「奥に敵がいない (壁の意味なし)」場合は発動しない。CT は消費せず fall-through。
            if (s.skillType == RealtimeSkillType.Buff && s.skillId == "Barricade")
            {
                if (!CanPlaceMeaningfulBarricade()) return false;
            }

            // スキル独自の対象を再選択
            var primary = SelectSkillTarget(s);
            if (primary == null) return false;

            // basic attack か?
            bool isBasicCast = skills != null && skills.Count > 0 && s == skills[0];
            // アーチャー rank 5 トライアロー: basic attack を 3 方向 (±15°、合計 30°扇) に同時発射、各 0.9x
            bool useTriple = isBasicCast && hasTripleArrow;
            // アーチャー rank 3 貫通射撃: basic attack の FirstEnemyInLine 制限を解除
            bool usePierce = isBasicCast && hasPierceShot;

            // 向きを primary に
            Vector3 toP = primary.transform.position - transform.position;
            toP.y = 0;
            if (toP.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(toP.normalized, Vector3.up);

            // メイン dir
            Vector3 castOrigin = transform.position;
            Vector3 castAim = primary.transform.position;
            Vector3 mainDir = (castAim - castOrigin); mainDir.y = 0;
            if (mainDir.sqrMagnitude < 0.01f) mainDir = transform.forward;
            mainDir.Normalize();

            // 発射 specs: (方向, ダメ倍率)。トライアローなら 3 個、通常 1 個
            var shotSpecs = new List<(Vector3 dir, float mul)>();
            if (useTriple)
            {
                shotSpecs.Add((Quaternion.AngleAxis(-15f, Vector3.up) * mainDir, 0.9f));
                shotSpecs.Add((mainDir, 0.9f));
                shotSpecs.Add((Quaternion.AngleAxis(15f, Vector3.up) * mainDir, 0.9f));
            }
            else
            {
                shotSpecs.Add((mainDir, 1f));
            }

            bool anyHit = false;

            foreach (var (shotDir, dmgMul) in shotSpecs)
            {
                List<RealtimeBattleUnit> shotHits;
                if (usePierce)
                {
                    // 貫通: shape (=Single) を無視して、shotDir 方向の rangeMax まで全敵を拾う。
                    // ターゲットが手前にいても奥の敵まで貫通させる。基本攻撃なので basicRangeBonus も加算。
                    bool isBasicHere = (skills != null && skills.Count > 0 && s == skills[0]);
                    float basicExtra = isBasicHere ? basicRangeBonus : 0f;
                    float pierceMaxDist = s.shape.rangeMax * rangeMul + rangeBonus + basicExtra;
                    shotHits = CollectEnemiesInPierceLine(castOrigin, shotDir, pierceMaxDist, 0.5f);
                }
                else if ((shotDir - mainDir).sqrMagnitude < 1e-6f)
                {
                    // メイン dir = primary 経由収集
                    shotHits = CollectSkillTargets(s, primary);
                }
                else
                {
                    // 側矢: 方向ベースで収集
                    shotHits = CollectTargetsInDirection(s, shotDir);
                }
                // FirstEnemyInLine フィルタ: 貫通でなければ最近敵のみ
                if (s.projectileHitMode == ProjectileHitMode.FirstEnemyInLine
                    && shotHits.Count > 0 && !usePierce)
                {
                    RealtimeBattleUnit first = null;
                    float bestD = float.MaxValue;
                    foreach (var u in shotHits)
                    {
                        float d = Vector3.Distance(transform.position, u.transform.position);
                        if (d < bestD) { bestD = d; first = u; }
                    }
                    shotHits.Clear();
                    if (first != null) shotHits.Add(first);
                }
                if (shotHits.Count == 0) continue;
                anyHit = true;

                // shot 用 castAim を計算 (projectile delay 用)
                // FirstEnemyInLine: 実際に当たる敵の位置を castAim にすることで
                //   - DealDamage タイミング = 岩が敵に到達するタイミング と同期
                //   - 視覚 (Projectile.maxDistance) も同じ位置で停止
                Vector3 shotAim;
                if (s.projectileHitMode == ProjectileHitMode.FirstEnemyInLine
                    && shotHits.Count > 0 && !usePierce
                    && shotHits[0] != null)
                {
                    shotAim = shotHits[0].transform.position;
                }
                else
                {
                    shotAim = castOrigin + shotDir * s.shape.rangeMax;
                }
                float projectileDistanceDelay = 0f;
                if (s.projectileSpeed > 0f)
                {
                    projectileDistanceDelay = Vector3.Distance(castOrigin, shotAim) / s.projectileSpeed;
                }
                if (s.timeline != null && s.timeline.Count > 0)
                {
                    // 貫通射撃 (Line projectile + 飛翔速度あり) は 1 本の矢が複数の敵を貫くので、
                    // 各敵への到達タイミングは個別に計算する (caster→敵距離 / projectileSpeed)。
                    // ダメージ表記もそれぞれの到達瞬間に出るので、矢が手前敵→奥敵の順に貫いて見える。
                    bool isPierceLine = usePierce
                        && s.projectileHitMode == ProjectileHitMode.FirstEnemyInLine
                        && s.projectileSpeed > 0f;
                    List<RealtimeBattleUnit> sortedPierceHits = null;
                    if (isPierceLine && shotHits.Count > 1)
                    {
                        sortedPierceHits = new List<RealtimeBattleUnit>(shotHits);
                        Vector3 originForSort = castOrigin;
                        sortedPierceHits.Sort((a, b) =>
                        {
                            float da = (a.transform.position - originForSort).sqrMagnitude;
                            float db = (b.transform.position - originForSort).sqrMagnitude;
                            return da.CompareTo(db);
                        });
                    }

                    foreach (var ev in s.timeline)
                    {
                        if (ev == null) continue;
                        bool isDamageEvent = ev.kind == RealtimeSkillEventKind.DealDamage;

                        if (isPierceLine && isDamageEvent)
                        {
                            // 貫通: 各敵 1 体ずつ独立した PendingSkillEvent を生成し、各到達時刻で発火。
                            var hitsToSchedule = sortedPierceHits ?? shotHits;
                            foreach (var tgt in hitsToSchedule)
                            {
                                if (tgt == null) continue;
                                float perTargetDelay = Vector3.Distance(castOrigin, tgt.transform.position) / s.projectileSpeed;
                                pendingEvents.Add(new PendingSkillEvent
                                {
                                    fireTime = BT + Mathf.Max(0f, ev.timeSec) + perTargetDelay,
                                    ev = ev,
                                    skill = s,
                                    primaryTarget = primary,
                                    hitTargets = new List<RealtimeBattleUnit> { tgt },
                                    castOrigin = castOrigin,
                                    castAim = shotAim,
                                    castDir = shotDir,
                                    damageMul = dmgMul,
                                });
                            }
                            continue;  // 貫通 DealDamage は per-target で処理済 → 一括イベントは作らない
                        }

                        // 通常: 全敵まとめて 1 イベント。 DealDamage は遠端到達 delay、それ以外は cast 時刻基準。
                        float extraDelay = isDamageEvent ? projectileDistanceDelay : 0f;
                        pendingEvents.Add(new PendingSkillEvent
                        {
                            fireTime = BT + Mathf.Max(0f, ev.timeSec) + extraDelay,
                            ev = ev,
                            skill = s,
                            primaryTarget = primary,
                            hitTargets = shotHits,
                            castOrigin = castOrigin,
                            castAim = shotAim,
                            castDir = shotDir,
                            damageMul = dmgMul,
                        });
                    }
                }
                else
                {
                    // DealDamage イベントが timeline に無ければ即時適用 (後方互換)
                    foreach (var tgt in shotHits)
                    {
                        ApplySkillEffect(s, tgt, dmgMul);
                    }
                }
            }
            if (!anyHit) return false;
            // hits 変数 (旧コード参照用に primary だけのプレースホルダ)
            var hits = new List<RealtimeBattleUnit> { primary };

            // アニメ: Skill SO の animTriggerName / animStateName を見てスキル別 trigger を立てる
            // (Animator Controller に対応 Parameter / State が無ければデフォルト Attack に fallback)
            anim?.PlayAttackForSkill(s);

            // CT + ロック
            skillNextReadyTime[index] = BT + s.cooldownSec * skillCooldownMul * cooldownMul;
            attackingUntil = BT + s.castAnimSec;
            activeCastSkill = s; // AnimationEvent 参照用
            BeginCastInfo(s, index); // 行動妨害共通仕様

            // Knight Defend: ガード 100% 状態を 5 秒付与 (盾副次効果は盾本来 % で判定)
            if (!string.IsNullOrEmpty(s.skillId) && s.skillId == "Defend")
            {
                ForceGuardFor(5.0f);
            }
            // Knight Barricade: Buff 効果は ApplySkillEffect 経由だが、 timeline に PlaySound 等が
            // 1 個でも入ると即時適用経路がスキップされる。 ここで直接 PlaceBarricade() を呼ぶ。
            if (!string.IsNullOrEmpty(s.skillId) && s.skillId == "Barricade")
            {
                PlaceBarricade();
            }
            activeAttachment = (skillAttachments != null && index >= 0 && index < skillAttachments.Length)
                ? skillAttachments[index] : null;
            // 手装備があればキャスト開始時点で自動表示 (ArrowEqOn の AnimationEvent 不要)
            if (s.handAttachmentPrefab != null) ArrowEqOn();
            return true;
        }

        // ─────────────────────────────────
        // AnimationEvent (弓射系)
        // ─────────────────────────────────

        /// <summary>手に事前配置した矢を表示 (activeAttachment = 現在のキャストに紐付く矢)</summary>
        public void ArrowEqOn()
        {
            if (activeAttachment == null) return;
            activeAttachment.SetActive(true);
        }

        /// <summary>矢を複製して飛翔させ、元の矢は非表示に戻す。
        /// アーチャー rank 3 貫通射撃 / rank 5 トライアロー対応。</summary>
        public void ArrowEqOff()
        {
            if (activeAttachment == null) return;
            var orig = activeAttachment;
            if (!orig.activeSelf) return;

            // basic attack か & passive 判定
            bool isBasicCast = activeCastSkill != null
                && skills != null && skills.Count > 0 && activeCastSkill == skills[0];
            bool useTriple = isBasicCast && hasTripleArrow;
            bool usePierce = isBasicCast && hasPierceShot;

            // メイン方向 + currentTarget までの距離
            Vector3 origPos = orig.transform.position;
            Vector3 mainTargetPos = (currentTarget != null && currentTarget.IsAlive())
                ? currentTarget.transform.position
                : origPos + transform.forward * 10f;
            Vector3 mainDir = (mainTargetPos - origPos); mainDir.y = 0;
            if (mainDir.sqrMagnitude < 0.01f) mainDir = transform.forward;
            float mainDist = mainDir.magnitude;
            mainDir.Normalize();

            // 飛ばす方向リスト (トライアローなら 3 つ、通常 1 つ)
            Vector3[] dirs = useTriple
                ? new[] {
                    Quaternion.AngleAxis(-15f, Vector3.up) * mainDir,
                    mainDir,
                    Quaternion.AngleAxis(15f, Vector3.up) * mainDir,
                }
                : new[] { mainDir };

            // 元矢を非表示
            orig.SetActive(false);

            float spd = (activeCastSkill != null && activeCastSkill.projectileSpeed > 0f)
                ? activeCastSkill.projectileSpeed : 15f;
            float spdMul = RealtimeBattleManager.GlobalSpeed;

            // 貫通時は飛距離 = スキル実効 rangeMax (rangeMul / rangeBonus / basicRangeBonus 込み)
            // 通常時は飛距離 = primary までの距離
            float arrowMaxDist;
            if (usePierce && activeCastSkill != null && activeCastSkill.shape.rangeMax > 0f)
            {
                bool isBasic = (skills != null && skills.Count > 0 && activeCastSkill == skills[0]);
                float basicExtra = isBasic ? basicRangeBonus : 0f;
                arrowMaxDist = activeCastSkill.shape.rangeMax * rangeMul + rangeBonus + basicExtra;
            }
            else
            {
                arrowMaxDist = mainDist;
            }

            foreach (var dir in dirs)
            {
                // 飛行終点を決定:
                //   貫通: 終点 = self + dir * rangeMax (ターゲットも貫いて最大射程まで飛ぶ)
                //   通常: 終点 = 直線上で最初に当たる敵の位置、なければ self + dir * mainDist
                Vector3 ideal = origPos + dir * arrowMaxDist;
                Vector3 endpoint = ideal;
                if (!usePierce)
                {
                    var firstEnemy = FindFirstEnemyInArrowPath(origPos, dir, mainDist);
                    if (firstEnemy != null)
                        endpoint = firstEnemy.transform.position;
                }

                // 矢複製
                var copy = Instantiate(orig, origPos, Quaternion.LookRotation(dir, Vector3.up), null);
                copy.transform.localScale = orig.transform.lossyScale;
                copy.SetActive(true);

                if (manager != null)
                {
                    float endDist = Vector3.Distance(origPos, endpoint);
                    manager.BattleLog.Add(
                        $"[{BT:F2}s] VFX spawn: kind=Arrow prefab={orig.name} by={displayName} " +
                        $"pos=({origPos.x:F2},{origPos.z:F2}) dir=({dir.x:F2},{dir.z:F2}) " +
                        $"endpoint=({endpoint.x:F2},{endpoint.z:F2}) dist={endDist:F2}m pierce={usePierce}");
                }
                var drv = copy.GetComponent<AttachmentCurveDriver>();
                if (drv != null) Destroy(drv);
                var rotComp = copy.GetComponent<AttachmentRotator>();
                if (rotComp != null) Destroy(rotComp);

                // 発光
                var lightGo = new GameObject("ArrowLight");
                lightGo.transform.SetParent(copy.transform, false);
                lightGo.transform.localPosition = Vector3.zero;
                var lt = lightGo.AddComponent<Light>();
                lt.type = LightType.Point;
                lt.color = new Color(1f, 0.85f, 0.4f);
                lt.intensity = 2f;
                lt.range = 3f;

                // 貫通射撃 VFX: 矢に親子で貼る → 矢と一緒に Destroy
                if (usePierce && pierceShotVfxPrefab != null)
                {
                    var vfx = Instantiate(pierceShotVfxPrefab, copy.transform);
                    vfx.transform.localPosition = Vector3.zero;
                    vfx.transform.localRotation = Quaternion.Euler(pierceShotVfxLocalEuler);
                    if (pierceShotVfxScale > 0f && Mathf.Abs(pierceShotVfxScale - 1f) > 0.001f)
                        vfx.transform.localScale *= pierceShotVfxScale;

                    if (manager != null)
                    {
                        Vector3 fs = vfx.transform.lossyScale;
                        manager.BattleLog.Add(
                            $"[{BT:F2}s] VFX spawn: kind=PierceShot prefab={pierceShotVfxPrefab.name} " +
                            $"by={displayName} attach=arrow scale=({fs.x:F2},{fs.y:F2},{fs.z:F2})");
                    }
                }

                // 飛翔
                Quaternion aimRot = Quaternion.LookRotation(dir, Vector3.up);
                if (activeCastSkill != null)
                    aimRot *= Quaternion.Euler(activeCastSkill.handAttachmentEulerOffset);
                float dist = Vector3.Distance(origPos, endpoint);
                float flyTime = Mathf.Max(0.05f, dist / spd);
                float rotBlend = activeCastSkill != null ? activeCastSkill.flyRotationBlendTime : 0.2f;

                var moveT = DOTween.To(() => copy.transform.position,
                        x => copy.transform.position = x,
                        endpoint, flyTime)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => { if (copy != null) Destroy(copy); });
                moveT.timeScale = spdMul;
                if (rotBlend > 0.001f)
                {
                    var rotT = copy.transform.DORotateQuaternion(aimRot, Mathf.Min(flyTime, rotBlend)).SetEase(Ease.OutQuad);
                    rotT.timeScale = spdMul;
                }
                else
                {
                    copy.transform.rotation = aimRot;
                }
            }
        }

        /// <summary>射撃経路 (origPos → aimPos) と交差するバリケードの中で、最も caster に近いものを返す。
        /// 非貫通矢の遮蔽判定用。敵側のバリケード or 自陣のバリケード関係なく、
        /// 経路上に物理的に存在するなら blocker 扱い。</summary>
        private Barricade FindBarricadeInArrowPath(Vector3 origPos, Vector3 dir, Vector3 aimPos)
        {
            if (manager?.barricades == null || manager.barricades.Count == 0) return null;
            origPos.y = 0;
            aimPos.y = 0;
            Vector3 d = dir; d.y = 0;
            if (d.sqrMagnitude < 1e-6f) return null;
            d.Normalize();
            float maxDist = Vector3.Distance(origPos, aimPos);

            Barricade best = null;
            float bestT = float.MaxValue;
            const float lineHalfWidth = 0.4f;  // 矢の判定幅
            foreach (var b in manager.barricades)
            {
                if (b == null || !b.IsActive) continue;
                Vector3 bcenter = b.center; bcenter.y = 0;
                Vector3 rel = bcenter - origPos;
                float along = Vector3.Dot(rel, d);
                if (along < 0.05f || along > maxDist + 0.5f) continue;
                Vector3 closestOnLine = origPos + d * along;
                float distToWall = b.DistanceTo(closestOnLine);
                if (distToWall > lineHalfWidth) continue;
                if (along < bestT)
                {
                    bestT = along;
                    best = b;
                }
            }
            return best;
        }

        /// <summary>貫通射撃用: caster の位置から dir 方向に maxDist まで、 perp 半幅 halfWidth 以内の
        /// 全敵を集める。 shape.rangeMax と同じ距離を渡せば「最大射程まで貫く」挙動になる。
        /// 槍の Line shape と同等の判定だが、 caller (CastSkillByIndex の usePierce 分岐) で
        /// shape を無視してこちらを使うことで、 Single shape の射撃武器に貫通を後付けできる。</summary>
        private List<RealtimeBattleUnit> CollectEnemiesInPierceLine(Vector3 origPos, Vector3 dir, float maxDist, float halfWidth)
        {
            var result = new List<RealtimeBattleUnit>();
            if (manager == null) return result;
            Vector3 dn = dir; dn.y = 0f;
            if (dn.sqrMagnitude < 1e-6f) return result;
            dn.Normalize();
            origPos.y = 0f;
            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                if (u.ownerSide == ownerSide) continue;  // 敵のみ
                if (u == this) continue;
                Vector3 rel = u.transform.position - origPos; rel.y = 0f;
                float along = Vector3.Dot(rel, dn);
                if (along < 0.1f || along > maxDist) continue;
                float perp = (rel - dn * along).magnitude;
                if (perp > halfWidth) continue;
                result.Add(u);
            }
            return result;
        }

        /// <summary>caster (origPos) から dir 方向に maxDist までの間で最初に当たる敵を返す。
        /// 矢の通常着弾位置決定用 (vis のみ)。線幅は約 0.5m。</summary>
        private RealtimeBattleUnit FindFirstEnemyInArrowPath(Vector3 origPos, Vector3 dir, float maxDist)
        {
            if (manager == null) return null;
            origPos.y = 0;
            RealtimeBattleUnit best = null;
            float bestT = float.MaxValue;
            const float lineHalfWidth = 0.4f;
            foreach (var u in manager.AllUnits)
            {
                if (u == null || u == this || !u.IsAlive() || u.ownerSide == ownerSide) continue;
                Vector3 rel = u.transform.position - origPos; rel.y = 0;
                float along = Vector3.Dot(rel, dir);
                if (along < 0.1f || along > maxDist + 0.5f) continue;
                Vector3 perpVec = rel - dir * along;
                if (perpVec.magnitude > lineHalfWidth) continue;
                if (along < bestT) { bestT = along; best = u; }
            }
            return best;
        }

        /// <summary>保留中イベントを時刻到達で発火</summary>
        private void ProcessPendingEvents()
        {
            for (int i = pendingEvents.Count - 1; i >= 0; i--)
            {
                var pe = pendingEvents[i];
                if (BT >= pe.fireTime)
                {
                    FireEvent(pe);
                    pendingEvents.RemoveAt(i);
                }
            }
        }

        /// <summary>SubSkill の DealDamage 時に専用挙動 (AoE / Pull / Knockback) を発動。
        /// SO で表現できない radius 等は enum 値 ごとに hardcode。 ダメ計算は subSkill.powerMul を AT 倍率として使用。</summary>
        private void ApplySubSkillBehavior(RealtimeSkillDefinition subSkill, RealtimeBattleUnit tgt, float extraMul = 1f)
        {
            if (subSkill == null || tgt == null || manager == null || unit == null) return;
            // damageKind に応じて baseStat 切替: Magical/PenetrateMagical なら MAT、 それ以外 (Physical 等) は AT
            bool isMagical = subSkill.damageKind == SkillDamageKind.Magical
                          || subSkill.damageKind == SkillDamageKind.PenetrateMagical;
            int baseStat = isMagical ? unit.MatFinal : unit.AtFinal;
            int dmg = Mathf.Max(1, Mathf.RoundToInt(baseStat * subSkill.powerMul * extraMul));

            switch (subSkill.subSkillBehavior)
            {
                case SubSkillBehavior.AoeAroundTarget:
                {
                    float radius = 2.0f;
                    foreach (var u in manager.AllUnits)
                    {
                        if (u == null || !u.IsAlive() || u.ownerSide == ownerSide) continue;
                        if (Vector3.Distance(u.transform.position, tgt.transform.position) > radius) continue;
                        int eachDmg = dmg;
                        ProcessAutoRevive(u, ref eachDmg);
                        ProcessSurviveLethal(u, ref eachDmg);
                        int before = u.unit.CurrentHp;
                        u.unit.TakeDamage(eachDmg);
                        u.OnHit(u.unit.IsDead, this);
                        manager.OnAttack(this, u, eachDmg, before, u.unit.CurrentHp);
                        SpawnDamagePopup(u.transform.position, eachDmg, DamagePopupSpawner.PopupKind.Damage);
                        // ノックバック: subSkill.knockbackMass > 0 ならキャスター中心の放射方向 (= ApplyKnockback 既定挙動)
                        if (subSkill.knockbackMass > 0f && u.IsAlive())
                        {
                            ApplyKnockback(u, subSkill.knockbackMass);
                        }
                    }
                    break;
                }
                case SubSkillBehavior.PullEnemies:
                {
                    // 自中心 半径 4m 以内の敵全員を powerMul (m) 自分方向へ引き寄せ
                    // (戦士は近接 → 単体引き寄せは意味薄いので、 周囲全員をまとめて寄せる)
                    float pullRadius = 4f;
                    float pullDist = Mathf.Max(0.5f, subSkill.powerMul);
                    foreach (var u in manager.AllUnits)
                    {
                        if (u == null || !u.IsAlive() || u.ownerSide == ownerSide) continue;
                        Vector3 toMe = transform.position - u.transform.position;
                        toMe.y = 0;
                        float dist = toMe.magnitude;
                        if (dist > pullRadius || dist <= 0.01f) continue;
                        Vector3 dir = toMe / dist;
                        MovePreventOverlap(u, dir, pullDist);
                    }
                    break;
                }
                case SubSkillBehavior.KnockbackAoE:
                {
                    // 自前方 2x2 (2m 幅 × 2m 奥行) の正方形 AoE 内の敵を BOX forward 方向に powerMul (m) 押し戻す。
                    // 範囲ごとずらす (玉突き式): 遠い敵から先に押す → 手前の敵の進行先が空くので
                    // 全員まるごとノックバックされる。 BOX 後方の非対象 enemy が居ると、
                    // 衝突回避で押し戻し量が減る (それは仕様通り)。
                    Vector3 forward = tgt.transform.position - transform.position;
                    forward.y = 0;
                    if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
                    forward.Normalize();
                    const float halfWidth = 1.0f; // 2m 幅 → 左右 1.0m
                    const float depth = 2f;       // 2m 奥行
                    float kbDist = Mathf.Max(0.5f, subSkill.powerMul);

                    // 1) BOX 内の対象を収集 (along 距離付き)
                    var boxTargets = new List<(RealtimeBattleUnit u, float along)>();
                    foreach (var u in manager.AllUnits)
                    {
                        if (u == null || !u.IsAlive() || u.ownerSide == ownerSide) continue;
                        Vector3 delta = u.transform.position - transform.position;
                        delta.y = 0;
                        float along = Vector3.Dot(delta, forward);
                        if (along < 0f || along > depth) continue;
                        Vector3 side = delta - forward * along;
                        if (side.magnitude > halfWidth) continue;
                        boxTargets.Add((u, along));
                    }
                    // 2) 遠い敵から先に押す (along 降順) — 玉突き挙動の擬似実装
                    boxTargets.Sort((a, b) => b.along.CompareTo(a.along));

                    // BOX 内対象同士は擦り抜ける (一緒に動く想定)。 BOX 外の他 unit (BOX 後方の敵 / 自軍味方) は障害として残る。
                    var ignoreSet = new HashSet<RealtimeBattleUnit>();
                    foreach (var (u, _) in boxTargets) ignoreSet.Add(u);

                    // 3) 全員 BOX forward 方向に同じ距離押す + OnDisrupted (行動妨害共通仕様)
                    foreach (var (u, _) in boxTargets)
                    {
                        MovePreventOverlap(u, forward, kbDist, this, ignoreSet);
                        float lockoutSec = u.ComputeKnockbackLockout();
                        u.OnDisrupted(lockoutSec);
                    }
                    break;
                }
            }
        }

        /// <summary>サブスキル SO を発動: anim を再生し、 timeline の各イベントを時刻指定でスケジュール。
        /// ConditionalCast (timeline 上の条件分岐) から呼ばれる。</summary>
        private void CastSubSkill(RealtimeSkillDefinition subSkill, RealtimeBattleUnit tgt)
        {
            if (subSkill == null || tgt == null || !tgt.IsAlive() || tgt.unit == null || unit == null) return;

            // anim 再生 (subSkill の animTriggerName / animStateName を使用)
            anim?.PlayAttackForSkill(subSkill);
            // anim 中の移動/別 cast を抑止
            float animDur = subSkill.castAnimSec > 0f ? subSkill.castAnimSec : 0.5f;
            attackingUntil = Mathf.Max(attackingUntil, BT + animDur);
            BeginCastInfo(subSkill, -1); // 行動妨害共通仕様 (sub-skill は CT entry なし)

            // timeline をスケジュール (DealDamage / VFX / PlaySound 等は通常経路で発火)
            if (subSkill.timeline != null && subSkill.timeline.Count > 0)
            {
                foreach (var ev in subSkill.timeline)
                {
                    if (ev == null) continue;
                    pendingEvents.Add(new PendingSkillEvent
                    {
                        fireTime = BT + Mathf.Max(0f, ev.timeSec),
                        ev = ev,
                        skill = subSkill,
                        primaryTarget = tgt,
                        hitTargets = new List<RealtimeBattleUnit> { tgt },
                        castOrigin = transform.position,
                        castAim = tgt.transform.position,
                        castDir = (tgt.transform.position - transform.position).normalized,
                        damageMul = 1f,
                    });
                }
            }
            else
            {
                // timeline 無しなら anim 表示のみ + ダメは標準計算で即時 1 ヒット
                ApplySkillEffect(subSkill, tgt, 1f);
            }
        }

        /// <summary>ConditionalCast の発動条件を判定。 true なら CastSubSkill を呼ぶ。
        /// branchRequiredPassive が指定されてれば unit.activePassives 内に存在することを要求。
        /// 加えて branchProcChance のロールが通れば発動。</summary>
        private void EvaluateConditionalCast(RealtimeSkillEvent ev, RealtimeBattleUnit tgt)
        {
            if (ev == null || ev.branchSubSkill == null || tgt == null)
            {
                manager?.BattleLog.Add($"[{BT:F2}s] ConditionalCast skip: ev/subSkill/tgt null on {displayName}");
                return;
            }
            // 必須パッシブ: 指定されてれば activePassives 内に存在すること
            if (ev.branchRequiredPassive != null)
            {
                if (activePassives == null || !activePassives.Contains(ev.branchRequiredPassive))
                {
                    manager?.BattleLog.Add($"[{BT:F2}s] ConditionalCast skip: passive '{ev.branchRequiredPassive.displayName}' not in activePassives on {displayName}");
                    return;
                }
            }
            // 発動条件: branchTriggerInterval > 0 なら window-random (N 攻撃中 1 度発動、 周期最後まで進める)
            if (ev.branchTriggerInterval > 0)
            {
                int n = ev.branchTriggerInterval;
                if (!_conditionalCastWindowStates.TryGetValue(ev, out var ws))
                {
                    ws = new WindowState();
                    _conditionalCastWindowStates[ev] = ws;
                }
                if (ws.fireAtOffset == 0)
                    ws.fireAtOffset = UnityEngine.Random.Range(1, n + 1);
                ws.attackCount++;
                bool willFire = !ws.fired && ws.attackCount == ws.fireAtOffset;
                if (willFire) ws.fired = true;
                // 周期完了 → 次の周期へ
                if (ws.attackCount >= n)
                {
                    ws.attackCount = 0;
                    ws.fireAtOffset = UnityEngine.Random.Range(1, n + 1);
                    ws.fired = false;
                }
                if (!willFire)
                {
                    manager?.BattleLog.Add($"[{BT:F2}s] ConditionalCast skip: window (offset={ws.attackCount}, fireAt={ws.fireAtOffset}, N={n}) on {displayName}");
                    return;
                }
            }
            else if (ev.branchProcChance < 1f && UnityEngine.Random.value >= ev.branchProcChance)
            {
                manager?.BattleLog.Add($"[{BT:F2}s] ConditionalCast skip: procChance roll fail ({ev.branchProcChance:F2}) on {displayName}");
                return;
            }
            manager?.BattleLog.Add($"[{BT:F2}s] ConditionalCast fire: SubSkill={ev.branchSubSkill.skillName} on {displayName}");
            CastSubSkill(ev.branchSubSkill, tgt);
        }

        private void FireEvent(PendingSkillEvent pe)
        {
            // 行動妨害共通仕様: 進行中SOの DealDamage 発火を記録 (CT判定に使う)
            if (pe.ev != null
                && pe.ev.kind == RealtimeSkillEventKind.DealDamage
                && currentCast.valid
                && pe.skill == currentCast.skill)
            {
                currentCast.hasFiredDealDamage = true;
            }

            switch (pe.ev.kind)
            {
                case RealtimeSkillEventKind.DealDamage:
                    // 近接 Slash 等で barricade ターゲット直接指定の場合 (ApplyFixedHit のみ、status/knockback 無視)
                    if (pe.barricadeTarget != null)
                    {
                        if (pe.barricadeTarget.IsActive)
                        {
                            int b4 = pe.barricadeTarget.hp;
                            int dlt = pe.barricadeTarget.ApplyFixedHit();
                            manager?.OnAttackBarricade(this, pe.barricadeTarget, dlt, b4, pe.barricadeTarget.hp);
                        }
                        return;
                    }
                    if (pe.hitTargets == null) return;
                    {
                        // basic attack 発射時は MultiShot (日本刀=2 / トリデントアーチャー=3 等) を適用
                        bool isBasicCast = skills != null && skills.Count > 0 && pe.skill == skills[0];
                        int hitCount = isBasicCast ? Mathf.Max(1, multiShotCount) : 1;
                        float dmgMul = pe.damageMul > 0f ? pe.damageMul : 1f;

                        // 非貫通の射撃 (FirstEnemyInLine + pierce 不在) で経路にバリケードがあれば、
                        // 矢はバリケードに当たって停止 (ApplyFixedHit 1ダメ)
                        bool isProjectileLine = pe.skill != null
                            && pe.skill.projectileHitMode == ProjectileHitMode.FirstEnemyInLine;
                        bool blockedByBarricade = false;
                        if (isProjectileLine && !hasPierceShot && manager?.barricades != null)
                        {
                            var blocker = FindBarricadeInArrowPath(pe.castOrigin, pe.castDir, pe.castAim);
                            if (blocker != null)
                            {
                                int before = blocker.hp;
                                int dealt = blocker.ApplyFixedHit();
                                manager?.OnAttackBarricade(this, blocker, dealt, before, blocker.hp);
                                blockedByBarricade = true;
                            }
                        }

                        if (!blockedByBarricade)
                        {
                            foreach (var tgt in pe.hitTargets)
                            {
                                if (tgt == null || !tgt.IsAlive()) continue;
                                // SubSkill 専用挙動 (AoE / Pull / Knockback) があれば標準ダメの代わりに発動
                                if (pe.skill != null && pe.skill.subSkillBehavior != SubSkillBehavior.None)
                                {
                                    ApplySubSkillBehavior(pe.skill, tgt, dmgMul);
                                    continue;
                                }
                                for (int shot = 0; shot < hitCount && tgt.IsAlive(); shot++)
                                {
                                    ApplySkillEffect(pe.skill, tgt, dmgMul);
                                }
                            }
                        }
                    }
                    break;

                case RealtimeSkillEventKind.Effect:
                    if (pe.ev.effectPrefab != null)
                    {
                        Quaternion rot = (pe.castDir.sqrMagnitude > 0.01f)
                            ? Quaternion.LookRotation(pe.castDir, Vector3.up)
                            : Quaternion.identity;
                        GameObject go;
                        string vfxAttachInfo;
                        // ボーン子化する場合、親の座標系でオフセットを掛ける
                        if (!string.IsNullOrEmpty(pe.ev.attachBoneName))
                        {
                            // requireBone: 厳密検索、fallback 無し。見つからなければ skip。
                            Transform boneTf;
                            if (pe.ev.requireBone)
                            {
                                boneTf = FindChildByName(transform, pe.ev.attachBoneName);
                                if (boneTf == null) break; // 装備武器に TrailTip 等が無い → Effect 不発
                            }
                            else
                            {
                                boneTf = ResolveAttachTransform(pe.ev.attachBoneName);
                            }
                            if (boneTf != null)
                            {
                                go = Instantiate(pe.ev.effectPrefab, boneTf);
                                go.transform.localPosition = pe.ev.offset;
                                go.transform.localRotation = Quaternion.identity;
                                vfxAttachInfo = $"bone:{pe.ev.attachBoneName}";
                            }
                            else
                            {
                                Debug.LogWarning($"[RealtimeSkill] attachBoneName '{pe.ev.attachBoneName}' not found on {name} (even via Humanoid fallback). Spawning in world.");
                                Vector3 wp = ResolveEffectPosition(pe);
                                go = Instantiate(pe.ev.effectPrefab, wp, rot);
                                vfxAttachInfo = $"world(fallback:{pe.ev.attachBoneName})";
                            }
                        }
                        else
                        {
                            Vector3 wp = ResolveEffectPosition(pe);
                            go = Instantiate(pe.ev.effectPrefab, wp, rot);
                            vfxAttachInfo = "world";
                        }
                        // 倍速適用: 生成したエフェクト配下の Particle/Animator を mul で加速
                        ApplyBattleSpeedToEffect(go);
                        // 通常攻撃 (skills[0]) の VFX は basicShapeMul で拡大 (太陽の膨張 1.5 倍)
                        if (pe.skill != null && skills != null && skills.Count > 0 && pe.skill == skills[0]
                            && Mathf.Abs(basicShapeMul - 1f) > 0.001f)
                        {
                            go.transform.localScale = go.transform.localScale * basicShapeMul;
                        }
                        // 紅蓮の大地 (shapeSizeBonus > 0): 直撃/範囲型 (Line/Fan/Diamond/Square/CircleAtSelf/CrossAtSelf) は
                        // shape が拡大される → VFX も同比率でスケール。 着弾型 (Single/CircleAtTarget) は射程だけ伸びる
                        // (rangeBonus 経由) ので VFX 据え置き。
                        if (pe.skill?.shape != null && shapeSizeBonus > 0.01f)
                        {
                            var sh = pe.skill.shape;
                            bool isProjectileLand = sh.shape == RealtimeTargetShape.Single
                                                 || sh.shape == RealtimeTargetShape.CircleAtTarget;
                            if (!isProjectileLand)
                            {
                                float orig = Mathf.Max(sh.lengthMass, sh.widthMass);
                                if (orig > 0.01f)
                                {
                                    float ratio = (orig + shapeSizeBonus) / orig;
                                    go.transform.localScale = go.transform.localScale * ratio;
                                }
                            }
                        }
                        // lifeSec も倍速で縮める (見た目と破棄タイミングを揃える)
                        if (pe.ev.lifeSec > 0f)
                        {
                            float mul = Mathf.Max(0.01f, RealtimeBattleManager.GlobalSpeed);
                            Destroy(go, pe.ev.lifeSec / mul);
                        }

                        // Projectile: skill.projectileSpeed > 0 のとき、Projectile 持ちなら速度同期、
                        // 無ければ自動で AddComponent して飛翔体化する (Rock 等の岩弾用)。
                        if (pe.skill != null && pe.skill.projectileSpeed > 0f)
                        {
                            var proj = go.GetComponent<Projectile>();
                            if (proj == null) proj = go.AddComponent<Projectile>();
                            proj.speed = pe.skill.projectileSpeed;
                            if (pe.ev.lifeSec > 0f) proj.lifeSec = pe.ev.lifeSec;
                            // FirstEnemyInLine: 最初に当たる敵の位置で停止 (矢と同じ動き)
                            if (pe.skill.projectileHitMode == ProjectileHitMode.FirstEnemyInLine
                                && pe.hitTargets != null && pe.hitTargets.Count > 0
                                && pe.hitTargets[0] != null)
                            {
                                Vector3 spawnPos = go.transform.position;
                                Vector3 targetPos = pe.hitTargets[0].transform.position;
                                Vector3 horiz = targetPos - spawnPos; horiz.y = 0f;
                                proj.maxDistance = Mathf.Max(0.1f, horiz.magnitude);
                            }
                        }

                        if (manager != null && go != null)
                        {
                            Vector3 fp = go.transform.position;
                            Vector3 fs = go.transform.localScale;
                            float ry = go.transform.rotation.eulerAngles.y;
                            string skName = pe.skill != null ? pe.skill.skillName : "?";
                            manager.BattleLog.Add(
                                $"[{BT:F2}s] VFX spawn: skill={skName} prefab={pe.ev.effectPrefab.name} " +
                                $"by={displayName} attach={vfxAttachInfo} pos=({fp.x:F2},{fp.z:F2}) " +
                                $"rot_y={ry:F0}° scale=({fs.x:F2},{fs.y:F2},{fs.z:F2})");
                        }
                    }
                    break;

                case RealtimeSkillEventKind.PlaySound:
                    if (pe.ev.sound != null)
                    {
                        AudioSource.PlayClipAtPoint(pe.ev.sound, transform.position);
                    }
                    break;

                case RealtimeSkillEventKind.UnequipWeapon:
                    HideEquippedWeapons();
                    break;

                case RealtimeSkillEventKind.ConditionalCast:
                    {
                        var castTgt = (pe.hitTargets != null && pe.hitTargets.Count > 0)
                            ? pe.hitTargets[0]
                            : pe.primaryTarget;
                        EvaluateConditionalCast(pe.ev, castTgt);
                    }
                    break;
            }
        }

        // 装備武器 (Weapon_Root_R/L 配下の EquippedWeapon_*) を非表示にする
        private void HideEquippedWeapons()
        {
            var rootR = FindChildByName(transform, "Weapon_Root_R");
            var rootL = FindChildByName(transform, "Weapon_Root_L");
            for (int side = 0; side < 2; side++)
            {
                var root = side == 0 ? rootR : rootL;
                if (root == null) continue;
                for (int i = 0; i < root.childCount; i++)
                {
                    var child = root.GetChild(i);
                    if (!child.gameObject.activeSelf) continue;
                    if (child.name.StartsWith("EquippedWeapon_"))
                    {
                        child.gameObject.SetActive(false);
                        hiddenWeapons.Add(child.gameObject);
                    }
                }
            }
        }

        // 一時非表示にした武器を復帰 (cast 終了時に呼ばれる)
        private void RestoreHiddenWeapons()
        {
            for (int i = 0; i < hiddenWeapons.Count; i++)
            {
                var go = hiddenWeapons[i];
                if (go != null) go.SetActive(true);
            }
            hiddenWeapons.Clear();
        }

        /// <summary>生成直後のエフェクト (ParticleSystem / Animator / DOTween) に戦闘速度倍率を反映。</summary>
        private static void ApplyBattleSpeedToEffect(GameObject go)
        {
            if (go == null) return;
            float mul = Mathf.Max(0.01f, RealtimeBattleManager.GlobalSpeed);
            if (mul <= 0f || Mathf.Approximately(mul, 1f)) return;
            var particles = go.GetComponentsInChildren<ParticleSystem>(true);
            for (int i = 0; i < particles.Length; i++)
            {
                var main = particles[i].main;
                main.simulationSpeed *= mul;
            }
            var animators = go.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].speed *= mul;
            }
        }

        /// <summary>指定名の子 Transform を再帰検索。多段 fallback 付き。
        /// Spear_Root → Spear_Root(Human) → Weapon_Root_R → Humanoid RightHand
        /// Bow_Root / Sheild_Root → Weapon_Root_L → Humanoid LeftHand</summary>
        private Transform ResolveAttachTransform(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var direct = FindChildByName(transform, name);
            if (direct != null) return direct;

            // 論理名別 fallback
            switch (name)
            {
                case "Spear_Root":
                {
                    var alt = FindChildByName(transform, "Spear_Root(Human)");
                    if (alt != null) return alt;
                    break;
                }
                case "Bow_Root":
                {
                    // 人型は Weapon_Root_L 直下で扱うのでフォールバック
                    break;
                }
                case "Sheild_Root":
                    // 見つからない場合は Weapon_Root_L に fallback
                    break;
            }

            // 左右ヒント判定
            string nameLower = name.ToLowerInvariant();
            bool leftHint = nameLower.Contains("left")
                         || nameLower.StartsWith("l_")
                         || nameLower.Contains("bow")
                         || nameLower.Contains("sheild")
                         || nameLower.Contains("shield");

            var weaponRoot = FindChildByName(transform, leftHint ? "Weapon_Root_L" : "Weapon_Root_R");
            if (weaponRoot != null) return weaponRoot;

            // Humanoid フォールバック
            var an = GetComponentInChildren<Animator>();
            if (an != null && an.avatar != null && an.avatar.isHuman)
            {
                return leftHint
                    ? an.GetBoneTransform(HumanBodyBones.LeftHand)
                    : an.GetBoneTransform(HumanBodyBones.RightHand);
            }
            return null;
        }

        /// <summary>指定名の空 GameObject を Weapon_Root_R (なければ右手 bone) 直下に生成</summary>
        private Transform CreateAttachPivot(string pivotName)
        {
            // Weapon_Root_R (or L) を優先
            var parent = FindChildByName(transform, "Weapon_Root_R");
            if (parent == null) parent = FindChildByName(transform, "Weapon_Root_L");
            if (parent == null)
            {
                // Humanoid フォールバック
                var an = GetComponentInChildren<Animator>();
                if (an != null && an.avatar != null && an.avatar.isHuman)
                {
                    string nl = pivotName.ToLowerInvariant();
                    parent = (nl.Contains("left") || nl.StartsWith("l_"))
                        ? an.GetBoneTransform(HumanBodyBones.LeftHand)
                        : an.GetBoneTransform(HumanBodyBones.RightHand);
                }
            }
            if (parent == null) return null;
            var go = new GameObject(pivotName);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        /// <summary>指定名の子 Transform を再帰検索</summary>
        private static Transform FindChildByName(Transform root, string name)
        {
            if (root == null) return null;
            if (root.name == name) return root;
            for (int i = 0; i < root.childCount; i++)
            {
                var r = FindChildByName(root.GetChild(i), name);
                if (r != null) return r;
            }
            return null;
        }

        private Vector3 ResolveEffectPosition(PendingSkillEvent pe)
        {
            Vector3 basePos;
            switch (pe.ev.spawnOrigin)
            {
                case RealtimeEffectOrigin.Self:
                    basePos = pe.castOrigin;
                    break;
                case RealtimeEffectOrigin.SelfForward:
                    basePos = pe.castOrigin + pe.castDir * (pe.skill.shape.lengthMass * 0.5f);
                    break;
                case RealtimeEffectOrigin.Target:
                    basePos = (pe.primaryTarget != null && pe.primaryTarget.IsAlive())
                        ? pe.primaryTarget.transform.position : pe.castAim;
                    break;
                case RealtimeEffectOrigin.ImpactPoint:
                    basePos = pe.castOrigin + pe.castDir * pe.skill.shape.rangeMax;
                    break;
                case RealtimeEffectOrigin.CasterHand:
                    // TODO: 手ボーン探索。当面は Self + 少し前・上
                    basePos = pe.castOrigin + pe.castDir * 0.3f + Vector3.up * 1.0f;
                    break;
                default:
                    basePos = pe.castOrigin;
                    break;
            }
            // offset はローカル軸 (x=右, y=上, z=前) で加算
            Vector3 right = Vector3.Cross(Vector3.up, pe.castDir).normalized;
            return basePos + right * pe.ev.offset.x + Vector3.up * pe.ev.offset.y + pe.castDir * pe.ev.offset.z;
        }

        private void ApplySkillEffect(RealtimeSkillDefinition s, RealtimeBattleUnit tgt, float extraMul = 1f)
        {
            if (tgt == null || tgt.unit == null || unit == null) return;

            switch (s.skillType)
            {
                case RealtimeSkillType.Attack:
                    {
                        int at = unit.AtFinal;
                        int df = tgt.unit.DfFinal;
                        bool isBasicHere = (skills != null && skills.Count > 0 && s == skills[0]);
                        // Basic 攻撃の属性 override (ブリザードロッド 等)
                        SkillDamageKind effKind = s.damageKind;
                        if (isBasicHere && basicKindOverrideValid) effKind = basicKindOverride;
                        int raw;
                        // Physical/Magical は平方根スケールで両端を抑制:
                        // - 弱い側でも 0 ダメにならない (sqrt(AT) が緩やかに減少)
                        // - 強い側でも線形より抑えめ (sqrt が逓減)
                        // 係数: AtScale=5, DfScale=2 で「中庸 60 vs 60 ≈ 23 ダメ」を狙い値とする
                        const float AtScale = 5.0f;
                        const float DfScale = 2.0f;
                        switch (effKind)
                        {
                            case SkillDamageKind.Fixed:
                                raw = Mathf.RoundToInt(s.powerMul);
                                break;
                            case SkillDamageKind.MaxHpRate:
                                raw = Mathf.RoundToInt(tgt.unit.MaxHp * s.powerMul);
                                break;
                            case SkillDamageKind.Magical:
                            case SkillDamageKind.PenetrateMagical:
                                raw = Mathf.RoundToInt(
                                    Mathf.Sqrt(Mathf.Max(0f, unit.MatFinal * s.powerMul)) * AtScale
                                    - Mathf.Sqrt(Mathf.Max(0f, (float)tgt.unit.MdfFinal)) * DfScale);
                                break;
                            default: // Physical, PenetratePhysical
                                raw = Mathf.RoundToInt(
                                    Mathf.Sqrt(Mathf.Max(0f, at * s.powerMul)) * AtScale
                                    - Mathf.Sqrt(Mathf.Max(0f, (float)df)) * DfScale);
                                break;
                        }
                        // BasicAttackMul / SkillMul を適用
                        float mul = isBasicHere ? basicAttackMul : skillMul;
                        // Charge 効果: 次の通常攻撃のみ 1.5x (1回消費)
                        if (isBasicHere && chargeActive)
                        {
                            mul *= 1.5f;
                            chargeActive = false;
                            manager?.BattleLog.Add($"[{BT:F2}s] {displayName} Charge 消費!");
                        }
                        mul *= extraMul; // トライアローの側矢 0.9x など
                        raw = Mathf.RoundToInt(raw * mul);
                        int dmg = Mathf.Max(1, raw);

                        // === ガード判定 (騎士 / 盾を装備している時のみ) ===
                        bool guarded = tgt.TryRollGuard(this, out float mitigation);
                        if (guarded && mitigation > 0f)
                        {
                            dmg = Mathf.Max(1, Mathf.RoundToInt(dmg * (1f - mitigation)));
                        }
                        // NextHitDamageReduction (受け流し等のパッシブ効果)
                        if (tgt._nextHitDamageReductionRatio > 0f)
                        {
                            dmg = Mathf.Max(1, Mathf.RoundToInt(dmg * (1f - tgt._nextHitDamageReductionRatio)));
                            tgt._nextHitDamageReductionRatio = 0f; // 1 回限りで消費
                        }

                        int before = tgt.unit.CurrentHp;
                        ProcessAutoRevive(tgt, ref dmg);
                        ProcessSurviveLethal(tgt, ref dmg);
                        tgt.unit.TakeDamage(dmg);
                        tgt.OnHit(tgt.unit.IsDead);
                        manager?.OnAttack(this, tgt, dmg, before, tgt.unit.CurrentHp);
                        // ダメージ数値ポップアップ
                        var kind = (s.damageKind == SkillDamageKind.Magical || s.damageKind == SkillDamageKind.PenetrateMagical)
                            ? DamagePopupSpawner.PopupKind.Magic
                            : DamagePopupSpawner.PopupKind.Damage;
                        SpawnDamagePopup(tgt.transform.position, dmg, kind);

                        // 戦士 rank3+ 自動追撃は通常攻撃 SO の timeline に ConditionalCast
                        // (HasExtraAttack 条件) を置いて発動する形式に統一済 (Slash.timeline 参照)。
                        // 旧 isExtraAttackContinuation 経由のスケジュールは廃止。

                        // ランサー rank 5 ひるみ突き: 通常攻撃ヒット時に flinch
                        if (isBasicHere && hasFlinchBasic && tgt.IsAlive() && tgt != this)
                        {
                            tgt.ApplyFlinch();
                        }

                        // ===== Passive トリガ =====
                        // 攻撃者側: OnDealDamage + basic の時のみ N 回目 + 連続命中
                        bool isBasic = (skills != null && skills.Count > 0 && s == skills[0]);
                        if (isBasic) _basicAttackHitCount++;
                        if (_lastHitTarget == tgt) _sameTargetHitStreak++;
                        else { _lastHitTarget = tgt; _sameTargetHitStreak = 1; }

                        // 与ダメ % 系 passive (LifestealPercent / HealAlliesOnHitPercent) のため
                        // 実際に通った値 (= before - after、 overkill 除外) をセットする。
                        _lastDealtDamage = before - tgt.unit.CurrentHp;
                        EvaluatePassives(PassiveTrigger.OnDealDamage, ctxTarget: tgt);
                        if (isBasic)
                            EvaluatePassives(PassiveTrigger.OnNthBasicAttackHit, ctxTarget: tgt);
                        EvaluatePassives(PassiveTrigger.OnConsecutiveHitSameTarget, ctxTarget: tgt);

                        // OnDamagedThenNextHit (自分が攻撃した側なので、攻撃者=自分の被弾後フラグ)
                        if (_damagedPendingCounter > 0)
                        {
                            EvaluatePassives(PassiveTrigger.OnDamagedThenNextHit, ctxTarget: tgt);
                            _damagedPendingCounter--;
                        }

                        // 被害者側: OnDamaged + OnSelfHpBelowPercent
                        tgt.OnDamagedByPassive(this);

                        if (tgt.unit.IsDead)
                        {
                            EvaluatePassives(PassiveTrigger.OnKill, ctxTarget: tgt);
                        }
                    }
                    break;

                case RealtimeSkillType.Heal:
                    {
                        int healAmt = 0;
                        if (s.healMaxHpRate > 0f)
                        {
                            // 最大HP割合回復 (rod等の healMul 込み)
                            healAmt = Mathf.RoundToInt(tgt.unit.MaxHp * s.healMaxHpRate * healMul);
                        }
                        else if (s.healAmount > 0)
                        {
                            healAmt = Mathf.RoundToInt(s.healAmount * healMul);
                        }
                        if (healAmt > 0)
                        {
                            tgt.unit.Heal(healAmt);
                            SpawnDamagePopup(tgt.transform.position, healAmt, DamagePopupSpawner.PopupKind.Heal);
                        }
                        ApplyStatusEffects(s, tgt);
                    }
                    break;

                case RealtimeSkillType.Buff:
                    // 自己バフ系 skill_id で個別効果を分岐
                    if (s.skillId == "Charge")
                    {
                        chargeActive = true;
                        manager?.BattleLog.Add($"[{BT:F2}s] {displayName} Charge 発動 (次の通常攻撃 1.5x)");
                    }
                    else if (s.skillId == "Barricade")
                    {
                        PlaceBarricade();
                    }
                    ApplyStatusEffects(s, tgt);
                    break;
                case RealtimeSkillType.Debuff:
                    ApplyStatusEffects(s, tgt);
                    break;

                case RealtimeSkillType.Control:
                    // ノックバック
                    if (s.knockbackMass > 0f) ApplyKnockback(tgt, s.knockbackMass);
                    // 挑発
                    if (s.tauntDurationSec > 0f) tgt.ForceTarget(this, s.tauntDurationSec);
                    // 付随ステータス (スタン等)
                    ApplyStatusEffects(s, tgt);
                    break;
            }

            // Attack 系でも knockback 指定があれば付与
            if (s.skillType == RealtimeSkillType.Attack)
            {
                if (s.knockbackMass > 0f && tgt != this) ApplyKnockback(tgt, s.knockbackMass);
            }
        }

        private void ApplyStatusEffects(RealtimeSkillDefinition s, RealtimeBattleUnit tgt)
        {
            if (tgt == null || tgt.unit == null) return;
            if (s.statusEffects == null) return;
            foreach (var eff in s.statusEffects)
            {
                if (eff == null) continue;
                int dur = eff.duration > 0 ? eff.duration : 5; // duration=0 は 5秒扱い
                tgt.unit.ApplyStatusEffect(eff.effectType, eff.value, dur);
            }
        }

        // ===========================================================================
        // 行動妨害共通仕様 (2026-05-05 確定 / 仕様書 §21 / project_disruption_spec.md)
        // ノックバック / スタン / フリーズ等を喰らった unit は以下の共通処理に乗せる:
        //
        //   ▼ 進行中SOキャンセル & CT 判定 (汎用ルール — 全 skill 共通)
        //     DealDamage 1個も未発火 → currentCast.skill 由来 pending events 削除 + アニメ停止 + CT 払戻
        //     DealDamage 1個でも発火済 → 同上 (currentCast.skill 由来のみ削除) + アニメ停止 + CT 通常消費
        //   ▼ 硬直時間 (lockout)
        //     ノックバック (前): 0  (即時別行動可)
        //     ノックバック (後): 進行中SOの残アニメ秒
        //     スタン/フリーズ等  : 状態異常の duration
        //
        // 重要: pendingEvents は currentCast.skill 由来のみ削除する。 既に launch 済の別 sub-skill
        // (= 別 skill 参照) は in-flight として残る。 例: 親 Slash の Branch_Sazanami 発火後、
        // Sazanami sub-skill cast 済の状態で殴られても Sazanami DealDamage は予定通り発火する。
        // 「並行進行中の sub-skill」と「未発火の連鎖 ConditionalCast」を区別する。
        //
        // caster≠被妨害unitの場合、 caster側の進行中SubSkillには影響なし (B側のみキャンセル)。
        // 全ノックバック呼出は ApplyKnockback 経由で共通化。 状態異常からも同じ OnDisrupted(lockoutSec) を呼ぶ。
        // ===========================================================================
        private void ApplyKnockback(RealtimeBattleUnit tgt, float dist)
        {
            if (tgt == null) return;
            Vector3 dir = tgt.transform.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            dir.Normalize();
            MovePreventOverlap(tgt, dir, dist, this); // caster (this) を衝突判定から除外
            // 行動妨害共通仕様: 進行中SOキャンセル + CT判定 + 硬直
            float lockoutSec = tgt.ComputeKnockbackLockout();
            tgt.OnDisrupted(lockoutSec);
        }

        // ===========================================================================
        // 行動妨害共通エントリポイント (仕様書 §21)
        // 呼出者: ApplyKnockback / 状態異常付与 (将来) など。
        // 動作: 進行中SOキャンセル + pendingEvents全クリア + CT払戻 (DealDamage未) or 維持 (DealDamage後)
        //       + アニメ中断 (Damage anim) + disruptedUntil 設定 + AI 即再評価。
        // lockoutSec: 硬直時間 (秒)。 ノックバック=ComputeKnockbackLockout、 状態異常=duration。
        // ===========================================================================
        public void OnDisrupted(float lockoutSec)
        {
            if (currentCast.valid)
            {
                bool fired = currentCast.hasFiredDealDamage;
                if (!fired
                    && currentCast.skillIndex >= 0
                    && skillNextReadyTime != null
                    && currentCast.skillIndex < skillNextReadyTime.Length)
                {
                    skillNextReadyTime[currentCast.skillIndex] = currentCast.refundCtTime;
                }
                // currentCast.skill 由来の pending イベントのみ削除。
                // 親 skill timeline の未発火 ConditionalCast event も同 skill 参照なので削除される (= 連鎖 SO 後続抑止)。
                // 既に launch 済の別 sub-skill (= 別 skill 参照) は in-flight として残る。 全 skill 共通汎用ルール。
                var castedSkill = currentCast.skill;
                pendingEvents.RemoveAll(pe => pe.skill == castedSkill);
                if (anim != null) anim.PlayDamage(); // attack anim を Damage 遷移で中断
                currentCast.valid = false;
            }
            attackingUntil = BT;             // 移動ロック解除
            disruptedUntil = Mathf.Max(disruptedUntil, BT + Mathf.Max(0f, lockoutSec));
            nextDecisionTime = 0f;           // 即時 AI 再評価
        }

        // 行動妨害 (Knockback / Stun / Freeze 共通) lockout 計算
        // memory [Disruption spec]: DealDamage 前=0 (CT払戻 + 即時再行動可)、 後=残アニメ秒 (硬直)
        private float ComputeKnockbackLockout()
        {
            if (!currentCast.valid) return 0f;
            if (!currentCast.hasFiredDealDamage) return 0f;
            return Mathf.Max(0f, currentCast.animEndTime - BT);
        }

        // SpaceJourneyUnit.OnDisruptApplied (Stun/Freeze/Petrify/Charm/Silence 付与時) のハンドラ。
        // ノックバックと同じ OnDisrupted を呼ぶ → 進行中SOキャンセル + CT払戻 + 残アニメ秒硬直。
        // duration (状態異常の持続秒) は lockout には使わない (= 状態異常自体は別管理で IsActionDisabled が見る)。
        private void OnDisruptAppliedHandler(float duration)
        {
            OnDisrupted(ComputeKnockbackLockout());
        }

        // unsubscribe (重複防止 + シーン破棄時の安全策)
        private void OnDestroy()
        {
            if (unit != null) unit.OnDisruptApplied -= OnDisruptAppliedHandler;
        }

        // cast 起点で呼んで現在キャスト中 SO を記録 (skillIndex=-1 はサブスキル)
        private void BeginCastInfo(RealtimeSkillDefinition s, int skillIndex)
        {
            if (s == null) return;
            currentCast.valid = true;
            currentCast.skill = s;
            currentCast.skillIndex = skillIndex;
            currentCast.refundCtTime = BT; // 払戻 = 即時使用可
            currentCast.animEndTime = BT + Mathf.Max(0f, s.castAnimSec);
            currentCast.hasFiredDealDamage = false;
        }

        /// <summary>他 unit と重ならないよう連続位置ベースで動かす。knockback/pull 共通処理。
        /// 連続移動版: 0.1m ステップで WouldOverlap チェックしながら進む。
        /// ignoreUnit を指定するとそのユニットは衝突判定から除外 (knockback/pull の caster を除外する用途)。
        /// ignoreSet を指定するとその全 unit を衝突判定から除外 (cascade knockback で BOX 内仲間同士を擦り抜ける用途)。
        /// caster と target が melee 範囲 (body半径合計内) で接近時、 caster を除外しないと第 1 ステップで break してしまう。</summary>
        private static void MovePreventOverlap(RealtimeBattleUnit unit, Vector3 dir, float dist, RealtimeBattleUnit ignoreUnit = null, HashSet<RealtimeBattleUnit> ignoreSet = null)
        {
            if (unit == null || dist <= 0f) return;
            const float step = 0.1f;
            float remaining = dist;
            Vector3 currentPos = unit.transform.position;
            while (remaining > 0f)
            {
                float s = Mathf.Min(step, remaining);
                Vector3 nextPos = currentPos + dir * s;
                if (unit.WouldOverlap(nextPos, ignoreUnit, ignoreSet)) break;
                currentPos = nextPos;
                remaining -= s;
            }
            unit.transform.position = currentPos;
            unit.OnForcedDisplacement();
        }

        /// <summary>強制移動 (knockback / pull / 瞬間移動 / 引き寄せ等) 共通の事後処理。
        /// - mover を新位置に Teleport (velocity リセット)
        /// - simulateMovement=false で旧 destination の transit 状態をクリア
        /// - nextDecisionTime=0 で次フレに即 target/action 再評価
        /// 棒立ち化バグ予防のため、強制で transform.position を変えた直後に必ず呼ぶ。</summary>
        public void OnForcedDisplacement()
        {
            if (mover != null)
            {
                mover.Teleport(transform.position, clearVelocity: true);
                mover.simulateMovement = false;
            }
            nextDecisionTime = 0f;
        }

        /// <summary>Move スキル: 現在ターゲット方向 (あれば) or 前方に shape.rangeMax 瞬間移動</summary>
        private void ApplySelfMove(RealtimeSkillDefinition s)
        {
            Vector3 fwd;
            if (currentTarget != null && currentTarget.IsAlive())
            {
                fwd = currentTarget.transform.position - transform.position;
                fwd.y = 0;
                if (fwd.sqrMagnitude < 0.01f) fwd = transform.forward;
                fwd.Normalize();
            }
            else
            {
                fwd = transform.forward;
            }
            MovePreventOverlap(this, fwd, s.shape.rangeMax);
        }

        /// <summary>自分→primary の射線上で、自分に一番近い敵を返す (味方は素通り)</summary>
        private RealtimeBattleUnit FindFirstEnemyInLine(RealtimeBattleUnit primary)
        {
            if (primary == null || manager == null) return primary;
            Vector3 self = transform.position; self.y = 0;
            Vector3 aim = primary.transform.position; aim.y = 0;
            Vector3 dir = aim - self;
            float totalDist = dir.magnitude;
            if (totalDist < 0.01f) return primary;
            dir /= totalDist;
            const float lineWidth = 0.5f; // 射線太さ (m)、この内に入った敵を候補

            RealtimeBattleUnit first = primary;
            float bestAlong = totalDist;
            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                if (u.ownerSide == ownerSide) continue; // 味方は素通り
                if (u == primary) continue;
                Vector3 toU = u.transform.position - self; toU.y = 0;
                float along = Vector3.Dot(toU, dir);
                if (along <= 0.2f || along >= totalDist) continue; // 自分の前、primary より手前
                float perp = (toU - dir * along).magnitude;
                if (perp > lineWidth) continue;
                if (along < bestAlong) { bestAlong = along; first = u; }
            }
            return first;
        }

        /// <summary>スキル設定に従い、最適なターゲットを選ぶ</summary>
        private RealtimeBattleUnit SelectSkillTarget(RealtimeSkillDefinition s)
        {
            if (manager == null) return null;
            if (s.targetSide == RealtimeTargetSide.Self) return this;

            var candidates = new List<RealtimeBattleUnit>();
            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                switch (s.targetSide)
                {
                    case RealtimeTargetSide.Enemy:
                        if (u.ownerSide == ownerSide) continue; break;
                    case RealtimeTargetSide.Ally:
                        if (u.ownerSide != ownerSide || u == this) continue; break;
                    case RealtimeTargetSide.AnyAllyIncludingSelf:
                        if (u.ownerSide != ownerSide) continue; break;
                }
                // ジョブフィルタ
                if (s.targetJobFilter != null && s.targetJobFilter.Length > 0)
                {
                    string jid = u.unit?.Body?.BodyJobId;
                    bool match = false;
                    foreach (var j in s.targetJobFilter)
                        if (j != null && j.bodyJobId == jid) { match = true; break; }
                    if (!match) continue;
                }
                // 射程内 (shape 種別に応じた実効リーチで判定)
                float d = Vector3.Distance(transform.position, u.transform.position);
                float maxReach = GetShapeMaxReach(s.shape);
                if (d < s.shape.rangeMin || d > maxReach) continue;
                candidates.Add(u);
            }
            if (candidates.Count == 0) return null;

            // currentTarget が候補内なら優先 (向き振り戻し往復を抑止)。
            // ※ Self skill は targetSide==Self で既に return 済みなのでここには来ない。
            if (currentTarget != null && currentTarget.IsAlive() && candidates.Contains(currentTarget))
                return currentTarget;

            switch (s.targetSelect)
            {
                case RealtimeTargetSelect.Self: return this;
                case RealtimeTargetSelect.Random:
                    return candidates[Random.Range(0, candidates.Count)];
                case RealtimeTargetSelect.LowestHp:
                    return BestBy(candidates, u => u.unit != null ? (float)u.unit.CurrentHp / Mathf.Max(1, u.unit.MaxHp) : 1f, false);
                case RealtimeTargetSelect.HighestHp:
                    return BestBy(candidates, u => u.unit != null ? (float)u.unit.CurrentHp / Mathf.Max(1, u.unit.MaxHp) : 1f, true);
                case RealtimeTargetSelect.Farthest:
                    return BestBy(candidates, u => Vector3.Distance(transform.position, u.transform.position), true);
                default: // Nearest
                    return BestBy(candidates, u => Vector3.Distance(transform.position, u.transform.position), false);
            }
        }

        private static RealtimeBattleUnit BestBy(List<RealtimeBattleUnit> list, System.Func<RealtimeBattleUnit, float> key, bool max)
        {
            RealtimeBattleUnit best = null;
            float bestVal = max ? float.MinValue : float.MaxValue;
            foreach (var u in list)
            {
                float v = key(u);
                if ((max && v > bestVal) || (!max && v < bestVal)) { bestVal = v; best = u; }
            }
            return best;
        }

        /// <summary>指定方向 dir に対して shape 内の対象を取得 (トライアローの側矢用)</summary>
        private List<RealtimeBattleUnit> CollectTargetsInDirection(RealtimeSkillDefinition s, Vector3 dir)
        {
            var result = new List<RealtimeBattleUnit>();
            if (manager == null) return result;
            Vector3 selfPos = transform.position; selfPos.y = 0;
            Vector3 dn = dir; dn.y = 0;
            if (dn.sqrMagnitude < 0.01f) return result;
            dn.Normalize();
            // aim 点は self から dir 方向に rangeMax 距離
            Vector3 aim = selfPos + dn * s.shape.rangeMax;
            bool meleeBlockedByBarricade = IsMeleeBlockedByBarricade(s);
            bool isBasicHere = (skills != null && skills.Count > 0 && s == skills[0]);
            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                switch (s.targetSide)
                {
                    case RealtimeTargetSide.Enemy:
                        if (u.ownerSide == ownerSide) continue; break;
                    case RealtimeTargetSide.Ally:
                        if (u.ownerSide != ownerSide || u == this) continue; break;
                    case RealtimeTargetSide.Self:
                        if (u != this) continue; break;
                    case RealtimeTargetSide.AnyAllyIncludingSelf:
                        if (u.ownerSide != ownerSide) continue; break;
                }
                if (IsInShape(s.shape, selfPos, aim, dn, u.transform.position, isBasicHere))
                {
                    if (meleeBlockedByBarricade && IsBarricadeBetween(selfPos, u.transform.position)) continue;
                    result.Add(u);
                }
            }
            return result;
        }

        /// <summary>近接単体スキル (Single + 短射程 + 非投射) はバリケード越しの敵を取れない。
        /// Lancer Thrust (Line) / 弓 (Single + projectile) / Burst (CircleAtTarget) は除外。</summary>
        private static bool IsMeleeBlockedByBarricade(RealtimeSkillDefinition s)
        {
            if (s == null) return false;
            return s.shape.shape == RealtimeTargetShape.Single
                && s.projectileHitMode != ProjectileHitMode.FirstEnemyInLine
                && s.shape.rangeMax <= 2.0f;
        }

        /// <summary>caster 位置から target 位置への直線上に Active barricade があるか。 melee filter 用。</summary>
        private bool IsBarricadeBetween(Vector3 from, Vector3 to)
        {
            if (manager?.barricades == null || manager.barricades.Count == 0) return false;
            Vector3 a = from; a.y = 0;
            Vector3 b = to; b.y = 0;
            Vector3 d = b - a;
            float maxDist = d.magnitude;
            if (maxDist < 1e-4f) return false;
            d /= maxDist;
            const float halfThk = Barricade.THICKNESS * 0.5f + 0.05f;
            foreach (var bar in manager.barricades)
            {
                if (bar == null || !bar.IsActive) continue;
                Vector3 bc = bar.center; bc.y = 0;
                Vector3 rel = bc - a;
                float along = Vector3.Dot(rel, d);
                if (along < -halfThk || along > maxDist + halfThk) continue;
                Vector3 closest = a + d * Mathf.Clamp(along, 0f, maxDist);
                if (bar.DistanceTo(closest) < halfThk) return true;
            }
            return false;
        }

        /// <summary>スキル形状内に入る対象全員を取得 (primary 方向基準)</summary>
        private List<RealtimeBattleUnit> CollectSkillTargets(RealtimeSkillDefinition s, RealtimeBattleUnit primary)
        {
            var result = new List<RealtimeBattleUnit>();
            if (manager == null || primary == null) return result;

            Vector3 selfPos = transform.position; selfPos.y = 0;
            Vector3 aim = primary.transform.position; aim.y = 0;
            Vector3 dir = (aim - selfPos);
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            dir.y = 0; dir.Normalize();
            bool meleeBlockedByBarricade = IsMeleeBlockedByBarricade(s);
            bool isBasicHere = (skills != null && skills.Count > 0 && s == skills[0]);

            foreach (var u in manager.AllUnits)
            {
                if (u == null || !u.IsAlive()) continue;
                // side フィルタ
                switch (s.targetSide)
                {
                    case RealtimeTargetSide.Enemy:
                        if (u.ownerSide == ownerSide) continue; break;
                    case RealtimeTargetSide.Ally:
                        if (u.ownerSide != ownerSide || u == this) continue; break;
                    case RealtimeTargetSide.Self:
                        if (u != this) continue; break;
                    case RealtimeTargetSide.AnyAllyIncludingSelf:
                        if (u.ownerSide != ownerSide) continue; break;
                }
                Vector3 uPos = u.transform.position; uPos.y = 0;
                if (IsInShape(s.shape, selfPos, aim, dir, uPos, isBasicHere))
                {
                    if (meleeBlockedByBarricade && IsBarricadeBetween(selfPos, uPos)) continue;
                    result.Add(u);
                }
            }
            return result;
        }

        /// <summary>点 p が形状に含まれるか。shapeSizeBonus は size 系フィールドに加算される。
        /// isBasic=true の時は通常攻撃 (skills[0]) なので basicShapeMul を width/length / 半径に乗算する
        /// (太陽の膨張: Burst の AoE が 1.5 倍に広がる用)。</summary>
        private bool IsInShape(RealtimeSkillShape sh, Vector3 selfPos, Vector3 aim, Vector3 dir, Vector3 p, bool isBasic = false)
        {
            Vector3 fromSelf = p - selfPos;
            float along = Vector3.Dot(fromSelf, dir);
            Vector3 side = fromSelf - dir * along;
            float perp = side.magnitude;
            float distFromSelf = fromSelf.magnitude;
            float bonus = shapeSizeBonus;
            float mul = isBasic ? basicShapeMul : 1f;
            float w = sh.widthMass * mul;
            float lng = sh.lengthMass * mul;

            switch (sh.shape)
            {
                case RealtimeTargetShape.Single:
                    return Vector3.Distance(p, aim) < 0.3f; // primary のみ (倍率非適用)
                case RealtimeTargetShape.CircleAtSelf:
                    return distFromSelf <= w + bonus;
                case RealtimeTargetShape.CircleAtTarget:
                    return Vector3.Distance(p, aim) <= w + bonus;
                case RealtimeTargetShape.Line:
                    return along >= 0f && along <= lng + bonus && perp <= w * 0.5f;
                case RealtimeTargetShape.Fan:
                    {
                        if (distFromSelf > w + bonus) return false;
                        float ang = Vector3.Angle(dir, fromSelf.normalized);
                        return ang <= sh.angleDeg * 0.5f;
                    }
                case RealtimeTargetShape.Diamond:
                    return Mathf.Abs(along) / Mathf.Max(0.01f, lng + bonus)
                         + perp / Mathf.Max(0.01f, w + bonus) <= 1f;
                case RealtimeTargetShape.Square:
                    return along >= 0f && along <= lng + bonus && perp <= w * 0.5f;
                case RealtimeTargetShape.CrossAtSelf:
                    {
                        bool onForward = Mathf.Abs(along) <= lng + bonus && perp <= w * 0.5f;
                        bool onSide = perp <= lng + bonus && Mathf.Abs(along) <= w * 0.5f;
                        return onForward || onSide;
                    }
            }
            return false;
        }
    }
}
