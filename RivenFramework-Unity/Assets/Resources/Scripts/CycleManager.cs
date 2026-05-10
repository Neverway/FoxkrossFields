using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RivenFramework;
using UnityEngine;

/// <summary>
/// Keeps track of timed events in the game, like the mysteriousMerchantSpawning
/// </summary>
public class CycleManager : MonoBehaviour
{
    [Header("Cycle Settings")]
    public float cycleDuration = 600f;
    
    [Header("References")]
    public Light directionalLight;
    public GameObject fogSystemPrefab, mysteriousMerchantPrefab;
    public Vector2 merchantSpawnRange = new Vector2(5,12);
    
    [Header("Visuals")]
    public float lightTweenDuration = 3f;
    public Color dayLightColor, nightLightColor;
    
    public float cycleTimer = 0f;
    private bool isNight = false;

    private GameObject spawnedFog;
    private GameObject spawnedMerchant;
    private Transform playerTransform;

    private GI_TileDataManager tileDataManager;
    private GI_TileWorldGenerator worldGenerator;

    private void Update()
    {
        if (playerTransform == null)
        {
            var player = FindObjectOfType<TDPawn_Player>();
            if (player) playerTransform = player.transform;
            return;
        }

        if (directionalLight == null)
        {
            directionalLight = GameObject.FindGameObjectWithTag("SunLight").GetComponent<Light>();
            return;
        }

        cycleTimer += Time.deltaTime;

        if (cycleTimer >= cycleDuration)
        {
            cycleTimer -= cycleDuration;
            if (!isNight) BeginNight();
            else BeginDay();
        }

        if (spawnedFog) spawnedFog.transform.position = playerTransform.position;
    }
    
    private void BeginNight()
    {
        isNight = true;

        if (directionalLight != null)
            DOTween.To(() => directionalLight.color, c => directionalLight.color = c, nightLightColor, lightTweenDuration);

        if (fogSystemPrefab != null && spawnedFog == null)
            spawnedFog = Instantiate(fogSystemPrefab, playerTransform.position, Quaternion.identity);

        if (mysteriousMerchantPrefab != null && spawnedMerchant == null)
        {
            var spawnPos = FindValidMerchantSpawn();
            if (spawnPos.HasValue)
                spawnedMerchant = Instantiate(mysteriousMerchantPrefab, spawnPos.Value, Quaternion.identity);
        }

        Debug.Log("[CycleManager] Night began");
    }

    private void BeginDay()
    {
        isNight = false;

        if (directionalLight != null)
            DOTween.To(() => directionalLight.color, c => directionalLight.color = c, dayLightColor, lightTweenDuration);

        if (spawnedFog != null)
        {
            Destroy(spawnedFog);
            spawnedFog = null;
        }

        if (spawnedMerchant != null)
        {
            Destroy(spawnedMerchant);
            spawnedMerchant = null;
        }

        Debug.Log("[CycleManager] Day began");
    }
    
    private Vector3? FindValidMerchantSpawn()
    {
        if (tileDataManager == null) tileDataManager = GameInstance.Get<GI_TileDataManager>();
        if (worldGenerator == null) worldGenerator = GameInstance.Get<GI_TileWorldGenerator>();

        var solidTilemap = tileDataManager.GetTilemapFromLayer((int)TileLayers.ObjectsSolid);
        var objectTilemap = tileDataManager.GetTilemapFromLayer((int)TileLayers.Objects);

        for (int attempt = 0; attempt < 64; attempt++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float dist = Random.Range(merchantSpawnRange.x, merchantSpawnRange.y);
            int wx = Mathf.RoundToInt(playerTransform.position.x + Mathf.Cos(angle) * dist);
            int wy = Mathf.RoundToInt(playerTransform.position.y + Mathf.Sin(angle) * dist);
            var cell = new Vector3Int(wx, wy, 0);

            if (worldGenerator.IsWater(wx, wy)) continue;
            if (solidTilemap != null && solidTilemap.GetTile(cell) != null) continue;
            if (objectTilemap != null && objectTilemap.GetTile(cell) != null) continue;

            return tileDataManager.tileGrid.CellToWorld(cell) + tileDataManager.tileGrid.cellSize * 0.5f;
        }

        Debug.LogWarning("[CycleManager] Could not find valid merchant spawn position");
        return null;
    }
    
    public void SaveCycle(out float savedTimer, out bool savedIsNight)
    {
        savedTimer = cycleTimer;
        savedIsNight = isNight;
    }
    
    public void LoadCycle(float savedTimer, bool savedIsNight, long savedTimestamp)
    {
        float elapsed = savedTimestamp > 0
            ? (float)System.TimeSpan.FromTicks(System.DateTime.UtcNow.Ticks - savedTimestamp).TotalSeconds
            : 0f;

        float totalTime = savedTimer + elapsed;

        int halfCycles = Mathf.FloorToInt(totalTime / cycleDuration);
        cycleTimer = totalTime % cycleDuration;

        bool phaseFlipped = (halfCycles % 2) != 0;
        isNight = phaseFlipped ? !savedIsNight : savedIsNight;

        ApplyStateImmediate();

        Debug.Log($"[CycleManager] Loaded cycle — isNight:{isNight} timer:{cycleTimer:F1} (offline: {elapsed:F1}s, {halfCycles} half-cycles passed)");
    }

    private void ApplyStateImmediate()
    {
        if (directionalLight != null)
            directionalLight.color = isNight ? nightLightColor : dayLightColor;

        if (isNight)
        {
            if (fogSystemPrefab != null && spawnedFog == null && playerTransform != null)
                spawnedFog = Instantiate(fogSystemPrefab, playerTransform.position, Quaternion.identity);

            if (mysteriousMerchantPrefab != null && spawnedMerchant == null)
            {
                Vector3? spawnPos = FindValidMerchantSpawn();
                if (spawnPos.HasValue)
                    spawnedMerchant = Instantiate(mysteriousMerchantPrefab, spawnPos.Value, Quaternion.identity);
            }
        }
        else
        {
            if (spawnedFog != null) { Destroy(spawnedFog); spawnedFog = null; }
            if (spawnedMerchant != null) { Destroy(spawnedMerchant); spawnedMerchant = null; }
        }
    }
}
