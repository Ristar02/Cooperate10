using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseButton : MonoBehaviour
{
    public void Close()
    {
        // 버튼이 속한 부모 패널 찾기
        GameObject panel = transform.parent.gameObject;
        panel.SetActive(false);
    }
}
