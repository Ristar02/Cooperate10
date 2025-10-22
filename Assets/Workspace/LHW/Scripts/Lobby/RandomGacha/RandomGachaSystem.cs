using Firebase.Database;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class RandomGachaSystem : MonoBehaviour
{
    [Header("Reference")]
    // 캐릭터 가챠
    [SerializeField] private CharacterDatabase _charData;
    [SerializeField] private ItemProbabilitySO _charProb;
    // 마법석
    [SerializeField] private MagicStoneDatabase _stoneDatabase;
    [SerializeField] private MagicStoneGachaSO _stoneProb;
    // UI
    [SerializeField] private GachaResultUI _resultUI;

    [Header("GachaListUIButton")]
    [SerializeField] private Button _characterGachaButton;
    [SerializeField] private Button _stoneGachaButton;

    [Header("GachaListUI")]
    [SerializeField] private GameObject _characterGacha;
    [SerializeField] private GameObject _stoneGacha;

    [Header("CharacterGachaUI")]
    [SerializeField] private Button _dailyCharacterGachaButton;
    [SerializeField] private Button _oneCharacterGachaButton;
    [SerializeField] private Button _tenCharacterGachaButton;

    [Header("MagicStoneGachaUI")]
    [SerializeField] private Button _dailyStoneButton;
    [SerializeField] private Button _oneStoneGachaButton;
    [SerializeField] private Button _tenStoneGachaButton;

    // 확률 소수점 자릿수
    [Header("ProbOffset")]
    [SerializeField] private int digits = 2;

    [Header("DB Test")]
    [SerializeField] private UnitData _testData;
    [SerializeField] private Button _testCharacterGachaButton;

    // 캐릭터 확률
    private WeightedRandom<Grade> _gradeCharRandom = new WeightedRandom<Grade>();
    private WeightedRandom<int>[] _gradeCharPieceRandom = new WeightedRandom<int>[4];
    // 마법석 확률
    private WeightedRandom<MagicStoneRewardType> _magicStoneRewardRandom = new WeightedRandom<MagicStoneRewardType>();
    private WeightedRandom<int> _magicStonePieceRandom = new WeightedRandom<int>();

    private void Awake()
    {
        Init();
    }

    #region Init

    private void Init()
    {
        // 확률 테이블 초기화
        CharRandomInit(_charProb);
        MagicStoneRandomInit(_stoneProb);

        // 가챠 종류 전환용 버튼 이벤트
        _characterGachaButton.onClick.AddListener(() => SetActivePanel("CharacterGacha"));
        _stoneGachaButton.onClick.AddListener(() => SetActivePanel("MagicStoneGacha"));

        // 캐릭터 가챠에 대한 버튼 이벤트
        _dailyCharacterGachaButton.onClick.AddListener(CharAdButtonClick);
        _oneCharacterGachaButton.onClick.AddListener(CharOneButtonClick);
        _tenCharacterGachaButton.onClick.AddListener(() => ConsumeGoodsButtonClick(GachaType.Char, 10));

        // 마법석 가챠에 대한 버튼 이벤트
        _dailyStoneButton.onClick.AddListener(StoneAdButtonClick);
        _oneStoneGachaButton.onClick.AddListener(StoneOneButtonClick);
        _tenStoneGachaButton.onClick.AddListener(() => ConsumeGoodsButtonClick(GachaType.Stone, 10));

        // 테스트 기능
        _testCharacterGachaButton.onClick.AddListener(TestItemSelect);
    }

    /// <summary>
    /// 확률표 데이터(SO)를 바탕으로 캐릭터 확률 초기화.
    /// * 유의사항 - 확률표에서 소수점의 길이만큼 digits를 늘려줄 것.
    /// ex) 확률표상 소수점 네 자리(70.4356) -> digits : 최소 4 이상의 수 입력
    /// </summary>
    /// <param name="probability"></param>
    private void CharRandomInit(ItemProbabilitySO probability)
    {
        for (int i = 0; i < _gradeCharPieceRandom.Length; i++)
        {
            _gradeCharPieceRandom[i] = new WeightedRandom<int>();
        }

        for (int i = 0; i < probability.ItemsProbability.Count; i++)
        {
            Grade grade = probability.ItemsProbability[i].ItemGrade;
            int value = (int)(probability.ItemsProbability[i].Probability * Math.Pow(10, digits));
            _gradeCharRandom.Add(grade, value);

            for (int j = 0; j < probability.ItemsProbability[i].Pieces.Count; j++)
            {
                int pieceValue = (int)(probability.ItemsProbability[i].Pieces[j].PieceProbability * Math.Pow(10, digits));
                _gradeCharPieceRandom[i].Add(probability.ItemsProbability[i].Pieces[j].PieceNum, pieceValue);
            }
        }
    }

    private void MagicStoneRandomInit(MagicStoneGachaSO probability)
    {
        for (int i = 0; i < probability.rewards.Count; i++)
        {
            MagicStoneRewardType type = probability.rewards[i].rewardType;
            int value = (int)(probability.rewards[i].RewardProbability * Math.Pow(10, digits));
            _magicStoneRewardRandom.Add(type, value);
        }

        for (int i = 0; i < probability.rewards[0].PieceEntries.Count; i++)
        {
            int pieceNum = probability.rewards[0].PieceEntries[i].PieceNum;
            int pieceProbable = (int)(probability.rewards[0].PieceEntries[i].PieceProbability * Math.Pow(10, digits));

            _magicStonePieceRandom.Add(pieceNum, pieceProbable);
        }
    }

    #endregion

    #region Button Click Event

    #region GachaListButton

    /// <summary>
    /// 선택한 가챠 종류의 패널을 표시.
    /// </summary>
    /// <param name="activePanel"></param>
    private void SetActivePanel(string activePanel)
    {
        _characterGacha.SetActive(activePanel.Equals(_characterGacha.name));
        _stoneGacha.SetActive(activePanel.Equals(_stoneGacha.name));
    }

    #endregion

    #region CharacterGachaButton

    #region AdButton

    /// <summary>
    /// 광고 버튼 클릭 이벤트
    /// </summary>
    private void CharAdButtonClick()
    {
        // 일일 광고 가챠가 가능할 경우 실행
        if (DailyCharAdGacha()) return;

        // 광고 가챠가 불가능할 경우 경고 팝업
        if (PopupManager.Instance != null)
        {
            PopupManager.instance.ShowPopup("일일 광고 가챠를 전부 사용하였습니다.");
        }
    }

    /// <summary>
    /// 광고 가챠의 가능 여부를 판별하고 가능할 시 광고 시청 후 가챠를 진행
    /// </summary>
    /// <returns></returns>
    private bool DailyCharAdGacha()
    {
        // 광고 가챠가 가능할 때
        if (TimeManager.Instance.CanObtainAdGachaReward(GachaType.Char))
        {
            // 광고 가챠 쿨타임 업데이트
            TimeManager.Instance.UpdateAdGachaResetTimeInfo(GachaType.Char);
            // 1회 뽑기 진행
            CharacterSelect(1);
            TimeManager.Instance.OnDailyGachaInfoChanged?.Invoke();
            return true;
        }

        return false;
    }

    #endregion

    #region DailyButton

    /// <summary>
    /// 1회 뽑기 버튼 클릭 이벤트
    /// </summary>
    private void CharOneButtonClick()
    {
        // 일일 무료 뽑기가 가능할 때 해당 뽑기 우선 진행
        if (DailyCharFreeGacha()) return;

        ConsumeGoodsButtonClick(GachaType.Char, 1);
    }

    /// <summary>
    /// 일일 뽑기의 가능 여부를 판별하고, 가능할 시 횟수를 소모하고 진행
    /// </summary>
    /// <returns></returns>
    private bool DailyCharFreeGacha()
    {
        // 일일 무료 뽑기가 가능할 때
        if (TimeManager.Instance.CanObtainedFreeGachaReward(GachaType.Char))
        {
            // 1회 뽑기 진행
            CharacterSelect(1);
            // 일일 무료 뽑기 쿨타임 업데이트
            TimeManager.Instance.UpdateDailyFreeGachaResetTimeInfo(GachaType.Char);
            return true;
        }
        return false;
    }

    #endregion

    #endregion

    #region MagicStoneGachaButton

    #region AdButton

    /// <summary>
    /// 광고 버튼 클릭 이벤트
    /// </summary>
    private async void StoneAdButtonClick()
    {
        // 일일 광고 가챠가 가능할 경우 실행
        if (await DailyStoneAdGacha()) return;

        // 광고 가챠가 불가능할 경우 경고 팝업
        if (PopupManager.Instance != null)
        {
            PopupManager.instance.ShowPopup("일일 광고 가챠를 전부 사용하였습니다.");
        }
    }

    /// <summary>
    /// 광고 가챠의 가능 여부를 판별하고 가능할 시 광고 시청 후 가챠를 진행
    /// </summary>
    /// <returns></returns>
    private async Task<bool> DailyStoneAdGacha()
    {
        // 광고 가챠가 가능할 때
        if (TimeManager.Instance.CanObtainAdGachaReward(GachaType.Stone))
        {
            // 광고 가챠 쿨타임 업데이트
            TimeManager.Instance.UpdateAdGachaResetTimeInfo(GachaType.Stone);
            // 1회 뽑기 진행
            await MagicStoneSelect(1);
            TimeManager.Instance.OnDailyGachaInfoChanged?.Invoke();
            return true;
        }

        return false;
    }

    #endregion

    #region DailyButton

    /// <summary>
    /// 1회 뽑기 버튼 클릭 이벤트
    /// </summary>
    private async void StoneOneButtonClick()
    {
        // 일일 무료 뽑기가 가능할 때 해당 뽑기 우선 진행
        if (await DailyStoneFreeGacha()) return;

        ConsumeGoodsButtonClick(GachaType.Stone, 1);
    }

    /// <summary>
    /// 일일 뽑기의 가능 여부를 판별하고, 가능할 시 횟수를 소모하고 진행
    /// </summary>
    /// <returns></returns>
    private async Task<bool> DailyStoneFreeGacha()
    {
        // 일일 무료 뽑기가 가능할 때
        if (TimeManager.Instance.CanObtainedFreeGachaReward(GachaType.Stone))
        {
            // 1회 뽑기 진행
            await MagicStoneSelect(1);
            // 일일 무료 뽑기 쿨타임 업데이트
            TimeManager.Instance.UpdateDailyFreeGachaResetTimeInfo(GachaType.Stone);
            return true;
        }
        return false;
    }

    #endregion

    #endregion

    /// <summary>
    /// 재화를 소모하고 뽑기를 진행 - 이후 소모하는 재료 종류에 대한 확장성 고려 필요 (input으로 넣기?)
    /// </summary>
    /// <param name="number"></param>
    private async void ConsumeGoodsButtonClick(GachaType type, int number)
    {
        string uid = FirebaseManager.Auth.CurrentUser.UserId;
        var diaRef = FirebaseManager.DataReference.Child("UserData").Child(uid).Child("Diamond");

        DataSnapshot snapshot = await diaRef.GetValueAsync();

        int currentDiamond = 0;

        if (snapshot.Exists && snapshot.Value != null)
        {
            currentDiamond = Convert.ToInt32(snapshot.Value);
        }

        if (currentDiamond >= 300 * number)
        {
            switch (type)
            {
                case GachaType.Char:
                    CharacterSelect(number);
                    break;
                case GachaType.Stone:
                    await MagicStoneSelect(number);
                    break;
            }

            await DBManager.Instance.SubtractDiamondAsync(300 * number);
        }
        else
        {
            if (PopupManager.Instance != null)
            {
                PopupManager.instance.ShowPopup("다이아몬드가 부족합니다.");
            }
        }
    }

    #region DB Test

    /// <summary>
    /// 데이터베이스 연동 테스트용 기능. 1회 뽑기와 동일 로직
    /// </summary>
    private async void TestItemSelect()
    {
        UnitData data = _testData;

        // 조각 등장 확률도 나중에 가중치로 전환되면 가중치로 적용 필요
        int pieceNum = UnityEngine.Random.Range(1, 11);

        _testData.UpgradeData.AddPiece(pieceNum);

        _resultUI.HeroGachaUpdate(data, 0, pieceNum.ToString());

        await DBManager.Instance.charDB.SaveCharacterUpgradeData(_testData);

        _resultUI.gameObject.SetActive(true);
    }

    #endregion

    #endregion

    #region 가중치 확률 선택

    #region CharacterGacha

    // 확률 변동이 없는 가중치 확률
    private async void CharacterSelect(int number)
    {
        if (_gradeCharRandom.GetList() == null) CharRandomInit(_charProb);

        for (int i = 0; i < number; i++)
        {
            UnitData data = ReturnCharacterData();

            int pieceNum = ReturnCharacterPieceByGrade(data);

            if (IsOveredCharPieceUpperLimit(data, pieceNum, out int overPiece, out int mythPiece))
            {
                if (overPiece != pieceNum)
                {
                    data.UpgradeData.AddPiece(pieceNum - overPiece);
                    await DBManager.Instance.charDB.SaveCharacterUpgradeData(data);
                }

                _resultUI.HeroGachaUpdate(data, i, pieceNum.ToString());

                await DBManager.Instance.AddMythStoneAsync(mythPiece);
            }
            else
            {
                data.UpgradeData.AddPiece(pieceNum);

                _resultUI.HeroGachaUpdate(data, i, pieceNum.ToString());

                await DBManager.Instance.charDB.SaveCharacterUpgradeData(data);
            }
        }

        _resultUI.gameObject.SetActive(true);
    }

    /// <summary>
    /// 등급에 따른 캐릭터 랜덤 반환
    /// </summary>
    /// <returns></returns>
    private UnitData ReturnCharacterData()
    {
        Grade grade = _gradeCharRandom.GetRandomItem();
        return _charData.GetRandomUnitByGrade(grade);
    }

    /// <summary>
    /// 등급에 따른 캐릭터 조각 개수 반환
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private int ReturnCharacterPieceByGrade(UnitData data)
    {
        Grade grade = data.Grade;
        int piece = _gradeCharPieceRandom[(int)grade].GetRandomItem();
        return piece;
    }

    /// <summary>
    /// 캐릭터 조각을 획득했을 때, 조각 개수가 해당 캐릭터의
    /// 최대 강화에 필요한 조각 개수를 초과했는지 확인함.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="inputPiece"></param>
    /// <param name="overPiece"></param>
    /// <param name="mythPiece"></param>
    /// <returns></returns>
    private bool IsOveredCharPieceUpperLimit(UnitData data, int inputPiece, out int overPiece, out int mythPiece)
    {
        Grade grade = data.Grade;
        int level = data.UpgradeData.CurrentUpgradeData.UpgradeLevel;
        int requirePiece = data.LevelUpData.GetCumulativePiece(grade, level);

        if (data.UpgradeData.CurrentUpgradeData.CurrentPieces + inputPiece < requirePiece)
        {
            overPiece = 0;
            mythPiece = 0;
            return false;
        }
        else
        {
            if (data.UpgradeData.CurrentUpgradeData.CurrentPieces > requirePiece)
            {
                overPiece = inputPiece;
            }
            else
            {
                overPiece = data.UpgradeData.CurrentUpgradeData.CurrentPieces + inputPiece - requirePiece;
            }

            int pieceRatio = 0;
            switch (grade)
            {
                case Grade.NORMAL: pieceRatio = overPiece * 2; break;
                case Grade.RARE: pieceRatio = overPiece * 4; break;
                case Grade.UNIQUE: pieceRatio = overPiece * 6; break;
                case Grade.LEGEND: pieceRatio = overPiece * 9; break;
            }
            Debug.Log($"{data.Name}의 조각 개수 초과. {overPiece}개를 초과하여 신화석 {pieceRatio}만큼 지급");

            mythPiece = pieceRatio;

            return true;
        }
    }

    #endregion

    #region MagicStoneGacha

    private async Task MagicStoneSelect(int number)
    {
        if (_magicStoneRewardRandom.GetList() == null) MagicStoneRandomInit(_stoneProb);

        for (int i = 0; i < number; i++)
        {
            MagicStoneRewardType type = _magicStoneRewardRandom.GetRandomItem();

            switch (type)
            {
                case MagicStoneRewardType.MagicStone:
                    MagicStoneSelection(i);
                    break;
                case MagicStoneRewardType.Gold500:
                    await DBManager.Instance.AddGoldAsync(500);
                    _resultUI.StoneGachaUpdate(type, i, 500.ToString());
                    Debug.Log("골드 500");
                    break;
                case MagicStoneRewardType.Gold1000:
                    await DBManager.Instance.AddGoldAsync(1000);
                    _resultUI.StoneGachaUpdate(type, i, 1000.ToString());
                    Debug.Log("골드 1000");
                    break;
                case MagicStoneRewardType.Gold10000:
                    await DBManager.Instance.AddGoldAsync(10000);
                    _resultUI.StoneGachaUpdate(type, i, 10000.ToString());
                    Debug.Log("골드 10000");
                    break;
                case MagicStoneRewardType.Dia50:
                    await DBManager.Instance.AddDiamondAsync(50);
                    _resultUI.StoneGachaUpdate(type, i, 50.ToString());
                    Debug.Log("다이아 50");
                    break;
                case MagicStoneRewardType.Dia100:
                    await DBManager.Instance.AddDiamondAsync(100);
                    _resultUI.StoneGachaUpdate(type, i, 100.ToString());
                    Debug.Log("다이아 100");
                    break;
                case MagicStoneRewardType.Dia1000:
                    await DBManager.Instance.AddDiamondAsync(1000);
                    _resultUI.StoneGachaUpdate(type, i, 1000.ToString());
                    Debug.Log("다이아 1000");
                    break;
            }
        }

        _resultUI.gameObject.SetActive(true);
    }

    private void MagicStoneSelection(int index)
    {
        int pickedStone = UnityEngine.Random.Range(0, _stoneDatabase.MagicStoneDatas.Count);
        MagicStoneData data = _stoneDatabase.MagicStoneDatas[pickedStone];

        int pieces = _magicStonePieceRandom.GetRandomItem();

        _resultUI.StoneGachaUpdate(data, index, pieces.ToString());
    }

    #endregion

    #endregion
}