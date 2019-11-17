using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorManager : MonoBehaviour
{
    public GUIController gui;
    public Transform playerOrigin;
    public Transform meteorOrigin;
    public Transform spawnPoint;
    private Vector3[] spawnPoints;
    private List<int> spawnPointPool;

    [Header("Prefabs")]
    public GameObject meteor;
    public GameObject meteorZanbato;
    public GameObject meteorBroadsword;
    public GameObject meteorKatana;
    public GameObject flyingMeteor;


    [Header("Meteor Settings")]
    public float meteorSpeed = 10.0f; //for Inspector usage, may not be necessary
    public static float fallSpeed = 10.0f;

    [Header("Spawn Settings")]
    public GameObject[] currentMeteors;
    public int maxMeteorsOnScreen = 5;
    public float spawnDelay = 5.0f;
    private static float meteorSpawnTimer;
    public int maxMeteorsForLevel = 10;
    public int numOfMeteorsSpawned = 0; //Make private eventually

    public int specialMeteorRate = 3;
    private int specialMeteorCounter = 0;

    public bool flyingMeteors;
    public static float flyingMeteorMoveSpeed;
    public float flyingMeteorSpeed = 20.0f;
    public float flyingMeteorSpawnDelay = 3.0f;
    private float flyingMeteorSpawnTimer;

    void Start()
    {
        meteorSpawnTimer = 0.0f;
        flyingMeteorSpawnTimer = 0.0f;
        PlayerController.maxMeteorsForLevel = maxMeteorsForLevel;
        numOfMeteorsSpawned = GameObject.FindGameObjectsWithTag("Meteor").Length; //This counts the meteors that appeared from default.
        CreateSpawnPoints(6);
    }

    void Update()
    {
        fallSpeed = meteorSpeed;
        flyingMeteorMoveSpeed = flyingMeteorSpeed;

        meteorSpawnTimer += Time.deltaTime;
        if (!TutorialManager.tutorialActive
            && PlayerController.playerState == 1
            && GameObject.FindGameObjectsWithTag("Meteor").Length < maxMeteorsOnScreen
            && numOfMeteorsSpawned < maxMeteorsForLevel)
        {

            if (meteorSpawnTimer > spawnDelay)
            {
                SpawnMeteor();
            }
        }

        if (flyingMeteors)
        {
            flyingMeteorSpawnTimer += Time.deltaTime;
            if (flyingMeteorSpawnTimer > flyingMeteorSpawnDelay)
            {
                SpawnFlyingMeteor();
                flyingMeteorSpawnTimer = 0.0f;
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
            meteorOrigin.Rotate(0.0f, 360.0f / numOfPoints, 0.0f);
            GameObject newSpawnPoint = new GameObject("MeteorSpawnPoint" + i);
            newSpawnPoint.transform.SetParent(transform);
            newSpawnPoint.transform.position = meteorOrigin.position + (meteorOrigin.transform.forward * PlayerController.worldRadius) + (playerOrigin.transform.up * PlayerController.worldHeight);
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
    public void SpawnSpecialMeteor()
    {
        specialMeteorCounter = specialMeteorRate;
        SpawnMeteor();
    }
    public void SpawnMeteor()
    {
        int meteorID = 0;
        GameObject meteorToSpawn = meteor;
        if (specialMeteorRate != 0)
        {
            specialMeteorCounter++;
            if (specialMeteorCounter >= specialMeteorRate)
            {
                specialMeteorCounter = 0;
                meteorID = Random.Range(0, 3) + 1;
                switch (meteorID)
                {
                    default:
                        meteorToSpawn = meteorZanbato;
                        Debug.Log("red meteor appeared");
                        break;
                    case 2:
                        meteorToSpawn = meteorBroadsword;
                        Debug.Log("yellow meteor appeared");
                        break;
                    case 3:
                        meteorToSpawn = meteorKatana;
                        Debug.Log("blue meteor appeared");
                        break;
                }
            }
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

        GameObject newMeteor = Instantiate(meteorToSpawn, spawnPoint.position, spawnPoint.rotation);
        if (meteor.GetComponent<MeteorController>() != null)
        {
            gui.AddMinimapMeteor(newMeteor, meteorID);
        }
        currentMeteors = GameObject.FindGameObjectsWithTag("Meteor");

        numOfMeteorsSpawned++;
        meteorSpawnTimer = 0.0f;
    }
    private void SpawnFlyingMeteor()
    {
        meteorOrigin.Rotate(0.0f, Random.Range(0, 360.0f), 0.0f);
        spawnPoint.position = meteorOrigin.position + (meteorOrigin.transform.forward * PlayerController.worldRadius * Random.Range(0, 1.0f)) + (playerOrigin.transform.up * PlayerController.worldHeight);

        Instantiate(flyingMeteor, spawnPoint.position, spawnPoint.rotation);
    }

    public void ResetMeteors()
    {
        ResetSpawnPointPool(-1);
        meteorSpawnTimer = 0.0f;
        if (currentMeteors.Length > 0)
        {
            currentMeteors = GameObject.FindGameObjectsWithTag("Meteor");
            foreach (GameObject meteor in currentMeteors)
            {
                if (meteor.GetComponent<MeteorController>() != null)
                {
                    Destroy(meteor); //THIS CAN DESTROY FILES, USE CAREFULLY
                }
            }
        }
        numOfMeteorsSpawned = GameObject.FindGameObjectsWithTag("Meteor").Length; //This counts the meteors that appeared from default.
    }
}
