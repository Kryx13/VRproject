using UnityEngine;

/// <summary>
/// Master controller that creates a complete bonsai setup on the table.
/// This is the main script to use - attach it to an empty GameObject.
/// It will automatically create the pot, tree, and position everything on the table.
/// </summary>
[RequireComponent(typeof(BonsaiGenerator))]
[RequireComponent(typeof(BonsaiPot))]
public class BonsaiMaster : MonoBehaviour
{
    [Header("Table Reference")]
    [Tooltip("Reference to the table. If empty, will search for 'Table' GameObject.")]
    public Transform table;

    [Header("Bonsai Style Presets")]
    public BonsaiStyle style = BonsaiStyle.FormalUpright;

    [Header("Quick Settings")]
    [Range(0.5f, 2f)]
    public float overallScale = 1f;

    [Header("Scissors")]
    public bool generateScissors = true;

    [Header("Auto Generation")]
    public bool generateOnStart = true;

    private BonsaiGenerator generator;
    private BonsaiPot pot;
    private ScissorsGenerator scissors;

    public enum BonsaiStyle
    {
        FormalUpright,      // Chokkan - straight trunk
        InformalUpright,    // Moyogi - curved trunk
        Slanting,           // Shakan - angled trunk
        Cascade,            // Kengai - branches cascade down
        Windswept           // Fukinagashi - shaped by wind
    }

    void Awake()
    {
        InitializeComponents();
    }

    void Start()
    {
        if (generateOnStart)
        {
            CreateCompleteBonsai();
        }
    }

    private void InitializeComponents()
    {
        if (generator == null)
            generator = GetComponent<BonsaiGenerator>();
        if (pot == null)
            pot = GetComponent<BonsaiPot>();
        if (scissors == null)
        {
            scissors = GetComponent<ScissorsGenerator>();
            if (scissors == null && generateScissors)
                scissors = gameObject.AddComponent<ScissorsGenerator>();
        }
    }

    /// <summary>
    /// Creates the complete bonsai with pot on the table.
    /// </summary>
    [ContextMenu("Create Complete Bonsai")]
    public void CreateCompleteBonsai()
    {
        InitializeComponents();
        // Find table if not assigned
        if (table == null)
        {
            GameObject tableObj = GameObject.Find("Table");
            if (tableObj != null)
                table = tableObj.transform;
        }

        // Position this object on the table
        PositionOnTable();

        // Apply style preset
        ApplyStylePreset();

        // Apply scale
        ApplyScale();

        // Share table reference
        generator.tableTransform = null;
        generator.generateOnStart = false;
        pot.generateOnStart = false;

        if (scissors != null)
        {
            scissors.generateOnStart = false;
            scissors.table = table;
        }

        // Generate pot, tree, and scissors
        pot.GeneratePot();
        generator.GenerateBonsai();

        if (generateScissors && scissors != null)
            scissors.GenerateScissors();

        AdjustTreeInPot();

        Debug.Log($"Complete {style} bonsai created with {generator.GetLeafCount()} leaves.");
    }

    /// <summary>
    /// Clears the entire bonsai setup.
    /// </summary>
    [ContextMenu("Clear Complete Bonsai")]
    public void ClearCompleteBonsai()
    {
        InitializeComponents();
        generator.ClearBonsai();
        pot.ClearPot();
        if (scissors != null)
            scissors.ClearScissors();
    }

    private void PositionOnTable()
    {
        if (table != null)
        {
            Renderer tableRenderer = table.GetComponent<Renderer>();
            float tableTopY = table.position.y;

            if (tableRenderer != null)
            {
                tableTopY = tableRenderer.bounds.max.y;
            }
            else
            {
                tableTopY = table.position.y + (table.localScale.y / 2f);
            }

            transform.position = new Vector3(
                table.position.x,
                tableTopY,
                table.position.z
            );
        }
    }

    private void ApplyStylePreset()
    {
        switch (style)
        {
            case BonsaiStyle.FormalUpright:
                ApplyFormalUpright();
                break;
            case BonsaiStyle.InformalUpright:
                ApplyInformalUpright();
                break;
            case BonsaiStyle.Slanting:
                ApplySlanting();
                break;
            case BonsaiStyle.Cascade:
                ApplyCascade();
                break;
            case BonsaiStyle.Windswept:
                ApplyWindswept();
                break;
        }
    }

    private void ApplyFormalUpright()
    {
        generator.trunkCurvature = 0.002f;
        generator.trunkSegments = 5;
        generator.mainBranches = 6;
        generator.branchDepth = 3;
        generator.branchAngleMin = 35f;
        generator.branchAngleMax = 55f;
        generator.foliageDensity = 0.95f;
        generator.leafColorBase = new Color(0.2f, 0.5f, 0.2f);
        generator.wildness = 0.6f;
        generator.suckerBranches = 4;
        generator.foliageOvergrowth = 2f;
    }

    private void ApplyInformalUpright()
    {
        generator.trunkCurvature = 0.015f;
        generator.trunkSegments = 6;
        generator.mainBranches = 7;
        generator.branchDepth = 3;
        generator.branchAngleMin = 30f;
        generator.branchAngleMax = 60f;
        generator.foliageDensity = 0.9f;
        generator.leafColorBase = new Color(0.25f, 0.55f, 0.2f);
        generator.wildness = 0.75f;
        generator.suckerBranches = 6;
        generator.foliageOvergrowth = 2.5f;
    }

    private void ApplySlanting()
    {
        generator.trunkCurvature = 0.012f;
        generator.trunkSegments = 5;
        generator.mainBranches = 5;
        generator.branchDepth = 3;
        generator.branchAngleMin = 25f;
        generator.branchAngleMax = 50f;
        generator.foliageDensity = 0.85f;
        generator.leafColorBase = new Color(0.18f, 0.45f, 0.18f);
        generator.wildness = 0.7f;
        generator.suckerBranches = 5;
        generator.foliageOvergrowth = 2f;

        // Tilt the whole setup
        transform.rotation = Quaternion.Euler(0, 0, 15f);
    }

    private void ApplyCascade()
    {
        generator.trunkCurvature = 0.02f;
        generator.trunkSegments = 7;
        generator.trunkHeight = 0.15f;
        generator.mainBranches = 5;
        generator.branchDepth = 4;
        generator.branchAngleMin = 50f;
        generator.branchAngleMax = 80f;
        generator.branchLengthRatio = 0.7f;
        generator.foliageDensity = 0.95f;
        generator.leafColorBase = new Color(0.15f, 0.4f, 0.2f);
        generator.wildness = 0.8f;
        generator.suckerBranches = 7;
        generator.foliageOvergrowth = 2.5f;

        // Taller pot for cascade style
        pot.potHeight = 0.1f;
    }

    private void ApplyWindswept()
    {
        generator.trunkCurvature = 0.018f;
        generator.trunkSegments = 6;
        generator.mainBranches = 5;
        generator.branchDepth = 3;
        generator.branchAngleMin = 20f;
        generator.branchAngleMax = 45f;
        generator.foliageDensity = 0.85f;
        generator.leafColorBase = new Color(0.22f, 0.48f, 0.18f);
        generator.wildness = 0.65f;
        generator.suckerBranches = 4;
        generator.foliageOvergrowth = 1.8f;

        // Slight tilt to suggest wind direction
        transform.rotation = Quaternion.Euler(0, 0, -8f);
    }

    private void ApplyScale()
    {
        generator.trunkHeight *= overallScale;
        generator.trunkBaseRadius *= overallScale;
        generator.leafSize *= overallScale;
        generator.clusterRadius *= overallScale;

        pot.potDiameter *= overallScale;
        pot.potHeight *= overallScale;
    }

    private void AdjustTreeInPot()
    {
        // Find the Bonsai object and adjust its position
        Transform bonsai = transform.Find("Bonsai");
        if (bonsai != null)
        {
            // Position tree so trunk starts at soil level
            bonsai.localPosition = new Vector3(0, pot.potHeight, 0);
        }
    }

    /// <summary>
    /// Randomizes the seed and regenerates the bonsai.
    /// </summary>
    [ContextMenu("Randomize Bonsai")]
    public void RandomizeBonsai()
    {
        generator.randomSeed = Random.Range(1, 99999);
        CreateCompleteBonsai();
    }
}
