#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAndAttackTest : MonoBehaviour
{
    [SerializeField] UnitBase unit;
    [SerializeField] UnitStatus unitStatus;

    [Header("Buff_Effect")]
    [SerializeField] BuffEffect _buffEffect;
    [SerializeField] float _duration = 2;

    [Header("Skill")]
    [SerializeField] UnitSkill skill;
    [Space]

    [Header("Attack")]
    [SerializeField] UnitAttackData attackData;

    private void Awake()
    {
        unit.Status = unitStatus;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            UseSkill();

        if (Input.GetKeyDown(KeyCode.W))
            EffectTest();
    }

    [ContextMenu("UseSkill")]
    public void UseSkill()
    {
        skill.Active(unit);
    }

    [ContextMenu("UseAttack")]
    public void UseAttack()
    {
        attackData.Attack(unit);
    }

    [ContextMenu("BuffEffect_Test")]
    public void EffectTest()
    {
        if (unit.StatusController.EffectController == null)
            unit.StatusController.EffectController = new EffectController(unit.transform);

        unit.StatusController.EffectController.AddBuffEffect(_buffEffect, _duration);
    }

    private void OnDrawGizmos()
    {
        skill.DrawGizmos(unit);

        if(attackData is SplashAttack splash)
        {
            splash.DrawGizmos(unit);
        }
    }
}
#endif