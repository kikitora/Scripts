// IconView.cs
// アイコン用の最小限コンポーネント。頻出UI要素（画像・ラベル・ボタン）への簡易Setterを提供します。
// これらの参照は任意設定です。未設定でも実行時エラーにはなりません。

using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
#if TMP_PRESENT
using TMPro;
#endif

public class IconView : MonoBehaviour
{
    [SerializeField] private Image image;   // アイコン画像（任意）

    [SerializeField] private TMP_Text label; // テキスト（任意, TextMeshPro）

    [SerializeField] private Button button; // クリック用ボタン（任意）

    /// <summary>
    /// 画像・テキスト・ボタンのリスナーなど、前回の状態をクリアして初期化します。
    /// </summary>
    public void ResetView()
    {
        if (image) image.sprite = null;
        if (label) label.text = string.Empty;
        if (button) button.onClick.RemoveAllListeners();
    }

    /// <summary>画像の差し替え</summary>
    public void SetImage(Sprite sprite)
    {
        if (image) image.sprite = sprite;
    }

    /// <summary>テキストの差し替え</summary>
    public void SetLabel(string text)
    {
        if (label) label.text = text ?? string.Empty;
    }

    /// <summary>クリック時の処理を登録（既存リスナは消去）</summary>
    public void SetOnClick(Action onClick)
    {
        if (!button) return;
        button.onClick.RemoveAllListeners();
        if (onClick != null) button.onClick.AddListener(() => onClick());
    }

    // 必要に応じて直接触りたい場合用の参照（高度な表現やバッジ付与など）
    public Image Image => image;
#if TMP_PRESENT
    public TMP_Text Label => label;
#else
    public TMP_Text Label => label;
#endif
    public Button Button => button;
}
