using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LvlEndpoint : MonoBehaviour

{
    private Collider2D col;
    public ParticleSystem confettiPS; // Optional: Partikeleffekt für Levelabschluss
    public AudioClip levelCompleteSound; // Optional: Soundeffekt für Levelabschluss
    public AudioSource audioSource; // Optional: AudioSource für Soundeffekte
    private UIManager uiManager;
    void Awake()
    {
        if (confettiPS == null)
            confettiPS = GetComponent<ParticleSystem>();
        col = GetComponent<Collider2D>();
        col.isTrigger = true;  // Stelle sicher, dass es als Trigger eingestellt ist
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        col.enabled = false;
        StartCoroutine(LevelComplete());
    }

    IEnumerator LevelComplete()
    {
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
        PlayKonfetti();
        yield return new WaitForSeconds(1f);

        uiManager.ShowCompleteMenu();
    }

    void PlayKonfetti()
    {
        confettiPS?.Play();
        if (audioSource != null && levelCompleteSound != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
        }
    }
}
