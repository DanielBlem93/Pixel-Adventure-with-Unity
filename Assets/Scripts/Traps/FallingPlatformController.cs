using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FallingPlatformController : TrapBase
{
    [Header("Hover")]
    public float hoverAmplitude = 0.2f;
    public float hoverFrequency = 1f;

    [Header("Fall Settings")]
    public float fallDelay = 0.1f;            // Zeit bis wirklich fallen
    public float destroyAfter = 5f;           // Cleanup nach dem Fallen

    [Header("Effects")]
    public ParticleSystem hoverParticles;     // z.B. Propeller-Particles


    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Vector3 startLocalPos;
    private bool isFalling = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        startLocalPos = transform.localPosition;

        if (hoverParticles != null) hoverParticles.Play();
        audioSource.loop = true;
        audioSource.Play();
    }

    void Update()
    {
        // Nur im Schwebe-Zustand auf/ab bewegen
        if (!isFalling)
        {
            float y = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            transform.localPosition = startLocalPos + Vector3.up * y;
        }
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        if (isFalling) return;
        if (!col.collider.CompareTag("Player")) return;

        // Wir wollen nur echte Top-Hits (von oben auf die Plattform)
        foreach (var cp in col.contacts)
        {
            // cp.normal.y ist bei Top-Aufprall ≈ -1
            if (cp.normal.y < -0.5f)
            {
                StartCoroutine(TriggerFall());
                break;
            }
        }
    }

    private IEnumerator TriggerFall()
    {
        isFalling = true;

        if (hoverParticles != null) hoverParticles.Stop();
        audioSource.Stop();

        yield return new WaitForSeconds(fallDelay);


        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 2f;


        Destroy(gameObject, destroyAfter);
    }
}
