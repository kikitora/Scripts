using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// 戦場の特定セルを一時的に点滅ハイライトするコンポーネント。
    /// TestBattleStarter が各 tick 後に HighlightTick(manager) を呼ぶ。
    /// </summary>
    public class CellHighlighter : MonoBehaviour
    {
        [Header("参照")]
        public BattleFieldVisualizer fieldVisualizer;
        public BattleUnitSpawner unitSpawner;

        [Header("色設定")]
        [Tooltip("攻撃者マス (フェーズ1)")]
        public Color attackerColor = new(1f, 0.9f, 0.3f, 0.7f); // 黄
        [Tooltip("効果範囲マス (フェーズ2)")]
        public Color rangeColor = new(1f, 0.6f, 0.1f, 0.6f);    // 橙
        [Tooltip("被弾マス (フェーズ3)")]
        public Color targetColor = new(1f, 0.25f, 0.25f, 0.7f); // 赤

        [Header("タイミング (秒)")]
        [Tooltip("フェーズ1: 攻撃者マスの点滅時間")]
        public float attackerDuration = 0.4f;
        [Tooltip("フェーズ2: 効果範囲マスの点滅時間")]
        public float rangeDuration = 0.35f;
        [Tooltip("フェーズ3: 被弾マスの点滅時間")]
        public float targetDuration = 0.4f;
        [Tooltip("点滅周波数")]
        public float blinkHz = 4f;

        [Header("マス形状")]
        public float cellSize = 1f;
        public float cellYOffset = 0.02f;

        /// <summary>
        /// 現在の tick に発生したアクション/効果範囲を読み、2段階で点滅させる。
        /// フェーズ1: 攻撃者マス (黄)
        /// フェーズ2: スキル効果範囲マス (橙) — 実際に当たらなくても範囲全体が光る
        /// </summary>
        public void HighlightTick(BattleManager manager)
        {
            if (manager == null || fieldVisualizer == null) return;

            // 攻撃者マス
            var actorCells = new List<Vector3>();
            foreach (var actor in manager.SkillActorsThisTick)
            {
                if (actor == null) continue;
                if (TryGetCellWorldPos(manager, actor, out var w)) actorCells.Add(w);
            }

            // 効果範囲マス (実セル、当たった/当たらない問わず)
            var rangeCells = new List<Vector3>();
            foreach (var (side, cell) in manager.SkillRangeCellsThisTick)
            {
                if (!manager.Field.IsValidCell(cell)) continue;
                var w = fieldVisualizer.GridToWorldPosition(side, cell.x, cell.y);
                w.y += cellYOffset;
                rangeCells.Add(w);
            }

            // フェーズ1: 攻撃者マス (即時)
            foreach (var pos in actorCells)
                StartCoroutine(BlinkQuad(pos, attackerColor, attackerDuration));

            // フェーズ2: 効果範囲 (attackerDuration 後)
            if (rangeCells.Count > 0)
                StartCoroutine(DelayedBlink(rangeCells, rangeColor, rangeDuration, attackerDuration));
        }

        private bool TryGetCellWorldPos(BattleManager manager, SpaceJourneyUnit unit, out Vector3 worldPos)
        {
            worldPos = default;
            var pos = manager.Field.FindUnit(unit);
            if (pos.x < 0) return false;
            int side = pos[0];
            worldPos = fieldVisualizer.GridToWorldPosition(side, pos[1], pos[2]);
            worldPos.y += cellYOffset;
            return true;
        }

        private IEnumerator DelayedBlink(List<Vector3> positions, Color color, float duration, float delay)
        {
            yield return new WaitForSeconds(delay);
            foreach (var p in positions)
                StartCoroutine(BlinkQuad(p, color, duration));
        }

        private IEnumerator BlinkQuad(Vector3 worldPos, Color color, float duration)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = "HighlightCell";
            go.transform.SetParent(transform, worldPositionStays: true);
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.Euler(90, 0, 0); // XZ平面に寝かせる
            go.transform.localScale = new Vector3(cellSize * 0.95f, cellSize * 0.95f, 1f);

            var col = go.GetComponent<Collider>();
            if (col != null) Destroy(col);

            // URP 対応の透過シェーダーを探す
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit")
                         ?? Shader.Find("Sprites/Default")
                         ?? Shader.Find("Unlit/Transparent");
            var mat = new Material(shader);
            mat.color = color;
            // URP Unlit 用の透過設定
            if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f); // 1 = transparent
            if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f); // 0 = alpha blend
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            mat.renderQueue = 3000;
            var renderer = go.GetComponent<Renderer>();
            renderer.material = mat;

            float elapsed = 0f;
            while (elapsed < duration && go != null)
            {
                elapsed += Time.deltaTime;
                float blinkT = Mathf.PingPong(elapsed * blinkHz, 1f);
                var c = color;
                c.a = color.a * blinkT;
                mat.color = c;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
                yield return null;
            }

            if (mat != null) Destroy(mat);
            if (go != null) Destroy(go);
        }
    }
}
