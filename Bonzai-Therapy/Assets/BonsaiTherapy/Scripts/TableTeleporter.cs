using UnityEngine;
using Unity.XR.CoreUtils;

/// <summary>
/// Fixes seated height for table-level VR play.
/// Joystick locomotion is left enabled for free movement.
/// </summary>
public class TableTeleporter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The XR Origin. If empty, will be found automatically.")]
    public XROrigin xrOrigin;

    [Header("Seated Height")]
    [Tooltip("Height offset for seated play (meters above floor). Adjust so you sit at table level.")]
    public float seatedHeightOffset = 0.7f;

    void Start()
    {
        if (xrOrigin == null)
            xrOrigin = FindAnyObjectByType<XROrigin>();

        ApplySeatedHeight();
    }

    void ApplySeatedHeight()
    {
        if (xrOrigin == null) return;

        xrOrigin.CameraYOffset = seatedHeightOffset;
        Debug.Log($"Seated height offset set to {seatedHeightOffset}m");
    }
}
