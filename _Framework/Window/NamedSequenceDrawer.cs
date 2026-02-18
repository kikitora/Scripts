#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// CommentUI.NamedSequence 用 PropertyDrawer
[CustomPropertyDrawer(typeof(CommentUI.NamedSequence))]
public class NamedSequenceDrawer : PropertyDrawer
{
    const float PAD = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.isExpanded = EditorGUI.Foldout(
            new Rect(position.x, position.y, 14f, EditorGUIUtility.singleLineHeight),
            property.isExpanded,
            GUIContent.none
        );

        float y = position.y;
        float lh = EditorGUIUtility.singleLineHeight;

        var keyProp = property.FindPropertyRelative("key");
        var seqProp = property.FindPropertyRelative("sequence");

        // 1行目：Key（常に表示）
        var keyRect = new Rect(position.x + 16f, y, position.width - 16f, lh);
        if (keyProp != null) EditorGUI.PropertyField(keyRect, keyProp, new GUIContent("Key"));
        y += lh + PAD;

        if (property.isExpanded)
        {
            // 2行目：Sequence
            if (seqProp != null)
            {
                EditorGUI.PropertyField(new Rect(position.x, y, position.width, lh), seqProp, new GUIContent("Sequence"));
                y += lh + PAD;
            }

            // 薄い線
            EditorGUI.DrawRect(new Rect(position.x, y + 1f, position.width, 1f), new Color(0, 0, 0, 0.1f));
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int lines = 1; // Key
        if (property.isExpanded) lines += 1; // Sequence
        return lines * (EditorGUIUtility.singleLineHeight + PAD) + (property.isExpanded ? 4f : 0f);
    }
}
#endif
