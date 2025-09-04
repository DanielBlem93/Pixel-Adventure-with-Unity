using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockHeadCollisions : TrapBase
{
    [Tooltip("Kontakt-Normale muss so stark nach oben zeigen, damit 'oben drauf' gilt.")]
    public float topNormalThreshold = 0.2f;  // 0.1–0.2
    [Tooltip("Fallback: Toleranz unterhalb Oberkante der Plattform (Weltmaß).")]
    public float topHeightTolerance = 0.03f;
    public Transform riderAnchor;
    public string riderTag = "Player";
    public PolygonCollider2D hurtBox;
    LayerMask GroundLayer;


    private void Awake()
    {
        GroundLayer = LayerMask.GetMask("Ground");
    }
    void OnCollisionEnter2D(Collision2D c) { TryParent(c); PlayerWallHit(c); }
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

    void PlayerWallHit(Collision2D c)
    {
        if (!c.gameObject.CompareTag(riderTag)) return;
        Debug.Log("Player Detected");
        if (c.gameObject.CompareTag(riderTag) && c.gameObject.layer == GroundLayer && hurtBox.IsTouching(c.collider))
        {
            Debug.Log("Player hit wall");
            KillPlayer();
        }
        ;
        if (c.gameObject.layer == GroundLayer)
        {
            Debug.Log("Wall detected");
        }
    }
}

