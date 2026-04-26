using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// ダメージ表示ポップアップの生成管理。RealtimeBattleManager に紐付けて使う。
    /// 外部から Spawn() を呼ぶだけで、プレハブ未設定時はフォールバックで動的生成。
    /// </summary>
    public class DamagePopupSpawner : MonoBehaviour
    {
        [Tooltip("生成するポップアッププレハブ (DamagePopup 付き)。null でも簡易フォールバックで動作")]
        public DamagePopup prefab;

        [Tooltip("被弾キャラ位置からの加算オフセット")]
        public Vector3 offset = new Vector3(0f, 2.0f, 0f);

        [Header("色設定")]
        public Color damageColor = Color.white;
        public Color critColor = new Color(1f, 0.75f, 0f);    // 濃い黄
        public Color healColor = new Color(0.2f, 1f, 0.3f);   // 濃い緑
        public Color magicColor = new Color(0.3f, 0.6f, 1f);  // 濃い青

        public enum PopupKind { Damage, Crit, Heal, Magic }

        public void Spawn(Vector3 worldPos, int value, PopupKind kind = PopupKind.Damage)
        {
            Color color = kind switch
            {
                PopupKind.Crit => critColor,
                PopupKind.Heal => healColor,
                PopupKind.Magic => magicColor,
                _ => damageColor,
            };
            Spawn(worldPos, value, color);
        }

        public void Spawn(Vector3 worldPos, int value, Color color)
        {
            Vector3 pos = worldPos + offset;
            DamagePopup popup;
            if (prefab != null)
            {
                popup = Instantiate(prefab, pos, Quaternion.identity);
            }
            else
            {
                // フォールバック: 動的に TextMeshPro を組み立て
                popup = CreateFallbackPopup(pos);
            }
            popup.Setup(value, color);
        }

        private static DamagePopup CreateFallbackPopup(Vector3 pos)
        {
            var root = new GameObject("DamagePopup(Dynamic)");
            root.transform.position = pos;
            var popup = root.AddComponent<DamagePopup>();
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(root.transform, false);
            var tmp = textGo.AddComponent<TMPro.TextMeshPro>();
            tmp.fontSize = 4f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.text = "";
            tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = Color.black;
            // DamagePopup 側の text 参照をセット
            var field = typeof(DamagePopup).GetField("text",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(popup, tmp);
            return popup;
        }

#if UNITY_EDITOR
        /// <summary>DamagePopup プレハブを生成するエディタユーティリティ</summary>
        [UnityEditor.MenuItem("Tools/SteraCube/Create Damage Popup Prefab")]
        private static void CreatePrefab()
        {
            const string path = "Assets/0SteraCube/Prefabs/UI/DamagePopup.prefab";
            var dir = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            if (!UnityEditor.AssetDatabase.IsValidFolder(dir))
            {
                var parent = System.IO.Path.GetDirectoryName(dir).Replace("\\", "/");
                if (!UnityEditor.AssetDatabase.IsValidFolder(parent))
                    UnityEditor.AssetDatabase.CreateFolder(System.IO.Path.GetDirectoryName(parent).Replace("\\", "/"),
                        System.IO.Path.GetFileName(parent));
                UnityEditor.AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(dir));
            }

            var root = new GameObject("DamagePopup");
            var popup = root.AddComponent<DamagePopup>();
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(root.transform, false);
            var tmp = textGo.AddComponent<TMPro.TextMeshPro>();
            tmp.fontSize = 4f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.text = "0";
            tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
            tmp.outlineWidth = 0.2f;
            tmp.outlineColor = Color.black;
            tmp.fontStyle = TMPro.FontStyles.Bold;

            var field = typeof(DamagePopup).GetField("text",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            field?.SetValue(popup, tmp);

            var prefabAsset = UnityEditor.PrefabUtility.SaveAsPrefabAsset(root, path);
            DestroyImmediate(root);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"[DamagePopupSpawner] Created prefab: {path}");
            UnityEditor.EditorGUIUtility.PingObject(prefabAsset);
        }
#endif
    }
}
