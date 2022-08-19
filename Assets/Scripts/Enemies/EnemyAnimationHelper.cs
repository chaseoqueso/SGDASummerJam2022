using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationHelper : MonoBehaviour
{
    public EnemyBase EnemyScript;

    void Awake()
    {
        if(EnemyScript == null)
            EnemyScript = GetComponentInParent<EnemyBase>();
    }

    public void UseAbility1()
    {
        EnemyScript.TriggerAbility1();
    }

    public void UseAbility2()
    {
        EnemyScript.TriggerAbility2();
    }
}
