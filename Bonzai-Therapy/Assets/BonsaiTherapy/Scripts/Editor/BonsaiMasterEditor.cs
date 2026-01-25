using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BonsaiMaster))]
public class BonsaiMasterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BonsaiMaster master = (BonsaiMaster)target;

        // Header
        EditorGUILayout.Space(5);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Bonsai Therapy Generator", headerStyle);
        EditorGUILayout.Space(10);

        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        // Main action buttons
        EditorGUILayout.LabelField("Generation", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
        if (GUILayout.Button("Create Complete Bonsai", GUILayout.Height(35)))
        {
            master.CreateCompleteBonsai();
            EditorUtility.SetDirty(master);
        }

        GUI.backgroundColor = new Color(0.7f, 0.5f, 0.3f);
        if (GUILayout.Button("Randomize", GUILayout.Height(35)))
        {
            master.RandomizeBonsai();
            EditorUtility.SetDirty(master);
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        GUI.backgroundColor = new Color(0.8f, 0.3f, 0.3f);
        if (GUILayout.Button("Clear Bonsai", GUILayout.Height(25)))
        {
            master.ClearCompleteBonsai();
        }
        GUI.backgroundColor = Color.white;

        // Quick create menu
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Quick Create (One-Click)", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Formal"))
        {
            master.style = BonsaiMaster.BonsaiStyle.FormalUpright;
            master.CreateCompleteBonsai();
        }
        if (GUILayout.Button("Informal"))
        {
            master.style = BonsaiMaster.BonsaiStyle.InformalUpright;
            master.CreateCompleteBonsai();
        }
        if (GUILayout.Button("Slanting"))
        {
            master.style = BonsaiMaster.BonsaiStyle.Slanting;
            master.CreateCompleteBonsai();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Cascade"))
        {
            master.style = BonsaiMaster.BonsaiStyle.Cascade;
            master.CreateCompleteBonsai();
        }
        if (GUILayout.Button("Windswept"))
        {
            master.style = BonsaiMaster.BonsaiStyle.Windswept;
            master.CreateCompleteBonsai();
        }
        EditorGUILayout.EndHorizontal();

        // Info box
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "How to use:\n" +
            "1. Create an empty GameObject in your scene\n" +
            "2. Add this BonsaiMaster component\n" +
            "3. Assign the Table transform (or name it 'Table')\n" +
            "4. Choose a style and click 'Create Complete Bonsai'\n\n" +
            "The bonsai will appear on the table with pot and realistic foliage.",
            MessageType.Info
        );
    }

    [MenuItem("GameObject/3D Object/Bonsai Therapy/Complete Bonsai", false, 10)]
    static void CreateBonsaiFromMenu(MenuCommand menuCommand)
    {
        GameObject bonsaiObj = new GameObject("BonsaiSetup");
        bonsaiObj.AddComponent<BonsaiMaster>();

        GameObjectUtility.SetParentAndAlign(bonsaiObj, menuCommand.context as GameObject);
        Undo.RegisterCreatedObjectUndo(bonsaiObj, "Create Bonsai");
        Selection.activeObject = bonsaiObj;

        Debug.Log("Bonsai setup created! Assign a table and click 'Create Complete Bonsai' in the inspector.");
    }
}
