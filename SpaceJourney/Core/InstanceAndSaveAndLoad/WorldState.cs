// SpaceJourneyGameState.cs
// このクラスで何をするか：
// ジャーニーモード全体の「状態」を1つにまとめて持ちます。
// - プレイヤーのソウル一覧
// - 余っているボディ一覧（倉庫など）
// - 現在のフィールド（マップ）状態と、その上のキューブ一覧
// などを含み、セーブ／ロードの単位にもなります。

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// ジャーニーモード全体の状態を保持するルートクラス。
    /// セーブ/ロードの単位にもなる。
    /// </summary>
    [Serializable]
    public class WorldState
    {
        [Header("メタ情報")]
        [SerializeField] private string profileId;
        [SerializeField] private string playerName;
        [Header("プレイヤー情報")]
        [SerializeField] private int gold;  // 所持ゴールド
        [SerializeField] private List<int> itemNumList = new(); // 所持アイテム数（アイテムID順）
        [Header("キューブ")]
        [SerializeField] private List<CubeInstance> exCubes = new(); // 現在マップ上に存在しているキューブ

        [Header("ソウル・ボディ")]
        [SerializeField] private List<SoulInstance> exSouls = new();
        [SerializeField] private List<BodyInstance> exBodys = new();

        [Header("フィールド（マップ）")]
        [SerializeField] private JourneyFieldState currentField;
        // 将来的に複数フィールド持つなら List<JourneyFieldState> fields にしてもOK

        // ─────────────────────────────
        // プロパティ（外部からは基本ここ経由で触る）
        // ─────────────────────────────

        /// <summary>セーブスロットなどを識別するためのプロフィールID。</summary>
        public string ProfileId
        {
            get => profileId;
            set => profileId = value;
        }

        /// <summary>プレイヤー名。</summary>
        public string PlayerName
        {
            get => playerName;
            set => playerName = value;
        }

        /// <summary>現在マップ上に存在しているキューブ一覧。</summary>
        public List<CubeInstance> ExCubes
        {
            get => exCubes;
            set => exCubes = value ?? new List<CubeInstance>();
        }

        /// <summary>ゲーム内に存在しているソウル一覧。</summary>
        public List<SoulInstance> ExSouls
        {
            get => exSouls;
            set => exSouls = value ?? new List<SoulInstance>();
        }

        /// <summary>ゲーム内に存在しているボディ一覧。</summary>
        public List<BodyInstance> ExBodys
        {
            get => exBodys;
            set => exBodys = value ?? new List<BodyInstance>();
        }

        // ─────────────────────────────
        // ランタイム生成オブジェクトの登録ヘルパ
        // ─────────────────────────────

        /// <summary>
        /// ランタイムで生成された SoulInstance を ExSouls に登録する。
        /// 同じ参照が既に含まれている場合は何もしない。
        /// </summary>
        public void RegisterSoulInstance(SoulInstance soul)
        {
            if (soul == null) return;

            if (ExSouls == null)
            {
                ExSouls = new List<SoulInstance>();
            }

            if (!ExSouls.Contains(soul))
            {
                ExSouls.Add(soul);
            }
        }

        /// <summary>
        /// ランタイムで生成された BodyInstance を ExBodys に登録する。
        /// 同じ参照が既に含まれている場合は何もしない。
        /// </summary>
        public void RegisterBodyInstance(BodyInstance body)
        {
            if (body == null) return;

            if (ExBodys == null)
            {
                ExBodys = new List<BodyInstance>();
            }

            if (!ExBodys.Contains(body))
            {
                ExBodys.Add(body);
            }
        }

        /// <summary>
        /// ランタイムで生成された CubeInstance を ExCubes に登録する。
        /// 同じ参照が既に含まれている場合は何もしない。
        /// </summary>
        public void RegisterCubeInstance(CubeInstance cube)
        {
            if (cube == null) return;

            if (ExCubes == null)
            {
                ExCubes = new List<CubeInstance>();
            }

            if (!ExCubes.Contains(cube))
            {
                ExCubes.Add(cube);
            }
        }


        /// <summary>現在のフィールド（マップ）状態。</summary>
        public JourneyFieldState CurrentField
        {
            get => currentField;
            set => currentField = value;
        }

        /// <summary>
        /// この WorldState 内でまだ使われていない一意なIDを生成して返す。
        /// - ベースは GUID(N)。
        /// - すでに使用中の ID と被っていた場合は、別の GUID を再生成する。
        /// ※ 実運用上、1回で決まる想定だが、万が一の重複にも備える。
        /// </summary>
        public string GenerateUniqueInstanceId()
        {
            // すでに使われているIDを全部集める
            var used = new HashSet<string>();

            // Souls
            if (ExSouls != null)
            {
                foreach (var soul in ExSouls)
                {
                    if (soul == null) continue;
                    var id = soul.InstanceId;
                    if (!string.IsNullOrEmpty(id))
                    {
                        used.Add(id);
                    }
                }
            }

            // Bodys
            if (ExBodys != null)
            {
                foreach (var body in ExBodys)
                {
                    if (body == null) continue;
                    var id = body.InstanceId;
                    if (!string.IsNullOrEmpty(id))
                    {
                        used.Add(id);
                    }
                }
            }

            // Cubes
            if (ExCubes != null)
            {
                foreach (var cube in ExCubes)
                {
                    if (cube == null) continue;
                    var id = cube.CubeId;
                    if (!string.IsNullOrEmpty(id))
                    {
                        used.Add(id);
                    }
                }
            }

            // ここから GUID を生成して「まだ使われていないもの」が出るまで回す
            while (true)
            {
                string candidate = Guid.NewGuid().ToString("N");
                if (!used.Contains(candidate))
                {
                    return candidate;
                }

                // ありえないほど低確率だけど、万が一かぶった場合は while ループがもう1周する。
                // 実質1回で決まる想定。
            }
        }

        public int Gold { get => gold; set => gold = value; }
        public List<int> ItemNumList { get => itemNumList; set => itemNumList = value; }
    }

    /// <summary>
    /// 1枚のフィールド（マップ）の状態。
    /// </summary>
    [Serializable]
    public class JourneyFieldState
    {
        [SerializeField] private MapStateType fieldType;  // マップのタイプ（ボス、普通、天国、地獄など）
        [SerializeField] private int bossNumber;          // 次もしくは現在マップのボス番号（0〜n）
        [SerializeField] private int mapRank;             // マップのランク（1〜10など）
        [SerializeField] private string fieldId;          // マップID
        [SerializeField] private UpperSideNum rotate;              // マップの回転（0〜3）

        /// <summary>マップのタイプ（ボス、普通、天国、地獄など）。</summary>
        public MapStateType FieldType
        {
            get => fieldType;
            set => fieldType = value;
        }

        /// <summary>次もしくは現在マップのボス番号。</summary>
        public int BossNumber
        {
            get => bossNumber;
            set => bossNumber = value;
        }

        /// <summary>マップのランク（難易度など）。</summary>
        public int MapRank
        {
            get => mapRank;
            set => mapRank = value;
        }

        /// <summary>マップID。</summary>
        public string FieldId
        {
            get => fieldId;
            set => fieldId = value;
        }

        /// <summary>マップの回転。</summary>
        public void SwithFieldRote(Dir dir)//引数は回転方向
        {

        }
    }

    
}
