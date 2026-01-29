using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using Unity.XR.CoreUtils;

/// <summary>
/// Creates 4 teleport spots around the table. Point at a spot and press the index trigger to teleport.
/// The player always faces the bonsai after teleporting.
/// Also fixes seated height and disables joystick movement.
/// </summary>
public class TableTeleporter : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The XR Origin. If empty, will be found automatically.")]
    public XROrigin xrOrigin;

    [Tooltip("The table Transform. If empty, will search for 'Table'.")]
    public Transform table;

    [Tooltip("The bonsai Transform. If empty, will search for 'BonsaiSetup'.")]
    public Transform bonsai;

    [Header("Seated Height")]
    [Tooltip("Height offset for seated play (meters above floor). Adjust so you sit at table level.")]
    public float seatedHeightOffset = 0.7f;

    [Header("Teleport Spots")]
    [Tooltip("Distance from the center of the table to each teleport spot.")]
    public float distanceFromTable = 0.8f;

    [Tooltip("Height of the teleport spots (Y position). Should match the floor level.")]
    public float spotHeight = 0f;

    [Header("Spot Visuals")]
    [Tooltip("Color of the teleport spot markers.")]
    public Color spotColor = new Color(0.2f, 0.6f, 1f, 0.5f);

    [Tooltip("Size of the teleport spot markers.")]
    public float spotSize = 0.3f;

    [Header("Input")]
    [Tooltip("Input action for the right index trigger.")]
    public InputActionReference triggerAction;

    [Header("Disable Locomotion")]
    [Tooltip("Disable continuous move and turn providers on start.")]
    public bool disableJoystickMovement = true;

    private GameObject[] spotMarkers = new GameObject[4];
    private Vector3[] spotPositions = new Vector3[4];
    private Transform rightController;
    private int currentSpot = 0;

    void Start()
    {
        // Find references
        if (xrOrigin == null)
            xrOrigin = FindAnyObjectByType<XROrigin>();

        if (table == null)
        {
            GameObject tableObj = GameObject.Find("Table");
            if (tableObj != null)
                table = tableObj.transform;
        }

        if (bonsai == null)
        {
            GameObject bonsaiObj = GameObject.Find("BonsaiSetup");
            if (bonsaiObj != null)
                bonsai = bonsaiObj.transform;
        }

        // Find right controller for ray direction
        if (xrOrigin != null)
        {
            // Look for right controller in XR Origin hierarchy
            foreach (var interactor in xrOrigin.GetComponentsInChildren<XRRayInteractor>(true))
            {
                if (interactor.gameObject.name.Contains("Right"))
                {
                    rightController = interactor.transform;
                    break;
                }
            }
            // Fallback: find any ray interactor
            if (rightController == null)
            {
                var rayInteractor = xrOrigin.GetComponentInChildren<XRRayInteractor>();
                if (rayInteractor != null)
                    rightController = rayInteractor.transform;
            }
        }

        // Disable joystick movement
        if (disableJoystickMovement)
            DisableJoystickLocomotion();

        // Fix seated height
        ApplySeatedHeight();

        // Create the 4 teleport spots
        CreateTeleportSpots();

        // Teleport to the first spot initially
        TeleportToSpot(0);

        // Setup input
        if (triggerAction != null && triggerAction.action != null)
        {
            triggerAction.action.Enable();
            triggerAction.action.performed += OnTriggerPressed;
        }
        else
        {
            // Fallback: try to find trigger action from input system
            SetupFallbackInput();
        }
    }

    void OnDestroy()
    {
        if (triggerAction != null && triggerAction.action != null)
        {
            triggerAction.action.performed -= OnTriggerPressed;
        }
    }

    void DisableJoystickLocomotion()
    {
        if (xrOrigin == null) return;

        // Disable all move providers (continuous movement)
        foreach (var provider in xrOrigin.GetComponentsInChildren<LocomotionProvider>(true))
        {
            // Keep teleportation provider alive, disable move/turn/climb/jump
            string typeName = provider.GetType().Name;
            if (typeName.Contains("Move") || typeName.Contains("Turn") ||
                typeName.Contains("Climb") || typeName.Contains("Jump") ||
                typeName.Contains("Snap") || typeName.Contains("Grab"))
            {
                provider.enabled = false;
            }
        }

        Debug.Log("Joystick movement disabled.");
    }

    void ApplySeatedHeight()
    {
        if (xrOrigin == null) return;

        // Adjust the camera Y offset for seated play
        // This raises the camera so you're at table height instead of below it
        xrOrigin.CameraYOffset = seatedHeightOffset;
        Debug.Log($"Seated height offset set to {seatedHeightOffset}m");
    }

    void CreateTeleportSpots()
    {
        if (table == null)
        {
            Debug.LogError("TableTeleporter: No table found! Cannot create teleport spots.");
            return;
        }

        Vector3 tableCenter = table.position;

        // If spotHeight is 0, use the floor (table position minus half its height)
        float floorY = spotHeight;
        if (Mathf.Approximately(floorY, 0f))
        {
            Renderer tableRenderer = table.GetComponent<Renderer>();
            if (tableRenderer != null)
                floorY = tableRenderer.bounds.min.y;
            else
                floorY = tableCenter.y - (table.localScale.y / 2f);
        }

        // 4 spots: front, right, back, left of the table
        Vector3[] directions = new Vector3[]
        {
            Vector3.forward,   // Front (Z+)
            Vector3.right,     // Right (X+)
            Vector3.back,      // Back (Z-)
            Vector3.left       // Left (X-)
        };

        for (int i = 0; i < 4; i++)
        {
            spotPositions[i] = new Vector3(
                tableCenter.x + directions[i].x * distanceFromTable,
                floorY,
                tableCenter.z + directions[i].z * distanceFromTable
            );

            // Create visual marker
            spotMarkers[i] = CreateSpotMarker(spotPositions[i], i);
        }

        Debug.Log("4 teleport spots created around the table.");
    }

    GameObject CreateSpotMarker(Vector3 position, int index)
    {
        // Create a flat cylinder as a spot marker
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = $"TeleportSpot_{index}";
        marker.transform.position = position + Vector3.up * 0.01f; // Slightly above floor
        marker.transform.localScale = new Vector3(spotSize, 0.01f, spotSize);

        // Set material
        Renderer renderer = marker.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = spotColor;
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 0);
        mat.SetFloat("_AlphaClip", 0);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = 3000;
        renderer.material = mat;

        // Remove the default collider and add a trigger collider for raycasting
        Object.Destroy(marker.GetComponent<Collider>());
        SphereCollider col = marker.AddComponent<SphereCollider>();
        col.radius = 1f; // Larger radius for easier targeting
        col.isTrigger = true;

        // Tag for identification
        marker.layer = 0; // Default layer, raycast will hit it

        return marker;
    }

    void OnTriggerPressed(InputAction.CallbackContext context)
    {
        TryTeleportFromRay();
    }

    void SetupFallbackInput()
    {
        Debug.LogWarning("TableTeleporter: No trigger action assigned. Assign 'XRI Right Interaction/Activate' in the Inspector.");
    }

    void TryTeleportFromRay()
    {
        if (rightController == null) return;

        // Raycast from the right controller
        Ray ray = new Ray(rightController.position, rightController.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, 10f);

        float closestDist = float.MaxValue;
        int closestSpot = -1;

        foreach (var hit in hits)
        {
            for (int i = 0; i < 4; i++)
            {
                if (spotMarkers[i] != null && hit.collider.gameObject == spotMarkers[i])
                {
                    if (hit.distance < closestDist)
                    {
                        closestDist = hit.distance;
                        closestSpot = i;
                    }
                }
            }
        }

        if (closestSpot >= 0)
        {
            TeleportToSpot(closestSpot);
        }
    }

    void TeleportToSpot(int spotIndex)
    {
        if (xrOrigin == null || bonsai == null) return;

        currentSpot = spotIndex;
        Vector3 targetPos = spotPositions[spotIndex];

        // Calculate rotation to face the bonsai
        Vector3 lookTarget = bonsai.position;
        lookTarget.y = targetPos.y; // Keep rotation horizontal
        Vector3 lookDir = (lookTarget - targetPos).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(lookDir, Vector3.up);

        // Move the XR Origin
        xrOrigin.transform.position = targetPos;
        xrOrigin.transform.rotation = targetRotation;

        // Highlight current spot
        UpdateSpotHighlights(spotIndex);

        Debug.Log($"Teleported to spot {spotIndex}, facing bonsai.");
    }

    void UpdateSpotHighlights(int activeSpot)
    {
        for (int i = 0; i < 4; i++)
        {
            if (spotMarkers[i] == null) continue;

            Renderer renderer = spotMarkers[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                if (i == activeSpot)
                    renderer.material.color = new Color(0.1f, 1f, 0.3f, 0.6f); // Green for active
                else
                    renderer.material.color = spotColor; // Default color
            }
        }
    }
}
