using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    static ScreenShake _instance;
    Vector3 originalLocalPos;

    void Awake()
    {
        _instance = this;
    }

    public static void TryShake(float duration, float magnitude)
    {
        if (_instance != null) _instance.StartCoroutine(_instance.Shake(duration, magnitude));
    }

    IEnumerator Shake(float duration, float magnitude)
    {
        var currentPosition = transform.localPosition;
        float t = 0f;
        while (t < duration)
        {
            transform.localPosition = currentPosition + (Vector3)Random.insideUnitCircle * magnitude;
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localPosition = currentPosition;
    }
}
