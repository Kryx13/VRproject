using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates a procedural bonsai tree with realistic foliage.
/// Attach this script to an empty GameObject and call GenerateBonsai() or enable generateOnStart.
/// The bonsai will be placed on the table automatically.
/// </summary>
public class BonsaiGenerator : MonoBehaviour
{
    [Header("Table Reference")]
    [Tooltip("Reference to the table. If null, will search for 'Table' GameObject.")]
    public Transform tableTransform;

    [Header("Trunk Settings")]
    [Range(0.03f, 0.1f)]
    public float trunkBaseRadius = 0.05f;
    [Range(0.1f, 0.4f)]
    public float trunkHeight = 0.2f;
    [Range(3, 8)]
    public int trunkSegments = 5;
    [Range(0f, 0.02f)]
    public float trunkCurvature = 0.01f;
    public Color trunkColor = new Color(0.36f, 0.25f, 0.2f);

    [Header("Branch Settings")]
    [Range(3, 8)]
    public int mainBranches = 5;
    [Range(1, 4)]
    public int branchDepth = 3;
    [Range(0.4f, 0.8f)]
    public float branchLengthRatio = 0.6f;
    [Range(0.5f, 0.8f)]
    public float branchRadiusRatio = 0.65f;
    [Range(20f, 60f)]
    public float branchAngleMin = 30f;
    [Range(40f, 80f)]
    public float branchAngleMax = 60f;

    [Header("Foliage Settings")]
    [Range(5, 30)]
    public int leavesPerCluster = 15;
    [Range(0.01f, 0.04f)]
    public float leafSize = 0.02f;
    [Range(0.02f, 0.12f)]
    public float clusterRadius = 0.05f;
    public Color leafColorBase = new Color(0.2f, 0.5f, 0.2f);
    public Color leafColorVariation = new Color(0.1f, 0.15f, 0.05f);
    [Range(0f, 1f)]
    public float foliageDensity = 0.9f;

    [Header("Wildness Settings")]
    [Range(0f, 1f)]
    [Tooltip("How messy/wild the tree looks. 0 = organized, 1 = very wild")]
    public float wildness = 0.7f;
    [Range(0, 10)]
    [Tooltip("Extra random branches growing in wrong directions")]
    public int suckerBranches = 5;
    [Range(1f, 3f)]
    [Tooltip("Multiplier for extra foliage clusters")]
    public float foliageOvergrowth = 2f;

    [Header("Generation")]
    public bool generateOnStart = true;
    public int randomSeed = 0;

    private GameObject bonsaiRoot;
    private Material trunkMaterial;
    private Material leafMaterial;
    private List<GameObject> allLeaves = new List<GameObject>();

    void Start()
    {
        if (generateOnStart)
        {
            GenerateBonsai();
        }
    }

    [ContextMenu("Generate Bonsai")]
    public void GenerateBonsai()
    {
        ClearBonsai();

        if (randomSeed != 0)
            Random.InitState(randomSeed);
        else
            Random.InitState(System.DateTime.Now.Millisecond);

        CreateMaterials();
        FindTable();

        bonsaiRoot = new GameObject("Bonsai");
        bonsaiRoot.transform.SetParent(transform);
        bonsaiRoot.transform.localPosition = Vector3.zero;
        bonsaiRoot.transform.localRotation = Quaternion.identity;
        bonsaiRoot.transform.localScale = Vector3.one;

        PositionOnTable();
        GenerateTrunk();

        Debug.Log($"Bonsai generated with {allLeaves.Count} leaves.");
    }

    [ContextMenu("Clear Bonsai")]
    public void ClearBonsai()
    {
        if (bonsaiRoot != null)
        {
            if (Application.isPlaying)
                Destroy(bonsaiRoot);
            else
                DestroyImmediate(bonsaiRoot);
        }
        allLeaves.Clear();
    }

    [ContextMenu("Reset All Leaves")]
    public void ResetAllLeaves()
    {
        foreach (var leaf in allLeaves)
        {
            if (leaf != null)
                leaf.SetActive(true);
        }
    }

    private void CreateMaterials()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        trunkMaterial = new Material(shader);
        trunkMaterial.color = trunkColor;

        leafMaterial = new Material(shader);
        leafMaterial.color = leafColorBase;
    }

    private void FindTable()
    {
        if (tableTransform == null)
        {
            GameObject table = GameObject.Find("Table");
            if (table != null)
            {
                tableTransform = table.transform;
            }
        }
    }

    private void PositionOnTable()
    {
        if (tableTransform != null)
        {
            Renderer tableRenderer = tableTransform.GetComponent<Renderer>();
            float tableTopY = tableTransform.position.y;

            if (tableRenderer != null)
            {
                tableTopY = tableRenderer.bounds.max.y;
            }
            else
            {
                tableTopY = tableTransform.position.y + (tableTransform.localScale.y / 2f);
            }

            bonsaiRoot.transform.position = new Vector3(
                tableTransform.position.x,
                tableTopY,
                tableTransform.position.z
            );
        }
    }

    private void GenerateTrunk()
    {
        GameObject trunkParent = new GameObject("Trunk");
        trunkParent.transform.SetParent(bonsaiRoot.transform);
        trunkParent.transform.localPosition = Vector3.zero;
        trunkParent.transform.localRotation = Quaternion.identity;
        trunkParent.transform.localScale = Vector3.one;

        Vector3 currentWorldPos = bonsaiRoot.transform.position;
        Vector3 currentDir = Vector3.up;
        float segmentHeight = trunkHeight / trunkSegments;
        float currentRadius = trunkBaseRadius;

        List<BranchPoint> branchPoints = new List<BranchPoint>();

        for (int i = 0; i < trunkSegments; i++)
        {
            Vector3 segmentCenter = currentWorldPos + currentDir * (segmentHeight / 2f);

            GameObject segment = CreateCylinderAtPosition(
                $"TrunkSegment_{i}",
                segmentCenter,
                currentDir,
                currentRadius,
                segmentHeight,
                trunkMaterial
            );
            segment.transform.SetParent(trunkParent.transform);
            segment.isStatic = true;

            Vector3 segmentTop = currentWorldPos + currentDir * segmentHeight;
            currentWorldPos = segmentTop;

            Vector3 curvatureOffset = new Vector3(
                Random.Range(-trunkCurvature, trunkCurvature),
                0,
                Random.Range(-trunkCurvature, trunkCurvature)
            );
            currentDir = (currentDir + curvatureOffset).normalized;
            currentRadius *= 0.85f;

            // Store branch points
            if (i == trunkSegments - 1)
            {
                branchPoints.Add(new BranchPoint(segmentTop, currentDir, currentRadius));
            }
            else if (i >= 1)
            {
                float branchChance = (float)i / trunkSegments;
                if (Random.value < branchChance + 0.3f)
                {
                    Vector3 branchPos = currentWorldPos - currentDir * (segmentHeight * 0.3f);
                    branchPoints.Add(new BranchPoint(branchPos, currentDir, currentRadius));
                }
            }
        }

        // Generate branches from stored points
        float angleStep = 360f / mainBranches;
        int branchIndex = 0;

        for (int p = 0; p < branchPoints.Count; p++)
        {
            var point = branchPoints[p];
            bool isLastPoint = (p == branchPoints.Count - 1);
            int branchesFromThisPoint = isLastPoint ?
                Mathf.CeilToInt(mainBranches * 0.6f) :
                Random.Range(1, 3);

            for (int i = 0; i < branchesFromThisPoint; i++)
            {
                float angle = (branchIndex * angleStep) + Random.Range(-20f, 20f);
                float pitchAngle = Random.Range(branchAngleMin, branchAngleMax);

                Vector3 branchDir = Quaternion.Euler(-pitchAngle, angle, 0) * Vector3.up;

                GenerateBranch(
                    trunkParent.transform,
                    point.position,
                    branchDir,
                    point.radius * branchRadiusRatio,
                    trunkHeight * branchLengthRatio,
                    branchDepth,
                    $"Branch_{branchIndex}"
                );

                branchIndex++;
            }
        }

        // Add sucker branches (small unwanted branches growing from trunk)
        GenerateSuckerBranches(trunkParent.transform, bonsaiRoot.transform.position);
    }

    private void GenerateSuckerBranches(Transform parent, Vector3 trunkBase)
    {
        for (int i = 0; i < suckerBranches; i++)
        {
            // Position along the lower part of trunk
            float height = Random.Range(trunkHeight * 0.1f, trunkHeight * 0.6f);
            Vector3 suckerPos = trunkBase + Vector3.up * height;

            // Random outward direction, sometimes downward
            float angle = Random.Range(0f, 360f);
            float pitch = Random.Range(-20f, 60f); // Can point slightly down
            Vector3 suckerDir = Quaternion.Euler(-pitch, angle, 0) * Vector3.up;

            // Small thin branches
            float suckerRadius = trunkBaseRadius * Random.Range(0.2f, 0.4f);
            float suckerLength = trunkHeight * Random.Range(0.2f, 0.5f);

            GenerateBranch(
                parent,
                suckerPos,
                suckerDir,
                suckerRadius,
                suckerLength,
                Mathf.Min(2, branchDepth), // Shallow depth
                $"Sucker_{i}"
            );
        }

        // Add random foliage clusters around the base and trunk
        int extraClusters = Mathf.RoundToInt(foliageOvergrowth * wildness * 5);
        for (int i = 0; i < extraClusters; i++)
        {
            float height = Random.Range(trunkHeight * 0.3f, trunkHeight * 1.2f);
            Vector3 clusterPos = trunkBase + Vector3.up * height;
            clusterPos += Random.insideUnitSphere * (trunkHeight * 0.3f);
            CreateFoliageCluster(parent, clusterPos);
        }
    }

    private struct BranchPoint
    {
        public Vector3 position;
        public Vector3 direction;
        public float radius;

        public BranchPoint(Vector3 pos, Vector3 dir, float rad)
        {
            position = pos;
            direction = dir;
            radius = rad;
        }
    }

    private void GenerateBranch(Transform parent, Vector3 startPos, Vector3 direction, float radius, float length, int depth, string name)
    {
        if (depth <= 0 || radius < 0.003f)
        {
            // Terminal branches get multiple foliage clusters based on overgrowth
            int clusterCount = Mathf.RoundToInt(foliageOvergrowth * (0.5f + wildness * 0.5f));
            for (int c = 0; c < clusterCount; c++)
            {
                if (Random.value < foliageDensity)
                {
                    Vector3 offset = Random.insideUnitSphere * (length * 0.5f * (1f + wildness));
                    CreateFoliageCluster(parent, startPos + direction * (length * 0.3f) + offset);
                }
            }
            return;
        }

        Vector3 branchCenter = startPos + direction * (length / 2f);
        Vector3 branchEnd = startPos + direction * length;

        GameObject branch = CreateCylinderAtPosition(name, branchCenter, direction, radius, length, trunkMaterial);
        branch.transform.SetParent(parent);

        // More child branches when wild
        int childCount = Random.Range(2, 4 + Mathf.RoundToInt(wildness * 2));
        float baseAngle = Random.Range(0f, 360f);

        for (int i = 0; i < childCount; i++)
        {
            // More random angles when wild
            float angleVariation = 30f + wildness * 40f;
            float angle = baseAngle + (i * (360f / childCount)) + Random.Range(-angleVariation, angleVariation);

            // Wilder pitch range - can even go downward
            float pitchMin = 20f - wildness * 30f; // Can go negative (downward) when wild
            float pitchMax = 50f + wildness * 30f;
            float pitch = Random.Range(pitchMin, pitchMax);

            Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
            if (right.magnitude < 0.1f)
                right = Vector3.Cross(Vector3.forward, direction).normalized;

            Vector3 childDir = Quaternion.AngleAxis(angle, direction) *
                              Quaternion.AngleAxis(pitch, right) * direction;

            // Less upward tendency when wild - branches go in all directions
            float upwardBias = 0.15f * (1f - wildness * 0.8f);
            childDir = Vector3.Lerp(childDir, Vector3.up, upwardBias).normalized;

            // Random length variation for messiness
            float lengthVar = 1f + Random.Range(-wildness * 0.3f, wildness * 0.5f);

            GenerateBranch(
                parent,
                branchEnd,
                childDir,
                radius * branchRadiusRatio,
                length * branchLengthRatio * lengthVar,
                depth - 1,
                $"{name}_{i}"
            );
        }

        // More foliage clusters along branches when wild
        int foliageAlongBranch = Mathf.RoundToInt(foliageOvergrowth * wildness * 2);
        for (int f = 0; f < foliageAlongBranch; f++)
        {
            if (Random.value < foliageDensity * 0.6f)
            {
                float t = Random.Range(0.2f, 0.8f);
                Vector3 posOnBranch = Vector3.Lerp(startPos, branchEnd, t);
                Vector3 randomOffset = Random.onUnitSphere * (length * 0.3f);
                CreateFoliageCluster(parent, posOnBranch + randomOffset);
            }
        }
    }

    private void CreateFoliageCluster(Transform parent, Vector3 worldPosition)
    {
        GameObject cluster = new GameObject("FoliageCluster");
        cluster.transform.SetParent(parent);
        cluster.transform.position = worldPosition;
        cluster.transform.localScale = Vector3.one;

        for (int i = 0; i < leavesPerCluster; i++)
        {
            CreateLeaf(cluster.transform, worldPosition);
        }
    }

    private void CreateLeaf(Transform clusterParent, Vector3 clusterCenter)
    {
        GameObject leaf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaf.name = "Leaf";

        // Set tag safely
        try
        {
            leaf.tag = "Leaf";
        }
        catch
        {
            // Tag doesn't exist, skip
        }

        // Don't set layer if it doesn't exist (avoid -1 error)
        int leafLayer = LayerMask.NameToLayer("Leaf");
        if (leafLayer >= 0 && leafLayer <= 31)
        {
            leaf.layer = leafLayer;
        }

        Vector3 offset = Random.insideUnitSphere * clusterRadius;
        offset.y *= 0.6f;

        leaf.transform.SetParent(clusterParent);
        leaf.transform.localPosition = offset;
        leaf.transform.localRotation = Random.rotation;

        float sizeVariation = Random.Range(0.8f, 1.2f);
        float flatness = Random.Range(0.5f, 0.8f);
        leaf.transform.localScale = new Vector3(
            leafSize * sizeVariation,
            leafSize * sizeVariation * flatness,
            leafSize * sizeVariation
        );

        Renderer renderer = leaf.GetComponent<Renderer>();
        Material leafMat = new Material(leafMaterial);
        Color colorVar = new Color(
            Random.Range(-leafColorVariation.r, leafColorVariation.r),
            Random.Range(-leafColorVariation.g, leafColorVariation.g),
            Random.Range(-leafColorVariation.b, leafColorVariation.b)
        );
        leafMat.color = leafColorBase + colorVar;
        renderer.material = leafMat;

        SphereCollider collider = leaf.GetComponent<SphereCollider>();
        collider.isTrigger = true;

        allLeaves.Add(leaf);
    }

    private GameObject CreateCylinderAtPosition(string name, Vector3 worldCenter, Vector3 upDirection, float radius, float height, Material mat)
    {
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.name = name;

        // Position and orient BEFORE scaling
        cylinder.transform.position = worldCenter;
        cylinder.transform.up = upDirection;

        // Scale last (default cylinder is 2 units tall, 1 unit diameter)
        cylinder.transform.localScale = new Vector3(radius * 2f, height / 2f, radius * 2f);

        cylinder.GetComponent<Renderer>().material = mat;

        return cylinder;
    }

    public List<GameObject> GetAllLeaves()
    {
        return new List<GameObject>(allLeaves);
    }

    public int GetLeafCount()
    {
        return allLeaves.Count;
    }

    public int GetActiveLeafCount()
    {
        int count = 0;
        foreach (var leaf in allLeaves)
        {
            if (leaf != null && leaf.activeSelf)
                count++;
        }
        return count;
    }
}
