using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Pfad")]
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;
    public float waitAtEnds = 2f;


    Animator animator;

    [Header("Rider-Handling (Parent)")]
    public string riderTag = "Player";
    [Tooltip("Kontakt-Normale muss so stark nach oben zeigen, damit 'oben drauf' gilt.")]
    public float topNormalThreshold = 0.2f;  // 0.1–0.2
    [Tooltip("Fallback: Toleranz unterhalb Oberkante der Plattform (Weltmaß).")]
    public float topHeightTolerance = 0.03f;

    Vector3 nextPosition;
    bool goingToB;     // wohin aktuell?
    float waitTimer;

    public AudioClip platformClip;
    public AudioSource platformAudio;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        platformAudio = GetComponent<AudioSource>();

    }
    void Start()
    {
        waitTimer = waitAtEnds;
        transform.position = pointA.position;
        nextPosition = pointB.position;
        UpdateAnimatorDirection();

    }

    void Update()
    {
        MovePlatform();
        ChangeDirectrion();
    }



    void MovePlatform()
    {
        if (animator)
        {
            animator.SetBool("IsMoving", true);
            if (!platformAudio.isPlaying) PlayPlatfromSound();
        }
        transform.position = Vector3.MoveTowards(transform.position, nextPosition, speed * Time.deltaTime);
    }
    void ChangeDirectrion()
    {
        if (transform.position == nextPosition)
        {
            animator.SetBool("IsMoving", false);
            StopPlatformSound();
            if (waitTimer > 0f) { waitTimer -= Time.deltaTime; return; }
            if (waitAtEnds > 0f) waitTimer = waitAtEnds;
            goingToB = !goingToB;
            nextPosition = goingToB ? pointB.position : pointA.position;
            UpdateAnimatorDirection();
        }
    }


    void UpdateAnimatorDirection()
    {
        if (!animator) return;
        animator.SetBool("forwards", goingToB);
        animator.SetBool("backwards", !goingToB);
        animator.SetBool("IsMoving", true);
    }

    // — Parent nur, wenn der Spieler OBEN drauf steht —
    void OnCollisionEnter2D(Collision2D c) { TryParent(c); }
    void OnCollisionStay2D(Collision2D c) { TryParent(c); }
    void OnCollisionExit2D(Collision2D c)
    {
        if (!c.gameObject.CompareTag(riderTag)) return;
        if (c.transform.parent == transform) c.transform.SetParent(null, true);
    }

    void TryParent(Collision2D c)
    {
        if (!c.gameObject.CompareTag(riderTag)) return;
        if (!IsStandingOnTop(c)) return;

        if (c.transform.parent != transform)
            c.transform.SetParent(transform, /*worldPositionStays*/ true);
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

    void PlayPlatfromSound()
    {
        if (platformAudio == null) platformAudio = gameObject.AddComponent<AudioSource>();
        platformAudio.PlayOneShot(platformClip);
    }

    void StopPlatformSound()
    {
        if (platformAudio.isPlaying)
        {
            platformAudio.Stop();
        }
    }
}
