using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase.Database;

public class PlayerMailBoxController : MonoBehaviour
{
    private List<MailData> _mail;
    private List<CouponData> _coupon;
    private DatabaseReference _mailRef;

    public event Action<List<MailData>> OnMailboxUpdated;
    public event Action<List<CouponData>> OnCouponUpdated;
    public List<MailData> Mail { get { return _mail; } }
    public List<CouponData> Coupon { get { return _coupon; } }


    private void Start()
    {
        InitAsync();
    }

    private void OnEnable()
    {
        StartListeningToMailData();
    }

    private void OnDisable()
    {
        StopListeningToMailData();
    }

    private async void InitAsync()
    {
        // TODO: [CYH] 로그인 씬 호출
       await Manager.DB.SyncMailsOnLoginAsync();

        List<MailData> userMailDB = await LoadAsync();
        RefreshUI(userMailDB);
        Debug.Log("[PlayerMailBoxController] InitAsync");
    }

    private void RefreshUI(List<MailData> mailList)
    {
        Debug.Log("[PlayerMailBoxController] RefreshUI");
        _mail = mailList;

        // ReceivedDate 기준 내림차순 정렬
        _mail.Sort((a, b) => b.ReceivedDate.CompareTo(a.ReceivedDate));
        OnMailboxUpdated?.Invoke(_mail);
    }

    private async Task<List<MailData>> LoadAsync()
    {
        // DB에서 UserMail + MasterMail 리스트 로드
        List<MailData> mails = await Manager.DB.LoadUserMailsAsync();
        return mails;
    }

    /// <summary>
    /// 강제 새로고침
    /// </summary>
    public async Task RefreshAsync()
    {
        List<MailData> loaded = await LoadAsync();
        RefreshUI(loaded);
    }

    /// <summary>
    /// 입력된 쿠폰코드 검증 및 우편 전송하는 메서드
    /// </summary>
    /// <param name="couponCode"></param>
    public async void CheckCouponCode(string couponCode)
    {
        await Manager.DB.CheckCouponDataAsync(couponCode);
    }

    /// <summary>
    ///  미수령 보상 지급 후 DB에 연동 및 삭제하는 메서드 
    /// </summary>
    /// <param name="mailId">보상 받을 메일 ID</param>
    public async Task ReceiveRewardAsync(string mailId)
    {
        MailData mail = _mail?.Find(m => m.MailId == mailId);

        if(mail == null)
        {
            List<MailData> loadedMail = await LoadAsync();
            mail = loadedMail?.Find(m => m.MailId == mailId);

            if (mail == null)
            {
                Debug.LogWarning($"[ReceiveReward] mail == null : {mailId}");
                return;
            }
        }

        // IsReceived == false일 때 보상 지급
        if (!mail.IsReceived)
        {
            Task goldTask = Manager.DB.AddGoldAsync(mail.Gold);
            Task diaTask = Manager.DB.AddDiamondAsync(mail.Diamond);
            await Task.WhenAll(goldTask, diaTask);
        }

        await Manager.DB.SetMailIsReceivedAsync(mailId, true);
        
        // 메일 삭제
        //await DeleteMail(mailId);
    }

    /// <summary>
    /// 미수령 우편을 모두 수령하는 메서드
    /// </summary>
    /// <returns>수령할 총 골드/다이아</returns>
    public async Task<(int totalGold, int totalDiamond)> ReceiveAllAsync()
    {
        int totalGold = 0;
        int totalDiamond = 0;
        
        if (_mail == null || _mail.Count == 0) return (0, 0);

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        List<MailData> unReceivedMailList = _mail.FindAll(m => !m.IsReceived && !m.IsExpired(currentTime));
        
        foreach (var mail in unReceivedMailList)
        {
            totalGold += mail.Gold;
            totalDiamond += mail.Diamond;
        }

        if (unReceivedMailList.Count == 0) return (0, 0);

        // TODO: [CYH] 일괄 수령 리팩토링
        foreach (MailData mail in unReceivedMailList)
        {
            Debug.Log($"ReceiveRewardAsync 실행");
            await ReceiveRewardAsync(mail.MailId);

            await Task.Yield();
        }

        await RefreshAsync();
        return (totalGold, totalDiamond);
    }

    /// <summary>
    /// 유저 메일 DB - mailId의 IsExpired = false로 변경하는 메서드
    /// </summary>
    /// <param name="mailId">메일 ID</param>
    public async void SetIsExpired(string mailId)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        await Manager.DB.SetMailIsExpiredAsync(mailId, false);
    }

    /// <summary>
    /// 유저 메일 DB에서 해당 mailId를 삭제하는 메서드
    /// </summary>
    /// <param name="mailId">메일 ID</param>
    public async Task DeleteMail(string mailId)
    {
        await Manager.DB.DeleteMailAsync(mailId);
    }

    private void StartListeningToMailData()
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        _mailRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("MailData");

        _mailRef.ValueChanged += OnMailChanged;
    }

    private void StopListeningToMailData()
    {
        _mailRef.ValueChanged -= OnMailChanged;
    }

    private async void OnMailChanged(object sender, ValueChangedEventArgs changeEvent)
    {
        if (changeEvent.DatabaseError != null)
        {
            Debug.LogError($"[Mail DB Error] {changeEvent.DatabaseError.Message}");
            return;
        }

        List<MailData> mailList = await Manager.DB.LoadUserMailsAsync();
        Debug.Log("[PlayerMailBoxController] OnMailChanged 실행");

        var loaded = await LoadAsync();
        RefreshUI(loaded);
    }
}