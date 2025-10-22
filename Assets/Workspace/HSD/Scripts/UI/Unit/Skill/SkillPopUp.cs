using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillPopUp : MonoBehaviour
{
    [SerializeField] Image _unitIcon;
    [SerializeField] Image _gradeImage;
    [SerializeField] TMP_Text _skillName;

    public void Setup(UnitStatus status, bool isPlayer = false)
    {
        _unitIcon.sprite = status.Data.Icon;
        _skillName.text = status.Data.Skill.SkillName;

        if (isPlayer)
            _gradeImage.color = status.GetGradeColor();
        else
            _gradeImage.color = status.GetEnemyLevelColor();

        Destroy(gameObject, 1);
    }
}