using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    public Button[] backButton;
    public Button creditsButton;
    public GameObject titleScreen;
    public GameObject settingsMenu;
    public GameObject creditsMenu;
    
    void Start()
    {
        TitleScreen();
    }
    void TitleScreen()
    {
        titleScreen.SetActive(true);
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(SettingsMenu);;
        exitButton.onClick.AddListener(ExitGame);
        creditsButton.onClick.AddListener(CreditsMenu);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

    void StartGame()
    {
        SceneManager.LoadScene("Prototype");
        
    }

    void SettingsMenu()
    {
        titleScreen.SetActive(false);
        settingsMenu.SetActive(true);
        backButton[0].onClick.AddListener(TitleScreen);
    }

    void CreditsMenu()
    {
        creditsMenu.SetActive(true);
        titleScreen.SetActive(false);
        backButton[1].onClick.AddListener(TitleScreen);
    }

    void ExitGame()
    {
        Application.Quit();
    }

    /*public AudioClip[] buttonsSFX; // Array de sonidos
    //public float stepsFrequency = 0.5f; // Determina la velocidad de los pasos
    //private PlayerController playerMovement; // Hace referencia al movimiento dentro del controlador

    // Para que funcione tenemos que actualizar el movimiento de private a public
    IEnumerator ButtonSound()
    {
        while (true)
        {
            if (playerMovement.movimiento.magnitude > 0.1f)
            {
                int randomSound = Random.Range(0, 3);
                AudioManager.instance.PlaySFX(footstepsSFX[randomSound], 0.5f, 0.95f, 1.02f);
                yield return new WaitForSeconds(stepsFrequency);
            }
            else
            {
                yield return null;
            }
        }
    }
    */
}
