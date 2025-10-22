using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillPopUpController : MonoBehaviour
{
    [SerializeField] string _playerUnitSkillPopUpAddress;
    [SerializeField] string _enemyUnitSkillPopUpAddress;

    [SerializeField] Transform _playerContent;
    [SerializeField] Transform _enemyContent;

    [SerializeField] UnitBase[] _playerUnits;
    [SerializeField] UnitBase[] _enemyUnits;

    private GameObject _playerUnitSkillPopUpPrefab;
    private GameObject _enemyUnitSkillPopUpPrefab;

    public void Init(UnitBase[] playerUnits, UnitBase[] enemyUnits)
    {
        _playerUnitSkillPopUpPrefab = Manager.Resources.Load<GameObject>(_playerUnitSkillPopUpAddress);
        _enemyUnitSkillPopUpPrefab = Manager.Resources.Load<GameObject>(_enemyUnitSkillPopUpAddress);

        if (_playerUnits != null && _playerUnits.Length != 0)
        {
            foreach (UnitBase unit in _playerUnits)
            {
                if(unit != null)
                    unit.StatusController.OnUseSkill -= AddPlayerSkillPopUp;
            }
        }
            
        if (_enemyUnits != null && _enemyUnits.Length != 0)
        {
            foreach (UnitBase unit in _enemyUnits)
            {
                if(unit != null)
                    unit.StatusController.OnUseSkill -= AddEnemySkillPopUp;
            }
        }
            
        _playerUnits = playerUnits;
        _enemyUnits = enemyUnits;

        if (_playerUnits != null && _playerUnits.Length != 0)
        {
            foreach (UnitBase unit in _playerUnits)
            {
                if (unit != null)
                    unit.StatusController.OnUseSkill += AddPlayerSkillPopUp;
            }
        }

        if (_enemyUnits != null && _enemyUnits.Length != 0)
        {
            foreach (UnitBase unit in _enemyUnits)
            {
                if (unit != null)
                    unit.StatusController.OnUseSkill += AddEnemySkillPopUp;
            }
        }
    }

    public void AddEnemySkillPopUp(UnitStatus status)
    {
        Instantiate(_enemyUnitSkillPopUpPrefab, _enemyContent).GetComponent<SkillPopUp>().Setup(status);
    }

    public void AddPlayerSkillPopUp(UnitStatus status)
    {
        Instantiate(_playerUnitSkillPopUpPrefab, _playerContent).GetComponent<SkillPopUp>().Setup(status, true);
    }
}
