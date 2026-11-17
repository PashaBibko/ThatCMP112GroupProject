using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private BoxCollider boxCol;
    public TimerScript TimerScript;
    public GameObject GoalUI;
    public GameObject Enemy;
    public GameObject Player;
    void Start()
    {
        boxCol = GetComponent<BoxCollider>();
        boxCol.enabled = false;
        GoalUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(TimerScript.Timer > 15)
        {
            boxCol.enabled = true;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        string name = "";

        if (other.gameObject == Enemy)
        {
            Debug.Log("Enemy Passed the Finish Line");
            name = "The Enemy!!";
        }
        if(other.gameObject == Player)
        {
            Debug.Log("Player Passed the Finish Line");
            name = "Player!!";
        }

        ShowGoalUI(name);
    }

    private void ShowGoalUI(string name)
    {
        boxCol.gameObject.SetActive(false);
        GoalUI.SetActive(true);
        TimerScript.TimerRunning = false;
        Text WinnerText = GoalUI.transform.GetChild(1).gameObject.GetComponent<Text>();
        WinnerText.text = name;
    }

    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene("TitleScreen");
    }
}
