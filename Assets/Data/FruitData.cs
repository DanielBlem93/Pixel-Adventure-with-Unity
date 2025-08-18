using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Collectibles/Fruit Data")]
public class FruitData : ScriptableObject
{
    public string fruitName;      // z. B. "Apple"
    public Sprite[] frames;         // alle Frames deiner Frucht‑Animation
    public float frameRate = 12; // wie viele Frames pro Sekunde
}