using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Level Setup")]
    public Transform startPoint;    // dein StartPoint in der Szene
    public GameObject playerPrefab;  // dein Player-Prefab

    private GameObject currentPlayer;
    private UIManager uiManager;

    void Start()
    {
        StartCoroutine(SpawnPlayer());
    }

    void Update()
    {
        // Hier kannst du Logik hinzufügen, um den Spieler neu zu spawnen, z.B. bei einem Tod
        if (Input.GetKeyDown(KeyCode.R)) // Beispiel: R-Taste zum Respawn
        {
            StartCoroutine(SpawnPlayer());
        }
    }

    public IEnumerator SpawnPlayer()
    {
        if (currentPlayer != null)
            Destroy(currentPlayer);

        yield return new WaitForSeconds(0.5f);
        currentPlayer = Instantiate(
            playerPrefab,
            startPoint.position,
            startPoint.rotation
        );
    }



}
