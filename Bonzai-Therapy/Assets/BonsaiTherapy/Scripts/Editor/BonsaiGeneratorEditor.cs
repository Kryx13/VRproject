using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BonsaiGenerator))]
public class BonsaiGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        BonsaiGenerator generator = (BonsaiGenerator)target;

        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        // Generate button
        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Generate Bonsai", GUILayout.Height(30)))
        {
            generator.GenerateBonsai();
        }

        // Clear button
        GUI.backgroundColor = new Color(0.8f, 0.4f, 0.4f);
        if (GUILayout.Button("Clear Bonsai", GUILayout.Height(30)))
        {
            generator.ClearBonsai();
        }

        GUI.backgroundColor = Color.white;
        EditorGUILayout.EndHorizontal();

        // Reset leaves button
        GUI.backgroundColor = new Color(0.4f, 0.6f, 0.9f);
        if (GUILayout.Button("Reset All Leaves", GUILayout.Height(25)))
        {
            generator.ResetAllLeaves();
        }
        GUI.backgroundColor = Color.white;

        // Info section
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"Total Leaves: {generator.GetLeafCount()}");
        EditorGUILayout.LabelField($"Active Leaves: {generator.GetActiveLeafCount()}");
        EditorGUILayout.EndVertical();

        // Setup helper
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Setup Helper", EditorStyles.boldLabel);

        if (GUILayout.Button("Setup Leaf Layer & Tag"))
        {
            SetupLeafLayerAndTag();
        }

        // Tips
        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "Tips:\n" +
            "1. Assign the Table transform or name it 'Table' for auto-detection.\n" +
            "2. Use 'Setup Leaf Layer & Tag' to create required layers.\n" +
            "3. Adjust Foliage Density for performance optimization.",
            MessageType.Info
        );
    }

    private void SetupLeafLayerAndTag()
    {
        // Check if Leaf tag exists, if not create it
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]
        );

        // Setup Tag
        SerializedProperty tagsProp = tagManager.FindProperty("tags");
        bool tagExists = false;

        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Leaf")
            {
                tagExists = true;
                break;
            }
        }

        if (!tagExists)
        {
            tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
            tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = "Leaf";
            Debug.Log("Created 'Leaf' tag.");
        }
        else
        {
            Debug.Log("'Leaf' tag already exists.");
        }

        // Setup Layer
        SerializedProperty layersProp = tagManager.FindProperty("layers");
        int leafLayerIndex = -1;

        // Find first empty layer slot (starting from 8, user layers)
        for (int i = 8; i < layersProp.arraySize; i++)
        {
            string layerName = layersProp.GetArrayElementAtIndex(i).stringValue;
            if (layerName == "Leaf")
            {
                leafLayerIndex = i;
                Debug.Log($"'Leaf' layer already exists at index {i}.");
                break;
            }
            if (string.IsNullOrEmpty(layerName) && leafLayerIndex == -1)
            {
                leafLayerIndex = i;
            }
        }

        if (leafLayerIndex != -1 && layersProp.GetArrayElementAtIndex(leafLayerIndex).stringValue != "Leaf")
        {
            layersProp.GetArrayElementAtIndex(leafLayerIndex).stringValue = "Leaf";
            Debug.Log($"Created 'Leaf' layer at index {leafLayerIndex}.");
        }

        tagManager.ApplyModifiedProperties();

        EditorUtility.DisplayDialog("Setup Complete",
            "Leaf Tag and Layer have been configured.\n\n" +
            "Remember to configure the Layer Collision Matrix:\n" +
            "Edit > Project Settings > Physics > Layer Collision Matrix\n" +
            "Make 'Leaf' layer only collide with 'Tool' layer.",
            "OK");
    }
}
