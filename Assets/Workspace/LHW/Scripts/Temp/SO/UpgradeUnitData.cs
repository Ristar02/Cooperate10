using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "Unit_UpgradeUnitData", menuName = "Data/Upgrade/Unit_UpgradeUnitData")]
public class UpgradeUnitData : ScriptableObject
{
    // 캐릭터의 업그레이드 레벨 - 레벨이 0일 때는 획득하지 않은 상태
    private Grade _grade;
    private LevelUpData _levelUpData;

    public CurrentUpgradeData CurrentUpgradeData;

    public event Action OnLevelUp;

    public void Init(Grade grade, LevelUpData data)
    {
        _grade = grade;
        _levelUpData = data;
    }
    
    public int GetRequiredPiece()
    {
        if (CurrentUpgradeData.UpgradeLevel >= 10 || 
            CurrentUpgradeData.UpgradeLevel <= 0) return 0;

        if (_levelUpData == null)
        {
            Debug.LogError($"[{name}] LevelUpData가 설정되지 않았습니다.");
            return 0;
        }

        RequirePiece requirePiece = _levelUpData.RequirePieceData.Find(r=>r.Grade == _grade);
        if (requirePiece == null)
        {
            Debug.LogError($"[{name}] {_grade} 등급에 맞는 RequirePiece 데이터가 없습니다.");
            return 0;
        }

        PieceLevelRatio pieceLevelRatio = requirePiece.LevelRatio.Find(l => l.Level == CurrentUpgradeData.UpgradeLevel + 1);
        if (pieceLevelRatio == null)
        {
            Debug.LogError($"[{name}] {_grade} / {CurrentUpgradeData.UpgradeLevel}에 맞는 PieceLevelRatio 데이터가 없습니다.");
            return 0;
        }

        return pieceLevelRatio.RequirePiece;
    }

    public int GetRequiredGold()
    {
        if (CurrentUpgradeData.UpgradeLevel >= 10 ||
            CurrentUpgradeData.UpgradeLevel <= 0) return 0;

        if (_levelUpData == null)
        {
            Debug.LogError($"[{name}] LevelUpData가 설정되지 않았습니다.");
            return 0;
        }

        RequirePiece requirePiece = _levelUpData.RequirePieceData.Find(r => r.Grade == _grade);
        if (requirePiece == null)
        {
            Debug.LogError($"[{name}] {_grade} 등급에 맞는 RequirePiece 데이터가 없습니다.");
            return 0;
        }

        PieceLevelRatio pieceLevelRatio = requirePiece.LevelRatio.Find(l => l.Level == CurrentUpgradeData.UpgradeLevel + 1);
        if (pieceLevelRatio == null)
        {
            Debug.LogError($"[{name}] {_grade} / {CurrentUpgradeData.UpgradeLevel}에 맞는 PieceLevelRatio 데이터가 없습니다.");
            return 0;
        }

        return pieceLevelRatio.RequireGold;
    }

    public void AddPiece(int piece)
    {
        CurrentUpgradeData.CurrentPieces += piece;
    }

    public async Task<bool> LevelUpWithPiecesOnly()
    {
        if (CurrentUpgradeData.UpgradeLevel >= 10) return false;

        if (CurrentUpgradeData.UpgradeLevel <= 0)
        {
            if (CurrentUpgradeData.CurrentPieces >= 10)
            {
                CurrentUpgradeData.CurrentPieces -= 10;
                CurrentUpgradeData.UpgradeLevel += 1;
                return true;
            }
            else
            {
                if(PopupManager.Instance != null)
                {
                    PopupManager.instance.ShowPopup("조각이 부족합니다.");
                }
                return false;
            }
        }
        else if (CurrentUpgradeData.UpgradeLevel >= 1)
        {
            int requiredPiece = GetRequiredPiece();
            int requiredGold = GetRequiredGold();

            // 골드, 조각 체크
            string uid = FirebaseManager.Auth.CurrentUser.UserId;
            var goldRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("Gold");
            DataSnapshot snapshot = await goldRef.GetValueAsync();
            int currentGold = snapshot.Exists ? Convert.ToInt32(snapshot.Value) : 0;

            if (CurrentUpgradeData.CurrentPieces >= requiredPiece && currentGold >= requiredGold)
            {
                CurrentUpgradeData.CurrentPieces -= requiredPiece;
                CurrentUpgradeData.UpgradeLevel += 1;

                await DBManager.Instance.SubtractGoldAsync(requiredGold);
                OnLevelUp?.Invoke();
                return true;
            }
        }
        return false;
    }

    public async Task<bool> CanLevelUpWithMythStone()
    {
        if (CurrentUpgradeData.UpgradeLevel >= 10) return false;

        int requiredPiece = GetRequiredPiece();
        int requiredGold = GetRequiredGold();

        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        var goldRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("Gold");
        var mythStoneRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("MythStone");

        DataSnapshot goldSnap = await goldRef.GetValueAsync();
        DataSnapshot mythSnap = await mythStoneRef.GetValueAsync();

        int currentGold = goldSnap.Exists ? Convert.ToInt32(goldSnap.Value) : 0;
        int currentMythStone = mythSnap.Exists ? Convert.ToInt32(mythSnap.Value) : 0;

        int ratio = MythStonePieceRatio(_grade);
        int conversedMythstone = currentMythStone / ratio;

        return (CurrentUpgradeData.CurrentPieces + conversedMythstone >= requiredPiece
            && currentGold >= requiredGold);
    }

    public async Task LevelUpWithMythStone()
    {
        int requiredPiece = GetRequiredPiece();
        int requiredGold = GetRequiredGold();
        int ratio = MythStonePieceRatio(_grade);

        int neededFromMythStone = requiredPiece - CurrentUpgradeData.CurrentPieces;
        if (neededFromMythStone > 0)
            await DBManager.Instance.SubtractMythStoneAsync(neededFromMythStone * ratio);

        CurrentUpgradeData.CurrentPieces = 0;
        CurrentUpgradeData.UpgradeLevel += 1;
        await DBManager.Instance.SubtractGoldAsync(requiredGold);

        OnLevelUp?.Invoke();
    }

    private int MythStonePieceRatio(Grade grade)
    {
        int pieceRatio = 0;

        switch (grade)
        {
            case Grade.NORMAL: pieceRatio = 2; break;
            case Grade.RARE: pieceRatio = 4; break;
            case Grade.UNIQUE: pieceRatio = 6; break;
            case Grade.LEGEND: pieceRatio = 9; break;
        }

        return pieceRatio;
    }
}

[CreateAssetMenu(fileName = "Unit_LevelUpData", menuName = "Data/Temp/Unit_LevelUpData")]
public class LevelUpData : ScriptableObject
{
    public List<RequirePiece> RequirePieceData = new();

    /// <summary>
    /// 현재 캐릭터의 등급, 레벨을 기반으로, 최대 레벨을 찍기까지
    /// 남은 조각 개수를 반환하는 함수
    /// </summary>
    /// <param name="grade"></param>
    /// <param name="level"></param>
    public int GetCumulativePiece(Grade grade, int level)
    {
        RequirePiece requirePiece = RequirePieceData.Find(r => r.Grade == grade);
        if (requirePiece == null)
        {
            Debug.LogError($"[{name}] {grade} 등급에 맞는 RequirePiece 데이터가 없습니다.");
            return 0;
        }

        int cumulativePiece = 0;

        int index = level - 1;
        if (index < 0)
        {
            index = 0;
            cumulativePiece += 10;
        }

        for(int i = index; i < requirePiece.LevelRatio.Count; i++)
        {
            cumulativePiece += requirePiece.LevelRatio[i].RequirePiece;
        }
        Debug.Log(cumulativePiece);

        return cumulativePiece;
    }
}

[Serializable]
public class RequirePiece
{
    public Grade Grade;
    public List<PieceLevelRatio> LevelRatio = new List<PieceLevelRatio>();
}

/// <summary>
/// 에디터상 입력을 위해 임시로 List 로 처리함. 추후에 Dictionary로 전환할 필요성 있음
/// </summary>
[Serializable]
public class PieceLevelRatio
{
    public int RequireGold;
    public int RequirePiece;
    public int Level;
}

[Serializable]
public class CurrentUpgradeData
{
    // 현재 보유 캐릭터 조각 수
    public int CurrentPieces;

    public int UpgradeLevel;

    public void SetData(int currentPieces, int upgradeLevel)
    {
        CurrentPieces = currentPieces;
        UpgradeLevel = upgradeLevel;
    }
}