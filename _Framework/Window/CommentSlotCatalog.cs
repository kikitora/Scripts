using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CommentSlotCatalog", menuName = "Comment/Slot Catalog", order = 1)]
public class CommentSlotCatalog : ScriptableObject
{
    public List<string> commentSlots = new List<string>();
    public List<string> imageSlots = new List<string>();

#if UNITY_EDITOR
    private const string RES_PATH = "CommentSlotCatalog";
    public static CommentSlotCatalog LoadOrCreate()
    {
        var asset = Resources.Load<CommentSlotCatalog>(RES_PATH);
        if (asset) return asset;

        var inst = ScriptableObject.CreateInstance<CommentSlotCatalog>();
        string dir = "Assets/Resources";
        if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
        string path = $"{dir}/{RES_PATH}.asset";
        UnityEditor.AssetDatabase.CreateAsset(inst, path);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        return inst;
    }

    public static void AutoSyncFrom(CommentUI ui)
    {
        if (ui == null) return;
        var cat = LoadOrCreate();
        cat.commentSlots.Clear();
        cat.imageSlots.Clear();

        var so = new SerializedObject(ui);
        var slotsProp = so.FindProperty("slots");
        for (int i = 0; i < slotsProp.arraySize; i++)
        {
            var el = slotsProp.GetArrayElementAtIndex(i);
            var name = el.FindPropertyRelative("name").stringValue;
            var kind = (UISlotKind)el.FindPropertyRelative("kind").enumValueIndex;
            if (string.IsNullOrWhiteSpace(name)) continue;
            if (kind == UISlotKind.CommentWindow) { if (!cat.commentSlots.Contains(name)) cat.commentSlots.Add(name); }
            else if (kind == UISlotKind.ImageWindow) { if (!cat.imageSlots.Contains(name)) cat.imageSlots.Add(name); }
        }
        UnityEditor.EditorUtility.SetDirty(cat);
        UnityEditor.AssetDatabase.SaveAssets();
    }
#endif
}
