using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FruitAnimator : MonoBehaviour
{
    public FruitData data;              // im Prefab-Inspector zuweisen
    private SpriteRenderer spriteRenderer;
    private int currentFrame;
    private float timer;
    public AudioClip pickupSound;
    [Range(0f, 1f)]
    public float volume = 1f;       // Sound, der beim Aufsammeln abgespielt wird

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (data == null || data.frames.Length == 0) return;

        timer += Time.deltaTime;
        float interval = 1f / data.frameRate;
        if (timer >= interval)
        {
            timer -= interval;
            currentFrame = (currentFrame + 1) % data.frames.Length;
            spriteRenderer.sprite = data.frames[currentFrame];
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayPickUpSound();
            Destroy(gameObject);
        }
    }

    void PlayPickUpSound()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position,volume);
        }
    }
}