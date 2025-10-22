using UnityEngine;
using UnityEngine.UI;

public class LinkedIn : MonoBehaviour
{
    [SerializeField] private Button termsButton;
    [SerializeField] private Button privacyButton;

    private const string TermsUrl = "https://sites.google.com/kiweb.or.kr/terms";
    private const string PrivacyUrl = "https://sites.google.com/kiweb.or.kr/terms-privacy/";

    void Awake()
    {
        termsButton.onClick.AddListener(() => OpenUrlSafe(TermsUrl));
        privacyButton.onClick.AddListener(() => OpenUrlSafe(PrivacyUrl));
    }

    private void OpenUrlSafe(string url)
    {
        Application.OpenURL(url);
    }
}