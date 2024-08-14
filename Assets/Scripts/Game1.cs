using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Game1 : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject PausePanel;
    public GameObject StartPanel;

    [Header("Game Elements")]
    public GameObject objectToDestroy;
    public Image Heart1;
    public Image Heart2;
    public Image Heart3;

    private int score = 0;
    private int hearts = 3;
    private bool isPaused = true;

    private void Start()
    {
        PauseGame(); // 게임 시작 시 일시정지 상태로 설정
        UpdateHearts(); // 하트 UI 초기화
    }

    #region Game Flow
    public void StartGame()
    {
        StartPanel.SetActive(false); // 시작 패널 비활성화
        ResumeGame(); // 게임 시작
    }

    public void Pause()
    {
        PausePanel.SetActive(true); // 일시정지 패널 활성화
        PauseGame();
    }

    public void ResumeGame()
    {
        PausePanel.SetActive(false); // 일시정지 패널 비활성화
        UnpauseGame();
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // 게임 속도를 0으로 설정하여 일시정지
        isPaused = true;
    }

    private void UnpauseGame()
    {
        Time.timeScale = 1f; // 게임 속도를 1로 설정하여 재개
        isPaused = false;
    }

    public void LeaveGame()
    {
        ActivateCanvasObjects();
        DestroyObjectToDestroy();
    }

    private void ActivateCanvasObjects()
    {
        // 모든 Canvas 오브젝트를 활성화
        Canvas[] allCanvasObjects = GameObject.FindObjectsOfType<Canvas>(true);
        foreach (Canvas canvasObject in allCanvasObjects)
        {
            canvasObject.gameObject.SetActive(true);
        }
    }

    private void DestroyObjectToDestroy()
    {
        if (objectToDestroy != null)
        {
            Destroy(objectToDestroy);
        }
    }
    #endregion

    #region Score Management
    public void IncreaseScore(int amount)
    {
        if (amount > 0) // 점수는 양수로만 증가하도록
        {
            score += amount;
            UpdateScoreUI();
        }
    }

    private void UpdateScoreUI()
    {
        // 점수 UI 업데이트 로직 추가
        Debug.Log("Score updated: " + score);
    }

    private void AnswerCorrect()
    {
        int pointsAwarded = 10; // 정답 시 획득하는 점수
        IncreaseScore(pointsAwarded);
        Debug.Log("Correct Answer! Score increased by " + pointsAwarded);
    }

    private void AnswerIncorrect()
    {
        DecreaseHearts();
        if (hearts <= 0)
        {
            GameOver();
        }
        else
        {
            Debug.Log("Incorrect Answer! Hearts remaining: " + hearts);
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
        // 게임 오버 로직 추가
        Debug.Log("Game Over! No more hearts.");
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