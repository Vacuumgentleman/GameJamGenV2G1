using UnityEngine.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using GameJamG1.Utils;

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
        //Deactivate interfaces and set time to normal
        pauseMenu.SetActive(false);
        instructionsMenu.SetActive(false);
        settingsMenu.SetActive(false);
        isPaused = false;
        // Add listeners for each button
        resumeButton.onClick.AddListener(ResumeGame);
        instructionsButton.onClick.AddListener(Instructions);
        settingsButton.onClick.AddListener(SettingsMenu);
        returnToMenu.onClick.AddListener(BackToMenu);
        // Buttons to go back to pause menu
        backToPause[0].onClick.AddListener(PauseGame);
        backToPause[1].onClick.AddListener(PauseGame);
        // Set the time to normal
        Time.timeScale = 1f;
    }

    private void Awake()
    {
        //Find the cursor lock script
        lockController = FindFirstObjectByType<CursorLockController>();
    }

    void Update()
    {
        // If Escape is pressed, resume or pause the game.
        if(Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // If the game is paused, resume game. Else, pause the game.
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
        // Activate pause interface
        pauseMenu.SetActive(true);
        settingsMenu.SetActive(false);
        instructionsMenu.SetActive(false);
        // Pause the game by setting the time to 0
        Time.timeScale = 0f;
        isPaused = true;
        // Unlock the cursor to select inside pause menu
        lockController.Unlock();
    }

    public void ResumeGame()
    {
        
        //Debug.Log("resume");
        // Deactivate interfaces
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
        //Debug.Log(lockController);
        // Lock the cursor back to the fps mode
        lockController.Lock();
        // Set the time to normal
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
