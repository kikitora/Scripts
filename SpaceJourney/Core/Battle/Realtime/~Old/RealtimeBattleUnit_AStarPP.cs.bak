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
            MaxFar = 3,   // 8 mass
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

        // ===== Passive 由来 状態 (AlwaysWhileActive で装備武器から設定) =====
        [HideInInspector] public float basicAttackMul = 1.0f;
        [HideInInspector] public float skillMul = 1.0f;
        [HideInInspector] public float cooldownMul = 1.0f;  // CT 倍率 (0.9x 等)
        [HideInInspector] public float healMul = 1.0f;
        [HideInInspector] public float rangeBonus = 0.0f;   // rangeMax + amount(m)
        [HideInInspector] public float shapeSizeBonus = 0.0f; // length/width + amount(m)
        [HideInInspector] public bool basicKindOverrideValid = false;
        [HideInInspector] public SkillDamageKind basicKindOverride = SkillDamageKind.Physical;
        [HideInInspector] public bool basicAttackDisabled = false;
        [HideInInspector] public int multiShotCount = 1;

        // ===== Passive 評価用 追跡フィールド =====
        private int _basicAttackHitCount = 0;
        private RealtimeBattleUnit _lastHitTarget = null;
        private int _sameTargetHitStreak = 0;
        private int _damagedPendingCounter = 0; // OnDamagedThenNextHit 用フラグ
        private float _lastHpPercentForTrigger = 100f; // OnSelfHpBelowPercent 閾値跨ぎ検出
        private float _nextHitDamageReductionRatio = 0f; // NextHitDamageReduction 効果の残量
        private bool _battleStartFired = false;

        private class PassiveRuntimeState
        {
            public int triggerCount = 0;
            public float nextReadyTime = 0f;
        }
        private readonly System.Collections.Generic.Dictionary<RealtimePassiveDefinition, PassiveRuntimeState> _passiveStates
            = new System.Collections.Generic.Dictionary<RealtimePassiveDefinition, PassiveRuntimeState>();

        // 内部状態
        private float nextAttackTime = 0f;
        private float attackingUntil = 0f; // この時刻まで移動禁止
        private float[] skillNextReadyTime; // skills と同じ長さで各スキルCT管理
        private float nextDecisionTime = 0f;
        private int lastDominantIndex = -1;
        private RealtimeBattleUnit forcedTarget;
        private float forcedUntil;
        private readonly List<PendingSkillEvent> pendingEvents = new();
        private RealtimeSkillDefinition activeCastSkill;
        private GameObject activeAttachment;
        private GameObject[] skillAttachments;
        private RealtimeBattleUnit lastAttacker;  // 最後に自分を攻撃したユニット
        private float lastAttackerUntil;          // 有効期限

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
                    return _basicAttackHitCount == Mathf.Max(1, p.triggerIntParam);
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
                    rangeBonus += p.effectAmount;  // amount を m 加算
                    break;
                case PassiveEffect.AttackShapeOverride:
                    shapeSizeBonus += p.effectAmount;
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
                            u.unit.TakeDamage(extra);
                            u.OnHit(u.unit.IsDead, this);
                            manager?.OnAttack(this, u, extra, before, u.unit.CurrentHp);
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

                case PassiveEffect.CooldownResetEnemyBasic:
                    // 対象の通常攻撃 CT を初期化
                    if (ctxTarget != null) ctxTarget.ResetBasicAttackCooldown();
                    break;

                case PassiveEffect.AutoReviveOnce:
                    // BattleStart で SurviveLethal を付与 (致死ダメを 1 回だけ HP1 に)
                    if (unit != null)
                    {
                        int pct = Mathf.Max(1, Mathf.RoundToInt(p.effectAmount));
                        // 既存 StatusEffect.SurviveLethal を使用 (1 回限り消費)
                        unit.ApplyStatusEffect(StatusEffectType.SurviveLethal, pct, 9999);
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
            public Vector3 castOrigin;
            public Vector3 castAim;
            public Vector3 castDir;
        }
        private RealtimeBattleUnit currentTarget;
        private RealtimeBattleManager manager;
        private JobAnimator anim;
        private bool wasMoving;
        private float stuckTime; // 立ち止まり累積時間

        // A* Pathfinding Project 連携
        private Pathfinding.AIPath aiPath;
        private Pathfinding.RVO.RVOController rvoCtrl;

        // 生死状態切替用: 生存時の Layer をキャッシュ (死亡時 Default に変え、復活で復元)
        private int _aliveLayer = -1;
        private bool _prevAlive = true;

        // フィールド境界 (RealtimeBattleStarter から設定される)
        public BattleFieldVisualizer fieldVisualizer;

        // 範囲マップ (manager 側で設定される)
        public static float RangeCloseMass = 1f;
        public static float RangeMidMass = 2f;
        public static float RangeFarMass = 5f;
        public static float RangeMaxFarMass = 8f;

        public void Setup(SpaceJourneyUnit u, int side, string name,
                          AttackRangeCategory range, float speed,
                          RealtimeBattleManager mgr)
        {
            unit = u;
            ownerSide = side;
            displayName = name;
            preferredRange = range;
            walkSpeed = speed;
            manager = mgr;
            basicAttackDamage = u != null ? u.AtFinal : 10f;

            anim = GetComponent<JobAnimator>();
            if (anim == null) anim = gameObject.AddComponent<JobAnimator>();
            anim.RefreshParams();

            // A* PP コンポーネント参照キャッシュ
            aiPath = GetComponent<Pathfinding.AIPath>();
            rvoCtrl = GetComponent<Pathfinding.RVO.RVOController>();
            if (aiPath != null)
            {
                aiPath.maxSpeed = walkSpeed;
                aiPath.simulateMovement = false;
            }
            if (rvoCtrl != null)
            {
                rvoCtrl.radius = bodyRadius;
            }

            // スキル CT 配列を用意 (opening CT 分先送り)
            skillNextReadyTime = new float[skills != null ? skills.Count : 0];
            for (int i = 0; i < skillNextReadyTime.Length; i++)
            {
                var s = skills[i];
                skillNextReadyTime[i] = BT + (s != null ? s.openingCooldownSec : 0f);
            }
            // スキル毎の手装備アタッチ (弓射の矢 等) を事前配置
            SpawnHandAttachments();
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

        private void Update()
        {
            // Pending イベント処理 (死亡しても発火継続: 既に放たれた矢は届く)
            ProcessPendingEvents();

            // 生死遷移時に RVO + Layer を同期 (死亡で他 unit から邪魔されなくなる、復活で復帰)
            SyncAliveState();

            if (manager == null || manager.IsFinished)
            {
                // 戦闘終了: AIPath を止めないと LateUpdate で動き続ける
                if (aiPath != null) aiPath.simulateMovement = false;
                return;
            }
            if (manager.IsCountdown) return; // カウントダウン中は unit AI 停止
            if (!IsAlive()) return;

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
            if (currentTarget == null) return; // 選べる対象なし

            Vector3 myPos = transform.position;
            Vector3 targetPos = currentTarget.transform.position;
            Vector3 toTarget = targetPos - myPos;
            toTarget.y = 0f;
            float dist = toTarget.magnitude;
            float preferred = GetPreferredRangeWorld();

            // 攻撃アニメ中 + postAttackLockSec 猶予まで「アニメ中」として扱う
            // (回転/AIPath を再開させず、終端付近の変な向きジャンプを防ぐ)
            bool isAttacking = BT < attackingUntil + postAttackLockSec;

            bool moving = false;
            if (isAttacking)
            {
                // 攻撃アニメ中: 完全停止 + RVO で押されないようロック
                if (aiPath != null) aiPath.simulateMovement = false;
                if (rvoCtrl != null) rvoCtrl.locked = true;
            }
            else
            {
                // アニメ中以外: 通常の RVO 挙動に戻す
                if (rvoCtrl != null) rvoCtrl.locked = false;
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

            // 向き制御: 攻撃アニメ中はルートモーション任せで固定しない
            // AIPath は LateUpdate で動くので Update 内の transform.position delta は常に 0。
            // 実際の移動方向は aiPath.velocity から取る (迂回時は target 方向と異なる)。
            if (!isAttacking)
            {
                Vector3 moveDir = Vector3.zero;
                if (aiPath != null)
                {
                    Vector3 v = aiPath.velocity; v.y = 0f;
                    if (v.sqrMagnitude > 0.0001f) moveDir = v.normalized;
                }
                Vector3 facingDir;
                if (moveDir.sqrMagnitude > 0.5f && toTarget.sqrMagnitude > 0.01f)
                {
                    float dot = Vector3.Dot(moveDir, toTarget.normalized);
                    facingDir = (dot < -0.1f)
                        ? toTarget.normalized       // 後退 → target 向き
                        : moveDir;                  // 前進/迂回/横移動 → 実移動方向向き
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
                transform.rotation = Quaternion.Slerp(transform.rotation, faceRot, 6f * BDT);
            }

            // フィールド境界クランプ (5x5 × 2 side = 幅5×奥行10)
            ClampToField();

            // moving 判定を AIPath の state から取り直す (AIPath は LateUpdate で動くので
            // posBefore-posAfter delta は Update 内では 0。AIPath.velocity を信用する)
            if (aiPath != null)
            {
                Vector3 v = aiPath.velocity; v.y = 0f;
                moving = aiPath.simulateMovement && !aiPath.reachedDestination && v.sqrMagnitude > 0.0001f;
            }
            stuckTime = 0f;

            // アニメ反映
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
            // AIPath が constrainInsideGraph で処理してくれるので、ClampToField は
            // 0.5m 以上大きくズレた時だけ安全ネットとして作動 (毎フレーム clamp → AIPath と競合 防止)
            if ((clamped - pos).sqrMagnitude > 0.25f)
            {
                transform.position = clamped;
                if (aiPath != null) aiPath.Teleport(clamped, false);
            }
        }

        /// <summary>指定位置が他ユニットと重なるか (最低距離以内か)。MovePreventOverlap 用。</summary>
        private bool WouldOverlap(Vector3 pos)
        {
            if (manager == null) return false;
            foreach (var u in manager.AllUnits)
            {
                if (u == null || u == this || !u.IsAlive()) continue;
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

        /// <summary>被弾時に外部から呼ぶ。attacker 情報で lastAttacker を更新 (ターゲット切替用)</summary>
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

        /// <summary>生死が切り替わった瞬間に RVO / Layer を同期する。
        /// 死亡: RVO 無効 + Layer を Default (0) に変えて GridGraph collisionCheck の対象外にする。
        /// 復活: 元の Layer に復元 + RVO 有効化。
        /// DynamicObstacle が checkTime 経過後に graph 更新するので walkable 化は少し遅延する。</summary>
        private void SyncAliveState()
        {
            bool alive = IsAlive();
            if (_prevAlive == alive) return;

            if (alive)
            {
                if (_aliveLayer >= 0) gameObject.layer = _aliveLayer;
                if (rvoCtrl != null) rvoCtrl.enabled = true;
            }
            else
            {
                if (_aliveLayer < 0) _aliveLayer = gameObject.layer;
                gameObject.layer = 0; // Default: graph mask (UnitObstacle) 外 → 邪魔にならない
                if (rvoCtrl != null) rvoCtrl.enabled = false;
                if (aiPath != null) aiPath.simulateMovement = false;
            }
            _prevAlive = alive;
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
                        return IsSkillReady(0) && dist >= bs.shape.rangeMin && dist <= (bs.shape.rangeMax + rangeBonus);
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
            }
            return false;
        }

        private bool IsLastAttackerValid()
        {
            return lastAttacker != null && lastAttacker.IsAlive() && BT < lastAttackerUntil;
        }

        /// <summary>被弾直近 AND lastAttacker が現ターゲットより近い</summary>
        private bool IsAttackerCloserThanCurrentTarget()
        {
            if (!IsLastAttackerValid()) return false;
            if (currentTarget == null || !currentTarget.IsAlive()) return true; // target 無し → attacker 優先
            float dTarget = Vector3.Distance(transform.position, currentTarget.transform.position);
            float dAttacker = Vector3.Distance(transform.position, lastAttacker.transform.position);
            return dAttacker < dTarget;
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

        public void ForceTarget(RealtimeBattleUnit taunter, float durationSec)
        {
            forcedTarget = taunter;
            forcedUntil = BT + durationSec;
            nextDecisionTime = 0f; // 即時再評価
        }

        private void MaintainCurrentTarget()
        {
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

            // 3. targetList 上から評価して dominant entry を特定
            var list = GetEffectiveTargetList();
            int dominantIdx = -1;
            RealtimeTargetEntry dominant = null;
            for (int i = 0; i < list.Count; i++)
            {
                var e = list[i];
                if (EvalTargetCondition(e))
                {
                    dominantIdx = i;
                    dominant = e;
                    break;
                }
            }

            nextDecisionTime = BT + decisionIntervalSec;

            if (dominant == null)
            {
                currentTarget = null;
                lastDominantIndex = -1;
                return;
            }

            // 4. dominant が前回と同じで currentTarget が生きてれば維持
            if (dominantIdx == lastDominantIndex && currentTarget != null && currentTarget.IsAlive())
            {
                return;
            }

            // 5. 新 dominant or target 死亡 → 再選択
            currentTarget = SelectTargetByEntry(dominant);
            lastDominantIndex = dominantIdx;
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
                case RealtimeCondition.FragileAllyTargetedByEnemy:
                    return IsFragileAllyTargetedByEnemy();
                case RealtimeCondition.WasAttackedRecently:
                    return IsLastAttackerValid();
                case RealtimeCondition.AttackedAndNoEnemyInBasicRange:
                    return IsLastAttackerValid() && !AnyEnemyInPreferredRange();
                case RealtimeCondition.AttackerCloserThanCurrentTarget:
                    return IsAttackerCloserThanCurrentTarget();
            }
            return false;
        }

        /// <summary>shape ごとの実効リーチ (m)。shapeSizeBonus を加算する。</summary>
        private float GetShapeMaxReach(RealtimeSkillShape sh)
        {
            if (sh == null) return 1f;
            float bonus = shapeSizeBonus;
            switch (sh.shape)
            {
                case RealtimeTargetShape.Single:
                case RealtimeTargetShape.CircleAtTarget:
                    return sh.rangeMax + rangeBonus; // 着弾型は rangeBonus 優先
                case RealtimeTargetShape.CircleAtSelf:
                case RealtimeTargetShape.Fan:
                    return sh.widthMass + bonus;
                case RealtimeTargetShape.Line:
                case RealtimeTargetShape.Square:
                case RealtimeTargetShape.CrossAtSelf:
                    return sh.lengthMass + bonus;
                case RealtimeTargetShape.Diamond:
                    return Mathf.Max(sh.widthMass, sh.lengthMass) + bonus;
                default: return sh.rangeMax + rangeBonus;
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
                candidates.Add(u);
            }
            if (candidates.Count == 0) return null;

            switch (e.targetSelect)
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

        private bool ExecuteAction(RealtimeActionEntry entry, Vector3 toTarget, float dist, float preferred, out bool committed)
        {
            committed = true; // 既定: 発火完了 (break)
            var act = entry.action;
            switch (act)
            {
                case RealtimeAction.Wait:
                    return false;

                case RealtimeAction.BasicAttack:
                    // DisableBasicAttack (アルカナマキシマス等) 有効なら basic 攻撃をスキップ
                    if (basicAttackDisabled) { committed = false; break; }
                    // skills[0] があれば新スキルとして発動、なければ内蔵処理
                    if (skills != null && skills.Count > 0 && skills[0] != null)
                    {
                        committed = CastSkill(0); // 失敗なら次優先度へ fall-through
                    }
                    else if (BT >= nextAttackTime && dist <= GetAttackRangeMass())
                    {
                        FireBasicAttack(currentTarget);
                        nextAttackTime = BT + basicAttackCooldownSec;
                        attackingUntil = BT + attackAnimDurationSec;
                    }
                    else
                    {
                        committed = false;
                    }
                    return false;

                case RealtimeAction.CastSkill:
                    committed = CastSkill(entry.actionSkillIndex);
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

                case RealtimeAction.MoveToOwnRange:     return MoveToDistance(toTarget, dist, preferred);
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

        /// <summary>ターゲットとの距離を targetDist 以下まで詰める。
        /// A* Pathfinding Project (AIPath + RVO) に委譲。</summary>
        private bool MoveToDistance(Vector3 toTarget, float dist, float targetDist)
        {
            if (dist <= targetDist)
            {
                if (aiPath != null) aiPath.simulateMovement = false;
                return false;
            }
            if (currentTarget == null || aiPath == null) return false;
            // destination は target が動いた時のみ更新 (path 再計算頻度を下げる)
            Vector3 newDest = currentTarget.transform.position;
            if (Vector3.SqrMagnitude(newDest - aiPath.destination) > 0.25f) // 0.5m 以上差
            {
                aiPath.destination = newDest;
            }
            aiPath.endReachedDistance = targetDist;
            aiPath.maxSpeed = walkSpeed;
            aiPath.simulateMovement = true;
            return !aiPath.reachedDestination;
        }

        /// <summary>ターゲット距離を targetDist 以下 0.3m 以内に維持。AIPath に委譲。</summary>
        private bool MaintainDistance(Vector3 toTarget, float dist, float targetDist)
        {
            const float tolerance = 0.3f;
            float maxDist = targetDist;
            float minDist = targetDist - tolerance;
            if (dist >= minDist && dist <= maxDist)
            {
                if (aiPath != null) aiPath.simulateMovement = false;
                return false;
            }
            if (toTarget.sqrMagnitude < 0.01f || aiPath == null) return false;

            if (dist > maxDist && currentTarget != null)
            {
                // 前進: target に向かう
                aiPath.destination = currentTarget.transform.position;
                aiPath.endReachedDistance = maxDist;
                aiPath.maxSpeed = walkSpeed;
                aiPath.simulateMovement = true;
                return true;
            }
            // 後退: 反対方向の point を destination に
            Vector3 awayPos = transform.position - toTarget.normalized * (minDist - dist);
            aiPath.destination = awayPos;
            aiPath.endReachedDistance = 0.1f;
            aiPath.maxSpeed = walkSpeed;
            aiPath.simulateMovement = true;
            return true;
        }

        /// <summary>ターゲットから targetDist 以上離れる。AIPath に委譲。</summary>
        private bool MoveAwayToDistance(Vector3 toTarget, float dist, float targetDist)
        {
            if (dist >= targetDist)
            {
                if (aiPath != null) aiPath.simulateMovement = false;
                return false;
            }
            if (toTarget.sqrMagnitude < 0.01f || aiPath == null) return false;
            // 反対方向に targetDist まで離れる
            Vector3 awayPos = transform.position - toTarget.normalized * (targetDist - dist);
            aiPath.destination = awayPos;
            aiPath.endReachedDistance = 0.1f;
            aiPath.maxSpeed = walkSpeed;
            aiPath.simulateMovement = true;
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

        /// <summary>現在ターゲットがスキルの射程内かチェック (rangeBonus 込み)</summary>
        private bool IsTargetInSkillRange(int index)
        {
            var s = GetSkill(index);
            if (s == null || currentTarget == null) return false;
            float d = Vector3.Distance(transform.position, currentTarget.transform.position);
            return d >= s.shape.rangeMin && d <= (s.shape.rangeMax + rangeBonus);
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
                anim?.PlayAttack();
                return true;
            }

            // スキル独自の対象を再選択
            var primary = SelectSkillTarget(s);
            if (primary == null) return false;

            // 形状範囲内の全ヒット対象
            var hits = CollectSkillTargets(s, primary);
            // FirstEnemyInLine モード: 射線上で最初に遭遇する敵に命中差替
            if (s.projectileHitMode == ProjectileHitMode.FirstEnemyInLine && hits.Count > 0)
            {
                var firstInLine = FindFirstEnemyInLine(primary);
                if (firstInLine != null && firstInLine != primary)
                {
                    hits.Clear();
                    hits.Add(firstInLine);
                }
            }
            if (hits.Count == 0) return false;

            // 向きを primary に
            Vector3 toP = primary.transform.position - transform.position;
            toP.y = 0;
            if (toP.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(toP.normalized, Vector3.up);

            // スナップショット (timeline 用)
            Vector3 castOrigin = transform.position;
            Vector3 castAim = primary.transform.position;
            Vector3 castDir = (castAim - castOrigin); castDir.y = 0;
            if (castDir.sqrMagnitude < 0.01f) castDir = transform.forward;
            castDir.Normalize();

            // timeline 処理
            bool hasDealDamage = false;
            float projectileDistanceDelay = 0f;
            if (s.projectileSpeed > 0f)
            {
                float dist = Vector3.Distance(castOrigin, castAim);
                projectileDistanceDelay = dist / s.projectileSpeed;
            }
            if (s.timeline != null && s.timeline.Count > 0)
            {
                foreach (var ev in s.timeline)
                {
                    if (ev == null) continue;
                    if (ev.kind == RealtimeSkillEventKind.DealDamage) hasDealDamage = true;
                    // DealDamage だけ projectileSpeed による距離分遅延を加算
                    float extraDelay = (ev.kind == RealtimeSkillEventKind.DealDamage) ? projectileDistanceDelay : 0f;
                    pendingEvents.Add(new PendingSkillEvent
                    {
                        fireTime = BT + Mathf.Max(0f, ev.timeSec) + extraDelay,
                        ev = ev,
                        skill = s,
                        primaryTarget = primary,
                        hitTargets = hits,
                        castOrigin = castOrigin,
                        castAim = castAim,
                        castDir = castDir,
                    });
                }
            }

            // DealDamage イベントが timeline に無ければ即時適用 (後方互換)
            if (!hasDealDamage)
            {
                foreach (var tgt in hits)
                {
                    ApplySkillEffect(s, tgt);
                }
            }

            // アニメ (現状 Attack Trigger 流用。スキル固有は今後)
            anim?.PlayAttack();

            // CT + ロック
            skillNextReadyTime[index] = BT + s.cooldownSec * skillCooldownMul * cooldownMul;
            attackingUntil = BT + s.castAnimSec;
            activeCastSkill = s; // AnimationEvent 参照用

            // Knight Defend: ガード 100% 状態を 2 秒付与 (盾副次効果は盾本来 % で判定)
            if (!string.IsNullOrEmpty(s.skillId) && s.skillId == "Defend")
            {
                ForceGuardFor(2.0f);
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

        /// <summary>矢を複製して飛翔させ、元の矢は非表示に戻す</summary>
        public void ArrowEqOff()
        {
            if (activeAttachment == null) return;
            var orig = activeAttachment;
            if (!orig.activeSelf) return;

            // 複製: nock 状態の world pos/rot をそのまま引き継ぐ (ジャンプ防止)
            var copy = Instantiate(orig, orig.transform.position, orig.transform.rotation, null);
            copy.transform.localScale = orig.transform.lossyScale;
            copy.SetActive(true);
            // Driver/Rotator を複製側では無効化 (飛翔に干渉させない)
            var drv = copy.GetComponent<AttachmentCurveDriver>();
            if (drv != null) Destroy(drv);
            var rotComp = copy.GetComponent<AttachmentRotator>();
            if (rotComp != null) Destroy(rotComp);
            orig.SetActive(false);
            // 発光: Point Light を子に追加
            var lightGo = new GameObject("ArrowLight");
            lightGo.transform.SetParent(copy.transform, false);
            lightGo.transform.localPosition = Vector3.zero;
            var lt = lightGo.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = new Color(1f, 0.85f, 0.4f); // 暖色
            lt.intensity = 2f;
            lt.range = 3f;

            // ターゲット位置 + aim rotation
            Vector3 targetPos = (currentTarget != null && currentTarget.IsAlive())
                ? currentTarget.transform.position
                : copy.transform.position + transform.forward * 10f;
            Quaternion aimRot = Quaternion.LookRotation((targetPos - copy.transform.position).normalized, Vector3.up);
            if (activeCastSkill != null)
                aimRot *= Quaternion.Euler(activeCastSkill.handAttachmentEulerOffset);

            float dist = Vector3.Distance(copy.transform.position, targetPos);
            float spd = (activeCastSkill != null && activeCastSkill.projectileSpeed > 0f)
                ? activeCastSkill.projectileSpeed : 15f;
            float flyTime = Mathf.Max(0.05f, dist / spd);
            float rotBlend = activeCastSkill != null ? activeCastSkill.flyRotationBlendTime : 0.2f;

            // 位置: target へ線形移動 (倍速適用)
            float spdMul = RealtimeBattleManager.GlobalSpeed;
            var moveT = DOTween.To(() => copy.transform.position,
                    x => copy.transform.position = x,
                    targetPos, flyTime)
                .SetEase(Ease.Linear)
                .OnComplete(() => { if (copy != null) Destroy(copy); });
            moveT.timeScale = spdMul;
            // 回転は既に aim 済 (AttachmentRotator により nock 中に完了)、微補正のみ
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

        private void FireEvent(PendingSkillEvent pe)
        {
            switch (pe.ev.kind)
            {
                case RealtimeSkillEventKind.DealDamage:
                    if (pe.hitTargets == null) return;
                    {
                        // basic attack 発射時は MultiShot (日本刀=2 / トリデントアーチャー=3 等) を適用
                        bool isBasicCast = skills != null && skills.Count > 0 && pe.skill == skills[0];
                        int hitCount = isBasicCast ? Mathf.Max(1, multiShotCount) : 1;
                        foreach (var tgt in pe.hitTargets)
                        {
                            if (tgt == null || !tgt.IsAlive()) continue;
                            for (int shot = 0; shot < hitCount && tgt.IsAlive(); shot++)
                            {
                                ApplySkillEffect(pe.skill, tgt);
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
                        // ボーン子化する場合、親の座標系でオフセットを掛ける
                        if (!string.IsNullOrEmpty(pe.ev.attachBoneName))
                        {
                            var boneTf = ResolveAttachTransform(pe.ev.attachBoneName);
                            if (boneTf != null)
                            {
                                go = Instantiate(pe.ev.effectPrefab, boneTf);
                                go.transform.localPosition = pe.ev.offset;
                                go.transform.localRotation = Quaternion.identity;
                            }
                            else
                            {
                                Debug.LogWarning($"[RealtimeSkill] attachBoneName '{pe.ev.attachBoneName}' not found on {name} (even via Humanoid fallback). Spawning in world.");
                                Vector3 wp = ResolveEffectPosition(pe);
                                go = Instantiate(pe.ev.effectPrefab, wp, rot);
                            }
                        }
                        else
                        {
                            Vector3 wp = ResolveEffectPosition(pe);
                            go = Instantiate(pe.ev.effectPrefab, wp, rot);
                        }
                        // 倍速適用: 生成したエフェクト配下の Particle/Animator を mul で加速
                        ApplyBattleSpeedToEffect(go);
                        // lifeSec も倍速で縮める (見た目と破棄タイミングを揃える)
                        if (pe.ev.lifeSec > 0f)
                        {
                            float mul = Mathf.Max(0.01f, RealtimeBattleManager.GlobalSpeed);
                            Destroy(go, pe.ev.lifeSec / mul);
                        }

                        // Projectile 付きなら skill.projectileSpeed を同期
                        if (pe.skill != null && pe.skill.projectileSpeed > 0f)
                        {
                            var proj = go.GetComponent<Projectile>();
                            if (proj != null) proj.speed = pe.skill.projectileSpeed;
                        }
                    }
                    break;

                case RealtimeSkillEventKind.PlaySound:
                    if (pe.ev.sound != null)
                    {
                        AudioSource.PlayClipAtPoint(pe.ev.sound, transform.position);
                    }
                    break;
            }
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

        private void ApplySkillEffect(RealtimeSkillDefinition s, RealtimeBattleUnit tgt)
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
                                raw = Mathf.RoundToInt(unit.MatFinal * s.powerMul - tgt.unit.MdfFinal * 0.5f);
                                break;
                            default: // Physical, PenetratePhysical
                                raw = Mathf.RoundToInt(at * s.powerMul - df * 0.5f);
                                break;
                        }
                        // BasicAttackMul / SkillMul を適用
                        float mul = isBasicHere ? basicAttackMul : skillMul;
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
                        tgt.unit.TakeDamage(dmg);
                        tgt.OnHit(tgt.unit.IsDead);
                        manager?.OnAttack(this, tgt, dmg, before, tgt.unit.CurrentHp);
                        // ダメージ数値ポップアップ
                        var kind = (s.damageKind == SkillDamageKind.Magical || s.damageKind == SkillDamageKind.PenetrateMagical)
                            ? DamagePopupSpawner.PopupKind.Magic
                            : DamagePopupSpawner.PopupKind.Damage;
                        SpawnDamagePopup(tgt.transform.position, dmg, kind);

                        // ===== Passive トリガ =====
                        // 攻撃者側: OnDealDamage + basic の時のみ N 回目 + 連続命中
                        bool isBasic = (skills != null && skills.Count > 0 && s == skills[0]);
                        if (isBasic) _basicAttackHitCount++;
                        if (_lastHitTarget == tgt) _sameTargetHitStreak++;
                        else { _lastHitTarget = tgt; _sameTargetHitStreak = 1; }

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
                    if (s.healAmount > 0)
                    {
                        int healAmt = Mathf.RoundToInt(s.healAmount * healMul);
                        tgt.unit.Heal(healAmt);
                        SpawnDamagePopup(tgt.transform.position, healAmt, DamagePopupSpawner.PopupKind.Heal);
                    }
                    ApplyStatusEffects(s, tgt);
                    break;

                case RealtimeSkillType.Buff:
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

        private void ApplyKnockback(RealtimeBattleUnit tgt, float dist)
        {
            if (tgt == null) return;
            Vector3 dir = tgt.transform.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude < 0.01f) dir = transform.forward;
            dir.Normalize();
            MovePreventOverlap(tgt, dir, dist);
        }

        /// <summary>他unitと重なる寸前まで stepwise に動かす。knockback/pull/teleport 共通処理</summary>
        private static void MovePreventOverlap(RealtimeBattleUnit unit, Vector3 dir, float dist)
        {
            if (unit == null || dist <= 0f) return;
            const float step = 0.1f;
            float remaining = dist;
            Vector3 currentPos = unit.transform.position;
            while (remaining > 0f)
            {
                float s = Mathf.Min(step, remaining);
                Vector3 nextPos = currentPos + dir * s;
                if (unit.WouldOverlap(nextPos)) break;
                currentPos = nextPos;
                remaining -= s;
            }
            unit.transform.position = currentPos;
            // AIPath に位置同期 (ないと path が旧位置ベースのまま計算される)
            if (unit.aiPath != null) unit.aiPath.Teleport(currentPos, false);
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
                if (IsInShape(s.shape, selfPos, aim, dir, uPos))
                    result.Add(u);
            }
            return result;
        }

        /// <summary>点 p が形状に含まれるか。shapeSizeBonus は size 系フィールドに加算される。</summary>
        private bool IsInShape(RealtimeSkillShape sh, Vector3 selfPos, Vector3 aim, Vector3 dir, Vector3 p)
        {
            Vector3 fromSelf = p - selfPos;
            float along = Vector3.Dot(fromSelf, dir);
            Vector3 side = fromSelf - dir * along;
            float perp = side.magnitude;
            float distFromSelf = fromSelf.magnitude;
            float bonus = shapeSizeBonus;

            switch (sh.shape)
            {
                case RealtimeTargetShape.Single:
                    return Vector3.Distance(p, aim) < 0.3f; // primary のみ
                case RealtimeTargetShape.CircleAtSelf:
                    return distFromSelf <= sh.widthMass + bonus;
                case RealtimeTargetShape.CircleAtTarget:
                    return Vector3.Distance(p, aim) <= sh.widthMass + bonus;
                case RealtimeTargetShape.Line:
                    return along >= 0f && along <= sh.lengthMass + bonus && perp <= sh.widthMass * 0.5f;
                case RealtimeTargetShape.Fan:
                    {
                        if (distFromSelf > sh.widthMass + bonus) return false;
                        float ang = Vector3.Angle(dir, fromSelf.normalized);
                        return ang <= sh.angleDeg * 0.5f;
                    }
                case RealtimeTargetShape.Diamond:
                    return Mathf.Abs(along) / Mathf.Max(0.01f, sh.lengthMass + bonus)
                         + perp / Mathf.Max(0.01f, sh.widthMass + bonus) <= 1f;
                case RealtimeTargetShape.Square:
                    return along >= 0f && along <= sh.lengthMass + bonus && perp <= sh.widthMass * 0.5f;
                case RealtimeTargetShape.CrossAtSelf:
                    {
                        bool onForward = Mathf.Abs(along) <= sh.lengthMass + bonus && perp <= sh.widthMass * 0.5f;
                        bool onSide = perp <= sh.lengthMass + bonus && Mathf.Abs(along) <= sh.widthMass * 0.5f;
                        return onForward || onSide;
                    }
            }
            return false;
        }
    }
}
