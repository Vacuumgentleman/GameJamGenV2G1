using UnityEngine;
using UnityEngine.Audio;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    public AudioSource sfxSource;
    public AudioClip[] hoverSound;
    public AudioClip clickSound;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    public void PlayHoverSound()
    {
        int randomSound = Random.Range(0, 2);
        sfxSource.PlayOneShot(hoverSound[randomSound]);
    }
    public void PlayClickSound()
    {
        sfxSource.PlayOneShot(clickSound);
    }


}
