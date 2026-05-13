using UnityEngine;
using TMPro;

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
        [Tooltip("ダメージ数値 / GUARD / 状態異常名ポップアップに使う TMP Font Asset。未設定ならプレハブ側のフォントを使う")]
        public TMP_FontAsset popupFont;

        [Header("表示位置")]
        [Tooltip("被弾キャラ位置からの加算オフセット")]
        public Vector3 offset = new Vector3(0f, 0.55f, 0f);

        [Tooltip("GUARD や状態異常名など、文字ポップアップだけをダメージ数値より上にずらす追加オフセット")]
        public Vector3 textOffset = new Vector3(0f, 0.12f, 0f);

        [Tooltip("ON にすると、ダメージ数値 / GUARD 文字がキャラの体に隠れないように前面描画します")]
        public bool renderOnTop = true;

        [Tooltip("ポップアップをカメラ手前方向へずらす距離。低い位置で体に隠れる時は少し増やす")]
        public float cameraForwardOffset = 0.06f;

        [Tooltip("ポップアップの上昇速度。低いほど誰のダメージか追いやすい")]
        public float popupRiseSpeed = 0.45f;

        [Tooltip("ポップアップの表示秒数")]
        public float popupLifeSec = 0.95f;

        [Header("色設定")]
        public Color damageColor = Color.white;
        public Color critColor = new Color(1f, 0.75f, 0f);    // 濃い黄
        public Color healColor = new Color(0.2f, 1f, 0.3f);   // 濃い緑
        public Color magicColor = new Color(0.3f, 0.6f, 1f);  // 濃い青
        public Color buffColor = new Color(0.4f, 1f, 0.55f);
        public Color debuffColor = new Color(1f, 0.35f, 0.35f);
        public Color guardColor = new Color(0.55f, 0.85f, 1f);

        public enum PopupKind { Damage, Crit, Heal, Magic, Buff, Debuff, Guard }

        public void Spawn(Vector3 worldPos, int value, PopupKind kind = PopupKind.Damage)
        {
            Color color = kind switch
            {
                PopupKind.Crit => critColor,
                PopupKind.Heal => healColor,
                PopupKind.Magic => magicColor,
                PopupKind.Buff => buffColor,
                PopupKind.Debuff => debuffColor,
                PopupKind.Guard => guardColor,
                _ => damageColor,
            };
            Spawn(worldPos, value, color);
        }

        public void SpawnText(Vector3 worldPos, string text, PopupKind kind)
        {
            Color color = kind switch
            {
                PopupKind.Crit => critColor,
                PopupKind.Heal => healColor,
                PopupKind.Magic => magicColor,
                PopupKind.Buff => buffColor,
                PopupKind.Debuff => debuffColor,
                PopupKind.Guard => guardColor,
                _ => damageColor,
            };
            SpawnText(worldPos, text, color);
        }

        public void SpawnText(Vector3 worldPos, string text, Color color)
        {
            Vector3 pos = ResolvePopupPosition(worldPos + offset + textOffset);
            DamagePopup popup;
            if (prefab != null)
            {
                popup = Instantiate(prefab, pos, Quaternion.identity);
            }
            else
            {
                popup = CreateFallbackPopup(pos);
            }
            ApplyStyle(popup);
            popup.Setup(text, color);
        }

        public void Spawn(Vector3 worldPos, int value, Color color)
        {
            Vector3 pos = ResolvePopupPosition(worldPos + offset);
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
            ApplyStyle(popup);
            popup.Setup(value, color);
        }

        private void ApplyStyle(DamagePopup popup)
        {
            if (popup == null) return;
            popup.ApplyFont(popupFont);
            popup.ApplyMotion(popupRiseSpeed, popupLifeSec);
            popup.SetRenderOnTop(renderOnTop);
        }

        private Vector3 ResolvePopupPosition(Vector3 basePos)
        {
            if (!renderOnTop || cameraForwardOffset <= 0f) return basePos;

            var cam = Camera.main;
            if (cam == null) return basePos;

            Vector3 toCamera = cam.transform.position - basePos;
            if (toCamera.sqrMagnitude < 0.0001f) return basePos;
            return basePos + toCamera.normalized * cameraForwardOffset;
        }

        private static DamagePopup CreateFallbackPopup(Vector3 pos)
        {
            var root = new GameObject("DamagePopup(Dynamic)");
            root.transform.position = pos;
            var popup = root.AddComponent<DamagePopup>();
            var textGo = new GameObject("Text");
            textGo.transform.SetParent(root.transform, false);
            var tmp = textGo.AddComponent<TMPro.TextMeshPro>();
            tmp.fontSize = 2.7f;
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
            tmp.fontSize = 2.7f;
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
