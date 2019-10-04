using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordManager : MonoBehaviour
{
    public Transform playerOrigin;
    private float worldRadius;

    public GameObject sword;

    [Header("Spawn Settings")]
    public int maxSwordsOnScreen = 5;
    public float spawnDelay = 5.0f;
    private static float swordSpawnTimer;

    public Transform swordOrigin;
    public Transform spawnPoint;

    [Header("Sword Settings")]
    public float spawnHeight = 100.0f;


    void Start()
    {
        swordSpawnTimer = 0.0f;
        worldRadius = PlayerController.worldRadius;
    }

    void Update()
    {
        if (PlayerController.playerState == 1)
        {
            swordSpawnTimer += Time.deltaTime;
            if (swordSpawnTimer > spawnDelay)
            {
                if (GameObject.FindGameObjectsWithTag("Sword").Length < maxSwordsOnScreen)
                {
                    SpawnSword();
                }
            }
        }            
    }

    void SpawnSword()
    {
        swordOrigin.Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        spawnPoint.position = swordOrigin.position + (swordOrigin.transform.forward * worldRadius) + (playerOrigin.transform.up * spawnHeight);


        Instantiate(sword, spawnPoint.position, spawnPoint.rotation);
        swordSpawnTimer = 0.0f;

        // If the player has no health left...
        //if (playerHealth.currentHealth <= 0f)
        //{
        // ... exit the function.
        //  return;
        //}
    }
    public static void ResetSwords()
    {
        swordSpawnTimer = 0.0f;
        var currentSwords = GameObject.FindGameObjectsWithTag("Sword");
        foreach (var sword in currentSwords)
        {
            Destroy(sword);
        }
        Debug.Log("Swords Cleared!");
    }

}
