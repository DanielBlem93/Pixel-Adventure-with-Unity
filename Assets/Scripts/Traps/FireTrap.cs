using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class FireTrap : TrapBase
{
    [Header("Refs (auf diesem GO)")]
    public PolygonCollider2D hitCollider;       // immer an (IsTrigger)
    public CapsuleCollider2D flameCollider; // Start: disabled (IsTrigger)

    [Header("Animator Params")]
    public string hitTrigger = "Hit";
    public string fireBool = "Fire";

    [Header("Ziel / Schaden")]
    public string playerTag = "Player";
    public LayerMask targetLayers;          // optional
    public float FireTime = 5f;

    [Header("Cooldown")]
    public bool armedOnStart = true;
    public float cooldown = 1f;
    public float topMargin = 0.1f;
    public float minDownwardSpeed = 0.05f;
    public AudioClip fireSfx;
    public AudioClip klick;
    public AudioSource FireaudioSource;
    public AudioSource ClickaudioSource;
    Animator anim;
    bool armed;
    bool fireActive;
    [Header("Auto Fire Mode")]
    public bool AutoFireMode = false;
    [Tooltip("AutoFireInterval must be greater than FireTime!")]
    public float AutoFireInterval = 5f;


    void Awake()
    {
        anim = GetComponent<Animator>();
        armed = armedOnStart;

        if (!hitCollider) hitCollider = GetComponent<PolygonCollider2D>();
        if (!flameCollider) flameCollider = GetComponent<CapsuleCollider2D>();

        // Sicherheit
        if (hitCollider) hitCollider.isTrigger = true;
        if (flameCollider) { flameCollider.isTrigger = true; flameCollider.enabled = false; }
        anim.SetBool("off", true);
    }

    void Start()
    {
        StartAutoFireMode();
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        var trapHit = armed && !fireActive && hitCollider && hitCollider.IsTouching(other) && IsFromAbove(other);
        var fireHit = fireActive && flameCollider && flameCollider.enabled && flameCollider.IsTouching(other);
        if (!IsPlayer(other)) return;

        if (trapHit)
        {
            OnTrapHit();
            return;
        }
        if (fireHit)
        {
            ApplyDamage(other);
        }
    }

    void OnTrapHit()
    {
        if (AutoFireMode) return;
        armed = false;
        anim.SetTrigger(hitTrigger);
        PlayClickAudio();
    }

    bool IsFromAbove(Collider2D other)
    {
        // Spieler-Unterkante vs. Hit-Top
        var hb = hitCollider.bounds;
        var ob = other.bounds;
        float trapTop = hb.max.y - topMargin;
        float playerBottom = ob.min.y;
        bool isAbove = playerBottom >= trapTop;
        bool movingDown = true;
        var rb = other.attachedRigidbody;
        if (rb != null) movingDown = rb.velocity.y <= -minDownwardSpeed;
        return isAbove && movingDown;
    }

    // Falls die Flamme aktiv wird, während der Spieler schon drin steht
    void OnTriggerStay2D(Collider2D other)
    {
        if (!IsPlayer(other)) return;
        if (fireActive && flameCollider && flameCollider.enabled && flameCollider.IsTouching(other))
            ApplyDamage(other);
    }

    bool IsPlayer(Collider2D other)
    {
        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag)) return true;
        if (targetLayers.value != 0 && ((1 << other.gameObject.layer) & targetLayers.value) != 0) return true;
        return false;
    }

    void ApplyDamage(Collider2D other)
    {
        KillPlayer();
    }

    public void EVT_StartFire()
    {
        fireActive = true;
        if (flameCollider) flameCollider.enabled = true;
        anim.SetBool(fireBool, true);
        StartCoroutine(StopFireAfterDelay());
    }

    public void EVT_EndFire()
    {
        fireActive = false;
        anim.SetBool("Fire", false);
        anim.SetBool("off", true);
        if (flameCollider) flameCollider.enabled = false;
        anim.SetBool(fireBool, false);
        StopFireAudio();
        StartCoroutine(RearmAfterCooldown());
    }

    IEnumerator StopFireAfterDelay()
    {
        yield return new WaitForSeconds(FireTime);
        EVT_EndFire();

    }

    IEnumerator RearmAfterCooldown()
    {

        if (cooldown > 0f) yield return new WaitForSeconds(cooldown);
        armed = true;
    }

    public void PlayFireAudio()
    {
        if (FireaudioSource && fireSfx)
        {
            FireaudioSource.clip = fireSfx;
            FireaudioSource.Play();
        }
    }

    void StopFireAudio()
    {
        if (FireaudioSource && FireaudioSource.isPlaying)
        {
            FireaudioSource.Stop();
        }
    }

    void PlayClickAudio()
    {
        if (FireaudioSource && klick)
        {
            FireaudioSource.clip = klick;
            FireaudioSource.Play();
        }
    }

    void StopClickAudio()
    {
        if (FireaudioSource && FireaudioSource.isPlaying)
        {
            FireaudioSource.Stop();
        }
    }

    void StartAutoFireMode()
    {
        if (AutoFireMode)
        {
            StartCoroutine(AutoFireModeOn());
        }
    }

    //  !!!!!Fire Time cant be greater then AutoFireInterval !!!!!!!
    IEnumerator AutoFireModeOn()
    {
        while (AutoFireMode)
        {
            if (!armed) yield return null; // Warten, bis die Falle wieder scharf ist
            if (fireActive) yield return null;
            yield return new WaitForSeconds(cooldown);
            anim.SetTrigger("Hit");
            yield return new WaitForSeconds(FireTime);
            EVT_EndFire();
        }

    }
}
