using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    InputAction menuAction;
    Canvas pauseCanvas;
    GameObject mainPanel;
    GameObject settingsPanel;
    bool isPaused;
    static readonly Color textColor = new Color(0.996f, 0.839f, 0.996f, 1f); // #FED6FE

    void Start()
    {
        menuAction = new InputAction("MenuButton", InputActionType.Button);
        menuAction.AddBinding("<XRController>{LeftHand}/menuButton");
        menuAction.performed += ctx => TogglePause();
        menuAction.Enable();

        BuildUI();
        pauseCanvas.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (menuAction != null)
        {
            menuAction.performed -= ctx => TogglePause();
            menuAction.Disable();
            menuAction.Dispose();
        }
    }

    void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    [Header("Panel Distance")]
    [Tooltip("Distance of the pause panel in front of the player camera.")]
    float panelDistance = 1.5f;

    [Tooltip("Smoothing speed for panel follow movement.")]
    float followSpeed = 8f;

    void Update()
    {
        if (isPaused && pauseCanvas != null && pauseCanvas.gameObject.activeSelf)
            FollowPlayer();
    }

    void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        PositionInFrontOfPlayer();
        pauseCanvas.gameObject.SetActive(true);
        settingsPanel.SetActive(false);
    }

    void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseCanvas.gameObject.SetActive(false);
    }

    void PositionInFrontOfPlayer()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = cam.transform.forward;
        forward.Normalize();

        pauseCanvas.transform.position = cam.transform.position + forward * panelDistance;
        pauseCanvas.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    void FollowPlayer()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = cam.transform.forward;
        forward.Normalize();

        Vector3 targetPos = cam.transform.position + forward * panelDistance;
        Quaternion targetRot = Quaternion.LookRotation(forward, Vector3.up);

        float dt = Time.unscaledDeltaTime;
        pauseCanvas.transform.position = Vector3.Lerp(pauseCanvas.transform.position, targetPos, followSpeed * dt);
        pauseCanvas.transform.rotation = Quaternion.Slerp(pauseCanvas.transform.rotation, targetRot, followSpeed * dt);
    }

    void BuildUI()
    {
        // World-space canvas
        GameObject canvasObj = new GameObject("PauseCanvas");
        canvasObj.transform.SetParent(transform);
        pauseCanvas = canvasObj.AddComponent<Canvas>();
        pauseCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = pauseCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400, 500);
        canvasRect.localScale = Vector3.one * 0.002f;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // Main panel
        mainPanel = CreatePanel(canvasObj.transform, "MainPanel", canvasRect.sizeDelta);

        float y = 180f;

        // Title
        CreateText(mainPanel.transform, "TitleText", "Paused", 48, y);
        y -= 100f;

        // Resume button
        CreateButton(mainPanel.transform, "ResumeButton", "Resume", y, Resume);
        y -= 80f;

        // Settings button
        CreateButton(mainPanel.transform, "SettingsButton", "Settings", y, ToggleSettings);
        y -= 80f;

        // Leave button
        CreateButton(mainPanel.transform, "LeaveButton", "Leave", y, LeaveToMenu);

        // Settings sub-panel
        settingsPanel = CreatePanel(canvasObj.transform, "SettingsPanel", new Vector2(380, 160));
        RectTransform settingsRect = settingsPanel.GetComponent<RectTransform>();
        settingsRect.anchoredPosition = new Vector2(0, -180f);

        CreateText(settingsPanel.transform, "VolumeLabel", "Volume", 28, 40f);
        CreateVolumeSlider(settingsPanel.transform, -20f);

        settingsPanel.SetActive(false);
    }

    GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        return panel;
    }

    void CreateText(Transform parent, string name, string content, int fontSize, float yPos)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(360, 60);
        rect.anchoredPosition = new Vector2(0, yPos);

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = textColor;
    }

    void CreateButton(Transform parent, string name, string label, float yPos, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 60);
        rect.anchoredPosition = new Vector2(0, yPos);

        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.25f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(action);

        // Button label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);

        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.sizeDelta = rect.sizeDelta;
        labelRect.anchoredPosition = Vector2.zero;

        Text text = labelObj.AddComponent<Text>();
        text.text = label;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 32;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = textColor;
    }

    void CreateVolumeSlider(Transform parent, float yPos)
    {
        GameObject sliderObj = new GameObject("VolumeSlider");
        sliderObj.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(300, 30);
        sliderRect.anchoredPosition = new Vector2(0, yPos);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = AudioListener.volume;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-20, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.4f, 0.8f, 0.4f, 1f);

        // Handle area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-20, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handle.AddComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImg;

        slider.onValueChanged.AddListener(SetVolume);
    }

    void ToggleSettings()
    {
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    void LeaveToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene("MenuScene 1");
    }
}
