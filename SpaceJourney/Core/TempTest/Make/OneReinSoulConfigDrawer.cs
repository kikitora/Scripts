#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SteraCube.SpaceJourney.DevTools
{
    /// <summary>
    /// OneReinSoulConfig の PropertyDrawer。
    /// jobId → SoulJobDefinition ドロップダウン
    /// learnedSkillIds → SkillDefinition ドロップダウンリスト
    /// その他のフィールドは標準描画。
    /// </summary>
    [CustomPropertyDrawer(typeof(OneReinSoulConfig))]
    public class OneReinSoulConfigDrawer : PropertyDrawer
    {
        // 1行の高さ
        private const float LineH = 18f;
        private const float LineP = 2f;   // 行間
        private const float Step = LineH + LineP;
        private const float BtnH = 20f;

        // ── GetPropertyHeight ────────────────────────────────────
        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
        {
            if (!prop.isExpanded)
                return EditorGUI.GetPropertyHeight(prop, label, false); // foldout 行のみ

            // 固定フィールド数（rank / growthType / jobId / title / level / lv1Stats）
            int fixedLines = 6;

            // lv1Stats array（展開時 + 1 line per element）
            var lv1StatsProp = prop.FindPropertyRelative("lv1Stats");
            float lv1StatsH = EditorGUI.GetPropertyHeight(lv1StatsProp, true);

            // learnedSkillIds リスト
            var skillsProp = prop.FindPropertyRelative("learnedSkillIds");
            int skillCount = skillsProp != null ? skillsProp.arraySize : 0;
            float skillsH = Step // ラベル
                          + skillCount * Step
                          + BtnH // ＋ボタン
                          + LineP * 2;

            return Step                 // foldout ヘッダー
                 + (fixedLines - 1) * Step // rank?level
                 + lv1StatsH + LineP    // lv1Stats
                 + skillsH;             // learnedSkillIds
        }

        // ── OnGUI ────────────────────────────────────────────────
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            EditorGUI.BeginProperty(pos, label, prop);

            // Foldout ヘッダー
            var foldRect = new Rect(pos.x, pos.y, pos.width, LineH);
            prop.isExpanded = EditorGUI.Foldout(foldRect, prop.isExpanded, label, true);

            if (!prop.isExpanded)
            {
                EditorGUI.EndProperty();
                return;
            }

            EditorGUI.indentLevel++;

            float y = pos.y + Step;

            // rank
            y = DrawField(pos, y, prop, "rank");
            // growthType
            y = DrawField(pos, y, prop, "growthType");
            // jobId → ドロップダウン
            y = DrawJobIdDropdown(pos, y, prop);
            // title
            y = DrawField(pos, y, prop, "title");
            // level
            y = DrawField(pos, y, prop, "level");
            // lv1Stats
            y = DrawLv1Stats(pos, y, prop);
            // learnedSkillIds
            y = DrawSkillIdList(pos, y, prop);

            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }

        // ── 各フィールド描画ヘルパー ─────────────────────────────

        private float DrawField(Rect pos, float y, SerializedProperty parent, string name)
        {
            var child = parent.FindPropertyRelative(name);
            if (child == null) return y;
            float h = EditorGUI.GetPropertyHeight(child, true);
            EditorGUI.PropertyField(Rect(pos, y, h), child, true);
            return y + h + LineP;
        }

        private float DrawJobIdDropdown(Rect pos, float y, SerializedProperty parent)
        {
            var jobIdProp = parent.FindPropertyRelative("jobId");
            if (jobIdProp == null) return y;

            CharaDataIdFields.RefreshIfNeeded();
            var ids = CharaDataIdFields.SoulJobIds;
            CharaDataIdFields.DrawDropdownGUI(Rect(pos, y, LineH), jobIdProp, ids, new GUIContent("Soul Job ID"));
            return y + Step;
        }

        private float DrawLv1Stats(Rect pos, float y, SerializedProperty parent)
        {
            var lv1Prop = parent.FindPropertyRelative("lv1Stats");
            if (lv1Prop == null) return y;
            float h = EditorGUI.GetPropertyHeight(lv1Prop, true);
            EditorGUI.PropertyField(Rect(pos, y, h), lv1Prop, true);
            return y + h + LineP;
        }

        private float DrawSkillIdList(Rect pos, float y, SerializedProperty parent)
        {
            var skillsProp = parent.FindPropertyRelative("learnedSkillIds");
            if (skillsProp == null) return y;

            CharaDataIdFields.RefreshIfNeeded();
            var ids = CharaDataIdFields.SkillIds;
            string[] options = PrependNone(ids);

            // ラベル
            EditorGUI.LabelField(Rect(pos, y, LineH), "Learned Skills", EditorStyles.boldLabel);
            y += Step;

            EditorGUI.indentLevel++;
            int count = skillsProp.arraySize;
            int removeAt = -1;

            for (int i = 0; i < count; i++)
            {
                var elem = skillsProp.GetArrayElementAtIndex(i);
                string cur = elem.stringValue ?? "";
                int selIdx = CharaDataIdFields.FindIndex(ids, cur);

                var rowRect = Rect(pos, y, LineH);
                // ?ボタン分を右端に確保
                var popRect = new Rect(rowRect.x, rowRect.y, rowRect.width - 24f, rowRect.height);
                var btnRect = new Rect(rowRect.xMax - 22f, rowRect.y, 20f, rowRect.height);

                int newIdx = EditorGUI.Popup(popRect, $"[{i}]", selIdx, options);
                if (newIdx != selIdx)
                    elem.stringValue = newIdx == 0 ? "" : ids[newIdx - 1];

                if (GUI.Button(btnRect, "?"))
                    removeAt = i;

                y += Step;
            }

            if (removeAt >= 0)
                skillsProp.DeleteArrayElementAtIndex(removeAt);

            EditorGUI.indentLevel--;

            // ＋ボタン
            var addRect = Rect(pos, y, BtnH);
            if (GUI.Button(addRect, "＋ スキル追加"))
            {
                skillsProp.InsertArrayElementAtIndex(count);
                skillsProp.GetArrayElementAtIndex(count).stringValue = "";
            }
            y += BtnH + LineP * 2;

            return y;
        }

        // ── ユーティリティ ────────────────────────────────────────
        private static Rect Rect(Rect pos, float y, float h)
            => new Rect(pos.x, y, pos.width, h);

        private static string[] PrependNone(string[] ids)
        {
            var result = new string[ids.Length + 1];
            result[0] = "─ (なし) ─";
            System.Array.Copy(ids, 0, result, 1, ids.Length);
            return result;
        }
    }
}
#endif