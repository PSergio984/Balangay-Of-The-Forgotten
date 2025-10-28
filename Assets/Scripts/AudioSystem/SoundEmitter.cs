using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
namespace AudioSystem {
    /// <summary>
    /// A pooled object that actually plays audio using Unity's AudioSource.
    /// Gets configured with SoundData, plays the sound, then returns itself to the pool.
    /// You don't create these manually - the SoundManager handles it through the pool.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour {
        /// <summary>
        /// The sound data this emitter was configured with.
        /// Contains all the audio settings and the clip being played.
        /// </summary>
        public SoundData Data { get; private set; }
        
        /// <summary>
        /// If this is a frequent sound, stores its position in the frequency tracking list.
        /// Used by the SoundManager to enforce limits on how many frequent sounds can play.
        /// </summary>
        public LinkedListNode<SoundEmitter> Node { get; set; }
        
        /// <summary>
        /// The Unity AudioSource component that actually plays the audio.
        /// </summary>
        AudioSource audioSource;
        
        /// <summary>
        /// Coroutine that waits for the sound to finish, then returns emitter to pool.
        /// </summary>
        Coroutine playingCoroutine;
        
        void Awake() {
            audioSource = gameObject.GetOrAdd<AudioSource>();
        }
        
        /// <summary>
        /// Configures this emitter with all settings from a SoundData.
        /// Copies every property from the data to the AudioSource.
        /// Called by the SoundBuilder before playing.
        /// </summary>
        /// <param name="data">The sound data containing all audio settings.</param>
        public void Initialize(SoundData data) {
            Data = data;
            
            // Basic settings
            audioSource.clip = data.clip;
            audioSource.outputAudioMixerGroup = data.mixerGroup;
            audioSource.loop = data.loop;
            audioSource.playOnAwake = data.playOnAwake;
            
            // Bypass settings
            audioSource.mute = data.mute;
            audioSource.bypassEffects = data.bypassEffects;
            audioSource.bypassListenerEffects = data.bypassListenerEffects;
            audioSource.bypassReverbZones = data.bypassReverbZones;
            
            // Volume and pitch
            audioSource.priority = data.priority;
            audioSource.volume = data.volume;
            audioSource.pitch = data.pitch;
            audioSource.panStereo = data.panStereo;
            audioSource.spatialBlend = data.spatialBlend;
            audioSource.reverbZoneMix = data.reverbZoneMix;
            audioSource.dopplerLevel = data.dopplerLevel;
            audioSource.spread = data.spread;
            
            // 3D spatial settings
            audioSource.minDistance = data.minDistance;
            audioSource.maxDistance = data.maxDistance;
            
            // Listener settings
            audioSource.ignoreListenerVolume = data.ignoreListenerVolume;
            audioSource.ignoreListenerPause = data.ignoreListenerPause;
            
            // Falloff
            audioSource.rolloffMode = data.rolloffMode;
        }
        
        /// <summary>
        /// Starts playing the audio and sets up auto-cleanup when finished.
        /// If already playing, stops the old coroutine and starts fresh.
        /// Non-looping sounds automatically return to pool when done.
        /// </summary>
        public void Play() {
            if (playingCoroutine != null) {
                StopCoroutine(playingCoroutine);
            }
            
            audioSource.Play();
            playingCoroutine = StartCoroutine(WaitForSoundToEnd());
        }
        
        /// <summary>
        /// Waits for the sound to finish playing, then calls Stop to return to pool.
        /// For looping sounds, this never completes (must call Stop manually).
        /// </summary>
        IEnumerator WaitForSoundToEnd() {
            yield return new WaitWhile(() => audioSource.isPlaying);
            Stop();
        }
        
        /// <summary>
        /// Stops playback immediately and returns this emitter to the pool.
        /// Cleans up the coroutine and tells SoundManager to recycle this object.
        /// Don't use this emitter after calling Stop - it's back in the pool.
        /// </summary>
        public void Stop() {
            if (playingCoroutine != null) {
                StopCoroutine(playingCoroutine);
                playingCoroutine = null;
            }
            
            audioSource.Stop();
            SoundManager.Instance.ReturnToPool(this);
        }
        
        /// <summary>
        /// Adds a small random pitch variation to the current pitch.
        /// Makes repeated sounds feel less robotic and more natural.
        /// Called by SoundBuilder if WithRandomPitch() was used.
        /// </summary>
        /// <param name="min">Minimum pitch offset (default: -0.05 for slightly lower).</param>
        /// <param name="max">Maximum pitch offset (default: 0.05 for slightly higher).</param>
        public void WithRandomPitch(float min = -0.05f, float max = 0.05f) {
            audioSource.pitch += Random.Range(min, max);
        }
    }
}