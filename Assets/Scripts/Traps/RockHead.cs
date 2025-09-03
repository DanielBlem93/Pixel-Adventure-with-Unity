using System.Collections;
using UnityEngine;

public class RockHead : TrapBase
{
    [Header("Movement")]
    public float initialSpeed = 5f;
    public float waitTimer = 2f;
    [Tooltip("1 for right, -1 for left")]
    public float direction = -1f; // 1 = rechts, -1 = links

    private float speed;
    private bool isWaiting = false;
    private bool isMoving;
    private bool isBlinking;
    public float blinkRate;

    private Rigidbody2D rb;
    private Animator animator;

    [Header("Player Riding")]
    public Transform riderAnchor; // Hier wird der Spieler als Child angehängt

    void Awake()
    {
        blinkRate = Random.Range(2f, 5f);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        speed = initialSpeed;

        // Sicherheit: keine Gravitation und Rotation
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }

    void Update()
    {
        StartCoroutine(Blink());
        HandleMovement();
    }

    void FixedUpdate()
    {

    }

    void HandleMovement()
    {
        if (!isWaiting)
        {
            rb.velocity = new Vector2(direction * speed, 0f);
            isMoving = true;
        }
        else
        {
            rb.velocity = Vector2.zero;
            isMoving = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ChangeRockDir(collision);
        TryParent(collision);
    }
    void OnCollisionStay2D(Collision2D collision) { TryParent(collision); }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null); // Spieler wieder lösen
        }
    }

    void ChangeRockDir(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")
            || collision.gameObject.layer == LayerMask.NameToLayer("Platform"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.9f) // Seitenkontakt
                {
                    StartCoroutine(ChangeDirection());
                    break;
                }
            }
        }
    }

    private IEnumerator ChangeDirection()
    {
        isWaiting = true;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(waitTimer);

        direction *= -1; // Richtung wechseln
        isWaiting = false;
        animator.SetTrigger("Blink");
    }

    void TryParent(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Spieler berührt den RockHead von oben
                if (contact.normal.y < -0.2f)
                {
                    collision.transform.SetParent(transform, true);
                    break;
                }
            }
        }
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
}
