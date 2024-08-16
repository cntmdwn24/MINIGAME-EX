using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Game3 : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject PausePanel;
    public GameObject StartPanel;

    [Header("Score UI")]
    public TMP_Text scoreText;

    [Header("Game Elements")]
    public Image Heart1;
    public Image Heart2;
    public Image Heart3;

    private int score = 0;
    private int hearts = 3;
    private bool isPaused = true;
    private bool isPlayerTurn = false;

    private void Start()
    {
        PauseGame();
        UpdateHearts();
        UpdateScoreUI();
    }

    #region Game Flow

    public void StartGame()
    {
        StartPanel.SetActive(false);
        ResumeGame();
        StartCoroutine(GameTurn());
    }

    public void Pause()
    {
        PausePanel.SetActive(true);
        PauseGame();
    }

    public void ResumeGame()
    {
        PausePanel.SetActive(false);
        UnpauseGame();
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
    }

    private void UnpauseGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void LeaveGame()
    {
        ActivateCanvasObjects();
    }

    private void ActivateCanvasObjects()
    {
        Canvas[] allCanvasObjects = GameObject.FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvasObject in allCanvasObjects)
        {
            canvasObject.gameObject.SetActive(true);
        }
    }

    #endregion

    #region Game Logic

    private IEnumerator GameTurn()
    {
        isPlayerTurn = false;

        yield return new WaitForSeconds(1f);

        isPlayerTurn = true;
    }

    #endregion

    #region Score Management

    public void IncreaseScore(int amount)
    {
        if (amount > 0)
        {
            score += amount;
            UpdateScoreUI();
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    private void DecreaseHearts()
    {
        if (hearts > 0)
        {
            hearts--;
            UpdateHearts();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! No more hearts.");
        LeaveGame();
    }

    #endregion

    #region Heart Management

    private void UpdateHearts()
    {
        Heart1.gameObject.SetActive(hearts > 0);
        Heart2.gameObject.SetActive(hearts > 1);
        Heart3.gameObject.SetActive(hearts > 2);
    }

    #endregion
}