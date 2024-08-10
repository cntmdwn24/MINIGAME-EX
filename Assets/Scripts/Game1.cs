using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Game1 : MonoBehaviour
{
    public TMP_Text Score;
    public TMP_Text Bear1;
    public TMP_Text Bear2;
    public TMP_Text Bear3;
    public Button Pause;

    private int score = 0;
    private bool isPaused = false;

    void Start()
    {
        Pause.onClick.AddListener(TogglePause);

        UpdateScore();
        UpdateBearStatus();
    }

    void Update()
    {
        
    }

    void UpdateScore()
    {
        Score.text = "Score: " + score;
    }
    
    void UpdateBearStatus()
    {
        Bear1.text = "Bear 1: Active";
        Bear2.text = "Bear 2: Active";
        Bear3.text = "Bear 3: Active";
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        Pause.GetComponentInChildren<TMP_Text>().text = isPaused ? "Resume" : "Pause";
    }
}