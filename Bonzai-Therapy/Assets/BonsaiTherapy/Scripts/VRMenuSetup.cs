using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

/// <summary>
/// Converts the menu Canvas from Screen Space Overlay to World Space
/// so it is visible and interactable in VR via XR controller rays.
/// Attach this component to the Canvas GameObject in MenuScene 1.
/// Also drag the XR Origin (XR Rig) prefab into the scene.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class VRMenuSetup : MonoBehaviour
{
    [Tooltip("Distance in meters the menu canvas is placed in front of the camera.")]
    public float canvasDistance = 3f;

    [Tooltip("Scale of the canvas in world space. Adjust to taste.")]
    public float canvasScale = 0.005f;

    [Tooltip("Height offset from the camera (in meters).")]
    public float heightOffset = 0f;

    void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();

        // Switch from Screen Space Overlay to World Space
        canvas.renderMode = RenderMode.WorldSpace;

        // Set a reasonable size for the world-space canvas
        RectTransform rt = canvas.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1920, 1080);
        transform.localScale = Vector3.one * canvasScale;

        // Replace GraphicRaycaster with TrackedDeviceGraphicRaycaster
        // so VR controller rays can interact with UI elements.
        GraphicRaycaster gr = GetComponent<GraphicRaycaster>();
        if (gr != null)
            Destroy(gr);

        if (GetComponent<TrackedDeviceGraphicRaycaster>() == null)
            gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Remove CanvasScaler — it is not needed in World Space
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
            Destroy(scaler);
    }

    void Start()
    {
        PositionCanvasInFrontOfCamera();
    }

    void PositionCanvasInFrontOfCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
            return;

        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 position = cam.transform.position + forward * canvasDistance;
        position.y = cam.transform.position.y + heightOffset;

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(forward);
    }
}
