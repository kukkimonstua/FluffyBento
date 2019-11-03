using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeteorMinimapMarker : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isLowest;

    void Start()
    {
        isLowest = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isLowest)
        {
            GetComponent<Image>().color = Color.yellow;
        }
        else
        {
            GetComponent<Image>().color = Color.green;
        }
    }
}
