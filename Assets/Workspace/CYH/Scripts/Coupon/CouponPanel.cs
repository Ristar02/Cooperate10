using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CouponPanel : MonoBehaviour
{
    [Header("Controller")]
    [SerializeField] private PlayerMailBoxController _controller;

    [Header("UI")]
    [SerializeField] TMP_InputField _couponInputField;
    [SerializeField] Button _confirmButton;


    private void Start()
    {
        _confirmButton.onClick.AddListener(OnSubmitCoupon);
    }

    private void OnSubmitCoupon()
    {
        string code = _couponInputField.text.Trim();
        Debug.Log($"입력된 쿠폰코드 : {code}");

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("쿠폰 코드 입력x");
            return;
        }

        _controller.CheckCouponCode(code);
        _couponInputField.text = "";
    }
}