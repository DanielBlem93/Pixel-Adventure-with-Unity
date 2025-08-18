using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpArrowController : TrapBase
{
    public float jumpArrowDeathTime = 0.2f; // Zeit bis die Pfeilfalle zerstört wird
    private Animator animator;
    private bool active = true;
    public float forceMultiplier = 1.2f; // Multiplikator für die Sprungkraft
    public AudioClip jumpArrowSoundClip;
    public AudioSource jumpArrowSoundSource;

    [Range(0f, 1f)] public float jumpArrowVolume = 0.5f;
    [Range(0f, 3f)] public float jumpArrowPitch = 1f;
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.collider.CompareTag("Player") || !active) return;
        var player = other.collider.GetComponent<PlayerMovement>();
        if (player == null) return;
        PerformArrowJump(player);
    }

    void PerformArrowJump(PlayerMovement player)
    {
        Vector2 dir = transform.up.normalized;
        float strength = player.jumpForce * forceMultiplier;  // oder deinen gewünschten Faktor
        Vector2 impulse = dir * strength;
        
        player.JumpWithLock(impulse);
        PerformJumpEffects(player);
    }

    void PerformJumpEffects(PlayerMovement player)
    {
        PlayJumpArrowSound();
        ResetDoubleJump(player);
        animator.SetTrigger("Hit");
    }
    void PerformArrowJumpUp(PlayerMovement player)
    {
        player.PerformJump();
    }
    void ResetDoubleJump(PlayerMovement player)
    {
        player.jumpCount = 1;
        player.animator.ResetTrigger("DoubleJump");
        player.hasJumped = true;
        this.active = false;
    }

    public IEnumerator DestroyArrow()
    {
        yield return new WaitForSeconds(jumpArrowDeathTime);
        Destroy(gameObject);
    }

    public void PlayJumpArrowSound()
    {
        if (jumpArrowSoundClip == null) return;
        jumpArrowSoundSource.pitch = jumpArrowPitch;
        jumpArrowSoundSource.volume = jumpArrowVolume;
        jumpArrowSoundSource.PlayOneShot(jumpArrowSoundClip);
    }

}

