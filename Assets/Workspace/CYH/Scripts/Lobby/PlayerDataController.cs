using UnityEngine;
using Firebase.Database;
using System;
using System.Collections;

public class PlayerDataController : MonoBehaviour
{
    [SerializeField] private PlayerDataView _view;
 
    private DatabaseReference _userRef;
    private PlayerData _data;

    public PlayerData Data { get { return _data; } }

    public Action<PlayerData> OnPlayerProfilePopupUpdated;
    public Action<PlayerData> OnUpdateUI;
    
    public Action<int> OnStaminaRecoveryTimer; 
    
    // 스테미나 타이머 관련
    private Coroutine _staminaTimer;
    private float _nextStaminaRecoveryTime;

    private void Start()
    {
        InitAsync();
        OnUpdateUI += _view.UpdateUI;
    }

    private void OnEnable()
    {
        StartListeningToPlayerData();
        StartStaminaTimer();
    }

    private void OnDisable()
    {
        StopListeningToPlayerData();
        StopStaminaTimer();
    }

    public async void InitAsync()
    {
        _data = await DBManager.Instance.LoadLobbyDataAsync();
        _view.UpdateUI(_data);
        RefreshUI(_data);
        UpdateStaminaTimer();
    }

    public void RefreshUI(PlayerData data)
    {
        _data = data;
        OnUpdateUI?.Invoke(data);
        OnPlayerProfilePopupUpdated?.Invoke(data);
        UpdateStaminaTimer();
    }

    #region Stamina System

    private void StartStaminaTimer()
    {
        if (_staminaTimer != null)
        {
            StopCoroutine(_staminaTimer);
        }
        _staminaTimer = StartCoroutine(StaminaTimerCoroutine());
    }

    private void StopStaminaTimer()
    {
        if (_staminaTimer != null)
        {
            StopCoroutine(_staminaTimer);
            _staminaTimer = null;
        }
    }

    private void UpdateStaminaTimer()
    {
        if (_data != null && _data.Stamina < _data.MaxStamina)
        {
            int timeUntilNext = DBManager.Instance.GetTimeUntilNextStaminaRecovery(_data.LastStaminaRecoveryTime);
            _nextStaminaRecoveryTime = timeUntilNext;
        }
        else
        {
            _nextStaminaRecoveryTime = 0;
        }
    }

    private IEnumerator StaminaTimerCoroutine()
    {
        while (true)
        {
            if (_data != null && _data.Stamina < _data.MaxStamina)
            {
                // 매초마다 남은 시간 계산 및 UI 업데이트
                int timeUntilNext = DBManager.Instance.GetTimeUntilNextStaminaRecovery(_data.LastStaminaRecoveryTime);
                OnStaminaRecoveryTimer?.Invoke(timeUntilNext);
                
                if (timeUntilNext <= 1) 
                {
                    StartCoroutine(CheckAndRecoverStaminaCoroutine());
                }
            }
            else
            {
                OnStaminaRecoveryTimer?.Invoke(0);
            }
            
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator CheckAndRecoverStaminaCoroutine()
    {
        var (recoveredStamina, newRecoveryTime) = DBManager.Instance.CalculateStaminaRecovery(_data.Stamina, _data.LastStaminaRecoveryTime);
        
        if (recoveredStamina != _data.Stamina)
        {
            // 스테미나가 회복되었다면 DB에 저장하고 UI 업데이트
            var saveTask = DBManager.Instance.SaveStaminaAsync(recoveredStamina, newRecoveryTime);
            
            // Task가 완료될 때까지 대기
            while (!saveTask.IsCompleted)
            {
                yield return null;
            }
            
            if (saveTask.Result)
            {
                _data.Stamina = recoveredStamina;
                _data.LastStaminaRecoveryTime = newRecoveryTime;
                
                // UI 업데이트는 Firebase ValueChanged 이벤트에서 처리됨
                Debug.Log($"스테미나 자동 회복: {_data.Stamina}/{_data.MaxStamina}");
            }
        }
    }

    /// <summary>
    /// 스테미나를 소모하는 메서드 
    /// </summary>
    /// <param name="amount">소모할 스테미나 양</param>
    /// <returns>소모 성공 여부</returns>
    public async System.Threading.Tasks.Task<bool> UseStamina(int amount)
    {
        if (_data.Stamina < amount)
        {
            Debug.LogWarning($"스테미나 부족: 현재 {_data.Stamina}, 필요 {amount}");
            return false;
        }

        bool result = await DBManager.Instance.ConsumeStaminaAsync(amount);
        
        if (result)
        {
            Debug.Log($"스테미나 사용: {amount} (남은 스테미나: {_data.Stamina - amount})");
        }

        return result;
    }

    #endregion

    private void StartListeningToPlayerData()
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        _userRef = FirebaseManager.DataReference.Child("UserData").Child(uid);

        _userRef.ValueChanged += OnPlayerDataChanged;
    }

    private void StopListeningToPlayerData()
    {
        if (_userRef != null)
        {
            _userRef.ValueChanged -= OnPlayerDataChanged;
        }
    }

    private void OnPlayerDataChanged(object sender, ValueChangedEventArgs changeEvent)
    {
        if (changeEvent.DatabaseError != null)
        {
            Debug.LogError($"[DB ERROR] {changeEvent.DatabaseError.Message}");
            return;
        }

        DataSnapshot snapshot = changeEvent.Snapshot;
        
        Debug.Log("데이터 변경");

        PlayerData data = new PlayerData
        {
            PlayerUid = FirebaseManager.Auth.CurrentUser.UserId,
            PlayerName = snapshot.Child("Nickname").Value?.ToString() ?? "LoadFailed",
            Gold = int.TryParse(snapshot.Child("Gold").Value?.ToString(), out int gold) ? gold : 0,
            Diamond = int.TryParse(snapshot.Child("Diamond").Value?.ToString(), out int diamond) ? diamond : 0,
            Stamina = int.TryParse(snapshot.Child("Stamina").Value?.ToString(), out int stamina) ? stamina : 30,
            MaxStamina = 30,
            LastStaminaRecoveryTime = long.TryParse(snapshot.Child("LastStaminaRecoveryTime").Value?.ToString(), out long recoveryTime) ? recoveryTime : DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        Debug.Log($"[OnPlayerDataChanged] UI 업데이트 / UID: {FirebaseManager.Auth.CurrentUser.UserId}");

        _data = data;
        _view.UpdateUI(data);
        OnPlayerProfilePopupUpdated?.Invoke(data);
        UpdateStaminaTimer();
    }
}