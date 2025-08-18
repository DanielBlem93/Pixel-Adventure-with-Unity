using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class BlockController : TrapBase
{
    [Header("Fragments")]
    public GameObject topFragmentPrefab;
    public GameObject bottomFragmentPrefab;
    public float fragmentSpeedX = 3f;
    public Vector2 fragmentUpSpeedRange = new Vector2(2f, 4f);
    public float fragmentTorque = 180f;
    public float fragmentLifetime = 5000.0f;   // nach dem Blinken zerstören

    [Header("FX")]
    public AudioClip breakSfx;
    public float shakeDuration = 0.12f;
    public float shakeMagnitude = 0.06f;

    private bool broken = false;
    private Collider2D col;
    private AudioSource audioSource;
    CompositeCollider2D compCol;
    SpriteRenderer sr; // optional, falls am Parent ein Sprite hängt
    AudioSource audioSrc;


    void Awake()
    {
        compCol = GetComponent<CompositeCollider2D>();
        audioSrc = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>(); // optional
        // Parent-Rigidbody2D bleibt STATIC – das ist okay für Wall-Slide
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (broken) return;
        if (!c.collider.CompareTag("Player")) return;
        foreach (var cp in c.contacts)
        {

            if (Mathf.Abs(cp.normal.y) > 0.5f)
            {
                if (TryGetComponent(out Animator anim))
                {
                    anim.SetTrigger("Hit");
                    ScreenShake.TryShake(shakeDuration, shakeMagnitude);
                }
                break;
            }
        }
    }

    void Break()
    {
        if (broken) return;
        broken = true;
        if (breakSfx != null)
        {
            if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
            audioSrc.PlayOneShot(breakSfx);
        }
        compCol.enabled = false;
        if (sr) sr.enabled = false;
        SpawnFragments();
        Destroy(gameObject, 0.2f);
    }

    void SpawnFragments()
    {
        Vector3 pos = transform.position;

        var left = Instantiate(topFragmentPrefab, pos + new Vector3(-0.1f, 0f, 0f), Quaternion.identity);
        var right = Instantiate(bottomFragmentPrefab, pos + new Vector3(0.1f, 0f, 0f), Quaternion.identity);
        Kick(left, -1);
        Kick(right, +1);
    }

    void Kick(GameObject frag, int dir)
    {
        if (frag.TryGetComponent(out Rigidbody2D rb))
        {
            float up = Random.Range(fragmentUpSpeedRange.x, fragmentUpSpeedRange.y);
            rb.velocity = new Vector2(dir * fragmentSpeedX, up);
            rb.angularVelocity = dir * fragmentTorque;
        }
    }


}
