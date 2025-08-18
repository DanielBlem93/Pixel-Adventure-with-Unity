using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class FanController : MonoBehaviour
{
    [Header("Richtung & Stärke")]
    public bool useTransformUp = true;
    public Vector2 customDirection = Vector2.up;
    [Tooltip("m/s² – so stark beschleunigt der Wind (mass-unabhängig).")]
    public float acceleration = 22f;       // 20–25 ist ein guter Start für Aufwind
    public float maxAlongSpeed = 8f;       // 0 = kein Clamping

    [Header("Betroffene Layer")]
    public LayerMask affectedLayers;       // z.B. nur Player

    [Header("Schweben (optional)")]
    public bool reduceGravityInside = true;
    [Range(0f, 1f)] public float gravityScaleMultiplier = 0.35f;

    [Header("Zyklus AN/AUS")]
    public float onDuration = 5f;
    public float offDuration = 5f;
    public bool startOn = true;

    [Header("VFX / Animation (optional)")]
    public ParticleSystem[] particles;
    public Animator animator;
    public string animatorIsOnParam = "IsOn";
    AudioSource audioSrc;
    public AudioClip FanSfx;


    // intern
    Collider2D triggerCol;
    bool isOn;
    Vector2 dir = Vector2.up;
    readonly HashSet<Rigidbody2D> bodiesInside = new();
    private Dictionary<Rigidbody2D, float> originalGravity = new();
    public float playerGravity = 3f;

    void Awake()
    {
        audioSrc = GetComponent<AudioSource>();
        triggerCol = GetComponent<Collider2D>();
        triggerCol.isTrigger = true;
        UpdateDirection();
    }

    void OnEnable()
    {
        SetOnImmediate(startOn);
        StartCoroutine(FanLoop());
    }

    void UpdateDirection()
    {
        dir = useTransformUp ? (Vector2)transform.up : customDirection.normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector2.up;
    }

    IEnumerator FanLoop()
    {
        while (true)
        {
            if (onDuration > 0f) { SetOn(true); yield return new WaitForSeconds(onDuration); }
            if (offDuration > 0f) { SetOn(false); yield return new WaitForSeconds(offDuration); }
            if (onDuration <= 0f && offDuration <= 0f) yield return null;
        }
    }

    void SetOnImmediate(bool value)
    {
        isOn = value;
        SyncVFX();
        PlayFanAudio();
    }

    void SetOn(bool value)
    {
        if (isOn == value) return;
        if (value)
            PlayFanAudio();
        else
            audioSrc?.Stop();
        isOn = value;
        SyncVFX();

        // Gravitation für alle, die gerade drin sind, an/aus
        foreach (var rb in bodiesInside)
        {
            if (rb == null) continue;
            if (reduceGravityInside)
            {
                if (isOn) ApplyReducedGravity(rb);
                else RestoreGravity(rb);
            }
        }
    }

    void SyncVFX()
    {
        if (particles != null)
            foreach (var ps in particles) if (ps) { if (isOn) ps.Play(true); else ps.Stop(true, ParticleSystemStopBehavior.StopEmitting); }
        if (animator) animator.SetBool(animatorIsOnParam, isOn);
    }

    void FixedUpdate()
    {
        if (useTransformUp) UpdateDirection();
        if (!isOn) return;

        foreach (var rb in bodiesInside)
        {
            if (rb == null) continue;

            // Beschleunigung mass-unabhängig: F = m * a
            rb.AddForce(dir * acceleration * rb.mass, ForceMode2D.Force);

            if (maxAlongSpeed > 0f)
            {
                Vector2 v = rb.velocity;
                float along = Vector2.Dot(v, dir);
                if (along > maxAlongSpeed)
                {
                    Vector2 perp = v - along * dir;
                    rb.velocity = perp + dir * maxAlongSpeed;
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) { TryAdd(other); }
    void OnTriggerStay2D(Collider2D other) { TryAdd(other); } // wichtig: fängt „startet im Trigger“ ab
    void OnTriggerExit2D(Collider2D other) { TryRemove(other); }

    void TryAdd(Collider2D other)
    {
        var rb = other.attachedRigidbody;
        if (rb == null) return;
        if (!IsAffected(rb.gameObject)) return;

        if (bodiesInside.Add(rb) && reduceGravityInside && isOn)
            ApplyReducedGravity(rb);
    }

    void TryRemove(Collider2D other)
    {
        var rb = other.attachedRigidbody;
        if (rb == null) return;
        if (bodiesInside.Remove(rb) && reduceGravityInside)
            RestoreGravity(rb);
        ResetPlayerDoubleJump(rb.GetComponent<PlayerMovement>());
    }

    void ResetPlayerDoubleJump(PlayerMovement player)
    {
        if (!player) return;
        player.jumpCount = 1;
        player.animator.ResetTrigger("DoubleJump");
        player.hasJumped = true;
    }

    bool IsAffected(GameObject go) => (affectedLayers.value & (1 << go.layer)) != 0;

    void ApplyReducedGravity(Rigidbody2D rb)
    {
        if (!originalGravity.ContainsKey(rb))
            originalGravity[rb] = rb.gravityScale;
        rb.gravityScale = originalGravity[rb] * gravityScaleMultiplier;
    }

    void RestoreGravity(Rigidbody2D rb)
    {
        if (originalGravity.TryGetValue(rb, out var g))
        {
            rb.gravityScale = g;
            originalGravity.Remove(rb);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        UpdateDirection();
        Gizmos.color = Color.cyan;
        var p = transform.position;
        Gizmos.DrawLine(p, p + (Vector3)dir * 1.5f);
        Gizmos.DrawSphere(p + (Vector3)dir * 1.5f, 0.06f);
    }
#endif

    void PlayFanAudio()
    {
        if (audioSrc == null) audioSrc = gameObject.AddComponent<AudioSource>();
        audioSrc.PlayOneShot(FanSfx);
    }
}
