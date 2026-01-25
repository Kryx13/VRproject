using UnityEngine;

/// <summary>
/// Generates a traditional bonsai pot with soil.
/// Attach to the same GameObject as BonsaiGenerator for automatic integration.
/// </summary>
public class BonsaiPot : MonoBehaviour
{
    [Header("Pot Dimensions")]
    [Range(0.1f, 0.3f)]
    public float potDiameter = 0.2f;
    [Range(0.03f, 0.1f)]
    public float potHeight = 0.06f;
    [Range(0.005f, 0.02f)]
    public float potWallThickness = 0.01f;
    [Range(0.01f, 0.03f)]
    public float potRimHeight = 0.015f;

    [Header("Appearance")]
    public Color potColor = new Color(0.4f, 0.25f, 0.15f); // Terracotta
    public Color soilColor = new Color(0.2f, 0.15f, 0.1f); // Dark brown soil
    public Color mossColor = new Color(0.3f, 0.45f, 0.25f); // Green moss

    [Header("Decorations")]
    public bool addMoss = true;
    [Range(0f, 1f)]
    public float mossCoverage = 0.6f;
    public bool addDecorativeRocks = true;
    [Range(2, 8)]
    public int rockCount = 4;

    [Header("Generation")]
    public bool generateOnStart = true;

    private GameObject potRoot;
    private Material potMaterial;
    private Material soilMaterial;
    private Material mossMaterial;
    private Material rockMaterial;

    void Start()
    {
        if (generateOnStart)
        {
            GeneratePot();
        }
    }

    [ContextMenu("Generate Pot")]
    public void GeneratePot()
    {
        ClearPot();
        CreateMaterials();

        potRoot = new GameObject("BonsaiPot");
        potRoot.transform.SetParent(transform);
        potRoot.transform.localPosition = Vector3.zero;

        CreatePotBase();
        CreateSoil();

        if (addMoss)
            CreateMoss();

        if (addDecorativeRocks)
            CreateRocks();
    }

    [ContextMenu("Clear Pot")]
    public void ClearPot()
    {
        if (potRoot != null)
        {
            if (Application.isPlaying)
                Destroy(potRoot);
            else
                DestroyImmediate(potRoot);
        }
    }

    private void CreateMaterials()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");

        potMaterial = new Material(litShader);
        potMaterial.color = potColor;
        potMaterial.SetFloat("_Smoothness", 0.4f);

        soilMaterial = new Material(litShader);
        soilMaterial.color = soilColor;
        soilMaterial.SetFloat("_Smoothness", 0.0f);

        mossMaterial = new Material(litShader);
        mossMaterial.color = mossColor;
        mossMaterial.SetFloat("_Smoothness", 0.2f);

        rockMaterial = new Material(litShader);
        rockMaterial.color = new Color(0.5f, 0.5f, 0.5f);
        rockMaterial.SetFloat("_Smoothness", 0.3f);
    }

    private void CreatePotBase()
    {
        // Main pot body (cylinder)
        GameObject potBody = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        potBody.name = "PotBody";
        potBody.transform.SetParent(potRoot.transform);
        potBody.transform.localPosition = new Vector3(0, potHeight / 2f, 0);
        potBody.transform.localScale = new Vector3(potDiameter, potHeight / 2f, potDiameter);
        potBody.GetComponent<Renderer>().material = potMaterial;
        potBody.isStatic = true;

        // Pot rim (slightly wider cylinder at top)
        GameObject potRim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        potRim.name = "PotRim";
        potRim.transform.SetParent(potRoot.transform);
        potRim.transform.localPosition = new Vector3(0, potHeight - potRimHeight / 2f, 0);
        potRim.transform.localScale = new Vector3(
            potDiameter + potWallThickness * 2,
            potRimHeight / 2f,
            potDiameter + potWallThickness * 2
        );
        potRim.GetComponent<Renderer>().material = potMaterial;
        potRim.isStatic = true;

        // Pot feet (4 small cylinders)
        float footRadius = potDiameter * 0.08f;
        float footHeight = 0.01f;
        float footOffset = potDiameter * 0.35f;

        for (int i = 0; i < 4; i++)
        {
            float angle = i * 90f * Mathf.Deg2Rad;
            Vector3 footPos = new Vector3(
                Mathf.Cos(angle) * footOffset,
                footHeight / 2f,
                Mathf.Sin(angle) * footOffset
            );

            GameObject foot = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            foot.name = $"PotFoot_{i}";
            foot.transform.SetParent(potRoot.transform);
            foot.transform.localPosition = footPos;
            foot.transform.localScale = new Vector3(footRadius * 2, footHeight / 2f, footRadius * 2);
            foot.GetComponent<Renderer>().material = potMaterial;
            foot.isStatic = true;
        }
    }

    private void CreateSoil()
    {
        // Soil surface (flattened cylinder)
        GameObject soil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        soil.name = "Soil";
        soil.transform.SetParent(potRoot.transform);
        soil.transform.localPosition = new Vector3(0, potHeight - 0.005f, 0);
        soil.transform.localScale = new Vector3(
            potDiameter - potWallThickness * 4,
            0.005f,
            potDiameter - potWallThickness * 4
        );
        soil.GetComponent<Renderer>().material = soilMaterial;
        soil.isStatic = true;
    }

    private void CreateMoss()
    {
        float soilRadius = (potDiameter - potWallThickness * 4) / 2f;
        int mossPatches = Mathf.RoundToInt(20 * mossCoverage);

        for (int i = 0; i < mossPatches; i++)
        {
            // Random position within soil area, avoiding center (where tree is)
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(soilRadius * 0.2f, soilRadius * 0.9f);

            Vector3 mossPos = new Vector3(
                Mathf.Cos(angle) * distance,
                potHeight,
                Mathf.Sin(angle) * distance
            );

            // Create small flattened sphere for moss patch
            GameObject moss = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            moss.name = $"Moss_{i}";
            moss.transform.SetParent(potRoot.transform);
            moss.transform.localPosition = mossPos;

            float patchSize = Random.Range(0.01f, 0.025f);
            moss.transform.localScale = new Vector3(patchSize, 0.005f, patchSize);

            // Color variation
            Material mossMat = new Material(mossMaterial);
            float colorVar = Random.Range(-0.1f, 0.1f);
            mossMat.color = mossColor + new Color(colorVar, colorVar * 0.5f, colorVar);
            moss.GetComponent<Renderer>().material = mossMat;

            // Remove collider for performance
            DestroyImmediate(moss.GetComponent<Collider>());
            moss.isStatic = true;
        }
    }

    private void CreateRocks()
    {
        float soilRadius = (potDiameter - potWallThickness * 4) / 2f;

        for (int i = 0; i < rockCount; i++)
        {
            float angle = (i * (360f / rockCount) + Random.Range(-20f, 20f)) * Mathf.Deg2Rad;
            float distance = Random.Range(soilRadius * 0.3f, soilRadius * 0.85f);

            Vector3 rockPos = new Vector3(
                Mathf.Cos(angle) * distance,
                potHeight + 0.003f,
                Mathf.Sin(angle) * distance
            );

            GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = $"Rock_{i}";
            rock.transform.SetParent(potRoot.transform);
            rock.transform.localPosition = rockPos;

            // Random deformed shape
            float rockSize = Random.Range(0.008f, 0.015f);
            rock.transform.localScale = new Vector3(
                rockSize * Random.Range(0.8f, 1.2f),
                rockSize * Random.Range(0.5f, 0.8f),
                rockSize * Random.Range(0.8f, 1.2f)
            );
            rock.transform.localRotation = Random.rotation;

            // Color variation for rocks
            Material rockMat = new Material(rockMaterial);
            float grayVar = Random.Range(-0.15f, 0.15f);
            rockMat.color = new Color(0.5f + grayVar, 0.5f + grayVar, 0.5f + grayVar);
            rock.GetComponent<Renderer>().material = rockMat;

            DestroyImmediate(rock.GetComponent<Collider>());
            rock.isStatic = true;
        }
    }
}
