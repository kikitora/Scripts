using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// このクラスで何をするか：
    /// ジャーニーモード中の WorldState を1か所で管理する司令塔。
    /// - セーブデータからの WorldState ロード
    /// - セーブが見つからない場合の新規 WorldState 初期化
    /// - 手動セーブ呼び出し
    /// を担当する MonoBehaviour。
    /// </summary>
    public class WorldStateRuntime : MonoBehaviour
    {
        [Header("Save Profile")]
        [SerializeField]
        private string profileId = "default";

        [Header("Boot Options")]
        [Tooltip("Awake 時に自動でロード or 新規ワールドを準備するか")]
        [SerializeField]
        private bool autoBootOnAwake = true;

        [Tooltip("セーブが見つからなかったとき、自動で新規ワールドを生成するか")]
        [SerializeField]
        private bool createNewIfNoSave = true;

        /// <summary>
        /// 現在アクティブなワールド状態（セーブ／ロードの単位）。
        /// </summary>
        public WorldState CurrentWorld { get; private set; }

        /// <summary>WorldState が既に用意されているか。</summary>
        public bool HasWorld => CurrentWorld != null;

        //======================================================================
        // ライフサイクル
        //======================================================================

        private void Awake()
        {
            if (autoBootOnAwake)
            {
                Boot();
            }
        }

        //======================================================================
        // パブリック API
        //======================================================================

        /// <summary>
        /// セーブを探してロードし、無ければ新規ワールドを生成する。
        /// forceNewGame = true の場合は、セーブを見ずに必ず新規作成。
        /// </summary>
        public void Boot(bool forceNewGame = false)
        {
            // 1) 既存セーブから読み込み
            if (!forceNewGame)
            {
                if (WorldStateSaveSystem.TryLoadWorld(profileId, out var loadedWorld))
                {
                    CurrentWorld = loadedWorld;

                    // ProfileId が空なら補完
                    if (string.IsNullOrEmpty(CurrentWorld.ProfileId))
                    {
                        CurrentWorld.ProfileId = profileId;
                    }

                    DumpCurrentWorldSummary("Loaded");
                    return;
                }
            }

            // 2) セーブが見つからない場合 or 強制NewGame
            if (createNewIfNoSave || forceNewGame)
            {
                CurrentWorld = CreateNewWorld_Default(profileId);
                DumpCurrentWorldSummary("CreatedNew");
            }
            else
            {
                Debug.LogWarning("[WorldStateRuntime] Boot: セーブが存在せず、新規作成もしない設定になっています。CurrentWorld は null のままです。");
            }
        }

        /// <summary>
        /// 現在の WorldState をセーブします。
        /// </summary>
        public void SaveCurrentWorld()
        {
            if (CurrentWorld == null)
            {
                Debug.LogWarning("[WorldStateRuntime] SaveCurrentWorld: CurrentWorld が null です。");
                return;
            }

            WorldStateSaveSystem.SaveWorld(CurrentWorld);
        }

        //======================================================================
        // New Game 生成（暫定）
        //======================================================================

        /// <summary>
        /// デフォルト設定の新規ワールドを生成する。
        /// （暫定実装：後で SO 定義などに基づいて作り直す前提）
        /// </summary>
        private WorldState CreateNewWorld_Default(string profileId)
        {
            // プロファイルIDとプレイヤー名だけ先に整える
            var world = new WorldState
            {
                ProfileId = profileId,
                PlayerName = "New Player",
            };

            // 所持ゴールドなどの初期値
            world.Gold = 0;
            world.ItemNumList = new List<int>();

            // --- キューブ / ソウル / ボディ 初期生成 ---
            // とりあえず 1 個のプレイヤーキューブと 1 ソウル＋1ボディだけ用意する。

            world.ExCubes = new List<CubeInstance>();
            world.ExSouls = new List<SoulInstance>();
            world.ExBodys = new List<BodyInstance>();

            // 1) キューブ生成
            var playerCube = new CubeInstance
            {
                CubeDefId = CubeKind.Player,
                FieldX = 0,
                FieldY = 0,
                // 足元の面。とりあえず "下面" 相当の dawn を使用
                FloorIndex = UpperSideNum.dawn,
                // プレイヤーは初期北向き
                Direction = Dir.North,
                // Prefab ID などは後で SO ルールが固まったら埋める
            };
            // ここで WorldState 内で重複しないキューブIDを発行
            playerCube.CubeId = world.GenerateUniqueInstanceId();

            world.ExCubes.Add(playerCube);

            // 2) ソウル生成（ランク1の初期ソウル）
            var playerSoul = SoulInstance.CreateRandomInitialSoul(
                rank: 1,
                soulType: SoulType.Normal,
                soulTendency: null,
                jobId: null,
                title: null,
                growthType: null,
                level: null,
                lv1Stats: null,
                growthTargets: null,
                permanentBonuses: null,
                historyEvents: null,
                learnedSkillIds: null,
                reinSouls: null,
                instanceId: world.GenerateUniqueInstanceId()
            );

            world.ExSouls.Add(playerSoul);

            // 3) ボディ生成（暫定：種族/職業/武器は未指定）
            const int bodyRank = 1;
            int hpBase = SpaceJourneyStatMath.GenerateBodyHpBase(bodyRank);
            int otherBase = SpaceJourneyStatMath.GenerateBodyOtherBase(bodyRank);

            var playerBody = new BodyInstance(
                raceId: "",
                bodyJobId: "",
                weaponId: "",
                weaponCandidateIds: null,
                maxHp: hpBase,
                at: otherBase,
                df: otherBase,
                agi: otherBase,
                mat: otherBase,
                mdf: otherBase
            );

            // WorldState 内で重複しない InstanceId を発行してボディに付与
            playerBody.SetInstanceIdForWorld(world.GenerateUniqueInstanceId());

            // Soul 側では equippedBodyInstanceId の紐づけのみを行う
            playerSoul.EquipBody(playerBody);

            // ワールドにボディ実体を登録
            world.ExBodys.Add(playerBody);

            // 4) キューブとソウル／ボディの関係などを必要に応じてセット
            // TODO: 後で「どのキューブに誰が乗っているか」を明示するフィールドを追加する場合、ここで紐づける。

            return world;
        }

        //======================================================================
        // デバッグ用：ワールド概要ダンプ
        //======================================================================

        /// <summary>
        /// デバッグ用に、現在の WorldState 概要をログに出す。
        /// </summary>
        private void DumpCurrentWorldSummary(string tag)
        {
            if (CurrentWorld == null)
            {
                Debug.Log($"[WorldStateRuntime] DumpCurrentWorldSummary({tag}): CurrentWorld is null.");
                return;
            }

            int cubeCount = CurrentWorld.ExCubes?.Count ?? 0;
            int soulCount = CurrentWorld.ExSouls?.Count ?? 0;
            int bodyCount = CurrentWorld.ExBodys?.Count ?? 0;

            Debug.Log($"[WorldStateRuntime] {tag}: ProfileId={CurrentWorld.ProfileId}, PlayerName={CurrentWorld.PlayerName}, " +
                      $"Cubes={cubeCount}, Souls={soulCount}, Bodys={bodyCount}");
        }

        //======================================================================
        // コンテキストメニュー（エディタから操作しやすくするため）
        //======================================================================

        /// <summary>
        /// （任意）現在の WorldState をコンテキストメニューから保存する。
        /// </summary>
        [ContextMenu("Save Current World (Debug)")]
        public void SaveCurrentWorldFromContextMenu()
        {
            if (CurrentWorld == null)
            {
                Debug.LogWarning("[WorldStateRuntime] SaveCurrentWorldFromContextMenu: CurrentWorld が null です。");
                return;
            }

            SaveCurrentWorld();
            DumpCurrentWorldSummary("ManualSave");
        }

        /// <summary>
        /// （任意）ワールドを強制的に新規作成してセーブまで行うデバッグ用メニュー。
        /// まず完全なNewGame状態を作り直してから保存したいときに使う。
        /// </summary>
        [ContextMenu("Force New World and Save (Debug)")]
        public void ForceNewWorldAndSave()
        {
            Boot(forceNewGame: true);
            SaveCurrentWorld();
            DumpCurrentWorldSummary("ForceNewAndSave");
        }

#if UNITY_EDITOR
        [System.Serializable]
        private class DebugCubeInfo
        {
            public string id;
            public string kind;
        }

        [System.Serializable]
        private class DebugSoulInfo
        {
            public string id;
            public string name;
        }

        [System.Serializable]
        private class DebugBodyInfo
        {
            public string id;
            public string jobName;
            public string raceName;
        }

        [Header("Debug - Loaded World Summary")]
        [SerializeField] private List<DebugCubeInfo> debugCubes = new List<DebugCubeInfo>();
        [SerializeField] private List<DebugSoulInfo> debugSouls = new List<DebugSoulInfo>();
        [SerializeField] private List<DebugBodyInfo> debugBodys = new List<DebugBodyInfo>();

        /// <summary>
        /// CurrentWorld の ExCubes / ExSouls / ExBodys から
        /// デバッグ用の一覧情報を再構築します（エディタ専用）。
        /// </summary>
        [ContextMenu("Refresh Debug World Lists (Editor Only)")]
        private void RefreshDebugLists()
        {
            if (CurrentWorld == null)
            {
                Debug.LogWarning("[WorldStateRuntime] RefreshDebugLists: CurrentWorld が null です。");
                if (debugCubes != null) debugCubes.Clear();
                if (debugSouls != null) debugSouls.Clear();
                if (debugBodys != null) debugBodys.Clear();
                return;
            }

            if (debugCubes == null) debugCubes = new List<DebugCubeInfo>();
            if (debugSouls == null) debugSouls = new List<DebugSoulInfo>();
            if (debugBodys == null) debugBodys = new List<DebugBodyInfo>();

            debugCubes.Clear();
            debugSouls.Clear();
            debugBodys.Clear();

            // Cubes
            if (CurrentWorld.ExCubes != null)
            {
                foreach (var cube in CurrentWorld.ExCubes)
                {
                    if (cube == null) continue;

                    var info = new DebugCubeInfo
                    {
                        id = cube.CubeId,
                        kind = cube.CubeDefId.ToString()
                    };
                    debugCubes.Add(info);
                }
            }

            // Souls
            if (CurrentWorld.ExSouls != null)
            {
                foreach (var soul in CurrentWorld.ExSouls)
                {
                    if (soul == null) continue;

                    var info = new DebugSoulInfo
                    {
                        id = soul.InstanceId,
                        name = soul.SoulName
                    };
                    debugSouls.Add(info);
                }
            }

            // Bodys
            if (CurrentWorld.ExBodys != null)
            {
                foreach (var body in CurrentWorld.ExBodys)
                {
                    if (body == null) continue;

                    var info = new DebugBodyInfo
                    {
                        id = body.InstanceId,
                        jobName = ResolveBodyJobName(body),
                        raceName = ResolveRaceName(body)
                    };
                    debugBodys.Add(info);
                }
            }

            Debug.Log($"[WorldStateRuntime] RefreshDebugLists: Cubes={debugCubes.Count}, Souls={debugSouls.Count}, Bodys={debugBodys.Count}");
        }

        private static string ResolveBodyJobName(BodyInstance body)
        {
            if (body == null) return string.Empty;

            var jobDef = body.BodyJob;
            if (jobDef != null && !string.IsNullOrEmpty(jobDef.displayName))
            {
                return jobDef.displayName;
            }

            return string.IsNullOrEmpty(body.BodyJobId) ? "(no job)" : body.BodyJobId;
        }

        private static string ResolveRaceName(BodyInstance body)
        {
            if (body == null) return string.Empty;

            var raceDef = body.Race;
            if (raceDef != null && !string.IsNullOrEmpty(raceDef.displayName))
            {
                return raceDef.displayName;
            }

            return string.IsNullOrEmpty(body.RaceId) ? "(no race)" : body.RaceId;
        }
#endif
    }
}
