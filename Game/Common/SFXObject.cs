using System.Collections;
using UnityEngine;

namespace Toggle.Game.Common
{
    public class SFXObject : MonoBehaviour
    {
        private AudioSource audioSource;
        private WaitUntil waitComplete;
        void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0.0f;    // 공간감 zero (2D)

            // WaitUntil 객체 캐싱
            waitComplete = new WaitUntil(() => !audioSource.isPlaying);
        }

        public void Play(AudioClip clip, float volume = 1, float pitch = 1)
        {
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.Play();

            StartCoroutine(DisableCoroutine());
        }

        // 재생이 끝났다면 오브젝트 비활성화
        IEnumerator DisableCoroutine()
        {
            yield return waitComplete;

            gameObject.SetActive(false);
        }
    }
}
