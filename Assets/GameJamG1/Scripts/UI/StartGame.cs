using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    private Button button;
    void Start()
    {
        button.onClick.AddListener(LoadGame);
    }

    void LoadGame()
    {
        SceneManager.LoadScene("Prototype");
    }
}
