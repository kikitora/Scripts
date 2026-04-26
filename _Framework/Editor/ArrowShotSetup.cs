#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime.EditorTools
{
    /// <summary>
    /// ArrowShot SO に timeline / prefab refs を一括セット。
    /// </summary>
    public static class ArrowShotSetup
    {
        [MenuItem("Tools/SteraCube/Configure ArrowShot Timeline")]
        public static void Configure()
        {
            var skillPath = "Assets/0SteraCube/ScriptableObject/Skill/BodySkill/ArrowShot.asset";
            var skill = AssetDatabase.LoadAssetAtPath<RealtimeSkillDefinition>(skillPath);
            if (skill == null) { Debug.LogError($"ArrowShot not found at {skillPath}"); return; }

            // 手装備・飛翔は DownrainDC の本物の矢モデルを使用
            var nock = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/DownrainDC/HeroSeries/HS_03_ModularHero/Prefab/Single_Static/Prefab_Static_Weapon_Bow/Bow_Arrow/Bow_Arrow_01_1.prefab");
            var proj = nock; // 飛翔側も同じ prefab を使う (複製・DOTween で飛ばす設計)
            if (nock == null)
            {
                Debug.LogError("Bow_Arrow_01_1.prefab not found");
                return;
            }

            Undo.RecordObject(skill, "Configure ArrowShot");
            skill.projectileSpeed = 15f;
            // 手装備アタッチ: ArrowPivot (runtime で Weapon_Root_R 下に自動生成される empty)
            skill.handAttachmentPrefab = nock;
            skill.handAttachmentBoneName = "ArrowPivot";
            skill.handAttachmentOffset = Vector3.zero;
            // DownrainDC の矢は +X が tip。Y=-90 で tip を +Z に揃える + Z 軸 90→0 で構え→発射姿勢
            skill.handAttachmentEulerStart = new Vector3(0f, -90f, 90f);
            skill.handAttachmentEulerOffset = new Vector3(0f, -90f, 0f);
            skill.handAttachmentRotationEndTime = 0.182f;
            skill.flyRotationBlendTime = 0f; // nock 中に aim 完了してるので追加補間不要
            // Effect/視覚は AnimationEvent (ArrowEqOn/ArrowEqOff) 経由。timeline は DealDamage のみ。
            skill.timeline = new List<RealtimeSkillEvent>
            {
                new RealtimeSkillEvent
                {
                    timeSec = 0.55f, // ArrowEqOff が発火するフレーム相当 (アニメ中の放つ瞬間)
                    kind = RealtimeSkillEventKind.DealDamage,
                    effectPrefab = null,
                    spawnOrigin = RealtimeEffectOrigin.Self,
                    offset = Vector3.zero,
                    lifeSec = 0f,
                    attachBoneName = "",
                    label = "着弾 (距離自動補正)"
                }
            };
            EditorUtility.SetDirty(skill);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ArrowShotSetup] Configured {skillPath}: projectileSpeed=15, 3 events.");
            EditorGUIUtility.PingObject(skill);
        }
    }
}
#endif
