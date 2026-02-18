#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// CommentUI.UISlot 用 PropertyDrawer
[CustomPropertyDrawer(typeof(CommentUI.UISlot))]
public class UISlotDrawer : PropertyDrawer
{
    const float PAD = 2f;
    const float LINE = 1f;

    // 各プロパティ名
    const string PROP_NAME = "name";
    const string PROP_KIND = "kind";
    const string PROP_PARENT = "parent";
    const string PROP_HEADER = "headerText";
    const string PROP_BODY = "bodyText";
    const string PROP_IMAGE = "image";

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // フォールドアウト管理
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, 14f, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            GUIContent.none
        );

        // 1行目：Name（フル幅、ラベル幅固定で余白を作らない）
        float y = position.y;
        float lh = EditorGUIUtility.singleLineHeight;

        float labelWidthBackup = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 60f; // Name/Kind/Parent等のラベル幅

        var nameProp = property.FindPropertyRelative(PROP_NAME);
        var kindProp = property.FindPropertyRelative(PROP_KIND);
        var parent = property.FindPropertyRelative(PROP_PARENT);
        var header = property.FindPropertyRelative(PROP_HEADER);
        var body = property.FindPropertyRelative(PROP_BODY);
        var image = property.FindPropertyRelative(PROP_IMAGE);

        // Name 行
        var nameRect = new Rect(position.x + 16f, y, position.width - 16f, lh);
        if (nameProp != null)
            EditorGUI.PropertyField(nameRect, nameProp, new GUIContent("Name"));
        y += lh + PAD;

        if (property.isExpanded)
        {
            // Kind
            if (kindProp != null)
            {
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lh), kindProp, new GUIContent("Kind"));
                y += lh + PAD;
            }

            // Parent
            if (parent != null)
            {
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lh), parent, new GUIContent("Parent"));
                y += lh + PAD;
            }

            // Kind別
            int kind = (kindProp != null) ? kindProp.enumValueIndex : 0; // 0: CommentWindow, 1: ImageWindow
            if (kind == 0)
            {
                if (header != null)
                {
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, lh), header, new GUIContent("Header Text"));
                    y += lh + PAD;
                }
                if (body != null)
                {
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, lh), body, new GUIContent("Body Text"));
                    y += lh + PAD;
                }
            }
            else
            {
                if (image != null)
                {
                    EditorGUI.PropertyField(new Rect(position.x, y, position.width, lh), image, new GUIContent("Image"));
                    y += lh + PAD;
                }
            }

            // 下線（視覚の区切り）
            EditorGUI.DrawRect(new Rect(position.x, y + LINE, position.width, LINE), new Color(0, 0, 0, 0.1f));
        }

        EditorGUIUtility.labelWidth = labelWidthBackup;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Name 行は常に表示
        int lines = 1;

        var kindProp = property.FindPropertyRelative(PROP_KIND);
        if (property.isExpanded)
        {
            lines += 2; // Kind + Parent
            int kind = (kindProp != null) ? kindProp.enumValueIndex : 0;
            lines += (kind == 0) ? 2 : 1; // Comment: Header+Body / Image: Image
        }

        return lines * (EditorGUIUtility.singleLineHeight + PAD) + (property.isExpanded ? 4f : 0f);
    }
}
#endif
