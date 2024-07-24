using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 近战武器碰撞检测层
/// </summary>
public class Knife_Collider : MonoBehaviour
{
    private Knife knife;
    private bool canAttack = false;
    private List<GameObject> attackTargets = new List<GameObject>();
    public void Init(Knife knife)
    {
        this.knife = knife;
    }

    public void StartAttack()
    {
        canAttack = true;
    }

    public void StopAttack()
    {
        canAttack = false;
        attackTargets.Clear();
    }

    private void OnTriggerStay(Collider other)
    {
        if (canAttack == false) return;

        // 如果还没打过这个单位，则附加一个伤害
        if (!attackTargets.Contains(other.gameObject))
        {
            attackTargets.Add(other.gameObject);
            // 伤害
            knife.HitTarget(other.gameObject, other.ClosestPoint(transform.position));
        }
    }
}
