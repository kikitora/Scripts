using System.Collections.Generic;
using UnityEngine;

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
        [Header("優先度行動リスト")]
        [Tooltip("上から順に条件を評価し、最初にマッチしたアクションを実行。空なら既定ルール適用。")]
        public List<RealtimeActionEntry> actionList = new();

        // 内部状態
        private float nextAttackTime = 0f;
        private float attackingUntil = 0f; // この時刻まで移動禁止
        private RealtimeBattleUnit currentTarget;
        private RealtimeBattleManager manager;
        private JobAnimator anim;
        private bool wasMoving;

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
        }

        public float GetPreferredRangeWorld()
        {
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

        private void Update()
        {
            if (manager == null || manager.IsFinished) return;
            if (!IsAlive()) return;

            // ターゲット探索 (一番近い敵)
            currentTarget = FindNearestEnemy();
            if (currentTarget == null)
            {
                // 敵不在
                return;
            }

            Vector3 myPos = transform.position;
            Vector3 targetPos = currentTarget.transform.position;
            Vector3 toTarget = targetPos - myPos;
            toTarget.y = 0f;
            float dist = toTarget.magnitude;
            float preferred = GetPreferredRangeWorld();

            // 進行方向を target に向ける
            if (toTarget.sqrMagnitude > 0.01f)
            {
                Quaternion facing = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, facing, 10f * Time.deltaTime);
            }

            // preferred = 最大攻撃可能距離。範囲内なら攻撃優先、範囲外なら接近。
            // 攻撃アニメ中のみ完全停止。
            bool isAttacking = Time.time < attackingUntil;

            bool moving = false;
            if (isAttacking)
            {
                // 攻撃アニメ中: 完全停止 (リスト評価しない)
            }
            else
            {
                // 優先度リストを上から評価。空ならデフォルトリスト使用
                var list = (actionList != null && actionList.Count > 0)
                    ? actionList
                    : DefaultActionList();

                foreach (var entry in list)
                {
                    if (!EvalCondition(entry.condition, dist)) continue;
                    moving = ExecuteAction(entry.action, toTarget, dist, preferred);
                    break;
                }
            }

            // フィールド境界クランプ (5x5 × 2 side = 幅5×奥行10)
            ClampToField();

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
            transform.position = clamped;

            // 味方同士の重なり回避 (最低距離 = 0.7)
            AvoidOverlap();
        }

        private static readonly float MinUnitDistance = 0.7f;

        private void AvoidOverlap()
        {
            if (manager == null) return;
            Vector3 myPos = transform.position;
            foreach (var u in manager.AllUnits)
            {
                if (u == null || u == this || !u.IsAlive()) continue;
                // 同陣営のみ弾く (敵とは preferred range で間合いを取る)
                if (u.ownerSide != ownerSide) continue;
                Vector3 toU = u.transform.position - myPos;
                toU.y = 0;
                float d = toU.magnitude;
                if (d < MinUnitDistance && d > 0.01f)
                {
                    Vector3 away = -toU.normalized * (MinUnitDistance - d) * 0.5f;
                    transform.position += away;
                }
            }
        }

        /// <summary>被弾時に外部から呼ぶ</summary>
        public void OnHit(bool died)
        {
            if (died) anim?.PlayDie();
            else anim?.PlayDamage();
        }

        // ─────────────────────────────────
        // 優先度アクション評価
        // ─────────────────────────────────

        /// <summary>デフォルト行動リスト (Inspector で override なしの場合)</summary>
        private static readonly List<RealtimeActionEntry> _defaultList = new()
        {
            new RealtimeActionEntry(RealtimeCondition.CanBasicAttack, RealtimeAction.BasicAttack, "攻撃可能なら攻撃"),
            new RealtimeActionEntry(RealtimeCondition.Always,         RealtimeAction.MoveToOwnRange, "さもなくば自分の射程まで接近"),
        };

        private List<RealtimeActionEntry> DefaultActionList() => _defaultList;

        /// <summary>
        /// 職別距離カテゴリ (基本攻撃の射程)。preferredRange から逆算。
        /// </summary>
        public float GetAttackRangeMass()
        {
            return GetPreferredRangeWorld();
        }

        private bool EvalCondition(RealtimeCondition cond, float dist)
        {
            switch (cond)
            {
                case RealtimeCondition.Always: return true;
                case RealtimeCondition.CanBasicAttack:
                    return Time.time >= nextAttackTime && dist <= GetAttackRangeMass();
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
            }
            return false;
        }

        private bool ExecuteAction(RealtimeAction act, Vector3 toTarget, float dist, float preferred)
        {
            switch (act)
            {
                case RealtimeAction.Wait:
                    return false;

                case RealtimeAction.BasicAttack:
                    FireBasicAttack(currentTarget);
                    nextAttackTime = Time.time + basicAttackCooldownSec;
                    attackingUntil = Time.time + attackAnimDurationSec;
                    return false;

                case RealtimeAction.MoveToOwnRange:     return MoveToDistance(toTarget, dist, preferred);
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

        /// <summary>ターゲットとの距離を targetDist 以下まで詰める。既に近ければ何もしない</summary>
        private bool MoveToDistance(Vector3 toTarget, float dist, float targetDist)
        {
            if (dist <= targetDist) return false;
            if (toTarget.sqrMagnitude < 0.01f) return false;
            transform.position += toTarget.normalized * walkSpeed * Time.deltaTime;
            return true;
        }

        /// <summary>ターゲットから targetDist 以上離れる。既に離れてれば何もしない</summary>
        private bool MoveAwayToDistance(Vector3 toTarget, float dist, float targetDist)
        {
            if (dist >= targetDist) return false;
            if (toTarget.sqrMagnitude < 0.01f) return false;
            transform.position -= toTarget.normalized * walkSpeed * Time.deltaTime;
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

            // 簡易ダメ計算: 攻撃力 - 防御力*0.5 (最低1)
            int at = unit.AtFinal;
            int df = target.unit.DfFinal;
            int dmg = Mathf.Max(1, Mathf.RoundToInt(at - df * 0.5f));

            int before = target.unit.CurrentHp;
            target.unit.TakeDamage(dmg);
            int after = target.unit.CurrentHp;

            // 攻撃アニメ
            anim?.PlayAttack();
            // 被弾側アニメ
            target.OnHit(target.unit.IsDead);

            manager?.OnAttack(this, target, dmg, before, after);
        }
    }
}
