using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeAnimationHelper : EnemyAnimationHelper
{
    public void Throw()
    {
        ((SpookyTree) EnemyScript).Throw();
    }
}
