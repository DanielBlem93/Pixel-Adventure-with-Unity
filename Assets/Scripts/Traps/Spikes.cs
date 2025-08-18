using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : TrapBase
{

  

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.collider.CompareTag("Player")) return;
        KillPlayer();
    }
}

