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
    public float spawnRate = 5.0f;
    private float spawnDelay = 1.0f;

    public Transform swordOrigin;
    public Transform spawnPoint;

    [Header("Sword Settings")]
    public float spawnHeight = 100.0f;


    void Start()
    {
        worldRadius = PlayerController.worldRadius;
    }

    void Update()
    {
        spawnDelay += Time.deltaTime;
        if (spawnDelay > spawnRate)
        {
            spawnDelay = 0.0f;
            SpawnSword();
        }
        
    }

    void SpawnSword()
    {
        swordOrigin.Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        spawnPoint.position = swordOrigin.position + (swordOrigin.transform.forward * worldRadius) + (playerOrigin.transform.up * spawnHeight);


        Instantiate(sword, spawnPoint.position, spawnPoint.rotation);

        // If the player has no health left...
        //if (playerHealth.currentHealth <= 0f)
        //{
        // ... exit the function.
        //  return;
        //}

    }

}
