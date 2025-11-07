using UnityEngine;
using UnityEngine.UI;
using Dekora.Core;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;
    
    private void Start()
    {
        _playButton.onClick.AddListener(OnPlayClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
    }
    
    private void OnPlayClicked()
    {
        GameManager.Instance.TransitionToState(GameState.LevelSelection);
        // Ou carregar direto:
        // Managers.LevelManager.Instance.LoadLevel("TestLevel");
    }
    
    private void OnSettingsClicked()
    {
        GameManager.Instance.OpenSettings();
    }
    
    private void OnQuitClicked()
    {
        GameManager.Instance.QuitGame();
    }
}