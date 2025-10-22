using Cysharp.Threading.Tasks;
using UnityEngine;

public class AddressableLoad : MonoBehaviour
{
    void Start()
    {
        Manager.Resources.LoadLabel("Stage").Forget();
    }
}
