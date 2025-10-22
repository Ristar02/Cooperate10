using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

[Serializable]
public class ShopCSVData
{
    public int ItemID;
    public string ItemName;
    public float ItemProb;
    public int GoldPrice;
    public int DiamondPrice;
}

public class DailyShopCSV
{
    private string _url;

    public DailyShopCSV(string url)
    {
        _url = url;
    }

    public async UniTask<List<ShopCSVData>> LoadAsync(int minLine = 1, int maxLine = int.MaxValue)
    {
        using UnityWebRequest request = UnityWebRequest.Get(_url);
        await request.SendWebRequest().ToUniTask();

        if (!string.IsNullOrEmpty(request.error))
        {
            Debug.LogError($"CSV 다운로드 실패: {request.error}");
            return null;
        }

        string raw = request.downloadHandler.text.Trim();
        string[] lines = raw.Split('\n');

        List<ShopCSVData> items = new List<ShopCSVData>();

        for (int i = minLine; i < lines.Length && i <= maxLine; i++)
        {
            string[] row = lines[i].Trim().Split(',');

            if (row.Length <= 10) continue;

            ShopCSVData data = new ShopCSVData
            {
                ItemID = row.Length > 2 && int.TryParse(row[2], out var itemId) ? itemId : 0,
                ItemName = row.Length > 3 ? row[3] : string.Empty,
                ItemProb = row.Length > 5 && float.TryParse(row[5], out var prob) ? prob : 0f,
                GoldPrice = row.Length > 8 && int.TryParse(row[8], out var gold) ? gold : 0,
                DiamondPrice = row.Length > 10 && int.TryParse(row[10], out var diamond) ? diamond : 0
            };

            items.Add(data);
        }

        return items;
    }

    /// <summary>
    /// 로드된 ShopItemData 리스트를 로그로 출력
    /// </summary>
    public static void PrintItems(List<ShopCSVData> items)
    {
        if (items == null || items.Count == 0)
        {
            Debug.LogWarning("상점 아이템 데이터 x");
            return;
        }

        foreach (var item in items)
        {
            Debug.Log($"[ShopItem] ID:{item.ItemID}, " +
                      $"ItemName:{item.ItemName}, " +
                      $"Currency:{item.GoldPrice}, " +
                      $"Currency:{item.DiamondPrice}, " +
                      $"ItemProb:{item.ItemProb}, ");
        }
    }
}