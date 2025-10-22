using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AnimationType
{
    Normal_Attack,
    Bow_Attack,
    Spear_Attack,
    Axe_Attack,
    Magic_Attack,    
    Normal_Skill,
    Bow_Skill,
    Spear_Skill,
    Axe_Skill,
    Magic_Skill,
    Buff,
    Concentrate,
    Horse_Attack
}

public enum BaseControllerType
{
    Unit, Horse
}

[Serializable]
public struct AnimationData
{
    public AnimationType AnimationType;
    public AnimationClip AnimationClip;
}

[Serializable]
public struct AnimatorData : IEquatable<AnimatorData>
{
    public AnimationType AttackAnimationType;
    public AnimationType SkillAnimationType;
    public BaseControllerType BaseControllerType;

    public bool Equals(AnimatorData other)
    {
        return AttackAnimationType == other.AttackAnimationType &&
               SkillAnimationType == other.SkillAnimationType &&
               BaseControllerType == other.BaseControllerType;
    }

    public override bool Equals(object obj)
    {
        return obj is AnimatorData other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((int)AttackAnimationType * 397) ^ (int)SkillAnimationType;
        }
    }
}

[CreateAssetMenu(fileName = "AnimatorData", menuName = "UnitAnimatorData")]
public class UnitAnimatorData : ScriptableObject
{
    private static readonly Dictionary<AnimationType, AnimationClip> _clips = new Dictionary<AnimationType, AnimationClip>(20);

    [Header("AnimationClipSetting")]
    public AnimationData[] Animations;

    [Header("Control")]
    public RuntimeAnimatorController HorseBaseController;
    public RuntimeAnimatorController BaseController;
    public AnimatorData[] Animators;

    public void SettingAnimationClip()
    {
        foreach (var anim in Animations)
        {
            if (!_clips.ContainsKey(anim.AnimationType))
                _clips.Add(anim.AnimationType, anim.AnimationClip);
        }
    }

    public AnimationClip GetAnimationClip(AnimationType animType)
    {
        return _clips.TryGetValue(animType, out var animationClip) ? animationClip : null;
    }
}
