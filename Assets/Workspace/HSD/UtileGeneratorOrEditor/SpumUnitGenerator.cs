using Cysharp.Threading.Tasks.Triggers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SpumUnitGenerator : MonoBehaviour
{
    [SerializeField] Transform _pivot;
    [SerializeField] GameObject[] objs;
    [SerializeField] Vector2Int size;
    [SerializeField] Vector2 spacing;

    [ContextMenu("Create")]
    public void Generate()
    {
        foreach (var obj in objs)
        {
            DestroyImmediate(obj.GetComponent<RectTransform>());
            obj.AddComponent<Transform>();
            
            var child = obj.transform.Find("UnitRoot")?.gameObject;
            if (child == null)
                child = obj.transform.Find("HorseRoot")?.gameObject;

            DestroyImmediate(child.GetComponent<SortingGroup>());
            
            if (!child.TryGetComponent<BaseFSM>(out var fsm))
                 fsm = child.AddComponent<BaseFSM>();

            if (!child.TryGetComponent<BoxCollider2D>(out var bc))
            {
                bc = child.AddComponent<BoxCollider2D>();
                bc.isTrigger = true;                
            }

            bc.offset = new Vector2(0, 0.35f);

            fsm.Owner = obj.GetComponent<UnitBase>();
            obj.GetComponent<UnitBase>().Awake();
        }
    }

    [ContextMenu("Sorting Units")]
    public void Sorting()
    {
        int index = 0;

        for (int i = 0; i < size.y; i++)
        {
            for (int j = 0; j < size.x; j++)
            {
                if (index >= objs.Length)
                    return;

                Vector2 pos = new Vector2(j * spacing.x, -i * spacing.y);
                objs[index].transform.position = (Vector2)_pivot.position + pos;

                index++;
            }
        }
    }
}
