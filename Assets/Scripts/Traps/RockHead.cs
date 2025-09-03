using System;
using System.Collections;
using UnityEngine;

public class RockHead : TrapBase
{
    bool isMoving = false;
    bool isBlinking = false;
    bool isWaiting = false;
    public float blinkRate = 2f;

    Animator animator;

    [Header("Rider-Handling (Parent)")]
    public string riderTag = "Player";
    [Tooltip("Kontakt-Normale muss so stark nach oben zeigen, damit 'oben drauf' gilt.")]
    public float topNormalThreshold = 0.2f;  // 0.1–0.2
    [Tooltip("Fallback: Toleranz unterhalb Oberkante der Plattform (Weltmaß).")]
    public float topHeightTolerance = 0.03f;
    public Transform riderAnchor;



    private void Awake()
    {
        animator = GetComponent<Animator>();
        // platformAudio = GetComponent<AudioSource>();

    }
    void Start()
    {
        StartCoroutine(BlinkLoop());
    }
    void Update()
    {
    }

    void OnCollisionEnter2D(Collision2D c) { TryParent(c); }
    void OnCollisionStay2D(Collision2D c) { TryParent(c); }
    void OnCollisionExit2D(Collision2D c)
    {
        if (!c.gameObject.CompareTag(riderTag)) return;
        if (c.transform.parent == riderAnchor.transform) c.transform.SetParent(null, true);
    }

    void TryParent(Collision2D c)
    {
        if (!c.gameObject.CompareTag(riderTag)) return;
        if (!IsStandingOnTop(c)) return;

        if (c.transform.parent != riderAnchor.transform)
            c.transform.SetParent(riderAnchor.transform, /*worldPositionStays*/ true);
    }


    bool IsStandingOnTop(Collision2D col)
    {
        // 1) Kontakt-Normalen prüfen
        foreach (var ct in col.contacts)
            if (ct.normal.y > topNormalThreshold) return true;

        // 2) Fallback über Bounds (Unterkante Rider >= Oberkante Plattform - Tol.)
        var myCol = GetComponent<Collider2D>();
        if (myCol)
        {
            float platformTop = myCol.bounds.max.y - topHeightTolerance;
            if (col.collider.bounds.min.y >= platformTop) return true;
        }
        return false;
    }

    // void PlayPlatfromSound()
    // {
    //     if (platformAudio == null) platformAudio = gameObject.AddComponent<AudioSource>();
    //     platformAudio.PlayOneShot(platformClip);
    // }

    // void StopPlatformSound()
    // {
    //     if (platformAudio.isPlaying)
    //     {
    //         platformAudio.Stop();
    //     }
    // }


    IEnumerator BlinkLoop()
    {
        while (true)
        {
            if (animator && isMoving && !isWaiting && !isBlinking)
            {
                isBlinking = true;
                yield return new WaitForSeconds(blinkRate);
                animator.SetTrigger("Blink");
                isBlinking = false;
            }
            else yield return null;
        }
    }
}
