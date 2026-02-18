using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// フィールド上に存在する1つのキューブの「セーブ可能な状態」を保持する。
    /// - 見た目生成に必要なID（cubePrefabId / groundId / visualVariantId）を持つ
    /// - 向きは Dir(enum) を保持する
    /// - ランタイムで生成された CubeUnit や Ground の実体参照は NonSerialized で保持する
    /// </summary>
    [Serializable]
    public class CubeInstance
    {
        [Header("Identity")]
        [SerializeField] private string cubeId;

        [Header("Kind")]
        [SerializeField] private CubeKind cubeDefId;

        [Header("Board Position")]
        [SerializeField] private int fieldX;
        [SerializeField] private int fieldY;
        [SerializeField] private UpperSideNum floorIndex;

        [Header("Facing")]
        [SerializeField] private Dir direction = Dir.North;

        [Header("Visual / Prefab IDs")]
        [SerializeField] private string cubePrefabId;     // 本体モデルの参照ID（MasterDatabase.GetCubeById）
        [SerializeField] private string groundId;         // 地面モデルの参照ID（MasterDatabase.GetCubeGroundById）
        [SerializeField] private string visualVariantId;  // 見た目差分（必要なら cubePrefabId に統合でもOK）

        [Header("Souls on this cube (IDs only)")]
        [SerializeField] private List<string> soulIds = new List<string>();

        [Header("Resources")]
        [SerializeField] private int vp;
        [SerializeField] private int ep;

        [SerializeField] private OneSide[] sides = new OneSide[4];

        // ===== ランタイム参照（セーブしない）=====
        [NonSerialized] private CubeUnit cubeUnit;
        [NonSerialized] private GameObject groundGO;

        public string CubeId { get => cubeId; set => cubeId = value; }
        public CubeKind CubeDefId { get => cubeDefId; set => cubeDefId = value; }


        /// <summary>
        /// WorldStateRuntime が存在すれば、この CubeInstance を WorldState.ExCubes に登録する。
        /// </summary>
        private static void RegisterCubeToCurrentWorld(CubeInstance cube)
        {
            if (cube == null) return;

            var runtime = UnityEngine.Object.FindObjectOfType<WorldStateRuntime>();
            if (runtime == null) return;

            var world = runtime.CurrentWorld;
            if (world == null) return;

            world.RegisterCubeInstance(cube);
        }

        /// <summary>
        /// cubeId が未設定の場合に WorldState 経由で一意なIDを発行してセットする。
        /// 既に値が入っている（セーブ復元済みなど）の場合は上書きしない。
        /// 
        /// 本番運用では、WorldState.GenerateUniqueInstanceId() で ID を決め、
        /// CubeId プロパティ経由で代入することを推奨する。
        /// このメソッドは WorldState を参照しないテスト用フォールバック。
        /// </summary>
        public void EnsureCubeId()
        {
            if (string.IsNullOrEmpty(cubeId))
            {
                var tempWorld = new WorldState();
                cubeId = tempWorld.GenerateUniqueInstanceId();
            }

            // WorldStateRuntime が動いていれば自動的に登録しておく
            RegisterCubeToCurrentWorld(this);
        }


        public int FieldX { get => fieldX; set => fieldX = value; }
        public int FieldY { get => fieldY; set => fieldY = value; }
        public Vector2Int BoardPos => new Vector2Int(fieldX, fieldY);

        public UpperSideNum FloorIndex { get => floorIndex; set => floorIndex = value; }

        public Dir Direction { get => direction; set => direction = value; }

        public string CubePrefabId { get => cubePrefabId; set => cubePrefabId = value; }
        public string GroundId { get => groundId; set => groundId = value; }
        public string VisualVariantId { get => visualVariantId; set => visualVariantId = value; }

        public List<string> SoulIds { get => soulIds; set => soulIds = value ?? new List<string>(); }

        public int Vp { get => vp; set => vp = value; }
        public int Ep { get => ep; set => ep = value; }

        public CubeUnit RuntimeCubeUnit => cubeUnit;
        public GameObject RuntimeGroundGO => groundGO;

        public void SetRuntimeCubeUnit(CubeUnit unit) => cubeUnit = unit;
        public void SetRuntimeGroundGO(GameObject go) => groundGO = go;

    }

    [Serializable]
    public class OneSide
    {
        [SerializeField] private int morale;
        public int Morale { get => morale; set => morale = value; }
    }
}
