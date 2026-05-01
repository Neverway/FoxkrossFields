//===================== (Neverway 2024) Written by Liz M. =====================
//
// Purpose:
// Notes:
//
//=============================================================================

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
    
public class TDPawn : Pawn
{
    //=-----------------=
    // Public Variables
    //=-----------------=
    [HideInInspector] public List<Pawn> visiblePawns = new List<Pawn>();
    [HideInInspector] public List<Pawn> visibleHostiles = new List<Pawn>();
    [HideInInspector] public List<Pawn> visibleAllies = new List<Pawn>();


    //=-----------------=
    // Private Variables
    //=-----------------=


    //=-----------------=
    // Reference Variables
    //=-----------------=
    // These values are cast back to the base pawn classes currentStats and defaultStats
    public TDPawnStats TDDefaultStats;
    public TDPawnStats TDCurrentStats => (TDPawnStats)currentStats;
    public TDPawnActions TDaction => (TDPawnActions)action;
    
    [HideInInspector] public Rigidbody physicsbody;
    [SerializeField] public GameObject interactionPrefab;


    //=-----------------=
    // Mono Functions
    //=-----------------=
    public void Awake()
    {
        // Get references
        physicsbody = GetComponent<Rigidbody>();
        viewPoint = transform.Find("ViewPoint");

        defaultStats = TDDefaultStats;
        currentStats = (TDPawnStats)TDDefaultStats.Clone(); // Don't forget to clone so that you don't overwrite the pawns default values! ~Liz
        action = TDaction;
    }
    

    //=-----------------=
    // Internal Functions
    //=-----------------=


    //=-----------------=
    // External Functions
    //=-----------------=
}