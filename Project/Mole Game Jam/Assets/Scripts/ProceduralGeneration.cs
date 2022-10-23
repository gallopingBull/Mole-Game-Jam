using UnityEngine;

[CreateAssetMenu(fileName = "BlockProcGenData", 
    menuName = "ScriptableObjects/BlockProcGenDatas")]
public class ProceduralGeneration : ScriptableObject
{
    public GameObject[] Blocks;
}
