using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class FXHelper
{
    public static void SpawnTileBreakParticles(Vector3 worldPos, TileBase tileBase, Tilemap tilemap, Vector3Int cell, int count = 1)
    {
        if (tileBase == null) return;

        Sprite sprite = tilemap.GetSprite(cell);
        if (sprite == null) return;

        var prefab = GameInstance.Get<GI_FXManager>().tileBreakParticlePrefab;
        if (prefab == null) return;

        var go = GameInstance.Get<GI_FXManager>().RentParticleSystem();
        if (go == null) return;

        var particleSystem = go.GetComponent<ParticleSystem>();
        go.transform.position = worldPos;

        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        Texture2D tex = sprite.texture;
        Rect r = sprite.textureRect;
        var mat = new Material(GameInstance.Get<GI_FXManager>().particleBaseMaterial);
        mat.mainTexture = tex;
        mat.mainTextureScale  = new Vector2(r.width / tex.width, r.height / tex.height);
        mat.mainTextureOffset = new Vector2(r.x / tex.width, r.y / tex.height);
        renderer.material = mat;
        
        var main = particleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;
        
        particleSystem.Emit(count);
        GameInstance.Get<GI_FXManager>().ReturnAfterFinished(go, particleSystem);
    }
    
    

    private static Sprite GetSpriteFromTile(TileBase tileBase)
    {
        if (tileBase is Tile t) return t.sprite;

        var property = tileBase.GetType().GetProperty("sprite");
        if (property != null) return property.GetValue(tileBase) as Sprite;

        return null;
    }
}
