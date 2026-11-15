using UnityEngine;
using UnityEngine.Audio;

public class MixerController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    
    //Set the volume to the slider's value, adjusted logarithmically
    public void SetMasterVolume(float sliderValue)
    {
        audioMixer.SetFloat("MasterBus", Mathf.Log10(sliderValue) * 20);
    }
    public void SetMusicVolume(float sliderValue)
    {
        audioMixer.SetFloat("MusicBus", Mathf.Log10(sliderValue) * 20);
    }
    public void SetSoundEffectVolume(float sliderValue)
    {
        audioMixer.SetFloat("SoundEffectsBus", Mathf.Log10(sliderValue) * 20);
    }
}
