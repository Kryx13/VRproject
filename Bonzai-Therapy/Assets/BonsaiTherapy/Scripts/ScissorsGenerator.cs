using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ScissorsGenerator : MonoBehaviour
{
    [Header("Placement")]
    public Transform table;
    public Vector3 offsetFromTable = new Vector3(0.4f, 0.05f, 0f);

    [Header("Small Scissors (Precision)")]
    public float smallBladeLength = 0.05f;
    public float smallHandleLength = 0.06f;
    public float smallThickness = 0.006f;
    public Color smallHandleColor = new Color(0.15f, 0.15f, 0.15f);
    public Color smallBladeColor = new Color(0.75f, 0.75f, 0.8f);

    [Header("Large Scissors (Fast Cut)")]
    public float largeBladeLength = 0.20f;
    public float largeHandleLength = 0.12f;
    public float largeThickness = 0.012f;
    public Color largeHandleColor = new Color(0.25f, 0.2f, 0.2f);
    public Color largeBladeColor = new Color(0.65f, 0.65f, 0.7f);

    [Header("Spacing")]
    public float spacingBetween = 0.25f;

    [Header("Generation")]
    public bool generateOnStart = true;

    private GameObject smallScissors;
    private GameObject largeScissors;

    void Start()
    {
        if (generateOnStart)
            GenerateScissors();
    }

    [ContextMenu("Generate Scissors")]
    public void GenerateScissors()
    {
        ClearScissors();

        Vector3 baseOffset = offsetFromTable;

        // Small scissors on the left
        Vector3 smallOffset = baseOffset + new Vector3(0, 0, -spacingBetween / 2f);
        smallScissors = CreateOneScissors("SmallScissors", smallHandleLength, smallBladeLength, smallThickness, smallHandleColor, smallBladeColor, smallOffset);

        // Large scissors on the right
        Vector3 largeOffset = baseOffset + new Vector3(0, 0, spacingBetween / 2f);
        largeScissors = CreateOneScissors("LargeScissors", largeHandleLength, largeBladeLength, largeThickness, largeHandleColor, largeBladeColor, largeOffset);
    }

    [ContextMenu("Clear Scissors")]
    public void ClearScissors()
    {
        DestroyObject(ref smallScissors);
        DestroyObject(ref largeScissors);
    }

    private void DestroyObject(ref GameObject obj)
    {
        if (obj != null)
        {
            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
            obj = null;
        }
    }

    private GameObject CreateOneScissors(string name, float handleLength, float bladeLength, float thickness, Color handleCol, Color bladeCol, Vector3 offset)
    {
        // Materials
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material handleMat = new Material(shader);
        handleMat.color = handleCol;

        Material bladeMat = new Material(shader);
        bladeMat.color = bladeCol;
        bladeMat.SetFloat("_Smoothness", 0.8f);

        // Root object
        GameObject scissors = new GameObject(name);
        scissors.transform.SetParent(transform);

        // Position on table
        PositionScissors(scissors, offset);

        // Build model
        BuildScissorsModel(scissors, handleLength, bladeLength, thickness, handleMat, bladeMat);

        // Setup XR interaction
        SetupInteraction(scissors, handleLength, bladeLength, thickness);

        return scissors;
    }

    private void PositionScissors(GameObject scissors, Vector3 offset)
    {
        if (table == null)
        {
            GameObject tableObj = GameObject.Find("Table");
            if (tableObj != null)
                table = tableObj.transform;
        }

        if (table != null)
        {
            Renderer tableRenderer = table.GetComponent<Renderer>();
            float tableTopY = table.position.y;

            if (tableRenderer != null)
                tableTopY = tableRenderer.bounds.max.y;
            else
                tableTopY = table.position.y + (table.localScale.y / 2f);

            scissors.transform.position = new Vector3(
                table.position.x + offset.x,
                tableTopY + offset.y,
                table.position.z + offset.z
            );
        }

        scissors.transform.rotation = Quaternion.Euler(0, 0, 90f);
    }

    private void BuildScissorsModel(GameObject scissors, float handleLength, float bladeLength, float thickness, Material handleMat, Material bladeMat)
    {
        // Handle 1
        GameObject handle1 = CreateCube("Handle1", handleLength, thickness * 1.5f, thickness);
        handle1.transform.SetParent(scissors.transform);
        handle1.transform.localPosition = new Vector3(-handleLength / 2f, thickness, 0);
        handle1.GetComponent<Renderer>().material = handleMat;
        Object.Destroy(handle1.GetComponent<Collider>());

        // Handle 2
        GameObject handle2 = CreateCube("Handle2", handleLength, thickness * 1.5f, thickness);
        handle2.transform.SetParent(scissors.transform);
        handle2.transform.localPosition = new Vector3(-handleLength / 2f, -thickness, 0);
        handle2.GetComponent<Renderer>().material = handleMat;
        Object.Destroy(handle2.GetComponent<Collider>());

        // Pivot point
        GameObject pivot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pivot.name = "Pivot";
        pivot.transform.SetParent(scissors.transform);
        pivot.transform.localPosition = Vector3.zero;
        pivot.transform.localRotation = Quaternion.Euler(90, 0, 0);
        pivot.transform.localScale = new Vector3(thickness * 2f, thickness, thickness * 2f);
        pivot.GetComponent<Renderer>().material = handleMat;
        Object.Destroy(pivot.GetComponent<Collider>());

        // Blade 1
        GameObject blade1 = CreateCube("Blade1", bladeLength, thickness * 0.8f, thickness * 1.2f);
        blade1.transform.SetParent(scissors.transform);
        blade1.transform.localPosition = new Vector3(bladeLength / 2f, thickness * 0.3f, 0);
        blade1.GetComponent<Renderer>().material = bladeMat;
        Object.Destroy(blade1.GetComponent<Collider>());

        // Blade 2
        GameObject blade2 = CreateCube("Blade2", bladeLength, thickness * 0.8f, thickness * 1.2f);
        blade2.transform.SetParent(scissors.transform);
        blade2.transform.localPosition = new Vector3(bladeLength / 2f, -thickness * 0.3f, 0);
        blade2.GetComponent<Renderer>().material = bladeMat;
        Object.Destroy(blade2.GetComponent<Collider>());

        // Cutting trigger zone along the blades
        GameObject triggerZone = new GameObject("CuttingZone");
        triggerZone.transform.SetParent(scissors.transform);
        triggerZone.transform.localPosition = new Vector3(bladeLength * 0.75f, 0, 0);

        BoxCollider trigger = triggerZone.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(bladeLength * 0.8f, thickness * 6f, thickness * 5f);
    }

    private void SetupInteraction(GameObject scissors, float handleLength, float bladeLength, float thickness)
    {
        // Add rigidbody
        Rigidbody rb = scissors.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.mass = 0.1f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Add main collider for grabbing
        BoxCollider grabCollider = scissors.AddComponent<BoxCollider>();
        grabCollider.size = new Vector3(handleLength + bladeLength, thickness * 4f, thickness * 2f);
        grabCollider.center = new Vector3(0, 0, 0);

        // Add XR Grab Interactable
        XRGrabInteractable grab = scissors.AddComponent<XRGrabInteractable>();
        grab.movementType = XRBaseInteractable.MovementType.VelocityTracking;
        grab.throwOnDetach = true;

        // Add PruningTool script
        scissors.AddComponent<PruningTool>();
    }

    private GameObject CreateCube(string name, float x, float y, float z)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.localScale = new Vector3(x, y, z);
        return cube;
    }
}
