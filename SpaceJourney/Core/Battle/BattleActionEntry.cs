using System;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    // ================================================================
    // 行動リストのエントリ (最大10個/ユニット)
    // ================================================================
    // 上から順に条件チェック → 最初に全条件を満たしたスキルを実行。
    // 同じスキルを条件違いで複数回入れられる。
    // プレイヤーがカスタマイズ可能。おすすめボタンで作戦ベースの自動生成。
    // ================================================================

    [Serializable]
    public class BattleActionEntry
    {
        [Tooltip("使用するスキル。null の場合は待機扱い (cost=1)。")]
        public SkillDefinition skill;

        [Tooltip("AND で評価される条件リスト。空 = 無条件 (Always)。")]
        public List<ActionCondition> conditions = new();

        public BattleActionEntry() { }

        public BattleActionEntry(SkillDefinition skill, params ActionCondition[] conditions)
        {
            this.skill = skill;
            this.conditions = new List<ActionCondition>(conditions);
        }
    }

    // ================================================================
    // 行動条件
    // ================================================================

    [Serializable]
    public class ActionCondition
    {
        public ActionConditionKind kind = ActionConditionKind.Always;

        [Tooltip("整数パラメータ (N体以上 等)")]
        public int intParam = 0;

        [Tooltip("割合パラメータ (HP N% 以下 等。0.0~1.0)")]
        [Range(0f, 1f)]
        public float rateParam = 0f;

        public ActionCondition() { }

        public ActionCondition(ActionConditionKind kind, int intParam = 0, float rateParam = 0f)
        {
            this.kind = kind;
            this.intParam = intParam;
            this.rateParam = rateParam;
        }
    }

    // ================================================================
    // 条件の種類
    // ================================================================

    public enum ActionConditionKind
    {
        Always = 0,             // 無条件

        // ── 敵関連 ──
        EnemyInRange = 10,      // スキルの射程内に敵が1体以上いる
        EnemyCountInRange = 11, // 射程内の敵が intParam 体以上
        EnemyHpBelowRate = 12,  // 射程内に HP rateParam 以下の敵がいる
        NoEnemyForward = 13,    // 前方列に敵がいない

        // ── 味方関連 ──
        AllyInRange = 20,       // スキルの射程内に味方がいる
        AllyHpBelowRate = 21,   // 射程内に HP rateParam 以下の味方がいる
        NoAllyForward = 22,     // 前方に味方がいない (前衛不在)

        // ── 自分関連 ──
        SelfHpBelowRate = 30,   // 自分の HP が rateParam 以下
        SelfHpAboveRate = 31,   // 自分の HP が rateParam 以上

        // ── その他 ──
        SkillReady = 40,        // 特定スキルの CT が解けている (コンボ用)
    }

    // ================================================================
    // 簡単作戦 (おすすめプリセット)
    // ================================================================
    // プレイヤーが選ぶ「おすすめ」の方針。
    // 詳細画面では自分で全エントリを自由に並べ替え・条件変更できる。

    public enum BattleTactic
    {
        Balanced = 0,    // バランス (recommendedPriority そのまま)
        Offensive = 1,   // 攻撃重視 (攻撃スキル優先、回復後回し)
        Supportive = 2,  // 回復重視 (回復/補助優先)
        Defensive = 3,   // 守り重視 (Defend/バフ優先)
    }

    // ================================================================
    // スキルの戦術カテゴリ (作戦ボーナス計算用)
    // ================================================================

    public enum SkillTacticCategory
    {
        Attack,     // 攻撃 (物理/魔法ダメージ)
        Heal,       // 回復
        Buff,       // 防御/バフ (Defend, 自己強化)
        Debuff,     // デバフ
        Move,       // 移動
        Wait,       // 待機
    }
}
