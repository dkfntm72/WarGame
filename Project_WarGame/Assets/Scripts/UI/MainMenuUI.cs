using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button settingsButton;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
    }

    private void OnStartClicked()
    {
        SceneManager.LoadScene("StageSelect");
    }

    private void OnSettingsClicked()
    {
        // TODO: 설정 패널 열기
        Debug.Log("[MainMenu] 설정 버튼 클릭");
    }
}
