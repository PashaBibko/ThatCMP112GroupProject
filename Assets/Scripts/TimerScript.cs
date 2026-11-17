using UnityEngine;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float Timer;
    public Text TimerText;
    public bool TimerRunning;
    void Start()
    {
        StartTimer();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(TimerRunning)
        {
            Timer = Timer + Time.deltaTime;
            float Result = (Mathf.Round(Timer * 10f)) / 10.0f; // round timer to 1 decimal place
            TimerText.text = ("TIME: " + Result);
        }
    }

    void StartTimer()
    {
        Timer = 0;
        TimerRunning = true;
    }

    void EndTimer()
    {
        TimerRunning = false;
        //add anything else here
    }

}
