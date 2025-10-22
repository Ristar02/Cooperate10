using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DataManager : Singleton<DataManager>
{
    // 유닛 데이터 관련
    public Dictionary<string, UnitData> UnitDataDic;    
    public UnitData[] EnemyUnitDatas;
    public UnitData[] PlayerUnitDatas;
    public SynergyDatabase SynergyDB;
    public UnitSpawnChanceData UnitSpawnChanceData;

    public AnimationManager AnimationManager = new();

    // 프리셋 데이터 관련
    public PresetDatabase PresetDB { get; private set; } = new PresetDatabase();

    // 맵 데이터 관련
    public MapDatabase MapDB { get; private set; } = new MapDatabase();
    public StageGridData StageGridData = new();

    // 추후 Init으로 뺄 예정
    public async UniTask InitAsync()
    {
        PresetDB.InitPresetData();
        MapDB.InitMapData();
        await InitData();
    }
    #region UniData

    public async UniTask InitData()
    {
        await Manager.Resources.SpriteLoadLable("MonsterIcon");
        await Manager.Resources.SpriteLoadLable("PlayerUnitIcon");
        await Manager.Resources.SpriteLoadLable("SkillIcon");

        UnitSpawnChanceData = await Addressables.LoadAssetAsync<UnitSpawnChanceData>("Data/UnitSpawnChanceData");
        await AnimationManager.Init();
        await PreLoadData();
        await CsvDownload();
    }

    private async UniTask CsvDownload()
    {
        CsvLoadData data = await Addressables.LoadAssetAsync<CsvLoadData>("Data/CsvLoadData");
        CsvDownloader csvDownloader = new CsvDownloader(data);

        csvDownloader.DownloadDataAsync().Forget();
    }

    private async UniTask PreLoadData()
    {
        UniTask[] tasks = new UniTask[2];

        tasks[0] = PreLoadSynergyDB();
        tasks[1] = PreLoadUnitDatas();

        await UniTask.WhenAll(tasks);
    }

    private async UniTask PreLoadUnitDatas()
    {
        EnemyUnitDatas = await Manager.Resources.LoadAll<UnitData>("EnemyUnitData");        
        PlayerUnitDatas = await Manager.Resources.LoadAll<UnitData>("UnitData");

        UnitDataDic = new Dictionary<string, UnitData>(PlayerUnitDatas.Length);

        foreach (var unitData in PlayerUnitDatas)
        {
            if (!UnitDataDic.ContainsKey(unitData.Name))
                UnitDataDic.Add(unitData.Name, unitData);

            unitData.Init();
        }

        foreach (var unitData in EnemyUnitDatas)
        {
            if (!UnitDataDic.ContainsKey(unitData.Name))
                UnitDataDic.Add(unitData.Name, unitData);

            unitData.Init();
        }
    }    

    private async UniTask PreLoadSynergyDB()
    {
        SynergyDB = await Addressables.LoadAssetAsync<SynergyDatabase>("Database/SynergyDatabase");
        SynergyDB.Init();
    }

    public UnitData GetUnitData(string unitName)
    {
        return UnitDataDic.TryGetValue(unitName, out var unitData) ? unitData : null;
    }

    #endregion
}
