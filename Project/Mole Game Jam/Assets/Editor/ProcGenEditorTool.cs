using UnityEditor;
using UnityEngine;
using System;

[Serializable]
public class ProcGenEditorTool : EditorWindow
{
    private static int[] _possibleValues = new int[] {0, 1, 0, 2, 0, 3, 0, 4, 0, 5};
    private static GameObject[] _blockMeshes;
    private static ProceduralGeneration _blockData;

    public void OnGUI()
    {
        EditorGUILayout.LabelField("Level", "hello");
        if (GUILayout.Button("Randomize Bricks"))
            SwapBlocks();
        if (GUILayout.Button("Delete All Bricks"))
            DeleteAllBlocks();
    }

    [MenuItem("Tools/Generate Random Blocks", false, -1)]  
    private static void Init()
    {
        GetWindow(typeof(ProcGenEditorTool));
    }

    // This method will be called on load or recompile
    //[InitializeOnLoadMethod]
    //private static void OnLoad()
    //{
    //    // if no data exists yet create and reference a new instance
    //    if (!_blockData)
    //    {
    //        var SOpath = GetPathToBlocksScriptableObject();
    //
    //        _blockData = (ProceduralGeneration)AssetDatabase.LoadAssetAtPath(SOpath, typeof(ProceduralGeneration));
    //        _blockMeshes = _blockData.Blocks;
    //
    //        Init();
    //    }
    //}
    
    public static void SwapBlocks()
    {
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Wall");

        // only swap blocks if first wall block has no child objects
        if (blocks[0].transform.childCount == 0)
            for (int i = 0; i < blocks.Length; i++)
                GenerateNewBlock(blocks[i].transform);
    }

    private static void GenerateNewBlock(Transform target)
    {
        int index = UnityEngine.Random.Range(0, _possibleValues.Length);

        GameObject tmpBlock = Instantiate(_blockMeshes[_possibleValues[index]], target);

        tmpBlock.transform.localPosition = Vector3.zero;
        tmpBlock.transform.localRotation = GenerateNewRotation();
    }

    private static void DeleteAllBlocks()
    {
        GameObject[] blocksMeshes = GameObject.FindGameObjectsWithTag("BlockMesh");

        if (blocksMeshes.Length > 0)
            for (int i = 0; i < blocksMeshes.Length; i++)
                DestroyImmediate(blocksMeshes[i]);
    }

    private static Quaternion GenerateNewRotation()
    {
        float x = UnityEngine.Random.Range(0, 4) * 90;
        float y = UnityEngine.Random.Range(0, 4) * 90;
        float z = UnityEngine.Random.Range(0, 4) * 90;
        return Quaternion.Euler(x, y, z);
    }

    private static string GetPathToBlocksScriptableObject()
    {
        string[] _assetSearchFolders = new string[1];
        _assetSearchFolders[0] = "Assets/Scripts/ScriptableObjects";

        string[] SO_GUIDs = AssetDatabase.FindAssets("t:ProceduralGeneration", _assetSearchFolders);
        return AssetDatabase.GUIDToAssetPath(SO_GUIDs[0]);
    }
}
