using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaBackgroundMap : MonoBehaviour
{
    public Transform targetTransform;

    void Awake()
    {
        targetTransform = GetComponent<Transform>();
    }
}
