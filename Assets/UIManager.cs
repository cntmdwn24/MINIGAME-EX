using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject StartPanel;

    public void GameStart()
    {
        StartPanel.SetActive(false);
    }
}