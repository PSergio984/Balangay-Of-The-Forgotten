using UnityEngine;
namespace AudioSystem {
    /// <summary>
    /// A builder pattern class for creating and playing sounds with custom settings.
    /// Use this to chain together sound configurations before playing.
    /// Get a builder by calling SoundManager.Instance.CreateSoundBuilder().
    /// </summary>
    /// <example>
    /// SoundManager.Instance.CreateSoundBuilder()
    ///     .WithPosition(enemy.position)
    ///     .WithRandomPitch()
    ///     .Play(explosionSound);
    /// </example>
    public class SoundBuilder {
        /// <summary>
        /// Reference to the sound manager that will handle pooling and playback.
        /// </summary>
        readonly SoundManager soundManager;
        
        /// <summary>
        /// The world position where the sound will play.
        /// Only matters for 3D sounds (when spatialBlend > 0).
        /// </summary>
        Vector3 position = Vector3.zero;
        
        /// <summary>
        /// If true, adds a small random pitch variation to the sound.
        /// Makes repeated sounds feel more natural and less robotic.
        /// </summary>
        bool randomPitch;
        
        /// <summary>
        /// Creates a new sound builder.
        /// Don't call this directly - use SoundManager.Instance.CreateSoundBuilder() instead.
        /// </summary>
        /// <param name="soundManager">The sound manager that owns this builder.</param>
        public SoundBuilder(SoundManager soundManager) {
            if (soundManager == null) {
                Debug.LogError("SoundManager cannot be null");
                throw new System.ArgumentNullException(nameof(soundManager));
            }
            this.soundManager = soundManager;
        }        
        /// <summary>
        /// Sets where in the world this sound should play.
        /// For 3D sounds, this affects volume and stereo positioning based on distance.
        /// For 2D sounds, this has no effect.
        /// </summary>
        /// <param name="position">World position for the sound.</param>
        /// <returns>This builder for method chaining.</returns>
        public SoundBuilder WithPosition(Vector3 position) {
            this.position = position;
            return this;
        }
        
        /// <summary>
        /// Adds a slight random pitch variation to make the sound more natural.
        /// Great for gunshots, footsteps, or any sound that repeats often.
        /// Prevents the "machine gun" effect where sounds feel robotic.
        /// </summary>
        /// <returns>This builder for method chaining.</returns>
        public SoundBuilder WithRandomPitch() {
            this.randomPitch = true;
            return this;
        }
        
        /// <summary>
        /// Plays the sound with all the configured settings.
        /// Gets a sound emitter from the pool, sets it up, and starts playback.
        /// The emitter automatically returns to the pool when the sound finishes.
        /// </summary>
        /// <param name="soundData">The sound data containing the audio clip and settings.</param>
        public void Play(SoundData soundData) {
            if (soundData == null) {
                Debug.LogError("SoundData is null");
                return;
            }
            
            // Check if we're allowed to play (frequency limits)
            if (!soundManager.CanPlaySound(soundData)) return;
            
            // Get an emitter from the pool and configure it
            SoundEmitter soundEmitter = soundManager.Get();
            soundEmitter.Initialize(soundData);
            soundEmitter.transform.position = position;
            soundEmitter.transform.parent = soundManager.transform;
            
            //randomize the pitch if it's requested
            if (randomPitch) {
                soundEmitter.WithRandomPitch();
            }
            
            // Track frequent sounds to enforce limits
            if (soundData.frequentSound) {
                soundEmitter.Node = soundManager.FrequentSoundEmitters.AddLast(soundEmitter);
            }
            
            soundEmitter.Play();
        }
    }
}