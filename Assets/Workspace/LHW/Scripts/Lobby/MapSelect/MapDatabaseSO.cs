using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MapDataSO", menuName = "Data/Map/MapDataSO")]
public class MapDatabaseSO : ScriptableObject
{
    public List<MapData> Maps = new List<MapData>();
}

[Serializable]
public class MapData
{
    public string MapName;
    public string MapDescription;
    public Sprite MapImage;
}