using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pausePanel;

    [Header("Buttons")]
    public Button menuButton;
    public Button resumeButton;
    public Button restartButton;
    public Button settingsButton;
    public Button quitButton;

    void Start()
    {
        menuButton.onClick.AddListener(OnMenuClicked);
        resumeButton.onClick.AddListener(OnResumeClicked);
        restartButton.onClick.AddListener(OnRestartClicked);
        settingsButton.onClick.AddListener(OnSettingsClicked);
        quitButton.onClick.AddListener(OnQuitClicked);
    }

    void OnMenuClicked()
    {
        TogglePauseMenu(true);
    }

    void OnResumeClicked()
    {
        TogglePauseMenu(false);
    }

    public void OnRestartClicked()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
        );
    }

    void OnSettingsClicked()
    {
        // TODO: Implement settings menu
    }

    void OnQuitClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelection");
    }

    public void TogglePauseMenu(bool show)
    {
        pausePanel.SetActive(show);

        menuButton.gameObject.SetActive(!show);

        if (show)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }
    }
}