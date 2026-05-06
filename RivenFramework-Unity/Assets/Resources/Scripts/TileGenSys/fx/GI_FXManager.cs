using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GI_FXManager : MonoBehaviour
{
    [Header("Tile Break FX")]
    public GameObject tileBreakParticlePrefab;
    public Material particleBaseMaterial;
    public int poolSize = 16;

    private Queue<GameObject> pool = new();

    private void Start()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(tileBreakParticlePrefab);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    public GameObject RentParticleSystem()
    {
        GameObject go;
        if (pool.Count > 0)
        {
            go = pool.Dequeue();
        }
        else
        {
            return null;
            //go = Instantiate(tileBreakParticlePrefab);
        }
        go.SetActive(true);
        return go;
    }

    public void ReturnAfterFinished(GameObject go, ParticleSystem ps)
    {
        StartCoroutine(ReturnWhenDone(go, ps));
    }

    private IEnumerator ReturnWhenDone(GameObject go, ParticleSystem ps)
    {
        yield return new WaitUntil(() => !ps.IsAlive(true));
        
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        if (renderer.material != null)
            Destroy(renderer.material);
        
        go.SetActive(false);
        pool.Enqueue(go);
    }
}
