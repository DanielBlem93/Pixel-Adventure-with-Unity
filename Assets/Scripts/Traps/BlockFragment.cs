using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BlockFragment : MonoBehaviour
{
    private float lifetime = 3f;            // Gesamtlebensdauer
    private float blinkStartTime;      // Wann vor Ablauf soll das Blinken starten?
    public float blinkInterval = 0.1f;     // Blinkgeschwindigkeit
    private SpriteRenderer sr;

    void Start()
    {
        blinkStartTime = UnityEngine.Random.Range(2f, 3f);
        lifetime = UnityEngine.Random.Range(3f, 4f);
        sr = GetComponent<SpriteRenderer>();
        StartCoroutine(LifeCycle());
    }

    private IEnumerator LifeCycle()
    {
        // 1) Warten, bis Blinkphase beginnt
        yield return new WaitForSeconds(lifetime - blinkStartTime);

        // 2) Blinkphase
        float blinkTimer = 0f;
        bool visible = true;
        while (blinkTimer < blinkStartTime)
        {
            visible = !visible;
            sr.enabled = visible;

            yield return new WaitForSeconds(blinkInterval);
            blinkTimer += blinkInterval;
        }

        // 3) Objekt zerstören
        Destroy(gameObject);
    }
}
