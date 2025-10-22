using UnityEngine;
using UnityEngine.UI;

public class LobbyPopupManager : MonoBehaviour
{
    [SerializeField] private PlayerDataController _playerDataController;
    [SerializeField] private PlayerMailBoxController _playerMailBoxController;
    [SerializeField] private GoogleAdMob _googleAdMob;
    
    [Header("PlayerProfile")]
    [SerializeField] private GameObject _playerProfilePopup;
    [SerializeField] private PlayerProfilePopup _profilePopup;
    [SerializeField] private Button _playerProfileButton;

    [Header("Ad")]
    [SerializeField] private Button _adButton;

    [Header("AccountLink")]
    [SerializeField] private GameObject _googleLinkPopup;
    [SerializeField] private Button _accountLinkButton;

    [Header("MailBox")]
    [SerializeField] private GameObject _mailboxPopup;
    [SerializeField] private MailBoxPopup _mailPopup;
    [SerializeField] private Button _mailboxButton;

    [Header("Coupon")]
    [SerializeField] private GameObject _couponPopup;
    [SerializeField] private CouponPanel _couponPopupCs;
    [SerializeField] private Button _couponButton;


    private void Start()
    {
        _playerProfileButton.onClick.AddListener(() =>
        {
            ShowPopup_playerProfile();
        });

        _adButton.onClick.AddListener(() =>
        {
            Manager.Popup.ShowConfirmationPopup(
                "Watch Ad",
                () => _googleAdMob.ShowAd(),
                () => gameObject.SetActive(false));
        });

        _accountLinkButton.onClick.AddListener(() =>
        {
            _googleLinkPopup.SetActive(true);
        });

        _mailboxButton.onClick.AddListener(() =>
        {
            ShowPopup_mail();
        });

        _couponButton.onClick.AddListener(() =>
        {
            ShowPopup_coupon();
        });
    }

    private void ShowPopup_playerProfile()
    {
        HideAllPopup();

        if (_playerProfilePopup != null)
        {
            _profilePopup.Init(_playerDataController.Data);
            _profilePopup.EnableDataBind(true);
            _playerProfilePopup.SetActive(true);
        }
    }

    private void ShowPopup_mail()
    {
        HideAllPopup();

        if (_mailboxPopup != null)
        {
            _mailPopup.Init(_playerMailBoxController.Mail);
            _mailPopup.EnableDataBind(true);
            _mailboxPopup.SetActive(true);
        }
    }

    private void ShowPopup_coupon()
    {
        HideAllPopup();

        if (_couponPopup != null)
        {
            _couponPopup.SetActive(true);
        }
    }

    private void HideAllPopup()
    {
        _playerProfilePopup.SetActive(false);
        _googleLinkPopup.SetActive(false);
        _mailboxPopup.SetActive(false);
        _couponPopup.SetActive(false);
    }
}
