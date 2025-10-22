using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseState
{
    protected BaseFSM _fsm { get; private set; }
    protected Animator _anim => _fsm.Owner.Anim;
    protected Rigidbody2D _rb;
    protected Transform _target => _fsm.Owner.Target;
    protected StateMachine _stateMachine;
    protected UnitStatusController _status;
    protected UnitBase _owner;
    private int _animHash;

    protected bool _isAnimFinished;

    public BaseState(BaseFSM fsm, int animHash)
    {
        _fsm = fsm;
        _animHash = animHash;
        _stateMachine = _fsm.StateMachine;        
        _status = _fsm.Owner.StatusController;
        _rb = _fsm.Owner.Rb;
        _owner = _fsm.Owner;        
    }

    public virtual void Enter()
    {        
        _isAnimFinished = false;
        _anim.SetTrigger(_animHash);
    }

    public virtual void Update()
    {
        
    }

    public virtual void Exit()
    {
        _anim.ResetTrigger(_animHash);
    }

    public void AnimationFinished() => _isAnimFinished = true;
}
