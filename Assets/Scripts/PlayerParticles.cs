using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    [Header("Dust Effect")]
    public ParticleSystem dustPS;
    [Tooltip("Offset, wenn der Player nach rechts blickt")]
    public Vector2 dustOffsetLeft = new Vector2(-0.3f, -0.5f);
    [Tooltip("Offset, wenn der Player nach links blickt")]
    public Vector2 dustOffsetRight = new Vector2(0.3f, -0.5f);
    public ParticleSystem jumpPS;
    public ParticleSystem LandingRightPS;
    public ParticleSystem LandingLeftPS;


    void Awake()
    {
        // Stelle sicher, dass das System nicht schon mitläuft
        if (dustPS != null)
        {
            dustPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        if (jumpPS != null)
        {
            jumpPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        if (LandingRightPS != null)
        {
            LandingRightPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        if (LandingLeftPS != null)
        {
            LandingLeftPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
    public void SpawnFootDust()
    {
        if (dustPS == null) return;

        // 1) Position relativ zum Player
        Vector2 offset = transform.localScale.x > 0
            ? dustOffsetLeft
            : dustOffsetRight;
        dustPS.transform.localPosition = offset;

        // 2) Rotation: +X wenn Player nach rechts, 180° um Z wenn nach links
        if (transform.localScale.x > 0)
            dustPS.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        else
            dustPS.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);

        // 3) Starte den Burst
        var em = dustPS.emission;
        em.enabled = true;
        dustPS.Play();
    }
    public void SpawnJumpDust()
    {
        if (jumpPS == null) return;
        var em = dustPS.emission;
        em.enabled = true;
        jumpPS.Play();
    }

    public void SpawnLandingDust()
    {
        if (LandingRightPS == null && LandingLeftPS == null) return;

        var em = LandingRightPS.emission;
        var ems = LandingLeftPS.emission;
        ems.enabled = true;
        em.enabled = true;
        LandingRightPS.Play();
        LandingLeftPS.Play();
    }

public void ThrowConfetti()
    {
   
        Debug.Log("Konfetti wird geworfen!");
    }

}
