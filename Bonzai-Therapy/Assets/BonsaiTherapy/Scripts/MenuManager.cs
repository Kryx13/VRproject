using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button settingsButton;
    public Button quitButton;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Slider volumeSlider;
    public Button closeSettingsButton;

    private AudioSource musicSource;

    void Start()
    {
        // Find the AudioSource in the scene (background music)
        musicSource = FindObjectOfType<AudioSource>();

        // Wire up button listeners
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(ToggleSettings);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        // Initialize volume slider
        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // Hide settings panel at start
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void ToggleSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;

        if (musicSource != null)
            musicSource.volume = volume;
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
