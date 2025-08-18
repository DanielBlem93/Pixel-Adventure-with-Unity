using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class TrapBase : MonoBehaviour
{
    public PlayerMovement playerMovement; // Referenz auf den Spieler

    void Awake()
    {
     
    }
    void Start()
    {

    }


    void Update()
    {

    }


    public void KillPlayer()
    {
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement == null) return;
        playerMovement.Die();
    }

}
