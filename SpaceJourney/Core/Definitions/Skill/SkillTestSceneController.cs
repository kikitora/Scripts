// SkillTestSceneController.cs
// このクラスで何をするか：
// スキル挙動確認用の簡易テストシーンの制御を行います。
// - インスペクターから SkillDefinition の配列を登録
// - 数字キー 1〜9 で現在選択スキルを切り替え
// - Sceneビュー上に targetRange / effectRange をギズモとして描画
// - スペースキーでスキルアニメーション（animatorTriggerKey）を再生
// 本番のバトルロジックとは切り離された、デバッグ専用のテスト用クラスです。

using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SteraCube.SpaceJourney
{
    public class SkillTestSceneController : MonoBehaviour
    {
        [Header("テスト対象スキル")]
        [Tooltip("挙動を確認したい SkillDefinition をここに並べる。")]
        public SkillDefinition[] skills;

        [Header("キャラ配置")]
        [Tooltip("グリッドの (0,0) として扱うキャラの原点位置。")]
        public Transform casterOrigin;

        [Tooltip("キャラに付いている UnitAnimationController。")]
        public UnitAnimationController casterAnimation;

        [Header("グリッド表示設定")]
        [Tooltip("1マスあたりのサイズ（ワールド座標）。")]
        public float cellSize = 1.0f;

        [Tooltip("ターゲット選択範囲の色。")]
        public Color selectionColor = Color.yellow;

        [Tooltip("効果範囲の色。")]
        public Color effectColor = Color.red;

        [Header("現在選択中のスキル")]
        [Tooltip("現在どのスキルを表示/再生対象にするかのインデックス。")]
        public int currentSkillIndex = 0;

        private void Update()
        {
            // 数字キー 1〜9 でスキル切り替え（存在する範囲のみ）
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    if (skills != null && i < skills.Length)
                    {
                        currentSkillIndex = i;
                        Debug.Log($"[SkillTest] 選択スキルを {skills[i].name} (index {i}) に変更しました。");
                    }
                }
            }

            // スペースでスキルアニメ再生
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PlaySkillAnimation();
            }
        }

        private void PlaySkillAnimation()
        {
            if (skills == null || skills.Length == 0)
            {
                Debug.LogWarning("[SkillTest] skills が設定されていません。");
                return;
            }

            if (currentSkillIndex < 0 || currentSkillIndex >= skills.Length)
            {
                Debug.LogWarning("[SkillTest] currentSkillIndex が範囲外です。");
                return;
            }

            if (casterAnimation == null || casterAnimation.Animator == null)
            {
                Debug.LogWarning("[SkillTest] casterAnimation / Animator が設定されていません。");
                return;
            }

            var skill = skills[currentSkillIndex];

            if (string.IsNullOrEmpty(skill.AnimatorTriggerKey))
            {
                Debug.Log($"[SkillTest] {skill.name} は animatorTriggerKey が未設定です。アニメ再生はスキップします。");
                return;
            }

            casterAnimation.Animator.ResetTrigger(skill.AnimatorTriggerKey);
            casterAnimation.Animator.SetTrigger(skill.AnimatorTriggerKey);

            Debug.Log($"[SkillTest] {skill.name} のアニメーショントリガー `{skill.AnimatorTriggerKey}` を発火しました。");
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (skills == null || skills.Length == 0)
                return;

            if (currentSkillIndex < 0 || currentSkillIndex >= skills.Length)
                return;

            if (casterOrigin == null)
                return;

            var skill = skills[currentSkillIndex];

            // シーンビューで前後関係がわかりやすいように zTest を設定
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            // ターゲット選択範囲（黄色）
            if (skill.targetRange != null && skill.targetRange.Offsets != null)
            {
                Gizmos.color = selectionColor;
                DrawGridOffsets(skill.targetRange, casterOrigin.position);
            }

            // 効果範囲（赤）
            if (skill.effectRange != null && skill.effectRange.Offsets != null)
            {
                Gizmos.color = effectColor;
                DrawGridOffsets(skill.effectRange, casterOrigin.position);
            }
        }

        private void DrawGridOffsets(GridRangePattern pattern, Vector3 originWorldPos)
        {
            if (pattern.Offsets == null)
                return;

            foreach (var offset in pattern.Offsets)
            {
                // 上が前方(y負方向)という前提で Z を -offset.y として変換
                Vector3 center = originWorldPos
                                 + new Vector3(offset.x * cellSize, 0f, -offset.y * cellSize);

                Vector3 size = new Vector3(cellSize * 0.9f, 0.01f, cellSize * 0.9f);

                Gizmos.DrawCube(center, size);
            }
        }
#endif
    }
}
