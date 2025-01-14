using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace BazarEsKrim
{
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource voSource;
        // [SerializeField] private AudioSource sfxSourceObject;

        public static AudioManager Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlaySFX(AudioClip clip, float volume)
        {
            // StopSFX();

            sfxSource.volume = volume;
            sfxSource.PlayOneShot(clip);
        }

        public void PlayVO(AudioClip clip)
        {
            // StopSFX();

            voSource.PlayOneShot(clip);
        }

        public void PlaySFX(AudioClip clip)
        {
            // StopSFX();
            sfxSource.volume = 1f;
            sfxSource.PlayOneShot(clip);
        }

        public AudioSource PlaySFXAndGetSource(AudioClip clip)
        {
            PlaySFX(clip);

            return sfxSource;
        }

        public void StopSFX()
        {
            sfxSource.Stop();
        }

        public void PlayMusic(AudioClip clip, float volume = 0.6f) // default volume
        {
            if (musicSource.clip != clip)
            {
                musicSource.clip = clip;
                musicSource.volume = volume;
                musicSource.Play();
            }
        }

        public void SetMusicVolume(float volume)
        {
            musicSource.DOFade(volume, 0.1f);
        }

        public float GetMusicVolume()
        {
            return musicSource.volume;
        }

        public void StopMusic()
        {
            musicSource.Stop();
            musicSource.clip = null;
        }
    }
}
