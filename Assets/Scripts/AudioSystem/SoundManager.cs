using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace AudioSystem {
    /// <summary>
    /// Manages all audio playback in the game using an optimized pooling system.
    /// This prevents creating/destroying audio objects constantly, which improves performance.
    /// </summary>
    public class SoundManager : PersistentSingleton<SoundManager> {
        
        /// <summary>
        /// The pool that stores and manages reusable SoundEmitter objects.
        /// </summary>
        IObjectPool<SoundEmitter> soundEmitterPool;
        
        /// <summary>
        /// Tracks all sound emitters currently playing audio.
        /// </summary>
        readonly List<SoundEmitter> activeSoundEmitters = new();
        
        /// <summary>
        /// Tracks frequently played sounds (like gunshots, footsteps).
        /// Limits how many can play at once to avoid audio spam.
        /// </summary>
        public readonly LinkedList<SoundEmitter> FrequentSoundEmitters = new();
        
        /// <summary>
        /// The prefab used to create new sound emitter instances.
        /// Assign this in the Unity Inspector.
        /// </summary>
        [SerializeField] SoundEmitter soundEmitterPrefab;
        
        /// <summary>
        /// Safety check to make sure objects aren't accidentally put back in the pool twice.
        /// Turn off in production builds for better performance.
        /// </summary>
        [SerializeField] bool collectionCheck = true;
        
        /// <summary>
        /// How many sound emitters to create when the pool starts up.
        /// Higher = less lag when playing sounds, but uses more memory.
        /// </summary>
        [SerializeField] int defaultCapacity = 10;
        
        /// <summary>
        /// Maximum number of sound emitters the pool can hold.
        /// Extra ones get destroyed instead of pooled.
        /// </summary>
        [SerializeField] int maxPoolSize = 100;
        
        /// <summary>
        /// Maximum number of frequent sounds that can play at the same time.
        /// When this limit is reached, the oldest sound gets stopped.
        /// </summary>
        [SerializeField] int maxSoundInstances = 30;
        
        void Start() {
            InitializePool();
        }
        
        /// <summary>
        /// Creates a new SoundBuilder to play audio with custom settings.
        /// Use this to play sounds with specific volume, pitch, position, etc.
        /// </summary>
        /// <returns>A new SoundBuilder instance ready to configure.</returns>
        /// <example>
        /// soundManager.CreateSoundBuilder()
        ///     .WithSoundData(mySoundData)
        ///     .WithPosition(transform.position)
        ///     .WithVolume(0.8f)
        ///     .Play();
        /// </example>
        public SoundBuilder CreateSoundBuilder() => new SoundBuilder(this);
        
        /// <summary>
        /// Checks if a sound is allowed to play based on frequency limits.
        /// If too many frequent sounds are playing, stops the oldest one first.
        /// </summary>
        /// <param name="data">The sound data to check.</param>
        /// <returns>True if the sound can play, false if it should be blocked.</returns>
        public bool CanPlaySound(SoundData data) {
            // Non-frequent sounds always play
            if (!data.frequentSound) return true;
            
            // If we hit the limit, stop the oldest sound
            if (FrequentSoundEmitters.Count >= maxSoundInstances) {
                try {
                    FrequentSoundEmitters.First.Value.Stop();
                    return true;
                } catch {
                    Debug.Log("SoundEmitter is already released");
                }
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Gets a sound emitter from the pool. The emitter is ready to play audio.
        /// Don't call this directly - use CreateSoundBuilder() instead.
        /// </summary>
        /// <returns>An available SoundEmitter from the pool.</returns>
        public SoundEmitter Get() {
            return soundEmitterPool.Get();
        }
        
        /// <summary>
        /// Returns a sound emitter back to the pool when it's done playing.
        /// Don't call this directly - the SoundEmitter handles this automatically.
        /// </summary>
        /// <param name="soundEmitter">The emitter to return to the pool.</param>
        public void ReturnToPool(SoundEmitter soundEmitter) {
            soundEmitterPool.Release(soundEmitter);
        }
        
        /// <summary>
        /// Immediately stops all currently playing sounds.
        /// Useful for scene transitions or pause menus.
        /// </summary>
        public void StopAll() {
            foreach (var soundEmitter in activeSoundEmitters) {
                soundEmitter.Stop();
            }
            FrequentSoundEmitters.Clear();
        }
        
        /// <summary>
        /// Sets up the object pool with the configured settings.
        /// Called automatically on Start.
        /// </summary>
        void InitializePool() {
            soundEmitterPool = new ObjectPool<SoundEmitter>(
                CreateSoundEmitter,
                OnTakeFromPool,
                OnReturnedToPool,
                OnDestroyPoolObject,
                collectionCheck,
                defaultCapacity,
                maxPoolSize);
        }
        
        /// <summary>
        /// Creates a new SoundEmitter instance from the prefab.
        /// Called by the pool when it needs more objects.
        /// </summary>
        SoundEmitter CreateSoundEmitter() {
            var soundEmitter = Instantiate(soundEmitterPrefab);
            soundEmitter.gameObject.SetActive(false);
            return soundEmitter;
        }
        
        /// <summary>
        /// Prepares a sound emitter when it's taken from the pool.
        /// Activates the GameObject and adds it to the active list.
        /// </summary>
        void OnTakeFromPool(SoundEmitter soundEmitter) {
            soundEmitter.gameObject.SetActive(true);
            activeSoundEmitters.Add(soundEmitter);
        }
        
        /// <summary>
        /// Cleans up a sound emitter when it's returned to the pool.
        /// Removes it from tracking lists and deactivates the GameObject.
        /// </summary>
        void OnReturnedToPool(SoundEmitter soundEmitter) {
            if (soundEmitter.Node != null) {
                FrequentSoundEmitters.Remove(soundEmitter.Node);
                soundEmitter.Node = null;
            }
            soundEmitter.gameObject.SetActive(false);
            activeSoundEmitters.Remove(soundEmitter);
        }
        
        /// <summary>
        /// Destroys a sound emitter when the pool is full.
        /// Called automatically when the pool exceeds maxPoolSize.
        /// </summary>
        void OnDestroyPoolObject(SoundEmitter soundEmitter) {
            Destroy(soundEmitter.gameObject);
        }
    }
}