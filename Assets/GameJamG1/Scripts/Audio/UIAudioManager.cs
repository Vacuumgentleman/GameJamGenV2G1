using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance;

    public AudioSource sfxSource;
    public AudioClip[] hoverSound;
    public AudioClip clickSound;

    private void Awake()
    {
        // Hay uno en el menu y otro en la escena de juego, y sobreviven a los cambios de escena:
        // si ya existe uno, este sobra (si no, se acumulan copias al ir y volver del menu).
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayHoverSound()
    {
        if (sfxSource == null || hoverSound == null || hoverSound.Length == 0)
        {
            return;
        }

        var clip = hoverSound[Random.Range(0, hoverSound.Length)];
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }
}
