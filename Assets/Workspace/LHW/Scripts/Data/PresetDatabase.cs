using System.Collections.Generic;

public class PresetDatabase
{
    // 프리셋 관련
    private int _selectedPresetIndex = 0;
    public int SelectedPresetIndex => _selectedPresetIndex;
    private List<TeamPresetData> _presetData = new List<TeamPresetData>();
    public List<TeamPresetData> PresetData => _presetData;

    #region PresetData

    /// <summary>
    /// 프리셋 데이터 초기화
    /// 신규 유저여서 프리셋 데이터가 없는 경우와
    /// 이미 있는 데이터를 로드해야 하는 경우를 나눠야 함
    /// </summary>
    public void InitPresetData()
    {
        // 신규 유저일 경우(프리셋 데이터가 없을 경우)
        if (_presetData.Count == 0)
        {
            for (int i = 0; i < 2; i++)
            {
                _presetData.Add(new TeamPresetData(5));
            }
        }
        // TODO : 신규 유저가 아닐 경우(프리셋 데이터가 있을 경우)
        // TODO : DB에서 [프리셋] 데이터를 로드 후 캐싱
    }

    /// <summary>
    /// 프리셋을 확장함.
    /// </summary>
    /// <param name="size"></param>
    public void CreatePreset(int size)
    {
        _presetData.Add(new TeamPresetData(size));
        // TODO : DB에 [프리셋] 데이터를 저장
    }

    /// <summary>
    /// 현재 선택된 프리셋을 반환함.
    /// </summary>
    /// <returns></returns>
    public TeamPresetData ReadCurrentSelectedPreset()
    {
        if (_selectedPresetIndex == -1) return null;

        return _presetData[_selectedPresetIndex];
    }

    /// <summary>
    /// 현재 선택된 프리셋의 인덱스를 설정함.
    /// </summary>
    /// <param name="index"></param>
    public void SelectPresetIndex(int index)
    {
        _selectedPresetIndex = index;
    }

    #endregion
}