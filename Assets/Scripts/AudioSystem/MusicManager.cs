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

            // Normalize and store the desired target volume for this track
            float targetVolume = Mathf.Clamp(musicData.volume, 0f, 1f);
            currentVolume = targetVolume;

            if (currentFadeCoroutine != null)
            {
                // Gracefully cancel any in-progress fade and restore a consistent audio state
                CancelCurrentFadeAndStabilize();
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
                // Ensure no partial fades remain before starting fade out
                CancelCurrentFadeAndStabilize();
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
            float targetVol = currentVolume; // read shared target volume so runtime volume changes affect fades

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeTime;
                float curveValue = fadeCurve.Evaluate(progress);

                // Fade in new source (respecting the shared target volume)
                newSource.volume = curveValue * targetVol;

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
            
            // Finalize volumes to the shared target
            float finalTarget = currentVolume;
            newSource.volume = finalTarget;
            currentClip = musicData.clip;
            currentVolume = finalTarget;
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
        /// Cancel any in-progress fade coroutine and restore a consistent audio state.
        /// Ensures one source is the active source at full/target volume and others are stopped at 0.
        /// </summary>
        private void CancelCurrentFadeAndStabilize()
        {
            if (currentFadeCoroutine == null) return;

            try
            {
                StopCoroutine(currentFadeCoroutine);
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to stop coroutine: {ex.Message}");
            }
            currentFadeCoroutine = null;
            if (audioSourcePool == null || audioSourcePool.Length == 0)
            {
                // Nothing to stabilize
                currentClip = null;
                isMusicPlaying = false;
                return;
            }

            // Find the loudest source (best candidate to preserve)
            float maxVol = -1f;
            int maxIdx = -1;
            for (int i = 0; i < audioSourcePool.Length; i++)
            {
                var s = audioSourcePool[i];
                if (s == null) continue;
                if (s.volume > maxVol)
                {
                    maxVol = s.volume;
                    maxIdx = i;
                }
            }

            if (maxIdx >= 0 && maxVol > 0.0001f)
            {
                // Use the loudest source as the active source
                currentSourceIndex = maxIdx;
                var active = audioSourcePool[maxIdx];
                // If it has a clip, keep it as currentClip
                currentClip = active.clip;
                // Normalize its volume to the configured currentVolume or 1
                currentVolume = Mathf.Approximately(currentVolume, 0f) ? active.volume : currentVolume;
                active.volume = currentVolume;
                if (!active.isPlaying) active.Play();

                // Stop and zero other sources
                for (int i = 0; i < audioSourcePool.Length; i++)
                {
                    if (i == maxIdx) continue;
                    var s = audioSourcePool[i];
                    if (s == null) continue;
                    s.volume = 0f;
                    if (s.isPlaying) s.Stop();
                }

                isMusicPlaying = true;
            }
            else
            {
                // No audible source: stop all and clear state
                for (int i = 0; i < audioSourcePool.Length; i++)
                {
                    var s = audioSourcePool[i];
                    if (s == null) continue;
                    s.volume = 0f;
                    if (s.isPlaying) s.Stop();
                }

                currentClip = null;
                isMusicPlaying = false;
                currentSourceIndex = 0;
            }
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
                // Clamp and store the desired target volume
                float clamped = Mathf.Clamp(value, 0f, 1f);
                currentVolume = clamped;

                // Apply the change to all audio sources so active/fading sources stay in sync.
                if (audioSourcePool == null) return;

                // Find current max to preserve relative fade ratios when possible
                float maxVol = 0f;
                for (int i = 0; i < audioSourcePool.Length; i++)
                {
                    var s = audioSourcePool[i];
                    if (s == null) continue;
                    if (s.volume > maxVol) maxVol = s.volume;
                }

                if (maxVol > 0.0001f)
                {
                    float scale = clamped / maxVol;
                    for (int i = 0; i < audioSourcePool.Length; i++)
                    {
                        var s = audioSourcePool[i];
                        if (s == null) continue;
                        s.volume = Mathf.Clamp01(s.volume * scale);
                    }
                }
                else
                {
                    // No current audible source: apply to the primary source and zero others
                    var current = GetCurrentAudioSource();
                    for (int i = 0; i < audioSourcePool.Length; i++)
                    {
                        var s = audioSourcePool[i];
                        if (s == null) continue;
                        if (s == current)
                            s.volume = clamped;
                        else
                            s.volume = 0f;
                    }
                }
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
                // Reflect paused state in public flag
                isMusicPlaying = false;
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
                // Reflect resumed state in public flag
                isMusicPlaying = true;
            }
        }
    }
}
