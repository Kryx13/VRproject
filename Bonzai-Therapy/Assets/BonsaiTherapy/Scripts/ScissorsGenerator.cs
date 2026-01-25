using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ScissorsGenerator : MonoBehaviour
{
    [Header("Placement")]
    public Transform table;
    public Vector3 offsetFromTable = new Vector3(0.4f, 0.05f, 0f);

    [Header("Scissors Size")]
    public float handleLength = 0.08f;
    public float bladeLength = 0.05f;
    public float thickness = 0.008f;

    [Header("Colors")]
    public Color handleColor = new Color(0.2f, 0.2f, 0.2f);
    public Color bladeColor = new Color(0.7f, 0.7f, 0.75f);

    [Header("Generation")]
    public bool generateOnStart = true;

    private GameObject scissors;
    private Material handleMaterial;
    private Material bladeMaterial;

    void Start()
    {
        if (generateOnStart)
            GenerateScissors();
    }

    [ContextMenu("Generate Scissors")]
    public void GenerateScissors()
    {
        ClearScissors();
        CreateMaterials();
        FindTable();

        scissors = new GameObject("Scissors");
        scissors.transform.SetParent(transform);

        PositionScissors();
        BuildScissorsModel();
        SetupInteraction();
    }

    [ContextMenu("Clear Scissors")]
    public void ClearScissors()
    {
        if (scissors != null)
        {
            if (Application.isPlaying)
                Destroy(scissors);
            else
                DestroyImmediate(scissors);
        }
    }

    private void CreateMaterials()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        handleMaterial = new Material(shader);
        handleMaterial.color = handleColor;

        bladeMaterial = new Material(shader);
        bladeMaterial.color = bladeColor;
        bladeMaterial.SetFloat("_Smoothness", 0.8f);
    }

    private void FindTable()
    {
        if (table == null)
        {
            GameObject tableObj = GameObject.Find("Table");
            if (tableObj != null)
                table = tableObj.transform;
        }
    }

    private void PositionScissors()
    {
        if (table != null)
        {
            Renderer tableRenderer = table.GetComponent<Renderer>();
            float tableTopY = table.position.y;

            if (tableRenderer != null)
                tableTopY = tableRenderer.bounds.max.y;
            else
                tableTopY = table.position.y + (table.localScale.y / 2f);

            scissors.transform.position = new Vector3(
                table.position.x + offsetFromTable.x,
                tableTopY + offsetFromTable.y,
                table.position.z + offsetFromTable.z
            );
        }

        scissors.transform.rotation = Quaternion.Euler(0, 0, 90f);
    }

    private void BuildScissorsModel()
    {
        // Handle 1
        GameObject handle1 = CreateCube("Handle1", handleLength, thickness * 1.5f, thickness);
        handle1.transform.SetParent(scissors.transform);
        handle1.transform.localPosition = new Vector3(-handleLength / 2f, thickness, 0);
        handle1.GetComponent<Renderer>().material = handleMaterial;

        // Handle 2
        GameObject handle2 = CreateCube("Handle2", handleLength, thickness * 1.5f, thickness);
        handle2.transform.SetParent(scissors.transform);
        handle2.transform.localPosition = new Vector3(-handleLength / 2f, -thickness, 0);
        handle2.GetComponent<Renderer>().material = handleMaterial;

        // Pivot point
        GameObject pivot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pivot.name = "Pivot";
        pivot.transform.SetParent(scissors.transform);
        pivot.transform.localPosition = Vector3.zero;
        pivot.transform.localRotation = Quaternion.Euler(90, 0, 0);
        pivot.transform.localScale = new Vector3(thickness * 2f, thickness, thickness * 2f);
        pivot.GetComponent<Renderer>().material = handleMaterial;

        // Blade 1
        GameObject blade1 = CreateCube("Blade1", bladeLength, thickness * 0.5f, thickness * 0.8f);
        blade1.transform.SetParent(scissors.transform);
        blade1.transform.localPosition = new Vector3(bladeLength / 2f, thickness * 0.3f, 0);
        blade1.GetComponent<Renderer>().material = bladeMaterial;

        // Blade 2
        GameObject blade2 = CreateCube("Blade2", bladeLength, thickness * 0.5f, thickness * 0.8f);
        blade2.transform.SetParent(scissors.transform);
        blade2.transform.localPosition = new Vector3(bladeLength / 2f, -thickness * 0.3f, 0);
        blade2.GetComponent<Renderer>().material = bladeMaterial;

        // Cutting trigger zone at blade tips
        GameObject triggerZone = new GameObject("CuttingZone");
        triggerZone.transform.SetParent(scissors.transform);
        triggerZone.transform.localPosition = new Vector3(bladeLength, 0, 0);

        BoxCollider trigger = triggerZone.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(thickness * 3f, thickness * 4f, thickness * 3f);
    }

    private void SetupInteraction()
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
        PruningTool pruning = scissors.AddComponent<PruningTool>();
    }

    private GameObject CreateCube(string name, float x, float y, float z)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.localScale = new Vector3(x, y, z);
        return cube;
    }
}
