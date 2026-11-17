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
        // Los listeners se registran una sola vez: antes se volvian a añadir cada vez que se volvia
        // a la pantalla de titulo, y un clic en Empezar acababa cargando la escena varias veces.
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(SettingsMenu);
        exitButton.onClick.AddListener(ExitGame);
        creditsButton.onClick.AddListener(CreditsMenu);
        backButton[0].onClick.AddListener(TitleScreen);
        backButton[1].onClick.AddListener(TitleScreen);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        TitleScreen();
    }

    void TitleScreen()
    {
        titleScreen.SetActive(true);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false);
    }

    void StartGame()
    {
        startButton.interactable = false;
        SceneManager.LoadScene("Prototype");
    }

    void SettingsMenu()
    {
        titleScreen.SetActive(false);
        settingsMenu.SetActive(true);
    }

    void CreditsMenu()
    {
        creditsMenu.SetActive(true);
        titleScreen.SetActive(false);
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
