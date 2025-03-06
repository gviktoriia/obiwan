using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    [Header("Game Settings")]
    public HoleSpawner[] holes;
    public float spawnInterval = 2f;
    public int lives = 3;
    public int score = 0;

    [Header("UI References")]
    public TMP_Text scoreText;
    public TMP_Text livesText;

    void Awake()
    {
        // start
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // to avoid replicate
        }
    }

    void Start()
    {
        UpdateUI();
        StartCoroutine(SpawnMoles());
    }

    IEnumerator SpawnMoles()
    {
        yield return new WaitForSeconds(spawnInterval);
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateUI();
    }

    void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        // livesText.text = "Lives: " + lives;
    }

    void EndGame()
    {
        
    }
}