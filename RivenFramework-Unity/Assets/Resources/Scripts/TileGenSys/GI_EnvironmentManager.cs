using System;
using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using Unity.Mathematics;
using UnityEngine;
using Object = System.Object;

public class GI_EnvironmentManager : MonoBehaviour
{
    public ScriptableEnvironment[] environments;
    public ScriptableEnvironment startingEnvironment;
    public ScriptableEnvironment activeEnvironment;

    private void Awake()
    {
        activeEnvironment = startingEnvironment;
    }

    public ScriptableEnvironment GetEnvironment(string id)
    {
        foreach (var env in environments)
        {
            if (env.environmentID == id) return env;
        }
        return null;
    }

    /// <summary>
    /// Converts the coordinate scale from the relative scale to the global environment scale (1)
    /// </summary>
    public Vector2Int ToGlobalCoords(Vector2Int envCoord)
    {
        float scale = activeEnvironment.environmentRelativeScale;
        return new Vector2Int(Mathf.RoundToInt(envCoord.x * scale), Mathf.RoundToInt(envCoord.y * scale));
    }

    /// <summary>
    /// Converts the coordinate scale from the global scale to the relative environment scale
    /// </summary>
    public Vector2Int FromGlobalCoords(Vector2Int globalCoord, ScriptableEnvironment targetEnv)
    {
        float scale = targetEnv.environmentRelativeScale;
        return new Vector2Int(Mathf.RoundToInt(globalCoord.x * scale), Mathf.RoundToInt(globalCoord.y * scale));
    }

    public void WarpTo(string targetEnvironmentID, Vector2Int targetWorldCell, TDPawn_Player player)
    {
        var target = GetEnvironment(targetEnvironmentID);
        if (target == null)
        {
            Debug.LogWarning($"[EnvironmentManager] Unknown environment {targetEnvironmentID}");
            return;
        }

        StartCoroutine(DoWarp(target, targetWorldCell, player));
    }

    private IEnumerator DoWarp(ScriptableEnvironment target, Vector2Int targetCell, TDPawn_Player player)
    {
        var chunkManager = GameInstance.Get<GI_TileChunkManager>();
        chunkManager.OnSaveFileUnloaded();

        activeEnvironment = target;

        var tileDataManager = GameInstance.Get<GI_TileDataManager>();
        var worldPos = tileDataManager.tileGrid.CellToWorld(new Vector3Int(targetCell.x, targetCell.y, 0)) +
                       tileDataManager.tileGrid.cellSize * 0.5f;
        player.transform.position = worldPos;

        yield return null;
        
        chunkManager.OnSaveFileLoaded();
    }
}
