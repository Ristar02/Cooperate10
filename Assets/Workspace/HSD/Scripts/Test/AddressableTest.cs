using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddressableTest : MonoBehaviour
{
    [SerializeField] AssetLabelReference labelReference;
    [SerializeField] string adress;
    private async void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            await Manager.Resources.UnloadLabel(labelReference);
            Debug.Log("StageUnload");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            Manager.Resources.Instantiate<GameObject>(adress, Vector2.zero);
        }
    }
}
