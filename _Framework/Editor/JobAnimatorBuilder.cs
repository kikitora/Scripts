#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UAnim = UnityEditor.Animations;

namespace SteraCube._Framework.Editor
{
    /// <summary>
    /// ExplosiveLLC の各職クリップから、
    /// Idle / Walk / Attack / Damage / Death のシンプルな AnimatorController を職ごとに生成する。
    /// パラメータ:
    ///   Bool   Moving  — true: Walk、false: Idle
    ///   Trigger Attack — 攻撃再生 → Idle に戻る
    ///   Trigger Damage — 被弾再生 → Idle に戻る
    ///   Trigger Die    — 死亡再生 (Any State 経由)、戻らない
    /// </summary>
    public static class JobAnimatorBuilder
    {
        private static readonly (string jobKey, string folder, string prefix)[] Jobs = new[]
        {
            // jobKey は BodyJobDefinition.bodyJobId と一致させる
            ("Warrior",  "Assets/ExplosiveLLC/2 Handed Warrior Mecanim Animation Pack/Animations", "2Handed"),
            ("Archer",   "Assets/ExplosiveLLC/Archer Warrior Mecanim Animation Pack/Animations",  "Archer"),
            ("Knight",   "Assets/ExplosiveLLC/Knight Warrior Mecanim Animation Pack/Animations",  "Knight"),
            ("Mage",     "Assets/ExplosiveLLC/Mage Warrior Mecanim Animation Pack/Animations",    "Mage"),
            ("Lancer",   "Assets/ExplosiveLLC/Spearman Warrior Mecanim Animation Pack/Animations", "Spearman"),
        };

        private const string OutputFolder = "Assets/0SteraCube/Animators";

        [MenuItem("Tools/SteraCube/Build Job Animators")]
        public static void Build()
        {
            EnsureFolder(OutputFolder);

            int built = 0;
            foreach (var (jobKey, folder, prefix) in Jobs)
            {
                if (!AssetDatabase.IsValidFolder(folder))
                {
                    Debug.LogWarning($"[JobAnimatorBuilder] Folder not found: {folder}");
                    continue;
                }

                var idle    = FindClip(folder, $"{prefix}@Idle.FBX") ?? FindClip(folder, $"{prefix}@Idle1.FBX");
                var walk    = FindClip(folder, $"{prefix}@Walk.FBX") ?? FindClip(folder, $"{prefix}@Run.FBX") ?? FindClip(folder, $"{prefix}@DashForward.FBX");
                // Archer は弓射 (ArrowAt1 / RangeAttack1) を基本攻撃モーションに。
                // ユーザー編集の standalone .anim を最優先 (AnimationEvents 保持のため)。
                AnimationClip attack;
                if (jobKey == "Archer")
                {
                    attack = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                            "Assets/0SteraCube/Copy/ExplosiveLLC/Archer Warrior Mecanim Animation Pack/Animations/ArrowAt1.anim")
                          ?? FindClip(folder, $"{prefix}@RangeAttack1.FBX")
                          ?? FindClip(folder, $"{prefix}@Attack1.FBX");
                }
                else
                {
                    attack = FindClip(folder, $"{prefix}@Attack1.FBX");
                }
                var damage  = FindClip(folder, $"{prefix}@LightHit.FBX");
                var death   = FindClip(folder, $"{prefix}@Death.FBX");
                // Victory/Defeat は各職の Revive / Stunned を代用 (Humanoid motion 確実)
                var victory = FindClip(folder, $"{prefix}@Revive.FBX");
                var defeat  = FindClip(folder, $"{prefix}@Stunned.FBX");

                if (idle == null || attack == null || damage == null || death == null)
                {
                    Debug.LogWarning($"[JobAnimatorBuilder] {jobKey}: missing clips (idle={idle} walk={walk} attack={attack} damage={damage} death={death})");
                }

                string outPath = $"{OutputFolder}/{jobKey}_Animator.controller";
                BuildController(outPath, idle, walk, attack, damage, death, victory, defeat);
                built++;
                Debug.Log($"[JobAnimatorBuilder] Built: {outPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[JobAnimatorBuilder] Done. Built {built} animators.");
        }

        private static AnimationClip FindClip(string folder, string fbxFileName)
        {
            string path = $"{folder}/{fbxFileName}";
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets)
            {
                if (a is AnimationClip ac && !ac.name.StartsWith("__preview__"))
                    return ac;
            }
            return null;
        }

        private static void BuildController(
            string outPath,
            AnimationClip idle, AnimationClip walk, AnimationClip attack,
            AnimationClip damage, AnimationClip death,
            AnimationClip victory = null, AnimationClip defeat = null)
        {
            // 既存があれば中身だけクリア (GUID/参照を保つため DeleteAsset は使わない)
            var ac = AssetDatabase.LoadAssetAtPath<UAnim.AnimatorController>(outPath);
            if (ac != null)
            {
                // Parameters 全削除
                while (ac.parameters != null && ac.parameters.Length > 0)
                    ac.RemoveParameter(0);
                // 既存 layer 内の stateMachine をクリア
                var sm0 = ac.layers[0].stateMachine;
                // AnyState transitions
                foreach (var t in sm0.anyStateTransitions.ToArray()) sm0.RemoveAnyStateTransition(t);
                // Entry transitions
                foreach (var t in sm0.entryTransitions.ToArray()) sm0.RemoveEntryTransition(t);
                // States
                foreach (var cs in sm0.states.ToArray()) sm0.RemoveState(cs.state);
                // SubStateMachines
                foreach (var ss in sm0.stateMachines.ToArray()) sm0.RemoveStateMachine(ss.stateMachine);
            }
            else
            {
                ac = UAnim.AnimatorController.CreateAnimatorControllerAtPath(outPath);
            }

            // パラメータ
            ac.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            ac.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Damage", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Die",    AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Victory", AnimatorControllerParameterType.Trigger);
            ac.AddParameter("Defeat",  AnimatorControllerParameterType.Trigger);

            var rootSm = ac.layers[0].stateMachine;

            // State
            var stIdle   = rootSm.AddState("Idle");   stIdle.motion   = idle;
            rootSm.defaultState = stIdle;
            var stWalk   = rootSm.AddState("Walk");   stWalk.motion   = (walk != null) ? walk : idle;
            var stAttack = rootSm.AddState("Attack"); stAttack.motion = (attack != null) ? attack : idle;
            var stDamage = rootSm.AddState("Damage"); stDamage.motion = (damage != null) ? damage : idle;
            var stDeath  = rootSm.AddState("Death");  stDeath.motion  = (death != null) ? death : idle;

            // Victory / Defeat state (クリップあれば利用、なければ Idle 代用だが state 名は同じ)
            var stVictory = rootSm.AddState("Victory"); stVictory.motion = (victory != null) ? victory : idle;
            var stDefeat  = rootSm.AddState("Defeat");  stDefeat.motion  = (defeat != null) ? defeat : idle;

            // Idle <-> Walk (ブレンド長めで滑らか)
            var t_iw = stIdle.AddTransition(stWalk);
            t_iw.hasExitTime = false; t_iw.duration = 0.25f;
            t_iw.AddCondition(UAnim.AnimatorConditionMode.If, 0, "Moving");

            var t_wi = stWalk.AddTransition(stIdle);
            t_wi.hasExitTime = false; t_wi.duration = 0.25f;
            t_wi.AddCondition(UAnim.AnimatorConditionMode.IfNot, 0, "Moving");

            // AnyState → Attack (即時感のため短め)
            var t_aa = rootSm.AddAnyStateTransition(stAttack);
            t_aa.hasExitTime = false; t_aa.duration = 0.08f;
            t_aa.AddCondition(UAnim.AnimatorConditionMode.If, 0, "Attack");
            t_aa.canTransitionToSelf = false;

            var t_ai = stAttack.AddTransition(stIdle);
            t_ai.hasExitTime = true; t_ai.exitTime = 0.9f; t_ai.duration = 0.2f;

            // AnyState → Damage
            var t_ad = rootSm.AddAnyStateTransition(stDamage);
            t_ad.hasExitTime = false; t_ad.duration = 0.08f;
            t_ad.AddCondition(UAnim.AnimatorConditionMode.If, 0, "Damage");
            t_ad.canTransitionToSelf = false;

            var t_di = stDamage.AddTransition(stIdle);
            t_di.hasExitTime = true; t_di.exitTime = 0.85f; t_di.duration = 0.2f;

            // AnyState → Death
            var t_dd = rootSm.AddAnyStateTransition(stDeath);
            t_dd.hasExitTime = false; t_dd.duration = 0.15f;
            t_dd.AddCondition(UAnim.AnimatorConditionMode.If, 0, "Die");
            t_dd.canTransitionToSelf = false;

            // AnyState → Victory
            var t_vv = rootSm.AddAnyStateTransition(stVictory);
            t_vv.hasExitTime = false; t_vv.duration = 0.2f;
            t_vv.AddCondition(UAnim.AnimatorConditionMode.If, 0, "Victory");
            t_vv.canTransitionToSelf = false;

            // AnyState → Defeat
            var t_ff = rootSm.AddAnyStateTransition(stDefeat);
            t_ff.hasExitTime = false; t_ff.duration = 0.2f;
            t_ff.AddCondition(UAnim.AnimatorConditionMode.If, 0, "Defeat");
            t_ff.canTransitionToSelf = false;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            var name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
