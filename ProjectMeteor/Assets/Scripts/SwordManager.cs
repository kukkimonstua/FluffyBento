using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordManager : MonoBehaviour
{
    public Transform playerOrigin;
    private float worldRadius;

    public GameObject sword1;
    public GameObject sword2;
    public GameObject sword3;

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
        GameObject swordToSpawn = new GameObject();
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

        swordOrigin.Rotate(0.0f, Random.Range(0, 24) * 15.0f, 0.0f);
        spawnPoint.position = swordOrigin.position + (swordOrigin.transform.forward * worldRadius) + (playerOrigin.transform.up * spawnHeight);

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
        Debug.Log("Swords Cleared!");
    }

}
