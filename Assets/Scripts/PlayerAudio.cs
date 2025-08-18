using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudio : MonoBehaviour
{

    [Header("Footstep Sounds")]
    public AudioClip leftFootstepClip;
    public AudioClip rightFootstepClip;
    [Range(0f, 1f)] public float footstepVolume = 0.5f;
    [Range(0f, 1f)] public float footstepPitch = 2f;

    [Header("Jump Sounds")]
    public AudioClip jumpClip;
    [Range(0f, 1f)] public float jumpVolume = 0.5f;
    [Range(0f, 3f)] public float jumpPitch = 3f;

    [Header("Land Sound")]
    public AudioClip jumpLandClip;
    [Range(0f, 1f)] public float jumpLandVolume = 0.5f;
    [Range(0f, 2f)] public float jumpLandPitch = 1f;

    [Header("Wall Slide")]
    public AudioClip WallSlideClip;
    public AudioSource wallSlideSource;
    [Range(0f, 1f)] public float WallSlideVolume = 0.5f;
    [Range(0f, 3f)] public float WallSlidePitch = 1f;

    [Header("Game Over")]
    public AudioClip hitSoundClip;
    public AudioSource hitSoundSource;
    [Range(0f, 1f)] public float hitSoundVolume = 0.5f;
    [Range(0f, 3f)] public float hitSoundPitch = 1f;
    public AudioClip gameOverClip;
    public AudioSource gameOverSource;
    [Range(0f, 1f)] public float gameOverVolume = 0.5f;
    [Range(0f, 3f)] public float gameOverPitch = 1f;
    private AudioSource audioSource;



    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (wallSlideSource == null)
            wallSlideSource = gameObject.AddComponent<AudioSource>();

        wallSlideSource.clip = WallSlideClip;
        wallSlideSource.loop = true;
        wallSlideSource.playOnAwake = false;
    }

    public void PlayLeftFootstep()
    {
        if (leftFootstepClip == null) return;
        audioSource.pitch = footstepPitch;
        audioSource.volume = footstepVolume;
        audioSource.PlayOneShot(leftFootstepClip);
    }

    public void PlayRightFootstep()
    {
        if (rightFootstepClip == null) return;
        audioSource.pitch = footstepPitch;
        audioSource.volume = footstepVolume;
        audioSource.PlayOneShot(rightFootstepClip);
    }

    public void PlayJumpSound()
    {
        if (jumpClip == null) return;
        audioSource.pitch = jumpPitch;
        audioSource.volume = jumpVolume;
        audioSource.PlayOneShot(jumpClip);
    }

    public void PlayJumpLandSound()
    {
        if (jumpLandClip == null) return;
        audioSource.pitch = jumpLandPitch;
        audioSource.volume = jumpLandVolume;
        audioSource.PlayOneShot(jumpLandClip);
    }

    public void PlayWallSlideSound()
    {
        if (WallSlideClip == null) return;
        audioSource.pitch = WallSlidePitch;
        audioSource.volume = WallSlideVolume;
        audioSource.PlayOneShot(WallSlideClip);
    }

    /// <summary>Starte das loopende Wall-Slide-Signal.</summary>
    public void StartWallSlideLoop()
    {
        if (WallSlideClip == null || wallSlideSource.isPlaying)
            return;

        wallSlideSource.volume = WallSlideVolume;
        wallSlideSource.pitch = WallSlidePitch;
        wallSlideSource.Play();
    }

    /// <summary>Stoppe das Wall-Slide-Signal.</summary>
    public void StopWallSlideLoop()
    {
        if (wallSlideSource.isPlaying)
            wallSlideSource.Stop();
    }
    public void PlayHitSound()
    {
        if (hitSoundClip == null) return;
        hitSoundSource.PlayOneShot(hitSoundClip);
        hitSoundSource.pitch = hitSoundPitch;
        hitSoundSource.volume = hitSoundVolume;
    }

    public void PlayGameOverSound()
    {
        if (gameOverClip == null) return;
        gameOverSource.PlayOneShot(gameOverClip);
        gameOverSource.pitch = gameOverPitch;
        gameOverSource.volume = gameOverVolume;
    }
}