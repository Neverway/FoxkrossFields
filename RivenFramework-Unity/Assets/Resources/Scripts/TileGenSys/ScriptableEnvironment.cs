using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ENV_", menuName = "Neverway/Environment")]
public class ScriptableEnvironment : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("The name of the environment (used for stuff like the chunk folder data and shtuff)")]
    public string environmentID;

    [Header("Scale")]
    [Tooltip("1 for scale means that this environment is not scaled, 0.5 means that traveling 1 tile in this environment is the same as traveling 4 tiles in an unscaled environment")]
    public float environmentRelativeScale;
    
    [Header("Generation")]
    public GI_TileWorldGenerator generatorPrefab;
    
    [Header("Starting Environment")]
    public bool isStartingEnvironment = false;
}
