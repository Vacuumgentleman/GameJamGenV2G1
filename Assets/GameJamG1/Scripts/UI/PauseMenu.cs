using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using GameJamG1.Utils;

[DefaultExecutionOrder(100)]
public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject instructionsMenu;
    public CursorLockController lockController;
    public bool isPaused;
    public Button resumeButton;
    public Button instructionsButton;
    public Button settingsButton;
    public Button returnToMenu;
    public Button[] backToPause;
    void Start()
    {
        pauseMenu.SetActive(false);
        instructionsMenu.SetActive(false);
        settingsMenu.SetActive(false);
        isPaused = false;
        resumeButton.onClick.AddListener(ResumeGame);
        instructionsButton.onClick.AddListener(Instructions);
        settingsButton.onClick.AddListener(SettingsMenu);
        returnToMenu.onClick.AddListener(BackToMenu);
        backToPause[0].onClick.AddListener(PauseGame);
        backToPause[1].onClick.AddListener(PauseGame);
        Time.timeScale = 1f;
    }

    private void Awake()
    {
        lockController = FindFirstObjectByType<CursorLockController>();
    }

    void Update()
    {
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if(isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
    public void PauseGame()
    {
        
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
        instructionsMenu.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
        lockController.Unlock();
    }

    public void ResumeGame()
    {
        
        Debug.Log("resume");
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        Debug.Log(lockController);
        lockController.Lock();
        Time.timeScale = 1f;
        isPaused = false;
        
    }

    public void Instructions()
    {
        pauseMenu.SetActive(false);
        instructionsMenu.SetActive(true);
    }

    public void SettingsMenu()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
}
