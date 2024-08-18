using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class Game1 : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject PausePanel;
    public GameObject StartPanel;

    [Header("Score UI")]
    public TMP_Text scoreText;

    [Header("Game Elements")]
    public GameObject objectToDestroy;
    public Image Heart1;
    public Image Heart2;
    public Image Heart3;

    [Header("Bear")]
    public Button bearButton1;
    public Button bearButton2;
    public Button bearButton3;
    public TMP_Text bearText1;
    public TMP_Text bearText2;
    public TMP_Text bearText3;

    public Color eatColor1 = Color.red;
    public Color eatColor2 = Color.green;
    public Color eatColor3 = Color.blue;

    private Color originalColor1;
    private Color originalColor2;
    private Color originalColor3;

    private int[] bearMeatCounts = new int[3];
    private int score = 0;
    private int hearts = 3;
    private bool isPaused = true;
    private bool isPlayerTurn = false;
    private bool isGameTurnRunning = false; // 게임 턴 중복 실행 방지 플래그
    private int gameTurnCounter = 0;

    private GameManager gameManager;

    private void Start()
    {
        PauseGame();
        UpdateHearts();
        gameManager = GameManager.Instance;
        UpdateScoreUI();

        // 원래 색상 저장
        originalColor1 = bearButton1.image.color;
        originalColor2 = bearButton2.image.color;
        originalColor3 = bearButton3.image.color;

        // 버튼 클릭 이벤트 등록
        bearButton1.onClick.AddListener(() => OnBearButtonClick(0));
        bearButton2.onClick.AddListener(() => OnBearButtonClick(1));
        bearButton3.onClick.AddListener(() => OnBearButtonClick(2));
    }

    #region Game Flow
    public void StartGame()
    {
        // 점수를 0으로 초기화
        score = 0;
        UpdateScoreUI();

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
        DestroyObjectToDestroy();
        if (gameManager != null)
        {
            gameManager.EndMiniGame(score);
        }
    }

    private void ActivateCanvasObjects()
    {
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

    #region Game Logic
    private IEnumerator GameTurn()
    {
        if (isGameTurnRunning) yield break; // 이미 게임 턴이 실행 중이면 종료

        isGameTurnRunning = true;
        isPlayerTurn = false;

        // 고기 개수 무작위로 할당
        SetBearMeatCounts();

        // 색상 변경 횟수 랜덤하게 결정
        List<int> indicesToChangeColor = GetIndicesToChangeColor();

        // 색상 변경 및 텍스트 업데이트
        foreach (int index in indicesToChangeColor)
        {
            yield return StartCoroutine(ChangeBearButtonColorAndUpdateText(index));
        }

        yield return new WaitForSeconds(1f); // 턴 종료 후 1초 대기

        // 플레이어 턴이 끝난 후 다음 게임 턴으로 넘어감
        gameTurnCounter++;
        isPlayerTurn = true;
        isGameTurnRunning = false;
    }

    private void SetBearMeatCounts()
    {
        // 고기 개수 설정 (최대 고기 개수 조정)
        int numButtons = bearMeatCounts.Length;
        int totalMeat = Random.Range(1, 10); // 총 고기 개수 설정
        List<int> meatCounts = new List<int>(new int[numButtons]);

        // 고기 개수 무작위로 분배
        for (int i = 0; i < totalMeat; i++)
        {
            int index = Random.Range(0, numButtons);
            meatCounts[index]++;
        }

        // 결과를 배열에 저장
        for (int i = 0; i < numButtons; i++)
        {
            bearMeatCounts[i] = meatCounts[i];
        }

        UpdateBearTexts();

        // 디버그 로그
        Debug.Log("Bear Meat Counts: " + string.Join(", ", bearMeatCounts));
    }

    private List<int> GetIndicesToChangeColor()
    {
        List<int> indices = new List<int>();

        for (int i = 0; i < bearMeatCounts.Length; i++)
        {
            int changeCount = bearMeatCounts[i];
            for (int j = 0; j < changeCount; j++)
            {
                indices.Add(i);
            }
        }

        ShuffleList(indices);
        return indices;
    }

    private IEnumerator ChangeBearButtonColorAndUpdateText(int index)
    {
        // 색상 랜덤 선택
        Color[] colors = { eatColor1, eatColor2, eatColor3 };
        Color newColor = colors[Random.Range(0, colors.Length)];

        Button button = GetButtonByIndex(index);

        if (button != null)
        {
            button.image.color = newColor;

            yield return new WaitForSeconds(1f);

            // 버튼 색상 원래대로 복원
            button.image.color = GetOriginalColorByIndex(index);
        }
    }

    private void UpdateBearTexts()
    {
        bearText1.text = bearMeatCounts[0].ToString();
        bearText2.text = bearMeatCounts[1].ToString();
        bearText3.text = bearMeatCounts[2].ToString();
    }

    private Button GetButtonByIndex(int index)
    {
        switch (index)
        {
            case 0: return bearButton1;
            case 1: return bearButton2;
            case 2: return bearButton3;
            default: return null;
        }
    }

    private Color GetOriginalColorByIndex(int index)
    {
        switch (index)
        {
            case 0: return originalColor1;
            case 1: return originalColor2;
            case 2: return originalColor3;
            default: return Color.white;
        }
    }

    public void OnBearButtonClick(int buttonIndex)
    {
        if (!isPlayerTurn) return;

        Debug.Log("Bear Button " + buttonIndex + " clicked!");

        StartCoroutine(ShowBearTexts());
        CheckBearSelection(buttonIndex);
    }

    private IEnumerator ShowBearTexts()
    {
        bearText1.gameObject.SetActive(true);
        bearText2.gameObject.SetActive(true);
        bearText3.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        bearText1.gameObject.SetActive(false);
        bearText2.gameObject.SetActive(false);
        bearText3.gameObject.SetActive(false);

        // 다음 턴으로 넘어감
        StartCoroutine(GameTurn());
    }

    private void CheckBearSelection(int buttonIndex)
    {
        int clickedBearMeatCount = bearMeatCounts[buttonIndex];
        int maxMeatCount = Mathf.Max(bearMeatCounts);

        if (clickedBearMeatCount == maxMeatCount)
        {
            Debug.Log("Correct choice!");
            int scoreToAdd = 10; // 점수: 10으로 설정
            IncreaseScore(scoreToAdd);
        }
        else
        {
            Debug.Log("Incorrect choice!");
            DecreaseHearts();
            if (hearts <= 0)
            {
                GameOver();
                return; // 게임 오버 시 다음 턴을 시작하지 않도록
            }
        }
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
        if (gameManager != null)
        {
            gameManager.EndMiniGame(score);
        }
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

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}