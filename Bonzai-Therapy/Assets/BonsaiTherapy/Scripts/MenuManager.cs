using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class MenuManager : MonoBehaviour
{
    Canvas menuCanvas;
    GameObject mainPanel;
    GameObject settingsPanel;

    static readonly Color textColor  = new Color(0.996f, 0.839f, 0.996f, 1f); // #FED6FE
    static readonly Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
    static readonly Color btnColor   = new Color(0.25f, 0.25f, 0.25f, 1f);

    float panelDistance = 1.5f;
    float followSpeed   = 2f;
    bool  placed;

    void Start()
    {
        BuildUI();
    }

    void LateUpdate()
    {
        if (menuCanvas == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = cam.transform.forward;
        forward.Normalize();

        Vector3 targetPos = cam.transform.position + forward * panelDistance;
        Quaternion targetRot = Quaternion.LookRotation(forward, Vector3.up);

        if (!placed)
        {
            // First valid frame: snap into position
            menuCanvas.transform.position = targetPos;
            menuCanvas.transform.rotation = targetRot;
            placed = true;
        }
        else
        {
            // Gentle follow so the menu stays reachable but stable enough to interact with
            float dt = Time.unscaledDeltaTime;
            menuCanvas.transform.position = Vector3.Lerp(menuCanvas.transform.position, targetPos, followSpeed * dt);
            menuCanvas.transform.rotation = Quaternion.Slerp(menuCanvas.transform.rotation, targetRot, followSpeed * dt);
        }
    }

    // ───────────────────── Actions ─────────────────────

    void PlayGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    void ShowSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    void HideSettings()
    {
        settingsPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    void SetVolume(float value)
    {
        AudioListener.volume = value;
    }

    void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ───────────────────── UI Building ─────────────────────

    void BuildUI()
    {
        // ── World-space canvas ──
        GameObject canvasObj = new GameObject("MenuCanvas");
        canvasObj.layer = 5; // UI layer
        canvasObj.transform.SetParent(transform);

        menuCanvas = canvasObj.AddComponent<Canvas>();
        menuCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform canvasRect = menuCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(400, 600);
        canvasRect.localScale = Vector3.one * 0.002f;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<TrackedDeviceGraphicRaycaster>();

        // ── Main panel ──
        mainPanel = CreatePanel(canvasObj.transform, "MainPanel", canvasRect.sizeDelta);

        float y = 230f;

        CreateLabel(mainPanel.transform, "TitleText", "Bonza\u00ef Therapy", 44, y);
        y -= 120f;

        CreateButton(mainPanel.transform, "PlayButton", "Play", y, PlayGame);
        y -= 80f;

        CreateButton(mainPanel.transform, "SettingsButton", "Settings", y, ShowSettings);
        y -= 80f;

        CreateButton(mainPanel.transform, "QuitButton", "Quit", y, QuitGame);

        // ── Settings panel ──
        settingsPanel = CreatePanel(canvasObj.transform, "SettingsPanel", canvasRect.sizeDelta);

        CreateLabel(settingsPanel.transform, "SettingsTitle", "Settings", 44, 230f);
        CreateLabel(settingsPanel.transform, "VolumeLabel", "Volume", 28, 80f);
        CreateVolumeSlider(settingsPanel.transform, 20f);
        CreateButton(settingsPanel.transform, "BackButton", "Back", -150f, HideSettings);

        settingsPanel.SetActive(false);
    }

    // ───────────────────── Helpers ─────────────────────

    static void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

    GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.layer = 5;
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = panelColor;
        img.raycastTarget = true;

        return panel;
    }

    void CreateLabel(Transform parent, string name, string content, int fontSize, float yPos)
    {
        GameObject textObj = new GameObject(name);
        textObj.layer = 5;
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
        text.raycastTarget = false; // Don't block button clicks
    }

    void CreateButton(Transform parent, string name, string label, float yPos, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.layer = 5;
        btnObj.transform.SetParent(parent, false);

        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(280, 60);
        rect.anchoredPosition = new Vector2(0, yPos);

        Image img = btnObj.AddComponent<Image>();
        img.color = btnColor;
        img.raycastTarget = true;

        Button btn = btnObj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(action);

        // Label (child)
        GameObject labelObj = new GameObject("Label");
        labelObj.layer = 5;
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
        text.raycastTarget = false; // Don't block the button Image underneath
    }

    void CreateVolumeSlider(Transform parent, float yPos)
    {
        GameObject sliderObj = new GameObject("VolumeSlider");
        sliderObj.layer = 5;
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
        bg.layer = 5;
        bg.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bg.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.layer = 5;
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = new Vector2(-20, 0);

        GameObject fill = new GameObject("Fill");
        fill.layer = 5;
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fill.AddComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.4f, 0.8f, 0.4f, 1f);

        // Handle area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.layer = 5;
        handleArea.transform.SetParent(sliderObj.transform, false);
        RectTransform handleAreaRect = handleArea.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.sizeDelta = new Vector2(-20, 0);

        GameObject handle = new GameObject("Handle");
        handle.layer = 5;
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
}
