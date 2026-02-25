#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SteraCube.SpaceJourney.DevTools
{
    /// <summary>
    /// MasterDatabase 登録IDを AssetDatabase から読み取り、
    /// EditorWindow / PropertyDrawer でドロップダウン表示するための静的ヘルパー。
    /// MasterDatabase の Scene への配置不要（エディタ専用）。
    /// </summary>
    public static class CharaDataIdFields
    {
        // ── IDキャッシュ ─────────────────────────────────────────
        private static string[] _raceIds;
        private static string[] _bodyJobIds;
        private static string[] _weaponIds;
        private static string[] _soulJobIds;
        private static string[] _skillIds;
        private static double _lastRefresh = -1;

        // ── リフレッシュ ─────────────────────────────────────────
        /// <summary>5秒キャッシュ付きリフレッシュ。</summary>
        public static void RefreshIfNeeded()
        {
            if (_lastRefresh >= 0 && EditorApplication.timeSinceStartup - _lastRefresh < 5.0) return;
            Refresh();
        }

        public static void Refresh()
        {
            _raceIds = LoadIds<RaceDefinition>(r => r.raceId);
            _bodyJobIds = LoadIds<BodyJobDefinition>(b => b.bodyJobId);
            _weaponIds = LoadIds<WeaponDefinition>(w => w.weaponId);
            _soulJobIds = LoadIds<SoulJobDefinition>(s => s.JobId);
            _skillIds = LoadIds<SkillDefinition>(s => s.SkillId);
            _lastRefresh = EditorApplication.timeSinceStartup;
        }

        private static string[] LoadIds<T>(Func<T, string> getId) where T : ScriptableObject
            => AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null)
                .Select(a => { try { return getId(a); } catch { return null; } })
                .Where(id => !string.IsNullOrEmpty(id))
                .OrderBy(id => id)
                .ToArray();

        // ── 公開アクセサ ─────────────────────────────────────────
        public static string[] RaceIds { get { RefreshIfNeeded(); return _raceIds ?? Array.Empty<string>(); } }
        public static string[] BodyJobIds { get { RefreshIfNeeded(); return _bodyJobIds ?? Array.Empty<string>(); } }
        public static string[] WeaponIds { get { RefreshIfNeeded(); return _weaponIds ?? Array.Empty<string>(); } }
        public static string[] SoulJobIds { get { RefreshIfNeeded(); return _soulJobIds ?? Array.Empty<string>(); } }
        public static string[] SkillIds { get { RefreshIfNeeded(); return _skillIds ?? Array.Empty<string>(); } }

        // ── SerializedProperty 対応表（EditorWindow 用） ─────────
        /// <summary>
        /// プロパティ名から適切なIDドロップダウンを描画する。
        /// 対応していない場合は false を返す（通常の PropertyField を使うこと）。
        /// </summary>
        public static bool TryDrawDropdown(SerializedProperty prop)
        {
            RefreshIfNeeded();

            if (prop.propertyType != SerializedPropertyType.String) return false;

            switch (prop.name)
            {
                case "raceId": return DrawDropdown(prop, RaceIds, "Race ID");
                case "bodyJobId": return DrawDropdown(prop, BodyJobIds, "Body Job ID");
                case "weaponId": return DrawDropdown(prop, WeaponIds, "Weapon ID");
                case "jobId": return DrawDropdown(prop, SoulJobIds, "Soul Job ID");
            }
            return false;
        }

        /// <summary>
        /// ドロップダウン描画共通。EditorGUILayout 版（EditorWindow / DrawSOFields 内用）。
        /// </summary>
        public static bool DrawDropdown(SerializedProperty prop, string[] ids, string label)
        {
            string current = prop.stringValue ?? "";

            if (ids.Length == 0)
            {
                // 定義が1件も見つからない → テキストフィールドにフォールバック
                EditorGUILayout.PropertyField(prop, new GUIContent($"{label}  ⚠定義なし"));
                return true;
            }

            string[] options = PrependNone(ids);
            int selectedIdx = FindIndex(ids, current);

            using (new EditorGUILayout.HorizontalScope())
            {
                int newIdx = EditorGUILayout.Popup(new GUIContent(label), selectedIdx, options);
                if (newIdx != selectedIdx)
                    prop.stringValue = newIdx == 0 ? "" : ids[newIdx - 1];

                // 現在値が登録済みIDに存在しない場合に警告アイコン
                if (!string.IsNullOrEmpty(current) && selectedIdx == 0)
                {
                    var c = GUI.color; GUI.color = new Color(1f, 0.65f, 0.2f);
                    EditorGUILayout.LabelField($"⚠ \"{current}\" 未登録", EditorStyles.miniLabel, GUILayout.Width(110));
                    GUI.color = c;
                }
            }
            return true;
        }

        // ── EditorGUI 版（PropertyDrawer 内の固定高さ描画用） ────
        public static void DrawDropdownGUI(Rect rect, SerializedProperty prop, string[] ids, GUIContent label)
        {
            string current = prop.stringValue ?? "";
            if (ids.Length == 0)
            {
                EditorGUI.PropertyField(rect, prop, new GUIContent(label.text + " ⚠定義なし"));
                return;
            }

            string[] options = PrependNone(ids);
            int selectedIdx = FindIndex(ids, current);

            float warnW = (!string.IsNullOrEmpty(current) && selectedIdx == 0) ? 120f : 0f;
            Rect popupRect = new Rect(rect.x, rect.y, rect.width - warnW, rect.height);

            int newIdx = EditorGUI.Popup(popupRect, label.text, selectedIdx, options);
            if (newIdx != selectedIdx)
                prop.stringValue = newIdx == 0 ? "" : ids[newIdx - 1];

            if (warnW > 0f)
            {
                var warnRect = new Rect(rect.xMax - warnW, rect.y, warnW, rect.height);
                var c = GUI.color; GUI.color = new Color(1f, 0.65f, 0.2f);
                EditorGUI.LabelField(warnRect, $"⚠\"{current}\"未登録", EditorStyles.miniLabel);
                GUI.color = c;
            }
        }

        // ── スキルリスト（EditorGUILayout 版） ───────────────────
        /// <summary>learnedSkillIds 等の string list を EditorGUILayout でスキルドロップダウンとして描画。</summary>
        public static void DrawSkillIdList(SerializedProperty listProp, string listLabel = "Learned Skills")
        {
            RefreshIfNeeded();
            var ids = SkillIds;
            string[] options = PrependNone(ids);

            EditorGUILayout.LabelField(listLabel, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            int count = listProp.arraySize;
            int removeAt = -1;

            for (int i = 0; i < count; i++)
            {
                var elem = listProp.GetArrayElementAtIndex(i);
                string cur = elem.stringValue ?? "";
                string[] opt = PrependNone(ids);
                int selIdx = FindIndex(ids, cur);

                using (new EditorGUILayout.HorizontalScope())
                {
                    int newIdx = EditorGUILayout.Popup($"[{i}]", selIdx, opt);
                    if (newIdx != selIdx)
                        elem.stringValue = newIdx == 0 ? "" : ids[newIdx - 1];

                    var oldBg = GUI.backgroundColor;
                    GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
                    if (GUILayout.Button("✕", GUILayout.Width(20))) removeAt = i;
                    GUI.backgroundColor = oldBg;
                }
            }

            if (removeAt >= 0)
                listProp.DeleteArrayElementAtIndex(removeAt);

            EditorGUI.indentLevel--;

            if (GUILayout.Button("＋ スキル追加", GUILayout.Height(18)))
            {
                listProp.InsertArrayElementAtIndex(count);
                listProp.GetArrayElementAtIndex(count).stringValue = "";
            }
        }

        // ── ユーティリティ ───────────────────────────────────────
        private static string[] PrependNone(string[] ids)
        {
            var result = new string[ids.Length + 1];
            result[0] = "─ (なし) ─";
            Array.Copy(ids, 0, result, 1, ids.Length);
            return result;
        }

        /// <summary>ids内のcurrentのインデックス + 1（(なし)分）を返す。見つからない場合は0。</summary>
        public static int FindIndex(string[] ids, string current)
        {
            if (string.IsNullOrEmpty(current)) return 0;
            int i = Array.IndexOf(ids, current);
            return i >= 0 ? i + 1 : 0;
        }
    }
}
#endif