using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem {
    /// <summary>
    /// ScriptableObject that holds all settings for how a sound should play.
    /// Create these as individual assets for each sound in your game.
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Sound Data", fileName = "New Sound Data")]
    public class SoundData : ScriptableObject {
        
        [Header("Audio Clip")]
        [Tooltip("The actual audio file to play. Drag your audio clip here in the inspector.")]
        [Required]
        public AudioClip clip;
        
        [Header("Mixer Routing")]
        [Tooltip("Which audio mixer group this sound plays through. Use this to route sounds through different mixer channels (Music, SFX, Voice, etc). Leave empty to use the default output.")]
        public AudioMixerGroup mixerGroup;
        
        [Header("Playback Settings")]
        [Tooltip("If true, the sound repeats forever until manually stopped. Great for background music or ambient loops.")]
        public bool loop;
        
        [Tooltip("If true, the sound starts playing immediately when created. Usually leave this false and trigger sounds manually.")]
        public bool playOnAwake;
        
        [Tooltip("Mark this true for sounds that play very often (gunshots, footsteps, etc). The system will limit how many can play at once to prevent audio overload.")]
        public bool frequentSound;
        
        [Header("Volume & Pitch")]
        [Tooltip("How loud the sound plays (0 = silent, 1 = full volume). You can go above 1 to boost quiet sounds, but be careful of distortion.")]
        [Range(0f, 2f)]
        public float volume = 1f;
        
        [Tooltip("How high or low the sound plays (1 = normal pitch). Lower values make it deeper, higher values make it squeakier. Useful for adding variety to repeated sounds.")]
        [Range(0.1f, 3f)]
        public float pitch = 1f;
        
        [Header("3D Sound Settings")]
        [Tooltip("Blends between 2D and 3D sound (0 = pure 2D, 1 = pure 3D). 2D sounds play the same everywhere (UI, music). 3D sounds get quieter with distance and have directional audio.")]
        [Range(0f, 1f)]
        public float spatialBlend;
        
        [Tooltip("Stereo pan position (-1 = full left, 0 = center, 1 = full right). Only works for 2D sounds (when spatialBlend is 0).")]
        [Range(-1f, 1f)]
        public float panStereo;
        
        [Tooltip("Distance where 3D sound is at full volume. Closer than this = same volume, further = starts getting quieter.")]
        [MinValue(0.1f)]
        public float minDistance = 1f;
        
        [Tooltip("Distance where 3D sound stops getting quieter. Beyond this point, volume stays at the minimum level.")]
        [MinValue(0.1f)]
        public float maxDistance = 500f;
        
        [Header("Advanced 3D Settings")]
        [Tooltip("How the sound volume decreases over distance. Logarithmic (default) = realistic falloff. Linear = volume drops evenly with distance. Custom = use your own curve (set in AudioSource).")]
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
        
        [Tooltip("How much reverb affects this sound (0 = none, 1 = full reverb). Higher values make sounds more echoey in reverb zones.")]
        [Range(0f, 1.1f)]
        public float reverbZoneMix = 1f;
        
        [Tooltip("Doppler effect strength (pitch change when sound source moves fast). 0 = no doppler, higher values = stronger effect. Think of a race car zooming past you.")]
        [Range(0f, 5f)]
        public float dopplerLevel = 1f;
        
        [Tooltip("Angle of the 3D sound spread in degrees (0-360). 0 = sound comes from a point, 360 = sound comes from everywhere.")]
        [Range(0f, 360f)]
        public float spread;
        
        [Header("Bypass Settings")]
        [Tooltip("If true, the sound plays silently. Useful for debugging or temporarily disabling sounds.")]
        public bool mute;
        
        [Tooltip("If true, skips any effects applied to the AudioSource. Use this for sounds that need to be clean and unprocessed.")]
        public bool bypassEffects;
        
        [Tooltip("If true, ignores any effects from the Audio Listener. Rare to use, but available if needed.")]
        public bool bypassListenerEffects;
        
        [Tooltip("If true, this sound won't be affected by reverb zones in your scene. Good for UI sounds that shouldn't sound like they're in a cave.")]
        public bool bypassReverbZones;
        
        [Header("Listener Settings")]
        [Tooltip("If true, this sound ignores the master volume control. Rarely needed, but useful for critical audio that must always play.")]
        public bool ignoreListenerVolume;
        
        [Tooltip("If true, this sound keeps playing even when the game is paused. Good for pause menu sounds or UI feedback.")]
        public bool ignoreListenerPause;
        
        [Header("Priority")]
        [Tooltip("Priority of this sound (0 = highest, 256 = lowest). When too many sounds play at once, lower priority sounds get cut off first. Default is 128 (medium priority).")]
        [Range(0, 256)]
        public int priority = 128;
        
        
        [HorizontalGroup("Info")]
        [LabelText("Duration")]
        [ShowInInspector, DisplayAsString, EnableGUI]
        public string DurationText => clip ? $"{clip.length:F2}s" : "N/A";
        
        [HorizontalGroup("Info")]
        [LabelText("Channels")]
        [ShowInInspector, DisplayAsString, EnableGUI]
        public string ChannelsText => clip ? clip.channels.ToString() : "N/A";
        
        [HorizontalGroup("Info")]
        [LabelText("Frequency")]
        [ShowInInspector, DisplayAsString, EnableGUI]
        public string FrequencyText => clip ? $"{clip.frequency}Hz" : "N/A";
    }
}