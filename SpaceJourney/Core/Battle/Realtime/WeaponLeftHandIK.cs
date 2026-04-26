using UnityEngine;

namespace SteraCube.SpaceJourney.Realtime
{
    /// <summary>
    /// 左手を leftHandTarget に IK で吸着させる。両手武器の握り位置合わせ用。
    /// EquipWeapon が target を設定し、武器を外したら null を入れる。
    /// Animator と同じ GameObject にアタッチすること（OnAnimatorIK 仕様）。
    /// 該当 Animator Controller のレイヤーで IK Pass を ON にする必要がある。
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class WeaponLeftHandIK : MonoBehaviour
    {
        public Transform leftHandTarget;
        [Range(0f, 1f)] public float weight = 1f;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private bool _logged;

        private void OnAnimatorIK(int layerIndex)
        {
            if (!_logged)
            {
                _logged = true;
                Debug.Log($"[WeaponLeftHandIK] OnAnimatorIK called on {name} layer={layerIndex} target={(leftHandTarget != null ? leftHandTarget.name : "null")}");
            }

            if (animator == null || leftHandTarget == null)
            {
                if (animator != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0f);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0f);
                }
                return;
            }

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, weight);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTarget.position);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTarget.rotation);
        }
    }
}
