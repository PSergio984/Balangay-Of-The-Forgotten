using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
namespace AudioSystem {
    /// <summary>
    /// Manages background music with smooth crossfading between tracks.
    /// Handles a playlist queue and automatically plays the next track when one finishes.
    /// Supports seamless transitions without awkward silence or abrupt cuts.
    /// </summary>
    [RequireComponent(typeof(MusicManager))]
    public class MusicManager : PersistentSingleton<MonoBehaviour> {
        /// <summary>
        /// How long (in seconds) it takes to fade from one track to another.
        /// Shorter = snappier transitions, longer = smoother blending.
        /// </summary>
        const float crossFadeTime = 1.0f;
        
        /// <summary>
        /// Tracks the current fade progress. 0 = not fading, positive = actively fading.
        /// </summary>
        float fading;
        
        /// <summary>
        /// The AudioSource playing the current track (fading in or fully playing).
        /// </summary>
        AudioSource current;
        
        /// <summary>
        /// The AudioSource playing the previous track (fading out).
        /// Gets destroyed once the fade completes.
        /// </summary>
        AudioSource previous;
        
        /// <summary>
        /// Queue of music tracks waiting to play.
        /// Automatically plays the next track when the current one finishes.
        /// </summary>
        readonly Queue<AudioClip> playlist = new();
        
        /// <summary>
        /// Music tracks to load into the playlist on Start.
        /// Set this in the inspector to have music ready from the beginning.
        /// </summary>
        [SerializeField] List<AudioClip> initialPlaylist;
        
        /// <summary>
        /// Which audio mixer group music should play through.
        /// Use this to control music volume separately from sound effects.
        /// </summary>
        [SerializeField] AudioMixerGroup musicMixerGroup;
        
        void Start() {
            if (initialPlaylist == null) return;
            foreach (var clip in initialPlaylist) {
                AddToPlaylist(clip);
            }
        }        
        /// <summary>
        /// Adds a music track to the end of the playlist queue.
        /// If nothing is playing, starts playing immediately.
        /// </summary>
        /// <param name="clip">The audio clip to add to the playlist.</param>
        public void AddToPlaylist(AudioClip clip) {
            playlist.Enqueue(clip);
            if (current == null && previous == null) {
                PlayNextTrack();
            }
        }
        
        /// <summary>
        /// Removes all tracks from the playlist.
        /// Doesn't stop the currently playing track.
        /// </summary>
        public void Clear() => playlist.Clear();
        
        /// <summary>
        /// Plays the next track in the playlist queue.
        /// Called automatically when a track finishes, or call manually to skip.
        /// </summary>
        public void PlayNextTrack() {
            if (playlist.TryDequeue(out AudioClip nextTrack)) {
                Play(nextTrack);
            }
        }
        
        /// <summary>
        /// Plays a specific music track with crossfade from the current track.
        /// If the same track is already playing, does nothing.
        /// Old track fades out while new track fades in smoothly.
        /// </summary>
        /// <param name="clip">The music clip to play.</param>
        public void Play(AudioClip clip) {
            // Don't restart the same track
            if (current && current.clip == clip) return;
            
            // Clean up any leftover previous track
            if (previous) {
                Destroy(previous);
                previous = null;
            }
            
            // Shift current to previous and create new current
            previous = current;
            current = gameObject.GetOrAdd<AudioSource>();
            current.clip = clip;
            current.outputAudioMixerGroup = musicMixerGroup;
            current.loop = false; // Tracks play once, then playlist advances
            current.volume = 0; // Start silent, will fade in
            current.bypassListenerEffects = true;
            current.Play();
            
            fading = 0.001f; // Start the crossfade
        }
        
        void Update() {
            HandleCrossFade();
            
            // Auto-advance playlist when track finishes
            if (current && !current.isPlaying && playlist.Count > 0) {
                PlayNextTrack();
            }
        }
        
        /// <summary>
        /// Handles the smooth volume crossfade between tracks.
        /// Uses logarithmic fading for more natural-sounding transitions.
        /// Cleans up the old track when fade completes.
        /// </summary>
        void HandleCrossFade() {
            if (fading <= 0f) return;
            
            fading += Time.deltaTime;
            float fraction = Mathf.Clamp01(fading / crossFadeTime);
            
            // Logarithmic fade sounds more natural than linear
            float logFraction = fraction.ToLogarithmicFraction();
            
            if (previous) previous.volume = 1.0f - logFraction; // Fade out
            if (current) current.volume = logFraction; // Fade in
            
            if (fraction >= 1) {
                fading = 0.0f;
                if (previous) {
                    Destroy(previous);
                    previous = null;
                }
            }
        }
    }
}