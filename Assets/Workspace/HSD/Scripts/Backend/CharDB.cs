using Cysharp.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

public class CharDB
{
    private DatabaseReference _characterReference;
    private string _uid => FirebaseManager.Auth.CurrentUser.UserId;

    public void EventHandler()
    {
        FirebaseManager.DataReference.Child("UserData").Child(_uid).Child("CharacterData").
            ChildChanged += UpdateCharacterDatas;
        Debug.Log("캐릭터 데이터 연동됨");
    }

    public async UniTask InitializeCharacterData()
    {
        var initRef = FirebaseManager.DataReference.Child("InitCharacterData");

        foreach (var charData in Manager.Data.UnitDataDic.Values)
        {
            UnitDataDTO dto = charData.ToDTO(charData);
            await initRef.Child(charData.Name).
                SetRawJsonValueAsync(JsonUtility.ToJson(dto));
            Debug.Log($"{charData.Name}");
        }
    }

    public async UniTask InitializeCharacterUpgradeData()
    {
        _characterReference = FirebaseManager.DataReference
        .Child("UserData").Child(_uid).Child("CharacterData");

        Debug.Log("업그레이드 데이터 Initial 시작");
        await UniTask.Yield();
        Debug.Log("쉬기");
        var snapshot = await _characterReference.GetValueAsync();

        Debug.Log("ddd");

        if (!snapshot.Exists || snapshot.ChildrenCount == 0)
        {
            foreach (var charData in Manager.Data.UnitDataDic.Values)
            {
                if (charData.UpgradeData == null) continue;

                charData.UpgradeData.CurrentUpgradeData = new CurrentUpgradeData
                {
                    CurrentPieces = 0,
                    UpgradeLevel = 0
                };

                Debug.Log($"{charData.Name}");

                await SaveCharacterUpgradeData(charData);

                Debug.Log("업그레이드 데이터 저장 완료");
            }

            Debug.Log("최초 계정 → UpgradeData 초기화 완료");
        }
        else
        {
            Debug.Log("UpgradeData 이미 존재 → 초기화 생략");
        }
    }

    public async UniTask SaveAllCharacterDatas()
    {
        _characterReference = FirebaseManager.DataReference.Child("UserData").Child(_uid).Child("CharacterData");

        foreach (var charData in Manager.Data.UnitDataDic.Values)
        {
            await SaveCharacterUpgradeData(charData);
        }
    }

    public async UniTask SaveCharacterUpgradeData(UnitData charData)
    {
        _characterReference = FirebaseManager.DataReference.Child("UserData").Child(_uid).Child("CharacterData");

        await _characterReference.Child(charData.Name).
                SetRawJsonValueAsync(JsonUtility.ToJson(charData.UpgradeData.CurrentUpgradeData));
    }

    public async UniTask LoadAllCharacterDatas()
    {
        _characterReference = FirebaseManager.DataReference.Child("UserData").Child(_uid).Child("CharacterData");
        var dataSnapshot = await _characterReference.GetValueAsync();

        if (dataSnapshot.Exists)
        {
            foreach (var child in dataSnapshot.Children)
            {
                string charName = child.Key;
                UnitData data = Manager.Data.GetUnitData(charName);

                if (data != null)
                {
                    var loaded = JsonUtility.FromJson<CurrentUpgradeData>(child.GetRawJsonValue());
                    data.UpgradeData.CurrentUpgradeData = loaded ?? new CurrentUpgradeData { CurrentPieces = 0, UpgradeLevel = 0 };
                }
            }
        }
    }

    public void UpdateCharacterDatas(object sender, ChildChangedEventArgs args)
    {
        if (args.Snapshot.Exists)
        {
            string charName = args.Snapshot.Key;
            var charData = Manager.Data.GetUnitData(charName);

            if (charData != null)
            {
                int currentPieces = args.Snapshot.Child("CurrentPieces").Value is long pieces
                    ? (int)pieces : 0;
                int upgradeLevel = args.Snapshot.Child("UpgradeLevel").Value is long level
                    ? (int)level : 0;

                charData.UpgradeData.CurrentUpgradeData.SetData(currentPieces, upgradeLevel);
            }
        }
    }
}