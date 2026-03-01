using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    [Serializable]
    public struct SoulSnapshot
    {
        public int At;
        public int Df;
        public int Agi;
        public int Mat;
        public int Mdf;
        public int MaxHp;

        public SoulSnapshot(int at, int df, int agi, int mat, int mdf, int maxHp)
        {
            At = at;
            Df = df;
            Agi = agi;
            Mat = mat;
            Mdf = mdf;
            MaxHp = maxHp;
        }
    }

    /// <summary>
    /// SoulInstance
    /// このクラスで何をするか：
    /// ・ゲーム内に登場する「1人のソウル」の実体データを保持する。
    /// ・ランクは enum 固定ではなく int で扱う（将来 10 以上に伸ばす想定）。
    /// ・SoulType（Unique/Normal）は iconId の形式（数字のみ/それ以外）から自動分類する。
    /// ・TalentRank（才能ランク）は Soul に1つだけ保持し、転生ごとにブレない。
    /// </summary>
    [Serializable]
    public class SoulInstance : ISerializationCallbackReceiver
    {
        [Header("基本プロファイル")]
        [SerializeField] private string instanceId;
        [SerializeField] private string soulName;

        [Tooltip("顔アイコン用のID。セーブデータ上にはこのIDだけを保存し、復元時に MasterDatabase から Sprite を解決します。")]
        [SerializeField] private string iconId;
        [NonSerialized] private Sprite iconSprite;

        [Header("分類（ユニーク/ノーマル）")]
        [Tooltip("iconId が数字だけなら Normal（互換のランダム顔）、それ以外は Unique（ユニークID）として自動判定されます。")]
        [SerializeField] private SoulType soulType = SoulType.Normal;

        [Header("才能（ソウル固有）")]
        [Tooltip("ソウルの才能ランク。転生ごとにブレない（OneReinSoulData には持たせない）。")]
        [SerializeField] private TalentRank talentRank = TalentRank.C;

        [Header("ソウル傾向")]
        [Tooltip("このソウルがどのボディ職向きか（戦士/騎士/弓兵/魔術師/槍兵など）。転生しても変わらない性格寄りの指向性。")]
        [SerializeField] private SoulJobTendency soulTendency = SoulJobTendency.Warrior;

        [Header("性別（内部属性）")]
        [Tooltip("名前や顔グラフィックの系統を決めるための内部用の性別。UI には直接表示しない。")]
        [SerializeField] private FaceSexCategory sex = FaceSexCategory.Unknown;

        [Header("配置状態")]
        [SerializeField] private SoulState state;
        [SerializeField] private string currentCubeId;

        [SerializeField] private int sideNumber = -1;
        [SerializeField] private Vector2Int localCell = new Vector2Int(-1, -1);

        [Header("装備ボディ")]
        [Tooltip("このソウルが現在装備しているボディ個体の InstanceId。空の場合は未装備。")]
        [SerializeField] private string equippedBodyInstanceId;


        [Header("転生ソウルセット")]
        [SerializeField] private List<OneReinSoulData> reinSouls = new List<OneReinSoulData>();
        [SerializeField] private int selectedReinIndex = 0;

        public string InstanceId => instanceId;

        /// <summary>
        /// インスタンスIDが空なら一意なIDを割り当て、WorldStateRuntime が動作していれば
        /// CurrentWorld.ExSouls にこのソウルを登録する。
        /// </summary>
        public void EnsureInstanceId()
        {
            // まだ ID が空ならフォールバックとして一意なIDを発行
            if (string.IsNullOrEmpty(instanceId))
            {
                var tempWorld = new WorldState();
                instanceId = tempWorld.GenerateUniqueInstanceId();
            }

            // WorldStateRuntime があれば、そちらの WorldState に登録
            RegisterSoulToCurrentWorld(this);
        }

        /// <summary>
        /// WorldStateRuntime が存在すれば、この SoulInstance を WorldState.ExSouls に登録する。
        /// </summary>
        private static void RegisterSoulToCurrentWorld(SoulInstance soul)
        {
            if (soul == null) return;

            var runtime = UnityEngine.Object.FindObjectOfType<WorldStateRuntime>();
            if (runtime == null) return;

            var world = runtime.CurrentWorld;
            if (world == null) return;

            world.RegisterSoulInstance(soul);
        }


        /// <summary>
        /// ワールド登録用にインスタンス ID を設定する。
        /// 空文字や null の場合は何もしない。
        /// </summary>
        public void SetInstanceIdForWorld(string newId)
        {
            if (string.IsNullOrEmpty(newId))
            {
                return;
            }

            instanceId = newId;
        }


        /// <summary>このソウルが持つ固定の傾向（転生しても変わらない）。</summary>
        public SoulJobTendency SoulTendency
        {
            get => soulTendency;
            set => soulTendency = value;
        }
        /// <summary>名前や顔の系統を決めるための内部性別。</summary>
        public FaceSexCategory Sex => sex;

        // =====================================================================
        // 初期ソウル生成（SoulFactory に委譲）
        // =====================================================================

        /// <summary>
        /// ランク + 傾向だけ指定して Soul を生成する。SoulFactory.Create() に委譲。
        /// </summary>
        public static SoulInstance CreateRandomInitialSoul(int rank, SoulJobTendency soulTendency)
            => SoulFactory.Create(rank: rank, soulTendency: soulTendency);

        /// <summary>
        /// フル引数版。null / None = ランダム。SoulFactory.Create() に委譲。
        /// 後方互換のため維持。新規コードは SoulFactory.Create() を直接呼ぶこと。
        /// </summary>
        public static SoulInstance CreateRandomInitialSoul(
            int rank,
            string soulName = null,
            string iconId = null,
            SoulType? soulType = null,
            TalentRank? talentRank = null,
            SoulJobTendency? soulTendency = null,
            string jobId = null,
            string title = null,
            GrowthType? growthType = null,
            FaceSexCategory? sex = null,
            int? level = null,
            int[] lv1Stats = null,
            float[] growthTargets = null,
            int[] permanentBonuses = null,
            List<ReinEvent> historyEvents = null,
            List<string> learnedSkillIds = null,
            List<OneReinSoulData> reinSouls = null,
            string instanceId = null)
            => SoulFactory.Create(
                rank: rank,
                soulName: soulName,
                iconId: iconId,
                soulType: soulType,
                talentRank: talentRank,
                soulTendency: soulTendency,
                jobId: jobId,
                title: title,
                growthType: growthType,
                sex: sex,
                level: level,
                lv1Stats: lv1Stats,
                growthTargets: growthTargets,
                permanentBonuses: permanentBonuses,
                historyEvents: historyEvents,
                learnedSkillIds: learnedSkillIds,
                reinSouls: reinSouls,
                instanceId: instanceId);

        /// <summary>
        /// SoulFactory からのみ呼ぶ内部初期化メソッド。
        /// コンストラクタ後に sex / iconSprite / talentRank / soulTendency / soulType を設定する。
        /// </summary>
        internal void InitFromFactory(
            FaceSexCategory nameSex,
            Sprite sprite,
            TalentRank talent,
            SoulJobTendency tendency,
            SoulType? type)
        {
            sex = nameSex;
            iconSprite = sprite;
            talentRank = talent;
            soulTendency = tendency;
            if (type.HasValue) soulType = type.Value;
        }

        public SoulInstance(
    string instanceId,
    string soulName,
    string iconId,
    List<OneReinSoulData> reinSouls,
    int selectedReinIndex = 0
)
        {
            this.instanceId = instanceId;
            this.soulName = soulName;
            this.iconId = iconId;

            this.reinSouls = reinSouls ?? new List<OneReinSoulData>();
            this.selectedReinIndex = selectedReinIndex;

            UpdateSoulTypeFromIconId();
            ResolveJobsForReinSouls();
            ResolveIconSpriteFromDatabase();

            state = SoulState.None;
            currentCubeId = null;
            sideNumber = -1;
            localCell = new Vector2Int(-1, -1);
        }

        public SoulInstance() { }

        /// <summary>
        /// iconId の形式から SoulType を自動判定する。
        /// MasterDatabase.GetFaceIconById と同じ互換ルール：
        /// ・数字だけ → ランダム配列インデックス扱い（Normal）
        /// ・それ以外 → ユニークID扱い（Unique）
        /// </summary>
        private void UpdateSoulTypeFromIconId()
        {
            if (string.IsNullOrEmpty(iconId))
            {
                soulType = SoulType.Normal;
                return;
            }

            soulType = IsAllDigits(iconId) ? SoulType.Normal : SoulType.Unique;
        }

        private static bool IsAllDigits(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            for (int i = 0; i < s.Length; i++)
            {
                if (!char.IsDigit(s[i])) return false;
            }
            return true;
        }

        public void ResolveJobsForReinSouls()
        {
            var db = MasterDatabase.Instance;
            if (db == null || reinSouls == null) return;

            foreach (var rein in reinSouls)
            {
                rein?.ResolveJob(id => db.GetSoulJobById(id));
            }
        }

        public void ResolveIconSpriteFromDatabase()
        {
            UpdateSoulTypeFromIconId();

            if (string.IsNullOrEmpty(iconId))
            {
                iconSprite = null;
                return;
            }

            var db = MasterDatabase.Instance;
            if (db == null)
            {
                iconSprite = null;
                return;
            }

            iconSprite = db.GetFaceIconById(iconId, sex);
        }

        public string SoulName
        {
            get => soulName;
            set => soulName = value;
        }

        public string IconId
        {
            get => iconId;
            set
            {
                iconId = value;
                UpdateSoulTypeFromIconId();
            }
        }

        public Sprite IconSprite => iconSprite;

        public SoulType SoulType => soulType;

        public TalentRank Talent => talentRank;

        public SoulState State
        {
            get => state;
            set => state = value;
        }

        public string CurrentCubeId
        {
            get => currentCubeId;
            set => currentCubeId = value;
        }

        public int SideNumber
        {
            get => sideNumber;
            set => sideNumber = value;
        }

        public string EquippedBodyInstanceId
        {
            get => equippedBodyInstanceId;
            set => equippedBodyInstanceId = value;
        }

        public Vector2Int LocalCell
        {
            get => localCell;
            set => localCell = value;
        }

        public IReadOnlyList<OneReinSoulData> ReinSouls => reinSouls;

        /// <summary>転生データを末尾に追加する。</summary>
        public void AddReinSoul(OneReinSoulData data)
        {
            if (data == null) return;
            reinSouls ??= new List<OneReinSoulData>();
            reinSouls.Add(data);
            selectedReinIndex = reinSouls.Count - 1;
        }

        /// <summary>指定インデックスの転生データを上書きする。</summary>
        public void ReplaceReinSoul(int index, OneReinSoulData data)
        {
            if (data == null) return;
            reinSouls ??= new List<OneReinSoulData>();
            if (index < 0 || index >= reinSouls.Count)
            {
                Debug.LogWarning($"[SoulInstance] ReplaceReinSoul: index {index} は範囲外です。末尾に追加します。");
                AddReinSoul(data);
                return;
            }
            reinSouls[index] = data;
        }

        public int SelectedReinIndex
        {
            get
            {
                if (reinSouls == null || reinSouls.Count == 0) return -1;
                return Mathf.Clamp(selectedReinIndex, 0, reinSouls.Count - 1);
            }
            set
            {
                if (reinSouls == null || reinSouls.Count == 0)
                {
                    selectedReinIndex = -1;
                    return;
                }
                selectedReinIndex = Mathf.Clamp(value, 0, reinSouls.Count - 1);
            }
        }

        public OneReinSoulData CurrentReinSoul
        {
            get
            {
                int idx = SelectedReinIndex;
                if (idx < 0 || reinSouls == null || idx >= reinSouls.Count) return null;
                return reinSouls[idx];
            }
        }

        // 便利プロパティ（UI 用）
        public int Level => CurrentReinSoul?.Level ?? 1;
        public int CurrentExp => CurrentReinSoul?.CurrentExp ?? 0;

        /// <summary>
        /// 旧：Rank enum → 新：int rank
        /// </summary>
        public int Rank => CurrentReinSoul?.Rank ?? 1;

        public GrowthType GrowthType => CurrentReinSoul?.GrowthType ?? GrowthType.Normal;
        public SoulJobDefinition Job => CurrentReinSoul?.JobDefinition;

        public int GetSoulStat(StatKind kind)
        {
            var rein = CurrentReinSoul;
            if (rein == null) return 0;
            return rein.GetSoulStat(kind);
        }

        /// <summary>
        /// このソウルにボディ個体を装備し、その InstanceId を記録する。
        /// </summary>
        public void EquipBody(BodyInstance body)
        {
            if (body == null)
            {
                equippedBodyInstanceId = null;
                return;
            }

            if (string.IsNullOrEmpty(body.InstanceId))
            {
                Debug.LogWarning(
                    "[SoulInstance] EquipBody: BodyInstance.InstanceId が未設定です。" +
                    "本番では WorldState.GenerateUniqueInstanceId() で ID を付与してから " +
                    "EquipBody を呼び出してください。"
                );
            }

            equippedBodyInstanceId = body.InstanceId;
        }


        public SoulSnapshot GetFinalSnapshot(BodyInstance body)
        {
            int at = GetSoulStat(StatKind.AT);
            int df = GetSoulStat(StatKind.DF);
            int agi = GetSoulStat(StatKind.AGI);
            int mat = GetSoulStat(StatKind.MAT);
            int mdf = GetSoulStat(StatKind.MDF);

            int maxHp = 0;

            if (body != null)
            {
                at = body.ApplyToSoulStat(at, StatKind.AT);
                df = body.ApplyToSoulStat(df, StatKind.DF);
                agi = body.ApplyToSoulStat(agi, StatKind.AGI);
                mat = body.ApplyToSoulStat(mat, StatKind.MAT);
                mdf = body.ApplyToSoulStat(mdf, StatKind.MDF);

                maxHp = body.MaxHp;
            }

            return new SoulSnapshot(at, df, agi, mat, mdf, maxHp);
        }

        public void AddPermanentBonus(StatKind kind, int delta)
        {
            var rein = CurrentReinSoul;
            rein?.AddPermanentBonus(kind, delta);
        }

        public void AddExp(int amount)
        {
            var rein = CurrentReinSoul;
            rein?.AddExp(amount);
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            UpdateSoulTypeFromIconId();
            ResolveJobsForReinSouls();
            ResolveIconSpriteFromDatabase();
        }
    }

    #region OneReinSoulData
    [Serializable]
    public class OneReinSoulData
    {
        [Header("識別情報")]
        [SerializeField] private string reincarnationId;
        [SerializeField] private string title;

        [Header("成長プロファイル")]
        [SerializeField] private int rank = 1; // ★ int rank に変更
        [SerializeField] private GrowthType growthType;

        [SerializeField] private string jobId;
        [NonSerialized] private SoulJobDefinition jobDef;

        [Header("Lv / EXP")]
        [SerializeField] private int level = 1;
        [SerializeField] private int currentExp = 0;

        [Header("成長・補正（ステータス計算用）")]
        [SerializeField] private float[] growthTargets = new float[5];
        [SerializeField] private int[] lv1Stats = new int[5];
        [SerializeField] private int[] permanentBonuses = new int[5];

        [Header("来歴・スキル")]
        [SerializeField] private List<ReinEvent> historyEvents;
        [SerializeField] private List<string> learnedSkillIds;

        public string ReincarnationId => reincarnationId;
        public string Title => title;

        public int Rank => Mathf.Max(1, rank); // ★ int rank
        public GrowthType GrowthType => growthType;

        public string JobId => jobId;
        public SoulJobDefinition JobDefinition => jobDef;

        public int Level => level;
        public int CurrentExp => currentExp;

        public IReadOnlyList<ReinEvent> HistoryEvents => historyEvents;
        public IReadOnlyList<string> LearnedSkillIds => learnedSkillIds;
        /// <summary>
        /// ランダム初期化専用の簡易版。
        /// 中身は汎用版 CreateFromArgs に委譲する。
        /// </summary>
        public static OneReinSoulData CreateRandomInitial(
            int rank,
            GrowthType growthType,
            SoulJobDefinition jobDef,
            TalentRank talent
        )
        {
            return CreateFromArgs(
                rank: rank,
                growthType: growthType,
                jobDef: jobDef,
                talent: talent,
                title: null,
                level: 1,
                lv1Stats: null,
                growthTargets: null,
                permanentBonuses: null,
                historyEvents: null,
                learnedSkillIds: null
            );
        }

        /// <summary>
        /// ランク / 成長タイプ / ジョブ / 才能ランクを必須として、
        /// Lv1ステ・成長ターゲット・永続ボーナス・来歴・スキルリストを
        /// 任意に上書きできる汎用初期化。
        /// null の項目は従来どおりのランダム生成 or 0 クリアで埋める。
        /// </summary>
        public static OneReinSoulData CreateFromArgs(
            int rank,
            GrowthType growthType,
            SoulJobDefinition jobDef,
            TalentRank talent,
            string title,
            int level,
            int[] lv1Stats,
            float[] growthTargets,
            int[] permanentBonuses,
            List<ReinEvent> historyEvents,
            List<string> learnedSkillIds,
            float[] eventFactors = null
        )
        {
            var data = new OneReinSoulData();

            // 基本情報
            data.reincarnationId = Guid.NewGuid().ToString("N");
            if (!string.IsNullOrEmpty(title))
            {
                data.title = title;
            }
            else
            {
                data.title = jobDef != null ? jobDef.JobName : "転生";
            }

            data.rank = Mathf.Max(1, rank);
            data.growthType = growthType;

            data.level = Mathf.Max(1, level);
            data.currentExp = 0;

            data.SetJob(jobDef);

            // 配列初期化（AT/DF/AGI/MAT/MDF の5要素）
            data.lv1Stats = new int[5];
            data.growthTargets = new float[5];
            data.permanentBonuses = new int[5];

            bool hasCustomLv1 = lv1Stats != null && lv1Stats.Length >= 5;
            bool hasCustomGrowth = growthTargets != null && growthTargets.Length >= 5;
            bool hasCustomBonus = permanentBonuses != null && permanentBonuses.Length >= 5;

            // 才能倍率は「転生につき1回」抽選し、全ステ共通で使う
            float talentFactor = SpaceJourneyStatMath.GetTalentFactor(talent);

            // ステータス生成 or 上書き
            for (int i = 0; i < 5; i++)
            {
                // ★ AT/DF/AGI/MAT/MDF を 0..4 に対応させたいので、+1 してキャスト
                StatKind kind = (StatKind)(i + 1);

                // Lv1 ステータス
                if (hasCustomLv1)
                {
                    data.lv1Stats[i] = Mathf.Max(1, lv1Stats[i]);
                }
                else
                {
                    float jobMul = jobDef != null ? jobDef.GetMultiplier(kind) : 1f;
                    float baseStat = SpaceJourneyStatMath.CalcBaseStat(data.rank, jobMul);
                    // eventFactors が渡された場合はそれを使用、なければ仮の定数を使用
                    float evFactor = (eventFactors != null && eventFactors.Length > i)
                        ? eventFactors[i]
                        : SpaceJourneyConstants.TempInitialReincarnationEventFactor;
                    float potential = SpaceJourneyStatMath.CalcPotentialStat(baseStat, talentFactor, evFactor);
                    int lv1 = SpaceJourneyStatMath.CalcLv1Stat(potential);
                    data.lv1Stats[i] = lv1;
                }

                // 成長ターゲット
                if (hasCustomGrowth)
                {
                    data.growthTargets[i] = Mathf.Max(1f, growthTargets[i]);
                }
                else
                {
                    data.growthTargets[i] = SpaceJourneyStatMath.GetRandomGrowthTarget(data.growthType);
                }

                // 永続ボーナス
                if (hasCustomBonus)
                {
                    data.permanentBonuses[i] = permanentBonuses[i];
                }
                else
                {
                    data.permanentBonuses[i] = 0;
                }
            }

            // 来歴・スキル
            data.historyEvents = historyEvents != null
                ? new List<ReinEvent>(historyEvents)
                : new List<ReinEvent>();

            var learned = learnedSkillIds != null
                ? new List<string>(learnedSkillIds)
                : new List<string>();

            // ジョブスキル（unlockRank <= rank）を自動で習得済みにする
            if (jobDef != null)
            {
                var sets = jobDef.SkillSets;
                if (sets != null)
                {
                    foreach (var set in sets)
                    {
                        if (set == null || set.skill == null) continue;
                        if (set.unlockRank > data.rank) continue;

                        string skillId = set.skill.SkillId;
                        if (!string.IsNullOrEmpty(skillId) && !learned.Contains(skillId))
                        {
                            learned.Add(skillId);
                        }
                    }
                }
            }

            data.learnedSkillIds = learned;

            return data;
        }



        public void SetJob(SoulJobDefinition def)
        {
            jobDef = def;
            jobId = def != null ? def.JobId : null;
        }

        public void ResolveJob(Func<string, SoulJobDefinition> resolver)
        {
            if (resolver == null || string.IsNullOrEmpty(jobId))
            {
                jobDef = null;
                return;
            }

            jobDef = resolver(jobId);
        }

        public int GetSoulStat(StatKind kind)
        {
            // lv1Stats/growthTargets/permanentBonuses は「AT/DF/AGI/MAT/MDF の5枠」なので、
            // StatKind(HP,AT,DF,AGI,MAT,MDF) を 0..4 に詰め直す必要がある。
            int idx = kind switch
            {
                StatKind.AT => 0,
                StatKind.DF => 1,
                StatKind.AGI => 2,
                StatKind.MAT => 3,
                StatKind.MDF => 4,
                _ => -1
            };

            if (idx < 0) return 0;
            if (lv1Stats == null || lv1Stats.Length <= idx) return 0;
            if (growthTargets == null || growthTargets.Length <= idx) return 0;

            int lv1 = lv1Stats[idx];
            float gTarget = growthTargets[idx];

            int baseSoul = SpaceJourneyStatMath.CalcSoulStat(
                lv1,
                level,
                growthType,
                gTarget
            );

            int bonus = (permanentBonuses != null && permanentBonuses.Length > idx) ? permanentBonuses[idx] : 0;
            return Mathf.Max(0, baseSoul + bonus);
        }


        public void AddPermanentBonus(StatKind kind, int delta)
        {
            int idx = (int)kind;
            if (idx < 0 || idx >= permanentBonuses.Length) return;
            permanentBonuses[idx] += delta;
        }

        public void AddExp(int amount)
        {
            if (amount <= 0) return;
            currentExp += amount;
        }

        public void SetLevel(int newLevel)
        {
            level = Mathf.Max(1, newLevel);
        }

        public void SetRank(int newRank)
        {
            rank = Mathf.Max(1, newRank);
        }
    }
    #endregion
}