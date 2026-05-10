using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using Random = UnityEngine.Random;

public class GI_TileWorldGenerator : MonoBehaviour
{
    [Header("World Settings")] 
    public int seed = 1997;

    [Header("Generation Rules")] 
    public TileGenerationRule[] generationRules;
    
    [Header("Tree Settings")]
    public string treeTileID = "tree";
    public string leafTileID = "leaf";
    public float treeNoiseScale = 0.15f;
    public float treeThreshold = 0.6f;
    
    [Header("Path Settings")]
    public float pathNoiseScale = 0.12f;
    public float pathThreshold = 0.55f;
    
    [Header("Water Settings")]
    public string waterTileID = "water";
    public string groundEdgeTileID = "ground_edge";
    public float waterNoiseScale = 0.08f;
    public float waterThreshold = 0.72f;
    
    [Header("River Settings")]
    public float riverNoiseScale = 0.03f;

    public float riverThreshold = 0.52f;
    public float riverBandwidth = 0.04f;
    public float riverWarpScale = 0.06f;
    public float riverWarpStrength = 6f;
    
    private Vector2 pathOffset, treeOffset, waterOffset, riverOffset, riverWarpOffset;


    public void Awake()
    {
        // Select some deterministic random offsets for seeds
        var rng = new System.Random(seed);

        foreach (var rule in generationRules) rule.noiseOffset = NextVector2RNGOffset(rng);
        pathOffset = NextVector2RNGOffset(rng);
        treeOffset = NextVector2RNGOffset(rng);
        waterOffset = NextVector2RNGOffset(rng);
        riverOffset = NextVector2RNGOffset(rng);
        riverWarpOffset = NextVector2RNGOffset(rng);
    }

    private static Vector2 NextVector2RNGOffset(System.Random rng)
    {
        return new Vector2(rng.Next(-10000, 10000), rng.Next(-10000, 10000));
    } 

    public ChunkData GenerateChunk(Vector2Int chunkCoord, int chunkSize)
    {
        var data = new ChunkData(chunkCoord, chunkSize);

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int worldX = chunkCoord.x * chunkSize + x;
                int worldY = chunkCoord.y * chunkSize + y;
                ApplyRules(data, worldX, worldY, x, y);
            }
        }

        return data;
    }

    private void ApplyRules(ChunkData data, int worldX, int worldY, int layerX, int layerY)
    {
        foreach (var rule in generationRules)
        {
            if (rule.generationRuleDisabled) continue;
            
            switch (rule.generationProcedure)
            {
                case GenerationProcedure.Tree: 
                    ApplyTreeRule(data, worldX, worldY, layerX, layerY, rule); 
                    continue;
                case GenerationProcedure.Leaf: 
                    ApplyLeafRule(data, worldX, worldY, layerX, layerY, rule); 
                    continue;
                case GenerationProcedure.Path: 
                    ApplyPathRule(data, worldX, worldY, layerX, layerY, rule); 
                    continue;
                case GenerationProcedure.Water: 
                    ApplyWaterRule(data, worldX, worldY, layerX, layerY, rule); 
                    continue;
                case GenerationProcedure.GroundEdge: 
                    ApplyGroundEdgeRule(data, worldX, worldY, layerX, layerY, rule); 
                    continue;
            }

            float noise = SampleNoise(rule.noiseType, worldX, worldY, rule.noiseOffset, rule.noiseScale);

            if (rule.tileSelectionMode == TileSelectionMode.TwoTile)
            {
                if (!PassesConditions(data, layerX, layerY, rule)) continue;
                if (rule.tileIDs.Length < 2) continue;
                
                string pick = noise < rule.threshold ? rule.tileIDs[0] : rule.tileIDs[1];
                WriteTile(data, layerX, layerY, (int)rule.targetLayer, pick, rule.blendMode);
                continue;
            }
            
            if (noise <= rule.threshold) continue;
            if (!PassesConditions(data, layerX, layerY, rule)) continue;
            string tileID = rule.tileIDs.Length == 1
                ? rule.tileIDs[0]
                : rule.tileIDs[Random.Range(0, rule.tileIDs.Length)];
            WriteTile(data, layerX, layerY, (int)rule.targetLayer, tileID, rule.blendMode);
        }
    }

    private bool PassesConditions(ChunkData data, int layerX, int layerY, TileGenerationRule rule)
    {
        foreach (var layer in rule.requireLayersFilled)
        {
            if (layer == TileLayers.None) continue;
            if (string.IsNullOrEmpty(data.GetTile((int)layer, layerX, layerY))) return false;
        }

        foreach (var layer in rule.requireLayersEmpty)
        {
            if (layer == TileLayers.None) continue;
            if (!string.IsNullOrEmpty(data.GetTile((int)layer, layerX, layerY))) return false;
        }
        return true;
    }

    private void WriteTile(ChunkData data, int layerX, int layerY, int layer, string id, NoiseBlendMode blendMode)
    {
        if (blendMode == NoiseBlendMode.UnderOnly && !string.IsNullOrEmpty(data.GetTile(layer, layerX, layerY)))
        {
            return;
        }
        data.SetTile(layer, layerX, layerY, id);
    }

    
    // Procedures for trees and paths and things
    private void ApplyTreeRule(ChunkData data, int worldX, int worldY, int layerX, int layerY, TileGenerationRule rule)
    {
        if (IsWater(worldX, worldY)) return;
        if (IsTrunk(worldX, worldY)) WriteTile(data, layerX, layerY, (int)rule.targetLayer, treeTileID, rule.blendMode);
    }

    private void ApplyLeafRule(ChunkData data, int worldX, int worldY, int layerX, int layerY, TileGenerationRule rule)
    {
        if (ShouldHaveLeaf(worldX, worldY)) WriteTile(data, layerX, layerY, (int)rule.targetLayer, leafTileID, rule.blendMode);
    }

    private void ApplyPathRule(ChunkData data, int worldX, int worldY, int layerX, int layerY, TileGenerationRule rule)
    {
        if (IsWater(worldX, worldY)) return;
        float pNoise = Mathf.PerlinNoise((worldX + pathOffset.x) * pathNoiseScale, (worldY + pathOffset.y) * pathNoiseScale);
        if (pNoise <= pathThreshold) return;
        if (!PassesConditions(data, layerX, layerY, rule)) return;
        if (rule.tileIDs.Length < 1) return;

        string ground = data.GetTile(0, layerX, layerY);
        string tileID = rule.tileIDs.Length >= 2 ? (ground == rule.tileIDs[0] ? rule.tileIDs[0] : rule.tileIDs[1]) : rule.tileIDs[0];
        WriteTile(data, layerX, layerY, (int)rule.targetLayer, tileID, rule.blendMode);
    }
    
    private void ApplyWaterRule(ChunkData data, int worldX, int worldY, int layerX, int layerY, TileGenerationRule rule)
    {
        if (!IsWater(worldX, worldY)) return;
        if (!PassesConditions(data, layerX, layerY, rule)) return;
        string tileID = rule.tileIDs.Length > 0 ? rule.tileIDs[0] : waterTileID;
        WriteTile(data, layerX, layerY, (int)rule.targetLayer, tileID, rule.blendMode);
    }

    private void ApplyGroundEdgeRule(ChunkData data, int worldX, int worldY, int layerX, int layerY, TileGenerationRule rule)
    {
        if (!IsWater(worldX, worldY)) return;
        bool nextToLand = false;
        for (int dx = -1; dx <= 1 && !nextToLand; dx++)
        {
            for (int dy = -1; dy <= 1 && !nextToLand; dy++)
            {
                if (dx != 0 || dy != 0)
                {
                    if (!IsWater(worldX + dx, worldY + dy)) nextToLand = true;
                }
            }
        }
        if (!nextToLand) return;
        if (!PassesConditions(data, layerX, layerY, rule)) return;
        string tileID = rule.tileIDs.Length > 0 ? rule.tileIDs[0] : groundEdgeTileID;
        WriteTile(data, layerX, layerY, (int)rule.targetLayer, tileID, rule.blendMode);
    }

    public bool IsWater(int worldX, int worldY)
    {
        float noise = Mathf.PerlinNoise((worldX + waterOffset.x) * waterNoiseScale, (worldY + waterOffset.y) * waterNoiseScale);
        return noise > waterThreshold || IsRiver(worldX, worldY);
    }

    public bool IsRiver(int worldX, int worldY)
    {
        float warpX = Mathf.PerlinNoise((worldX + riverWarpOffset.x) * riverWarpScale, (worldY + riverWarpOffset.y) * riverWarpScale);
        float warpY = Mathf.PerlinNoise((worldX + riverWarpOffset.y) * riverWarpScale, (worldY + riverWarpOffset.x) * riverWarpScale);
        float sx = worldX + (warpX - 0.5f) * riverWarpStrength;
        float sy = worldY + (warpY - 0.5f) * riverWarpStrength;
        float noise = Mathf.PerlinNoise((sx + riverOffset.x) * riverNoiseScale, (sy + riverOffset.y) * riverNoiseScale);
        return Mathf.Abs(noise - riverThreshold) < riverBandwidth;
    }

    public bool IsTrunk(int worldX, int worldY, float cachedPathNoise = -1f)
    {
        if (IsWater(worldX, worldY)) return false;
        float pNoise = cachedPathNoise >= 0 ? cachedPathNoise : Mathf.PerlinNoise((worldX + pathOffset.x) * pathNoiseScale, (worldY + pathOffset.y) * pathNoiseScale);
        if (pNoise > pathThreshold) return false;
        float tNoise = Mathf.PerlinNoise((worldX + treeOffset.x) * treeNoiseScale, (worldY + treeOffset.y) * treeNoiseScale);
        return tNoise > treeThreshold;
    }

    private bool TrunkHasLeaves(int tx, int ty)
    { 
        return IsTrunk(tx, ty + 1);
    }

    public bool ShouldHaveLeaf(int worldX, int worldY)
    {
        for (int tx = worldX - 1; tx <= worldX + 1; tx++)
        {
            for (int ty = worldY - 3; ty <= worldY - 1; ty++)
            {
                if (IsTrunk(tx, ty) && TrunkHasLeaves(tx, ty)) return true;
            }
        }

        return false;
    }
    
    
    
    
    // CONFUSING NOISE MATH BELOW!!!
    private float SampleNoise(NoiseType type, int wx, int wy, Vector2 offset, float scale)
    {
        float sx = (wx + offset.x) * scale;
        float sy = (wy + offset.y) * scale;

        return type switch
        {
            NoiseType.Perlin => Mathf.PerlinNoise(sx, sy),
            NoiseType.Value => ValueNoise(sx, sy),
            NoiseType.Cellular => CellularNoise(sx, sy),
            NoiseType.White => Hash(wx + (int)offset.x, wy + (int)offset.y),
            _ => Mathf.PerlinNoise(sx, sy)
        };
    }

    private static float ValueNoise(float x, float y)
    {
        int x0 = Mathf.FloorToInt(x), y0 = Mathf.FloorToInt(y);
        float tx = x - x0, ty = y - y0;
        float u = tx * tx * (3 - 2 * tx);
        float v = ty * ty * (3 - 2 * ty);

        float v00 = Hash(x0, y0);
        float v10 = Hash(x0 + 1, y0);
        float v01 = Hash(x0, y0 + 1);
        float v11 = Hash(x0 + 1, y0 + 1);

        return Mathf.Lerp(Mathf.Lerp(v00, v10, u), Mathf.Lerp(v01, v11, u), v);
    }

    private static float CellularNoise(float x, float y)
    {
        int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y);
        float minDist = float.MaxValue;

        for (int dx = -1; dx <= 1; dx++)
        for (int dy = -1; dy <= 1; dy++)
        {
            int cx = ix + dx, cy = iy + dy;
            float px = cx + Hash2(cx, cy);
            float py = cy + Hash2(cy, cx);
            float d  = (x - px) * (x - px) + (y - py) * (y - py);
            if (d < minDist) minDist = d;
        }

        return Mathf.Clamp01(Mathf.Sqrt(minDist));
    }

    private static float Hash(int x, int y)
    {
        int h = x * 374761393 + y * 668265263;
        h = (h ^ (h >> 13)) * 1274126177;
        h ^= h >> 16;
        return (h & 0x7fffffff) / (float)0x7fffffff;
    }

    private static float Hash2(int a, int b)
    {
        int h = a * 1557234693 + b * 374761393;
        h = (int)((h ^ (h >> 15)) * 2246822519);
        h ^= h >> 13;
        return (h & 0x7fffffff) / (float)0x7fffffff;
    }
}
