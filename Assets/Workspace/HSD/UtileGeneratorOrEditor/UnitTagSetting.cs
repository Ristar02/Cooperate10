using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class UnitTagSetting : MonoBehaviour
{
    [SerializeField] GameObject[] objs;

    [ContextMenu("Setting")]
    public void Setting()
    {
        foreach (GameObject obj in objs)
        {
            Transform t = obj.transform.GetChild(0);

            t.tag = "UnitTrigger";
            t.gameObject.layer = 0;
        }
    }
}
