using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Replaces the scene Canvas (Screen Space Overlay) with a fresh
/// World Space canvas so the menu is visible inside the VR headset.
/// The canvas smoothly follows the player's gaze so it always stays in front.
/// Attach this component to the Canvas GameObject in MenuScene 1.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class VRMenuSetup : MonoBehaviour
{
    [Tooltip("Distance in meters the menu canvas is placed in front of the camera.")]
    public float canvasDistance = 1.5f;

    [Tooltip("Scale of the canvas in world space.")]
    public float canvasScale = 0.002f;

    [Tooltip("Height offset from the camera (in meters).")]
    public float heightOffset = 0f;

    [Tooltip("Smoothing speed for the canvas follow movement.")]
    public float followSpeed = 8f;

    Transform canvasTransform;
    bool initialized;

    void Start()
    {
        if (!initialized)
            StartCoroutine(SetupWorldCanvas());
    }

    void Update()
    {
        if (canvasTransform != null)
            FollowPlayer();
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

        Vector3 targetPos = cam.transform.position + forward * canvasDistance;
        targetPos.y = cam.transform.position.y + heightOffset;

        Quaternion targetRot = Quaternion.LookRotation(forward, Vector3.up);

        float dt = Time.unscaledDeltaTime;
        canvasTransform.position = Vector3.Lerp(canvasTransform.position, targetPos, followSpeed * dt);
        canvasTransform.rotation = Quaternion.Slerp(canvasTransform.rotation, targetRot, followSpeed * dt);
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

        // --- Move this script to the new canvas so Update() keeps running ---
        VRMenuSetup setup = newCanvasObj.AddComponent<VRMenuSetup>();
        setup.canvasDistance = canvasDistance;
        setup.canvasScale = canvasScale;
        setup.heightOffset = heightOffset;
        setup.followSpeed = followSpeed;
        setup.canvasTransform = newCanvasObj.transform;
        setup.initialized = true;

        // --- Destroy the old (Screen Space Overlay) canvas ---
        Destroy(gameObject);
    }
}
