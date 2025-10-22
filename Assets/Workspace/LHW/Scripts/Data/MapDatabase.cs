using System.Collections.Generic;
using UnityEngine;

public class MapDatabase : MonoBehaviour
{
    private MapDatabaseSO _mapDatabase;

    public void InitMapData()
    {
        // TODO : 임시로 Resouces.Load로 데이터를 로드 - 이후 DB로 로드하는 방식으로 변경
        _mapDatabase = Resources.Load<MapDatabaseSO>("MapDataSO");
    }

    public List<MapData> ReturnAllMapData()
    {
        return _mapDatabase.Maps;
    }

    public MapData ReturnMapData(int index)
    {
        return _mapDatabase.Maps[index];
    }
}