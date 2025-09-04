using System;
using System.Collections;
using UnityEngine;

public class RockHead : TrapBase
{
    bool isBlinking = false;
    public float blinkRate = 2f;

    Animator animator;







    private void Awake()
    {
        animator = GetComponent<Animator>();
        // platformAudio = GetComponent<AudioSource>();

    }
    void Start()
    {
        StartCoroutine(BlinkLoop());
    }
    void Update()
    {
    }



    // void PlayPlatfromSound()
    // {
    //     if (platformAudio == null) platformAudio = gameObject.AddComponent<AudioSource>();
    //     platformAudio.PlayOneShot(platformClip);
    // }

    // void StopPlatformSound()
    // {
    //     if (platformAudio.isPlaying)
    //     {
    //         platformAudio.Stop();
    //     }
    // }


    IEnumerator BlinkLoop()
    {
        while (true)
        {
            if (animator && !isBlinking)
            {
                isBlinking = true;
                yield return new WaitForSeconds(blinkRate);
                animator.SetTrigger("Blink");
                isBlinking = false;
            }
            else yield return null;
        }
    }

    public void OnWallHit(string side)
    {
        if (animator)
        {
            animator.SetTrigger(side);
        }
    }
}
