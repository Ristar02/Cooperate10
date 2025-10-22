using System;

[Serializable]
public class MailData
{
    public string MailId;
    public string Title;
    public string Body;
    public int Gold;
    public int Diamond;
    public long ReceivedDate;   
    public long ExpireDate;     
    public bool IsReceived;
    public bool IsCoupon;

    public bool IsExpired(long currentTime)
    {
        return ExpireDate > 0 && ExpireDate <= currentTime;
    }
}
