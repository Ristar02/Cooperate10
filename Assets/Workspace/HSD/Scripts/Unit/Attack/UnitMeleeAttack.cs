using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttack", menuName = "Data/Unit/Attack/Melee")]
public class UnitMeleeAttack : UnitAttackData
{
    public override void Attack(IAttacker attacker)
    {
        base.Attack(attacker);

        UnitStatusController status = attacker.GetStatusController();        
        
        if(attacker.GetTarget() == null)
        {
            Debug.Log("[일반 공격] 타겟이 없습니다.");
            return;
        }

        status.CalculateDamage(            
            ComponentProvider.Get<UnitBase>(attacker.GetTarget()?.gameObject).StatusController
            );

        GameObject prefab = Manager.Resources.Load<GameObject>(EffectAddress);

        if(prefab != null)
        {
            GameObject obj = Manager.Resources.Instantiate(prefab, attacker.GetTarget().gameObject.GetCenterPosition(), true);

            // 항상 프리팹 기준 스케일로 초기화
            obj.transform.localScale = prefab.transform.localScale;

            Vector3 finalScale =
                prefab.transform.localScale * Mathf.Abs(attacker.GetTransform().localScale.x);

            if (obj.transform.GetFacingDir() == attacker.GetTransform().GetFacingDir())
            {
                finalScale.x *= -1;
            }

            obj.transform.localScale = finalScale;

            Manager.Resources.Destroy(obj, 2f);
        }        

        status.GetMana();
    }
}
