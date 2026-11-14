using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button startButton;
    public Button settingsButton;
    public Button exitButton;
    public Button backButton;
    public GameObject titleScreen;
    public GameObject settingsMenu;
    
    void Start()
    {
        TitleScreen();
    }
    void TitleScreen()
    {
        titleScreen.SetActive(true);
        startButton.onClick.AddListener(StartGame);
        settingsButton.onClick.AddListener(SettingsMenu);
        exitButton.onClick.AddListener(ExitGame);
        settingsMenu.SetActive(false);
    }

    void StartGame()
    {
        SceneManager.LoadScene("Prototype");
    }

    void SettingsMenu()
    {
        titleScreen.SetActive(false);
        settingsMenu.SetActive(true);
        backButton.onClick.AddListener(TitleScreen);
    }

    void ExitGame()
    {
        Application.Quit();
    }
}
