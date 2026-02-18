// ArrangeIcons.cs
// このスクリプトは「アイコンを並べるコンテンツ用のGameObject」に取り付けて使います。
// List<SoulData>, List<BodyData> など “型が異なる複数のデータ一覧” を 1 本の仕組みで扱い、
// それぞれのデータをアイコンPrefabへとバインド（適用）する関数を登録して並べます。
// ※ IconView は別ファイル IconView.cs に分離してあります（同プロジェクト内ならOK）。
//
// ◆基本の考え方
// - データの列挙：何の型でもOK（IEnumerable<T>）
// - 見た目への適用：IconView + バインダー(Action<IconView, T>) を渡す
//   → AddSource(data, (view, item) => { view.SetLabel(...); view.SetImage(...); view.SetOnClick(...); });
// - Refresh()：登録済みすべてのソースを順番に生成・バインドして並べる
//
// ◆使用例（別スクリプトから）
//   arranger.ClearSources()
//       .AddSource(soulList, (view, soul) => {
//           view.SetLabel(soul.displayName);
//           view.SetImage(soul.icon);
//           view.SetOnClick(() => Debug.Log($"Soul: {soul.id}"));
//       })
//       .AddSource(bodyList, (view, body) => {
//           view.SetLabel(body.displayName);
//           view.SetImage(body.iconSprite);
//           view.SetOnClick(() => SelectBody(body));
//       });
//   arranger.Refresh();
//
// ◆前提
// - IconPrefab には IconView コンポーネントを付けておくこと（Image/TMP_Text/Button は任意）
// - contentRoot には Grid/Vertical/Horizontal Layout Group などを付けておくと自動整列できます

using System;
using System.Collections.Generic;
using UnityEngine;

public class ArrangeIcons : MonoBehaviour
{
    [Header("ターゲットとプレハブ")]
    [SerializeField] private Transform contentRoot;    // アイコンを並べる親。レイアウト用のTransformを指定
    [SerializeField] private GameObject iconPrefab;    // IconView を持つアイコン用プレハブ

    [Header("挙動設定")]
    [SerializeField] private bool clearOnRefresh = true;   // Refresh 時に既存の子（アイコン）を破棄してから再構築する
    [SerializeField] private bool refreshOnEnable = true;  // このオブジェクトが有効化されたとき自動で再構築する
    [Tooltip("true なら生成直後に SetActive(true) まで行います。false なら手動で有効化できます。")]
    [SerializeField] private bool activateOnCreate = true;

    // 登録されたソース（異種混在OK）。各ソースは列挙とバインド方法を保持。
    private readonly List<ISource> _sources = new();

    #region パブリックAPI --------------------------------------------------

    /// <summary>
    /// これまでに登録したデータソースをすべて解除します（アイコン破棄は Refresh / ClearVisuals 時に実施）。
    /// </summary>
    public ArrangeIcons ClearSources()
    {
        _sources.Clear();
        return this;
    }

    /// <summary>
    /// 任意の列挙可能データと、そのデータを IconView に適用するバインダーを登録します。
    /// </summary>
    public ArrangeIcons AddSource<T>(IEnumerable<T> data, Action<IconView, T> binder)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (binder == null) throw new ArgumentNullException(nameof(binder));
        _sources.Add(new Source<T>(data, binder));
        return this;
    }

    /// <summary>
    /// （必要に応じて）既存アイコンを消去し、登録済みすべてのソースからアイコンを再構築します。
    /// </summary>
    public void Refresh()
    {
        if (contentRoot == null || iconPrefab == null)
        {
            Debug.LogWarning("ArrangeIcons: contentRoot か iconPrefab が未設定です。");
            return;
        }

        if (clearOnRefresh)
        {
            ClearAllChildren();
        }

        foreach (var src in _sources)
        {
            src.Populate(contentRoot, iconPrefab, activateOnCreate);
        }
    }

    /// <summary>
    /// 表示上のアイコン（contentRoot の子）をすべて破棄します。ソース登録は維持されます。
    /// </summary>
    public void ClearVisuals()
    {
        ClearAllChildren();
    }

    #endregion -------------------------------------------------------------

    private void OnEnable()
    {
        if (refreshOnEnable)
        {
            Refresh();
        }
    }

    /// <summary>
    /// contentRoot の直下にある子 GameObject をすべて破棄。
    /// エディタ上（非実行時）は Undo 付きで即時破棄します。
    /// </summary>
    private void ClearAllChildren()
    {
        if (contentRoot == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                var c = contentRoot.GetChild(i);
                if (c != null) UnityEditor.Undo.DestroyObjectImmediate(c.gameObject);
            }
            return;
        }
#endif
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
        {
            var c = contentRoot.GetChild(i);
            if (c != null) Destroy(c.gameObject);
        }
    }

    // -------------------- ソース実装まわり -------------------------------
    private interface ISource
    {
        void Populate(Transform parent, GameObject prefab, bool activate);
    }

    /// <summary>
    /// ジェネリックなデータソース実体。列挙 and バインドの橋渡し。
    /// </summary>
    private sealed class Source<T> : ISource
    {
        private readonly IEnumerable<T> _data;
        private readonly Action<IconView, T> _binder;

        public Source(IEnumerable<T> data, Action<IconView, T> binder)
        {
            _data = data;
            _binder = binder;
        }

        public void Populate(Transform parent, GameObject prefab, bool activate)
        {
            if (_data == null) return;
            foreach (var item in _data)
            {
                var go = UnityEngine.Object.Instantiate(prefab, parent);
                if (!activate) go.SetActive(false);

                var view = go.GetComponent<IconView>();
                if (view == null)
                {
                    Debug.LogError("ArrangeIcons: iconPrefab に IconView コンポーネントが見つかりません。");
                    continue;
                }

                // バインド前に毎回リセット（前回のリスナや表示の残りを消す）
                view.ResetView();

                // 呼び出し元が渡したバインド関数で適用
                _binder?.Invoke(view, item);

                if (activate) go.SetActive(true);
            }
        }
    }
}
