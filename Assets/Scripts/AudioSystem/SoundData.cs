using System;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem {
    /// <summary>
    /// Holds all the settings for how a sound should play.
    /// Create these as ScriptableObjects or configure them in the inspector.
    /// Contains everything from volume and pitch to 3D spatial settings.
    /// </summary>
    [Serializable]
    public class SoundData {
        /// <summary>
        /// The actual audio file to play.
        /// Drag your audio clip here in the inspector.
        /// </summary>
        public AudioClip clip;
        
        /// <summary>
        /// Which audio mixer group this sound plays through.
        /// Use this to route sounds through different mixer channels (Music, SFX, Voice, etc).
        /// Leave empty to use the default output.
        /// </summary>
        public AudioMixerGroup mixerGroup;
        
        /// <summary>
        /// If true, the sound repeats forever until manually stopped.
        /// Great for background music or ambient loops.
        /// </summary>
        public bool loop;
        
        /// <summary>
        /// If true, the sound starts playing immediately when created.
        /// Usually leave this false and trigger sounds manually.
        /// </summary>
        public bool playOnAwake;
        
        /// <summary>
        /// Mark this true for sounds that play very often (gunshots, footsteps, etc).
        /// The system will limit how many can play at once to prevent audio overload.
        /// </summary>
        public bool frequentSound;
        
        /// <summary>
        /// If true, the sound plays silently.
        /// Useful for debugging or temporarily disabling sounds.
        /// </summary>
        public bool mute;
        
        /// <summary>
        /// If true, skips any effects applied to the AudioSource.
        /// Use this for sounds that need to be clean and unprocessed.
        /// </summary>
        public bool bypassEffects;
        
        /// <summary>
        /// If true, ignores any effects from the Audio Listener.
        /// Rare to use, but available if needed.
        /// </summary>
        public bool bypassListenerEffects;
        
        /// <summary>
        /// If true, this sound won't be affected by reverb zones in your scene.
        /// Good for UI sounds that shouldn't sound like they're in a cave.
        /// </summary>
        public bool bypassReverbZones;
        
        /// <summary>
        /// Priority of this sound (0 = highest, 256 = lowest).
        /// When too many sounds play at once, lower priority sounds get cut off first.
        /// Default is 128 (medium priority).
        /// </summary>
        public int priority = 128;
        
        /// <summary>
        /// How loud the sound plays (0 = silent, 1 = full volume).
        /// You can go above 1 to boost quiet sounds, but be careful of distortion.
        /// </summary>
        public float volume = 1f;
        
        /// <summary>
        /// How high or low the sound plays (1 = normal pitch).
        /// Lower values make it deeper, higher values make it squeakier.
        /// Useful for adding variety to repeated sounds.
        /// </summary>
        public float pitch = 1f;
        
        /// <summary>
        /// Stereo pan position (-1 = full left, 0 = center, 1 = full right).
        /// Only works for 2D sounds (when spatialBlend is 0).
        /// </summary>
        public float panStereo;
        
        /// <summary>
        /// Blends between 2D and 3D sound (0 = pure 2D, 1 = pure 3D).
        /// 2D sounds play the same everywhere (UI, music).
        /// 3D sounds get quieter with distance and have directional audio.
        /// </summary>
        public float spatialBlend;
        
        /// <summary>
        /// How much reverb affects this sound (0 = none, 1 = full reverb).
        /// Higher values make sounds more echoey in reverb zones.
        /// </summary>
        public float reverbZoneMix = 1f;
        
        /// <summary>
        /// Doppler effect strength (pitch change when sound source moves fast).
        /// 0 = no doppler, higher values = stronger effect.
        /// Think of a race car zooming past you.
        /// </summary>
        public float dopplerLevel = 1f;
        
        /// <summary>
        /// Angle of the 3D sound spread in degrees (0-360).
        /// 0 = sound comes from a point, 360 = sound comes from everywhere.
        /// </summary>
        public float spread;
        
        /// <summary>
        /// Distance where 3D sound is at full volume.
        /// Closer than this = same volume, further = starts getting quieter.
        /// </summary>
        public float minDistance = 1f;
        
        /// <summary>
        /// Distance where 3D sound stops getting quieter.
        /// Beyond this point, volume stays at the minimum level.
        /// </summary>
        public float maxDistance = 500f;
        
        /// <summary>
        /// If true, this sound ignores the master volume control.
        /// Rarely needed, but useful for critical audio that must always play.
        /// </summary>
        public bool ignoreListenerVolume;
        
        /// <summary>
        /// If true, this sound keeps playing even when the game is paused.
        /// Good for pause menu sounds or UI feedback.
        /// </summary>
        public bool ignoreListenerPause;
        
        /// <summary>
        /// How the sound volume decreases over distance.
        /// Logarithmic (default) = realistic falloff.
        /// Linear = volume drops evenly with distance.
        /// Custom = use your own curve (set in AudioSource).
        /// </summary>
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    }
}