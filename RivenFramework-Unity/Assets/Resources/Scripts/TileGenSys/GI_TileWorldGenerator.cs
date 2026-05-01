using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;

public class GI_TileWorldGenerator : MonoBehaviour
{
    [Header("World Settings")] 
    public int seed = 1997;
    
    [Header("Noise Settings")]
    public float groundNoiseScale = 0.08f;
    public float pathNoiseScale = 0.12f;
    public float treeNoiseScale = 0.15f;
    public float treeThreshold = 0.6f;
    public float pathThreshold = 0.55f;
    
    [Header("Tile IDs")]
    public string grassTileID = "grass";
    public string dirtTileID = "dirt";
    public string grassPathTileID = "grass_path";
    public string dirtPathTileID = "dirt_path";
    public string treeTileID = "tree";

    private Vector2 groundOffset, pathOffset, treeOffset;

    public void Awake()
    {
        // Select some deterministic random offsets for seeds
        var rng = new System.Random(seed);
        groundOffset = new Vector2(rng.Next(-10000, 10000), rng.Next(-10000, 10000));
        pathOffset = new Vector2(rng.Next(-10000, 10000), rng.Next(-10000, 10000));
        treeOffset = new Vector2(rng.Next(-10000, 10000), rng.Next(-10000, 10000));
    }

    public ChunkData GenerateChunk(Vector2Int chunkCoord, int chunkSize)
    {
        var tileDataManager = GameInstance.Get<GI_TileDataManager>();
        var data = new ChunkData(chunkCoord, chunkSize);

        for (int x = 0; x < chunkSize; x++)
        {
            for (int y = 0; y < chunkSize; y++)
            {
                int worldX = chunkCoord.x * chunkSize + x;
                int worldY = chunkCoord.y * chunkSize + y;
                
                // Layer 0 - ground and stuff
                float groundNoise = Mathf.PerlinNoise((worldX+groundOffset.x)*groundNoiseScale,(worldY+groundOffset.y)*groundNoiseScale);
                string groundID = groundNoise > 0.5f ? grassTileID : dirtTileID;
                data.SetTile(0, x, y, groundID);

                // Layer 1 - path stuff
                float pathNoise = Mathf.PerlinNoise((worldX+pathOffset.x)*pathNoiseScale,(worldY+pathOffset.y)*pathNoiseScale);
                if (pathNoise > pathThreshold)
                {
                    string pathID = groundNoise > 0.5f ? grassPathTileID : dirtPathTileID;
                    data.SetTile(1, x, y, pathID);
                }
                
                // Layer 2 - rivers and lakes

                // Layer 3 - trees
                if (pathNoise <= pathThreshold)
                {
                    float treeNoise = Mathf.PerlinNoise((worldX+treeOffset.x)*treeNoiseScale,(worldY+treeOffset.y)*treeNoiseScale);
                    if (treeNoise > treeThreshold) 
                        data.SetTile(3, x, y, treeTileID);
                }
            }
        }

        return data;
    }
}
