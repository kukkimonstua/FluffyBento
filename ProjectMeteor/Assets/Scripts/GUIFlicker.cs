using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIFlicker : MonoBehaviour
{
    public float speed = 5.0f;
    public float range = 0.3f;
    public float offset = 0.5f;

    private CanvasRenderer[] children;

    void Start()
    {
        children = GetComponentsInChildren<CanvasRenderer>();
    }

    void LateUpdate()
    {
        GetComponent<CanvasRenderer>().SetAlpha(Mathf.Sin(Time.time * speed) * range + offset);
        if (children != null)
        {
            foreach (CanvasRenderer c in children)
            {
                c.SetAlpha(Mathf.Sin(Time.time * speed) * range + offset);
            }
        }
    }
}
