using System;
using System.Collections;
using UnityEngine;

public class RockHead : TrapBase
{
<<<<<<< Updated upstream
    [Header("Movement")]
    public float initialSpeed = 5f;
    public float waitTimer = 2f;
    [Tooltip("1 for right, -1 for left")]
    public float direction = -1f; // 1 for right, -1 for left
=======
    bool isMoving = false;
    bool isBlinking = false;
    bool isWaiting = false;
    public float blinkRate = 2f;
>>>>>>> Stashed changes

    Animator animator;

    [Header("Rider-Handling (Parent)")]
    public string riderTag = "Player";
    [Tooltip("Kontakt-Normale muss so stark nach oben zeigen, damit 'oben drauf' gilt.")]
    public float topNormalThreshold = 0.2f;  // 0.1–0.2
    [Tooltip("Fallback: Toleranz unterhalb Oberkante der Plattform (Weltmaß).")]
    public float topHeightTolerance = 0.03f;
    public Transform riderAnchor;

<<<<<<< Updated upstream
    void Awake()
    {
        blinkRate = Random.Range(2f, 5f);
=======


    private void Awake()
    {
        animator = GetComponent<Animator>();
        // platformAudio = GetComponent<AudioSource>();

>>>>>>> Stashed changes
    }
    void Start()
    {
        StartCoroutine(BlinkLoop());
    }
    void Update()
    {
<<<<<<< Updated upstream
        StartCoroutine(Blink());
    }

    void FixedUpdate()
    {
        HandleMovement();
=======

>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Prüfen ob es eine Wand ist (Layer, Tag oder Normale der Kollision)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")
            || collision.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            // Normale checken -> nur umdrehen wenn er seitlich anstößt, nicht am Boden
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Wenn Normale fast horizontal ist, also links/rechts
                if (Mathf.Abs(contact.normal.x) > 0.9f)
                {
                    StartCoroutine(ChangeDirection());
                    break;
                }
=======
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
>>>>>>> Stashed changes
            }
            else yield return null;
        }
    }
<<<<<<< Updated upstream

    private IEnumerator ChangeDirection()
    {
        isWaiting = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(waitTimer);

        // Richtung wechseln
        direction *= -1;
        isWaiting = false;
        animator.SetTrigger("Blink");
    }

    IEnumerator Blink()
    {
        if (animator && isMoving && !isWaiting && !isBlinking)
        {
            isBlinking = true;
            yield return new WaitForSeconds(blinkRate);
            animator.SetTrigger("Blink");
            isBlinking = false;

        }
        yield return null;
    }
=======
>>>>>>> Stashed changes
}
