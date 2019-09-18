using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorManager : MonoBehaviour
{
    public Transform playerOrigin;
    private float offsetFromCentre;

    public GameObject meteor;
    public float spawnRate = 5.0f;
    private float spawnTimer = 0.0f;

    public Transform meteorOrigin;
    public Transform spawnPoint;

    public bool pauseMeteors = false; //for Inspector usage, may not be necessary
    private static bool meteorsPaused = false;

    void Start()
    {
        //offsetFromCentre = Vector3.Distance(playerOrigin.transform.position, meteorOrigin.position);
        offsetFromCentre = 500.0f;
        //InvokeRepeating("SpawnMeteor", spawnRate, spawnRate);
    }

    void Update()
    {
        if (pauseMeteors && !meteorsPaused) meteorsPaused = true; //Pauses the meteors        
        if (!pauseMeteors && meteorsPaused) meteorsPaused = false; //Unpauses the meteors
        

        if (!meteorsPaused)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > spawnRate)
            {
                spawnTimer = 0.0f;
                SpawnMeteor();
            }
        }
    }

    void SpawnMeteor()
    {
        meteorOrigin.Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        spawnPoint.position = meteorOrigin.position + (meteorOrigin.transform.forward * offsetFromCentre) + (playerOrigin.transform.up * 250);
        Instantiate(meteor, spawnPoint.position, spawnPoint.rotation);

        // If the player has no health left...
        //if (playerHealth.currentHealth <= 0f)
        //{
        // ... exit the function.
        //  return;
        //}

        // Find a random index between zero and one less than the number of spawn points.
        //int spawnPointIndex = Random.Range(0, spawnPoints.Length);

    }

    public static bool MeteorsPaused()
    {
        return meteorsPaused;
    }
}
