using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Shared Menu")]
    public GameObject menuPanel;    // dein MenuPanel
    public TextMeshProUGUI headerText;         // „Game Over!“ oder „Level Complete!“

    [Header("Buttons")]
    public Button retryButton;
    public Button nextButton;
    public Button menuButton;
    private PlayerMovement playerMovement;
    public AudioClip gameOverJingleClip;
    public AudioSource gameOverJingleSource;
    [Range(0f, 1f)] public float gameOverJingleVolume = 0.5f;
    [Range(0f, 3f)] public float gameOverJinglePitch = 1f;
    public AudioClip gameWonJingleClip;
    public AudioSource gameWonJingleSource;
    [Range(0f, 1f)] public float gameWonJingleVolume = 0.5f;
    [Range(0f, 3f)] public float gameWonJinglePitch = 1f;



    void Start()
    {
        menuPanel.SetActive(false);
        IsPlayer();
    }

    void IsPlayer()
    {
        var playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
            playerMovement = playerGO.GetComponent<PlayerMovement>();
    }

    // Aufruf bei Tod
    public void ShowDeathMenu()
    {
        ShowMenu("Game Over!");
        PlayGameOverJingle();
    }

    // Aufruf bei Level-Abschluss
    public void ShowCompleteMenu()
    {
        ShowMenu("Level Complete!");
        PlayGameWonJingle();
    }

    // Gemeinsame Logik
    private void ShowMenu(string title)
    {
        Time.timeScale = 0f;
        headerText.text = title;
        DisablePlayerMovement();
        menuPanel.SetActive(true);
        bool hasNext = SceneManager.GetActiveScene().buildIndex + 1
                     < SceneManager.sceneCountInBuildSettings;
        nextButton.gameObject.SetActive(hasNext);
    }

    // Button-Callbacks (im Inspector jeweils zuweisen)
    public void OnRetryPressed()
    {
        StopGameOverJingle();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnNextPressed()
    {
        StopGameOverJingle();
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(next);
        }
    }

    public void OnMenuPressed()
    {
        StopGameOverJingle();
        Debug.Log("Back to Menu pressed — implement loading of your main menu here.");
        // z.B. SceneManager.LoadScene("MainMenu");
    }

    public void PlayGameOverJingle()
    {
        if (gameOverJingleClip == null) return;
        gameOverJingleSource.PlayOneShot(gameOverJingleClip);
        gameOverJingleSource.pitch = gameOverJinglePitch;
        gameOverJingleSource.volume = gameOverJingleVolume;
    }

    void StopGameOverJingle()
    {
        if (gameOverJingleSource.isPlaying)
        {
            gameOverJingleSource.Stop();
        }
    }

    public void PlayGameWonJingle()
    {
        if (gameWonJingleClip == null) return;
        gameWonJingleSource.PlayOneShot(gameWonJingleClip);
        gameWonJingleSource.pitch = gameWonJinglePitch;
        gameWonJingleSource.volume = gameWonJingleVolume;
    }

    void StopGameWonJingle()
    {
        if (gameWonJingleSource.isPlaying)
        {
            gameWonJingleSource.Stop();
        }
    }

    void DisablePlayerMovement()
    {
        IsPlayer();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
            playerMovement.isDead = true; // Setze den isDead-Status

        }
    }

}
