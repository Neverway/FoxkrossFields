//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using RivenFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WB_DeathScreen : MonoBehaviour
{
    //=-----------------=
    // Public Variables
    //=-----------------=


    //=-----------------=
    // Private Variables
    //=-----------------=
    private bool acceptingInputs;


    //=-----------------=
    // Reference Variables
    //=-----------------=
    private GI_WorldLoader worldLoader;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    private void Start()
    {
        StartCoroutine(InputDelay());
    }

    private void Update()
    {
        if (!acceptingInputs) return;
        if (Input.anyKeyDown)
        {
            Destroy(gameObject);
            FindObjectOfType<TDPawn_Player>().Respawn();
        }
    }

    //=-----------------=
    // Internal Functions
    //=-----------------=
    private IEnumerator InputDelay()
    {
        yield return new WaitForSeconds(0.2f);
        acceptingInputs = true;
    }


    //=-----------------=
    // External Functions
    //=-----------------=
}
