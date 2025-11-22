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

    public void GOSetActive(GameObject buttonGroup)
    {
        buttonGroup.SetActive(true);
    }

    public void GOSetDisable(GameObject buttonGroup)
    {
        buttonGroup.SetActive(false);
    }
    

}
