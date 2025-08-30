using System.Collections;
using UnityEngine;

public class RockHead : TrapBase
{
    [Header("Movement")]
    public float initialSpeed = 5f;
    public float waitTimer = 2f;
    [Tooltip("1 for right, -1 for left")]
    public float direction = -1f; // 1 for right, -1 for left

    private float speed;
    private bool isWaiting = false;
    private bool isMoving;
    private bool isBlinking;
    public float blinkRate;

    private Rigidbody2D rb;
    private Animator animator;

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
    }

    void FixedUpdate()
    {
        HandleMovement();
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
            }
        }
    }

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
}
