using System.Collections.Generic;
using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// RealtimeSkillDefinition.attachmentKeyframes に従って time-based で
    /// transform.localPosition / localRotation を駆動する。
    /// OnEnable で elapsed リセット (キャスト開始時点で表示 = ArrowEqOn)。
    /// </summary>
    public class AttachmentCurveDriver : MonoBehaviour
    {
        public List<AttachmentKeyframe> keyframes;
        private float elapsed;

        private void OnEnable() { elapsed = 0f; }

        private void LateUpdate()
        {
            elapsed += Time.deltaTime * RealtimeBattleManager.GlobalSpeed;
            Apply(elapsed);
        }

        /// <summary>エディタプレビュー用: 任意時刻で transform を更新</summary>
        public void Apply(float t)
        {
            if (keyframes == null || keyframes.Count == 0) return;

            // 時刻で並んでると仮定 (未ソートの場合最初で sort)
            // find bracket
            if (t <= keyframes[0].time)
            {
                transform.localPosition = keyframes[0].localPosition;
                transform.localRotation = Quaternion.Euler(keyframes[0].localEulerAngles);
                return;
            }
            if (t >= keyframes[keyframes.Count - 1].time)
            {
                var k = keyframes[keyframes.Count - 1];
                transform.localPosition = k.localPosition;
                transform.localRotation = Quaternion.Euler(k.localEulerAngles);
                return;
            }
            for (int i = 0; i < keyframes.Count - 1; i++)
            {
                var a = keyframes[i]; var b = keyframes[i + 1];
                if (t >= a.time && t <= b.time)
                {
                    float span = Mathf.Max(0.0001f, b.time - a.time);
                    float u = Mathf.Clamp01((t - a.time) / span);
                    transform.localPosition = Vector3.Lerp(a.localPosition, b.localPosition, u);
                    transform.localRotation = Quaternion.Slerp(
                        Quaternion.Euler(a.localEulerAngles),
                        Quaternion.Euler(b.localEulerAngles), u);
                    return;
                }
            }
        }
    }
}
