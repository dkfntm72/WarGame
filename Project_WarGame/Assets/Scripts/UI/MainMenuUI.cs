using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;
    public Button settingsButton;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Button     quitButton;

    private void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);

        settingsPanel?.SetActive(false);
        quitButton?.onClick.AddListener(QuitGame);

        var overlay = settingsPanel?.transform.Find("Overlay")?.GetComponent<Button>();
        overlay?.onClick.AddListener(CloseSettings);
    }

    private void OnStartClicked()  => SceneManager.LoadScene("StageSelect");
    private void OnSettingsClicked() => settingsPanel?.SetActive(true);
    private void CloseSettings()    => settingsPanel?.SetActive(false);

    private void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
