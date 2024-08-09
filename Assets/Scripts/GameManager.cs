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

    public GameObject successMessagePrefab; // 성공 메시지 프리팹
    public GameObject failureMessagePrefab; // 실패 메시지 프리팹
    public Transform messageParent; // 메시지 프리팹을 소환할 부모 객체 (예: Canvas의 자식)

    private const int STAGE_COST = 10;

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

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnStageButtonClick(int stageIndex)
    {
        if (gameData.Key >= STAGE_COST)
        {
            gameData.Key -= STAGE_COST;
            UnlockStage(stageIndex);
            ShowMessage(successMessagePrefab); // 성공 메시지 표시
            UpdateUI();
        }
        else
        {
            ShowMessage(failureMessagePrefab); // 실패 메시지 표시
        }
    }

    void UnlockStage(int stageIndex)
    {
        if (stageIndex >= 0 && stageIndex < gameData.Games.Length)
        {
            gameData.Games[stageIndex] = true;
            Debug.Log("Stage " + stageIndex + " unlocked!");
        }
    }

    void ShowMessage(GameObject messagePrefab)
    {
        if (messagePrefab != null && messageParent != null)
        {
            GameObject messageInstance = Instantiate(messagePrefab, messageParent);
            Destroy(messageInstance, 2f); // 2초 후 메시지 제거
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

    [System.Serializable]
    public class GameData
    {
        public int Key;
        public int Coin;
        public bool[] Games = new bool[3];
    }
}