using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Button button;
    void Start()
    {
        button.onClick.AddListener(LoadGame);
    }

    void LoadGame()
    {
        SceneManager.LoadScene("Prototype");
    }
}
