using UnityEngine;
using UnityEngine.SceneManagement;
public class MapSelector : MonoBehaviour
{
    [SerializeField] private int sceneIndex =2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
    
}
