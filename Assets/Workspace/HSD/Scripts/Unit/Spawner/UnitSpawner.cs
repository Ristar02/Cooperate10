using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    private GameObject _unitPrefab;

    public void Init(GameObject unitPrefab)
    {
        _unitPrefab = unitPrefab;
    }

    private void SpawnUnit()
    {
        Instantiate(_unitPrefab, transform.position, Quaternion.identity);
    }
}
