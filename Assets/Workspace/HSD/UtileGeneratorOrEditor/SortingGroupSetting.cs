using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class SortingGroupSetting : MonoBehaviour
{
    [SerializeField] GameObject[] objs;

    [ContextMenu("Setting")]
    public void Setting()
    {
        foreach (GameObject obj in objs)
        {
            Transform t = obj.transform.Find("UnitRoot");

            if(t == null)
                t = obj.transform.Find("HorseRoot");

            if(t.GetComponent<SortingGroup>() == null)
                t.AddComponent<SortingGroup>();

            t.GetComponent<SortingGroup>().sortingLayerName = "Unit";
            
            t.tag = "UnitTrigger";
        }
    }
}
