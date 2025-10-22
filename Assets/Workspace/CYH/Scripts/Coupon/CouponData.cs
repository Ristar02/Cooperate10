using System;
using UnityEngine;

[Serializable]
public class CouponData
{
    public string MailId;
    public string Title;
    public string Body;
    public int Gold;
    public int Diamond;
    public long ReceivedDate;
    public long ExpireDate;
    public bool IsReceived;

    public bool IsExpired(long currentTime)
    {
        return ExpireDate > 0 && ExpireDate <= currentTime;
    }
}