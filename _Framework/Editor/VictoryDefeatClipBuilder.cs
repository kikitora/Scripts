#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SteraCube._Framework.Editor
{
    /// <summary>
    /// Humanoid Muscle 値を時間軸で設定して、Victory / Defeat の簡易 AnimationClip を生成する。
    /// 品質: 棒立ち感ありの仮モーション。
    /// 後でモーキャプ品のクリップに差し替え可能。
    /// </summary>
    public static class VictoryDefeatClipBuilder
    {
        private const string OutputFolder = "Assets/0SteraCube/Animators/GeneratedClips";

        [MenuItem("Tools/SteraCube/Build Victory Defeat Clips")]
        public static void Build()
        {
            EnsureFolder(OutputFolder);

            BuildVictory();
            BuildDefeat();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[VictoryDefeatClipBuilder] Done.");
        }

        // ─────────────────────────────
        // Victory: 両腕を上に上げ、頭を少し上向け、胸を張る
        // ─────────────────────────────
        private static void BuildVictory()
        {
            var clip = new AnimationClip { name = "Victory_Generic", frameRate = 30f };

            // 0.0s: idle → 0.4s: 腕上げ → 2.2s: hold → 2.6s: 元に戻る

            // 両腕を上げる (Down-Up を +1 に)
            SetMuscle(clip, "Left Arm Down-Up",  0f, 0.4f, 2.2f, 2.6f, 0f, 1f, 1f, 0f);
            SetMuscle(clip, "Right Arm Down-Up", 0f, 0.4f, 2.2f, 2.6f, 0f, 1f, 1f, 0f);

            // 少し後ろに開く (Front-Back = 後ろに +)
            SetMuscle(clip, "Left Arm Front-Back",  0f, 0.4f, 2.2f, 2.6f, 0f, 0.3f, 0.3f, 0f);
            SetMuscle(clip, "Right Arm Front-Back", 0f, 0.4f, 2.2f, 2.6f, 0f, 0.3f, 0.3f, 0f);

            // 腕を外側に広げる (In-Out: +)
            SetMuscle(clip, "Left Arm Twist In-Out",  0f, 0.4f, 2.2f, 2.6f, 0f, 0.3f, 0.3f, 0f);
            SetMuscle(clip, "Right Arm Twist In-Out", 0f, 0.4f, 2.2f, 2.6f, 0f, 0.3f, 0.3f, 0f);

            // 頭を上に向ける
            SetMuscle(clip, "Head Nod Down-Up", 0f, 0.4f, 2.2f, 2.6f, 0f, 0.4f, 0.4f, 0f);

            // 胸を張る (Chest 後ろに反る)
            SetMuscle(clip, "Chest Front-Back", 0f, 0.4f, 2.2f, 2.6f, 0f, 0.3f, 0.3f, 0f);

            // ヒジを曲げる (Forearm Stretch: +1 で曲げた状態、-1 で伸び切り)
            // 腕を高く上げて少し曲げる
            SetMuscle(clip, "Left Forearm Stretch",  0f, 0.4f, 2.2f, 2.6f, 0f, 0.5f, 0.5f, 0f);
            SetMuscle(clip, "Right Forearm Stretch", 0f, 0.4f, 2.2f, 2.6f, 0f, 0.5f, 0.5f, 0f);

            // ループしない
            SetLoop(clip, false);

            var outPath = $"{OutputFolder}/Victory_Generic.anim";
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(outPath) != null)
                AssetDatabase.DeleteAsset(outPath);
            AssetDatabase.CreateAsset(clip, outPath);
            Debug.Log($"[VictoryDefeatClipBuilder] Built: {outPath}");
        }

        // ─────────────────────────────
        // Defeat: 頭を垂れ、肩を落とし、前かがみ、腕だらり
        // ─────────────────────────────
        private static void BuildDefeat()
        {
            var clip = new AnimationClip { name = "Defeat_Generic", frameRate = 30f };

            // 0.0s: idle → 0.6s: しょんぼり → 2.8s: hold

            // 頭を下に
            SetMuscle(clip, "Head Nod Down-Up", 0f, 0.6f, 2.8f, 2.8f, 0f, -0.7f, -0.7f, -0.7f);

            // 首も前に
            SetMuscle(clip, "Neck Nod Down-Up", 0f, 0.6f, 2.8f, 2.8f, 0f, -0.5f, -0.5f, -0.5f);

            // 肩を落とす (Shoulder Down-Up = -1 下げる)
            SetMuscle(clip, "Left Shoulder Down-Up",  0f, 0.6f, 2.8f, 2.8f, 0f, -0.6f, -0.6f, -0.6f);
            SetMuscle(clip, "Right Shoulder Down-Up", 0f, 0.6f, 2.8f, 2.8f, 0f, -0.6f, -0.6f, -0.6f);

            // 腕を前に垂らす
            SetMuscle(clip, "Left Arm Front-Back",  0f, 0.6f, 2.8f, 2.8f, 0f, -0.3f, -0.3f, -0.3f);
            SetMuscle(clip, "Right Arm Front-Back", 0f, 0.6f, 2.8f, 2.8f, 0f, -0.3f, -0.3f, -0.3f);

            // 腕を少し内側に
            SetMuscle(clip, "Left Arm Twist In-Out",  0f, 0.6f, 2.8f, 2.8f, 0f, -0.3f, -0.3f, -0.3f);
            SetMuscle(clip, "Right Arm Twist In-Out", 0f, 0.6f, 2.8f, 2.8f, 0f, -0.3f, -0.3f, -0.3f);

            // 前かがみ
            SetMuscle(clip, "Spine Front-Back", 0f, 0.6f, 2.8f, 2.8f, 0f, -0.4f, -0.4f, -0.4f);
            SetMuscle(clip, "Chest Front-Back", 0f, 0.6f, 2.8f, 2.8f, 0f, -0.3f, -0.3f, -0.3f);

            // ループしない (Defeat は保持)
            SetLoop(clip, false);

            var outPath = $"{OutputFolder}/Defeat_Generic.anim";
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(outPath) != null)
                AssetDatabase.DeleteAsset(outPath);
            AssetDatabase.CreateAsset(clip, outPath);
            Debug.Log($"[VictoryDefeatClipBuilder] Built: {outPath}");
        }

        /// <summary>
        /// 4つのキーフレーム (t0=0, t1=rise, t2=holdEnd, t3=end) で値 (v0, v1, v2, v3) を設定。
        /// </summary>
        private static void SetMuscle(AnimationClip clip, string muscle,
                                       float t0, float t1, float t2, float t3,
                                       float v0, float v1, float v2, float v3)
        {
            var curve = new AnimationCurve(
                new Keyframe(t0, v0),
                new Keyframe(t1, v1),
                new Keyframe(t2, v2),
                new Keyframe(t3, v3)
            );
            // タンジェントを滑らかに
            for (int i = 0; i < curve.keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
            }

            var binding = new EditorCurveBinding
            {
                path = "",
                propertyName = muscle,
                type = typeof(Animator),
            };
            AnimationUtility.SetEditorCurve(clip, binding, curve);
        }

        private static void SetLoop(AnimationClip clip, bool loop)
        {
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.loopBlend = loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            // Humanoid として認識させる: m_AnimationClipSettings.m_HasMotionFloatCurves=1
            // と m_HumanMotion=1 が必要 (Unity は muscle curves を格納するが、フラグがないと Generic 扱い)
            var so = new SerializedObject(clip);
            var humanMotion = so.FindProperty("m_AnimationClipSettings.m_HasMotionFloatCurves");
            if (humanMotion != null) humanMotion.intValue = 1;

            so.ApplyModifiedProperties();

            // もう一つ: 直接 m_HumanMotion プロパティがあれば設定
            SerializedProperty hm2 = so.FindProperty("m_HumanMotion");
            if (hm2 != null)
            {
                hm2.boolValue = true;
                so.ApplyModifiedProperties();
            }
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
