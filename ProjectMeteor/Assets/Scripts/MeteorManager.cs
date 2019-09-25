using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorManager : MonoBehaviour
{
    [Header("Linked GameObjects")]
    public GameObject meteor;
    public Transform playerOrigin;
    private float worldRadius;
    public Transform meteorOrigin;
    public Transform spawnPoint;

    [Header("Meteor Settings")]
    public float spawnHeight = 300.0f;
    public float meteorSpeed = 10.0f; //for Inspector usage, may not be necessary
    public static float fallSpeed = 10.0f;

    [Header("Spawn Settings")]
    public int maxMeteorsOnScreen = 5;
    public float spawnDelay = 5.0f;
    private float spawnTimer = 0.0f;

    public bool pauseMeteors = false; //for Inspector usage, may not be necessary
    public static bool meteorsPaused = false;

    void Start()
    {
        //offsetFromCentre = Vector3.Distance(playerOrigin.transform.position, meteorOrigin.position);
        worldRadius = 500.0f;
        //InvokeRepeating("SpawnMeteor", spawnRate, spawnRate);
    }

    void Update()
    {
        //Use this if you need to test behaviour related to meteors in the main game
        //if (pauseMeteors && !meteorsPaused) meteorsPaused = true; //Pauses the meteors        
        //if (!pauseMeteors && meteorsPaused) meteorsPaused = false; //Unpauses the meteors

        fallSpeed = meteorSpeed;

        if (!meteorsPaused)
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer > spawnDelay)
            {
                
                if (GameObject.FindGameObjectsWithTag("Meteor").Length < maxMeteorsOnScreen)
                {
                    SpawnMeteor();
                }
            }
        }
    }

    void SpawnMeteor()
    {
        meteorOrigin.Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
        spawnPoint.position = meteorOrigin.position + (meteorOrigin.transform.forward * worldRadius) + (playerOrigin.transform.up * spawnHeight);

        var currentMeteors = GameObject.FindGameObjectsWithTag("Meteor");

        for (int i = 0; i < currentMeteors.Length; i++)
        {
            if (currentMeteors[i].GetComponent<Collider>().bounds.Contains(spawnPoint.position))
            {
                Debug.Log("Spawned inside another meteor");
                return;
            }
        }

        Instantiate(meteor, spawnPoint.position, spawnPoint.rotation);
        spawnTimer = 0.0f;
    }

}
