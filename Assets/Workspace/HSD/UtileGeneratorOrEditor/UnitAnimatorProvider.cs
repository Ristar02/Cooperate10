using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class AnimatorInfo
{
    public AnimatorType AnimatorType;
    public RuntimeAnimatorController AnimationController;
}

public class UnitAnimatorProvider : MonoBehaviour
{
    [SerializeField] UnitAnimatorType[] _unitAnimatorTypes;
    [SerializeField] AnimatorInfo[] _animatorInfos;
    private Dictionary<AnimatorType, RuntimeAnimatorController> animControllerDic;

    [ContextMenu("Setting")]
    public void AnimatorSetting()
    {
        animControllerDic = new Dictionary<AnimatorType, RuntimeAnimatorController>(10);

        foreach (var info in _animatorInfos)
        {
            animControllerDic.Add(info.AnimatorType, info.AnimationController);
        }

        foreach (var unitAnimator in _unitAnimatorTypes)
        {
            unitAnimator.SetAnimator(GetRuntimeAnimator(unitAnimator));
            EyeSortingLayerSetting(unitAnimator.gameObject);
        }
    }

    private RuntimeAnimatorController GetRuntimeAnimator(UnitAnimatorType type)
    {
        return animControllerDic[type.AnimatorType];
    }

    private void EyeSortingLayerSetting(GameObject obj)
    {
        foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == "P_LEye")
            {
                Transform front = child.Find("Front");
                if (front != null)
                {
                    SpriteRenderer sr = front.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.sortingOrder = 7;
                    }
                }
            }
        }
    }
}