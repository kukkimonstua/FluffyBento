using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordManager : MonoBehaviour
{
    public Transform playerOrigin;
    private float offsetFromCentre;

    public GameObject sword;
    public float spawnRate = 5.0f;
    private float spawnTimer = 1.0f;

    public Transform swordOrigin;
    public Transform spawnPoint;

    
    void Start()
    {
        offsetFromCentre = 500.0f;
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer > spawnRate)
        {
            spawnTimer = 0.0f;
            SpawnSword();
        }
        
    }

    void SpawnSword()
    {
        swordOrigin.Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        spawnPoint.position = swordOrigin.position + (swordOrigin.transform.forward * offsetFromCentre) + (playerOrigin.transform.up * -1);
        Instantiate(sword, spawnPoint.position, spawnPoint.rotation);

        // If the player has no health left...
        //if (playerHealth.currentHealth <= 0f)
        //{
        // ... exit the function.
        //  return;
        //}

    }

}
