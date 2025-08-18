using UnityEngine;
using UnityEngine.U2D; // für PixelPerfectCamera (optional)

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Ziel")]
    public Transform target;           // in der Regel: Player-Instanz aus der Szene
    public Rigidbody2D targetRb;       // optional; wird auto-gefüllt, wenn leer
    public bool autoFindTarget = true; // findet zur Laufzeit ein Objekt mit targetTag
    public string targetTag = "Player";

    [Header("Weiches Folgen")]
    public float smoothTime = 0.15f;

    [Header("Dead-Zone (Welt)")]
    public bool useDeadZone = true;
    public Vector2 deadZoneSize = new Vector2(3.5f, 2.0f);

    [Header("Look-Ahead")]
    public bool useLookAhead = true;
    public Vector2 maxLookAhead = new Vector2(2f, 0.75f);
    public float lookAheadSmoothing = 6f;
    public float lookAheadVelocityToOffset = 0.12f;

    [Header("Level-Grenzen (optional)")]
    public Collider2D boundsCollider;

    [Header("Follow-Steuerung")]
    public bool stopOnDeath = true;
    bool frozen = false;

    Camera cam;
    PixelPerfectCamera ppc; // optional
    Vector3 camVel;
    Vector2 currentLookAhead;
    Vector3 lastTargetPos;
    float refindTimer;

    void Awake()
    {
        cam = GetComponent<Camera>();
        ppc = GetComponent<PixelPerfectCamera>();
        TryResolveTarget(force: true);

        // z-Sicherheit (2D): Kamera meist bei z = -10
        if (Mathf.Abs(transform.position.z) < 0.001f)
            transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }

    void LateUpdate()
    {
        if (frozen) return;
        // Ziel ggf. nachladen (falls Prefab gesetzt war oder Player respawned)
        if (!HasSceneTarget())
        {
            TryResolveTarget();
            if (!HasSceneTarget()) return; // noch kein Ziel → nichts tun
        }

        // --- LookAhead ---
        Vector2 vel = targetRb ? targetRb.velocity
                               : (Vector2)((target.position - lastTargetPos) / Mathf.Max(Time.deltaTime, 1e-5f));

        Vector2 desiredLook = useLookAhead
            ? new Vector2(
                Mathf.Clamp(vel.x * lookAheadVelocityToOffset, -maxLookAhead.x, maxLookAhead.x),
                Mathf.Clamp(vel.y * lookAheadVelocityToOffset, -maxLookAhead.y, maxLookAhead.y)
              )
            : Vector2.zero;

        currentLookAhead = Vector2.Lerp(currentLookAhead, desiredLook, Time.deltaTime * lookAheadSmoothing);

        // Zielpunkt (Player + LookAhead)
        Vector3 focal = (Vector3)((Vector2)target.position + currentLookAhead);

        // --- Dead-Zone ---
        Vector3 camPos = transform.position;
        Vector3 camCenter = new Vector3(camPos.x, camPos.y, 0f);
        Vector3 desiredCenter = camCenter;

        if (useDeadZone)
        {
            Vector2 half = deadZoneSize * 0.5f;
            float minX = camCenter.x - half.x, maxX = camCenter.x + half.x;
            float minY = camCenter.y - half.y, maxY = camCenter.y + half.y;

            float dx = 0f, dy = 0f;
            if (focal.x < minX) dx = focal.x - minX; else if (focal.x > maxX) dx = focal.x - maxX;
            if (focal.y < minY) dy = focal.y - minY; else if (focal.y > maxY) dy = focal.y - maxY;

            desiredCenter = camCenter + new Vector3(dx, dy, 0f);
        }
        else
        {
            desiredCenter = focal;
        }

        // --- weich bewegen ---
        Vector3 smoothTarget = Vector3.SmoothDamp(camCenter, desiredCenter, ref camVel, smoothTime);
        smoothTarget.z = camPos.z;

        // --- Pixel Perfect quantisieren (falls Komponente vorhanden) ---
        if (ppc != null)
        {
            float unitsPerPixel = 1f / Mathf.Max(1, ppc.assetsPPU);
            smoothTarget.x = Mathf.Round(smoothTarget.x / unitsPerPixel) * unitsPerPixel;
            smoothTarget.y = Mathf.Round(smoothTarget.y / unitsPerPixel) * unitsPerPixel;
        }

        // --- Bounds clampen ---
        if (boundsCollider)
        {
            Bounds b = boundsCollider.bounds;
            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            float minX = b.min.x + halfW, maxX = b.max.x - halfW;
            float minY = b.min.y + halfH, maxY = b.max.y - halfH;

            smoothTarget.x = (minX > maxX) ? b.center.x : Mathf.Clamp(smoothTarget.x, minX, maxX);
            smoothTarget.y = (minY > maxY) ? b.center.y : Mathf.Clamp(smoothTarget.y, minY, maxY);
        }

        transform.position = smoothTarget;
        lastTargetPos = target.position;
    }

    public void StopFollowing() { if (stopOnDeath) frozen = true; }
    public void ResumeFollowing(Transform newTarget = null, Rigidbody2D newRb = null)
    {
        if (newTarget) target = newTarget;
        if (newRb) targetRb = newRb;
        frozen = false;
        // Optional: LookAhead zurücksetzen
        currentLookAhead = Vector2.zero;
        camVel = Vector3.zero;
        if (target) lastTargetPos = target.position;
    }

    // ---------- Helpers ----------
    bool HasSceneTarget()
    {
        return target != null && target.gameObject.scene.IsValid();
    }

    void TryResolveTarget(bool force = false)
    {
        if (!autoFindTarget && !force) return;

        // Nur alle 0.5s suchen, außer force
        if (!force)
        {
            refindTimer -= Time.deltaTime;
            if (refindTimer > 0f) return;
        }
        refindTimer = 0.5f;

        if (!HasSceneTarget())
        {
            var go = GameObject.FindGameObjectWithTag(targetTag);
            if (go != null)
            {
                target = go.transform;
                targetRb = targetRb ? targetRb : go.GetComponent<Rigidbody2D>();
                lastTargetPos = target.position;
            }
        }
        else if (!targetRb)
        {
            targetRb = target.GetComponent<Rigidbody2D>();
        }
    }
}
