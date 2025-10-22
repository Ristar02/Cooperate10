using Cysharp.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DBManager : Singleton<DBManager>
{
    public CharDB charDB = new CharDB();
    public QuestDB questDB = new QuestDB();
    public ShopDB shopDB = new ShopDB();

    #region Nickname/LobbyData

    /// <summary>
    /// 유저 닉네임(displayname)을 DB에 저장하는 메서드
    /// </summary>
    /// <returns>
    /// true: 저장 성공
    /// false: 저장 실패
    /// </returns>
    public async Task<bool> SaveNicknameAsync()
    {
        FirebaseUser currentUser = FirebaseManager.Auth.CurrentUser;

        string uid = currentUser.UserId;
        string userNickname = FirebaseManager.Auth.CurrentUser.DisplayName;

        Dictionary<string, object> dictionary = new Dictionary<string, object>();

        dictionary[$"UserData/{uid}/Nickname"] = userNickname;

        var task = FirebaseManager.DataReference.UpdateChildrenAsync(dictionary);
        await task;

        if (task.IsCompletedSuccessfully)
        {
            return true;
        }
        else
        {
            Debug.LogError("닉네임 저장 실패");
            return false;
        }
    }

    /// <summary>
    /// 유저 닉네임(string)을 DB에 저장하는 메서드
    /// </summary>
    /// <param name="nickname">저장할 닉네임</param>
    /// <returns>
    /// true: 저장 성공
    /// false: 저장 실패
    /// </returns>
    public async Task<bool> SaveNicknameAsync(string nickname)
    {
        FirebaseUser currentUser = FirebaseManager.Auth.CurrentUser;

        string uid = currentUser.UserId;
        string userNickname = nickname;

        Dictionary<string, object> dictionary = new Dictionary<string, object>();

        dictionary[$"UserData/{uid}/Nickname"] = userNickname;

        var task = FirebaseManager.DataReference.UpdateChildrenAsync(dictionary);
        await task;

        if (task.IsCompletedSuccessfully)
        {
            return true;
        }
        else
        {
            Debug.LogError("닉네임 저장 실패");
            return false;
        }
    }

    /// <summary>
    /// Firebase DB UserData에서 현재 유저 닉네임을 불러오는 메서드
    /// </summary>
    /// <param name="callback">로드된 닉네임을 반환하는 콜백 (없을 경우 null)</param>
    public async Task LoadNicknameAsync(Action<string> callback)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        DatabaseReference nicknameRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("Nickname");

        DataSnapshot snapshot = await nicknameRef.GetValueAsync();
        string nickname = snapshot.Value.ToString();

        if (snapshot.Exists)
        {
            callback(nickname);
        }
        else
        {
            Debug.LogWarning("닉네임 데이터 없음");
            callback(null);
        }
    }

    /// <summary>
    /// Firebase DB UserData에서 LoabbyScene에 표시되어야 할 모든 데이터를 불러오는 메서드
    /// </summary>
    public async Task<PlayerData> LoadLobbyDataAsync()
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        Debug.Log($"[LoadLobbyDataAsync] uid: {uid}");
        DatabaseReference userRef = FirebaseManager.DataReference.Child("UserData").Child(uid);

        DataSnapshot snapshot = await userRef.GetValueAsync();
        string nickname = snapshot.Value.ToString();

        if (!snapshot.Exists)
        {
            Debug.LogWarning("데이터 없음");
        }

        // 기본 스테미나 데이터 설정
        int stamina = int.TryParse(snapshot.Child("Stamina").Value?.ToString(), out int staminaValue) ? staminaValue : 30;
        long lastRecoveryTime = long.TryParse(snapshot.Child("LastStaminaRecoveryTime").Value?.ToString(), out long recoveryTime) ? recoveryTime : DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // 시간 기반 스테미나 회복 계산
        var (recoveredStamina, newRecoveryTime) = CalculateStaminaRecovery(stamina, lastRecoveryTime);

        // 스테미나가 회복되었다면 DB에 저장
        if (recoveredStamina != stamina || newRecoveryTime != lastRecoveryTime)
        {
            await SaveStaminaAsync(recoveredStamina, newRecoveryTime);
        }

        PlayerData data = new PlayerData
        {
            PlayerUid = uid,
            PlayerName = snapshot.Child("Nickname").Value?.ToString() ?? "LoadFailed",
            Gold = int.TryParse(snapshot.Child("Gold").Value?.ToString(), out int gold) ? gold : 0,
            Diamond = int.TryParse(snapshot.Child("Diamond").Value?.ToString(), out int diamond) ? diamond : 0,
            Stamina = recoveredStamina,
            MaxStamina = 30,
            LastStaminaRecoveryTime = newRecoveryTime
        };

        //await charDB.InitializeCharacterData();
        Debug.Log("Initial 완료");
        await charDB.InitializeCharacterUpgradeData();
        await charDB.LoadAllCharacterDatas();
        charDB.EventHandler();

        return data;
    }

    /// <summary>
    /// UserData 경로의 현재 유저 UID를 삭제하는 메서드
    /// </summary>
    public async Task DeleteUserUidAsync()
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;

        Debug.Log($"[DeleteUserUid] 현재 로그인된 유저 uid : {uid}");
        Dictionary<string, object> dictionary = new Dictionary<string, object> { [$"UserData/{uid}"] = null, };

        Task updateTask = FirebaseManager.DataReference.UpdateChildrenAsync(dictionary);
        await updateTask;
    }

    #endregion


    #region Tutorial

    /// <summary>
    /// 계정 로그인 시 튜토리얼 진행 여부를 체크하는 메서드
    /// </summary>
    /// <returns>
    /// true: 튜토리얼 진행
    /// false: 튜토리얼 미진행
    /// </returns>
    public async Task<bool> CheckTutorialCompletedAsync()
    {
        FirebaseAuth auth = FirebaseManager.Auth;
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        DatabaseReference userRef =
            FirebaseManager.DataReference.Child("UserData").Child(uid).Child("IsTutorialComplete");

        // 튜토리얼 진행 여부 확인
        DataSnapshot snapshot = await userRef.GetValueAsync();

        if (!snapshot.Exists)
        {
            Debug.LogWarning($"튜토리얼 상태 null / 유저 UID : {uid}");
            return false;
        }

        // 튜토리얼 진행
        if (snapshot.Value is bool isTutorialComplete)
        {
            Debug.Log("IsTutorialComplete: true / 튜토리얼 진행 계정");
            // 로비 씬으로 전환
            return true;
        }
        else
        {
            Debug.Log("IsTutorialComplete: false / 튜토리얼 미진행 계정");
            // 튜토리얼 패널 활성화
            return false;
        }
    }

    /// <summary>
    /// 튜토리얼 진행 후 튜토리얼 진행 여부 변수를 false 로 설정하는 메서드
    /// IsTutorialComplete = false
    /// </summary>
    public async Task SetTutorialInCompleteAsync()
    {
        string userUid = FirebaseManager.Auth.CurrentUser.UserId;
        DatabaseReference userRef =
            FirebaseManager.DataReference.Child("UserData").Child(userUid).Child("IsTutorialComplete");

        await userRef.SetValueAsync(false);
    }

    /// <summary>
    /// 계정의 생성 후 튜토리얼 진행 여부 변수를 true 로 설정하는 메서드
    /// IsTutorialComplete = true
    /// </summary>
    public async Task SetTutorialCompleteAsync()
    {
        string userUid = FirebaseManager.Auth.CurrentUser.UserId;
        DatabaseReference userRef =
            FirebaseManager.DataReference.Child("UserData").Child(userUid).Child("IsTutorialComplete");

        await userRef.SetValueAsync(true);
    }

    #endregion


    #region Currency

    /// <summary>
    /// 유저의 Gold와 Diamond 값을 동시에 저장하는 메서드
    /// </summary>
    /// <param name="gold">저장할 골드 값</param>
    /// <param name="diamond">저장할 다이아 값</param>
    public async Task<bool> SaveCurrencyAsync(int gold, int diamond)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;

        Dictionary<string, object> dictionary = new Dictionary<string, object>();

        dictionary[$"UserData/{uid}/Gold"] = gold;
        dictionary[$"UserData/{uid}/Diamond"] = diamond;

        var task = FirebaseManager.DataReference.UpdateChildrenAsync(dictionary);
        await task;

        if (task.IsCompletedSuccessfully)
        {
            Debug.Log($"골드/다이아 저장 성공: Gold = {gold}, Diamond = {diamond}");
            return true;
        }
        else
        {
            Debug.LogError("골드/다이아 저장 실패");
            return false;
        }
    }

    /// <summary>
    /// 유저의 현재 Gold 값을 읽어와 지정한 값만큼 증가시킨 뒤 저장하는 메서드
    /// </summary>
    /// <param name="addAmount">증가시킬 Gold 양</param>
    /// <returns></returns>
    public async Task AddGoldAsync(int addAmount)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        var goldRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("Gold");

        DataSnapshot snapshot = await goldRef.GetValueAsync();

        int current = 0;

        if (snapshot.Exists && snapshot.Value != null)
        {
            current = Convert.ToInt32(snapshot.Value);
        }

        int next = current + addAmount;

        await goldRef.SetValueAsync(next);
        Debug.Log($"증가한 Gold: {addAmount} -> {next}");
    }

    /// <summary>
    /// 유저의 현재 Diamond 값을 읽어와 지정한 값만큼 증가시킨 뒤 저장하는 메서드
    /// </summary>
    /// <param name="addAmount">증가시킬 Diamond 양</param>
    /// <returns></returns>
    public async Task AddDiamondAsync(int addAmount)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        var diaRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("Diamond");

        DataSnapshot snapshot = await diaRef.GetValueAsync();

        int current = 0;

        if (snapshot.Exists && snapshot.Value != null)
        {
            current = Convert.ToInt32(snapshot.Value);
        }

        int next = current + addAmount;

        await diaRef.SetValueAsync(next);
        Debug.Log($"증가한 Diamond: {addAmount} -> {next}");
    }


    /// <summary>
    /// 유저의 현재 Diamond 값을 읽어와 지정한 값만큼 감소시킨 뒤 저장하는 메서드
    /// </summary>
    /// <param name="addAmount">감소시킬 Diamond 양</param>
    /// <returns></returns>
    public async Task SubtractGoldAsync(int subAmount)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        var goldRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("Gold");

        DataSnapshot snapshot = await goldRef.GetValueAsync();

        int current = 0;

        if (snapshot.Exists && snapshot.Value != null)
        {
            current = Convert.ToInt32(snapshot.Value);
        }

        if (current-subAmount < 0)
        {
            Debug.LogWarning("골드 부족");
            return;
        }

        int next = current - subAmount;

        await goldRef.SetValueAsync(next);
        Debug.Log($"감소한 Diamond: {subAmount} -> {next}");
    }

    /// <summary>
    /// 유저의 현재 Diamond 값을 읽어와 지정한 값만큼 감소시킨 뒤 저장하는 메서드
    /// </summary>
    /// <param name="addAmount">감소시킬 Diamond 양</param>
    /// <returns></returns>
    public async Task SubtractDiamondAsync(int subAmount)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        var diaRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("Diamond");

        DataSnapshot snapshot = await diaRef.GetValueAsync();

        int current = 0;

        if (snapshot.Exists && snapshot.Value != null)
        {
            current = Convert.ToInt32(snapshot.Value);
        }

        if (current - subAmount < 0)
        {
            Debug.LogWarning("다이아 부족");
            return;
        }

        int next = current - subAmount;

        await diaRef.SetValueAsync(next);
        Debug.Log($"감소한 Diamond: {subAmount} -> {next}");
    }

    /// <summary>
    /// 유저의 현재 Diamond 값을 읽어와 지정한 값만큼 감소시킨 뒤 저장하는 메서드
    /// </summary>
    /// <param name="addAmount">감소시킬 Diamond 양</param>
    /// <returns></returns>
    public async Task<bool> TrySubtractGoldAsync(int subAmount)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        var goldRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("Gold");

        DataSnapshot snapshot = await goldRef.GetValueAsync();

        int current = 0;

        if (snapshot.Exists && snapshot.Value != null)
        {
            current = Convert.ToInt32(snapshot.Value);
        }

        if (current - subAmount < 0)
        {
            Debug.LogWarning("골드 부족");
            return false;
        }

        int next = current - subAmount;

        await goldRef.SetValueAsync(next);
        Debug.Log($"감소한 Diamond: {subAmount} -> {next}");
        return true;
    }

    /// <summary>
    /// 유저의 현재 Diamond 값을 읽어와 지정한 값만큼 감소시킨 뒤 저장하는 메서드
    /// </summary>
    /// <param name="addAmount">감소시킬 Diamond 양</param>
    /// <returns></returns>
    public async Task<bool> TrySubtractDiamondAsync(int subAmount)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        var diaRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("Diamond");

        DataSnapshot snapshot = await diaRef.GetValueAsync();

        int current = 0;

        if (snapshot.Exists && snapshot.Value != null)
        {
            current = Convert.ToInt32(snapshot.Value);
        }

        if (current - subAmount < 0)
        {
            Debug.LogWarning("다이아 부족");
            return false;
        }

        int next = current - subAmount;

        await diaRef.SetValueAsync(next);
        Debug.Log($"감소한 Diamond: {subAmount} -> {next}");
        return true;
    }

    /// <summary>
    /// 9/29 이형원 추가
    /// 유저의 현재 캐릭터 신화석 값을 읽어와 지정한 값만큼 증가시킨 뒤 저장하는 메서드
    /// </summary>
    /// <param name="subAmount"></param>
    /// <returns></returns>
    public async Task AddMythStoneAsync(int subAmount)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        var diaRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("MythStone");

        DataSnapshot snapshot = await diaRef.GetValueAsync();

        int current = 0;

        if (snapshot.Exists && snapshot.Value != null)
        {
            current = Convert.ToInt32(snapshot.Value);
        }

        int next = current + subAmount;

        await diaRef.SetValueAsync(next);
        Debug.Log($"감소한 Diamond: {subAmount} -> {next}");
    }

    /// <summary>
    /// 9/29 이형원 추가
    /// 유저의 현재 캐릭터 신화석 값을 읽어와 지정한 값만큼 감소시킨 뒤 저장하는 메서드
    /// </summary>
    /// <param name="subAmount"></param>
    /// <returns></returns>
    public async Task SubtractMythStoneAsync(int subAmount)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        var diaRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("MythStone");

        DataSnapshot snapshot = await diaRef.GetValueAsync();

        int current = 0;

        if (snapshot.Exists && snapshot.Value != null)
        {
            current = Convert.ToInt32(snapshot.Value);
        }

        int next = current - subAmount;

        await diaRef.SetValueAsync(next);
        Debug.Log($"감소한 Diamond: {subAmount} -> {next}");
    }

    #endregion


    #region Stamina

    /// <summary>
    /// 스테미나 데이터를 Firebase에 저장
    /// </summary>
    /// <param name="stamina">현재 스테미나</param>
    /// <param name="lastRecoveryTime">마지막 회복 시간</param>
    public async Task<bool> SaveStaminaAsync(int stamina, long lastRecoveryTime)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;

        Dictionary<string, object> dictionary = new Dictionary<string, object>();
        dictionary[$"UserData/{uid}/Stamina"] = stamina;
        dictionary[$"UserData/{uid}/LastStaminaRecoveryTime"] = lastRecoveryTime;

        var task = FirebaseManager.DataReference.UpdateChildrenAsync(dictionary);
        await task;

        if (task.IsCompletedSuccessfully)
        {
            Debug.Log($"스테미나 저장 성공: {stamina}, LastRecovery: {lastRecoveryTime}");
            return true;
        }
        else
        {
            Debug.LogError("스테미나 저장 실패");
            return false;
        }
    }

    /// <summary>
    /// 스테미나를 소모하는 메서드
    /// </summary>
    /// <param name="amount">소모할 스테미나 양</param>
    /// <returns>소모 성공 여부</returns>
    public async Task<bool> ConsumeStaminaAsync(int amount)
    {
        // 먼저 현재 스테미나 상태를 확인하고 업데이트
        PlayerData currentData = await LoadLobbyDataAsync();

        if (currentData.Stamina < amount)
        {
            Debug.LogWarning($"스테미나 부족: 현재 {currentData.Stamina}, 필요 {amount}");
            return false;
        }

        int newStamina = currentData.Stamina - amount;
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        bool saveResult = await SaveStaminaAsync(newStamina, currentTime);

        if (saveResult)
        {
            Debug.Log($"스테미나 소모 완료: {amount} (남은 스테미나: {newStamina})");
        }

        return saveResult;
    }

    /// <summary>
    /// 시간 기반으로 스테미나를 회복하는 메서드
    /// 상수 값 변경하여 커스터마이징 가능
    /// MAX_STAMINA 변경되거나
    /// RECOVERY_INTERVAL_SECONDS 에서 그그그 지금 메서드 에서 변경가능, 지금은 60초도 두었지만 recovery interval 조정을 통해 변경해도됨.
    /// </summary>
    /// <param name="currentStamina">현재 스테미나</param>
    /// <param name="lastRecoveryTime">마지막 회복 시간</param>
    /// <returns>회복된 스테미나 데이터</returns>
    public (int recoveredStamina, long newRecoveryTime) CalculateStaminaRecovery(int currentStamina,
        long lastRecoveryTime)
    {
        const int MAX_STAMINA = 30;
        const int RECOVERY_INTERVAL_SECONDS = 60;

        if (currentStamina >= MAX_STAMINA)
        {
            return (currentStamina, lastRecoveryTime);
        }

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long timeDifference = currentTime - lastRecoveryTime;

        // 1분마다 1씩 회복
        int recoveryAmount = (int)(timeDifference / RECOVERY_INTERVAL_SECONDS);

        if (recoveryAmount > 0)
        {
            int newStamina = Mathf.Min(currentStamina + recoveryAmount, MAX_STAMINA);
            long newRecoveryTime = lastRecoveryTime + (recoveryAmount * RECOVERY_INTERVAL_SECONDS);

            Debug.Log($"스테미나 회복: {currentStamina} → {newStamina} (+{recoveryAmount})");

            return (newStamina, newRecoveryTime);
        }

        return (currentStamina, lastRecoveryTime);
    }

    /// <summary>
    /// 다음 스테미나 회복까지 남은 시간을 계산하는 메서드
    /// </summary>
    /// <param name="lastRecoveryTime">마지막 회복 시간</param>
    /// <returns>남은 시간</returns>
    public int GetTimeUntilNextStaminaRecovery(long lastRecoveryTime)
    {
        const int RECOVERY_INTERVAL_SECONDS = 60;

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long timeSinceLastRecovery = currentTime - lastRecoveryTime;
        long timeUntilNext = RECOVERY_INTERVAL_SECONDS - (timeSinceLastRecovery % RECOVERY_INTERVAL_SECONDS);

        return (int)timeUntilNext;
    }

    #endregion


    #region Mail

    /// <summary>
    /// Firebase 서버 시간을 읽어오는 메서드
    /// RTDB - ServerTime에 현재 시간 저장 및 로드
    /// currentTime -> DateTime: DateTimeOffset.FromUnixTimeMilliseconds(currentTime).UtcDateTime.ToLocalTime()
    /// </summary>
    /// <returns>현재 시간</returns>
    public async Task<long> LoadSeverTimeAsync()
    {
        DatabaseReference timeRef = FirebaseManager.DataReference.Child("ServerTime");

        await FirebaseManager.DataReference.Child("ServerTime").SetValueAsync(ServerValue.Timestamp);

        DataSnapshot snapShot = await timeRef.GetValueAsync();

        long currentTime = snapShot.Exists ? Convert.ToInt64(snapShot.Value) : 0;

        return currentTime;
    }

    /// <summary>
    /// 로그인 시 유저 메일함을 최신화하는 메서드
    /// </summary>
    /// <returns></returns>
    public async Task SyncMailsOnLoginAsync()
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;

        // 1) User메일 DB에서 이미 있는 mailId 체크
        DataSnapshot userMailSnapShot = await FirebaseDatabase.DefaultInstance
            .RootReference.Child("UserData").Child(uid).Child("MailData").GetValueAsync();
        List<string> existingIdList = new List<string>();
        
        if (userMailSnapShot.Exists)
        {
            foreach (var child in userMailSnapShot.Children)
            {
                existingIdList.Add(child.Key);
                Debug.Log($"현재 유저가 가지고 있는 메일ID : Mail_{child.Key}");
            }
        }

        // 2) Master메일 DB에서 IsSent = true인 메일만 필터링
        Query sentMail = FirebaseDatabase.DefaultInstance
            .GetReference("MailBox/MailData").OrderByChild("IsSent").EqualTo(true);

        DataSnapshot snapShot = await sentMail.GetValueAsync();

        // 3) ExpireDate > currentTime 값이 큰 메일만 필터링
        long currentTime = await Manager.DB.LoadSeverTimeAsync();

        foreach (var mail in snapShot.Children)
        {
            string mailId = mail.Key;

            // 이미 유저가 있는 메일 -> 스킵
            if (existingIdList.Contains(mailId))
            {
                Debug.Log($"유저 DB에 Mail_{mailId} 존재 / 저장 스킵");
                continue;
            }

            string expireDateStr = mail.Child("ExpireDate").Value?.ToString();

            // 타입 변환 1.string -> DateTime   2. DateTime -> UnixTimeMillis(long)
            DateTime expireDateDT = DateTime.Parse(expireDateStr);

            long offset = (long)DateTimeOffset.Now.Offset.TotalMilliseconds;
            long expireDate = new DateTimeOffset(expireDateDT).ToUnixTimeMilliseconds() - offset;

            // 디버깅용
            DateTime expireDateUTC = DateTimeOffset.FromUnixTimeMilliseconds(expireDate).UtcDateTime;

            // 3. 서버 현재시간과 비교
            if (expireDate > currentTime)
            {
                Debug.Log($"우편_ {mail.Key} 사용 가능 (SendDate / ExpireDateUTC = {expireDateStr}/{expireDateUTC}, currentTimeServer/currentTimeLocal = {currentTime}/{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()} )");

                // 유저DB 등록
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    [$"UserData/{uid}/MailData/{mailId}/ReceivedDate"] = currentTime,
                    [$"UserData/{uid}/MailData/{mailId}/IsReceived"] = false
                };

                await FirebaseDatabase.DefaultInstance.RootReference.UpdateChildrenAsync(updates);
            }
            else
            {
                Debug.Log($"우편_ {mail.Key} 만료 (SendDate = {expireDateStr}/{expireDate}, currentTime = {currentTime})");
            }
        }
    }

    /// <summary>
    /// 유저 MailData-MailID를 로드하는 메서드 
    /// 각 메일의 MasterMail과 매칭해 MailData 리스트 생성
    /// </summary>
    /// <returns>유저가 가진 메일 정보를 담은 MailData 리스트</returns>
    public async Task<List<MailData>> LoadUserMailsAsync()
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        List<MailData> userMailList = new List<MailData>();

        DataSnapshot userSnapShot = await FirebaseDatabase.DefaultInstance.RootReference
            .Child("UserData").Child(uid).Child("MailData").GetValueAsync();

        // 유저 메일 존재x -> 빈 리스트 반환
        if (!userSnapShot.Exists) return userMailList;

        // 1) 유저 메일 DB - MailId 리스트 
        List<string> userMailIdList = new List<string>();

        foreach (var child in userSnapShot.Children)
        {
            userMailIdList.Add(child.Key);
        }

        // 2) 각 MailId의 MasterMail 로드
        List<Task<DataSnapshot>> masterMailList = new List<Task<DataSnapshot>>(userMailIdList.Count);

        foreach (var id in userMailIdList)
        {
            masterMailList.Add(FirebaseDatabase.DefaultInstance.RootReference
                .Child("MailBox").Child("MailData").Child(id).GetValueAsync());
        }

        DataSnapshot[] masterSnapShot = await Task.WhenAll(masterMailList);

        // 3) 유저가 가진 MailId - MasterMail snapshot 매칭
        Dictionary<string, DataSnapshot> userMailDict = new Dictionary<string, DataSnapshot>(userMailIdList.Count);
        bool isCoupon = false;
        
        for (int i = 0; i < userMailIdList.Count; i++)
        {
            string mailId = userMailIdList[i];
            DataSnapshot snapShot = masterSnapShot[i];
            
            // MailBox/MailData 조회 실패 -> CouponData 재조회
            if (!snapShot.Exists)
            {
                snapShot = await FirebaseDatabase.DefaultInstance
                    .RootReference.Child("MailBox").Child("CouponData").Child(mailId).GetValueAsync();
                isCoupon = true;
            }

            userMailDict[mailId] = snapShot;
        }

        // 4) 유저 메일 정보 + MasterData -> MailData List 생성
        foreach (var child in userSnapShot.Children)
        {
            string mailId = child.Key;
            userMailDict.TryGetValue(mailId, out var masterMail);

            // --- User Data ---
            long receivedDate = 0;
            long.TryParse(child.Child("ReceivedDate").Value?.ToString(), out receivedDate);

            bool isReceived = false;
            bool.TryParse(child.Child("IsReceived").Value?.ToString(), out isReceived);

            // --- Master Data ---
            string title = masterMail?.Child("Title").Value?.ToString() ?? "LoadFailed";
            string body = masterMail?.Child("Body").Value?.ToString() ?? "LoadFailed";

            int gold = 0;
            int.TryParse(masterMail?.Child("Gold").Value?.ToString(), out gold);

            int diamond = 0;
            int.TryParse(masterMail.Child("Diamond").Value?.ToString(), out diamond);

            string expireDateStr = masterMail?.Child("ExpireDate").Value?.ToString();
            DateTime expireDateDT = DateTime.Parse(expireDateStr);
            long offset = (long)DateTimeOffset.Now.Offset.TotalMilliseconds;
            long expireDate = new DateTimeOffset(expireDateDT).ToUnixTimeMilliseconds() - offset;

            // 쿠폰인 경우 ExpireDate = 수령일로부터 30일
            if (isCoupon)
            {
                expireDate = receivedDate + (30L * 24 * 60 * 60 * 1000);
            }

            userMailList.Add(new MailData
            {
                MailId = mailId,
                Title = title,
                Body = body,
                Gold = gold,
                Diamond = diamond,
                ReceivedDate = receivedDate,
                ExpireDate = expireDate,
                IsReceived = isReceived,
                IsCoupon = isCoupon
            });
        }
        return userMailList;
    }

    // 보상 수령
    public Task SetMailIsReceivedAsync(string mailId, bool value)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        return FirebaseDatabase.DefaultInstance.RootReference
            .Child("UserData").Child(uid).Child("MailData").Child(mailId).Child("IsReceived").SetValueAsync(value);
    }

    // 기간 만료
    public Task SetMailIsExpiredAsync(string mailId, bool value)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        return FirebaseDatabase.DefaultInstance.RootReference
            .Child("UserData").Child(uid).Child("MailData").Child(mailId).Child("IsExpired").SetValueAsync(value);
    }

    // 메일 삭제
    public Task DeleteMailAsync(string mailId)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        return FirebaseDatabase.DefaultInstance.RootReference
            .Child("UserData").Child(uid).Child("MailData").Child(mailId).RemoveValueAsync();
    }

    #endregion


    #region Coupon

    /// <summary>
    /// 쿠폰코드 검증 및 유저 우편함에 쿠폰을 등록하는 메서드
    /// </summary>
    /// <param name="couponCode">유저가 입력한 쿠폰 코드</param>
    /// <returns></returns>
    public async Task CheckCouponDataAsync(string couponCode)
    {
        string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        // 1) Master DB에 쿠폰 코드 정보 유무 체크 
        var couponSnapshot = await FirebaseDatabase.DefaultInstance.RootReference
            .Child("MailBox").Child("CouponData").Child(couponCode).GetValueAsync();

        if (!couponSnapshot.Exists)
        {
            Debug.LogWarning("쿠폰 코드 정보x");
            return;
        }
        
        string couponId = couponCode;

        // 2) 쿠폰 만료기간 체크
        string couponExpireDateStr = couponSnapshot.Child("ExpireDate").Value?.ToString();
        DateTime couponExpireDateDT = DateTime.Parse(couponExpireDateStr);
        long offset = (long)DateTimeOffset.Now.Offset.TotalMilliseconds;
        long expireDate = new DateTimeOffset(couponExpireDateDT).ToUnixTimeMilliseconds() - offset;
        long currentTime = await LoadSeverTimeAsync();
        
        if (expireDate < currentTime)
        {
            Debug.LogWarning($"CouponCode : {couponId} 기간 만료");
            return;
        }

        // 3) User메일 DB에서 이미 있는 쿠폰 체크
        DataSnapshot userCouponSnapShot = await FirebaseDatabase.DefaultInstance.RootReference
            .Child("UserData").Child(uid).Child("MailData").GetValueAsync();
        
        List<string> existingCouponList = new List<string>();
        
        if (userCouponSnapShot.Exists)
        {
            foreach (var child in userCouponSnapShot.Children)
            {
                existingCouponList.Add(child.Key);
                if (child.Key == couponId)
                {
                    Debug.Log($"CouponCode : {child.Key} 사용 완료된 쿠폰");
                    return;
                }
            }
        }

        // 4) 유저DB 등록
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            [$"UserData/{uid}/MailData/{couponId}/ReceivedDate"] = currentTime,
            [$"UserData/{uid}/MailData/{couponId}/IsReceived"] = false,
        };

        await FirebaseDatabase.DefaultInstance.RootReference.UpdateChildrenAsync(updates);
        Debug.Log($"쿠폰 등록 성공: {couponId} / 메일 생성 완료");
    }

    #endregion
}