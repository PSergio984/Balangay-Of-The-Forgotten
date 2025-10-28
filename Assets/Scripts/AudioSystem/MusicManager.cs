using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

namespace AudioSystem
{
    /// <summary>
    /// Professional music manager with smooth crossfading and proper resource management.
    /// Optimized for card games and multi-scene architectures.
    /// </summary>
    public class MusicManager : PersistentSingleton<MusicManager>
    {
        [Header("🎵 Audio Configuration")]
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private int maxAudioSources = 2; // For crossfading
        
        [Header("🎚️ Fade Settings")]
        [SerializeField] private float defaultFadeTime = 2f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        // Audio source pool for efficient memory management
        private AudioSource[] audioSourcePool;
        private int currentSourceIndex = 0;
        private Coroutine currentFadeCoroutine;
        
        // Current state
        private AudioClip currentClip;
        private float currentVolume = 1f;
        private bool isMusicPlaying = false;
        
        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSources();
        }
        
        /// <summary>
        /// Initialize audio source pool for efficient crossfading
        /// </summary>
        private void InitializeAudioSources()
        {
            audioSourcePool = new AudioSource[maxAudioSources];
            
            for (int i = 0; i < maxAudioSources; i++)
            {
                GameObject sourceGO = new GameObject($"MusicSource_{i}");
                sourceGO.transform.SetParent(transform);
                
                AudioSource source = sourceGO.AddComponent<AudioSource>();
                source.outputAudioMixerGroup = musicMixerGroup;
                source.loop = true;
                source.playOnAwake = false;
                source.volume = 0f;
                
                audioSourcePool[i] = source;
            }
        }
        
        /// <summary>
        /// Play music with smooth crossfade. Perfect for your scene transitions.
        /// </summary>
        public void PlayMusic(SoundData musicData, float fadeTime = -1f)
        {
            if (musicData?.clip == null) return;
            
            float actualFadeTime = fadeTime > 0 ? fadeTime : defaultFadeTime;
            
            // Don't restart the same track
            if (currentClip == musicData.clip && isMusicPlaying) return;
            
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            
            currentFadeCoroutine = StartCoroutine(CrossFadeToClip(musicData, actualFadeTime));
        }
        
        /// <summary>
        /// Stop music with smooth fade out
        /// </summary>
        public void StopMusic(float fadeTime = -1f)
        {
            if (!isMusicPlaying) return;
            
            float actualFadeTime = fadeTime > 0 ? fadeTime : defaultFadeTime;
            
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
            }
            
            currentFadeCoroutine = StartCoroutine(FadeOutMusic(actualFadeTime));
        }
        
        /// <summary>
        /// Professional crossfade implementation without volume dips
        /// </summary>
        private IEnumerator CrossFadeToClip(SoundData musicData, float fadeTime)
        {
            AudioSource oldSource = GetCurrentAudioSource();
            AudioSource newSource = GetNextAudioSource();
            
            // Setup new source
            newSource.clip = musicData.clip;
            newSource.volume = 0f;
            newSource.pitch = musicData.pitch;
            newSource.Play();
            
            // Crossfade
            float elapsedTime = 0f;
            float oldStartVolume = oldSource ? oldSource.volume : 0f;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeTime;
                float curveValue = fadeCurve.Evaluate(progress);
                
                // Fade in new source
                newSource.volume = curveValue * musicData.volume;
                
                // Fade out old source
                if (oldSource != null)
                {
                    oldSource.volume = oldStartVolume * (1f - curveValue);
                }
                
                yield return null;
            }
            
            // Cleanup
            if (oldSource != null)
            {
                oldSource.Stop();
                oldSource.volume = 0f;
            }
            
            newSource.volume = musicData.volume;
            currentClip = musicData.clip;
            currentVolume = musicData.volume;
            isMusicPlaying = true;
            currentSourceIndex = GetSourceIndex(newSource);
            
            currentFadeCoroutine = null;
        }
        
        /// <summary>
        /// Fade out current music
        /// </summary>
        private IEnumerator FadeOutMusic(float fadeTime)
        {
            AudioSource currentSource = GetCurrentAudioSource();
            if (currentSource == null) yield break;
            
            float startVolume = currentSource.volume;
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeTime;
                currentSource.volume = startVolume * (1f - fadeCurve.Evaluate(progress));
                yield return null;
            }
            
            currentSource.Stop();
            currentSource.volume = 0f;
            isMusicPlaying = false;
            currentClip = null;
            
            currentFadeCoroutine = null;
        }
        
        /// <summary>
        /// Get currently playing audio source
        /// </summary>
        private AudioSource GetCurrentAudioSource()
        {
            if (audioSourcePool == null || audioSourcePool.Length == 0) return null;
            return audioSourcePool[currentSourceIndex];
        }
        
        /// <summary>
        /// Get next audio source for crossfading
        /// </summary>
        private AudioSource GetNextAudioSource()
        {
            int nextIndex = (currentSourceIndex + 1) % audioSourcePool.Length;
            return audioSourcePool[nextIndex];
        }
        
        /// <summary>
        /// Get index of audio source in pool
        /// </summary>
        private int GetSourceIndex(AudioSource source)
        {
            for (int i = 0; i < audioSourcePool.Length; i++)
            {
                if (audioSourcePool[i] == source) return i;
            }
            return 0;
        }
        
        // Public properties for external control
        public bool IsPlaying => isMusicPlaying;
        public AudioClip CurrentClip => currentClip;
        public float Volume 
        { 
            get => currentVolume;
            set 
            {
                currentVolume = value;
                AudioSource current = GetCurrentAudioSource();
                if (current != null) current.volume = value;
            }
        }
        
        /// <summary>
        /// Pause current music
        /// </summary>
        public void Pause()
        {
            AudioSource current = GetCurrentAudioSource();
            if (current != null && current.isPlaying)
            {
                current.Pause();
            }
        }
        
        /// <summary>
        /// Resume paused music
        /// </summary>
        public void Resume()
        {
            AudioSource current = GetCurrentAudioSource();
            if (current != null)
            {
                current.UnPause();
            }
        }
    }
}
