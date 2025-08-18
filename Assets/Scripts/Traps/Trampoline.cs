using System.Collections;
using UnityEngine;

public class Trampoline : MonoBehaviour
{
    public float jumpPower = 30f;
    private Animator animator;
    private AudioSource audioSource;
    // Start is called before the first frame update
    private void Awake()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerMovement>();
            var particles = other.GetComponent<PlayerParticles>();
            StartCoroutine(TrampolineJump(player, particles));

        }

    }

    IEnumerator TrampolineJump(PlayerMovement player, PlayerParticles particles)
    {
        if (!player || !particles) yield return null;
        animator.SetTrigger("Hit");
        audioSource.Play();
        player.maxRiseSpeed = jumpPower;
        player.rb.velocity = new Vector2(player.rb.velocity.x, jumpPower);
        particles.SpawnJumpDust();
        yield return new WaitForSeconds(0.1f);
        player.maxRiseSpeed = 15f;
    }


}
