using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StateMachine
{
    public BaseState _currentState;
    private BaseState _nextState;
    private bool _isTransitioning;

    public void Update()
    {
        _currentState?.Update();
        
        if (_nextState != null)
        {
            Transition(_nextState);
            _nextState = null;
        }
    }

    public void ChangeState(BaseState newState)
    {
        if (_isTransitioning) return;
        _isTransitioning = true;
        _nextState = newState;
    }

    public void SetStateImmediate(BaseState newState)
    {
        _currentState = newState;
    }

    private void Transition(BaseState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState?.Enter();
        _isTransitioning = false;
    }
}
