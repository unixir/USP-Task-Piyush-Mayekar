using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Core
{
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private AudioSource musicSource, sfxSource;

        [Header("SFX")]
        [SerializeField] private AudioClip buttonTapSFX;
        [SerializeField] private List<SoundClip> soundClips;

        void Start()
        {
            var buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
            foreach (var button in buttons)
            {
                button.onClick.AddListener(() => PlaySFX(buttonTapSFX));
            }
        }

        private void PlaySFX(AudioClip clip)
        {
            sfxSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Plays one shot of a random SFX from the list of clips associated with the given name. If no clips are found, logs a warning.
        /// </summary>
        /// <param name="clipName"></param>
        public void PlaySFX(string clipName)
        {
            var clips = soundClips.Find(c => c.name == clipName)?.clips;
            var clip = clips != null && clips.Count > 0 ? clips[UnityEngine.Random.Range(0, clips.Count)] : null;
            if (clip != null)
            {
                PlaySFX(clip);
            }
            else
            {
                Debug.LogWarning($"SoundManager: No SFX found with name {clipName}");
            }
        }

        public void PlayMusic(string clipName)
        {
            var clips = soundClips.Find(c => c.name == clipName)?.clips;
            var clip = clips != null && clips.Count > 0 ? clips[UnityEngine.Random.Range(0, clips.Count)] : null;
            if (clip != null)
            {
                musicSource.clip = clip;
                musicSource.loop = true;
                musicSource.Play();
            }
            else
            {
                Debug.LogWarning($"SoundManager: No Music found with name {clipName}");
            }
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        [Serializable]
        public class SoundClip
        {
            public string name;
            public List<AudioClip> clips;
        }
    }
}