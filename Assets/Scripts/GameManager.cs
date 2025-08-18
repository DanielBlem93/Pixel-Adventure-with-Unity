using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int score = 0;
    public int lives = 3;

    void Awake()
    {
        // Singleton-Pattern: Nur ein GameManager im Spiel
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Bleibt beim Szenenwechsel erhalten
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Application.targetFrameRate =60;
    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("Score: " + score);
    }

    public void LoseLife()
    {
        lives--;
        Debug.Log("Lives: " + lives);
        if (lives <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        Debug.Log("Game Over!");
        // Hier kannst du z.B. ein Game Over Menü anzeigen
    }
}