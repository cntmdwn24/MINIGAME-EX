using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameData gameData;
    public GameObject gameOverPanel;
    public TMP_Text keyText;
    public TMP_Text coinText;

    public GameObject successMessagePrefab;
    public GameObject failureMessagePrefab;
    public GameObject lockedStageMessagePrefab; // "구매가 필요합니다" 메시지 프리팹
    public GameObject stageSelectedMessagePrefab; // "선택됨" 메시지 프리팹
    public GameObject SelectGamePanel;
    public GameObject MenuPanel;
    public GameObject StuffPanel;
    public Transform messageParent;

    public GameObject[] miniGamePrefabs;
    public Transform gameSpawnPoint;

    private const int STAGE_COST = 10;
    private int selectedStageIndex = -1;
    private GameObject currentMiniGameInstance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        UpdateUI();
    }

    public void GameStart()
    {
        MenuPanel.SetActive(true);
        StuffPanel.SetActive(true);
    }

    public void SelectGame()
    {
        SelectGamePanel.SetActive(true);
    }

    public void CloseSelect()
    {
        SelectGamePanel.SetActive(false);
    }

    public void OnStageButtonClick(int stageIndex)
    {
        if (gameData.Games[stageIndex])
        {
            selectedStageIndex = stageIndex;
            ShowMessage(stageSelectedMessagePrefab); // "스테이지 선택됨" 메시지
            CloseSelectGamePanel(); // 선택했을 때만 패널을 닫음
        }
        else
        {
            ShowMessage(lockedStageMessagePrefab); // "잘못된 스테이지 인덱스" 메시지
        }
    }

    public void UnlockStage(int stageIndex)
    {
        if (stageIndex >= 0 && stageIndex < gameData.Games.Length)
        {
            if (!gameData.Games[stageIndex])
            {
                gameData.Key -= STAGE_COST;
                UpdateUI();
                gameData.Games[stageIndex] = true;
                ShowMessage(successMessagePrefab); // "스테이지 잠금 해제 성공" 메시지
            }
        }
        else
        {
            ShowMessage(failureMessagePrefab); // "잘못된 스테이지 인덱스" 메시지
        }
    }

    public void PlaySelectedStage()
    {
        if (selectedStageIndex >= 0 && selectedStageIndex < miniGamePrefabs.Length)
        {
            if (gameData.Games[selectedStageIndex])
            {
                if (currentMiniGameInstance != null)
                {
                    Destroy(currentMiniGameInstance);
                }

                currentMiniGameInstance = Instantiate(miniGamePrefabs[selectedStageIndex], gameSpawnPoint.position, Quaternion.identity);
                MenuPanel.SetActive(false);
            }
            else
            {
                ShowMessage(lockedStageMessagePrefab); // "구매가 필요합니다" 메시지
            }
        }
        else
        {
            ShowMessage(stageSelectedMessagePrefab); // "선택됨" 메시지
            SelectGamePanel.SetActive(true);
            MenuPanel.SetActive(false);
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    [ContextMenu("To Json Data")]
    void SaveGameDataToJson()
    {
        string jsonData = JsonUtility.ToJson(gameData, true);
        string path = Path.Combine(Application.dataPath, "gameData.json");
        File.WriteAllText(path, jsonData);
    }

    [ContextMenu("From Json Data")]
    void LoadPlayerDataFromJson()
    {
        string path = Path.Combine(Application.dataPath, "gameData.json");
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            gameData = JsonUtility.FromJson<GameData>(jsonData);
            UpdateUI();
        }
    }

    void ShowMessage(GameObject messagePrefab)
    {
        if (messagePrefab != null && messageParent != null)
        {
            GameObject messageInstance = Instantiate(messagePrefab, messageParent);
            Destroy(messageInstance, 2f);
        }
    }

    void UpdateUI()
    {
        if (keyText != null)
        {
            keyText.text = "Keys: " + gameData.Key.ToString();
        }

        if (coinText != null)
        {
            coinText.text = "Coins: " + gameData.Coin.ToString();
        }
    }

    void CloseSelectGamePanel()
    {
        SelectGamePanel.SetActive(false);
        MenuPanel.SetActive(true);
    }

    [System.Serializable]
    public class GameData
    {
        public int Key;
        public int Coin;
        public bool[] Games = new bool[3];
    }
}
