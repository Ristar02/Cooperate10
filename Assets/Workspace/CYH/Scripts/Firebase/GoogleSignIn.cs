using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Extensions;
using Google;

public class GoogleSignin : MonoBehaviour
{
    [SerializeField] private Button _googleLoginButton;


    private void Start()
    {
        _googleLoginButton.onClick.AddListener(OnGoogleSignInClicked);
    }

    /// <summary>
    /// 구글 로그인 버튼 클릭 시 호출되는 메서드
    /// GoogleSignIn 설정 초기화 및 인증 
    /// 인증 결과는 OnGoogleAuthenticatedFinished() 메서드에서 처리
    /// </summary>
    public void OnGoogleSignInClicked()
    {
        Debug.Log("구글 로그인 버튼 입력");
        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnGoogleAuthenticatedFinished);
    }

    /// <summary>
    /// 구글 인증 완료 후 호출되는 콜백 메서드
    /// 인증 실패: 에러 코드
    /// 인증 성공: GoogleSignUp 메서드 호출
    /// </summary>
    /// <param name="task">구글 인증 결과 Task</param>
    private void OnGoogleAuthenticatedFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsCanceled)
        {
            Debug.LogError("구글 인증 취소");
        }

        if (task.IsFaulted)
        {
            Debug.LogError($"구글 인증 실패 : {task.Exception}");
        }

        else
        {
            GoogleSignUp(task);
        }
    }

    /// <summary>
    /// 구글 인증 완료 후 Credential 생성
    /// Firebase에 로그인/회원가입 시도하는 메서드
    /// </summary>
    /// <param name="userTask">구글 로그인 결과 Task</param>
    private void GoogleSignUp(Task<GoogleSignInUser> userTask)
    {
        Firebase.Auth.Credential credential =
        Firebase.Auth.GoogleAuthProvider.GetCredential(userTask.Result.IdToken, null);
        FirebaseManager.Auth.SignInAndRetrieveDataWithCredentialAsync(credential)
            .ContinueWithOnMainThread(async task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError($"구글 로그인 실패 / 원인: {task.Exception}");
                    return;
                }

                Firebase.Auth.AuthResult result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);

                // 구글 로그인한 계정을 currentUser로 설정
                FirebaseUser currentUser = FirebaseManager.Auth.CurrentUser;

                await DBManager.Instance.SaveNicknameAsync();
                await currentUser.ReloadAsync();

                if (currentUser != null)
                {
                    Debug.Log("구글 로그인 성공");
                }
            });
    }
}
