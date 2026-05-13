using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// Battle skill comments shown as stacked MiniWindow/MiniWindowEnemy entries.
    /// </summary>
    public class BattleCommentWindowFeed : MonoBehaviour
    {
        private const string RuntimeEntryPrefix = "[BattleComment] ";

        [Header("Prefabs")]
        public GameObject allyMiniWindowPrefab;
        public GameObject enemyMiniWindowPrefab;

        [Header("Parents")]
        public RectTransform allyParent;
        public RectTransform enemyParent;

        [Header("Behavior")]
        public bool useSceneTemplates = true;
        public bool createDefaultParentsIfMissing = false;
        public bool scrollToNewest = true;

#if UNITY_EDITOR
        public bool loadCopiedPrefabsInEditor = true;
#endif

        public void ShowSkillUse(int ownerSide, string message)
        {
            if (string.IsNullOrEmpty(message)) return;

            EnsureReady(ownerSide);

            RectTransform parent = ownerSide == 0 ? allyParent : enemyParent;
            GameObject prefab = ownerSide == 0 ? allyMiniWindowPrefab : enemyMiniWindowPrefab;
            if (parent == null || prefab == null) return;

            var go = Instantiate(prefab, parent);
            go.name = RuntimeEntryPrefix + (ownerSide == 0 ? "Ally" : "Enemy");
            go.SetActive(true);
            go.transform.SetAsFirstSibling();
            SetText(go, message);

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent);
            ScrollToNewest(parent);
        }

        private void EnsureReady(int ownerSide)
        {
            if (useSceneTemplates)
                FindSceneTemplates();

            FindScrollContentParent();

#if UNITY_EDITOR
            if (loadCopiedPrefabsInEditor)
            {
                if (allyMiniWindowPrefab == null)
                    allyMiniWindowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/0SteraCube/Copy/UI/BattleCommentWindow/Prefabs/MiniWindow.prefab");
                if (enemyMiniWindowPrefab == null)
                enemyMiniWindowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                        "Assets/0SteraCube/Copy/UI/BattleCommentWindow/Prefabs/MiniWindowEnemy.prefab");
            }
#endif
            if (!createDefaultParentsIfMissing) return;

            if ((ownerSide == 0 && allyParent != null) || (ownerSide != 0 && enemyParent != null)) return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            if (allyParent == null)
                allyParent = CreateParent(canvas.transform, "BattleCommentAllyParent", true);
            if (enemyParent == null)
                enemyParent = CreateParent(canvas.transform, "BattleCommentEnemyParent", false);
        }

        private void FindScrollContentParent()
        {
            if (allyParent != null && enemyParent != null) return;

            var scrollRects = FindObjectsByType<ScrollRect>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < scrollRects.Length; i++)
            {
                var scrollRect = scrollRects[i];
                if (scrollRect == null) continue;

                RectTransform content = scrollRect.content;
                if (content == null)
                {
                    var contentTransform = scrollRect.transform.Find("Viewport/Content");
                    content = contentTransform as RectTransform;
                }
                if (content == null) continue;

                if (allyParent == null) allyParent = content;
                if (enemyParent == null) enemyParent = content;
                return;
            }
        }

        private void FindSceneTemplates()
        {
            if (allyMiniWindowPrefab != null && enemyMiniWindowPrefab != null && allyParent != null && enemyParent != null)
                return;

            var rects = FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < rects.Length; i++)
            {
                var rt = rects[i];
                if (rt == null) continue;

                string objectName = rt.name;
                if (objectName.StartsWith(RuntimeEntryPrefix)) continue;

                if (objectName == "MiniWindow" && allyMiniWindowPrefab == null)
                {
                    allyMiniWindowPrefab = rt.gameObject;
                    allyParent = rt.parent as RectTransform;
                    rt.gameObject.SetActive(false);
                }
                else if (objectName == "MiniWindowEnemy" && enemyMiniWindowPrefab == null)
                {
                    enemyMiniWindowPrefab = rt.gameObject;
                    enemyParent = rt.parent as RectTransform;
                    rt.gameObject.SetActive(false);
                }
            }

            if (allyParent == null && allyMiniWindowPrefab != null)
                allyParent = allyMiniWindowPrefab.transform.parent as RectTransform;
            if (enemyParent == null && enemyMiniWindowPrefab != null)
                enemyParent = enemyMiniWindowPrefab.transform.parent as RectTransform;
        }

        private RectTransform CreateParent(Transform canvasTransform, string objectName, bool allySide)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(canvasTransform, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = allySide ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
            rt.anchorMax = rt.anchorMin;
            rt.pivot = allySide ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
            rt.anchoredPosition = allySide ? new Vector2(20f, -90f) : new Vector2(-20f, -90f);
            rt.sizeDelta = new Vector2(1000f, 0f);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = allySide ? TextAnchor.UpperLeft : TextAnchor.UpperRight;
            layout.spacing = 4f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return rt;
        }

        private void SetText(GameObject go, string message)
        {
            var text = go.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null) text.text = message;
        }

        private void ScrollToNewest(RectTransform parent)
        {
            if (!scrollToNewest || parent == null) return;

            var current = parent.transform;
            while (current != null)
            {
                var scrollRect = current.GetComponent<ScrollRect>();
                if (scrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    scrollRect.verticalNormalizedPosition = 1f;
                    return;
                }
                current = current.parent;
            }
        }
    }
}
