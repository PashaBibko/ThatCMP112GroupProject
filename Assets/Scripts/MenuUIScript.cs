using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuUIScript : MonoBehaviour
{
    Scene GameScene;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
  
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStartButtonPress()
    {
        SceneManager.LoadScene("TrackKartScene");

    }

    public void OnQuitButtonPress()
    {
        Application.Quit();
    }
}
