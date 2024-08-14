using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Game Data
    public GameData gameData;
    #endregion

    #region UI Elements
    public GameObject gameOverPanel;
    public TMP_Text keyText;
    public TMP_Text coinText;
    public GameObject selectGamePanel;
    public GameObject menuPanel;
    public GameObject stuffPanel;
    public Image panel;
    public Transform messageParent;
    public GameObject canvas;

    public TMP_Text selectedStageText;
    public Image selectedStageImage;

    #endregion

    #region Message Prefabs
    public GameObject successMessagePrefab;
    public GameObject failureMessagePrefab;
    public GameObject lockedStageMessagePrefab;
    public GameObject stageSelectedMessagePrefab;
    #endregion

    #region Mini Game Settings
    public GameObject[] miniGamePrefabs;
    public Transform gameSpawnPoint;
    public Sprite[] stageImages;
    private const int STAGE_COST = 10;
    private int selectedStageIndex = -1;
    private GameObject currentMiniGameInstance;
    private float time = 0f;
    private float fadeTime = 1f;
    #endregion

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

    #region UI Handling
    public void GameStart()
    {
        menuPanel.SetActive(true);
        stuffPanel.SetActive(true);
    }

    public void SelectGame()
    {
        selectGamePanel.SetActive(true);
    }

    public void CloseSelect()
    {
        selectGamePanel.SetActive(false);
    }

    void CloseSelectGamePanel()
    {
        selectGamePanel.SetActive(false);
        menuPanel.SetActive(true);
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
            keyText.text = "Keys: " + gameData.key.ToString();
        }

        if (coinText != null)
        {
            coinText.text = "Coins: " + gameData.coin.ToString();
        }

        UpdateSelectedStageDisplay();
    }

    void UpdateSelectedStageDisplay()
    {
        if (selectedStageText != null)
        {
            if (selectedStageIndex >= 0 && selectedStageIndex < miniGamePrefabs.Length)
            {
                selectedStageText.text = "Selected Stage: " + (selectedStageIndex + 1);
                if (selectedStageImage != null && stageImages != null && stageImages.Length > selectedStageIndex)
                {
                    selectedStageImage.sprite = stageImages[selectedStageIndex];
                }
            }
            else
            {
                selectedStageText.text = "No Stage Selected";
                if (selectedStageImage != null)
                {
                    selectedStageImage.sprite = null;
                }
            }
        }
    }
    #endregion

    #region Stage Management
    public void OnStageButtonClick(int stageIndex)
    {
        if (IsValidStageIndex(stageIndex) && gameData.games[stageIndex])
        {
            selectedStageIndex = stageIndex;
            UpdateSelectedStageDisplay();
            ShowMessage(stageSelectedMessagePrefab);
            CloseSelectGamePanel();
        }
        else
        {
            ShowMessage(lockedStageMessagePrefab);
        }
    }

    public void UnlockStage(int stageIndex)
    {
        if (IsValidStageIndex(stageIndex))
        {
            if (gameData.key >= STAGE_COST && !gameData.games[stageIndex])
            {
                gameData.key -= STAGE_COST;
                gameData.games[stageIndex] = true;
                UpdateUI();
                ShowMessage(successMessagePrefab);
            }
            else
            {
                ShowMessage(failureMessagePrefab);
            }
        }
        else
        {
            ShowMessage(failureMessagePrefab);
        }
    }

    bool IsValidStageIndex(int stageIndex)
    {
        return stageIndex >= 0 && stageIndex < gameData.games.Length;
    }
    #endregion

    #region Game Flow
    public void PlaySelectedStage()
    {
        if (IsValidStageIndex(selectedStageIndex) && gameData.games[selectedStageIndex])
        {
            if (currentMiniGameInstance != null)
            {
                Destroy(currentMiniGameInstance);
            }

            canvas.SetActive(false);
            currentMiniGameInstance = Instantiate(miniGamePrefabs[selectedStageIndex], gameSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            ShowMessage(lockedStageMessagePrefab);
        }
    }

    public void EndMiniGame(int score)
    {
        if (currentMiniGameInstance != null)
        {
            Destroy(currentMiniGameInstance);
        }
        canvas.SetActive(true);

        int keysToAdd = score / 10;
        gameData.key += keysToAdd;
        gameData.gameScores[selectedStageIndex] = score;
        UpdateUI();
    }
    #endregion

    #region Data Management
    [ContextMenu("To Json Data")]
    void SaveGameDataToJson()
    {
        string jsonData = JsonUtility.ToJson(gameData, true);
        string path = Path.Combine(Application.dataPath, "Scripts", "GameData.json");
        File.WriteAllText(path, jsonData);
    }

    [ContextMenu("From Json Data")]
    void LoadGameDataFromJson()
    {
        string path = Path.Combine(Application.dataPath, "Scripts", "GameData.json");
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            gameData = JsonUtility.FromJson<GameData>(jsonData);
            UpdateUI();
        }
    }
    #endregion

    #region Fade Animation
    public void Fade()
    {
        StartCoroutine(FadeFlow());
    }

    IEnumerator FadeFlow()
    {
        panel.gameObject.SetActive(true);
        time = 0f;
        Color alpha = panel.color;

        while (alpha.a < 1f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(0, 1, time);
            panel.color = alpha;
            yield return null;
        }

        time = 0f;
        yield return new WaitForSeconds(1f);

        while (alpha.a > 0f)
        {
            time += Time.deltaTime / fadeTime;
            alpha.a = Mathf.Lerp(1, 0, time);
            panel.color = alpha;
            yield return null;
        }

        panel.gameObject.SetActive(false);
    }
    #endregion

    [System.Serializable]
    public class GameData
    {
        public int key;
        public int coin;
        public bool[] games;
        public int[] gameScores;

        public GameData()
        {
        }

        public GameData(int numberOfStages)
        {
            key = 0;
            coin = 0;
            games = new bool[numberOfStages];
            gameScores = new int[numberOfStages];
        }
    }
}