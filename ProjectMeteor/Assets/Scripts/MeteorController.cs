using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorController : MonoBehaviour
{
    //public Material testColour;
    //private Material originalColour;
    public Transform meteorMesh;
    public int meteorID;
    public bool withinAttackRange;
    public bool isLowest;
    private float fallMultiplier;

    [SerializeField] private GameObject fallMarkerPrefab = null;
    [SerializeField] private Vector3 fallDirection = Vector3.down;
    private GameObject fallMarkerInstance = null;

    void Start()
    {
        isLowest = false;
        withinAttackRange = false;
        fallMultiplier = -1.0f;
        if (meteorID != 0) fallMultiplier /= 2; //Special meteors move at half speed

        if (fallMarkerPrefab != null)
        {
            if (Physics.Raycast(new Ray(transform.position, fallDirection), out RaycastHit hit))
            {
                Vector3 fallPosition = hit.point;
                fallMarkerInstance = Instantiate(fallMarkerPrefab, fallPosition - 0.05f * fallDirection, Quaternion.identity); // small displacement to avoid z-fighting with the ground
            }
        }
    }
    void Update()
    {
        meteorMesh.Rotate(Time.deltaTime * 5, Time.deltaTime * 5, Time.deltaTime * 5);
        if (fallMarkerInstance != null)
        {
            fallMarkerInstance.GetComponentInChildren<Light>().color = Color.red;
            fallMarkerInstance.GetComponentInChildren<Light>().intensity = 4.0f + (Mathf.Sin(Time.time * 3.0f) * 2.5f + 2.5f);
        }
        if (PlayerController.playerState == PlayerController.ACTIVELY_PLAYING && !PauseMenu.GameIsPaused)
        {
            transform.Translate(Vector3.up * fallMultiplier * MeteorManager.fallSpeed * Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        if (fallMarkerInstance != null) {
            Destroy(fallMarkerInstance);
        }
    }
}
