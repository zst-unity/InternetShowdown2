using System;
using DG.Tweening;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace InternetShowdown.Systems
{
    public static class AudioSystemExtensions
    {
        public static void Play(this AudioSystem.AudioProperties audioProperties, Vector3 position, bool spatialize = true)
        {
            AudioSystem.Play(audioProperties, position, spatialize);
        }

        public static void Play(this AudioSystem.AudioProperties audioProperties, Transform target, bool spatialize = true)
        {
            AudioSystem.Play(audioProperties, target, spatialize);
        }

        public static void PlaySFX(this AudioSystem.AudioProperties audioProperties, Vector3 position, bool spatialize)
        {
            AudioSystem.PlaySFX(audioProperties, position, spatialize);
        }

        public static void PlaySFX(this AudioSystem.AudioProperties audioProperties, Transform target, bool spatialize)
        {
            AudioSystem.PlaySFX(audioProperties, target, spatialize);
        }
    }

    public class AudioSystem : NetworkSystem<AudioSystem>
    {
        private AudioClip[] _audioClips;

        protected override void OnStartup()
        {
            _audioClips = Resources.LoadAll<AudioClip>("Audio");
        }

        public static void Play(AudioProperties audioProperties, Vector3 position, bool spatialize = true)
        {
            Play(audioProperties, position, null, spatialize);
        }

        public static void Play(AudioProperties audioProperties, Transform target, bool spatialize = true)
        {
            Play(audioProperties, Vector3.zero, target, spatialize);
        }

        private static void Play(AudioProperties audioProperties, Vector3 position, Transform target, bool spatialize)
        {
            if (audioProperties.clips.Length == 0)
            {
                Debug.LogWarning($"Cant play sfx with no audio clips specified");
                return;
            }

            var audio = audioProperties.clips[Random.Range(0, audioProperties.clips.Length)];
            var sourceObject = new GameObject($"Audio Effect: {audio.name}");
            sourceObject.transform.position = position;

            var audioEffect = sourceObject.AddComponent<AudioEffect>();
            audioEffect.destroyTime = audio.length;
            if (target) audioEffect.target = target;

            var audioSource = sourceObject.AddComponent<AudioSource>();
            audioSource.clip = audio;
            audioSource.pitch = Random.Range(audioProperties.pitchRange.x, audioProperties.pitchRange.y);
            audioSource.volume = audioProperties.volume;

            if (spatialize)
            {
                audioSource.rolloffMode = AudioRolloffMode.Custom;
                audioSource.maxDistance = audioProperties.maxDistance;

                audioSource.spatialBlend = 1;
                audioSource.dopplerLevel = 0;
            }
            audioSource.Play();

            if (audioProperties.shake.x <= 0 || audioProperties.shake.y <= 0 || audioProperties.shake.z <= 0) return;
            Camera.main.DOComplete();
            Camera.main.DOShakePosition
            (
                duration: audioProperties.shake.x,
                strength: (1 - Mathf.Clamp01(Vector3.Distance(Camera.main.transform.position, audioEffect.transform.position) / audioProperties.maxDistance)) * audioProperties.shake.y,
                vibrato: (int)audioProperties.shake.z
            );
        }

        public static void PlaySFX(AudioProperties audioProperties, Vector3 position, bool spatialize)
        {
            PlaySFX(audioProperties, position, null, spatialize);
        }

        public static void PlaySFX(AudioProperties audioProperties, Transform target, bool spatialize)
        {
            PlaySFX(audioProperties, Vector3.zero, target, spatialize);
        }

        private static void PlaySFX(AudioProperties audioProperties, Vector3 position, Transform target, bool spatialize)
        {
            if (!NetworkClient.active)
            {
                Debug.LogWarning($"Cant play sfx while not connected");
                return;
            }

            if (audioProperties.clips.Length == 0)
            {
                Debug.LogWarning($"Cant play sfx with no audio clips specified");
                return;
            }

            var audio = audioProperties.clips[Random.Range(0, audioProperties.clips.Length)];
            var audioIdx = Array.FindIndex(Singleton._audioClips, clip => clip == audio);

            if (audioIdx == -1)
            {
                Debug.LogWarning($"SFX \"{audio.name}\" not found");
                return;
            }

            Singleton.CmdPlaySFX(audioIdx, audioProperties.volume, audioProperties.pitchRange, audioProperties.maxDistance, audioProperties.shake, position, target, spatialize);
        }

        [Command(requiresAuthority = false)]
        private void CmdPlaySFX(int audioIdx, float volume, Vector2 pitchRange, float maxDistance, Vector3 shake, Vector3 position, Transform target, bool spatialize)
        {
            RpcPlaySFX(audioIdx, volume, pitchRange, maxDistance, shake, position, target, spatialize);
        }

        [ClientRpc]
        private void RpcPlaySFX(int audioIdx, float volume, Vector2 pitchRange, float maxDistance, Vector3 shake, Vector3 position, Transform target, bool spatialize)
        {
            var audioProperties = new AudioProperties
            {
                clips = new AudioClip[] { Singleton._audioClips[audioIdx] },
                pitchRange = pitchRange,
                volume = volume,
                maxDistance = maxDistance,
                shake = shake
            };

            Play(audioProperties, position, target, spatialize);
        }

        [Serializable]
        public class AudioProperties
        {
            public AudioClip[] clips;
            public float volume = 0.8f;
            public Vector2 pitchRange = Vector2.one;
            public float maxDistance = 85f;
            public Vector3 shake = new(0.35f, 0.5f, 10f);
        }

        public class AudioEffect : MonoBehaviour
        {
            public float destroyTime;
            public Transform target;

            private void Update()
            {
                if (target) transform.position = target.position;
            }

            private void Start()
            {
                Destroy(gameObject, destroyTime);
            }
        }
    }
}