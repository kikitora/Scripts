using UnityEngine;

namespace SteraCube.SpaceJourney
{
    /// <summary>
    /// AnimationClip に埋め込まれた AnimationEvent (SePlay / InstantiateInParts 等) を
    /// 空実装で受け取り、"has no receiver" 警告を抑制するだけのダミー。
    /// BattleUnitSpawner がモデル生成時に自動追加する。
    /// </summary>
    public class AnimationEventReceiver : MonoBehaviour
    {
        // SteraCube_Humanoid の各モーションクリップで使われるイベント群
        public void SePlay() { }
        public void SePlay(string key) { }
        public void SePlay(AnimationEvent e) { }

        public void InstantiateInParts() { }
        public void InstantiateInParts(string key) { }
        public void InstantiateInParts(AnimationEvent e) { }

        public void InstantiateRange() { }
        public void InstantiateRange(string key) { }
        public void InstantiateRange(AnimationEvent e) { }

        public void WaitDestroyParts() { }
        public void WaitDestroyParts(float sec) { }
        public void WaitDestroyParts(AnimationEvent e) { }

        // SteraCube_Humanoid 系クリップは Wait なし版も使う (Victory Idle, Telling A Secret, Sickle, Sakumotu, Roa, NIkutaiKouka, Mutant Idle, MagicStone, Jyouka, HeiwaNoIji, Bite 等)
        public void DestroyParts() { }
        public void DestroyParts(float sec) { }
        public void DestroyParts(AnimationEvent e) { }

        // Drinking, MagicStone, NIkutaiKouka, Sakumotu, Nusumu, Roa, Rangiri 等で使用
        public void HitStart() { }
        public void HitStart(string key) { }
        public void HitStart(AnimationEvent e) { }

        public void EndSkill() { }
        public void EndSkill(AnimationEvent e) { }

        // 追加で出そうなイベント名も空実装 (後で必要なら実装)
        public void StartSkill() { }
        public void HitImpact() { }
        public void Footstep() { }
        public void SpawnEffect() { }
        public void SpawnEffect(string key) { }
        public void DestroyEffect() { }

        // ExplosiveLLC パック用 AnimationEvent
        public void FootL() { }
        public void FootL(AnimationEvent e) { }
        public void FootR() { }
        public void FootR(AnimationEvent e) { }
        public void Hit() { }
        public void Hit(AnimationEvent e) { }
        public void Land() { }
        public void Land(AnimationEvent e) { }
        public void Fire() { }
        public void Fire(AnimationEvent e) { }
        public void Swing() { }
        public void Swing(AnimationEvent e) { }
        public void WeaponTrail() { }
        public void WeaponTrail(AnimationEvent e) { }

        // ─── 弓射系 (SteraCube 互換) ───
        /// <summary>矢を手に表示 (事前配置した子を Active)</summary>
        public void ArrowEqOn()
        {
            var rtu = GetComponentInParent<Realtime.RealtimeBattleUnit>();
            rtu?.ArrowEqOn();
        }
        public void ArrowEqOn(AnimationEvent e) { ArrowEqOn(); }

        /// <summary>矢を放つ (複製して飛翔、元は非表示)</summary>
        public void ArrowEqOff()
        {
            var rtu = GetComponentInParent<Realtime.RealtimeBattleUnit>();
            rtu?.ArrowEqOff();
        }
        public void ArrowEqOff(AnimationEvent e) { ArrowEqOff(); }
    }
}
