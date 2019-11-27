using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordManager : MonoBehaviour
{
    public Transform playerOrigin;
    public Transform swordOrigin;
    public Transform spawnPoint;
    private Vector3[] spawnPoints;
    private List<int> spawnPointPool;

    [Header("Prefabs")]
    public GameObject sword1;
    public GameObject sword2;
    public GameObject sword3;

    [Header("Spawn Settings")]
    public int maxSwordsOnScreen = 5;
    public float spawnDelay = 5.0f;
    private static float swordSpawnTimer;

    void Start()
    {
        swordSpawnTimer = 0.0f;
        CreateSpawnPoints(12);
    }

    void Update()
    {
        if (!TutorialManager.tutorialActive
            && PlayerController.playerState == PlayerController.ACTIVELY_PLAYING)
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

    private void CreateSpawnPoints(int points)
    {
        int numOfPoints = points;
        spawnPoints = new Vector3[numOfPoints];
        spawnPointPool = new List<int>();
        for (int i = 0; i < numOfPoints; i++)
        {
            swordOrigin.Rotate(0.0f, 360.0f / numOfPoints, 0.0f);
            GameObject newSpawnPoint = new GameObject("SwordSpawnPoint" + i);
            newSpawnPoint.transform.SetParent(transform);
            newSpawnPoint.transform.position = swordOrigin.position + (swordOrigin.transform.forward * PlayerController.worldRadius) + (playerOrigin.transform.up * PlayerController.worldHeight);
            spawnPoints[i] = newSpawnPoint.transform.position;
        }
        ResetSpawnPointPool(-1);
    }
    private void ResetSpawnPointPool(int exception)
    {
        spawnPointPool = new List<int>();
        int i = 0;
        foreach (Vector3 sp in spawnPoints)
        {
            if (i != exception)
            {
                spawnPointPool.Add(i);
            }
            i++;
        }
    }

    public void SpawnSword()
    {
        GameObject swordToSpawn; //DON'T SET TO NEW GAMEOBJECT
        int swordID = Random.Range(0, 3) + 1;
        switch (swordID)
        {
            default:
                swordToSpawn = sword1;
                break;
            case 2:
                swordToSpawn = sword2;
                break;
            case 3:
                swordToSpawn = sword3;
                break;
        }

        int randomizedSpawnPoint = 0;
        if (spawnPointPool.Count > 0)
        {
            randomizedSpawnPoint = spawnPointPool[Random.Range(0, spawnPointPool.Count)];
            spawnPointPool.Remove(randomizedSpawnPoint);
            if (spawnPointPool.Count <= 0)
            {
                ResetSpawnPointPool(randomizedSpawnPoint);
            }
        }
        spawnPoint.position = spawnPoints[randomizedSpawnPoint];

        Instantiate(swordToSpawn, spawnPoint.position, spawnPoint.rotation);
        swordSpawnTimer = 0.0f;
    }
    public void SpawnSpecificSword(int swordID)
    {
        GameObject swordToSpawn; //DON'T SET TO NEW GAMEOBJECT
        switch (swordID)
        {
            default:
                swordToSpawn = sword1;
                break;
            case 2:
                swordToSpawn = sword2;
                break;
            case 3:
                swordToSpawn = sword3;
                break;
        }

        int randomizedSpawnPoint = 0;
        if (spawnPointPool.Count > 0)
        {
            randomizedSpawnPoint = spawnPointPool[Random.Range(0, spawnPointPool.Count)];
            spawnPointPool.Remove(randomizedSpawnPoint);
            if (spawnPointPool.Count <= 0)
            {
                ResetSpawnPointPool(randomizedSpawnPoint);
            }
        }
        spawnPoint.position = spawnPoints[randomizedSpawnPoint];

        Instantiate(swordToSpawn, spawnPoint.position, spawnPoint.rotation);
        swordSpawnTimer = 0.0f;
    }
    public void ResetSwords()
    {
        swordSpawnTimer = 0.0f;
        var currentSwords = GameObject.FindGameObjectsWithTag("Sword");
        foreach (var sword in currentSwords)
        {
            Destroy(sword);
        }
    }

}
