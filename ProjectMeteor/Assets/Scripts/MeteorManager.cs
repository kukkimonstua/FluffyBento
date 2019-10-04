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
    private static float meteorSpawnTimer;

    void Start()
    {
        meteorSpawnTimer = 0.0f;
        worldRadius = PlayerController.worldRadius;
    }

    void Update()
    {
        fallSpeed = meteorSpeed;

        if (PlayerController.playerState == 1)
        {
            meteorSpawnTimer += Time.deltaTime;
            if (meteorSpawnTimer > spawnDelay)
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
        meteorOrigin.Rotate(0.0f, Random.Range(0, 6) * 60.0f, 0.0f);
        spawnPoint.position = meteorOrigin.position + (meteorOrigin.transform.forward * worldRadius) + (playerOrigin.transform.up * spawnHeight);

        var currentMeteors = GameObject.FindGameObjectsWithTag("Meteor");
        for (int i = 0; i < currentMeteors.Length; i++)
        {
            if (currentMeteors[i].GetComponent<Collider>().bounds.Contains(spawnPoint.position))
            {
                //Debug.Log("Spawned inside another meteor");    Problem: it will pass even if the future meteor to be spawned would overlap...
                return;
            }
        }

        Instantiate(meteor, spawnPoint.position, spawnPoint.rotation);
        meteorSpawnTimer = 0.0f;
    }

    public static void ResetMeteors()
    {
        meteorSpawnTimer = 0.0f;
        var currentMeteors = GameObject.FindGameObjectsWithTag("Meteor");
        foreach (var meteor in currentMeteors)
        {
            Destroy(meteor);
        }
        Debug.Log("Meteors Cleared!");
    }
}
