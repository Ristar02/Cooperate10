using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StandbyState : BaseState
{
    public StandbyState(BaseFSM fsm, int animHash) : base(fsm, animHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}

public class IdleState : BaseState
{
    private float attackTimer;

    public IdleState(BaseFSM fsm, int animHash) : base(fsm, animHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        attackTimer = _fsm.Owner.GetAttackTime(); // 공격 시간 체크
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if(_fsm.Owner.SkillCheck()) // 스킬 사용 가능 여부 체크
        {
            _stateMachine.ChangeState(_fsm.SkillState);
        }
        else if(CanAttack()) // 공격 가능 체크
        {
            _stateMachine.ChangeState(_fsm.AttackState);
        }
        else if (!_owner.IsTargetInRange()) // 멀어진거 체크
        {
            _stateMachine.ChangeState(_fsm.MoveState);
        }
    }

    private bool CanAttack()
    {        
        //if (!_owner.CanAttack())
        //    return false;

        attackTimer -= Time.deltaTime; 
        return attackTimer <= 0;        
    }
}

public class MoveState : BaseState
{
    public MoveState(BaseFSM fsm, int animHash) : base(fsm, animHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if(_owner.IsTargetInRange())
        {
            _stateMachine.ChangeState(_fsm.IdleState);
            return;
        }

        if(_target == null)
        {
            _owner.transform.Translate(new Vector2(_owner.transform.GetFacingDir() * _status.MoveSpeed.Value * Time.deltaTime, 0), Space.World);
        }
        else
        {
            _owner.transform.Translate(_owner.TargetDir * _status.MoveSpeed.Value * Time.deltaTime, Space.World);
        }
    }
}

public class AnimationFinishedState : BaseState
{
    public AnimationFinishedState(BaseFSM fsm, int animHash) : base(fsm, animHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (_isAnimFinished)
            _stateMachine.ChangeState(_fsm.IdleState);
    }
}

public class AttackState : AnimationFinishedState
{

    public AttackState(BaseFSM fsm, int animHash) : base(fsm, animHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}

public class StunState : BaseState
{
    private float timer;

    public StunState(BaseFSM fsm, int animHash) : base(fsm, animHash)
    {
        _status.IsStunned.AddEvent(fsm.ChangeStunState);
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}

public class SkillState : AnimationFinishedState
{
    public SkillState(BaseFSM fsm, int animHash) : base(fsm, animHash)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _status.OnUseSkill?.Invoke(_status.Status);
        _status.OnSkill?.Invoke();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();        
    }
}

public class DeadState : BaseState
{
    public DeadState(BaseFSM fsm, int animHash) : base(fsm, animHash)
    {
        _status.OnDied += ChangeDeadState;
    }

    public override void Enter()
    {
        base.Enter();
        
        _owner.Col.enabled = false;
        _status.OnUnitDied?.Invoke(_status);

        if (_owner.TargetLayer.Contain(LayerMask.NameToLayer("Player")))
            Object.Destroy(_owner.gameObject, 1);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }

    private void ChangeDeadState()
    {
        _stateMachine.ChangeState(_fsm.DeadState);
    }
}
