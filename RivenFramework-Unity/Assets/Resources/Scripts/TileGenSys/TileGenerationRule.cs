using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileGenerationRule
{
    [Header("Rule")] 
    public string ruleName = "";
    public bool generationRuleDisabled;

    [Header("Output")] 
    public TileLayers targetLayer = 0;
    public string[] tileIDs = new[] { "" };
    
    [Header("Noise")]
    [Tooltip("The type of noise pattern to use for placing these tiles")]
    public NoiseType noiseType = NoiseType.Perlin;
    [Tooltip("The scale of the noise pattern")]
    public float noiseScale = 0.08f;
    [Tooltip("Tiles with noise above this value are placed (or used as the split point when using two tiles)")]
    public float threshold = 0.5f;
    public NoiseBlendMode blendMode = NoiseBlendMode.Override;
    [HideInInspector] public Vector2 noiseOffset;

    [Header("Tile Selection")] 
    [Tooltip("Override: randomly pick a tile from tileIDs to place \n" +
             "TwoTile: tileID at index 0 will be placed below threshold, tileID at index 1 will be placed above threshold")]
    public TileSelectionMode tileSelectionMode = TileSelectionMode.Override;
    [Tooltip("Leave empty to run in all environments, other wise set this to only allow this operation to run in this environment")]
    public string environmentFilter = "";

    [Header("Conditions")] 
    [Tooltip("None: use the regular noise placement method \n" +
             "Tree: place trunk, dont place over paths\n" +
             "Leaf: place leaves around two-tall trunks or higher\n" +
             "Path: path overlay")] 
    public GenerationProcedure generationProcedure = GenerationProcedure.None;
    [Tooltip("Only place if this specified layer has a tile here (-1 ignore)")]
    public TileLayers[] requireLayersFilled = new TileLayers[0];
    [Tooltip("Only place if this specified layer does not have a tile here (-1 ignore)")]
    public TileLayers[] requireLayersEmpty = new TileLayers[0];
}

public enum NoiseBlendMode
{
    Override,
    UnderOnly
}

public enum TileSelectionMode
{
    Override,
    TwoTile
}

public enum GenerationProcedure
{
    None,
    Tree,
    Leaf,
    Path,
    Water,
    GroundEdge,
    CaveWall,
    DeathPit,
}

public enum NoiseType
{
    Perlin,
    Value,
    Cellular,
    White,
}
