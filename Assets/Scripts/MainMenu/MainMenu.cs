using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class MainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Button StartButton;
    [SerializeField] private Button QuitButton;
    void Start()
    {
        // Add listener for the Start button to call the StartGame method when clicked
        StartButton.onClick.AddListener(StartGame);
        // Add listener for the Quit button to call the QuitGame method when clicked
        QuitButton.onClick.AddListener(QuitGame);
    }

    private void StartGame()
    {
       SceneManager.LoadScene(1);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
                if (Application.isEditor)
                {
                    // Stop playing the scene in the editor
                    EditorApplication.isPlaying = false;
                }
                else
        #endif  
            {
            // Quit the application
            Application.Quit();
        }
    }
}
