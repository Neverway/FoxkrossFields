using UnityEngine;

[CreateAssetMenu(fileName = "PlantData", menuName = "FoFi/Plant Data")]
public class ScriptablePlant : ScriptableObject
{
    [System.Serializable]
    public class GrowthStage
    {
        public string tileID;
        public float duration;
    }

    public GrowthStage[] stages;
    public string seedItemID;
    public string harvestItemID;
    public int harvestCount = 1;
}