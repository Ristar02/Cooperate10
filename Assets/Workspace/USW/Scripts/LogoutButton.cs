using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;

public class LogoutButton : MonoBehaviour
{
    [SerializeField] private Button _logoutButton;

    private void Reset()
    {
        if (_logoutButton == null)
            _logoutButton = GetComponent<Button>();
    }

    private void Awake()
    {
        if (_logoutButton == null)
        {
            return;
        }

        _logoutButton.onClick.AddListener(OnClick_Logout);
    }

    private void OnDestroy()
    {
        if (_logoutButton != null)
            _logoutButton.onClick.RemoveListener(OnClick_Logout);
    }

    private void OnClick_Logout()
    {
        var auth = FirebaseManager.Auth;
        if (auth == null)
        {
            return;
        }

        if (auth.CurrentUser == null)
        {
            return;
        }

        string uid = auth.CurrentUser.UserId;
        string name = auth.CurrentUser.DisplayName;

        // 로그아웃
        auth.SignOut();

        Debug.Log($"로그아웃 완료 - 이전 UID={uid}, 닉네임={name}");
    }
}