using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteorController : MonoBehaviour
{
    //public Material testColour;
    //private Material originalColour;
    public int meteorID;
    public bool withinAttackRange;
    public bool isLowest;

    [SerializeField] private GameObject fallMarkerPrefab = null;
    [SerializeField] private Vector3 fallDirection = Vector3.down;
    private GameObject fallMarkerInstance = null;

    void Start()
    {
        //originalColour = transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material;
        isLowest = false;
        withinAttackRange = false;

        if (fallMarkerPrefab != null)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Ray(transform.position, fallDirection), out hit))
            {
                Vector3 fallPosition = hit.point;
                fallMarkerInstance = Instantiate(fallMarkerPrefab, fallPosition - 0.05f * fallDirection, Quaternion.identity); // small displacement to avoid z-fighting with the ground
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (fallMarkerInstance != null)
        {
            if (isLowest)
            {
                fallMarkerInstance.GetComponentInChildren<Light>().color = Color.yellow;
                fallMarkerInstance.GetComponentInChildren<Light>().intensity = 8.0f + (Mathf.Sin(Time.time * 9.0f) * 2.5f + 2.5f);
            }
            else
            {
                fallMarkerInstance.GetComponentInChildren<Light>().color = Color.red;
                fallMarkerInstance.GetComponentInChildren<Light>().intensity = 4.0f + (Mathf.Sin(Time.time * 3.0f) * 2.5f + 2.5f);
            }
        }
        if (PlayerController.playerState == 1)
        {
            transform.Translate(Vector3.up * -1 * MeteorManager.fallSpeed * Time.deltaTime);
        }

        if (isLowest)
        {
            //transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = testColour;
        }
        else
        {
            //transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = originalColour;
        }

    }

    private void OnDestroy()
    {
        if (fallMarkerInstance != null) {
            Destroy(fallMarkerInstance);
        }
    }
}
