using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;
    
    public void Start()
    {
        LoadVolume();
    }
    public void UpdateMusicVolume(float volume)
    {   
        audioMixer.SetFloat("MusicVolume", volume);
    } 
    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }
 
    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
 
        audioMixer.GetFloat("SFXVolume", out float SFXVolume);
        PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
    }

    public void LoadVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0f);
        
        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;
        
        audioMixer.SetFloat("MusicVolume", musicVolume);
        audioMixer.SetFloat("SFXVolume", sfxVolume);
    }

    [Header("UI References")]
    [SerializeField] private GameObject menuButtons;
    [SerializeField] private GameObject soundSettings;
    [SerializeField] private GameObject logos;

    /// <summary>
    /// Shows the sound settings panel and hides menu buttons and logos
    /// </summary>
    public void ShowSoundSettings()
    {
        if (soundSettings != null) soundSettings.SetActive(true);
        if (menuButtons != null) menuButtons.SetActive(false);
        if (logos != null) logos.SetActive(false);
    }

    /// <summary>
    /// Hides the sound settings panel and shows menu buttons and logos
    /// </summary>
    public void HideSoundSettings()
    {
        if (soundSettings != null) soundSettings.SetActive(false);
        if (menuButtons != null) menuButtons.SetActive(true);
        if (logos != null) logos.SetActive(true);
    }
    

}
