using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimatorType : MonoBehaviour
{
    public AnimatorType AnimatorType;

    public void SetAnimator(RuntimeAnimatorController controller)
    {
        Animator anim = GetComponentInChildren<Animator>();

        anim.runtimeAnimatorController = controller;
    }
}