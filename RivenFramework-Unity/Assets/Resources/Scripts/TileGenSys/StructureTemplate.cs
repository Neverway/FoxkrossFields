using UnityEngine;

[CreateAssetMenu(menuName = "World Gen/Structure Template")]
public class StructureTemplate : ScriptableObject
{
    public string id;
    public StructureTile[] tiles;
}

[System.Serializable]
public class StructureTile
{
    public int dx, dy;
    public string tileID;
    public int layer;
    public bool isWarp;
    public string warpTargetEnvironment;
}
