using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttacker
{
    public LayerMask TargetLayer { get; set; }
    public LayerMask GetAllyLayerMask();
    public UnitStatusController GetStatusController();
    public Transform GetTransform();
    public Vector2 GetCenter();
    public Transform GetTarget();
    public UnitStatus GetUnitStatus();
    public Vector2 GetTargetDir();
}
