// GridRangePatternDrawer.cs
// このクラスで何をするか：
// GridRangePattern をインスペクター上でマス目クリック編集できるようにする PropertyDrawer。
// 仕様：画面上方向(上)が「前方」で、その方向が +Y になる。
// 追加：旧データ（coordinateVersion=0）は表示時に自動移行（y反転）して version=1 にする。
// 追加：offsets がグリッド外にある場合、自動でサイズ拡張して隠れないようにする。

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    [CustomPropertyDrawer(typeof(GridRangePattern))]
    public class GridRangePatternDrawer : PropertyDrawer
    {
        private const float CellSize = 18f;
        private const float CellSpacing = 2f;

        private const int AutoMaxWidth = 10;
        private const int AutoMaxHeight = 11;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float vSpace = EditorGUIUtility.standardVerticalSpacing;

            var gridHeightProp = property.FindPropertyRelative("gridHeight");
            int gh = Mathf.Max(1, gridHeightProp.intValue);
            float gridPixelHeight = gh * (CellSize + CellSpacing) - CellSpacing;

            return line + vSpace + line + vSpace + gridPixelHeight + vSpace + line * 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // ★ 旧→新の移行（Inspectorで開いた瞬間に一度だけ）
            EnsureMigrated(property);

            EditorGUI.BeginProperty(position, label, property);

            float line = EditorGUIUtility.singleLineHeight;
            float vSpace = EditorGUIUtility.standardVerticalSpacing;

            var labelRect = new Rect(position.x, position.y, position.width, line);
            EditorGUI.LabelField(labelRect, label);

            var sizeRect = new Rect(position.x, labelRect.yMax + vSpace, position.width, line);

            SerializedProperty widthProp = property.FindPropertyRelative("gridWidth");
            SerializedProperty heightProp = property.FindPropertyRelative("gridHeight");
            SerializedProperty offsetsProp = property.FindPropertyRelative("offsets");

            float half = sizeRect.width * 0.5f;
            var wRect = new Rect(sizeRect.x, sizeRect.y, half - 2f, line);
            var hRect = new Rect(sizeRect.x + half + 2f, sizeRect.y, half - 2f, line);

            EditorGUI.PropertyField(wRect, widthProp, new GUIContent("Grid Width"));
            EditorGUI.PropertyField(hRect, heightProp, new GUIContent("Grid Height"));

            widthProp.intValue = Mathf.Max(1, widthProp.intValue);
            heightProp.intValue = Mathf.Max(1, heightProp.intValue);

            AutoExpandToFitOffsets(widthProp, heightProp, offsetsProp);

            int gridWidth = widthProp.intValue;
            int gridHeight = heightProp.intValue;

            float gridTop = sizeRect.yMax + vSpace;
            float gridPixelHeight = gridHeight * (CellSize + CellSpacing) - CellSpacing;
            var gridRect = new Rect(position.x, gridTop, position.width, gridPixelHeight);

            var click = DrawGrid(gridRect, offsetsProp, gridWidth, gridHeight);
            if (click.changed && click.turnedOn)
            {
                AutoExpandWhenEdgeClicked(widthProp, heightProp, click.offset);
            }

            float infoTop = gridRect.yMax + vSpace;
            var infoRect1 = new Rect(position.x, infoTop, position.width, line);
            var infoRect2 = new Rect(position.x, infoRect1.yMax, position.width, line);

            EditorGUI.LabelField(infoRect1,
                "中央の ★ がキャラ位置（原点: (0,0)）です。クリックで範囲ON/OFFできます。",
                EditorStyles.miniLabel);

            EditorGUI.LabelField(infoRect2,
                "このグリッド上では『上方向(画面上側)が前方（+Y）』としてパターンを作成します。",
                EditorStyles.miniLabel);

            EditorGUI.EndProperty();
        }

        private void EnsureMigrated(SerializedProperty property)
        {
            var verProp = property.FindPropertyRelative("coordinateVersion");
            var offsetsProp = property.FindPropertyRelative("offsets");

            if (verProp == null || offsetsProp == null) return;

            if (verProp.intValue >= GridRangePattern.CurrentCoordinateVersion)
                return;

            // 旧→新：y反転
            Undo.RecordObject(property.serializedObject.targetObject, "Migrate GridRangePattern");
            for (int i = 0; i < offsetsProp.arraySize; i++)
            {
                var e = offsetsProp.GetArrayElementAtIndex(i);
                var v = e.vector2IntValue;
                e.vector2IntValue = new Vector2Int(v.x, -v.y);
            }
            verProp.intValue = GridRangePattern.CurrentCoordinateVersion;
            property.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private struct ClickResult
        {
            public bool changed;
            public bool turnedOn;
            public Vector2Int offset;
        }

        private ClickResult DrawGrid(Rect gridRect, SerializedProperty offsetsProp, int gridWidth, int gridHeight)
        {
            ClickResult result = default;

            int pivotX = gridWidth / 2;
            int pivotY = gridHeight / 2;

            float totalGridWidth = gridWidth * (CellSize + CellSpacing) - CellSpacing;
            float startX = gridRect.x + (gridRect.width - totalGridWidth) * 0.5f;
            float y = gridRect.y;

            for (int gy = 0; gy < gridHeight; gy++)
            {
                float x = startX;

                for (int gx = 0; gx < gridWidth; gx++)
                {
                    // 上(画面上)が +Y
                    Vector2Int offset = new Vector2Int(gx - pivotX, pivotY - gy);

                    bool isOn = ContainsOffset(offsetsProp, offset);
                    bool isPivot = (gx == pivotX && gy == pivotY);

                    Rect cellRect = new Rect(x, y, CellSize, CellSize);

                    Color prevBg = GUI.backgroundColor;
                    GUI.backgroundColor = isOn ? Color.cyan : Color.gray * 0.9f;

                    string cellLabel = isPivot ? "★" : (isOn ? "■" : "");

                    if (GUI.Button(cellRect, cellLabel))
                    {
                        Undo.RecordObject(offsetsProp.serializedObject.targetObject, "Edit Grid Range");

                        bool newOn = !isOn;
                        ToggleOffset(offsetsProp, offset, newOn);
                        offsetsProp.serializedObject.ApplyModifiedProperties();

                        result.changed = true;
                        result.turnedOn = newOn;
                        result.offset = offset;
                    }

                    GUI.backgroundColor = prevBg;
                    x += CellSize + CellSpacing;
                }

                y += CellSize + CellSpacing;
            }

            return result;
        }

        private void AutoExpandToFitOffsets(SerializedProperty widthProp, SerializedProperty heightProp, SerializedProperty offsetsProp)
        {
            if (offsetsProp.arraySize <= 0) return;

            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;

            for (int i = 0; i < offsetsProp.arraySize; i++)
            {
                Vector2Int o = offsetsProp.GetArrayElementAtIndex(i).vector2IntValue;
                if (o.x < minX) minX = o.x;
                if (o.x > maxX) maxX = o.x;
                if (o.y < minY) minY = o.y;
                if (o.y > maxY) maxY = o.y;
            }

            int w = widthProp.intValue;
            int h = heightProp.intValue;

            int targetW = FindMinSizeToFitOffsets(w, minX, maxX, AutoMaxWidth, isWidth: true);
            int targetH = FindMinSizeToFitOffsets(h, minY, maxY, AutoMaxHeight, isWidth: false);

            if (targetW != w || targetH != h)
            {
                Undo.RecordObject(offsetsProp.serializedObject.targetObject, "Auto Expand Grid");
                widthProp.intValue = targetW;
                heightProp.intValue = targetH;
                widthProp.serializedObject.ApplyModifiedProperties();
            }
        }

        private int FindMinSizeToFitOffsets(int currentSize, int min, int max, int maxAuto, bool isWidth)
        {
            if (currentSize >= maxAuto) return currentSize;
            if (DoesSizeFit(currentSize, min, max, isWidth)) return currentSize;

            for (int s = currentSize; s <= maxAuto; s++)
                if (DoesSizeFit(s, min, max, isWidth))
                    return s;

            return maxAuto;
        }

        private bool DoesSizeFit(int size, int min, int max, bool isWidth)
        {
            int pivot = size / 2;

            if (isWidth)
            {
                int negCapacity = pivot;
                int posCapacity = size - pivot - 1;
                return (-min) <= negCapacity && max <= posCapacity;
            }
            else
            {
                // 上が +Y / 下が -Y
                int posCapacity = pivot;            // 上（+Y）
                int negCapacity = size - pivot - 1; // 下（-Y）
                return (-min) <= negCapacity && max <= posCapacity;
            }
        }

        private bool AutoExpandWhenEdgeClicked(SerializedProperty widthProp, SerializedProperty heightProp, Vector2Int clickedOffset)
        {
            bool changed = false;

            int w = widthProp.intValue;
            int h = heightProp.intValue;

            int pivotX = w / 2;
            int pivotY = h / 2;

            int leftEdge = -pivotX;
            int rightEdge = (w - pivotX - 1);

            int topEdge = pivotY;
            int bottomEdge = -(h - pivotY - 1);

            if (w < AutoMaxWidth && (clickedOffset.x == leftEdge || clickedOffset.x == rightEdge))
            {
                int newW = Mathf.Min(AutoMaxWidth, w + 2);
                if (newW != w) { widthProp.intValue = newW; changed = true; }
            }

            if (h < AutoMaxHeight && (clickedOffset.y == topEdge || clickedOffset.y == bottomEdge))
            {
                int newH = Mathf.Min(AutoMaxHeight, h + 2);
                if (newH != h) { heightProp.intValue = newH; changed = true; }
            }

            if (changed)
            {
                Undo.RecordObject(widthProp.serializedObject.targetObject, "Auto Expand Grid");
                widthProp.serializedObject.ApplyModifiedProperties();
            }

            return changed;
        }

        private bool ContainsOffset(SerializedProperty offsetsProp, Vector2Int offset)
        {
            for (int i = 0; i < offsetsProp.arraySize; i++)
                if (offsetsProp.GetArrayElementAtIndex(i).vector2IntValue == offset)
                    return true;
            return false;
        }

        private void ToggleOffset(SerializedProperty offsetsProp, Vector2Int offset, bool shouldBeOn)
        {
            if (shouldBeOn)
            {
                if (!ContainsOffset(offsetsProp, offset))
                {
                    int newIndex = offsetsProp.arraySize;
                    offsetsProp.InsertArrayElementAtIndex(newIndex);
                    offsetsProp.GetArrayElementAtIndex(newIndex).vector2IntValue = offset;
                }
            }
            else
            {
                for (int i = 0; i < offsetsProp.arraySize; i++)
                {
                    if (offsetsProp.GetArrayElementAtIndex(i).vector2IntValue == offset)
                    {
                        offsetsProp.DeleteArrayElementAtIndex(i);
                        break;
                    }
                }
            }
        }
    }
}
#endif
