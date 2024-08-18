using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    #region Game Data
    [Header("Game Data")]
    public GameData gameData;
    #endregion

    #region UI Elements
    [Header("UI Elements")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private GameObject selectGamePanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject stuffPanel;
    [SerializeField] private Transform messageParent;
    [SerializeField] private GameObject canvas;
    [SerializeField] private Sprite defaultStageImage;

    [Header("Selected Stage UI")]
    [SerializeField] private TMP_Text selectedStageText;
    [SerializeField] private Image selectedStageImage;
    #endregion

    #region Message Prefabs
    [Header("Message Prefabs")]
    [SerializeField] private GameObject successMessagePrefab;
    [SerializeField] private GameObject failureMessagePrefab;
    [SerializeField] private GameObject lockedStageMessagePrefab;
    [SerializeField] private GameObject stageSelectedMessagePrefab;
    #endregion

    #region Mini Game Settings
    [Header("Mini Game Settings")]
    [SerializeField] private GameObject[] miniGamePrefabs;
    [SerializeField] private Transform gameSpawnPoint;
    [SerializeField] private Sprite[] stageImages;
    [SerializeField] private string[] miniGameNames;

    private const int STAGE_COST = 10;
    private int selectedStageIndex = -1;
    private GameObject currentMiniGameInstance;
    #endregion

    #region Reward System
    [Header("Reward System")]
    [SerializeField] private Reward[] rewards;
    #endregion

    #region Unity Callbacks
    private void Awake()
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

    private void Start()
    {
        gameOverPanel.SetActive(false);
        UpdateUI();
        InitializeRewardButtons();
    }
    #endregion

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

    private void CloseSelectGamePanel()
    {
        selectGamePanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    private void ShowMessage(GameObject messagePrefab)
    {
        if (messagePrefab != null && messageParent != null)
        {
            GameObject messageInstance = Instantiate(messagePrefab, messageParent);
            Destroy(messageInstance, 2f);
        }
    }

    private void UpdateUI()
    {
        if (keyText != null) keyText.text = $"Keys: {gameData.key}";
        if (coinText != null) coinText.text = $"Coins: {gameData.coin}";
        UpdateSelectedStageDisplay();
    }

    private void UpdateSelectedStageDisplay()
    {
        if (selectedStageText != null)
        {
            if (IsValidStageIndex(selectedStageIndex))
            {
                selectedStageText.text = $"{miniGameNames[selectedStageIndex]} 선택됨";
                selectedStageImage.sprite = stageImages != null && stageImages.Length > selectedStageIndex
                    ? stageImages[selectedStageIndex]
                    : defaultStageImage;
            }
            else
            {
                selectedStageText.text = "선택된 미니게임이 없습니다";
                selectedStageImage.sprite = defaultStageImage;
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

    private bool IsValidStageIndex(int stageIndex)
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
        gameData.key += score / 10;
        gameData.gameScores[selectedStageIndex] = score;
        UpdateUI();
    }
    #endregion

    #region Reward System
    private void InitializeRewardButtons()
    {
        foreach (var reward in rewards)
        {
            if (reward.rewardButton != null)
            {
                Button localRewardButton = reward.rewardButton;
                localRewardButton.onClick.AddListener(() => CheckAndClaimReward(localRewardButton));
            }
        }
    }

    public void CheckAndClaimReward(Button rewardButton)
    {
        Reward reward = System.Array.Find(rewards, r => r.rewardButton == rewardButton);

        if (reward.rewardButton == null) return;

        if (!reward.rewardClaimed && gameData.key >= reward.requiredKeys)
        {
            ClaimReward(reward);
        }
        else
        {
            UpdateRewardButtonState(reward);
        }
    }

    private void ClaimReward(Reward reward)
    {
        gameData.coin += reward.rewardCoins;
        UpdateUI();
        ShowMessage(lockedStageMessagePrefab);

        var colors = reward.rewardButton.colors;
        colors.normalColor = Color.gray;
        reward.rewardButton.colors = colors;

        if (reward.PointImage != null)
        {
            reward.PointImage.gameObject.SetActive(false);
        }

        reward.rewardClaimed = true;
    }

    private void UpdateRewardButtonState(Reward reward)
    {
        var colors = reward.rewardButton.colors;

        if (gameData.key >= reward.requiredKeys)
        {
            colors.normalColor = Color.red;
            if (reward.PointImage != null)
            {
                reward.PointImage.gameObject.SetActive(true);
            }
        }
        else
        {
            colors.normalColor = Color.white;
            if (reward.PointImage != null)
            {
                reward.PointImage.gameObject.SetActive(false);
            }
        }

        reward.rewardButton.colors = colors;
    }
    #endregion

    #region Data Management
    [ContextMenu("To Json Data")]
    private void SaveGameDataToJson()
    {
        string jsonData = JsonUtility.ToJson(gameData, true);
        string path = Path.Combine(Application.dataPath, "Scripts", "GameData.json");
        File.WriteAllText(path, jsonData);
    }

    [ContextMenu("From Json Data")]
    private void LoadGameDataFromJson()
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

    #region Struct Definition
    [System.Serializable]
    public struct Reward
    {
        public int requiredKeys;
        public int rewardCoins;
        public Button rewardButton;
        public Image PointImage;
        public bool rewardClaimed;
    }
    #endregion

    [System.Serializable]
    public class GameData
    {
        public int key;
        public int coin;
        public bool[] games;
        public int[] gameScores;

        public GameData() { }

        public GameData(int numberOfStages)
        {
            key = 0;
            coin = 0;
            games = new bool[numberOfStages];
            gameScores = new int[numberOfStages];
        }
    }
}