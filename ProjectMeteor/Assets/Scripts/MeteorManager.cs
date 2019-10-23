using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorManager : MonoBehaviour
{
    [Header("Linked GameObjects")]
    public GameObject meteor;
    public GameObject meteorZanbato;
    public GameObject meteorBroadsword;
    public GameObject meteorKatana;
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
    public int maxMeteorsForLevel = 10;
    public int numOfMeteorsSpawned = 0; //Make private eventually

    public int specialMeteorRate = 3;
    private int specialMeteorCounter = 0;

    void Start()
    {
        meteorSpawnTimer = 0.0f;
        worldRadius = PlayerController.worldRadius;
        PlayerController.maxMeteorsForLevel = maxMeteorsForLevel;
        numOfMeteorsSpawned = GameObject.FindGameObjectsWithTag("Meteor").Length; //This counts the meteors that appeared from default.
    }

    void Update()
    {
        fallSpeed = meteorSpeed;

        if (PlayerController.playerState == 1
            && GameObject.FindGameObjectsWithTag("Meteor").Length < maxMeteorsOnScreen
            && numOfMeteorsSpawned < maxMeteorsForLevel)
        {
            meteorSpawnTimer += Time.deltaTime;
            if (meteorSpawnTimer > spawnDelay)
            {
                SpawnMeteor();
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
        GameObject meteorToSpawn = meteor;

        if (specialMeteorRate != 0)
        {
            specialMeteorCounter++;
            if (specialMeteorCounter >= specialMeteorRate)
            {
                specialMeteorCounter = 0;
                int meteorID = Random.Range(0, 3) + 1;
                switch (meteorID)
                {
                    default:
                        meteorToSpawn = meteorZanbato;
                        break;
                    case 2:
                        meteorToSpawn = meteorBroadsword;
                        break;
                    case 3:
                        meteorToSpawn = meteorKatana;
                        break;
                }
            }
        }
        Instantiate(meteorToSpawn, spawnPoint.position, spawnPoint.rotation);
        numOfMeteorsSpawned++;
        meteorSpawnTimer = 0.0f;
    }

    public void ResetMeteors()
    {
        meteorSpawnTimer = 0.0f;
        var currentMeteors = GameObject.FindGameObjectsWithTag("Meteor");
        foreach (var meteor in currentMeteors)
        {
            Destroy(meteor);
        }
        numOfMeteorsSpawned = GameObject.FindGameObjectsWithTag("Meteor").Length; //This counts the meteors that appeared from default.
        Debug.Log("Meteors Cleared!");
    }
}
