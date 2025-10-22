using System.Collections;
using UnityEditor;
using UnityEngine;

public class BaseFSM : MonoBehaviour
{
    public UnitBase Owner;    

    #region animHash
    private static readonly int _idleHash = Animator.StringToHash("Idle");
    private static readonly int _moveHash = Animator.StringToHash("Move");
    private static readonly int _attackHash = Animator.StringToHash("Attack");
    private static readonly int _skillHash = Animator.StringToHash("Skill");
    private static readonly int _deadHash = Animator.StringToHash("Dead");
    private static readonly int _stunHash = Animator.StringToHash("Stun");
    private static readonly int _dragHash = Animator.StringToHash("Drag");
    #endregion

    #region State
    public StateMachine StateMachine { get; private set; }
    public StandbyState StandbyState { get; private set; }
    public IdleState IdleState {  get; private set; }
    public MoveState MoveState { get; private set; }
    public DeadState DeadState { get; private set; }
    public AttackState AttackState { get; private set; }
    public StunState StunState { get; private set; }
    public SkillState SkillState { get; private set; }
    #endregion

    private Coroutine _fightRoutine;
    private bool _isInit = false;
    public virtual void Init(UnitBase owner)
    {
        if (_isInit) return;

        Owner = owner;

        StateMachine ??= new StateMachine();

        StandbyState ??= new StandbyState(this, _idleHash);
        IdleState ??= new IdleState(this, _idleHash);
        MoveState ??= new MoveState(this, _moveHash);
        AttackState ??= new AttackState(this, _attackHash);
        SkillState ??= new SkillState(this, _skillHash);
        DeadState ??= new DeadState(this, _deadHash);
        StunState ??= new StunState(this, _stunHash);

        _isInit = true;
    }

    public void Standby()
    {
        if(_fightRoutine != null)
        {
            StopCoroutine(_fightRoutine);
            _fightRoutine = null;
        }

        StateMachine.ChangeState(StandbyState);
        StateMachine.Update();
    }

    public void Move()
    {
        Owner.Anim.ResetTrigger(_idleHash);
        Owner.Anim.SetTrigger(_moveHash);
    }

    public void Idle()
    {
        Owner.Anim.ResetTrigger(_dragHash);
        Owner.Anim.SetTrigger(_idleHash);
    }

    public void Drag()
    {
        Owner.Anim.ResetTrigger(_idleHash);
        Owner.Anim.SetTrigger(_dragHash);
    }

    public void Fight()
    {
        _fightRoutine = StartCoroutine(FightRoutine());
        StateMachine.ChangeState(MoveState);
    }

    private IEnumerator FightRoutine()
    {
        while (true)
        {
            Owner.FlipToTarget();
            Owner.FindTarget();
            StateMachine.Update();
            yield return null;
        }
    }

    public void ChangeStunState(bool isStun)
    {
        if (isStun)
        {
            StateMachine.ChangeState(StunState);
        }
        else
        {
            StateMachine.ChangeState(IdleState);
        }
    }

    private void OnActionEvent()
    {
        if (StateMachine._currentState == AttackState)
        {
            Owner.Attack();
        }
        else if (StateMachine._currentState == SkillState)
        {
            Owner.UseSkill();
        }
    }

    private void AnimationFinished() => StateMachine._currentState.AnimationFinished();
}
