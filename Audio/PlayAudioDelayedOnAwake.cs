using UnityEngine;

namespace SteraCube.Audio
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class PlayAudioDelayedOnAwake : MonoBehaviour
    {
        [SerializeField]
        private AudioSource audioSource;

        [SerializeField, Min(0f)]
        private float delaySeconds = 1f;

        private void Awake()
        {
            ResolveAudioSource();
        }

        private void OnEnable()
        {
            var source = ResolveAudioSource();
            if (source == null || source.clip == null)
            {
                return;
            }

            source.Stop();
            source.playOnAwake = false;
            source.PlayDelayed(delaySeconds);
        }

        private void OnDisable()
        {
            var source = ResolveAudioSource();
            if (source != null)
            {
                source.Stop();
            }
        }

        private AudioSource ResolveAudioSource()
        {
            var source = audioSource;
            if (source == null && !TryGetComponent(out source))
            {
                source = GetComponentInChildren<AudioSource>(true);
            }

            if (source == null)
            {
                return null;
            }

            source.playOnAwake = false;
            audioSource = source;

            return source;
        }
    }
}
