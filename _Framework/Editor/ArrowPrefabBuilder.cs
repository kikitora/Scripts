#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime.EditorTools
{
    /// <summary>
    /// 単純なキューブベースの矢プレハブ生成。
    /// 細長く +Z 方向に伸びた Cube → 向きが予測通り、視認性高。
    /// NockArrow_Visual: 静止、手に追従
    /// Arrow_Projectile: Projectile.cs 付きで飛翔
    /// </summary>
    public static class ArrowPrefabBuilder
    {
        private const string OutputDir = "Assets/0SteraCube/Prefabs/Effects";

        [MenuItem("Tools/SteraCube/Build Arrow Prefabs")]
        public static void Build()
        {
            EnsureFolder(OutputDir);

            // マテリアル (明るい黄)
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            mat.color = new Color(1f, 0.85f, 0.1f);
            var matPath = $"{OutputDir}/Arrow_Mat.mat";
            AssetDatabase.CreateAsset(mat, matPath);

            // 矢形状: Cube を +Z 方向に細長く (先端 = +Z)
            GameObject MakeArrowGO(string name)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = name;
                go.transform.localScale = new Vector3(0.05f, 0.05f, 0.5f); // 細長 z方向
                // Collider 不要 (当たり判定はロジック側)
                var col = go.GetComponent<Collider>();
                if (col != null) Object.DestroyImmediate(col);
                var mr = go.GetComponent<MeshRenderer>();
                mr.sharedMaterial = mat;
                return go;
            }

            // NockArrow: 手に追従、静止
            var nock = MakeArrowGO("NockArrow_Visual");
            string nockPath = $"{OutputDir}/NockArrow_Visual.prefab";
            PrefabUtility.SaveAsPrefabAsset(nock, nockPath);
            Object.DestroyImmediate(nock);
            Debug.Log($"[ArrowPrefabBuilder] Created: {nockPath}");

            // Arrow_Projectile: Projectile.cs 付き
            var fly = MakeArrowGO("Arrow_Projectile");
            fly.AddComponent<Projectile>();
            string flyPath = $"{OutputDir}/Arrow_Projectile.prefab";
            var saved = PrefabUtility.SaveAsPrefabAsset(fly, flyPath);
            Object.DestroyImmediate(fly);
            Debug.Log($"[ArrowPrefabBuilder] Created: {flyPath}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ArrowPrefabBuilder] Done. Simple cube-based arrows created.");
            EditorGUIUtility.PingObject(saved);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace("\\", "/");
            var name = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
