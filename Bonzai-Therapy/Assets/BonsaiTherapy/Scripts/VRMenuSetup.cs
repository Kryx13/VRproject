using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Replaces the scene Canvas (Screen Space Overlay) with a fresh
/// World Space canvas so the menu is visible inside the VR headset.
/// Attach this component to the Canvas GameObject in MenuScene 1.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class VRMenuSetup : MonoBehaviour
{
    [Tooltip("Distance in meters the menu canvas is placed in front of the camera.")]
    public float canvasDistance = 3f;

    [Tooltip("Scale of the canvas in world space.")]
    public float canvasScale = 0.002f;

    [Tooltip("Height offset from the camera (in meters).")]
    public float heightOffset = 0f;

    void Start()
    {
        StartCoroutine(SetupWorldCanvas());
    }

    IEnumerator SetupWorldCanvas()
    {
        // Wait until the XR camera is ready
        Camera cam = Camera.main;
        while (cam == null)
        {
            yield return null;
            cam = Camera.main;
        }

        // --- Create a brand-new World Space canvas (same approach as PauseManager) ---
        GameObject newCanvasObj = new GameObject("VRMenuCanvas");
        Canvas newCanvas = newCanvasObj.AddComponent<Canvas>();
        newCanvas.renderMode = RenderMode.WorldSpace;

        RectTransform newRect = newCanvas.GetComponent<RectTransform>();
        newRect.sizeDelta = new Vector2(1920, 1080);
        newCanvasObj.transform.localScale = Vector3.one * canvasScale;

        newCanvasObj.AddComponent<GraphicRaycaster>();

        // --- Re-parent every child from the old canvas into the new one ---
        // Collect children first to avoid modifying the list while iterating.
        Transform oldTransform = transform;
        int childCount = oldTransform.childCount;
        Transform[] children = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
            children[i] = oldTransform.GetChild(i);

        foreach (Transform child in children)
            child.SetParent(newRect, false);

        // --- Position the new canvas in front of the player ---
        Vector3 forward = cam.transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = cam.transform.forward;
        forward.Normalize();

        Vector3 position = cam.transform.position + forward * canvasDistance;
        position.y = cam.transform.position.y + heightOffset;

        newCanvasObj.transform.position = position;
        newCanvasObj.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        // --- Rewire MenuManager references to the new canvas ---
        MenuManager menu = Object.FindFirstObjectByType<MenuManager>();
        if (menu != null)
        {
            // The buttons / panels are already re-parented;
            // MenuManager still holds its serialized references so nothing extra is needed.
        }

        // --- Destroy the old (Screen Space Overlay) canvas ---
        Destroy(gameObject);
    }
}
