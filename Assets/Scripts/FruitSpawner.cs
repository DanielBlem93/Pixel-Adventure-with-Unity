using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public GameObject fruitPrefab;      // dein generisches Fruit‑Prefab
    public FruitData[] allFruits;       // im Inspector deine FruitData SOs

    public void SpawnRandomFruit(Vector3 pos)
    {
        var go = Instantiate(fruitPrefab, pos, Quaternion.identity);
        var anim = go.GetComponent<FruitAnimator>();
        anim.data = allFruits[Random.Range(0, allFruits.Length)];
    }
}